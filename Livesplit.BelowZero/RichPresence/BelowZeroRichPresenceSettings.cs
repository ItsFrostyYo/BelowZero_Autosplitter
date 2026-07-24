using System;
using System.Drawing;
using System.Windows.Forms;

namespace LiveSplit.BelowZero
{
    public partial class BelowZeroSettings
    {
        private CheckBox chkDiscordStatus;

        public bool DiscordStatusEnabled { get; private set; } = true;

        internal void InitializeDiscordStatusSetting()
        {
            if (chkDiscordStatus != null)
                return;

            chkDiscordStatus = new CheckBox
            {
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Checked = DiscordStatusEnabled,
                CheckState = DiscordStatusEnabled
                    ? CheckState.Checked
                    : CheckState.Unchecked,
                Location = new Point(5, 19),
                Margin = new Padding(2),
                Name = "chkDiscordStatus",
                TabIndex = 0,
                Text = "Discord Status",
                UseVisualStyleBackColor = true
            };

            ToolTips.SetToolTip(
                chkDiscordStatus,
                "Show Below Zero speedrun information as your Discord activity.");

            chkDiscordStatus.CheckedChanged +=
                chkDiscordStatus_CheckedChanged;

            Other_GroupBox.Controls.Add(chkDiscordStatus);
            Other_GroupBox.Controls.SetChildIndex(
                chkDiscordStatus,
                0);

            // Keep all four options inside the original Others group without
            // increasing the settings panel height or moving the split list.
            chkAskForGoldSave.Location = new Point(5, 39);
            chkAskForGoldSave.TabIndex = 1;

            cbOrderedLiveSplit.Location = new Point(5, 59);
            cbOrderedLiveSplit.TabIndex = 2;

            cbOrderedAutoSplits.Location = new Point(5, 79);
            cbOrderedAutoSplits.TabIndex = 3;
        }

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