using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
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
        public bool pointersInitialized;
        public GameVersion gameVersion = BelowZeroVersionedData.BaselineVersion;

        private const int inventoryCapacitySlots = 48;
        private const int maxInventoryTimeWithoutChangingMs = 1000;
        private const int maxBuilderMenuSelectionWindowMs = 3000;
        private const int minThrowSplitCooldownMs = 250;
        private const int maxAcquireAlAnInteractionWindowMs = 5000;
        private const int acquireAlAnDestroyBurstWindowMs = 750;
        private const int maxBraceInteractionWindowMs = 120000;
        private const float mainMenuPosX = 0f;
        private const float mainMenuPosY = 1.75f;
        private const float mainMenuPosZ = 0f;
        private const float mainMenuPositionTolerance = 0.01f;
        private const string SanctuaryBiome = "Precursor_Sanctuary";
        private const string CrystalCavesCacheBiome = "CrystalCave_Cache";
        private const string DeepLilypadsCacheBiome = "lilyPads_Deep_Cache";
        private const string ArcticSpiresCacheBiome = "ArcticSpiresCache";
        private const string PurpleVentsBiome = "purpleVents";
        private const string TwistyBridgesBiome = "TwistyBridges";
        private const string DeepTwistyBridgesBiome = "twistyBridges_Deep";
        private const string ArcticKelpCaveInnerBiome = "ArcticKelp_CaveInner";
        private const string ArcticKelpCaveOuterBiome = "ArcticKelp_CaveOuter";
        private const string AcquireAlAnTooltipKey = "Alan_TerminalInteract";
        private const string AcquireAlAnReticleText = "Insert storage medium";
        private const string GoalSanctuaryCompleted = "SanctuaryCompleted";
        private const string GoalEndGameEnterShip = "EndGameEnterShip";
        // Reserved for future pillar-related prefabricated splits.
        private const string GoalEndOfRepairPillar1 = "EndofRepairPillar1";
        private const string GoalEndOfRepairPillar2 = "EndofRepairPillar2";
        private const string GoalAlAnTransferred = "OnPrecursorNPCTransfer";
        private const string GoalBodyBuilt = "Log_Alan_BodyBuilt";
        private const string FabricationFacilityBiome = "Precursor_Fabricator";
        private static readonly Regex BuildTimeYearRegex = new Regex(@"(?<!\d)(20\d{2})(?!\d)", RegexOptions.Compiled);
        public readonly Dictionary<SplitName, Func<bool>> splitConditions;
        public readonly Dictionary<SplitName, Func<bool>> subConditions;

        private readonly Dictionary<TechType, InvChangeInfo> curPickUpCounts = new Dictionary<TechType, InvChangeInfo>();
        private readonly Dictionary<TechType, InvChangeInfo> curDropCounts = new Dictionary<TechType, InvChangeInfo>();
        private Dictionary<TechType, int> currentInventoryChanges = new Dictionary<TechType, int>();
        private readonly Stopwatch throwSplitCooldown = new Stopwatch();
        private readonly Stopwatch builderMenuSelectionWindow = new Stopwatch();
        private readonly Stopwatch acquireAlAnInteractionWindow = new Stopwatch();
        private readonly Stopwatch acquireAlAnDestroyBurstWindow = new Stopwatch();
        private readonly Stopwatch braceInteractionWindow = new Stopwatch();
        private TechType lastThrowSplitTool = TechType.None;
        private TechType startedCraftThisFrame = TechType.None;
        private TechType completedCraftFallbackThisFrame = TechType.None;
        private TechType completedBuildThisFrame = TechType.None;
        private TechType pendingBuilderCompletionTechType = TechType.None;
        private IntPtr trackedHoverpadConstructorPtr = IntPtr.Zero;
        private bool hasUnityObjectCachedPtrOffset = true;
        private bool builderMenuSelectionPending;
        private bool bridgeInsertThisFrame;
        private bool braceReadyForCinematic;
        private bool craftAnalyticsInitialized;
        private bool hoverpadConstructingInitialized;
        private bool hoverpadConstructingOld;
        private bool acquireAlAnTargetActive;
        private bool inventoryInitialized;
        private bool blueprintsInitialized;
        private bool encyclopediaInitialized;
        private bool storyGoalsInitialized;
        private string storyGoalReaderMode = string.Empty;
        private string acquireAlAnTargetGoalKey = string.Empty;
        private string acquireAlAnArmedGoalKey = string.Empty;
        private bool heldMovementStartArmedAfterReset;
        private int playerInventorySlotsUsed;
        private int playerInventorySlotsUsedOld;
        private int unityObjectCachedPtrOffset = 0x10;
        private bool directPositionPointersBound;
        private DateTime nextDirectSignalRetryUtc = DateTime.MinValue;
        private IntPtr acquireAlAnTargetPtr = IntPtr.Zero;

        public Pointer<bool> IsIntroCinematicActive;
        public Pointer<bool> IsAnimationPlaying;
        public Pointer<bool> IsLoadingScreenShowing;
        public Pointer<bool> IsPlayerJumping;
        public Pointer<bool> IsDying;
        public Pointer<bool> PlayerControllerInputEnabled;
        public Pointer<float> PlayerControllerVelocityX;
        public Pointer<float> PlayerControllerVelocityY;
        public Pointer<float> PlayerControllerVelocityZ;
        public Pointer<float> Health;
        public Pointer<float> MoveDirectionX;
        public Pointer<float> MoveDirectionZ;
        public Pointer<bool> BuilderMenuState;
        public Pointer<int> BuilderLastTechType;
        public Pointer<IntPtr> ActiveCrafterLogic;
        public Pointer<bool> PlayerIsUnderwater;
        public Pointer<bool> PlayerIsInside;
        public Pointer<IntPtr> MainMenu;
        public Pointer<IntPtr> BuilderPrefab;
        public Pointer<IntPtr> PlayerMain;
        public Pointer<IntPtr> CraftingMenu;
        public Pointer<IntPtr> GuiHandActiveTarget;
        private Pointer<IntPtr> craftingAnalyticsEntriesPtr;
        private Pointer<IntPtr> knownTechPtr;
        private Pointer<IntPtr> encyclopediaPtr;
        public Pointer<bool> PDAIsInUse;
        public Pointer<int> PDATab;
        public Pointer<int> CraftedNode;
        public StringPointer BiomeString;
        public StringPointer ActiveToolName;
        public Pointer<float> PlayerLastPositionX;
        public Pointer<float> PlayerLastPositionY;
        public Pointer<float> PlayerLastPositionZ;
        private Pointer<float> DirectPositionX;
        private Pointer<float> DirectPositionY;
        private Pointer<float> DirectPositionZ;
        private StringPointer handReticleRawTextHand;
        private StringPointer handReticleVisibleTextHand;
        public Pointer<bool> PrecursorTransferActive;
        private Pointer<IntPtr> completedStoryGoalsPtr;

        public Dictionary<TechType, int> PlayerInventory = new Dictionary<TechType, int>();
        public Dictionary<TechType, int> PlayerInventoryOld = new Dictionary<TechType, int>();
        public List<TechType> PlayerEquipment = new List<TechType>();
        public List<TechType> PlayerEquipmentOld = new List<TechType>();
        public List<TechType> KnownTech = new List<TechType>();
        public List<TechType> KnownTechOld = new List<TechType>();
        public List<EncyclopediaEntry> Encyclopedia = new List<EncyclopediaEntry>();
        public List<EncyclopediaEntry> EncyclopediaOld = new List<EncyclopediaEntry>();
        private HashSet<string> completedStoryGoals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> completedStoryGoalsOld = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> storyGoalsCompletedThisFrame = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public bool wasInMainMenu;
        private readonly Dictionary<TechType, int> craftCounts = new Dictionary<TechType, int>();
        private readonly HashSet<TechType> suppressCraftCompletionFallback = new HashSet<TechType>();

        private IntPtr invKlass;
        private IntPtr equipmentKlass;
        private IntPtr icKlass;
        private IntPtr invStaticKlass;
        private IntPtr itemGroupKlass;

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
        private int hashset_off_version;
        private int arr_off_len;
        private int arr_data_base;
        private int off_hoverpadConstructorField;
        private int off_hoverpadTerminalHoverpad;
        private int off_hoverpadConstructorConstructing;
        private int off_storyHandTargetPrimaryTooltip;
        private int off_storyHandTargetSecondaryTooltip;
        private int off_storyHandTargetGoal;
        private int off_storyGoalKey;
        private int off_array3SizeX = 0x10;
        private int off_array3SizeY = 0x14;
        private int off_array3SizeZ = 0x18;
        private int off_array3Data = 0x20;
        private MethodInfo helperClassNameMethod;
        private MethodInfo helperClassNamespaceMethod;

        public BelowZeroMemory(LiveSplitState state, BelowZeroComponent component, Logger logger, BelowZeroSettings settings)
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
                gameVersion = BelowZeroVersionedData.BaselineVersion;
                settings.SetGameVersion(gameVersion);
                ResetRunState();
                wasInMainMenu = false;
                isInMainMenu = false;
                directPositionPointersBound = false;
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

            subConditions = new Dictionary<SplitName, Func<bool>>
            {
                { SplitName.Inventory, () =>
                    {
                        var split = (ItemSplit)CurrentSplitToCheck;
                        var techType = split.Item.ConvertTo<TechType>();
                        return !split.IsCount
                            ? HasPlayerItem(techType)
                            : GetPlayerItemCount(techType) == split.Count;
                    }
                },
                { SplitName.Blueprint, () => KnownTech.Contains(((BlueprintSplit)CurrentSplitToCheck).Blueprint.ConvertTo<TechType>()) },
                { SplitName.Encyclopedia, () => Encyclopedia.Contains(((EncyclopediaSplit)CurrentSplitToCheck).Entry) },
                { SplitName.Biome, () =>
                    {
                        var biomeSplit = (BiomeSplit)CurrentSplitToCheck;
                        return biomeSplit.Biomes.Biome1 == Biome.Any
                            || string.Equals(BiomeString.New, biomeSplit.Biomes.Biome1.ToString(), StringComparison.OrdinalIgnoreCase);
                    }
                },
            };

            splitConditions = new Dictionary<SplitName, Func<bool>>
            {
                {
                    SplitName.Inventory,
                    () =>
                    {
                        var split = (ItemSplit)CurrentSplitToCheck;
                        var techType = split.Item.ConvertTo<TechType>();

                        int currentPickUpChange = curPickUpCounts.TryGetValue(techType, out InvChangeInfo pickUpInfo) ? pickUpInfo.Count : 0;
                        int currentDropChange = curDropCounts.TryGetValue(techType, out InvChangeInfo dropInfo) ? dropInfo.Count : 0;

                        int change = split.PickUp ? currentPickUpChange : -currentDropChange;
                        bool changedInRightDirection = change > 0;

                        if (!split.IsCount)
                        {
                            int currentChange = currentInventoryChanges.TryGetValue(techType, out int delta) ? delta : 0;
                            return split.PickUp ? currentChange > 0 : currentChange < 0;
                        }

                        if (split.AlreadySplitInvChanging)
                            return false;

                        bool shouldSplit = change >= split.Count && changedInRightDirection;
                        split.AlreadySplitInvChanging = shouldSplit;
                        return shouldSplit;
                    }
                },
                {
                    SplitName.Blueprint,
                    () =>
                    {
                        TechType techType = ((BlueprintSplit)CurrentSplitToCheck).Blueprint.ConvertTo<TechType>();
                        return KnownTech.Contains(techType) && !KnownTechOld.Contains(techType);
                    }
                },
                {
                    SplitName.Encyclopedia,
                    () =>
                    {
                        EncyclopediaEntry entry = ((EncyclopediaSplit)CurrentSplitToCheck).Entry;
                        return Encyclopedia.Contains(entry) && !EncyclopediaOld.Contains(entry);
                    }
                },
                {
                    SplitName.Biome,
                    () =>
                    {
                        var biomeSplit = (BiomeSplit)CurrentSplitToCheck;
                        return (biomeSplit.Biomes.Biome1 == Biome.Any && biomeSplit.Biomes.Biome2 == Biome.Any && BiomeString.Changed)
                            || (biomeSplit.Biomes.Biome1 == Biome.Any && string.Equals(BiomeString.New, biomeSplit.Biomes.Biome2.ToString(), StringComparison.OrdinalIgnoreCase) && BiomeString.Changed)
                            || (biomeSplit.Biomes.Biome2 == Biome.Any && string.Equals(BiomeString.Old, biomeSplit.Biomes.Biome1.ToString(), StringComparison.OrdinalIgnoreCase) && BiomeString.Changed)
                            || (string.Equals(BiomeString.New, biomeSplit.Biomes.Biome2.ToString(), StringComparison.OrdinalIgnoreCase)
                                && string.Equals(BiomeString.Old, biomeSplit.Biomes.Biome1.ToString(), StringComparison.OrdinalIgnoreCase));
                    }
                },
                {
                    SplitName.Craft,
                    () =>
                    {
                        TechType techType = ((CraftSplit)CurrentSplitToCheck).Craftable.ConvertTo<TechType>();
                        return TryConsumeStartedCraft(techType) || TryConsumeCompletedCraftFallback(techType);
                    }
                },
                {
                    SplitName.Build,
                    () => TryConsumeCompletedBuild(((BuildSplit)CurrentSplitToCheck).Craftable.ConvertTo<TechType>())
                },
                { SplitName.FullInventorySplit, () => playerInventorySlotsUsed == inventoryCapacitySlots && playerInventorySlotsUsedOld != inventoryCapacitySlots },
                { SplitName.DeathSplit, () => (Health.New <= 0 && Health.Old > 0) || (IsDying.New && !IsDying.Old) },
                { SplitName.ThrowFlareSplit, () => TryConsumeThrownInventoryItem(TechType.Flare, "Flare") },
                { SplitName.ThrowSnowballSplit, () => TryConsumeThrownInventoryItem(TechType.SnowBall, "Snowball") },
                {
                    SplitName.PropulsionCannonDrownSplit,
                    () => IsDeathThisFrame()
                        && IsCurrentBiome(ArcticKelpCaveInnerBiome, ArcticKelpCaveOuterBiome)
                        && HasKnownTech(TechType.PropulsionCannon, TechType.PropulsionCannonBlueprint)
                },
                {
                    SplitName.TwistyBridgesDeathSplit,
                    () => IsDeathThisFrame()
                        && IsCurrentBiome(TwistyBridgesBiome, DeepTwistyBridgesBiome)
                },
                { SplitName.AcquireAlAnSplit, TryConsumeAcquireAlAn },
                {
                    SplitName.BoosterTankDeathSplit,
                    () => IsDeathThisFrame()
                        && IsCurrentBiome(PurpleVentsBiome)
                        && HasKnownTech(TechType.SuitBoosterTank)
                },
                {
                    SplitName.ArcticSpiresScanSplit,
                    () => HasKnownTechUnlockedThisFrame(TechType.PrecursorNPCTissue)
                },
                {
                    SplitName.ArcticSpiresDeathSplit,
                    () => IsDeathThisFrame() && IsCurrentBiome(ArcticSpiresCacheBiome)
                },
                {
                    SplitName.ArcticSpiresTransitionSplit,
                    () => HasPlayableBiomeTransitionFrom(ArcticSpiresCacheBiome)
                },
                {
                    SplitName.DeepLilypadsCacheScanSplit,
                    () => HasKnownTechUnlockedThisFrame(TechType.PrecursorNPCSkeleton)
                },
                {
                    SplitName.DeepLilypadCacheDeathSplit,
                    () => IsDeathThisFrame() && IsCurrentBiome(DeepLilypadsCacheBiome)
                },
                {
                    SplitName.DeepLilypadsCacheTransitionSplit,
                    () => HasPlayableBiomeTransitionFrom(DeepLilypadsCacheBiome)
                },
                {
                    SplitName.CrystalCavesCacheScanSplit,
                    () => HasKnownTechUnlockedThisFrame(TechType.PrecursorNPCOrgans)
                },
                {
                    SplitName.CrystalCavesCacheDeathSplit,
                    () => IsDeathThisFrame() && IsCurrentBiome(CrystalCavesCacheBiome)
                },
                {
                    SplitName.CrystalCavesCacheTransitionSplit,
                    () => HasPlayableBiomeTransitionFrom(CrystalCavesCacheBiome)
                },
                {
                    SplitName.CommenceStorageMediumFabricationSplit,
                    () => TryConsumeStartedCraft(TechType.PrecursorNPCBody) || HasStoryGoalCompletedThisFrame(GoalBodyBuilt)
                },
                {
                    SplitName.AlAnTransferSplit,
                    () => HasTransferStartedThisFrame() || HasStoryGoalCompletedThisFrame(GoalAlAnTransferred)
                },
                {
                    SplitName.AlAnTransferDeathSplit,
                    () => IsDeathThisFrame()
                        && IsCurrentBiome(FabricationFacilityBiome)
                },
                { SplitName.BraceSplit, TryConsumeBrace },
                { SplitName.InsertHydraulicsFluidSplit, TryConsumeBridgeInsert },
            };
        }

        public override bool Update()
        {
            if (!base.Update() || !pointersInitialized || game == null)
                return false;

            RefreshVersionSpecificSignalPointersIfNeeded();

            wasInMainMenu = isInMainMenu;
            isInMainMenu = IsInMainMenu();
            if (isInMainMenu)
            {
                if (!wasInMainMenu)
                    logger.Log("Main menu entered.");

                ResetRunState();
                startedTimerBefore = false;
            }
            else if (wasInMainMenu)
            {
                logger.Log("Left main menu.");
            }

            if (settings.StartEnabled || Needs(SplitName.Inventory, SplitName.FullInventorySplit, SplitName.ThrowFlareSplit, SplitName.ThrowSnowballSplit, SplitName.InsertHydraulicsFluidSplit))
                UpdateInventory();

            if (Needs(
                SplitName.Blueprint,
                SplitName.PropulsionCannonDrownSplit,
                SplitName.BoosterTankDeathSplit,
                SplitName.ArcticSpiresScanSplit,
                SplitName.DeepLilypadsCacheScanSplit,
                SplitName.CrystalCavesCacheScanSplit))
                UpdateBlueprints();

            if (Needs(SplitName.Encyclopedia))
                UpdateEncyclopedia();

            if (Needs(
                SplitName.AcquireAlAnSplit,
                SplitName.CommenceStorageMediumFabricationSplit,
                SplitName.AlAnTransferSplit,
                SplitName.BraceSplit))
            {
                UpdateCompletedStoryGoals();
            }

            if (Needs(SplitName.Craft, SplitName.AcquireAlAnSplit))
                CaptureTrackedInteractiveTargets();

            if (Needs(SplitName.Craft, SplitName.Build, SplitName.CommenceStorageMediumFabricationSplit))
                UpdateCraftAndBuildState();

            if (Needs(SplitName.InsertHydraulicsFluidSplit))
                UpdateBridgeInteractiveState();

            return true;
        }

        private void GetGameVersion()
        {
            string processPath = string.Empty;
            string processFileVersion = string.Empty;
            gameVersion = BelowZeroVersionedData.BaselineVersion;

            try
            {
                processPath = game.Process.MainModule?.FileName ?? string.Empty;
                if (!string.IsNullOrEmpty(processPath))
                {
                    processFileVersion = FileVersionInfo.GetVersionInfo(processPath).FileVersion ?? string.Empty;
                    logger.Log($"Attached executable: {processPath}");
                    if (!string.IsNullOrEmpty(processFileVersion))
                        logger.Log($"Executable file version: {processFileVersion}");
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to read executable version info: {ex.Message}");
            }

            System.Diagnostics.ProcessModule unityPlayer = null;
            try
            {
                unityPlayer = game.Process.Modules
                    .Cast<System.Diagnostics.ProcessModule>()
                    .FirstOrDefault(module => string.Equals(Path.GetFileName(module.FileName), "UnityPlayer.dll", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to enumerate process modules: {ex.Message}");
            }

            long unityPlayerDiskSize = 0;
            if (unityPlayer != null)
            {
                logger.Log($"UnityPlayer path: {unityPlayer.FileName}");
                logger.Log($"UnityPlayer size: {unityPlayer.ModuleMemorySize}");
                try
                {
                    unityPlayerDiskSize = new FileInfo(unityPlayer.FileName).Length;
                    logger.Log($"UnityPlayer disk size: {unityPlayerDiskSize}");
                }
                catch (Exception ex)
                {
                    logger.Log($"Failed to read UnityPlayer disk size: {ex.Message}");
                }
            }
            else
            {
                logger.Log("UnityPlayer.dll not found; continuing with fallback version detection.");
            }

            bool versionDetected = false;
            if (!string.IsNullOrWhiteSpace(processFileVersion) && Version.TryParse(processFileVersion, out Version parsedVersion))
            {
                if (parsedVersion == new Version(2019, 4, 9, 630))
                {
                    gameVersion = GameVersion.Aug2021;
                    versionDetected = true;
                    logger.Log("Executable version exact match 2019.4.9.630 -> Aug2021.");
                }
                else if (parsedVersion == new Version(2019, 4, 36, 870))
                {
                    gameVersion = GameVersion.Oct2025;
                    versionDetected = true;
                    logger.Log("Executable version exact match 2019.4.36.870 -> Oct2025.");
                }
                else if (parsedVersion.Build == 9)
                {
                    gameVersion = GameVersion.Aug2021;
                    versionDetected = true;
                    logger.Log($"Executable version {parsedVersion} matched 2019.4.9.x family -> Aug2021.");
                }
                else if (parsedVersion.Build == 36)
                {
                    gameVersion = GameVersion.Oct2025;
                    versionDetected = true;
                    logger.Log($"Executable version {parsedVersion} matched 2019.4.36.x family -> Oct2025.");
                }
            }

            if (!versionDetected && unityPlayer != null)
            {
                try
                {
                    string unityPlayerFileVersion = FileVersionInfo.GetVersionInfo(unityPlayer.FileName).FileVersion ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(unityPlayerFileVersion) && Version.TryParse(unityPlayerFileVersion, out Version parsedUnityPlayerVersion))
                    {
                        if (parsedUnityPlayerVersion == new Version(2019, 4, 9, 630) || parsedUnityPlayerVersion.Build == 9)
                        {
                            gameVersion = GameVersion.Aug2021;
                            versionDetected = true;
                            logger.Log($"UnityPlayer file version {parsedUnityPlayerVersion} -> Aug2021.");
                        }
                        else if (parsedUnityPlayerVersion == new Version(2019, 4, 36, 870) || parsedUnityPlayerVersion.Build == 36)
                        {
                            gameVersion = GameVersion.Oct2025;
                            versionDetected = true;
                            logger.Log($"UnityPlayer file version {parsedUnityPlayerVersion} -> Oct2025.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log($"Failed to read UnityPlayer version info: {ex.Message}");
                }
            }

            if (!versionDetected && TryDetectVersionFromBuildTime(processPath, out GameVersion buildTimeVersion))
            {
                gameVersion = buildTimeVersion;
                versionDetected = true;
            }

            if (!versionDetected && !string.IsNullOrEmpty(processPath))
            {
                if (processPath.IndexOf("2021", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    gameVersion = GameVersion.Aug2021;
                    versionDetected = true;
                    logger.Log("Executable path hint matched 2021 -> Aug2021.");
                }
                else if (processPath.IndexOf("2025", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    gameVersion = GameVersion.Oct2025;
                    versionDetected = true;
                    logger.Log("Executable path hint matched 2025 -> Oct2025.");
                }
            }

            if (!versionDetected && unityPlayer != null)
            {
                switch (unityPlayerDiskSize)
                {
                    case 25893376:
                        gameVersion = GameVersion.Aug2021;
                        versionDetected = true;
                        logger.Log("UnityPlayer size exact match 25893376 -> Aug2021.");
                        break;
                    case 26137088:
                        gameVersion = GameVersion.Oct2025;
                        versionDetected = true;
                        logger.Log("UnityPlayer size exact match 26137088 -> Oct2025.");
                        break;
                    default:
                        logger.Log($"Unknown UnityPlayer.dll size {unityPlayerDiskSize}, keeping fallback {gameVersion}.");
                        break;
                }
            }

            if (!versionDetected)
                logger.Log($"Version not matched exactly; using fallback {gameVersion}.");

            logger.Log($"Game version {gameVersion}");
            settings.SetGameVersion(gameVersion);
            OnVersionDetected?.Invoke(gameVersion.ToString());
        }

        private bool TryDetectVersionFromBuildTime(string processPath, out GameVersion detectedVersion)
        {
            detectedVersion = BelowZeroVersionedData.BaselineVersion;
            if (string.IsNullOrWhiteSpace(processPath))
                return false;

            string processDirectory = Path.GetDirectoryName(processPath);
            if (string.IsNullOrWhiteSpace(processDirectory))
                return false;

            string buildTimePath = Path.Combine(processDirectory, "SubnauticaZero_Data", "StreamingAssets", "__buildtime.txt");
            if (!File.Exists(buildTimePath))
                return false;

            try
            {
                string buildTimeText = File.ReadAllText(buildTimePath);
                Match match = BuildTimeYearRegex.Match(buildTimeText ?? string.Empty);
                if (!match.Success || !int.TryParse(match.Groups[1].Value, out int buildYear))
                {
                    logger.Log($"Build time file did not contain a usable year: {buildTimePath}");
                    return false;
                }

                detectedVersion = buildYear <= 2021 ? GameVersion.Aug2021 : GameVersion.Oct2025;
                logger.Log($"Build time year {buildYear} from {buildTimePath} -> {detectedVersion}.");
                return true;
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to read build time file '{buildTimePath}': {ex.Message}");
                return false;
            }
        }

        private void InitPointers(IMonoHelper mono)
        {
            this.mono = mono;
            logger.Log("Initializing Below Zero pointers.");
            var ptrFactory = new MonoNestedPointerFactory(game, mono);
            var unity = (UnityHelperTask.UnityHelperBase)mono;

            ForceRefreshVersionSpecificSignalPointers();

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

            itemGroupKlass = mono.FindClass("ItemGroup", mono.MainImage);
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

                IntPtr hashSetKlass = unity.TryFindClassOnce("HashSet`1", core);
                hashset_off_slots = 0x18;
                hashset_off_version = 0x28;
                if (hashSetKlass != IntPtr.Zero)
                {
                    int slotsOffset = unity.ResolveFieldOffsetByNameOrPredicate(
                        hashSetKlass,
                        new[] { "_slots", "m_slots", "slots" },
                        name => UnityHelperTask.UnityNameUtil.NameHas(name, "slots"));
                    if (slotsOffset != 0)
                        hashset_off_slots = slotsOffset;

                    int versionOffset = unity.ResolveFieldOffsetByNameOrPredicate(
                        hashSetKlass,
                        new[] { "_version", "m_version", "version" },
                        name => UnityHelperTask.UnityNameUtil.NameHas(name, "version"));
                    if (versionOffset != 0)
                        hashset_off_version = versionOffset;
                }

            }

            knownTechPtr = ptrFactory.Make<IntPtr>("Player", "main", "knownTech");
            encyclopediaPtr = ptrFactory.Make<IntPtr>("Player", "main", "encyclopedia");

            IntPtr storyGoalManagerKlass = mono.FindClass("StoryGoalManager", mono.MainImage);
            if (storyGoalManagerKlass != IntPtr.Zero)
            {
                int off_completedGoals = mono.GetFieldOffset(storyGoalManagerKlass, "completedGoals");
                if (off_completedGoals != 0)
                {
                    Pointer<IntPtr> storyGoalManagerMainPtr = ptrFactory.Make<IntPtr>("StoryGoalManager", "<main>k__BackingField")
                        ?? ptrFactory.Make<IntPtr>("StoryGoalManager", "main");
                    completedStoryGoalsPtr = ptrFactory.Make<IntPtr>(storyGoalManagerMainPtr, off_completedGoals);
                }
            }

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
            int off_crafterLogic = mono.GetFieldOffset(mono.FindClass("Crafter"), "crafterLogic");
            ActiveCrafterLogic = ptrFactory.Make<IntPtr>(CraftingMenu, off_crafterLogic);

            MoveDirectionX = ptrFactory.Make<float>("GameInput", "moveDirection", 0x0);
            MoveDirectionZ = ptrFactory.Make<float>("GameInput", "moveDirection", 0x8);

            IntPtr array3Klass = unity.TryFindClassOnce("Array3`1", mono.MainImage);
            if (array3Klass != IntPtr.Zero)
            {
                int candSizeX = mono.GetFieldOffset(array3Klass, "sizeX");
                int candSizeY = mono.GetFieldOffset(array3Klass, "sizeY");
                int candSizeZ = mono.GetFieldOffset(array3Klass, "sizeZ");
                int candData = mono.GetFieldOffset(array3Klass, "data");
                if (candSizeX != 0)
                    off_array3SizeX = candSizeX;
                if (candSizeY != 0)
                    off_array3SizeY = candSizeY;
                if (candSizeZ != 0)
                    off_array3SizeZ = candSizeZ;
                if (candData != 0)
                    off_array3Data = candData;
            }

            Pointer<IntPtr> groundMotorPtr = ptrFactory.Make<IntPtr>("Player", "main", "groundMotor");
            int off_jumpState = mono.GetFieldOffset(mono.FindClass("GroundMotor"), "jumping");
            Pointer<IntPtr> jumpingStatePtr = ptrFactory.Make<IntPtr>(groundMotorPtr, off_jumpState);
            logger.Log("Jump state flag offset fixed at 0x24.");
            IsPlayerJumping = ptrFactory.Make<bool>(jumpingStatePtr, 0x24);

            IsDying = ptrFactory.Make<bool>("uGUI_PlayerDeath", "main", "active");
            PrecursorTransferActive = ptrFactory.Make<bool>("PrecursorFabricatorController", "isTransferActive");

            Pointer<IntPtr> quickSlotsPtr = ptrFactory.Make<IntPtr>("Inventory", "main", "<quickSlots>k__BackingField");
            int off_activeToolName = mono.GetFieldOffset(mono.FindClass("QuickSlots"), "activeToolName");
            ActiveToolName = ptrFactory.MakeString(quickSlotsPtr, off_activeToolName, 0x14);

            Pointer<IntPtr> guiHandPtr = ptrFactory.Make<IntPtr>("Player", "main", "guiHand");
            int off_activeTarget = mono.GetFieldOffset(mono.FindClass("GUIHand"), "activeTarget");
            GuiHandActiveTarget = ptrFactory.Make<IntPtr>(guiHandPtr, off_activeTarget);
            handReticleRawTextHand = ptrFactory.MakeString("HandReticle", "main", "textHand", ptrFactory.StringHeaderSize);

            Pointer<IntPtr> handReticleCompTextHandPtr = ptrFactory.Make<IntPtr>("HandReticle", "main", "compTextHand");
            IntPtr tmpImage = unity.TryFindImageOnce(
                "Unity.TextMeshPro",
                "Unity.TextMeshPro.dll",
                "TMPro",
                "TMPro.dll");
            IntPtr tmpTextKlass = tmpImage != IntPtr.Zero ? unity.TryFindClassOnce("TMPro.TMP_Text", tmpImage) : IntPtr.Zero;
            if (tmpTextKlass != IntPtr.Zero)
            {
                int off_tmpText = unity.ResolveFieldOffsetByNameOrPredicate(
                    tmpTextKlass,
                    new[] { "m_text" },
                    name => string.Equals(name, "m_text", StringComparison.Ordinal));
                if (off_tmpText != 0)
                    handReticleVisibleTextHand = ptrFactory.MakeString(handReticleCompTextHandPtr, off_tmpText, ptrFactory.StringHeaderSize);
            }

            Pointer<IntPtr> playerControllerPtr = ptrFactory.Make<IntPtr>("Player", "main", "<playerController>k__BackingField");
            int off_velocity = mono.GetFieldOffset(mono.FindClass("PlayerController"), "velocity");
            PlayerControllerVelocityX = ptrFactory.Make<float>(playerControllerPtr, off_velocity + 0x0);
            PlayerControllerVelocityY = ptrFactory.Make<float>(playerControllerPtr, off_velocity + 0x4);
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

            IntPtr storyHandTargetKlass = mono.FindClass("StoryHandTarget", mono.MainImage);
            if (storyHandTargetKlass != IntPtr.Zero)
            {
                off_storyHandTargetPrimaryTooltip = mono.GetFieldOffset(storyHandTargetKlass, "primaryTooltip");
                off_storyHandTargetSecondaryTooltip = mono.GetFieldOffset(storyHandTargetKlass, "secondaryTooltip");
                off_storyHandTargetGoal = mono.GetFieldOffset(storyHandTargetKlass, "goal");
            }

            IntPtr storyGoalKlass = mono.FindClass("StoryGoal", mono.MainImage);
            if (storyGoalKlass != IntPtr.Zero)
                off_storyGoalKey = mono.GetFieldOffset(storyGoalKlass, "key");

            Type helperType = mono.GetType();
            while (helperType != null && (helperClassNameMethod == null || helperClassNamespaceMethod == null))
            {
                if (helperClassNameMethod == null)
                    helperClassNameMethod = helperType.GetMethod("ClassName", BindingFlags.Instance | BindingFlags.NonPublic);

                if (helperClassNamespaceMethod == null)
                    helperClassNamespaceMethod = helperType.GetMethod("ClassNamespace", BindingFlags.Instance | BindingFlags.NonPublic);

                helperType = helperType.BaseType;
            }

            pointersInitialized = true;
            logger.Log("Pointers initialized");
        }

        private bool Needs(params SplitName[] required)
        {
            if (settings?.Splits == null || settings.Splits.Count == 0)
                return false;

            var usedSplitNames = new HashSet<SplitName>();
            foreach (var split in settings.Splits)
            {
                usedSplitNames.Add(split.SplitName);
                foreach (var conditionSplit in BelowZeroComponent.GetAllConditions(split))
                    usedSplitNames.Add(conditionSplit.SplitName);
            }

            return required.Any(usedSplitNames.Contains);
        }

        private bool IsInMainMenu()
        {
            if (IsMainMenuPosition(DirectPositionX?.New ?? float.NaN, DirectPositionY?.New ?? float.NaN, DirectPositionZ?.New ?? float.NaN))
                return true;

            if (IsLiveManagedUnityObject(MainMenu.New))
                return true;

            return IsMainMenuPosition(PlayerLastPositionX?.New ?? float.NaN, PlayerLastPositionY?.New ?? float.NaN, PlayerLastPositionZ?.New ?? float.NaN);
        }

        public bool ShouldPause() => false;

        private void UpdateBlueprints()
        {
            List<TechType> currentKnownTech = ReadTechTypeList(knownTechPtr.New);
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

        private List<TechType> ReadTechTypeList(IntPtr listPtr)
        {
            var result = new List<TechType>();
            if (listPtr == IntPtr.Zero)
                return result;

            IntPtr itemsArr = game.Read<IntPtr>(listPtr + off_list_items);
            if (itemsArr == IntPtr.Zero)
                return result;

            int count = game.Read<int>(listPtr + off_list_size);
            if ((uint)count > 100000)
                return result;

            IntPtr basePtr = itemsArr + arr_data_base;
            for (int i = 0; i < count; i++)
            {
                int tech = game.Read<int>(basePtr + i * 4);
                if (tech > 0)
                    result.Add((TechType)tech);
            }

            return result;
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

        public bool HasInventoryChangesThisFrame() => currentInventoryChanges.Count > 0;

        public bool HasHeldMovementStartArmedAfterReset() => heldMovementStartArmedAfterReset;

        public void ClearHeldMovementStartAfterReset() => heldMovementStartArmedAfterReset = false;

        public bool HasMovementStartThisFrame(float velocityThreshold = 0.35f, float inputThreshold = 0.001f)
        {
            if (HasHorizontalVelocityStartThisFrame(velocityThreshold))
                return true;

            if (HasMovementInputStartedThisFrame(inputThreshold))
                return true;

            return false;
        }

        public bool HasHorizontalMovementThisFrame(float threshold = 0.01f)
        {
            if (HasHorizontalMovement(DirectPositionX, DirectPositionZ, threshold))
                return true;

            return HasHorizontalMovement(PlayerLastPositionX, PlayerLastPositionZ, threshold);
        }

        public bool HasActiveHorizontalVelocity(float threshold = 0.35f)
        {
            return HasHorizontalMagnitude(PlayerControllerVelocityX, PlayerControllerVelocityZ, threshold);
        }

        public bool HasBuilderMenuStartThisFrame()
        {
            if (BuilderMenuState == null || !BuilderMenuState.Changed || !BuilderMenuState.New)
                return false;

            return string.Equals(ActiveToolName.New, TechType.Builder.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(ActiveToolName.Old, TechType.Builder.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public bool HasSnowballPickupStartThisFrame()
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

        public bool IsIntroCinematicPlaying() => IsIntroCinematicActive?.New ?? false;

        private bool HasPlayerItem(TechType techType) => PlayerInventory.ContainsKey(techType) || PlayerEquipment.Contains(techType);

        private int GetPlayerItemCount(TechType techType) => PlayerInventory.GetCount(techType) + PlayerEquipment.Count(item => item == techType);

        private int GetPlayerItemCountOld(TechType techType) => PlayerInventoryOld.GetCount(techType) + PlayerEquipmentOld.Count(item => item == techType);

        public bool IsPDAInventoryOpen()
        {
            if (!(PDAIsInUse?.New ?? false))
                return false;

            return (global::LiveSplit.BelowZero.PDATab)PDATab.New == global::LiveSplit.BelowZero.PDATab.Inventory;
        }

        private void UpdateCompletedStoryGoals()
        {
            HashSet<string> currentGoals = ReadCompletedStoryGoals();
            storyGoalsCompletedThisFrame.Clear();
            if (!storyGoalsInitialized)
            {
                completedStoryGoals = currentGoals;
                completedStoryGoalsOld = new HashSet<string>(currentGoals, StringComparer.OrdinalIgnoreCase);
                storyGoalsInitialized = true;
                return;
            }

            completedStoryGoalsOld = completedStoryGoals;
            completedStoryGoals = currentGoals;
            foreach (string goalKey in completedStoryGoals)
            {
                if (completedStoryGoalsOld.Contains(goalKey))
                    continue;

                storyGoalsCompletedThisFrame.Add(goalKey);
                logger.Log($"Story goal completed: {goalKey}");
            }
        }

        private bool HasStoryGoalCompletedThisFrame(string goalKey)
        {
            return !string.IsNullOrEmpty(goalKey)
                && completedStoryGoals.Contains(goalKey)
                && !completedStoryGoalsOld.Contains(goalKey);
        }

        private bool HasTransferStartedThisFrame()
        {
            return PrecursorTransferActive != null
                && PrecursorTransferActive.Changed
                && PrecursorTransferActive.New
                && !PrecursorTransferActive.Old;
        }

        private bool TryConsumeBrace()
        {
            if (HasStoryGoalCompletedThisFrame(GoalEndGameEnterShip))
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

            if (!HasBraceCinematicStartedThisFrame())
                return false;

            braceInteractionWindow.Reset();
            braceReadyForCinematic = false;
            logger.Log("Brace split: endgame cinematic started after EndGameEnterShip.");
            return true;
        }

        private bool TryConsumeAcquireAlAn()
        {
            if (HasAcquireAlAnCinematicStartedThisFrame())
            {
                ClearAcquireAlAnArmedState();
                logger.Log("Acquire Al-An split: sanctuary cinematic started.");
                return true;
            }

            if (HasStoryGoalCompletedThisFrame(GoalSanctuaryCompleted))
            {
                ClearAcquireAlAnArmedState();
                logger.Log("Acquire Al-An split: SanctuaryCompleted story goal.");
                return true;
            }

            if (!acquireAlAnInteractionWindow.IsRunning)
                return false;

            if (acquireAlAnInteractionWindow.ElapsedMilliseconds > maxAcquireAlAnInteractionWindowMs)
            {
                ClearAcquireAlAnArmedState();
                return false;
            }

            if (!string.IsNullOrEmpty(acquireAlAnArmedGoalKey)
                && storyGoalsCompletedThisFrame.Contains(acquireAlAnArmedGoalKey))
            {
                logger.Log($"Acquire Al-An split: armed story goal completed ({acquireAlAnArmedGoalKey}).");
                ClearAcquireAlAnArmedState();
                return true;
            }

            return false;
        }

        private bool IsDeathThisFrame()
        {
            return (Health.New <= 0 && Health.Old > 0) || (IsDying.New && !IsDying.Old);
        }

        private bool IsCurrentBiome(params string[] biomeNames)
        {
            if (biomeNames == null || biomeNames.Length == 0 || string.IsNullOrWhiteSpace(BiomeString.New))
                return false;

            return biomeNames.Any(biomeName =>
                !string.IsNullOrEmpty(biomeName)
                && string.Equals(BiomeString.New, biomeName, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsPlayableBiome(string biomeName)
        {
            return !isInMainMenu
                && !string.IsNullOrWhiteSpace(biomeName)
                && !string.Equals(biomeName, "None", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(biomeName, "MainMenu", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasPlayableBiomeTransitionFrom(string sourceBiome)
        {
            return !string.IsNullOrWhiteSpace(sourceBiome)
                && BiomeString.Changed
                && string.Equals(BiomeString.Old, sourceBiome, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(BiomeString.New, sourceBiome, StringComparison.OrdinalIgnoreCase)
                && IsPlayableBiome(BiomeString.New);
        }

        private bool HasKnownTech(params TechType[] techTypes)
        {
            return techTypes != null && techTypes.Any(techType => techType != TechType.None && KnownTech.Contains(techType));
        }

        private bool HasKnownTechUnlockedThisFrame(params TechType[] techTypes)
        {
            return techTypes != null && techTypes.Any(techType =>
                techType != TechType.None
                && KnownTech.Contains(techType)
                && !KnownTechOld.Contains(techType));
        }

        private bool IsSanctuaryBiome()
        {
            return IsSanctuaryBiome(BiomeString.New);
        }

        private static bool IsSanctuaryBiome(string biomeName)
        {
            return !string.IsNullOrEmpty(biomeName)
                && biomeName.StartsWith(SanctuaryBiome, StringComparison.OrdinalIgnoreCase);
        }

        private void ClearAcquireAlAnArmedState()
        {
            acquireAlAnInteractionWindow.Reset();
            acquireAlAnArmedGoalKey = string.Empty;
            acquireAlAnDestroyBurstWindow.Reset();
        }

        private bool TryConsumeThrownInventoryItem(TechType techType, string displayName)
        {
            bool itemDropped = currentInventoryChanges.TryGetValue(techType, out int delta) && delta < 0;
            if (!itemDropped)
                return false;

            if (IsPDAInventoryOpen())
            {
                logger.Log($"Ignored {displayName} drop while PDA inventory was open.");
                return false;
            }

            bool split = true;
            logger.Log($"{displayName} drop detected outside PDA inventory.");
            if (split && lastThrowSplitTool == techType && throwSplitCooldown.ElapsedMilliseconds <= minThrowSplitCooldownMs)
                split = false;

            if (split)
            {
                lastThrowSplitTool = techType;
                throwSplitCooldown.Restart();
            }

            return split;
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
            startedCraftThisFrame = TechType.None;
            completedCraftFallbackThisFrame = TechType.None;
            completedBuildThisFrame = TechType.None;

            UpdateStartedCraftState();
            UpdateHoverpadCraftState();
            UpdateCraftAnalyticsState();
            UpdatePendingBuilderCompletion();
        }

        private void UpdateStartedCraftState()
        {
            if (CraftedNode != null && CraftedNode.Changed && CraftedNode.New != 0)
            {
                TrySetStartedCraftThisFrame((TechType)CraftedNode.New, "Craft");
            }
        }

        private void UpdateHoverpadCraftState()
        {
            IntPtr hoverpadConstructorPtr = ResolveTrackedHoverpadConstructorPointer();
            if (hoverpadConstructorPtr == IntPtr.Zero || off_hoverpadConstructorConstructing == 0)
                return;

            bool isConstructing = game.Read<bool>(hoverpadConstructorPtr + off_hoverpadConstructorConstructing);
            if (!hoverpadConstructingInitialized)
            {
                hoverpadConstructingOld = isConstructing;
                hoverpadConstructingInitialized = true;
                return;
            }

            if (!hoverpadConstructingOld && isConstructing)
                TrySetStartedCraftThisFrame(TechType.Hoverbike, "Craft");

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

            bool manualSelectionThisFrame = BuilderMenuState != null
                && BuilderMenuState.Changed
                && BuilderMenuState.Old
                && !BuilderMenuState.New;

            if (BuilderPrefab.Changed
                && BuilderPrefab.Old == IntPtr.Zero
                && BuilderPrefab.New != IntPtr.Zero
                && !manualSelectionThisFrame
                && (TechType)BuilderLastTechType.New == pendingBuilderCompletionTechType)
            {
                SetCompletedBuildThisFrame(pendingBuilderCompletionTechType);
                pendingBuilderCompletionTechType = TechType.None;
            }
        }

        private void SetCompletedBuildThisFrame(TechType techType)
        {
            completedBuildThisFrame = techType;
            logger.Log($"Build completed: {techType}");
        }

        private void TrySetStartedCraftThisFrame(TechType techType, string source)
        {
            if (techType == TechType.None)
                return;

            startedCraftThisFrame = techType;
            suppressCraftCompletionFallback.Add(techType);
            logger.Log($"{source} started: {startedCraftThisFrame}");
        }

        private bool TryConsumeStartedCraft(TechType techType)
        {
            if (startedCraftThisFrame != techType)
                return false;

            startedCraftThisFrame = TechType.None;
            return true;
        }

        private bool TryConsumeCompletedCraftFallback(TechType techType)
        {
            if (completedCraftFallbackThisFrame != techType)
                return false;

            completedCraftFallbackThisFrame = TechType.None;
            return true;
        }

        private bool TryConsumeCompletedBuild(TechType techType)
        {
            if (completedBuildThisFrame != techType)
                return false;

            completedBuildThisFrame = TechType.None;
            return true;
        }

        private void UpdateBridgeInteractiveState()
        {
            bridgeInsertThisFrame = false;
            if (!currentInventoryChanges.TryGetValue(TechType.HydraulicFluid, out int hydraulicsFluidDelta)
                || hydraulicsFluidDelta >= 0)
            {
                return;
            }

            if (IsPDAInventoryOpen())
            {
                logger.Log("Ignored Hydraulics Fluid drop while PDA inventory was open.");
                return;
            }

            bridgeInsertThisFrame = true;
            logger.Log("Bridge split: Insert Hydraulics Fluid");
        }

        private bool TryConsumeBridgeInsert()
        {
            if (!bridgeInsertThisFrame)
                return false;

            bridgeInsertThisFrame = false;
            return true;
        }

        private void CaptureTrackedInteractiveTargets()
        {
            IntPtr previousAcquireAlAnTargetPtr = acquireAlAnTargetPtr;
            string previousAcquireAlAnGoalKey = acquireAlAnTargetGoalKey;
            bool previousAcquireAlAnTargetActive = acquireAlAnTargetActive;
            bool trackAcquireAlAn = Needs(SplitName.AcquireAlAnSplit) && IsSanctuaryBiome();
            string acquireAlAnReticleText = trackAcquireAlAn ? GetHandReticleText() : string.Empty;
            bool acquireAlAnReticleActive = trackAcquireAlAn && IsAcquireAlAnReticleText(acquireAlAnReticleText);

            acquireAlAnTargetActive = false;
            acquireAlAnTargetPtr = IntPtr.Zero;
            acquireAlAnTargetGoalKey = string.Empty;

            IntPtr activeTarget = GuiHandActiveTarget?.New ?? IntPtr.Zero;
            if (activeTarget != IntPtr.Zero
                && TryGetManagedObjectTypeName(activeTarget, out string typeName)
                && !string.IsNullOrEmpty(typeName))
            {
                if (string.Equals(typeName, "StoryHandTarget", StringComparison.OrdinalIgnoreCase)
                    && TryReadStoryHandTargetInfo(activeTarget, out string primaryTooltip, out string secondaryTooltip, out string goalKey))
                {
                    if (IsSanctuaryBiome() && IsAcquireAlAnStoryTarget(primaryTooltip, secondaryTooltip))
                    {
                        acquireAlAnTargetActive = true;
                        acquireAlAnTargetPtr = activeTarget;
                        acquireAlAnTargetGoalKey = goalKey ?? string.Empty;
                    }
                }

                switch (typeName)
                {
                    case "HoverpadConstructor":
                        trackedHoverpadConstructorPtr = activeTarget;
                        break;

                    case "GUI_HoverpadTerminal":
                        trackedHoverpadConstructorPtr = ResolveHoverpadConstructorFromTerminal(activeTarget);
                        break;

                    case "Hoverpad":
                        trackedHoverpadConstructorPtr = ResolveHoverpadConstructorFromHoverpad(activeTarget);
                        break;
                }

                if (trackAcquireAlAn && acquireAlAnReticleActive)
                {
                    if (!acquireAlAnInteractionWindow.IsRunning)
                        logger.Log("Acquire Al-An watch armed from sanctuary terminal.");

                    acquireAlAnInteractionWindow.Restart();
                    acquireAlAnTargetActive = true;
                    acquireAlAnTargetPtr = activeTarget;
                }
            }

            if (previousAcquireAlAnTargetActive
                && (!acquireAlAnTargetActive || previousAcquireAlAnTargetPtr != acquireAlAnTargetPtr))
            {
                ArmAcquireAlAnFromTargetLoss(previousAcquireAlAnTargetPtr, previousAcquireAlAnGoalKey);
            }
        }

        private IntPtr ResolveTrackedHoverpadConstructorPointer()
        {
            if (trackedHoverpadConstructorPtr != IntPtr.Zero && IsLiveManagedUnityObject(trackedHoverpadConstructorPtr))
                return trackedHoverpadConstructorPtr;

            trackedHoverpadConstructorPtr = IntPtr.Zero;
            return IntPtr.Zero;
        }

        private IntPtr ResolveHoverpadConstructorFromTerminal(IntPtr terminalPtr)
        {
            IntPtr hoverpadPtr = ReadObjectFieldPointer(terminalPtr, off_hoverpadTerminalHoverpad);
            return ResolveHoverpadConstructorFromHoverpad(hoverpadPtr);
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

        private bool TryReadStoryHandTargetInfo(IntPtr storyHandTargetPtr, out string primaryTooltip, out string secondaryTooltip, out string goalKey)
        {
            primaryTooltip = string.Empty;
            secondaryTooltip = string.Empty;
            goalKey = string.Empty;

            if (storyHandTargetPtr == IntPtr.Zero)
                return false;

            try
            {
                if (off_storyHandTargetPrimaryTooltip != 0)
                    primaryTooltip = ReadManagedStringObject(ReadObjectFieldPointer(storyHandTargetPtr, off_storyHandTargetPrimaryTooltip));

                if (off_storyHandTargetSecondaryTooltip != 0)
                    secondaryTooltip = ReadManagedStringObject(ReadObjectFieldPointer(storyHandTargetPtr, off_storyHandTargetSecondaryTooltip));

                IntPtr goalPtr = ReadObjectFieldPointer(storyHandTargetPtr, off_storyHandTargetGoal);
                if (goalPtr != IntPtr.Zero && off_storyGoalKey != 0)
                    goalKey = ReadManagedStringObject(ReadObjectFieldPointer(goalPtr, off_storyGoalKey));

                return true;
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to read StoryHandTarget info: {ex.Message}");
                return false;
            }
        }

        private string ReadManagedStringObject(IntPtr stringPtr)
        {
            if (stringPtr == IntPtr.Zero)
                return string.Empty;

            try
            {
                int stringHeader = game.PointerSize * 2 + 0x4;
                return game.ReadString(stringPtr + stringHeader, EStringType.UTF16Sized) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool IsAcquireAlAnStoryTarget(string primaryTooltip, string secondaryTooltip)
        {
            return string.Equals(primaryTooltip, AcquireAlAnTooltipKey, StringComparison.OrdinalIgnoreCase)
                || string.Equals(secondaryTooltip, AcquireAlAnTooltipKey, StringComparison.OrdinalIgnoreCase);
        }

        private string GetHandReticleText()
        {
            string visibleText = handReticleVisibleTextHand?.New;
            if (!string.IsNullOrWhiteSpace(visibleText))
                return visibleText.Trim();

            string rawText = handReticleRawTextHand?.New;
            return string.IsNullOrWhiteSpace(rawText) ? string.Empty : rawText.Trim();
        }

        private static bool IsAcquireAlAnReticleText(string text)
        {
            return !string.IsNullOrWhiteSpace(text)
                && (text.StartsWith(AcquireAlAnReticleText, StringComparison.OrdinalIgnoreCase)
                    || text.StartsWith(AcquireAlAnTooltipKey, StringComparison.OrdinalIgnoreCase));
        }

        private void ArmAcquireAlAnFromTargetLoss(IntPtr previousTargetPtr, string previousGoalKey)
        {
            bool targetDestroyed = previousTargetPtr != IntPtr.Zero && !IsLiveManagedUnityObject(previousTargetPtr);
            if (!targetDestroyed && string.IsNullOrEmpty(previousGoalKey))
                return;

            acquireAlAnArmedGoalKey = previousGoalKey ?? string.Empty;
            acquireAlAnInteractionWindow.Restart();

            if (string.IsNullOrEmpty(acquireAlAnArmedGoalKey))
                logger.Log("Acquire Al-An target lost; interaction window armed.");
            else
                logger.Log($"Acquire Al-An target lost; armed goal {acquireAlAnArmedGoalKey}.");

            if (targetDestroyed)
            {
                if (!acquireAlAnDestroyBurstWindow.IsRunning
                    || acquireAlAnDestroyBurstWindow.ElapsedMilliseconds > acquireAlAnDestroyBurstWindowMs)
                    acquireAlAnDestroyBurstWindow.Restart();
                

                logger.Log("Acquire Al-An target object was destroyed.");
            }
        }

        private bool HasAcquireAlAnCinematicStartedThisFrame()
        {
            if (!acquireAlAnInteractionWindow.IsRunning)
                return false;

            if (acquireAlAnInteractionWindow.ElapsedMilliseconds > maxAcquireAlAnInteractionWindowMs)
                return false;

            if (isInMainMenu || wasInMainMenu || (IsLoadingScreenShowing?.New ?? false))
                return false;

            if (IsAnimationPlaying != null && IsAnimationPlaying.Changed)
            {
                if (IsAnimationPlaying.New && !IsAnimationPlaying.Old)
                    return true;
            }

            if (PlayerControllerInputEnabled != null && PlayerControllerInputEnabled.Changed)
            {
                if (!PlayerControllerInputEnabled.New
                    && PlayerControllerInputEnabled.Old
                    && (IsAnimationPlaying?.New ?? false))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasBraceCinematicStartedThisFrame()
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
            if (objectPtr == IntPtr.Zero || helperClassNameMethod == null)
                return false;

            IntPtr vtablePtr = game.Read<IntPtr>(objectPtr);
            if (vtablePtr == IntPtr.Zero)
                return false;

            IntPtr klassPtr = game.Read<IntPtr>(vtablePtr);
            if (klassPtr == IntPtr.Zero)
                return false;

            string className = helperClassNameMethod.Invoke(mono, new object[] { klassPtr }) as string;
            if (string.IsNullOrEmpty(className))
                return false;

            string classNamespace = helperClassNamespaceMethod?.Invoke(mono, new object[] { klassPtr }) as string;
            typeName = string.IsNullOrEmpty(classNamespace) ? className : $"{classNamespace}.{className}";
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

                if (completedCraftFallbackThisFrame == TechType.None)
                {
                    completedCraftFallbackThisFrame = pair.Key;
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
            heldMovementStartArmedAfterReset = armHeldMovementStartAfterReset;
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
            startedCraftThisFrame = TechType.None;
            completedCraftFallbackThisFrame = TechType.None;
            completedBuildThisFrame = TechType.None;
            craftAnalyticsInitialized = false;
            craftCounts.Clear();
            suppressCraftCompletionFallback.Clear();
            storyGoalsInitialized = false;
            completedStoryGoals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            completedStoryGoalsOld = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            storyGoalsCompletedThisFrame = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            storyGoalReaderMode = string.Empty;
            pendingBuilderCompletionTechType = TechType.None;
            builderMenuSelectionPending = false;
            builderMenuSelectionWindow.Reset();
            bridgeInsertThisFrame = false;
            acquireAlAnInteractionWindow.Reset();
            acquireAlAnDestroyBurstWindow.Reset();
            braceInteractionWindow.Reset();
            braceReadyForCinematic = false;
            acquireAlAnTargetActive = false;
            acquireAlAnTargetGoalKey = string.Empty;
            acquireAlAnArmedGoalKey = string.Empty;
            acquireAlAnTargetPtr = IntPtr.Zero;
            trackedHoverpadConstructorPtr = IntPtr.Zero;
            hoverpadConstructingInitialized = false;
            hoverpadConstructingOld = false;
        }

        private void ForceRefreshVersionSpecificSignalPointers()
        {
            nextDirectSignalRetryUtc = DateTime.UtcNow.AddSeconds(1);
            InitializeVersionSpecificSignalPointers();
        }

        private void RefreshVersionSpecificSignalPointersIfNeeded()
        {
            if (DateTime.UtcNow < nextDirectSignalRetryUtc)
                return;

            if (directPositionPointersBound)
                return;

            ForceRefreshVersionSpecificSignalPointers();
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
                        directPositionPointersBound = DirectPositionX != null && DirectPositionY != null && DirectPositionZ != null;
                        if (directPositionPointersBound)
                            logger.Log("Using Aug2021 direct position pointers.");
                        break;

                    case GameVersion.Oct2025:
                        DirectPositionX = MakeModulePointer<float>("fmodstudio.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x48C)
                            ?? MakeModulePointer<float>("fmodstudiol.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x48C);
                        DirectPositionY = MakeModulePointer<float>("fmodstudio.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x490)
                            ?? MakeModulePointer<float>("fmodstudiol.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x490);
                        DirectPositionZ = MakeModulePointer<float>("fmodstudio.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x494)
                            ?? MakeModulePointer<float>("fmodstudiol.dll", 0x2CED70, 0xE0, 0x8, 0x20, 0x494);
                        directPositionPointersBound = DirectPositionX != null && DirectPositionY != null && DirectPositionZ != null;
                        if (directPositionPointersBound)
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

        private static bool HasHorizontalMovement(Pointer<float> x, Pointer<float> z, float threshold)
        {
            if (x == null || z == null)
                return false;

            return Math.Abs(x.New - x.Old) > threshold || Math.Abs(z.New - z.Old) > threshold;
        }

        private bool HasHorizontalVelocityStartThisFrame(float threshold)
        {
            return BecameHorizontallyActiveThisFrame(PlayerControllerVelocityX, PlayerControllerVelocityZ, threshold);
        }

        private bool HasMovementInputStartedThisFrame(float threshold)
        {
            return BecameActiveThisFrame(MoveDirectionX, threshold) || BecameActiveThisFrame(MoveDirectionZ, threshold);
        }

        public bool HasActiveMovementInput(float threshold = 0.001f)
        {
            return IsActive(MoveDirectionX, threshold) || IsActive(MoveDirectionZ, threshold);
        }

        private static bool BecameActiveThisFrame(Pointer<float> axis, float threshold)
        {
            if (axis == null)
                return false;

            return Math.Abs(axis.New) > threshold && Math.Abs(axis.Old) <= threshold;
        }

        private static bool HasHorizontalMagnitude(Pointer<float> x, Pointer<float> z, float threshold)
        {
            if (x == null || z == null)
                return false;

            return Math.Sqrt(x.New * x.New + z.New * z.New) > threshold;
        }

        private static bool BecameHorizontallyActiveThisFrame(Pointer<float> x, Pointer<float> z, float threshold)
        {
            if (x == null || z == null)
                return false;

            double oldMagnitude = Math.Sqrt(x.Old * x.Old + z.Old * z.Old);
            double newMagnitude = Math.Sqrt(x.New * x.New + z.New * z.New);
            return oldMagnitude <= threshold && newMagnitude > threshold;
        }

        private static bool IsActive(Pointer<float> axis, float threshold)
        {
            if (axis == null)
                return false;

            return Math.Abs(axis.New) > threshold;
        }

        private void ResolveUnityObjectCachedPtrOffset(UnityHelperTask.UnityHelperBase unity)
        {
            hasUnityObjectCachedPtrOffset = true;
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

            if (!hasUnityObjectCachedPtrOffset)
                return true;

            return game.Read<IntPtr>(rawObject + unityObjectCachedPtrOffset) != IntPtr.Zero;
        }

        private static bool IsMainMenuPosition(float x, float y, float z)
        {
            if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z))
                return false;

            return Math.Abs(x - mainMenuPosX) <= mainMenuPositionTolerance
                && Math.Abs(y - mainMenuPosY) <= mainMenuPositionTolerance
                && Math.Abs(z - mainMenuPosZ) <= mainMenuPositionTolerance;
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

        private HashSet<string> ReadCompletedStoryGoals()
        {
            IntPtr hashSetPtr = completedStoryGoalsPtr?.New ?? IntPtr.Zero;
            if (hashSetPtr == IntPtr.Zero || hashset_off_slots == 0)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int stringHeader = game.PointerSize * 2 + 0x4;
            var candidates = new List<(HashSet<string> Goals, string Mode, int Score)>();

            foreach (int slotsOffset in new[] { hashset_off_slots, 0x18, 0x20, 0x10 }.Distinct())
            {
                if (TryReadStoryGoalsFromSlotArray(hashSetPtr, slotsOffset, stringHeader, out HashSet<string> slotGoals))
                    candidates.Add((slotGoals, $"slot-array@0x{slotsOffset:X}", ScoreGoalSet(slotGoals)));

                if (TryReadStoryGoalsFromStringArray(hashSetPtr, slotsOffset, stringHeader, out HashSet<string> stringGoals))
                    candidates.Add((stringGoals, $"string-array@0x{slotsOffset:X}", ScoreGoalSet(stringGoals)));
            }

            if (candidates.Count == 0)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var best = candidates
                .OrderByDescending(candidate => candidate.Score)
                .ThenByDescending(candidate => candidate.Goals.Count)
                .First();

            if (!string.Equals(storyGoalReaderMode, best.Mode, StringComparison.Ordinal))
            {
                storyGoalReaderMode = best.Mode;
                logger.Log($"Story goal reader selected {storyGoalReaderMode} ({best.Goals.Count} goals).");
            }

            return best.Goals;
        }

        private bool TryReadStoryGoalsFromSlotArray(IntPtr hashSetPtr, int slotsOffset, int stringHeader, out HashSet<string> result)
        {
            result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IntPtr slotsArr = game.Read<IntPtr>(hashSetPtr + slotsOffset);
            if (slotsArr == IntPtr.Zero)
                return false;

            int len = game.Read<int>(slotsArr + arr_off_len);
            if (len <= 0 || len > 200000)
                return false;

            int stride = game.PointerSize == 8 ? 0x10 : 0x0C;
            IntPtr basePtr = slotsArr + arr_data_base;
            for (int i = 0; i < len; i++)
            {
                IntPtr entry = basePtr + i * stride;
                int hashCode = game.Read<int>(entry + 0x00);
                if (hashCode < 0)
                    continue;

                IntPtr valuePtr = game.Read<IntPtr>(entry + 0x08);
                if (TryReadGoalKey(valuePtr, stringHeader, out string goalKey))
                    result.Add(goalKey);
            }

            return result.Count > 0;
        }

        private bool TryReadStoryGoalsFromStringArray(IntPtr hashSetPtr, int slotsOffset, int stringHeader, out HashSet<string> result)
        {
            result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IntPtr slotsArr = game.Read<IntPtr>(hashSetPtr + slotsOffset);
            if (slotsArr == IntPtr.Zero)
                return false;

            int len = game.Read<int>(slotsArr + arr_off_len);
            if (len <= 0 || len > 200000)
                return false;

            IntPtr basePtr = slotsArr + arr_data_base;
            for (int i = 0; i < len; i++)
            {
                IntPtr valuePtr = game.Read<IntPtr>(basePtr + i * game.PointerSize);
                if (TryReadGoalKey(valuePtr, stringHeader, out string goalKey))
                    result.Add(goalKey);
            }

            return result.Count > 0;
        }

        private bool TryReadGoalKey(IntPtr valuePtr, int stringHeader, out string goalKey)
        {
            goalKey = null;
            if (valuePtr == IntPtr.Zero)
                return false;

            try
            {
                string value = game.ReadString(valuePtr + stringHeader, EStringType.UTF16Sized);
                if (!IsLikelyStoryGoalKey(value))
                    return false;

                goalKey = value;
                return true;
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

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (!(char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '-'))
                    return false;
            }

            return true;
        }

        private static int ScoreGoalSet(HashSet<string> goals)
        {
            int score = 0;
            foreach (string goal in goals)
            {
                score += 1;

                if (string.Equals(goal, GoalSanctuaryCompleted, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(goal, GoalEndGameEnterShip, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(goal, GoalAlAnTransferred, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(goal, GoalBodyBuilt, StringComparison.OrdinalIgnoreCase))
                {
                    score += 100;
                }
                else if (goal.StartsWith("On", StringComparison.OrdinalIgnoreCase)
                    || goal.StartsWith("Log_", StringComparison.OrdinalIgnoreCase)
                    || goal.StartsWith("Call_", StringComparison.OrdinalIgnoreCase)
                    || goal.StartsWith("Scan_", StringComparison.OrdinalIgnoreCase))
                {
                    score += 3;
                }
            }

            return score;
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
