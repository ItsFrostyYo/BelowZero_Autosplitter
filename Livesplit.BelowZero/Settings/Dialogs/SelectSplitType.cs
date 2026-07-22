using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LiveSplit.BelowZero.Settings
{
    public partial class SelectSplitType : Form
    {
        public Func<bool, BelowZeroSplitSetting> Func { get; set; }
        public SelectSplitType(BelowZeroBaseSettings settings, bool isSubCondition)
        {
            InitializeComponent();
            List<SplitType> items = new List<SplitType>();

            if (!isSubCondition)
                items.Add(new SplitType { Text = "Pre-Made Conditions", Func = settings.CreatePrefabSplit });

            items.Add(new SplitType { Text = "Achievements", Func = settings.CreateAchievementSplit });
            items.Add(new SplitType { Text = "Artifacts", Func = settings.CreateArtifactSplit });
            items.Add(new SplitType { Text = "Blueprints", Func = settings.CreateBlueprintSplit });
            items.Add(new SplitType { Text = "Databanks", Func = settings.CreateEncyclopediaSplit });
            items.Add(new SplitType { Text = "Inventory", Func = settings.CreateItemSplit });

            if (!isSubCondition)
            {
                items.Add(new SplitType { Text = "Craft", Func = settings.CreateCraftSplit });
                items.Add(new SplitType { Text = "Build", Func = settings.CreateBuildSplit });
            }

            items.Add(new SplitType { Text = "Biome", Func = settings.CreateBiomeSplit });
            items.Add(new SplitType { Text = "Story Goals", Func = settings.CreateStoryGoalSplit });

            cboSplitType.DisplayMember = nameof(SplitType.Text);
            cboSplitType.ValueMember = nameof(SplitType.Func);
            cboSplitType.DataSource = items;
        }

        private class SplitType
        {
            public string Text { get; set; }
            public Func<bool, BelowZeroSplitSetting> Func { get; set; }

            public override string ToString() => Text;
        }

        private void btnOK_Click(object sender, EventArgs e) => OK();

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void OK()
        {
            if (cboSplitType.SelectedValue is Func<bool, BelowZeroSplitSetting> func)
                Func = func;
            DialogResult = DialogResult.OK;
        }
    }
}

