using System;

namespace LiveSplit.BelowZero
{
    // Present in the August 2021 patch. The October 2025 patch uses GameModePresetId.
    [Flags]
    public enum GameModeOption
    {
        None = 0,
        Permadeath = 1,
        NoSurvival = 2,
        NoCost = 4,
        NoBlueprints = 8,
        NoEnergy = 16,
        NoPressure = 32,
        NoOxygen = 64,
        NoAggression = 128,
        NoHints = 256,
        NoRadiation = 512,
        InitialItems = 1024,
        NoCold = 2048,
        Cheats = 3836,
        Survival = 0,
        Hardcore = 257,
        Freedom = 2,
        Creative = 3838
    }
}
