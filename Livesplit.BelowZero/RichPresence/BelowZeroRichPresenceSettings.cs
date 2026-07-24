using System;

namespace LiveSplit.BelowZero
{
    public partial class BelowZeroSettings
    {
        public bool DiscordStatusEnabled { get; private set; } = true;

        internal void SetDiscordStatusEnabled(bool enabled)
        {
            DiscordStatusEnabled = enabled;

            if (chkDiscordStatus == null ||
                chkDiscordStatus.Checked == enabled)
            {
                return;
            }

            bool wasLoading = IsLoading;
            IsLoading = true;

            try
            {
                chkDiscordStatus.Checked = enabled;
            }
            finally
            {
                IsLoading = wasLoading;
            }
        }

        private void chkDiscordStatus_CheckedChanged(
            object sender,
            EventArgs e)
        {
            if (IsLoading)
                return;

            DiscordStatusEnabled =
                chkDiscordStatus.Checked;

            ControlChanged(sender, e);
        }
    }
}