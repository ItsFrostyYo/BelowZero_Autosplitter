using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace LiveSplit.BelowZero
{
    public partial class BelowZeroCraftSplit : BelowZeroSplitSetting
    {
        public CraftableSplitBase _split;

        private int mX = 0;
        private int mY = 0;
        private bool isDragging = false;

        public BelowZeroCraftSplit() : this(new CraftSplit(Craftable.None, onlySplitOnce: true, isSubCondition: false)) { }
        public BelowZeroCraftSplit(CraftableSplitBase craftSplit)
        {
            InitializeComponent();

            _split = craftSplit ?? new CraftSplit(Craftable.None, onlySplitOnce: true, isSubCondition: false);

            cboCraftables.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCraftables.MouseWheel += (o, e) => ((HandledMouseEventArgs)e).Handled = true;
            cboCraftables.DisplayMember = "Display";
            cboCraftables.ValueMember = "Value";
            l_name.Text = _split.SplitName == SplitName.Build ? "Build" : "Craft";
        }

        private void BtnOptions_Click(object sender, EventArgs e)
        {
            var splitSettings = new BelowZeroCraftSplitSettings(_split);
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

            if (cboCraftables.SelectedValue is Craftable craftable)
                _split.Craftable = craftable;
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

        public override ComboBox ComboBox => this.cboCraftables;
        public override Button BtnEdit => this.btnEdit;
        public override Button BtnRemove => this.btnRemove;
        public override SplitName SplitName => _split?.SplitName ?? SplitName.Craft;
        public override BelowZeroSplit Split => this._split;
    }

    public abstract class CraftableSplitBase : BelowZeroSplit
    {
        public Craftable Craftable { get; set; }

        protected CraftableSplitBase(Craftable craftable, SplitName splitName, bool onlySplitOnce, bool isSubCondition)
        {
            Craftable = craftable;
            OnlySplitOnce = onlySplitOnce;
            SplitName = splitName;
            IsSubCondition = isSubCondition;
        }

        public override BelowZeroSplit DeepCopy()
        {
            var copy = (CraftableSplitBase)base.DeepCopy();
            copy.Craftable = Craftable;
            return copy;
        }
    }

    public class CraftSplit : CraftableSplitBase
    {
        public CraftSplit(Craftable craftable, bool onlySplitOnce, bool isSubCondition)
            : base(craftable, SplitName.Craft, onlySplitOnce, isSubCondition)
        {
        }

        public override string GetDescription() => $"Craft {Localization.GetDisplayName(Craftable)}";
    }

    public class BuildSplit : CraftableSplitBase
    {
        public BuildSplit(Craftable craftable, bool onlySplitOnce, bool isSubCondition)
            : base(craftable, SplitName.Build, onlySplitOnce, isSubCondition)
        {
        }

        public override string GetDescription() => $"Build {Localization.GetDisplayName(Craftable)}";
    }
}

