using LiveSplit.Model;
using LiveSplit.BelowZero.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Voxif.AutoSplitter;

namespace LiveSplit.BelowZero
{
    public class BelowZeroBaseSettings : UserControl
    {
        private const string EditButtonGlyph = "\u270E";
        private const string ConfirmEditGlyph = "\u2714";
        private const string OptionsButtonGlyph = "\u2699";

        public List<BelowZeroSplit> Splits { get; set; }
        public virtual RadioButton Alpha { get; set; }
        private GameVersion currentGameVersion = GameVersion.Oct2025;


        public LiveSplitState State;
        public bool IsLoading = false;

        private BelowZeroSplitSetting _dragItem;
        private int _dragTargetIndex = -1;
        private int _insertMarkerY = -1;
        private Control _insertBeforeControl;
        private Control _highlightControl;

        public virtual FlowLayoutPanel MainPanel { get; set; }
        public virtual FlowLayoutPanel Options { get; set; }

        #region Buttons
        public void BtnAddSplitClick(object sender, EventArgs e, bool isSubCondition = false)
        {
            var dialog = new SelectSplitType(this, isSubCondition);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var setting = dialog.Func(isSubCondition);
                MainPanel.Controls.Add(setting);
                UpdateSplits();
            }
        }

        public void BtnRemoveClick(object sender, EventArgs e)
        {
            for (int i = MainPanel.Controls.Count - 1; i > 0; i--)
            {
                if (MainPanel.Controls[i].Contains((Control)sender))
                {
                    RemoveHandlers((BelowZeroSplitSetting)((Button)sender).Parent);

                    MainPanel.Controls.RemoveAt(i);
                    break;
                }
            }
            UpdateSplits();
        }

        public void BtnEditClick(object sender, EventArgs e)
        {
            foreach (var setting in MainPanel.Controls.OfType<BelowZeroSplitSetting>())
            {
                if (ReferenceEquals(setting.BtnEdit, sender))
                {
                    bool anyEnabled = setting.ComboBox.Enabled || (setting.ComboBox2?.Enabled ?? false);
                    if (anyEnabled) DisableEdit(setting);
                    else EnableEdit(setting);
                    NormalizeButtonGlyphs(setting);
                    break;
                }
            }
        }
        #endregion Buttons

        public void RdSortCheckedChanged(object sender, EventArgs e)
        {
            MainPanel.SuspendLayout();

            foreach (var setting in MainPanel.Controls.OfType<BelowZeroSplitSetting>())
                ApplyDataSources(setting, Alpha.Checked);

            MainPanel.ResumeLayout();
        }

        private void EnableEdit(BelowZeroSplitSetting setting)
        {
            setting.BtnEdit.Text = ConfirmEditGlyph;
            ApplyDataSources(setting, Alpha.Checked);
            setting.ComboBox.Enabled = true;
            if (setting.ComboBox2 != null)
                setting.ComboBox2.Enabled = true;
        }

        private void DisableEdit(BelowZeroSplitSetting setting)
        {
            setting.BtnEdit.Text = EditButtonGlyph;
            setting.ComboBox.Enabled = false;
            if (setting.ComboBox2 != null)
                setting.ComboBox2.Enabled = false;
        }

        public virtual void ControlChanged(object sender, EventArgs e) => UpdateSplits();

        public virtual void UpdateSplits()
        {
            if (IsLoading)
                return;

            Splits.Clear();
            foreach (var setting in MainPanel.Controls.OfType<BelowZeroSplitSetting>())
                if (!string.IsNullOrEmpty(setting.ComboBox.Text))
                    Splits.Add(setting.Split);
        }

        private static List<ComboItem<TEnum>> BuildEnumList<TEnum>(int skip, Func<TEnum, bool> include = null) where TEnum : struct
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Skip(skip)
                .Where(value => include == null || include(value))
                .Select(value => new ComboItem<TEnum>
                {
                    Value = value,
                    Display = typeof(TEnum) == typeof(Biome)
                        ? value.ToString()
                        : Localization.GetDisplayName(value),
                })
                .ToList();
        }

        private static readonly StringComparer AlphaComparer = StringComparer.OrdinalIgnoreCase;

        private static readonly HashSet<Craftable> BuilderBuildables = new HashSet<Craftable>
        {
            Craftable.Workbench,
            Craftable.Fabricator,
            Craftable.Aquarium,
            Craftable.Locker,
            Craftable.Spotlight,
            Craftable.CurrentGenerator,
            Craftable.SolarPanel,
            Craftable.Sign,
            Craftable.PowerTransmitter,
            Craftable.ThermalPlant,
            Craftable.SmallLocker,
            Craftable.Bench,
            Craftable.PictureFrame,
            Craftable.PlanterPot,
            Craftable.PlanterBox,
            Craftable.PlanterShelf,
            Craftable.FarmingTray,
            Craftable.FiltrationMachine,
            Craftable.Techlight,
            Craftable.PlanterPot2,
            Craftable.PlanterPot3,
            Craftable.SingleWallShelf,
            Craftable.WallShelves,
            Craftable.Bed1,
            Craftable.Bed2,
            Craftable.NarrowBed,
            Craftable.BatteryCharger,
            Craftable.PowerCellCharger,
            Craftable.BaseColorCustomizer,
            Craftable.Jukebox,
            Craftable.Speaker,
            Craftable.QuantumLocker,
            Craftable.Recyclotron,
            Craftable.StarshipDesk,
            Craftable.StarshipChair,
            Craftable.StarshipChair2,
            Craftable.StarshipChair3,
            Craftable.CoffeeVendingMachine,
            Craftable.BarTable,
            Craftable.Trashcans,
            Craftable.LabTrashcan,
            Craftable.VendingMachine,
            Craftable.LabCounter,
            Craftable.Snowman,
            Craftable.Fridge,
            Craftable.Shower,
            Craftable.Sink,
            Craftable.SmallStove,
            Craftable.Toilet,
            Craftable.EmmanuelPendulum,
            Craftable.AromatherapyLamp,
            Craftable.BedDanielle,
            Craftable.BedEmmanuel,
            Craftable.BedFred,
            Craftable.BedJeremiah,
            Craftable.BedSam,
            Craftable.BedZeta,
            Craftable.ExecutiveDesk,
            Craftable.BedParvan,
            Craftable.BaseRoom,
            Craftable.BaseHatch,
            Craftable.BaseWall,
            Craftable.BaseDoor,
            Craftable.BaseLadder,
            Craftable.BaseWindow,
            Craftable.BaseCorridor,
            Craftable.BaseFoundation,
            Craftable.BaseCorridorI,
            Craftable.BaseCorridorL,
            Craftable.BaseCorridorT,
            Craftable.BaseCorridorX,
            Craftable.BaseReinforcement,
            Craftable.BaseBulkhead,
            Craftable.BaseCorridorGlassI,
            Craftable.BaseCorridorGlassL,
            Craftable.BaseObservatory,
            Craftable.BaseConnector,
            Craftable.BaseMoonpool,
            Craftable.BaseCorridorGlass,
            Craftable.BaseUpgradeConsole,
            Craftable.BasePlanter,
            Craftable.BaseFiltrationMachine,
            Craftable.BaseWaterPark,
            Craftable.BaseMapRoom,
            Craftable.BaseBioReactor,
            Craftable.BaseNuclearReactor,
            Craftable.BasePipeConnector,
            Craftable.BaseRechargePlatform,
            Craftable.BaseControlRoom,
            Craftable.BaseWallFoundation,
            Craftable.BaseLargeRoom,
            Craftable.BasePartitionDoor,
            Craftable.BaseGlassDome,
            Craftable.BasePartition,
            Craftable.BaseLargeGlassDome,
            Craftable.Hoverpad,
            Craftable.BaseMoonpoolExpansion,
        };

        public static readonly Lazy<IReadOnlyList<ComboItem<SplitName>>> Prefabs =
            new Lazy<IReadOnlyList<ComboItem<SplitName>>>(() =>
                new[]
                {
                    SplitName.FullInventorySplit,
                    SplitName.ThrowFlareSplit,
                    SplitName.ThrowSnowballSplit,
                    SplitName.DeathSplit,
                    SplitName.EnterBaseSplit,
                    SplitName.ExitBaseSplit,
                    SplitName.PropulsionCannonDrownSplit,
                    SplitName.TwistyBridgesDeathSplit,
                    SplitName.AcquireAlAnSplit,
                    SplitName.BoosterTankDeathSplit,
                    SplitName.ArcticSpiresScanSplit,
                    SplitName.ArcticSpiresDeathSplit,
                    SplitName.ArcticSpiresTransitionSplit,
                    SplitName.DeepLilypadsCacheScanSplit,
                    SplitName.DeepLilypadCacheDeathSplit,
                    SplitName.DeepLilypadsCacheTransitionSplit,
                    SplitName.CrystalCavesCacheScanSplit,
                    SplitName.CrystalCavesCacheDeathSplit,
                    SplitName.CrystalCavesCacheTransitionSplit,
                    SplitName.CommenceStorageMediumFabricationSplit,
                    SplitName.AlAnTransferSplit,
                    SplitName.AlAnTransferDeathSplit,
                    SplitName.BraceSplit,
                    SplitName.InsertHydraulicsFluidSplit,
                    SplitName.InsertTestOverrideModuleSplit,
                    SplitName.CureFrozenLeviathanSplit,
                }
                .Select(x => new ComboItem<SplitName> { Value = x, Display = x.GetDescription() })
                .ToList());

        public static readonly Lazy<IReadOnlyList<ComboItem<SplitName>>> PrefabAlpha =
            new Lazy<IReadOnlyList<ComboItem<SplitName>>>(() => Prefabs.Value.OrderBy(x => x.Display ?? string.Empty, AlphaComparer).ToList());

        private static readonly Lazy<IReadOnlyList<ComboItem<InventoryItem>>> Items =
            new Lazy<IReadOnlyList<ComboItem<InventoryItem>>>(() => BuildEnumList<InventoryItem>(1));

        private static readonly Lazy<IReadOnlyList<ComboItem<Unlockable>>> Blueprints =
            new Lazy<IReadOnlyList<ComboItem<Unlockable>>>(() => BuildEnumList<Unlockable>(1));

        private static readonly Lazy<IReadOnlyList<ComboItem<EncyclopediaEntry>>> EncyclopediaEntries =
            new Lazy<IReadOnlyList<ComboItem<EncyclopediaEntry>>>(() => BuildEnumList<EncyclopediaEntry>(1));

        private static readonly Lazy<IReadOnlyList<ComboItem<Artifact>>> Artifacts =
            new Lazy<IReadOnlyList<ComboItem<Artifact>>>(() => BuildEnumList<Artifact>(1));

        private static readonly Lazy<IReadOnlyList<ComboItem<StoryGoal>>> StoryGoals =
            new Lazy<IReadOnlyList<ComboItem<StoryGoal>>>(() =>
                Enum.GetValues(typeof(StoryGoal))
                    .Cast<StoryGoal>()
                    .Where(value => value != StoryGoal.None
                        && !value.ToString().StartsWith("Achievement", StringComparison.Ordinal))
                    .Select(value => new ComboItem<StoryGoal> { Value = value, Display = value.ToString() })
                    .ToList());

        private static readonly Lazy<IReadOnlyList<ComboItem<Achievement>>> Achievements =
            new Lazy<IReadOnlyList<ComboItem<Achievement>>>(() =>
                Enum.GetValues(typeof(Achievement))
                    .Cast<Achievement>()
                    .Where(value => value != Achievement.None)
                    .Select(value => new ComboItem<Achievement> { Value = value, Display = value.GetDescription() })
                    .ToList());

        private static readonly Lazy<IReadOnlyList<ComboItem<Biome>>> Biomes =
            new Lazy<IReadOnlyList<ComboItem<Biome>>>(() => BuildEnumList<Biome>(1));

        private static readonly Lazy<IReadOnlyList<ComboItem<Craftable>>> Craftables =
            new Lazy<IReadOnlyList<ComboItem<Craftable>>>(() => BuildEnumList<Craftable>(1, value => !BuilderBuildables.Contains(value)));

        private static readonly Lazy<IReadOnlyList<ComboItem<Craftable>>> Buildables =
            new Lazy<IReadOnlyList<ComboItem<Craftable>>>(() => BuildEnumList<Craftable>(1, BuilderBuildables.Contains));

        private static readonly Lazy<IReadOnlyList<ComboItem<InventoryItem>>> ItemsAlpha =
            new Lazy<IReadOnlyList<ComboItem<InventoryItem>>>(() => Items.Value.OrderBy(x => x.Display ?? string.Empty, AlphaComparer).ToList());

        private static readonly Lazy<IReadOnlyList<ComboItem<Unlockable>>> BlueprintsAlpha =
            new Lazy<IReadOnlyList<ComboItem<Unlockable>>>(() => Blueprints.Value.OrderBy(x => x.Display ?? string.Empty, AlphaComparer).ToList());

        private static readonly Lazy<IReadOnlyList<ComboItem<EncyclopediaEntry>>> EncyclopediaEntriesAlpha =
            new Lazy<IReadOnlyList<ComboItem<EncyclopediaEntry>>>(() => EncyclopediaEntries.Value.OrderBy(x => x.Display ?? string.Empty, AlphaComparer).ToList());

        private static readonly Lazy<IReadOnlyList<ComboItem<Artifact>>> ArtifactsAlpha =
            new Lazy<IReadOnlyList<ComboItem<Artifact>>>(() => Artifacts.Value.OrderBy(x => x.Display ?? string.Empty, AlphaComparer).ToList());

        private static readonly Lazy<IReadOnlyList<ComboItem<StoryGoal>>> StoryGoalsAlpha =
            new Lazy<IReadOnlyList<ComboItem<StoryGoal>>>(() => StoryGoals.Value.OrderBy(x => x.Display ?? string.Empty, AlphaComparer).ToList());

        private static readonly Lazy<IReadOnlyList<ComboItem<Achievement>>> AchievementsAlpha =
            new Lazy<IReadOnlyList<ComboItem<Achievement>>>(() => Achievements.Value.OrderBy(x => x.Display ?? string.Empty, AlphaComparer).ToList());

        private static readonly Lazy<IReadOnlyList<ComboItem<Biome>>> BiomesAlpha =
            new Lazy<IReadOnlyList<ComboItem<Biome>>>(() => Biomes.Value.OrderBy(x => x.Display ?? string.Empty, AlphaComparer).ToList());

        private static readonly Lazy<IReadOnlyList<ComboItem<Craftable>>> CraftablesAlpha =
            new Lazy<IReadOnlyList<ComboItem<Craftable>>>(() => Craftables.Value.OrderBy(x => x.Display ?? string.Empty, AlphaComparer).ToList());

        private static readonly Lazy<IReadOnlyList<ComboItem<Craftable>>> BuildablesAlpha =
            new Lazy<IReadOnlyList<ComboItem<Craftable>>>(() => Buildables.Value.OrderBy(x => x.Display ?? string.Empty, AlphaComparer).ToList());

        private IReadOnlyList<ComboItem<TEnum>> ForCurrentVersion<TEnum>(
            IReadOnlyList<ComboItem<TEnum>> values,
            Func<TEnum, bool> availableInAug2021)
        {
            return currentGameVersion == GameVersion.Aug2021
                ? values.Where(item => availableInAug2021(item.Value)).ToList()
                : values;
        }

        private IReadOnlyList<ComboItem<Unlockable>> GetBlueprints(bool alpha) =>
            ForCurrentVersion(alpha ? BlueprintsAlpha.Value : Blueprints.Value, value => value != Unlockable.BaseMoonpoolExpansion);

        private IReadOnlyList<ComboItem<EncyclopediaEntry>> GetEncyclopediaEntries(bool alpha) =>
            ForCurrentVersion(alpha ? EncyclopediaEntriesAlpha.Value : EncyclopediaEntries.Value, value => value != EncyclopediaEntry.MoonpoolExpansion);

        private IReadOnlyList<ComboItem<Craftable>> GetBuildables(bool alpha) =>
            ForCurrentVersion(alpha ? BuildablesAlpha.Value : Buildables.Value, value => value != Craftable.BaseMoonpoolExpansion);

        public void AddHandlers(BelowZeroSplitSetting setting)
        {
            setting.ComboBox.SelectedIndexChanged += new EventHandler(ControlChanged);
            setting.BtnRemove.Click += new EventHandler(BtnRemoveClick);
            setting.BtnEdit.Click += new EventHandler(BtnEditClick);
            NormalizeButtonGlyphs(setting);
        }

        public void RemoveHandlers(BelowZeroSplitSetting setting)
        {
            setting.ComboBox.SelectedIndexChanged -= ControlChanged;
            setting.BtnRemove.Click -= BtnRemoveClick;
            setting.BtnEdit.Click -= BtnEditClick;
        }

        private static void NormalizeButtonGlyphs(BelowZeroSplitSetting setting)
        {
            if (setting == null)
                return;

            bool isEditing = setting.ComboBox.Enabled || (setting.ComboBox2?.Enabled ?? false);
            setting.BtnEdit.Text = isEditing ? ConfirmEditGlyph : EditButtonGlyph;

            foreach (Button button in setting.Controls.OfType<Button>())
            {
                if (string.Equals(button.Name, "BtnOptions", StringComparison.Ordinal))
                    button.Text = OptionsButtonGlyph;
            }
        }

        internal void SetGameVersion(GameVersion version)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<GameVersion>(SetGameVersion), version);
                }
                catch (InvalidOperationException)
                {
                }
                return;
            }

            currentGameVersion = version;

            if (MainPanel == null || Alpha == null || MainPanel.Controls.Count == 0)
                return;

            MainPanel.SuspendLayout();
            try
            {
                foreach (var setting in MainPanel.Controls.OfType<BelowZeroSplitSetting>())
                    ApplyDataSources(setting, Alpha.Checked);
            }
            finally
            {
                MainPanel.ResumeLayout();
            }
        }

        public void ApplyDataSources(BelowZeroSplitSetting setting, bool alpha)
        {
            switch (setting)
            {
                case BelowZeroItemSplit _:
                    BindCombo(setting.ComboBox, alpha ? ItemsAlpha.Value : Items.Value, setting.ComboBox.SelectedValue, alpha);
                    break;
                case BelowZeroBlueprintSplit _:
                    BindCombo(setting.ComboBox, GetBlueprints(alpha), setting.ComboBox.SelectedValue, alpha);
                    break;
                case BelowZeroEncyclopediaSplit artifactSetting when artifactSetting.Split is ArtifactSplit:
                    BindCombo(setting.ComboBox, alpha ? ArtifactsAlpha.Value : Artifacts.Value, setting.ComboBox.SelectedValue, alpha);
                    break;
                case BelowZeroEncyclopediaSplit storyGoalSetting when storyGoalSetting.Split is StoryGoalSplit:
                    BindCombo(setting.ComboBox, alpha ? StoryGoalsAlpha.Value : StoryGoals.Value, setting.ComboBox.SelectedValue, alpha);
                    break;
                case BelowZeroEncyclopediaSplit achievementSetting when achievementSetting.Split is AchievementSplit:
                    BindCombo(setting.ComboBox, alpha ? AchievementsAlpha.Value : Achievements.Value, setting.ComboBox.SelectedValue, alpha);
                    break;
                case BelowZeroEncyclopediaSplit _:
                    BindCombo(setting.ComboBox, GetEncyclopediaEntries(alpha), setting.ComboBox.SelectedValue, alpha);
                    break;
                case BelowZeroBiomeSplit _:
                    BindCombo(setting.ComboBox, alpha ? BiomesAlpha.Value : Biomes.Value, setting.ComboBox.SelectedValue, alpha);
                    BindCombo(setting.ComboBox2, alpha ? BiomesAlpha.Value : Biomes.Value, setting.ComboBox2.SelectedValue ?? setting.ComboBox.SelectedValue, alpha);
                    break;
                case BelowZeroCraftSplit craftSetting when craftSetting.Split is BuildSplit:
                    BindCombo(setting.ComboBox, GetBuildables(alpha), setting.ComboBox.SelectedValue, alpha);
                    break;
                case BelowZeroCraftSplit _:
                    BindCombo(setting.ComboBox, alpha ? CraftablesAlpha.Value : Craftables.Value, setting.ComboBox.SelectedValue, alpha);
                    break;
                default:
                    BindCombo(setting.ComboBox, alpha ? PrefabAlpha.Value : Prefabs.Value, setting.ComboBox.SelectedValue, alpha);
                    break;
            }
        }

        private T CreateSplit<T, TEnum>(IEnumerable<ComboItem<TEnum>> data, Func<T, ComboBox> getCombo, bool isSubCondition = false) where T : BelowZeroSplitSetting, new()
        {
            var setting = new T();           
            var combo = getCombo(setting);
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.MouseWheel += (o, e) => ((HandledMouseEventArgs)e).Handled = true;

            combo.DisplayMember = "Display";
            combo.ValueMember = "Value";
            combo.DataSource = data.ToList();

            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;

            setting.IsSubCondition = isSubCondition;            
            setting.Split.IsSubCondition = isSubCondition;            
            AddHandlers(setting);
            return setting;
        }

        private BelowZeroCraftSplit CreateCraftableSplit(CraftableSplitBase split, IEnumerable<ComboItem<Craftable>> data, bool isSubCondition)
        {
            var setting = new BelowZeroCraftSplit(split);
            var combo = setting.cboCraftables;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.MouseWheel += (o, e) => ((HandledMouseEventArgs)e).Handled = true;
            combo.DisplayMember = "Display";
            combo.ValueMember = "Value";
            combo.DataSource = data.ToList();

            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;

            setting.IsSubCondition = isSubCondition;
            setting.Split.IsSubCondition = isSubCondition;
            AddHandlers(setting);
            return setting;
        }

        public BelowZeroPrefabSplit CreatePrefabSplit(bool isSubCondition) => CreateSplit<BelowZeroPrefabSplit, SplitName>(Alpha.Checked ? PrefabAlpha.Value : Prefabs.Value, s => s.cboName, isSubCondition);
        public BelowZeroItemSplit CreateItemSplit(bool isSubCondition) => CreateSplit<BelowZeroItemSplit, InventoryItem>(Alpha.Checked ? ItemsAlpha.Value : Items.Value, s => s.cboItem, isSubCondition);
        public BelowZeroBlueprintSplit CreateBlueprintSplit(bool isSubCondition) => CreateSplit<BelowZeroBlueprintSplit, Unlockable>(GetBlueprints(Alpha.Checked), s => s.cboBlueprint, isSubCondition);
        public BelowZeroEncyclopediaSplit CreateEncyclopediaSplit(bool isSubCondition) => CreateSplit<BelowZeroEncyclopediaSplit, EncyclopediaEntry>(GetEncyclopediaEntries(Alpha.Checked), s => s.cboEncy, isSubCondition);
        public BelowZeroEncyclopediaSplit CreateArtifactSplit(bool isSubCondition)
        {
            var setting = new BelowZeroEncyclopediaSplit(new ArtifactSplit(Artifact.None, onlySplitOnce: true, isSubCondition));
            BindCombo(setting.cboEncy, Alpha.Checked ? ArtifactsAlpha.Value : Artifacts.Value, null, Alpha.Checked);
            setting.IsSubCondition = isSubCondition;
            setting.Split.IsSubCondition = isSubCondition;
            AddHandlers(setting);
            return setting;
        }
        public BelowZeroEncyclopediaSplit CreateStoryGoalSplit(bool isSubCondition)
        {
            var setting = new BelowZeroEncyclopediaSplit(new StoryGoalSplit(StoryGoal.None, onlySplitOnce: true, isSubCondition));
            BindCombo(setting.cboEncy, Alpha.Checked ? StoryGoalsAlpha.Value : StoryGoals.Value, null, Alpha.Checked);
            setting.IsSubCondition = isSubCondition;
            setting.Split.IsSubCondition = isSubCondition;
            AddHandlers(setting);
            return setting;
        }
        public BelowZeroEncyclopediaSplit CreateAchievementSplit(bool isSubCondition)
        {
            var setting = new BelowZeroEncyclopediaSplit(new AchievementSplit(Achievement.None, onlySplitOnce: true, isSubCondition));
            BindCombo(setting.cboEncy, Alpha.Checked ? AchievementsAlpha.Value : Achievements.Value, null, Alpha.Checked);
            setting.IsSubCondition = isSubCondition;
            setting.Split.IsSubCondition = isSubCondition;
            AddHandlers(setting);
            return setting;
        }
        public BelowZeroCraftSplit CreateCraftSplit(bool isSubCondition) => CreateCraftableSplit(new CraftSplit(Craftable.None, onlySplitOnce: true, isSubCondition: false), Alpha.Checked ? CraftablesAlpha.Value : Craftables.Value, isSubCondition);
        public BelowZeroCraftSplit CreateBuildSplit(bool isSubCondition) => CreateCraftableSplit(new BuildSplit(Craftable.None, onlySplitOnce: true, isSubCondition: false), GetBuildables(Alpha.Checked), isSubCondition);
        public BelowZeroBiomeSplit CreateBiomeSplit(bool isSubCondition)
        {
            var setting = new BelowZeroBiomeSplit();
            var data = Alpha.Checked ? BiomesAlpha.Value : Biomes.Value;
            BindCombo(setting.cboBiome1, data, null, Alpha.Checked);
            BindCombo(setting.cboBiome2, data, null, Alpha.Checked);
            setting.IsSubCondition = isSubCondition;
            setting.Split.IsSubCondition = isSubCondition;
            AddHandlers(setting);

            if (isSubCondition)
            {
                setting.ComboBox2.Visible = false;
                setting.pictureBox1.Visible = false;
                setting.picHandle.Left = 3;
                setting.picHandle.Top = 13;
                setting.btnEdit.Top = 16;
                setting.btnRemove.Top = 16;
                setting.BtnOptions.Top = 16;
            }

            return setting;
        }

        private static void BindCombo<T>(ComboBox combo, IEnumerable<ComboItem<T>> data, object previousSelected, bool alpha)
        {
            combo.BeginUpdate();
            try
            {
                combo.BindingContext = new BindingContext();
                var list = BuildBoundList(data, previousSelected, alpha);
                combo.DisplayMember = "Display";
                combo.ValueMember = "Value";
                combo.DataSource = list;
                if (previousSelected is T t) combo.SelectedValue = t;
            }
            finally
            {
                combo.EndUpdate();
            }
        }

        private static List<ComboItem<T>> BuildBoundList<T>(IEnumerable<ComboItem<T>> data, object previousSelected, bool alpha)
        {
            var list = (data as IList<ComboItem<T>>)?.ToList() ?? data.ToList();

            if (previousSelected is T selected
                && !list.Any(item => EqualityComparer<T>.Default.Equals(item.Value, selected)))
            {
                list.Add(new ComboItem<T>
                {
                    Value = selected,
                    Display = GetMissingDisplay(selected),
                });

                if (alpha)
                    list = list.OrderBy(item => item.Display ?? string.Empty, AlphaComparer).ToList();
            }

            return list;
        }

        private static string GetMissingDisplay<T>(T value)
        {
            if (value is SplitName splitName)
                return splitName.GetDescription();

            if (value is StoryGoal storyGoal)
                return storyGoal.ToString();

            if (value is Achievement achievement)
                return achievement.GetDescription();

            return Localization.GetDisplayName(value);
        }

        public virtual void LoadSettings()
        {
            BelowZeroSplitSetting[] settings = new BelowZeroSplitSetting[Splits.Count];

            try
            {
                MainPanel.SuspendLayout();

                for (int i = MainPanel.Controls.Count - 1; i > 0; i--)
                    MainPanel.Controls.RemoveAt(i);

                for (int i = 0; i < Splits.Count; i++)
                {
                    var split = Splits[i];
                    BelowZeroSplitSetting setting = new BelowZeroSplitSetting();
                    switch (split)
                    {
                        case ItemSplit s:
                            setting = new BelowZeroItemSplit(s) { IsLoadingGetter = () => this.IsLoading };
                            ApplyDataSources(setting, Alpha.Checked);
                            setting.ComboBox.SelectedValue = ((ItemSplit)setting.Split).Item;
                            break;

                        case BlueprintSplit s:
                            setting = new BelowZeroBlueprintSplit(s) { IsLoadingGetter = () => this.IsLoading };
                            ApplyDataSources(setting, Alpha.Checked);
                            setting.ComboBox.SelectedValue = ((BlueprintSplit)setting.Split).Blueprint;
                            break;

                        case ArtifactSplit s:
                            setting = new BelowZeroEncyclopediaSplit(s) { IsLoadingGetter = () => this.IsLoading };
                            ApplyDataSources(setting, Alpha.Checked);
                            setting.ComboBox.SelectedValue = s.Artifact;
                            break;

                        case StoryGoalSplit s:
                            setting = new BelowZeroEncyclopediaSplit(s) { IsLoadingGetter = () => this.IsLoading };
                            ApplyDataSources(setting, Alpha.Checked);
                            setting.ComboBox.SelectedValue = s.Goal;
                            break;

                        case AchievementSplit s:
                            setting = new BelowZeroEncyclopediaSplit(s) { IsLoadingGetter = () => this.IsLoading };
                            ApplyDataSources(setting, Alpha.Checked);
                            setting.ComboBox.SelectedValue = s.Achievement;
                            break;

                        case EncyclopediaSplit s:
                            setting = new BelowZeroEncyclopediaSplit(s) { IsLoadingGetter = () => this.IsLoading };
                            ApplyDataSources(setting, Alpha.Checked);
                            setting.ComboBox.SelectedValue = ((EncyclopediaSplit)setting.Split).Entry;
                            break;

                        case BiomeSplit s:
                            setting = new BelowZeroBiomeSplit(s) { IsLoadingGetter = () => this.IsLoading };
                            ApplyDataSources(setting, Alpha.Checked);
                            setting.ComboBox.SelectedValue = ((BiomeSplit)setting.Split).Biomes.Biome1;
                            setting.ComboBox2.SelectedValue = ((BiomeSplit)setting.Split).Biomes.Biome2;
                            break;

                        case CraftSplit s:
                            setting = new BelowZeroCraftSplit(s) { IsLoadingGetter = () => this.IsLoading };
                            ApplyDataSources(setting, Alpha.Checked);
                            setting.ComboBox.SelectedValue = ((CraftSplit)setting.Split).Craftable;
                            break;

                        case BuildSplit s:
                            setting = new BelowZeroCraftSplit(s) { IsLoadingGetter = () => this.IsLoading };
                            ApplyDataSources(setting, Alpha.Checked);
                            setting.ComboBox.SelectedValue = ((BuildSplit)setting.Split).Craftable;
                            break;

                        case PrefabSplit s:
                            setting = new BelowZeroPrefabSplit(s) { IsLoadingGetter = () => this.IsLoading };
                            ApplyDataSources(setting, Alpha.Checked);
                            setting.ComboBox.SelectedValue = ((PrefabSplit)setting.Split).SplitName;
                            break;

                        default: break;
                    }

                    setting.ComboBox.Enabled = false;
                    if (setting.ComboBox2 != null) setting.ComboBox2.Enabled = false;

                    AddHandlers(setting);
                    settings[i] = setting;
                }
            }
            finally
            {
                MainPanel.Controls.AddRange(settings);
                MainPanel.ResumeLayout();
            }
        }

        #region Dragging
        public void flowMainPaint(object sender, PaintEventArgs e)
        {
            if (_insertMarkerY < 0)
                return;

            using (var pen = new Pen(Color.DodgerBlue, 3))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                int margin = 4;

                e.Graphics.DrawLine(
                    pen,
                    margin,
                    _insertMarkerY,
                    MainPanel.ClientSize.Width - margin,
                    _insertMarkerY
                );
            }
        }
        public void flowMainDragEnter(object sender, DragEventArgs e)
        {
            _dragItem = GetDraggedSetting(e);

            if (_dragItem != null)
            {
                e.Effect = DragDropEffects.Move;
                _insertMarkerY = -1;
                MainPanel.Invalidate();
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        public void flowMainDragLeave(object sender, EventArgs e)
        {
            _dragItem = null;
            _dragTargetIndex = -1;
            _insertMarkerY = -1;
            _insertBeforeControl = null;

            if (_highlightControl != null)
            {
                _highlightControl.BackColor = SystemColors.Control;
                _highlightControl = null;
            }

            MainPanel.Invalidate();
        }

        public void flowMainDragDrop(object sender, DragEventArgs e)
        {
            var panel = (FlowLayoutPanel)sender;

            if (_dragItem != null && _dragTargetIndex > 0)
            {
                panel.SuspendLayout();

                int minIndex = GetMinSplitIndex(panel);

                int index = _dragTargetIndex;
                if (index < minIndex)
                    index = minIndex;
                if (index > panel.Controls.Count)
                    index = panel.Controls.Count;

                int oldIndex = panel.Controls.IndexOf(_dragItem);

                if (oldIndex >= 0 && index > oldIndex)
                    index--;

                if (index >= panel.Controls.Count)
                    index = panel.Controls.Count - 1;

                panel.Controls.SetChildIndex(_dragItem, index);

                panel.ResumeLayout();
            }

            _dragItem = null;
            _dragTargetIndex = -1;
            _insertMarkerY = -1;
            _insertBeforeControl = null;

            if (_highlightControl != null)
            {
                _highlightControl.BackColor = SystemColors.Control;
                _highlightControl = null;
            }

            MainPanel.Invalidate();

            UpdateSplits();
        }

        public void flowMainDragOver(object sender, DragEventArgs e)
        {
            var panel = (FlowLayoutPanel)sender;

            if (_dragItem == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Move;

            Point clientPoint = panel.PointToClient(new Point(e.X, e.Y));

            const int scrollRegion = 25;
            const int scrollStep = 15;

            if (clientPoint.Y < scrollRegion &&
                panel.VerticalScroll.Value > panel.VerticalScroll.Minimum)
            {
                panel.VerticalScroll.Value = Math.Max(
                    panel.VerticalScroll.Value - scrollStep,
                    panel.VerticalScroll.Minimum);
            }
            else if (clientPoint.Y > panel.ClientSize.Height - scrollRegion &&
                     panel.VerticalScroll.Value < panel.VerticalScroll.Maximum)
            {
                panel.VerticalScroll.Value = Math.Min(
                    panel.VerticalScroll.Value + scrollStep,
                    panel.VerticalScroll.Maximum);
            }

            int minIndex = GetMinSplitIndex(panel);
            int targetIndex = minIndex;

            Control insertBefore = null;

            for (int i = minIndex; i < panel.Controls.Count; i++)
            {
                var c = panel.Controls[i];
                if (c == _dragItem)
                    continue;

                int controlMidY = c.Bounds.Top + c.Bounds.Height / 2;

                if (clientPoint.Y < controlMidY)
                {
                    targetIndex = i;
                    insertBefore = c;
                    break;
                }

                targetIndex = i + 1;
            }

            if (insertBefore == null && panel.Controls.Count > 0)
            {
                targetIndex = panel.Controls.Count;
                insertBefore = null;
            }

            if (targetIndex < minIndex)
                targetIndex = minIndex;
            if (targetIndex > panel.Controls.Count)
                targetIndex = panel.Controls.Count;

            _dragTargetIndex = targetIndex;
            _insertBeforeControl = insertBefore;

            Control newHighlight = _insertBeforeControl;

            if (newHighlight != null && !(newHighlight is BelowZeroSplitSetting))
            {
                newHighlight = null;
            }

            if (_highlightControl != newHighlight)
            {
                if (_highlightControl != null)
                    _highlightControl.BackColor = SystemColors.Control;

                _highlightControl = newHighlight;

                if (_highlightControl != null)
                    _highlightControl.BackColor = Color.FromArgb(230, 240, 255);
            }

            if (_insertBeforeControl != null)
            {
                _insertMarkerY = _insertBeforeControl.Bounds.Top;
            }
            else if (panel.Controls.Count > 0)
            {
                var last = panel.Controls[panel.Controls.Count - 1];
                _insertMarkerY = last.Bounds.Bottom;
            }
            else
            {
                _insertMarkerY = -1;
            }

            panel.Invalidate();
        }
        int GetMinSplitIndex(FlowLayoutPanel panel)
        {
            int settingsIndex = panel.Controls.IndexOf(Options);
            if (settingsIndex < 0)
                settingsIndex = 0;
            return settingsIndex + 1;
        }
        private BelowZeroSplitSetting GetDraggedSetting(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(BelowZeroSplitSetting)))
                return (BelowZeroSplitSetting)e.Data.GetData(typeof(BelowZeroSplitSetting));
            if (e.Data.GetDataPresent(typeof(BelowZeroBlueprintSplit)))
                return (BelowZeroSplitSetting)e.Data.GetData(typeof(BelowZeroBlueprintSplit));
            if (e.Data.GetDataPresent(typeof(BelowZeroItemSplit)))
                return (BelowZeroSplitSetting)e.Data.GetData(typeof(BelowZeroItemSplit));
            if (e.Data.GetDataPresent(typeof(BelowZeroPrefabSplit)))
                return (BelowZeroSplitSetting)e.Data.GetData(typeof(BelowZeroPrefabSplit));
            if (e.Data.GetDataPresent(typeof(BelowZeroEncyclopediaSplit)))
                return (BelowZeroSplitSetting)e.Data.GetData(typeof(BelowZeroEncyclopediaSplit));
            if (e.Data.GetDataPresent(typeof(BelowZeroBiomeSplit)))
                return (BelowZeroSplitSetting)e.Data.GetData(typeof(BelowZeroBiomeSplit));
            if (e.Data.GetDataPresent(typeof(BelowZeroCraftSplit)))
                return (BelowZeroSplitSetting)e.Data.GetData(typeof(BelowZeroCraftSplit));

            return null;
        }
        #endregion Dragging
    }

    public sealed class ComboItem<T>
    {
        public T Value { get; set; }
        public string Display { get; set; }
    }
}

