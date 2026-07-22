using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Voxif.AutoSplitter;

namespace LiveSplit.BelowZero
{
    public partial class BelowZeroEncyclopediaSplit : BelowZeroSplitSetting
    {
        public BelowZeroSplit _split;

        private int mX = 0;
        private int mY = 0;
        private bool isDragging = false;

        public BelowZeroEncyclopediaSplit() : this(new EncyclopediaSplit(EncyclopediaEntry.None, onlySplitOnce: true, isSubCondition: false)) { }
        public BelowZeroEncyclopediaSplit(BelowZeroSplit encySplit)
        {
            InitializeComponent();

            _split = encySplit ?? new EncyclopediaSplit(EncyclopediaEntry.None, onlySplitOnce: true, isSubCondition: false);
            l_name.Text = _split is ArtifactSplit ? "Artifacts"
                : _split is StoryGoalSplit ? "Story Goal"
                : _split is AchievementSplit ? "Achievement"
                : "Encyclopedia";

            if (_split.SplitName == SplitName.Encyclopedia)
                cboEncy.Width = 341;

            cboEncy.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEncy.MouseWheel += (o, e) => ((HandledMouseEventArgs)e).Handled = true;
            cboEncy.DisplayMember = "Display";
            cboEncy.ValueMember = "Value";
        }

        private void BtnOptions_Click(object sender, EventArgs e)
        {
            var splitSettings = new BelowZeroEncyclopediaSplitSettings(_split);
            var settings = new SplitSettingsDialog(splitSettings) { StartPosition = FormStartPosition.CenterParent };

            if (settings.ShowDialog() == DialogResult.OK)
            {
                _split.OnlySplitOnce = splitSettings.OnlySplitOnce;
                _split.Conditions = splitSettings.Splits;
            }
        }

        private void cboName_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedValueToolTip();

            if (IsLoading)
                return;

            if (_split is ArtifactSplit artifactSplit && cboEncy.SelectedValue is Artifact artifact)
                artifactSplit.Artifact = artifact;
            else if (_split is EncyclopediaSplit encyclopediaSplit && cboEncy.SelectedValue is EncyclopediaEntry entry)
                encyclopediaSplit.Entry = entry;
            else if (_split is StoryGoalSplit storyGoalSplit && cboEncy.SelectedValue is StoryGoal storyGoal)
                storyGoalSplit.Goal = storyGoal;
            else if (_split is AchievementSplit achievementSplit && cboEncy.SelectedValue is Achievement achievement)
                achievementSplit.Achievement = achievement;
        }

        private void UpdateSelectedValueToolTip()
        {
            if (!(cboEncy.SelectedValue is Enum selectedValue))
            {
                ToolTips.SetToolTip(cboEncy, string.Empty);
                return;
            }

            var member = selectedValue.GetType().GetMember(selectedValue.ToString()).FirstOrDefault();
            var toolTip = member?
                .GetCustomAttributes(typeof(ToolTipAttribute), inherit: false)
                .OfType<ToolTipAttribute>()
                .FirstOrDefault();

            ToolTips.SetToolTip(cboEncy, toolTip?.ToolTip ?? string.Empty);
        }

        private void picHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging)
            {
                if (e.Button == MouseButtons.Left)
                {
                    int num1 = mX - e.X;
                    int num2 = mY - e.Y;
                    if (((num1 * num1) + (num2 * num2)) > 20)
                    {
                        DoDragDrop(this, DragDropEffects.All);
                        isDragging = true;
                        return;
                    }
                }
            }
        }

        private void picHandle_MouseDown(object sender, MouseEventArgs e)
        {
            mX = e.X;
            mY = e.Y;
            isDragging = false;
        }

        public override ComboBox ComboBox => this.cboEncy;
        public override Button BtnEdit => this.btnEdit;
        public override Button BtnRemove => this.btnRemove;
        public override SplitName SplitName => _split.SplitName;
        public override BelowZeroSplit Split => this._split;
    }

    public class EncyclopediaSplit : BelowZeroSplit
    {
        public EncyclopediaEntry Entry { get; set; }

        public EncyclopediaSplit(EncyclopediaEntry entry, bool onlySplitOnce, bool isSubCondition)
        {
            Entry = entry;
            this.OnlySplitOnce = onlySplitOnce;
            this.SplitName = SplitName.Encyclopedia;
            this.IsSubCondition = isSubCondition;
        }
        public override string GetDescription() => $"{Localization.GetDisplayName(Entry)} in Encyclopedia Split";
    }

    public class ArtifactSplit : EncyclopediaSplit
    {
        private Artifact artifact;

        public Artifact Artifact
        {
            get => artifact;
            set
            {
                artifact = value;
                Entry = value.ConvertTo<EncyclopediaEntry>();
            }
        }

        public ArtifactSplit(Artifact artifact, bool onlySplitOnce, bool isSubCondition)
            : base(artifact.ConvertTo<EncyclopediaEntry>(), onlySplitOnce, isSubCondition)
        {
            Artifact = artifact;
            SplitName = SplitName.Artifact;
        }

        public override string GetDescription() => $"Scan {Localization.GetDisplayName(Artifact)}";
    }

    public class StoryGoalSplit : BelowZeroSplit
    {
        public StoryGoal Goal { get; set; }

        public StoryGoalSplit(StoryGoal goal, bool onlySplitOnce, bool isSubCondition)
        {
            Goal = goal;
            OnlySplitOnce = onlySplitOnce;
            SplitName = SplitName.StoryGoal;
            IsSubCondition = isSubCondition;
        }

        public override string GetDescription() => Goal.ToString();
    }

    public class AchievementSplit : BelowZeroSplit
    {
        public Achievement Achievement { get; set; }

        public AchievementSplit(Achievement achievement, bool onlySplitOnce, bool isSubCondition)
        {
            Achievement = achievement;
            OnlySplitOnce = onlySplitOnce;
            SplitName = SplitName.Achievement;
            IsSubCondition = isSubCondition;
        }

        public override string GetDescription() => Achievement.GetDescription();
    }
}

