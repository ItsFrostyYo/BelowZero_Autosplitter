[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$LiveSplitPath = "C:\Users\mxz\Desktop\LiveSplit\LiveSplit_1.8.37\LiveSplit.exe"
$LogPath = "C:\Users\mxz\Desktop\LiveSplit\LiveSplit_1.8.37\_BelowZero.log"

function Get-LogState {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return [pscustomobject]@{
            Exists       = $false
            Length       = [int64]0
            LastWriteUtc = [datetime]::MinValue
        }
    }

    $item = Get-Item -LiteralPath $Path

    return [pscustomobject]@{
        Exists       = $true
        Length       = [int64]$item.Length
        LastWriteUtc = $item.LastWriteTimeUtc
    }
}

function Read-NewLogBytes {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [ref]$Offset,

        [Parameter(Mandatory)]
        [System.Text.Decoder]$Decoder
    )

    $stream = $null

    try {
        $stream = [System.IO.File]::Open(
            $Path,
            [System.IO.FileMode]::Open,
            [System.IO.FileAccess]::Read,
            [System.IO.FileShare]::ReadWrite -bor [System.IO.FileShare]::Delete
        )

        if ($stream.Length -lt $Offset.Value) {
            $Offset.Value = [int64]0
            $Decoder.Reset()
        }

        if ($stream.Length -eq $Offset.Value) {
            return
        }

        [void]$stream.Seek($Offset.Value, [System.IO.SeekOrigin]::Begin)

        $buffer = New-Object byte[] 8192

        while (($bytesRead = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
            $charCount = $Decoder.GetCharCount($buffer, 0, $bytesRead, $false)
            $chars = New-Object char[] $charCount
            [void]$Decoder.GetChars($buffer, 0, $bytesRead, $chars, 0, $false)

            if ($chars.Length -gt 0) {
                Write-Host -NoNewline ([string]::new($chars))
            }
        }

        $Offset.Value = $stream.Position
    }
    finally {
        if ($null -ne $stream) {
            $stream.Dispose()
        }
    }
}

try {
    if (-not (Test-Path -LiteralPath $LiveSplitPath -PathType Leaf)) {
        throw "LiveSplit.exe was not found:`n$LiveSplitPath"
    }

    $BeforeLaunch = Get-LogState -Path $LogPath

    Write-Host ""
    Write-Host "Launching LiveSplit..." -ForegroundColor Cyan
    Write-Host $LiveSplitPath -ForegroundColor DarkGray
    Write-Host ""

    $LiveSplitProcess = Start-Process `
        -FilePath $LiveSplitPath `
        -WorkingDirectory (Split-Path -Parent $LiveSplitPath) `
        -PassThru

    Write-Host "LiveSplit started with process ID $($LiveSplitProcess.Id)." -ForegroundColor Green
    Write-Host "Waiting for a new or updated Below Zero log..." -ForegroundColor Yellow
    Write-Host $LogPath -ForegroundColor DarkGray
    Write-Host ""

    $Offset = [int64]0

    while ($true) {
        $Current = Get-LogState -Path $LogPath

        if ($Current.Exists) {
            if (-not $BeforeLaunch.Exists) {
                # The log was created after LiveSplit launched.
                $Offset = [int64]0
                break
            }

            $ChangedAfterLaunch =
                ($Current.LastWriteUtc -gt $BeforeLaunch.LastWriteUtc) -or
                ($Current.Length -ne $BeforeLaunch.Length)

            if ($ChangedAfterLaunch) {
                if ($Current.Length -gt $BeforeLaunch.Length) {
                    # The existing log was appended to. Begin exactly where
                    # the pre-launch file ended, so old lines are not printed.
                    $Offset = [int64]$BeforeLaunch.Length
                }
                else {
                    # The log was cleared, replaced, or rewritten.
                    $Offset = [int64]0
                }

                break
            }
        }

        if ($LiveSplitProcess.HasExited) {
            throw "LiveSplit exited before the log file was created or updated."
        }

        Start-Sleep -Milliseconds 200
        $LiveSplitProcess.Refresh()
    }

    Write-Host "Log activity detected. Showing only content written after this launch." -ForegroundColor Green
    Write-Host "Press Ctrl+C to stop watching the log." -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "==================== LIVE LOG ====================" -ForegroundColor Cyan
    Write-Host ""

    $Utf8 = [System.Text.UTF8Encoding]::new($false, $false)
    $Decoder = $Utf8.GetDecoder()
    $LogWasMissing = $false

    while ($true) {
        if (-not (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
            if (-not $LogWasMissing) {
                Write-Host ""
                Write-Host "[Log file was removed. Waiting for it to be created again...]" -ForegroundColor Yellow
                $LogWasMissing = $true
            }

            $Offset = [int64]0
            $Decoder.Reset()
            Start-Sleep -Milliseconds 200
            continue
        }

        if ($LogWasMissing) {
            Write-Host "[Log file recreated. Resuming...]" -ForegroundColor Green
            $LogWasMissing = $false
            $Offset = [int64]0
            $Decoder.Reset()
        }

        $item = Get-Item -LiteralPath $LogPath

        if ($item.Length -lt $Offset) {
            Write-Host ""
            Write-Host "[Log file was cleared or replaced. Restarting from its beginning...]" -ForegroundColor Yellow
            $Offset = [int64]0
            $Decoder.Reset()
        }
        Read-NewLogBytes -Path $LogPath -Offset ([ref]$Offset) -Decoder $Decoder
        Start-Sleep -Milliseconds 150
    }
}
catch {
    Write-Host ""
    Write-Host "Launch/log viewer failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    exit 1
}
