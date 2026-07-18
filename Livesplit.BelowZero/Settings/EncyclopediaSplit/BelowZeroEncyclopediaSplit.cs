using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace LiveSplit.BelowZero
{
    public partial class BelowZeroEncyclopediaSplit : BelowZeroSplitSetting
    {
        public EncyclopediaSplit _split;

        private int mX = 0;
        private int mY = 0;
        private bool isDragging = false;

        public BelowZeroEncyclopediaSplit() : this(new EncyclopediaSplit(EncyclopediaEntry.None, onlySplitOnce: true, isSubCondition: false)) { }
        public BelowZeroEncyclopediaSplit(EncyclopediaSplit encySplit)
        {
            InitializeComponent();

            _split = encySplit ?? new EncyclopediaSplit(EncyclopediaEntry.None, onlySplitOnce: true, isSubCondition: false);

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
            if (IsLoading)
                return;

            if (cboEncy.SelectedValue is EncyclopediaEntry entry)
                _split.Entry = entry;
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
        public override SplitName SplitName => SplitName.Encyclopedia;
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
}

