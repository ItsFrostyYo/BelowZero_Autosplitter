using LiveSplit.BelowZero;
using LiveSplit.BelowZero.Settings;
using LiveSplit.Model;
using LiveSplit.VoxSplitter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Voxif.AutoSplitter;

namespace LiveSplit.BelowZero
{
    public partial class BelowZeroSettings : BelowZeroBaseSettings
    {
        public bool StartEnabled { get; set; } = true;
        public bool IntroStart { get; set; } = true;
        public bool CreativeStart { get; set; } = true;
        public bool Reset { get; set; }
        public bool AskForGoldSave { get; set; }
        public static bool OrderedLiveSplit { get; set; }
        public static bool OrderedAutoSplits { get; set; }

        public override FlowLayoutPanel MainPanel => flowMain;
        public override FlowLayoutPanel Options => flowOptions;
        public override RadioButton Alpha => RdAlpha;

        private void BtnAddSplit_Click(object sender, EventArgs e) => BtnAddSplitClick(sender, e);
        private void BtnRemove_Click(object sender, EventArgs e) => BtnRemoveClick(sender, e);
        private void BtnEdit_Click(object sender, EventArgs e) => BtnEditClick(sender, e);
        private void flowMain_Paint(object sender, PaintEventArgs e) => flowMainPaint(sender, e);
        private void flowMain_DragEnter(object sender, DragEventArgs e) => flowMainDragEnter(sender, e);
        private void flowMain_DragLeave(object sender, EventArgs e) => flowMainDragLeave(sender, e);
        private void flowMain_DragDrop(object sender, DragEventArgs e) => flowMainDragDrop(sender, e);
        private void flowMain_DragOver(object sender, DragEventArgs e) => flowMainDragOver(sender, e);
        private void RdSort_CheckedChanged(object sender, EventArgs e) => RdSortCheckedChanged(sender, e);

        public BelowZeroSettings(LiveSplitState state)
        {
            InitializeComponent();
            Splits = new List<BelowZeroSplit>();
            State = state;
        }

        #region Buttons
        private void ButtonSplitGenerator_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Generating the splits will overwrite the existing splits and times, do you want to overwrite them?",
                "Generate Splits?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            using (SplitsGenerator splitGen = new SplitsGenerator())
            {
                int maxWidth = 0;
                foreach (BelowZeroSplit split in Splits)
                {
                    string splitName = "Split name";
                    splitName = split.GetDescription();
                    splitGen.ListView.Items.Add(splitName);
                    int width = TextRenderer.MeasureText(splitName, splitGen.ListView.Font).Width;
                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
                splitGen.ListView.Columns[0].Width = maxWidth + 10;
                splitGen.ListView.Size = new Size(maxWidth + 30, (int)Math.Min(splitGen.ListView.Items[0].Bounds.Height * (splitGen.ListView.Items.Count + 1), Screen.PrimaryScreen.Bounds.Height * .75f));
                if (splitGen.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                //Doesn't work with subsplits + show last split
                State.Run.Clear();
                foreach (ListViewItem item in splitGen.ListView.Items)
                {
                    State.Run.AddSegment(item.Text);
                }
                State.Form.Refresh();
            }
        }
        #endregion

        public override void ControlChanged(object sender, EventArgs e) => UpdateSplits();

        public override void UpdateSplits()
        {
            if (IsLoading)
                return;

            StartEnabled = chkIntroStart.Checked;
            IntroStart = StartEnabled;
            CreativeStart = StartEnabled;
            Reset = chkReset.Checked;
            AskForGoldSave = chkAskForGoldSave.Checked;
            OrderedLiveSplit = cbOrderedLiveSplit.Checked;
            OrderedAutoSplits = cbOrderedAutoSplits.Checked;

            base.UpdateSplits();
        }

        public XmlNode UpdateSettings(XmlDocument document)
        {
            XmlElement xmlSettings = document.CreateElement("Settings");

            AddBool(document, xmlSettings, "StartEnabled", StartEnabled);
            AddBool(document, xmlSettings, "IntroStart", StartEnabled);
            AddBool(document, xmlSettings, "CreativeStart", StartEnabled);
            AddBool(document, xmlSettings, "Reset", Reset);
            AddBool(document, xmlSettings, "AskForGoldSave", AskForGoldSave);
            AddBool(document, xmlSettings, "OrderedLiveSplit", OrderedLiveSplit);
            AddBool(document, xmlSettings, "OrderedAutoSplits", OrderedAutoSplits);

            XmlElement xmlSplits = document.CreateElement("Splits");
            xmlSettings.AppendChild(xmlSplits);

            UpdateSplits();

            foreach (var split in Splits)
            {
                var xmlSplit = CreateSplitElement(document, split);
                xmlSplits.AppendChild(xmlSplit);
            }

            return xmlSettings;
        }

        private static XmlElement CreateSplitElement(XmlDocument document, BelowZeroSplit split)
        {
            XmlElement xmlSplit = document.CreateElement("Split");

            XmlElement xmlName = document.CreateElement("Name");
            XmlElement xmlOnlySplitOnce = document.CreateElement("OnlySplitOnce");
            XmlElement xmlIsSubCondition = document.CreateElement("IsSubCondition");
            XmlElement xmlValue = document.CreateElement("Value");

            xmlName.InnerText = split.SplitName.ToString();
            xmlOnlySplitOnce.InnerText = split.OnlySplitOnce.ToString();
            xmlIsSubCondition.InnerText = split.IsSubCondition.ToString();

            switch (split)
            {
                case ItemSplit itemSplit:
                    xmlValue.InnerText = $"{itemSplit.Item}:{itemSplit.PickUp}:{itemSplit.IsCount}:{itemSplit.Count}";
                    break;
                case BlueprintSplit bpSplit:
                    xmlValue.InnerText = bpSplit.Blueprint.ToString();
                    break;
                case EncyclopediaSplit encySplit:
                    xmlValue.InnerText = encySplit.Entry.ToString();
                    break;
                case BiomeSplit biomeSplit:
                    xmlValue.InnerText = $"{biomeSplit.Biomes.Biome1}:{biomeSplit.Biomes.Biome2}";
                    break;
                case CraftableSplitBase craftableSplit:
                    xmlValue.InnerText = craftableSplit.Craftable.ToString();
                    break;
                default:
                    xmlValue.InnerText = split.SplitName.ToString();
                    break;
            }

            xmlSplit.AppendChild(xmlOnlySplitOnce);
            xmlSplit.AppendChild(xmlIsSubCondition);
            xmlSplit.AppendChild(xmlName);
            xmlSplit.AppendChild(xmlValue);

            if (split.Conditions != null && split.Conditions.Count > 0)
            {
                var xmlConditions = document.CreateElement("Conditions");
                foreach (var condition in split.Conditions)
                {
                    var xmlCondSplit = CreateSplitElement(document, condition);
                    xmlConditions.AppendChild(xmlCondSplit);
                }
                xmlSplit.AppendChild(xmlConditions);
            }

            return xmlSplit;
        }

        public void SetSettings(XmlNode settings)
        {
            try
            {
                XmlNode splitsNode = settings.SelectSingleNode(".//Splits");
                if (splitsNode != null)
                {
                    StartEnabled = ReadBool(settings, "StartEnabled", ReadBool(settings, "IntroStart") || ReadBool(settings, "CreativeStart"));
                    IntroStart = StartEnabled;
                    CreativeStart = StartEnabled;
                    Reset = ReadBool(settings, "Reset");
                    AskForGoldSave = ReadBool(settings, "AskForGoldSave");
                    OrderedLiveSplit = ReadBool(settings, "OrderedLiveSplit");
                    OrderedAutoSplits = ReadBool(settings, "OrderedAutoSplits");

                    Splits.Clear();
                    foreach (XmlNode splitNode in splitsNode.SelectNodes("Split"))
                    {
                        var split = ReadSplitNode(splitNode);
                        if (split != null)
                            Splits.Add(split);
                    }
                }
            }
            catch { }

            LoadSettings();
        }

        private BelowZeroSplit ReadSplitNode(XmlNode splitNode)
        {
            var name = splitNode.SelectSingleNode("Name")?.InnerText;
            var value = splitNode.SelectSingleNode("Value")?.InnerText;
            var values = value.Split(':');

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                return null;

            bool onlySplitOnce = true;
            bool.TryParse(splitNode.SelectSingleNode("OnlySplitOnce")?.InnerText, out onlySplitOnce);
            bool.TryParse(splitNode.SelectSingleNode("IsSubCondition")?.InnerText, out bool isSubCondition);


            var splitName = BelowZeroSplitSetting.GetSplitName(name);

            BelowZeroSplit split;
            switch (splitName)
            {
                case SplitName.Inventory:
                    var item = BelowZeroSplitSetting.GetTechType(values[0]);
                    split = new ItemSplit(item.ConvertTo<InventoryItem>(), onlySplitOnce, isSubCondition);
                    if (values.Length >= 3)
                    {
                        if (bool.TryParse(values[1], out var pickup))
                            ((ItemSplit)split).PickUp = pickup;

                        if (bool.TryParse(values[2], out var isCount))
                            ((ItemSplit)split).IsCount = isCount;

                        if (int.TryParse(values[3], out var count))
                            ((ItemSplit)split).Count = count;
                    }
                    break;

                case SplitName.Blueprint:
                    var blueprint = BelowZeroSplitSetting.GetTechType(value);
                    split = new BlueprintSplit(blueprint.ConvertTo<Unlockable>(), onlySplitOnce, isSubCondition);                    
                    break;

                case SplitName.Encyclopedia:
                    var encyEntry = BelowZeroSplitSetting.GetEncyclopediaEntry(value);
                    split = new EncyclopediaSplit(encyEntry, onlySplitOnce, isSubCondition);
                    break;

                case SplitName.Biome:
                    if (values.Length >= 2)
                    {
                        split = new BiomeSplit(
                            (BelowZeroSplitSetting.GetBiome(values[0]),
                             BelowZeroSplitSetting.GetBiome(values[1])),
                             onlySplitOnce, isSubCondition);
                    }
                    else return null;
                    break;

                case SplitName.Craft:
                    var craftable = BelowZeroSplitSetting.GetTechType(value);
                    if (craftable == TechType.PrecursorNPCBody)
                    {
                        split = new PrefabSplit(SplitName.CommenceStorageMediumFabricationSplit, onlySplitOnce, isSubCondition);
                        break;
                    }
                    split = new CraftSplit(craftable.ConvertTo<Craftable>(), onlySplitOnce, isSubCondition);
                    break;

                case SplitName.Build:
                    var buildable = BelowZeroSplitSetting.GetTechType(value);
                    split = new BuildSplit(buildable.ConvertTo<Craftable>(), onlySplitOnce, isSubCondition);
                    break;

                default:
                    split = new PrefabSplit(splitName, onlySplitOnce, isSubCondition);
                    break;
            }

            var conditionsNode = splitNode.SelectSingleNode("Conditions");
            if (conditionsNode != null)
            {
                foreach (XmlNode condNode in conditionsNode.SelectNodes("Split"))
                {
                    var cond = ReadSplitNode(condNode);
                    if (cond != null)
                        split.Conditions.Add(cond);
                }
            }

            return split;
        }

        public override void LoadSettings()
        {
            IsLoading = true;
            chkIntroStart.Checked = StartEnabled;
            chkCreativeStart.Checked = StartEnabled;
            chkReset.Checked = Reset;
            chkAskForGoldSave.Checked = AskForGoldSave;
            cbOrderedLiveSplit.Checked = OrderedLiveSplit;
            cbOrderedAutoSplits.Checked = OrderedAutoSplits;

            base.LoadSettings();
            IsLoading = false;
        }

        private void Settings_Load(object sender, EventArgs e) => LoadSettings();

        private static XmlElement AddBool(XmlDocument doc, XmlElement root, string name, bool value)
        {
            var e = doc.CreateElement(name); e.InnerText = value.ToString(); root.AppendChild(e); return e;
        }
        private static bool ReadBool(XmlNode root, string name, bool def = false)
        {
            var n = root.SelectSingleNode($".//{name}");
            return n != null && bool.TryParse(n.InnerText, out var b) ? b : def;
        }

        private void cbOrderedLiveSplit_CheckedChanged(object sender, EventArgs e)
        {
            if (cbOrderedLiveSplit.Checked && cbOrderedAutoSplits.Checked)
                cbOrderedAutoSplits.Checked = false;

            ControlChanged(sender, e);
        }

        private void cbOrderedAutoSplits_CheckedChanged(object sender, EventArgs e)
        {
            if (cbOrderedAutoSplits.Checked && cbOrderedLiveSplit.Checked)
                cbOrderedLiveSplit.Checked = false;

            ControlChanged(sender, e);
        }
    }   
}
