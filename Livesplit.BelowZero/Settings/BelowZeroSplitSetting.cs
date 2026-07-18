using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.BelowZero
{
    public class BelowZeroSplitSetting : UserControl
    {
        public Func<bool> IsLoadingGetter { get; set; }
        public bool IsLoading => IsLoadingGetter?.Invoke() ?? false;
        public bool IsSubCondition { get; set; } = false;
        public virtual ComboBox ComboBox { get; }
        public virtual ComboBox ComboBox2 { get; }
        public virtual Button BtnEdit { get; }
        public virtual Button BtnRemove { get; }
        public virtual SplitName SplitName { get; }
        public virtual BelowZeroSplit Split { get; }

        public static SplitName GetSplitName(string text)
        {
            foreach (SplitName split in Enum.GetValues(typeof(SplitName)))
            {
                string name = split.ToString();
                MemberInfo info = typeof(SplitName).GetMember(name)[0];
                DescriptionAttribute description = (DescriptionAttribute)info.GetCustomAttributes(typeof(DescriptionAttribute), false)[0];

                if (name.Equals(text, StringComparison.OrdinalIgnoreCase) || description.Description.Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    return split;
                }
            }
            return SplitName.None;
        }

        public static TechType GetTechType(string text)
        {
            foreach (TechType techType in Enum.GetValues(typeof(TechType)))
            {
                string name = techType.ToString();
                string displayName = Localization.GetDisplayName(name);

                if (name.Equals(text, StringComparison.OrdinalIgnoreCase) || displayName.Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    return techType;
                }
            }
            return TechType.None;
        }

        public static EncyclopediaEntry GetEncyclopediaEntry(string text)
        {
            foreach (EncyclopediaEntry encyEntry in Enum.GetValues(typeof(EncyclopediaEntry)))
            {
                string name = encyEntry.ToString();
                string displayName = Localization.GetDisplayName(name);

                if (name.Equals(text, StringComparison.OrdinalIgnoreCase) || displayName.Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    return encyEntry;
                }
            }
            return EncyclopediaEntry.None;
        }

        public static Biome GetBiome(string text)
        {
            foreach (Biome biome in Enum.GetValues(typeof(Biome)))
            {
                string name = biome.ToString();
                string displayName = Localization.GetDisplayName(name);

                if (name.Equals(text, StringComparison.OrdinalIgnoreCase) || displayName.Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    return biome;
                }
            }
            return Biome.None;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
                DoDragDrop(this, DragDropEffects.Move);
        }
    }    
    
    public class BelowZeroSplit
    {
        public SplitName SplitName { get; set; }
        public bool OnlySplitOnce { get; set; }
        public bool IsSubCondition { get; set; }
        public List<BelowZeroSplit> Conditions { get; set; } = new List<BelowZeroSplit>();
        public virtual string GetDescription() => "Split";
        public virtual BelowZeroSplit DeepCopy()
        {
            var copy = (BelowZeroSplit)this.MemberwiseClone();
            copy.Conditions = Conditions?.Select(c => c.DeepCopy()).ToList() ?? new List<BelowZeroSplit>();

            return copy;
        }
    }

    public enum SplitName
    {
        [Description("None"), ToolTip("None")]
        None,
        [Description("Inventory"), ToolTip("Splits when you pick up or drop a certain item")]
        Inventory,
        [Description("Blueprint"), ToolTip("Splits when you unlock a certain blueprint")]
        Blueprint,
        [Description("Encyclopedia"), ToolTip("Splits when you unlock a certain encyclopedia entry")]
        Encyclopedia,
        [Description("Biome"), ToolTip("Splits when you enter a biome or move between biomes")]
        Biome,
        [Description("Craft"), ToolTip("Splits when you start crafting a certain item")]
        Craft,
        [Description("Build"), ToolTip("Splits when you complete building a certain builder tool base piece or placeable")]
        Build,
        [Description("Full Inventory"), ToolTip("Splits when you have a full inventory")]
        FullInventorySplit,
        [Description("Throw Flare"), ToolTip("Splits when you throw a flare")]
        ThrowFlareSplit,
        [Description("Throw Snowball"), ToolTip("Splits when you throw a snowball")]
        ThrowSnowballSplit,
        [Description("Death"), ToolTip("Splits when you die")]
        DeathSplit,
        [Description("Propulsion Cannon Drown"), ToolTip("Splits when you die inside the Arctic Kelp Caves after unlocking the Propulsion Cannon blueprint")]
        PropulsionCannonDrownSplit,
        [Description("Twisty Bridges Death"), ToolTip("Splits when you die in the Twisty Bridges or Deep Twisty Bridges")]
        TwistyBridgesDeathSplit,
        [Description("Acquire Al-An"), ToolTip("Splits when you Insert Storage Medium for Al-An")]
        AcquireAlAnSplit,
        [Description("Booster Tank Death"), ToolTip("Splits when you Die in the Purple Vents after Unlocking the Booster Tank Blueprint")]
        BoosterTankDeathSplit,
        [Description("Arctic Spires Cache Scan"), ToolTip("Splits when you Scan the Architect Tissue")]
        ArcticSpiresScanSplit,
        [Description("Arctic Spires Cache Death"), ToolTip("Splits when you Die inside of the Arctic Spires Cache")]
        ArcticSpiresDeathSplit,
        [Description("Arctic Spires Cache Transition"), ToolTip("Splits when you leave Arctic Spires Cache for any other playable biome")]
        ArcticSpiresTransitionSplit,
        [Description("Deep Lilypads Cache Scan"), ToolTip("Splits when you Scan the Architect Skeleton")]
        DeepLilypadsCacheScanSplit,
        [Description("Deep Lilypads Cache Death"), ToolTip("Splits when you die inside of the Deep Lilypads Cache")]
        DeepLilypadCacheDeathSplit,
        [Description("Deep Lilypads Cache Transition"), ToolTip("Splits when you leave Deep Lilypads Cache for any other playable biome")]
        DeepLilypadsCacheTransitionSplit,
        [Description("Crystal Caves Cache Scan"), ToolTip("Splits when you Scan the Architect Organs")]
        CrystalCavesCacheScanSplit,
        [Description("Crystal Caves Cache Death"), ToolTip("Splits when you Die inside of the Crystal Caves Cache")]
        CrystalCavesCacheDeathSplit,
        [Description("Crystal Caves Cache Transition"), ToolTip("Splits when you leave Crystal Caves Cache for any other playable biome")]
        CrystalCavesCacheTransitionSplit,
        [Description("Build Storage Medium"), ToolTip("Splits when you Interact to Commence building Al-An's Storage Medium")]
        CommenceStorageMediumFabricationSplit,
        [Description("Al-An Transfer"), ToolTip("Splits when you Interact to Initiate Transfer with Al-An's Storage Medium")]
        AlAnTransferSplit,
        [Description("Al-An Transfer Death"), ToolTip("Splits when you die inside of the Fabrication Facility")]
        AlAnTransferDeathSplit,
        [Description("Brace"), ToolTip("Splits when you Interact with the Brace button inside Al-An's Vessel")]
        BraceSplit,
        [Description("Insert Hydraulics Fluid"), ToolTip("Splits when you Insert Hydraulics Fluid into the Bridge Controls in the Glacial Bay")]
        InsertHydraulicsFluidSplit,
    }
    public class ToolTipAttribute : Attribute
    {
        public string ToolTip { get; set; }
        public ToolTipAttribute(string text)
        {
            ToolTip = text;
        }
    }
}
