using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Voxif.AutoSplitter;
using Voxif.Helpers.Unity;
using Voxif.IO;
using Voxif.Memory;
using ProcessModule = Voxif.Memory.ProcessModule;

namespace LiveSplit.BelowZero
{
    public class BelowZeroMemory : Memory
    {
        protected override string[] ProcessNames => new[] { "SubnauticaZero" };

        public BelowZeroSplit CurrentSplitToCheck { get; set; }

        private IMonoHelper mono;
        private readonly BelowZeroSettings settings;
        private UnityHelperTask unityTask;

        public bool startedTimerBefore;
        public bool isInMainMenu;
        public bool wasInMainMenu;
        public bool pointersInitialized;
        private GameVersion gameVersion = GameVersion.Oct2025;

        private const int inventoryCapacitySlots = 48;
        private const int maxInventoryTimeWithoutChangingMs = 1000;
        private const int maxBuilderMenuSelectionWindowMs = 3000;
        private const int minThrowSplitCooldownMs = 250;
        private const int maxBraceInteractionWindowMs = 120000;
        public readonly Dictionary<SplitName, Func<bool>> splitConditions;
        public readonly Dictionary<SplitName, Func<bool>> subConditions;

        private readonly Dictionary<TechType, InvChangeInfo> curPickUpCounts = new Dictionary<TechType, InvChangeInfo>();
        private readonly Dictionary<TechType, InvChangeInfo> curDropCounts = new Dictionary<TechType, InvChangeInfo>();
        private Dictionary<TechType, int> currentInventoryChanges = new Dictionary<TechType, int>();
        private readonly Stopwatch throwSplitCooldown = new Stopwatch();
        private readonly Stopwatch builderMenuSelectionWindow = new Stopwatch();
        private readonly Stopwatch braceInteractionWindow = new Stopwatch();
        private TechType lastThrowSplitTool = TechType.None;
        private TechType startedCraft = TechType.None;
        private TechType fallbackCompletedCraft = TechType.None;
        private TechType completedBuild = TechType.None;
        private TechType pendingBuilderCompletionTechType = TechType.None;
        private IntPtr trackedHoverpadConstructorPtr = IntPtr.Zero;
        private bool builderMenuSelectionPending;
        private bool braceReadyForCinematic;
        private bool craftAnalyticsInitialized;
        private bool hoverpadConstructingInitialized;
        private bool hoverpadConstructingOld;
        private bool inventoryInitialized;
        private bool blueprintsInitialized;
        private bool encyclopediaInitialized;
        private bool storyGoalsInitialized;
        private string storyGoalReaderStatus = string.Empty;
        private string storyGoalReaderMode = string.Empty;
        private int storyGoalSlotsOffset;
        private bool storyGoalUsesStringArray;
        public bool MovementStartArmed { get; set; }
        private int playerInventorySlotsUsed;
        private int playerInventorySlotsUsedOld;
        private int unityObjectCachedPtrOffset = 0x10;

        public Pointer<bool> IsIntroCinematicActive;
        private Pointer<bool> IsAnimationPlaying;
        public Pointer<bool> IsLoadingScreenShowing;
        public Pointer<bool> IsPlayerJumping;
        private Pointer<bool> DeathScreenActive;
        public Pointer<bool> PlayerControllerInputEnabled;
        private Pointer<float> PlayerControllerVelocityX;
        private Pointer<float> PlayerControllerVelocityZ;
        private Pointer<float> Health;
        private Pointer<float> MoveDirectionX;
        private Pointer<float> MoveDirectionZ;
        private Pointer<bool> BuilderMenuState;
        private Pointer<int> BuilderLastTechType;
        private Pointer<bool> PlayerIsUnderwater;
        private Pointer<bool> PlayerIsInside;
        private Pointer<IntPtr> MainMenu;
        private Pointer<IntPtr> BuilderPrefab;
        public Pointer<IntPtr> PlayerMain;
        public Pointer<IntPtr> CraftingMenu;
        private Pointer<IntPtr> GuiHandActiveTarget;
        private Pointer<IntPtr> craftingAnalyticsEntriesPtr;
        private Pointer<IntPtr> knownTechPtr;
        private Pointer<IntPtr> encyclopediaPtr;
        private Pointer<bool> PDAIsInUse;
        public Pointer<int> PDATab;
        private Pointer<int> CraftedNode;
        public StringPointer BiomeString;
        private StringPointer ActiveToolName;
        private Pointer<float> PlayerLastPositionX;
        private Pointer<float> PlayerLastPositionY;
        private Pointer<float> PlayerLastPositionZ;
        private Pointer<float> DirectPositionX;
        private Pointer<float> DirectPositionY;
        private Pointer<float> DirectPositionZ;
        private Pointer<IntPtr> completedStoryGoalsPtr;

        private Dictionary<TechType, int> PlayerInventory = new Dictionary<TechType, int>();
        private Dictionary<TechType, int> PlayerInventoryOld = new Dictionary<TechType, int>();
        private List<TechType> PlayerEquipment = new List<TechType>();
        private List<TechType> PlayerEquipmentOld = new List<TechType>();
        private List<TechType> KnownTech = new List<TechType>();
        private List<TechType> KnownTechOld = new List<TechType>();
        private List<EncyclopediaEntry> Encyclopedia = new List<EncyclopediaEntry>();
        private List<EncyclopediaEntry> EncyclopediaOld = new List<EncyclopediaEntry>();
        private HashSet<string> completedStoryGoals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> newlyCompletedStoryGoals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<TechType, int> craftCounts = new Dictionary<TechType, int>();
        private readonly HashSet<TechType> suppressCraftCompletionFallback = new HashSet<TechType>();

        private IntPtr invKlass;
        private IntPtr equipmentKlass;
        private IntPtr icKlass;
        private IntPtr invStaticKlass;

        private int invStaticOffset;
        private int off_container;
        private int off_equipment;
        private int off_equippedCount;
        private int off_itemsDict;
        private int off_itemGroup_items;
        private int off_itemGroup_id;
        private int off_itemGroup_width;
        private int off_itemGroup_height;
        private int off_list_size;
        private int off_list_items;
        private int dict_off_entries;
        private int dict_off_version;
        private int hashset_off_slots;
        private int arr_off_len;
        private int arr_data_base;
        private int off_hoverpadConstructorField;
        private int off_hoverpadTerminalHoverpad;
        private int off_hoverpadConstructorConstructing;

        public BelowZeroMemory(Logger logger, BelowZeroSettings settings)
            : base(logger)
        {
            this.settings = settings;

            OnHook += () =>
            {
                logger.Log("OnHook: starting Below Zero memory initialization.");
                GetGameVersion();
                unityTask = new UnityHelperTask(game, logger);
                unityTask.Run(helper =>
                {
                    try
                    {
                        InitPointers(helper);
                    }
                    catch (OperationCanceledException)
                    {
                        logger.Log("Pointer initialization canceled.");
                    }
                    catch (Exception ex)
                    {
                        pointersInitialized = false;
                        logger.Log($"Pointer initialization failed: {ex}");
                    }
                });
            };

            OnExit += () =>
            {
                logger.Log("OnExit: clearing Below Zero memory state.");
                pointersInitialized = false;
                gameVersion = GameVersion.Oct2025;
                ResetRunState();
                wasInMainMenu = false;
                isInMainMenu = false;
                storyGoalReaderStatus = string.Empty;
                storyGoalReaderMode = string.Empty;
                storyGoalSlotsOffset = 0;
                storyGoalUsesStringArray = false;
                DirectPositionX = null;
                DirectPositionY = null;
                DirectPositionZ = null;
                IsAnimationPlaying = null;
                IsIntroCinematicActive = null;

                if (unityTask != null)
                {
                    unityTask.Dispose();
                    unityTask = null;
                }
            };

            subConditions = CreateSubConditions();
            splitConditions = CreateSplitConditions();
        }

        private Dictionary<SplitName, Func<bool>> CreateSubConditions()
        {
            return new Dictionary<SplitName, Func<bool>>
            {
                { SplitName.Inventory, IsInventorySubConditionMet },
                { SplitName.Blueprint, () => KnowsTech(((BlueprintSplit)CurrentSplitToCheck).Blueprint.ConvertTo<TechType>()) },
                { SplitName.Encyclopedia, () => Encyclopedia.Contains(((EncyclopediaSplit)CurrentSplitToCheck).Entry) },
                { SplitName.Biome, IsBiomeSubConditionMet },
            };
        }

        private Dictionary<SplitName, Func<bool>> CreateSplitConditions()
        {
            return new Dictionary<SplitName, Func<bool>>
            {
                { SplitName.Inventory, IsInventorySplitTriggered },
                { SplitName.Blueprint, () => UnlockedTech(((BlueprintSplit)CurrentSplitToCheck).Blueprint.ConvertTo<TechType>()) },
                { SplitName.Encyclopedia, () => Encyclopedia.Contains(((EncyclopediaSplit)CurrentSplitToCheck).Entry) && !EncyclopediaOld.Contains(((EncyclopediaSplit)CurrentSplitToCheck).Entry) },
                { SplitName.Biome, IsBiomeSplitTriggered },
                { SplitName.Craft, IsCraftSplitTriggered },
                { SplitName.Build, () => TryConsumeCompletedBuild(((BuildSplit)CurrentSplitToCheck).Craftable.ConvertTo<TechType>()) },
                { SplitName.FullInventorySplit, () => playerInventorySlotsUsed == inventoryCapacitySlots && playerInventorySlotsUsedOld != inventoryCapacitySlots },
                { SplitName.DeathSplit, PlayerDied },
                { SplitName.ThrowFlareSplit, () => InventoryItemRemovedOutsidePDA(TechType.Flare, "Flare") },
                { SplitName.ThrowSnowballSplit, () => InventoryItemRemovedOutsidePDA(TechType.SnowBall, "Snowball") },
                { SplitName.PropulsionCannonDrownSplit, () => PlayerDied() && IsInsideBiomes(Biome.arctickelp_caveinner, Biome.arctickelp_caveouter) && KnowsTech(TechType.PropulsionCannon, TechType.PropulsionCannonBlueprint) },
                { SplitName.TwistyBridgesDeathSplit, () => PlayerDied() && IsInsideBiomes(Biome.twistyBridges, Biome.twistyBridges_Deep) },
                { SplitName.AcquireAlAnSplit, () => IsAnimationPlaying != null && IsAnimationPlaying.Changed && IsAnimationPlaying.New && !IsAnimationPlaying.Old && !string.IsNullOrEmpty(BiomeString.New) && BiomeString.New.StartsWith("Precursor_Sanctuary", StringComparison.OrdinalIgnoreCase) },
                { SplitName.BoosterTankDeathSplit, () => PlayerDied() && IsInsideBiome(Biome.purpleVents) && KnowsTech(TechType.SuitBoosterTank) },
                { SplitName.ArcticSpiresScanSplit, () => UnlockedTech(TechType.PrecursorNPCTissue) },
                { SplitName.ArcticSpiresDeathSplit, () => PlayerDied() && IsInsideBiome(Biome.ArcticSpiresCache) },
                { SplitName.ArcticSpiresTransitionSplit, () => TransitionedFromBiome(Biome.ArcticSpiresCache) },
                { SplitName.DeepLilypadsCacheScanSplit, () => UnlockedTech(TechType.PrecursorNPCSkeleton) },
                { SplitName.DeepLilypadCacheDeathSplit, () => PlayerDied() && IsInsideBiome(Biome.lilyPads_Deep_Cache) },
                { SplitName.DeepLilypadsCacheTransitionSplit, () => TransitionedFromBiome(Biome.lilyPads_Deep_Cache) },
                { SplitName.CrystalCavesCacheScanSplit, () => UnlockedTech(TechType.PrecursorNPCOrgans) },
                { SplitName.CrystalCavesCacheDeathSplit, () => PlayerDied() && IsInsideBiome(Biome.CrystalCave_Cache) },
                { SplitName.CrystalCavesCacheTransitionSplit, () => TransitionedFromBiome(Biome.CrystalCave_Cache) },
                { SplitName.CommenceStorageMediumFabricationSplit, () => CompletedStoryGoal(StoryGoal.OnFabricatePrecursorNPC) },
                { SplitName.AlAnTransferSplit, () => CompletedStoryGoal(StoryGoal.BeginPrecursorNPCTransfer) },
                { SplitName.AlAnTransferDeathSplit, () => PlayerDied() && IsInsideBiome(Biome.Precursor_Fabricator) },
                { SplitName.BraceSplit, IsBraceSplitTriggered },
                { SplitName.InsertHydraulicsFluidSplit, () => CompletedStoryGoal(StoryGoal.OnGlacialBasinBridgeItemInserted) },
            };
        }

        private bool IsInventorySubConditionMet()
        {
            ItemSplit split = (ItemSplit)CurrentSplitToCheck;
            TechType techType = split.Item.ConvertTo<TechType>();
            return !split.IsCount ? HasPlayerItem(techType) : GetPlayerItemCount(techType) == split.Count;
        }

        private bool IsBiomeSubConditionMet()
        {
            BiomeSplit split = (BiomeSplit)CurrentSplitToCheck;
            return split.Biomes.Biome1 == Biome.Any || IsInsideBiome(split.Biomes.Biome1);
        }

        private bool IsInventorySplitTriggered()
        {
            ItemSplit split = (ItemSplit)CurrentSplitToCheck;
            TechType techType = split.Item.ConvertTo<TechType>();
            int currentPickUpChange = curPickUpCounts.TryGetValue(techType, out InvChangeInfo pickUpInfo) ? pickUpInfo.Count : 0;
            int currentDropChange = curDropCounts.TryGetValue(techType, out InvChangeInfo dropInfo) ? dropInfo.Count : 0;

            if (!split.IsCount)
            {
                int currentChange = currentInventoryChanges.TryGetValue(techType, out int delta) ? delta : 0;
                return split.PickUp ? currentChange > 0 : currentChange < 0;
            }

            if (split.AlreadySplitInvChanging)
                return false;

            int change = split.PickUp ? currentPickUpChange : -currentDropChange;
            bool shouldSplit = change >= split.Count && change > 0;
            split.AlreadySplitInvChanging = shouldSplit;
            return shouldSplit;
        }

        private bool IsBiomeSplitTriggered()
        {
            BiomeSplit split = (BiomeSplit)CurrentSplitToCheck;
            return (split.Biomes.Biome1 == Biome.Any && split.Biomes.Biome2 == Biome.Any && BiomeString.Changed)
                || (split.Biomes.Biome1 == Biome.Any && IsInsideBiome(split.Biomes.Biome2) && BiomeString.Changed)
                || (split.Biomes.Biome2 == Biome.Any && WasInsideBiome(split.Biomes.Biome1) && BiomeString.Changed)
                || TransitionedBetweenBiomes(split.Biomes.Biome1, split.Biomes.Biome2);
        }

        private bool IsCraftSplitTriggered()
        {
            TechType techType = ((CraftSplit)CurrentSplitToCheck).Craftable.ConvertTo<TechType>();
            return TryConsumeStartedCraft(techType) || TryConsumeCompletedCraftFallback(techType);
        }

        public override bool Update()
        {
            if (!base.Update())
                return false;

            if (!pointersInitialized || game == null)
                return false;

            UpdateMainMenuState();
            UpdateMemoryWatchers();
            return true;
        }

        private void UpdateMainMenuState()
        {
            wasInMainMenu = isInMainMenu;
            isInMainMenu = IsInMainMenu();
            if (isInMainMenu)
            {
                if (!wasInMainMenu)
                {
                    logger.Log("Main menu entered.");
                    ResetRunState();
                }
            }
            else if (wasInMainMenu)
            {
                logger.Log("Left main menu.");
            }
        }

        private void UpdateMemoryWatchers()
        {
            HashSet<SplitName> usedSplitNames = GetUsedSplitNames();

            if (settings.StartEnabled || Needs(usedSplitNames, SplitName.Inventory, SplitName.FullInventorySplit, SplitName.ThrowFlareSplit, SplitName.ThrowSnowballSplit))
                UpdateInventory();

            if (Needs(usedSplitNames,
                SplitName.Blueprint,
                SplitName.PropulsionCannonDrownSplit,
                SplitName.BoosterTankDeathSplit,
                SplitName.ArcticSpiresScanSplit,
                SplitName.DeepLilypadsCacheScanSplit,
                SplitName.CrystalCavesCacheScanSplit))
                UpdateBlueprints();

            if (Needs(usedSplitNames,
                SplitName.DeathSplit,
                SplitName.PropulsionCannonDrownSplit,
                SplitName.TwistyBridgesDeathSplit,
                SplitName.BoosterTankDeathSplit,
                SplitName.ArcticSpiresDeathSplit,
                SplitName.DeepLilypadCacheDeathSplit,
                SplitName.CrystalCavesCacheDeathSplit,
                SplitName.AlAnTransferDeathSplit))
            {
                Health.ForceUpdate();
                DeathScreenActive.ForceUpdate();
            }

            if (Needs(usedSplitNames, SplitName.Encyclopedia))
                UpdateEncyclopedia();

            UpdateStoryGoals();

            if (Needs(usedSplitNames, SplitName.Craft))
                CaptureHoverpadConstructor();

            if (Needs(usedSplitNames, SplitName.Craft, SplitName.Build))
                UpdateCraftAndBuildState();

        }

        private void GetGameVersion()
        {
            System.Diagnostics.ProcessModule firstModule = game.Process.Modules
                .Cast<System.Diagnostics.ProcessModule>()
                .FirstOrDefault();
            if (firstModule == null)
                return;

            int moduleLength = firstModule.ModuleMemorySize;
            switch (moduleLength)
            {
                case 671744:
                    gameVersion = GameVersion.Aug2021;
                    logger.Log("Game version August 2021");
                    break;

                case 675840:
                    gameVersion = GameVersion.Oct2025;
                    logger.Log("Game version October 2025");
                    break;

                default:
                    gameVersion = GameVersion.Oct2025;
                    MessageBox.Show(
                        $"Module length {moduleLength} does not match a version, defaulting to most recent (October 2025)",
                        "Below Zero Autosplitter",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
            }

            settings.SetGameVersion(gameVersion);
        }

        private void InitPointers(IMonoHelper mono)
        {
            this.mono = mono;
            logger.Log("Initializing Below Zero pointers.");
            var ptrFactory = new MonoNestedPointerFactory(game, mono);
            var unity = (UnityHelperTask.UnityHelperBase)mono;

            InitializeVersionSpecificSignalPointers();

            if (IsAnimationPlaying == null)
            {
                IsAnimationPlaying = ptrFactory.Make<bool>("Player", "main", "_cinematicModeActive");
                logger.Log("Using Player._cinematicModeActive animation pointer.");
            }

            if (IsIntroCinematicActive == null)
            {
                IsIntroCinematicActive = IsAnimationPlaying;
                logger.Log("Using animation pointer as intro cinematic fallback.");
            }

            Pointer<IntPtr> sceneLoadingPtr = ptrFactory.Make<IntPtr>("uGUI", "_main", "loading");
            int off_isLoading = mono.GetFieldOffset(mono.FindClass("uGUI_SceneLoading"), "isLoading");
            IsLoadingScreenShowing = ptrFactory.Make<bool>(sceneLoadingPtr, off_isLoading);

            Pointer<IntPtr> liveMixinPtr = ptrFactory.Make<IntPtr>("Player", "main", "liveMixin");
            int off_health = mono.GetFieldOffset(mono.FindClass("LiveMixin"), "health");
            Health = ptrFactory.Make<float>(liveMixinPtr, off_health);

            invKlass = mono.FindClass("Inventory", mono.MainImage);
            equipmentKlass = mono.FindClass("Equipment", mono.MainImage);
            icKlass = mono.FindClass("ItemsContainer", mono.MainImage);

            invStaticKlass = invKlass;
            invStaticOffset = mono.GetFieldOffset(invKlass, "main");
            ResolveUnityObjectCachedPtrOffset(unity);

            off_container = unity.ResolveFieldOffsetByNameOrPredicate(invKlass, new[] { "_container" }, name => UnityHelperTask.UnityNameUtil.NameHas(name, "container"));
            off_equipment = unity.ResolveFieldOffsetByNameOrPredicate(invKlass, new[] { "_equipment" }, name => UnityHelperTask.UnityNameUtil.NameHas(name, "equipment"));
            off_equippedCount = unity.ResolveFieldOffsetByNameOrPredicate(equipmentKlass, new[] { "equippedCount" }, name => UnityHelperTask.UnityNameUtil.NameHas(name, "equippedCount"));
            off_itemsDict = unity.ResolveFieldOffsetByNameOrPredicate(icKlass, new[] { "_items" }, name => UnityHelperTask.UnityNameUtil.NameHas(name, "items"));

            IntPtr itemGroupKlass = mono.FindClass("ItemGroup", mono.MainImage);
            off_itemGroup_items = unity.ResolveFieldOffsetByNameOrPredicate(itemGroupKlass, new[] { "items" }, name => UnityHelperTask.UnityNameUtil.NameHas(name, "items"));
            off_itemGroup_id = unity.ResolveFieldOffsetByNameOrPredicate(itemGroupKlass, new[] { "id", "techType" }, name =>
            {
                string fieldName = name.ToLowerInvariant();
                return fieldName == "id" || fieldName.Contains("techtype");
            });
            off_itemGroup_width = unity.ResolveFieldOffsetByNameOrPredicate(itemGroupKlass, new[] { "width" }, name => string.Equals(name, "width", StringComparison.OrdinalIgnoreCase));
            off_itemGroup_height = unity.ResolveFieldOffsetByNameOrPredicate(itemGroupKlass, new[] { "height" }, name => string.Equals(name, "height", StringComparison.OrdinalIgnoreCase));

            IntPtr core = unity.TryFindImageOnce(
                "mscorlib",
                "mscorlib.dll",
                "System.Private.CoreLib",
                "System.Private.CoreLib.dll",
                "netstandard",
                "netstandard.dll");

            IntPtr listKlass = core != IntPtr.Zero ? unity.TryFindClassOnce("List`1", core) : IntPtr.Zero;
            off_list_size = 0x18;
            off_list_items = 0x10;
            if (listKlass != IntPtr.Zero)
            {
                int candSize = mono.GetFieldOffset(listKlass, "_size");
                if (candSize != 0)
                    off_list_size = candSize;

                int candItems = mono.GetFieldOffset(listKlass, "_items");
                if (candItems != 0)
                    off_list_items = candItems;
            }

            dict_off_entries = 0x18;
            dict_off_version = 0x10;
            arr_off_len = 0x18;
            arr_data_base = 0x20;

            if (core != IntPtr.Zero)
            {
                IntPtr dictKlass = unity.TryFindClassOnce("Dictionary`2", core);
                if (dictKlass != IntPtr.Zero)
                {
                    int entriesOffset = mono.GetFieldOffset(dictKlass, "entries");
                    if (entriesOffset == 0)
                        entriesOffset = mono.GetFieldOffset(dictKlass, "_entries");
                    if (entriesOffset != 0)
                        dict_off_entries = entriesOffset;

                    int versionOffset = mono.GetFieldOffset(dictKlass, "version");
                    if (versionOffset == 0)
                        versionOffset = mono.GetFieldOffset(dictKlass, "_version");
                    if (versionOffset != 0)
                        dict_off_version = versionOffset;
                }

                IntPtr hashSetKlass = unity.TryFindClassOnce("System.Collections.Generic.HashSet`1", core);
                hashset_off_slots = 0x18;
                if (hashSetKlass != IntPtr.Zero)
                {
                    int slotsOffset = unity.ResolveFieldOffsetByNameOrPredicate(
                        hashSetKlass,
                        new[] { "_slots", "m_slots", "slots" },
                        name => UnityHelperTask.UnityNameUtil.NameHas(name, "slots"));
                    if (slotsOffset != 0)
                        hashset_off_slots = slotsOffset;
                }

            }

            knownTechPtr = ptrFactory.Make<IntPtr>("KnownTech", "knownTech");
            encyclopediaPtr = ptrFactory.Make<IntPtr>("Player", "main", "encyclopedia");

            completedStoryGoalsPtr = ptrFactory.Make<IntPtr>(
                "Story.StoryGoalManager",
                "<main>k__BackingField",
                "completedGoals");

            IntPtr craftingAnalyticsKlass = mono.FindClass("CraftingAnalytics", mono.MainImage);
            int off_craftingAnalyticsEntries = mono.GetFieldOffset(craftingAnalyticsKlass, "entries");
            Pointer<IntPtr> craftingAnalyticsMainPtr = ptrFactory.Make<IntPtr>("CraftingAnalytics", "_main");
            craftingAnalyticsEntriesPtr = ptrFactory.Make<IntPtr>(craftingAnalyticsMainPtr, off_craftingAnalyticsEntries);
            MainMenu = ptrFactory.Make<IntPtr>("uGUI_MainMenu", "main");
            PlayerMain = ptrFactory.Make<IntPtr>("Player", "main");
            IntPtr playerKlass = mono.FindClass("Player", mono.MainImage);
            int off_lastPosition = unity.ResolveFieldOffsetByNameOrPredicate(
                playerKlass,
                new[] { "lastPosition", "_lastPosition", "m_lastPosition" },
                name => UnityHelperTask.UnityNameUtil.NameHas(name, "lastPosition"));
            if (off_lastPosition != 0)
            {
                PlayerLastPositionX = ptrFactory.Make<float>(PlayerMain, off_lastPosition + 0x0);
                PlayerLastPositionY = ptrFactory.Make<float>(PlayerMain, off_lastPosition + 0x4);
                PlayerLastPositionZ = ptrFactory.Make<float>(PlayerMain, off_lastPosition + 0x8);
            }
            else
            {
                logger.Log("Player.lastPosition offset not found.");
            }

            BiomeString = ptrFactory.MakeString("Player", "main", "biomeString", 0x14);
            int off_playerPda = mono.GetFieldOffset(playerKlass, "pda");
            Pointer<IntPtr> playerPdaPtr = ptrFactory.Make<IntPtr>(PlayerMain, off_playerPda);
            int off_pdaIsInUse = mono.GetFieldOffset(mono.FindClass("PDA"), "<isInUse>k__BackingField");
            PDAIsInUse = ptrFactory.Make<bool>(playerPdaPtr, off_pdaIsInUse);
            PDATab = ptrFactory.Make<int>("uGUI_PDA", "<main>k__BackingField", "tabOpen");

            Pointer<IntPtr> craftingMenuRootPtr = ptrFactory.Make<IntPtr>("uGUI", "_main", "craftingMenu");
            int off_craftedNode = mono.GetFieldOffset(mono.FindClass("uGUI_CraftingMenu"), "craftedNode");
            Pointer<IntPtr> craftedNodePtr = ptrFactory.Make<IntPtr>(craftingMenuRootPtr, off_craftedNode);

            int off_nodeTechType = 0x34;
            logger.Log("Crafted node techType offset fixed at 0x34.");
            CraftedNode = ptrFactory.Make<int>(craftedNodePtr, off_nodeTechType);

            int off_client = mono.GetFieldOffset(mono.FindClass("uGUI_CraftingMenu"), "_client");
            CraftingMenu = ptrFactory.Make<IntPtr>(craftingMenuRootPtr, off_client);
            MoveDirectionX = ptrFactory.Make<float>("GameInput", "moveDirection", 0x0);
            MoveDirectionZ = ptrFactory.Make<float>("GameInput", "moveDirection", 0x8);

            Pointer<IntPtr> groundMotorPtr = ptrFactory.Make<IntPtr>("Player", "main", "groundMotor");
            int off_jumpState = mono.GetFieldOffset(mono.FindClass("GroundMotor"), "jumping");
            Pointer<IntPtr> jumpingStatePtr = ptrFactory.Make<IntPtr>(groundMotorPtr, off_jumpState);
            logger.Log("Jump state flag offset fixed at 0x24.");
            IsPlayerJumping = ptrFactory.Make<bool>(jumpingStatePtr, 0x24);

            DeathScreenActive = ptrFactory.Make<bool>("uGUI_PlayerDeath", "main", "active");

            Pointer<IntPtr> quickSlotsPtr = ptrFactory.Make<IntPtr>("Inventory", "main", "<quickSlots>k__BackingField");
            int off_activeToolName = mono.GetFieldOffset(mono.FindClass("QuickSlots"), "activeToolName");
            ActiveToolName = ptrFactory.MakeString(quickSlotsPtr, off_activeToolName, 0x14);

            Pointer<IntPtr> guiHandPtr = ptrFactory.Make<IntPtr>("Player", "main", "guiHand");
            int off_activeTarget = mono.GetFieldOffset(mono.FindClass("GUIHand"), "activeTarget");
            GuiHandActiveTarget = ptrFactory.Make<IntPtr>(guiHandPtr, off_activeTarget);

            Pointer<IntPtr> playerControllerPtr = ptrFactory.Make<IntPtr>("Player", "main", "<playerController>k__BackingField");
            int off_velocity = mono.GetFieldOffset(mono.FindClass("PlayerController"), "velocity");
            PlayerControllerVelocityX = ptrFactory.Make<float>(playerControllerPtr, off_velocity + 0x0);
            PlayerControllerVelocityZ = ptrFactory.Make<float>(playerControllerPtr, off_velocity + 0x8);
            int off_inputEnabled = mono.GetFieldOffset(mono.FindClass("PlayerController"), "inputEnabled");
            PlayerControllerInputEnabled = ptrFactory.Make<bool>(playerControllerPtr, off_inputEnabled);

            IntPtr builderMenuKlass = mono.FindClass("uGUI_BuilderMenu", mono.MainImage);
            int off_builderMenuState = mono.GetFieldOffset(builderMenuKlass, "<state>k__BackingField");
            Pointer<IntPtr> builderMenuSingletonPtr = ptrFactory.Make<IntPtr>("uGUI_BuilderMenu", "singleton");
            BuilderMenuState = ptrFactory.Make<bool>(builderMenuSingletonPtr, off_builderMenuState);
            BuilderLastTechType = ptrFactory.Make<int>("Builder", "<lastTechType>k__BackingField");
            BuilderPrefab = ptrFactory.Make<IntPtr>("Builder", "prefab");

            int off_playerUnderwater = mono.GetFieldOffset(playerKlass, "playerUnderwater");
            int off_playerInside = mono.GetFieldOffset(playerKlass, "playerInside");
            Pointer<IntPtr> playerUnderwaterPtr = ptrFactory.Make<IntPtr>(PlayerMain, off_playerUnderwater);
            Pointer<IntPtr> playerInsidePtr = ptrFactory.Make<IntPtr>(PlayerMain, off_playerInside);

            IntPtr boolVariableKlass = mono.FindClass("BoolVariable", mono.MainImage);
            if (boolVariableKlass == IntPtr.Zero)
                boolVariableKlass = mono.FindClass("PlayerEmotes.BoolVariable", mono.MainImage);

            if (boolVariableKlass != IntPtr.Zero)
            {
                int off_boolVariableValue = mono.GetFieldOffset(boolVariableKlass, "value");
                PlayerIsUnderwater = ptrFactory.Make<bool>(playerUnderwaterPtr, off_boolVariableValue);
                PlayerIsInside = ptrFactory.Make<bool>(playerInsidePtr, off_boolVariableValue);
            }

            IntPtr hoverpadKlass = mono.FindClass("Hoverpad", mono.MainImage);
            off_hoverpadConstructorField = mono.GetFieldOffset(hoverpadKlass, "constructor");

            IntPtr hoverpadTerminalKlass = mono.FindClass("GUI_HoverpadTerminal", mono.MainImage);
            off_hoverpadTerminalHoverpad = mono.GetFieldOffset(hoverpadTerminalKlass, "hoverpad");

            IntPtr hoverpadConstructorKlass = mono.FindClass("HoverpadConstructor", mono.MainImage);
            off_hoverpadConstructorConstructing = mono.GetFieldOffset(hoverpadConstructorKlass, "_constructing");

            pointersInitialized = true;
            logger.Log("Pointers initialized");
        }

        private HashSet<SplitName> GetUsedSplitNames()
        {
            var usedSplitNames = new HashSet<SplitName>();
            if (settings?.Splits == null || settings.Splits.Count == 0)
                return usedSplitNames;

            foreach (var split in settings.Splits)
            {
                usedSplitNames.Add(split.SplitName);
                foreach (var conditionSplit in BelowZeroComponent.GetAllConditions(split))
                    usedSplitNames.Add(conditionSplit.SplitName);
            }

            return usedSplitNames;
        }

        private static bool Needs(HashSet<SplitName> usedSplitNames, params SplitName[] required) =>
            required.Any(usedSplitNames.Contains);

        private bool IsInMainMenu()
        {
            if (IsMainMenuPosition(DirectPositionX?.New ?? float.NaN, DirectPositionY?.New ?? float.NaN, DirectPositionZ?.New ?? float.NaN))
                return true;

            if (IsLiveManagedUnityObject(MainMenu.New))
                return true;

            return IsMainMenuPosition(PlayerLastPositionX?.New ?? float.NaN, PlayerLastPositionY?.New ?? float.NaN, PlayerLastPositionZ?.New ?? float.NaN);
        }

        private void UpdateBlueprints()
        {
            if (!TryReadKnownTech(out List<TechType> currentKnownTech))
                return;

            if (!blueprintsInitialized)
            {
                KnownTech = currentKnownTech;
                KnownTechOld = new List<TechType>(currentKnownTech);
                blueprintsInitialized = true;
                return;
            }

            KnownTechOld = KnownTech;
            KnownTech = currentKnownTech;
        }

        private bool TryReadKnownTech(out List<TechType> result)
        {
            result = new List<TechType>();
            IntPtr hashSetPtr = knownTechPtr?.New ?? IntPtr.Zero;
            if (hashSetPtr == IntPtr.Zero || hashset_off_slots == 0)
                return false;

            IntPtr slotsArr = game.Read<IntPtr>(hashSetPtr + hashset_off_slots);
            if (slotsArr == IntPtr.Zero)
                return false;

            int length = game.Read<int>(slotsArr + arr_off_len);
            if (length <= 0 || length > 100000)
                return false;

            const int slotStride = 0x0C;
            const int slotValueOffset = 0x08;
            IntPtr basePtr = slotsArr + arr_data_base;
            for (int i = 0; i < length; i++)
            {
                IntPtr slot = basePtr + i * slotStride;
                int hashCode = game.Read<int>(slot);
                if (hashCode < 0)
                    continue;

                int tech = game.Read<int>(slot + slotValueOffset);
                if (tech > 0 && tech <= (int)TechType.BaseMoonpoolExpansion)
                    result.Add((TechType)tech);
            }

            return result.Count > 0;
        }

        private void UpdateInventory()
        {
            InventorySnapshot inventorySnapshot = ReadInventorySnapshot();
            List<TechType> equipmentTypes = ReadEquipmentTypes();

            if (!inventoryInitialized)
            {
                PlayerInventory = new Dictionary<TechType, int>(inventorySnapshot.Counts);
                PlayerInventoryOld = new Dictionary<TechType, int>(inventorySnapshot.Counts);
                playerInventorySlotsUsed = inventorySnapshot.SlotsUsed;
                playerInventorySlotsUsedOld = inventorySnapshot.SlotsUsed;
                PlayerEquipment = new List<TechType>(equipmentTypes);
                PlayerEquipmentOld = new List<TechType>(equipmentTypes);
                currentInventoryChanges = new Dictionary<TechType, int>();
                inventoryInitialized = true;
                return;
            }

            PlayerInventoryOld = PlayerInventory;
            playerInventorySlotsUsedOld = playerInventorySlotsUsed;
            PlayerInventory = inventorySnapshot.Counts;
            playerInventorySlotsUsed = inventorySnapshot.SlotsUsed;
            PlayerEquipmentOld = PlayerEquipment;
            PlayerEquipment = equipmentTypes;

            Dictionary<TechType, int> changedItems = PlayerInventory.Keys
                .Union(PlayerInventoryOld.Keys)
                .Union(PlayerEquipment)
                .Union(PlayerEquipmentOld)
                .Select(key => new
                {
                    Key = key,
                    Delta = GetPlayerItemCount(key) - GetPlayerItemCountOld(key)
                })
                .Where(x => x.Delta != 0)
                .ToDictionary(x => x.Key, x => x.Delta);

            currentInventoryChanges = changedItems;

            foreach (var key in curPickUpCounts.Keys.ToList())
            {
                if (curPickUpCounts[key].ElapsedTime.ElapsedMilliseconds <= maxInventoryTimeWithoutChangingMs)
                    continue;

                var split = settings.Splits.OfType<ItemSplit>().FirstOrDefault(s => s.Item.ConvertTo<TechType>() == key);
                if (split != null)
                    split.AlreadySplitInvChanging = false;

                curPickUpCounts.Remove(key);
            }

            foreach (var key in curDropCounts.Keys.ToList())
            {
                if (curDropCounts[key].ElapsedTime.ElapsedMilliseconds <= maxInventoryTimeWithoutChangingMs)
                    continue;

                var split = settings.Splits.OfType<ItemSplit>().FirstOrDefault(s => s.Item.ConvertTo<TechType>() == key);
                if (split != null)
                    split.AlreadySplitInvChanging = false;

                curDropCounts.Remove(key);
            }

            foreach (var changedItem in changedItems)
            {
                if (changedItem.Value > 0)
                    HandleChange(curPickUpCounts, changedItem.Key, changedItem.Value);
                else
                    HandleChange(curDropCounts, changedItem.Key, changedItem.Value);
            }

            void HandleChange(Dictionary<TechType, InvChangeInfo> dict, TechType key, int amount)
            {
                if (dict.TryGetValue(key, out var info))
                {
                    info.Count += amount;
                    info.ElapsedTime.Restart();
                }
                else
                {
                    dict[key] = new InvChangeInfo(amount, Stopwatch.StartNew());
                }
            }
        }

        public bool MovementStarted(float velocityThreshold = 0.35f, float inputThreshold = 0.001f)
        {
            if (BecameHorizontallyActive(PlayerControllerVelocityX, PlayerControllerVelocityZ, velocityThreshold))
                return true;

            if (BecameActive(MoveDirectionX, inputThreshold) || BecameActive(MoveDirectionZ, inputThreshold))
                return true;

            return false;
        }

        public bool IsMovingHorizontally(float threshold = 0.35f)
        {
            return IsHorizontallyActive(PlayerControllerVelocityX, PlayerControllerVelocityZ, threshold);
        }

        public bool BuilderMenuOpened()
        {
            if (BuilderMenuState == null || !BuilderMenuState.Changed || !BuilderMenuState.New)
                return false;

            return string.Equals(ActiveToolName.New, TechType.Builder.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(ActiveToolName.Old, TechType.Builder.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public bool PickedUpSnowball()
        {
            if (!currentInventoryChanges.TryGetValue(TechType.SnowBall, out int delta) || delta != 1)
                return false;

            if (currentInventoryChanges.Count != 1)
                return false;

            if ((PlayerIsUnderwater?.New ?? false) || (PlayerIsInside?.New ?? false))
                return false;

            if (GuiHandActiveTarget != null && GuiHandActiveTarget.New != IntPtr.Zero)
                return false;

            return true;
        }

        private bool HasPlayerItem(TechType techType) => PlayerInventory.ContainsKey(techType) || PlayerEquipment.Contains(techType);

        private int GetPlayerItemCount(TechType techType) => PlayerInventory.GetCount(techType) + PlayerEquipment.Count(item => item == techType);

        private int GetPlayerItemCountOld(TechType techType) => PlayerInventoryOld.GetCount(techType) + PlayerEquipmentOld.Count(item => item == techType);

        private bool IsPDAInventoryOpen()
        {
            if (!(PDAIsInUse?.New ?? false))
                return false;

            return (global::LiveSplit.BelowZero.PDATab)PDATab.New == global::LiveSplit.BelowZero.PDATab.Inventory;
        }

        private void UpdateStoryGoals()
        {
            newlyCompletedStoryGoals.Clear();
            if (isInMainMenu)
                return;

            if (!TryReadCompletedStoryGoals(out HashSet<string> currentGoals, out string failureReason))
            {
                LogStoryGoalReaderStatus($"Story goal reader waiting: {failureReason}");
                return;
            }

            if (!storyGoalsInitialized)
            {
                completedStoryGoals = currentGoals;
                storyGoalsInitialized = true;
                LogStoryGoalReaderStatus($"Story goal tracking initialized with {currentGoals.Count} completed goals.");
                return;
            }

            LogStoryGoalReaderStatus("Story goal reader active.");

            foreach (string goal in currentGoals)
            {
                if (completedStoryGoals.Contains(goal))
                    continue;

                newlyCompletedStoryGoals.Add(goal);
                logger.Log($"Story goal completed: {goal}");
            }

            completedStoryGoals = currentGoals;
        }

        private void LogStoryGoalReaderStatus(string status)
        {
            if (string.Equals(storyGoalReaderStatus, status, StringComparison.Ordinal))
                return;

            storyGoalReaderStatus = status;
            logger.Log(status);
        }

        private bool CompletedStoryGoal(StoryGoal goal)
        {
            return goal != StoryGoal.None
                && newlyCompletedStoryGoals.Contains(goal.ToString());
        }

        private bool HasNotCompletedStoryGoal(StoryGoal goal)
        {
            return storyGoalsInitialized
                && goal != StoryGoal.None
                && !completedStoryGoals.Contains(goal.ToString());
        }

        private bool IsBraceSplitTriggered()
        {
            if (CompletedStoryGoal(StoryGoal.EndGameEnterShip))
            {
                braceInteractionWindow.Restart();
                braceReadyForCinematic = false;
                logger.Log("Brace watch armed from EndGameEnterShip.");
                return false;
            }

            if (!braceInteractionWindow.IsRunning)
                return false;

            if (braceInteractionWindow.ElapsedMilliseconds > maxBraceInteractionWindowMs)
            {
                braceInteractionWindow.Reset();
                braceReadyForCinematic = false;
                logger.Log("Brace watch expired before cinematic started.");
                return false;
            }

            if (!braceReadyForCinematic)
            {
                if (IsBraceReadyForNextCinematic())
                {
                    braceReadyForCinematic = true;
                    logger.Log("Brace watch ready; waiting for next cinematic.");
                }

                return false;
            }

            if (!BraceCinematicStarted())
                return false;

            braceInteractionWindow.Reset();
            braceReadyForCinematic = false;
            logger.Log("Brace split: endgame cinematic started after EndGameEnterShip.");
            return true;
        }

        private bool PlayerDied()
        {
            return (Health.New <= 0 && Health.Old > 0)
                || (DeathScreenActive.New && !DeathScreenActive.Old);
        }

        private bool IsInsideBiome(Biome biome)
        {
            return BiomeMatches(BiomeString.New, biome);
        }

        private bool IsInsideBiomes(params Biome[] biomes)
        {
            return biomes != null && biomes.Any(IsInsideBiome);
        }

        private bool WasInsideBiome(Biome biome)
        {
            return BiomeMatches(BiomeString.Old, biome);
        }

        private bool TransitionedFromBiome(Biome sourceBiome)
        {
            return BiomeString.Changed
                && WasInsideBiome(sourceBiome)
                && !IsInsideBiome(sourceBiome)
                && !isInMainMenu
                && !string.IsNullOrWhiteSpace(BiomeString.New)
                && !string.Equals(BiomeString.New, "None", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(BiomeString.New, "MainMenu", StringComparison.OrdinalIgnoreCase);
        }

        private bool TransitionedBetweenBiomes(Biome sourceBiome, Biome destinationBiome)
        {
            return BiomeString.Changed
                && WasInsideBiome(sourceBiome)
                && IsInsideBiome(destinationBiome);
        }

        private static bool BiomeMatches(string currentBiome, Biome expectedBiome)
        {
            return expectedBiome != Biome.None
                && expectedBiome != Biome.Any
                && string.Equals(currentBiome, expectedBiome.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private bool KnowsTech(params TechType[] techTypes)
        {
            return techTypes != null
                && techTypes.Any(techType => techType != TechType.None && KnownTech.Contains(techType));
        }

        private bool UnlockedTech(params TechType[] techTypes)
        {
            return techTypes != null
                && techTypes.Any(techType => techType != TechType.None
                    && KnownTech.Contains(techType)
                    && !KnownTechOld.Contains(techType));
        }

        private bool InventoryItemRemovedOutsidePDA(TechType techType, string displayName)
        {
            bool itemDropped = currentInventoryChanges.TryGetValue(techType, out int delta) && delta < 0;
            if (!itemDropped)
                return false;

            if (IsPDAInventoryOpen())
            {
                logger.Log($"Ignored {displayName} inventory removal while PDA inventory was open.");
                return false;
            }

            logger.Log($"{displayName} removed from inventory outside PDA inventory.");
            if (lastThrowSplitTool == techType && throwSplitCooldown.ElapsedMilliseconds <= minThrowSplitCooldownMs)
                return false;

            lastThrowSplitTool = techType;
            throwSplitCooldown.Restart();
            return true;
        }

        private void UpdateEncyclopedia()
        {
            List<EncyclopediaEntry> currentEncyclopedia = ReadPDAEncyMapping();
            if (!encyclopediaInitialized)
            {
                Encyclopedia = currentEncyclopedia;
                EncyclopediaOld = new List<EncyclopediaEntry>(currentEncyclopedia);
                encyclopediaInitialized = true;
                return;
            }

            EncyclopediaOld = Encyclopedia;
            Encyclopedia = currentEncyclopedia;
        }

        private void UpdateCraftAndBuildState()
        {
            startedCraft = TechType.None;
            fallbackCompletedCraft = TechType.None;
            completedBuild = TechType.None;

            if (CraftedNode != null && CraftedNode.Changed && CraftedNode.New != 0)
                RecordStartedCraft((TechType)CraftedNode.New, "Craft");

            UpdateHoverpadCraftState();
            UpdateCraftAnalyticsState();
            UpdatePendingBuilderCompletion();
        }

        private void UpdateHoverpadCraftState()
        {
            IntPtr hoverpadConstructorPtr = trackedHoverpadConstructorPtr;
            if (hoverpadConstructorPtr == IntPtr.Zero || off_hoverpadConstructorConstructing == 0)
                return;

            if (!IsLiveManagedUnityObject(hoverpadConstructorPtr))
            {
                trackedHoverpadConstructorPtr = IntPtr.Zero;
                return;
            }

            bool isConstructing = game.Read<bool>(hoverpadConstructorPtr + off_hoverpadConstructorConstructing);
            if (!hoverpadConstructingInitialized)
            {
                hoverpadConstructingOld = isConstructing;
                hoverpadConstructingInitialized = true;
                return;
            }

            if (!hoverpadConstructingOld && isConstructing)
                RecordStartedCraft(TechType.Hoverbike, "Craft");

            hoverpadConstructingOld = isConstructing;
        }

        private void UpdatePendingBuilderCompletion()
        {
            if (BuilderMenuState != null && BuilderMenuState.Changed && BuilderMenuState.Old && !BuilderMenuState.New)
            {
                builderMenuSelectionPending = true;
                builderMenuSelectionWindow.Restart();
            }

            if (builderMenuSelectionPending && builderMenuSelectionWindow.ElapsedMilliseconds > maxBuilderMenuSelectionWindowMs)
            {
                builderMenuSelectionPending = false;
                builderMenuSelectionWindow.Reset();
            }

            if (builderMenuSelectionPending
                && BuilderPrefab != null
                && BuilderPrefab.Changed
                && BuilderPrefab.Old == IntPtr.Zero
                && BuilderPrefab.New != IntPtr.Zero
                && BuilderLastTechType != null
                && (TechType)BuilderLastTechType.New != TechType.None)
            {
                pendingBuilderCompletionTechType = (TechType)BuilderLastTechType.New;
                logger.Log($"Build armed: {pendingBuilderCompletionTechType}");
                builderMenuSelectionPending = false;
                builderMenuSelectionWindow.Reset();
                return;
            }

            if (pendingBuilderCompletionTechType == TechType.None
                || BuilderPrefab == null
                || BuilderLastTechType == null)
                return;

            bool builderMenuClosed = BuilderMenuState != null
                && BuilderMenuState.Changed
                && BuilderMenuState.Old
                && !BuilderMenuState.New;

            if (BuilderPrefab.Changed
                && BuilderPrefab.Old == IntPtr.Zero
                && BuilderPrefab.New != IntPtr.Zero
                && !builderMenuClosed
                && (TechType)BuilderLastTechType.New == pendingBuilderCompletionTechType)
            {
                completedBuild = pendingBuilderCompletionTechType;
                logger.Log($"Build completed: {completedBuild}");
                pendingBuilderCompletionTechType = TechType.None;
            }
        }

        private void RecordStartedCraft(TechType techType, string source)
        {
            if (techType == TechType.None)
                return;

            startedCraft = techType;
            suppressCraftCompletionFallback.Add(techType);
            logger.Log($"{source} started: {startedCraft}");
        }

        private bool TryConsumeStartedCraft(TechType techType)
        {
            if (startedCraft != techType)
                return false;

            startedCraft = TechType.None;
            return true;
        }

        private bool TryConsumeCompletedCraftFallback(TechType techType)
        {
            if (fallbackCompletedCraft != techType)
                return false;

            fallbackCompletedCraft = TechType.None;
            return true;
        }

        private bool TryConsumeCompletedBuild(TechType techType)
        {
            if (completedBuild != techType)
                return false;

            completedBuild = TechType.None;
            return true;
        }

        private void CaptureHoverpadConstructor()
        {
            IntPtr activeTarget = GuiHandActiveTarget?.New ?? IntPtr.Zero;
            if (activeTarget != IntPtr.Zero
                && TryGetManagedObjectTypeName(activeTarget, out string typeName)
                && !string.IsNullOrEmpty(typeName))
            {
                switch (typeName)
                {
                    case "HoverpadConstructor":
                        trackedHoverpadConstructorPtr = activeTarget;
                        break;

                    case "GUI_HoverpadTerminal":
                        trackedHoverpadConstructorPtr = ResolveHoverpadConstructorFromHoverpad(
                            ReadObjectFieldPointer(activeTarget, off_hoverpadTerminalHoverpad));
                        break;

                    case "Hoverpad":
                        trackedHoverpadConstructorPtr = ResolveHoverpadConstructorFromHoverpad(activeTarget);
                        break;
                }
            }
        }

        private IntPtr ResolveHoverpadConstructorFromHoverpad(IntPtr hoverpadPtr)
        {
            if (hoverpadPtr == IntPtr.Zero)
                return IntPtr.Zero;

            IntPtr constructorPtr = ReadObjectFieldPointer(hoverpadPtr, off_hoverpadConstructorField);
            return IsLiveManagedUnityObject(constructorPtr) ? constructorPtr : IntPtr.Zero;
        }

        private IntPtr ReadObjectFieldPointer(IntPtr objectPtr, int offset)
        {
            if (objectPtr == IntPtr.Zero || offset == 0)
                return IntPtr.Zero;

            return game.Read<IntPtr>(objectPtr + offset);
        }

        private bool BraceCinematicStarted()
        {
            if (!braceInteractionWindow.IsRunning)
                return false;

            if (isInMainMenu || wasInMainMenu)
                return false;

            if (IsLoadingScreenShowing != null && IsLoadingScreenShowing.Changed)
            {
                if (IsLoadingScreenShowing.New && !IsLoadingScreenShowing.Old)
                    return true;
            }

            if (IsAnimationPlaying != null && IsAnimationPlaying.Changed)
            {
                if (IsAnimationPlaying.New && !IsAnimationPlaying.Old)
                    return true;
            }

            if (PlayerControllerInputEnabled != null && PlayerControllerInputEnabled.Changed)
            {
                if (!PlayerControllerInputEnabled.New && PlayerControllerInputEnabled.Old)
                    return true;
            }

            return false;
        }

        private bool IsBraceReadyForNextCinematic()
        {
            if (!braceInteractionWindow.IsRunning || isInMainMenu || wasInMainMenu)
                return false;

            if (IsLoadingScreenShowing?.New ?? false)
                return false;

            if (IsAnimationPlaying?.New ?? false)
                return false;

            if (PlayerControllerInputEnabled != null && !PlayerControllerInputEnabled.New)
                return false;

            return true;
        }

        private bool TryGetManagedObjectTypeName(IntPtr objectPtr, out string typeName)
        {
            typeName = null;
            if (objectPtr == IntPtr.Zero)
                return false;

            IntPtr vtablePtr = game.Read<IntPtr>(objectPtr);
            if (vtablePtr == IntPtr.Zero)
                return false;

            IntPtr klassPtr = game.Read<IntPtr>(vtablePtr);
            if (klassPtr == IntPtr.Zero)
                return false;

            string className = mono.GetClassName(klassPtr);
            if (string.IsNullOrEmpty(className))
                return false;

            typeName = className;
            return true;
        }

        private void UpdateCraftAnalyticsState()
        {
            Dictionary<TechType, int> currentCounts = ReadCraftCountTotals();
            if (!craftAnalyticsInitialized)
            {
                craftCounts.Clear();
                foreach (KeyValuePair<TechType, int> pair in currentCounts)
                    craftCounts[pair.Key] = pair.Value;

                craftAnalyticsInitialized = true;
                return;
            }

            foreach (KeyValuePair<TechType, int> pair in currentCounts)
            {
                int oldCount = craftCounts.TryGetValue(pair.Key, out int previousCount) ? previousCount : 0;
                if (pair.Value <= oldCount)
                    continue;

                if (suppressCraftCompletionFallback.Remove(pair.Key))
                {
                    logger.Log($"Craft completion observed after start: {pair.Key}");
                    continue;
                }

                if (fallbackCompletedCraft == TechType.None)
                {
                    fallbackCompletedCraft = pair.Key;
                    logger.Log($"Craft completion fallback observed: {pair.Key}");
                }
            }

            craftCounts.Clear();
            foreach (KeyValuePair<TechType, int> pair in currentCounts)
                craftCounts[pair.Key] = pair.Value;
        }

        public void ResetRunState(bool armHeldMovementStartAfterReset = false)
        {
            startedTimerBefore = false;
            MovementStartArmed = armHeldMovementStartAfterReset;
            inventoryInitialized = false;
            blueprintsInitialized = false;
            encyclopediaInitialized = false;
            currentInventoryChanges = new Dictionary<TechType, int>();
            curPickUpCounts.Clear();
            curDropCounts.Clear();
            playerInventorySlotsUsed = 0;
            playerInventorySlotsUsedOld = 0;
            PlayerInventory.Clear();
            PlayerInventoryOld.Clear();
            PlayerEquipment.Clear();
            PlayerEquipmentOld.Clear();
            KnownTech.Clear();
            KnownTechOld.Clear();
            Encyclopedia.Clear();
            EncyclopediaOld.Clear();
            throwSplitCooldown.Reset();
            lastThrowSplitTool = TechType.None;
            startedCraft = TechType.None;
            fallbackCompletedCraft = TechType.None;
            completedBuild = TechType.None;
            craftAnalyticsInitialized = false;
            craftCounts.Clear();
            suppressCraftCompletionFallback.Clear();
            storyGoalsInitialized = false;
            completedStoryGoals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            newlyCompletedStoryGoals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            pendingBuilderCompletionTechType = TechType.None;
            builderMenuSelectionPending = false;
            builderMenuSelectionWindow.Reset();
            braceInteractionWindow.Reset();
            braceReadyForCinematic = false;
            trackedHoverpadConstructorPtr = IntPtr.Zero;
            hoverpadConstructingInitialized = false;
            hoverpadConstructingOld = false;
        }

        private void InitializeVersionSpecificSignalPointers()
        {
            try
            {
                Pointer<bool> animationPointer = MakeModulePointer<bool>("mono-2.0-bdwgc.dll", 0x499C78, 0x40, 0xF0, 0x28, 0x260, 0xA8);
                if (animationPointer != null)
                {
                    IsAnimationPlaying = animationPointer;
                    logger.Log("Using direct Below Zero animation pointer.");
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Direct animation pointer unavailable: {ex.Message}");
            }

            try
            {
                Pointer<bool> introPointer = MakeModulePointer<bool>("fmodstudio.dll", 0x2CED70, 0x28, 0x18, 0x190, 0x448, 0x20, 0xB0, 0x28)
                    ?? MakeModulePointer<bool>("fmodstudiol.dll", 0x2CED70, 0x28, 0x18, 0x190, 0x448, 0x20, 0xB0, 0x28);
                if (introPointer != null)
                {
                    IsIntroCinematicActive = introPointer;
                    logger.Log("Using direct Below Zero intro cinematic pointer.");
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Direct intro cinematic pointer unavailable: {ex.Message}");
            }

            try
            {
                switch (gameVersion)
                {
                    case GameVersion.Aug2021:
                        DirectPositionX = MakeModulePointer<float>("UnityPlayer.dll", 0x17B84D8, 0x150, 0xBF8);
                        DirectPositionY = MakeModulePointer<float>("UnityPlayer.dll", 0x17B84D8, 0x150, 0xBFC);
                        DirectPositionZ = MakeModulePointer<float>("UnityPlayer.dll", 0x17B84D8, 0x150, 0xC00);
                        if (DirectPositionX != null && DirectPositionY != null && DirectPositionZ != null)
                            logger.Log("Using Aug2021 direct position pointers.");
                        break;

                    case GameVersion.Oct2025:
                        DirectPositionX = MakeModulePointer<float>("fmodstudio.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x48C)
                            ?? MakeModulePointer<float>("fmodstudiol.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x48C);
                        DirectPositionY = MakeModulePointer<float>("fmodstudio.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x490)
                            ?? MakeModulePointer<float>("fmodstudiol.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x490);
                        DirectPositionZ = MakeModulePointer<float>("fmodstudio.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x494)
                            ?? MakeModulePointer<float>("fmodstudiol.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x494);
                        if (DirectPositionX != null && DirectPositionY != null && DirectPositionZ != null)
                            logger.Log("Using Oct2025 direct position pointers.");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Direct position pointers unavailable: {ex.Message}");
            }
        }

        private Pointer<T> MakeModulePointer<T>(string moduleName, int moduleOffset, params int[] offsets) where T : unmanaged
        {
            ProcessModule module = game?.Process.Modules()
                .FirstOrDefault(m =>
                    string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Path.GetFileName(m.FileName), moduleName, StringComparison.OrdinalIgnoreCase));

            if (module == null)
            {
                return null;
            }

            var factory = new NestedPointerFactory(game);
            return factory.Make<T>(module.BaseAddress + moduleOffset, offsets);
        }

        public bool IsMovementInputActive(float threshold = 0.001f)
        {
            return IsAxisActive(MoveDirectionX, threshold) || IsAxisActive(MoveDirectionZ, threshold);
        }

        private static bool BecameActive(Pointer<float> axis, float threshold)
        {
            if (axis == null)
                return false;

            return Math.Abs(axis.New) > threshold && Math.Abs(axis.Old) <= threshold;
        }

        private static bool IsHorizontallyActive(Pointer<float> x, Pointer<float> z, float threshold)
        {
            if (x == null || z == null)
                return false;

            return Math.Sqrt(x.New * x.New + z.New * z.New) > threshold;
        }

        private static bool BecameHorizontallyActive(Pointer<float> x, Pointer<float> z, float threshold)
        {
            if (x == null || z == null)
                return false;

            double oldMagnitude = Math.Sqrt(x.Old * x.Old + z.Old * z.Old);
            double newMagnitude = Math.Sqrt(x.New * x.New + z.New * z.New);
            return oldMagnitude <= threshold && newMagnitude > threshold;
        }

        private static bool IsAxisActive(Pointer<float> axis, float threshold)
        {
            if (axis == null)
                return false;

            return Math.Abs(axis.New) > threshold;
        }

        private void ResolveUnityObjectCachedPtrOffset(UnityHelperTask.UnityHelperBase unity)
        {
            unityObjectCachedPtrOffset = 0x10;

            try
            {
                IntPtr unityCoreImage = unity.TryFindImageOnce(
                    "UnityEngine.CoreModule",
                    "UnityEngine.CoreModule.dll",
                    "UnityEngine",
                    "UnityEngine.dll");

                if (unityCoreImage == IntPtr.Zero)
                {
                    logger.Log("Unity core image not found; using fallback UnityEngine.Object.m_CachedPtr offset 0x10.");
                    return;
                }

                IntPtr objectKlass = unity.TryFindClassOnce("Object", unityCoreImage);
                if (objectKlass == IntPtr.Zero)
                {
                    logger.Log("UnityEngine.Object class not found; using fallback UnityEngine.Object.m_CachedPtr offset 0x10.");
                    return;
                }

                int cachedPtrOffset = mono.GetFieldOffset(objectKlass, "m_CachedPtr");
                if (cachedPtrOffset != 0)
                {
                    unityObjectCachedPtrOffset = cachedPtrOffset;
                    logger.Log($"UnityEngine.Object.m_CachedPtr resolved at 0x{unityObjectCachedPtrOffset:X}.");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to resolve UnityEngine.Object.m_CachedPtr, using fallback 0x10: {ex.Message}");
                return;
            }

            logger.Log("Using fallback UnityEngine.Object.m_CachedPtr offset 0x10.");
        }

        private bool IsLiveManagedUnityObject(IntPtr rawObject)
        {
            if (rawObject == IntPtr.Zero)
                return false;

            return game.Read<IntPtr>(rawObject + unityObjectCachedPtrOffset) != IntPtr.Zero;
        }

        private static bool IsMainMenuPosition(float x, float y, float z)
        {
            if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z))
                return false;

            return Math.Abs(x) <= 0.01f
                && Math.Abs(y - 1.75f) <= 0.01f
                && Math.Abs(z) <= 0.01f;
        }

        private List<TechType> ReadEquipmentTypes()
        {
            var result = new List<TechType>();

            IntPtr invMain = game.Read<IntPtr>(mono.GetStaticAddress(invStaticKlass) + invStaticOffset);
            if (invMain == IntPtr.Zero)
                return result;

            IntPtr equipment = game.Read<IntPtr>(invMain + off_equipment);
            if (equipment == IntPtr.Zero)
                return result;

            IntPtr dict = game.Read<IntPtr>(equipment + off_equippedCount);
            if (dict == IntPtr.Zero)
                return result;

            for (int attempt = 0; attempt < 3; attempt++)
            {
                int versionBefore = dict_off_version != 0 ? game.Read<int>(dict + dict_off_version) : 0;

                IntPtr entriesArr = game.Read<IntPtr>(dict + dict_off_entries);
                if (entriesArr == IntPtr.Zero)
                    break;

                int len = game.Read<int>(entriesArr + arr_off_len);
                if (len <= 0 || len > 200000)
                    break;

                IntPtr basePtr = entriesArr + arr_data_base;
                const int stride = 16;

                for (int i = 0; i < len; i++)
                {
                    IntPtr entry = basePtr + i * stride;
                    int hashCode = game.Read<int>(entry + 0x00);
                    if (hashCode < 0)
                        continue;

                    int keyInt = game.Read<int>(entry + 0x08);
                    int count = game.Read<int>(entry + 0x0C);
                    if (keyInt <= 0 || count <= 0)
                        continue;

                    for (int item = 0; item < count; item++)
                        result.Add((TechType)keyInt);
                }

                int versionAfter = dict_off_version != 0 ? game.Read<int>(dict + dict_off_version) : versionBefore;
                if (versionAfter == versionBefore)
                    return result;

                result.Clear();
            }

            return result;
        }

        private InventorySnapshot ReadInventorySnapshot()
        {
            var empty = new Dictionary<TechType, int>();

            IntPtr invMain = game.Read<IntPtr>(mono.GetStaticAddress(invStaticKlass) + invStaticOffset);
            if (invMain == IntPtr.Zero)
                return new InventorySnapshot(empty, 0);

            IntPtr container = game.Read<IntPtr>(invMain + off_container);
            if (container == IntPtr.Zero)
                return new InventorySnapshot(empty, 0);

            IntPtr dict = game.Read<IntPtr>(container + off_itemsDict);
            if (dict == IntPtr.Zero)
                return new InventorySnapshot(empty, 0);

            for (int attempt = 0; attempt < 3; attempt++)
            {
                var result = new Dictionary<TechType, int>();
                int slotsUsed = 0;
                int versionBefore = dict_off_version != 0 ? game.Read<int>(dict + dict_off_version) : 0;

                IntPtr entriesArr = game.Read<IntPtr>(dict + dict_off_entries);
                if (entriesArr == IntPtr.Zero)
                    break;

                int len = game.Read<int>(entriesArr + arr_off_len);
                if (len <= 0 || len > 200000)
                    break;

                IntPtr basePtr = entriesArr + arr_data_base;
                const int stride = 24;

                for (int i = 0; i < len; i++)
                {
                    IntPtr entry = basePtr + i * stride;
                    int hashCode = game.Read<int>(entry + 0x00);
                    if (hashCode < 0)
                        continue;

                    int keyInt = game.Read<int>(entry + 0x08);
                    IntPtr itemGroupPtr = game.Read<IntPtr>(entry + 0x10);
                    if (keyInt <= 0 || itemGroupPtr == IntPtr.Zero)
                        continue;

                    int id = off_itemGroup_id != 0 ? game.Read<int>(itemGroupPtr + off_itemGroup_id) : keyInt;
                    if (id != keyInt)
                        continue;

                    IntPtr listPtr = game.Read<IntPtr>(itemGroupPtr + off_itemGroup_items);
                    if (listPtr == IntPtr.Zero)
                        continue;

                    int count = game.Read<int>(listPtr + off_list_size);
                    if ((uint)count > 100000)
                        continue;

                    int width = off_itemGroup_width != 0 ? game.Read<int>(itemGroupPtr + off_itemGroup_width) : 1;
                    int height = off_itemGroup_height != 0 ? game.Read<int>(itemGroupPtr + off_itemGroup_height) : 1;
                    if (width <= 0)
                        width = 1;
                    if (height <= 0)
                        height = 1;

                    result[(TechType)keyInt] = count;
                    slotsUsed += width * height * count;
                }

                int versionAfter = dict_off_version != 0 ? game.Read<int>(dict + dict_off_version) : versionBefore;
                if (versionAfter == versionBefore)
                    return new InventorySnapshot(result, slotsUsed);
            }

            return new InventorySnapshot(empty, 0);
        }

        private Dictionary<TechType, int> ReadCraftCountTotals()
        {
            var result = new Dictionary<TechType, int>();
            IntPtr dict = craftingAnalyticsEntriesPtr?.New ?? IntPtr.Zero;
            if (dict == IntPtr.Zero)
                return result;

            const int stride = 36;
            const int keyOffset = 0x08;
            const int craftCountOffset = 0x18;

            for (int attempt = 0; attempt < 3; attempt++)
            {
                int versionBefore = dict_off_version != 0 ? game.Read<int>(dict + dict_off_version) : 0;

                IntPtr entriesArr = game.Read<IntPtr>(dict + dict_off_entries);
                if (entriesArr == IntPtr.Zero)
                    break;

                int len = game.Read<int>(entriesArr + arr_off_len);
                if (len <= 0 || len > 200000)
                    break;

                IntPtr basePtr = entriesArr + arr_data_base;
                for (int i = 0; i < len; i++)
                {
                    IntPtr entry = basePtr + i * stride;
                    int hashCode = game.Read<int>(entry + 0x00);
                    if (hashCode < 0)
                        continue;

                    TechType techType = (TechType)game.Read<int>(entry + keyOffset);
                    int craftCount = game.Read<int>(entry + craftCountOffset);
                    if (techType == TechType.None || craftCount <= 0)
                        continue;

                    result[techType] = craftCount;
                }

                int versionAfter = dict_off_version != 0 ? game.Read<int>(dict + dict_off_version) : versionBefore;
                if (versionAfter == versionBefore)
                    return result;

                result.Clear();
            }

            return result;
        }

        private List<EncyclopediaEntry> ReadPDAEncyMapping()
        {
            var result = new List<EncyclopediaEntry>();
            IntPtr dict = encyclopediaPtr.New;
            if (dict == IntPtr.Zero)
                return result;

            int stringHeader = game.PointerSize * 2 + 0x4;

            for (int attempt = 0; attempt < 3; attempt++)
            {
                int versionBefore = dict_off_version != 0 ? game.Read<int>(dict + dict_off_version) : 0;

                IntPtr entriesArr = game.Read<IntPtr>(dict + dict_off_entries);
                if (entriesArr == IntPtr.Zero)
                    break;

                int len = game.Read<int>(entriesArr + arr_off_len);
                if (len <= 0 || len > 200000)
                    break;

                IntPtr basePtr = entriesArr + arr_data_base;
                const int stride = 24;

                for (int i = 0; i < len; i++)
                {
                    IntPtr entry = basePtr + i * stride;
                    int hashCode = game.Read<int>(entry + 0x00);
                    if (hashCode < 0)
                        continue;

                    IntPtr keyPtr = game.Read<IntPtr>(entry + 0x08);
                    IntPtr valuePtr = game.Read<IntPtr>(entry + 0x10);
                    if (keyPtr == IntPtr.Zero || valuePtr == IntPtr.Zero)
                        continue;

                    string key = game.ReadString(keyPtr + stringHeader, EStringType.UTF16Sized);
                    if (!string.IsNullOrEmpty(key) && Enum.TryParse(key, out EncyclopediaEntry encyEntry))
                        result.Add(encyEntry);
                }

                int versionAfter = dict_off_version != 0 ? game.Read<int>(dict + dict_off_version) : versionBefore;
                if (versionAfter == versionBefore)
                    return result;

                result.Clear();
            }

            return result;
        }

        private bool TryReadCompletedStoryGoals(out HashSet<string> result, out string failureReason)
        {
            result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            failureReason = string.Empty;

            if (completedStoryGoalsPtr == null)
            {
                failureReason = "completed-goals pointer was not created.";
                return false;
            }

            IntPtr hashSetPtr = completedStoryGoalsPtr?.New ?? IntPtr.Zero;
            if (hashSetPtr == IntPtr.Zero)
            {
                failureReason = "StoryGoalManager.main or completedGoals is not available yet.";
                return false;
            }

            int stringHeader = game.PointerSize * 2 + 0x4;
            if (storyGoalSlotsOffset != 0)
            {
                bool readCachedLayout = storyGoalUsesStringArray
                    ? TryReadStoryGoalsFromStringArray(hashSetPtr, storyGoalSlotsOffset, stringHeader, out result)
                    : TryReadStoryGoalsFromSlotArray(hashSetPtr, storyGoalSlotsOffset, stringHeader, out result);
                if (readCachedLayout)
                    return true;

                storyGoalReaderMode = string.Empty;
                storyGoalSlotsOffset = 0;
                storyGoalUsesStringArray = false;
            }

            var candidates = new List<(HashSet<string> Goals, string Mode, int Score, int Offset, bool UsesStringArray)>();
            foreach (int slotsOffset in new[] { hashset_off_slots, 0x18, 0x20, 0x10 }.Where(offset => offset != 0).Distinct())
            {
                if (TryReadStoryGoalsFromSlotArray(hashSetPtr, slotsOffset, stringHeader, out HashSet<string> slotGoals))
                    candidates.Add((slotGoals, $"slot-array@0x{slotsOffset:X}", ScoreStoryGoalSet(slotGoals), slotsOffset, false));

                if (TryReadStoryGoalsFromStringArray(hashSetPtr, slotsOffset, stringHeader, out HashSet<string> stringGoals))
                    candidates.Add((stringGoals, $"string-array@0x{slotsOffset:X}", ScoreStoryGoalSet(stringGoals), slotsOffset, true));
            }

            if (candidates.Count == 0)
            {
                failureReason = "no valid Story Goal strings were found in completedGoals.";
                return false;
            }

            var best = candidates
                .OrderByDescending(candidate => candidate.Score)
                .ThenByDescending(candidate => candidate.Goals.Count)
                .First();

            result = best.Goals;
            storyGoalSlotsOffset = best.Offset;
            storyGoalUsesStringArray = best.UsesStringArray;
            if (!string.Equals(storyGoalReaderMode, best.Mode, StringComparison.Ordinal))
            {
                storyGoalReaderMode = best.Mode;
                logger.Log($"Story goal reader selected {storyGoalReaderMode} ({result.Count} goals).");
            }

            return true;
        }

        private bool TryReadStoryGoalsFromSlotArray(IntPtr hashSetPtr, int slotsOffset, int stringHeader, out HashSet<string> result)
        {
            result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IntPtr slotsArray = game.Read<IntPtr>(hashSetPtr + slotsOffset);
            if (slotsArray == IntPtr.Zero)
                return false;

            int length = game.Read<int>(slotsArray + arr_off_len);
            if (length <= 0 || length > 200000)
                return false;

            int slotStride = game.PointerSize == 8 ? 0x10 : 0x0C;
            IntPtr firstSlot = slotsArray + arr_data_base;
            for (int i = 0; i < length; i++)
            {
                IntPtr slot = firstSlot + i * slotStride;
                if (game.Read<int>(slot) < 0)
                    continue;

                IntPtr valuePtr = game.Read<IntPtr>(slot + 0x08);
                if (TryReadStoryGoalKey(valuePtr, stringHeader, out string goal))
                    result.Add(goal);
            }

            return result.Count > 0;
        }

        private bool TryReadStoryGoalsFromStringArray(IntPtr hashSetPtr, int slotsOffset, int stringHeader, out HashSet<string> result)
        {
            result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IntPtr stringArray = game.Read<IntPtr>(hashSetPtr + slotsOffset);
            if (stringArray == IntPtr.Zero)
                return false;

            int length = game.Read<int>(stringArray + arr_off_len);
            if (length <= 0 || length > 200000)
                return false;

            IntPtr firstString = stringArray + arr_data_base;
            for (int i = 0; i < length; i++)
            {
                IntPtr valuePtr = game.Read<IntPtr>(firstString + i * game.PointerSize);
                if (TryReadStoryGoalKey(valuePtr, stringHeader, out string goal))
                    result.Add(goal);
            }

            return result.Count > 0;
        }

        private bool TryReadStoryGoalKey(IntPtr valuePtr, int stringHeader, out string goal)
        {
            goal = string.Empty;
            if (valuePtr == IntPtr.Zero)
                return false;

            try
            {
                goal = game.ReadString(valuePtr + stringHeader, EStringType.UTF16Sized);
                return IsLikelyStoryGoalKey(goal);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsLikelyStoryGoalKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
                return false;

            return value.All(character =>
                char.IsLetterOrDigit(character)
                || character == '_'
                || character == '.'
                || character == '-');
        }

        private static int ScoreStoryGoalSet(HashSet<string> goals)
        {
            int score = goals.Count;
            foreach (string goal in goals)
            {
                if (TryParseStoryGoal(goal, out _))
                    score += 100;
                else if (goal.StartsWith("On", StringComparison.OrdinalIgnoreCase)
                    || goal.StartsWith("Log_", StringComparison.OrdinalIgnoreCase)
                    || goal.StartsWith("Call_", StringComparison.OrdinalIgnoreCase)
                    || goal.StartsWith("Scan_", StringComparison.OrdinalIgnoreCase))
                    score += 3;
            }

            return score;
        }

        private static bool TryParseStoryGoal(string value, out StoryGoal goal)
        {
            return Enum.TryParse(value, true, out goal)
                && goal != StoryGoal.None
                && Enum.IsDefined(typeof(StoryGoal), goal);
        }

    }

    public class InvChangeInfo
    {
        public int Count { get; set; }
        public Stopwatch ElapsedTime { get; }

        public InvChangeInfo(int count, Stopwatch elapsedTime)
        {
            Count = count;
            ElapsedTime = elapsedTime ?? throw new ArgumentNullException(nameof(elapsedTime));
        }
    }

    internal sealed class InventorySnapshot
    {
        public InventorySnapshot(Dictionary<TechType, int> counts, int slotsUsed)
        {
            Counts = counts ?? throw new ArgumentNullException(nameof(counts));
            SlotsUsed = slotsUsed;
        }

        public Dictionary<TechType, int> Counts { get; }
        public int SlotsUsed { get; }
    }
}
