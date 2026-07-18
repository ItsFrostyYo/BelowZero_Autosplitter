using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveSplit.BelowZero
{
    public static class BelowZeroVersionedData
    {
        public const GameVersion BaselineVersion = GameVersion.Oct2025;
        public static GameVersion ActiveVersion { get; private set; } = BaselineVersion;

        private static readonly StringComparer AlphaComparer = StringComparer.OrdinalIgnoreCase;

        private static readonly HashSet<Unlockable> Aug2021MissingBlueprints = new HashSet<Unlockable>
        {
            Unlockable.BaseMoonpoolExpansion,
        };

        private static readonly HashSet<EncyclopediaEntry> Aug2021MissingEncyclopediaEntries = new HashSet<EncyclopediaEntry>
        {
            EncyclopediaEntry.MoonpoolExpansion,
        };

        private static readonly HashSet<Craftable> Aug2021MissingCraftables = new HashSet<Craftable>
        {
            Craftable.BaseMoonpoolExpansion,
        };

        private static readonly HashSet<Craftable> PrefabOnlyCraftables = new HashSet<Craftable>
        {
            Craftable.PrecursorNPCBody,
        };

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

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<InventoryItem>>>> Items =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<InventoryItem>>>>(() => BuildVersionMap<InventoryItem>(1, IsAvailable));

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<InventoryItem>>>> ItemsAlpha =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<InventoryItem>>>>(() => BuildAlphaMap(Items.Value));

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Unlockable>>>> Blueprints =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Unlockable>>>>(() => BuildVersionMap<Unlockable>(1, IsAvailable));

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Unlockable>>>> BlueprintsAlpha =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Unlockable>>>>(() => BuildAlphaMap(Blueprints.Value));

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<EncyclopediaEntry>>>> EncyclopediaEntries =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<EncyclopediaEntry>>>>(() => BuildVersionMap<EncyclopediaEntry>(1, IsAvailable));

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<EncyclopediaEntry>>>> EncyclopediaEntriesAlpha =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<EncyclopediaEntry>>>>(() => BuildAlphaMap(EncyclopediaEntries.Value));

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Biome>>>> Biomes =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Biome>>>>(() => BuildVersionMap<Biome>(1, IsAvailable));

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Biome>>>> BiomesAlpha =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Biome>>>>(() => BuildAlphaMap(Biomes.Value));

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Craftable>>>> Craftables =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Craftable>>>>(() => BuildVersionMap<Craftable>(1, IsCraftable));

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Craftable>>>> CraftablesAlpha =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Craftable>>>>(() => BuildAlphaMap(Craftables.Value));

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Craftable>>>> Buildables =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Craftable>>>>(() => BuildVersionMap<Craftable>(1, IsBuildable));

        private static readonly Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Craftable>>>> BuildablesAlpha =
            new Lazy<IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<Craftable>>>>(() => BuildAlphaMap(Buildables.Value));

        public static IReadOnlyList<ComboItem<InventoryItem>> GetItems(GameVersion version, bool alpha) =>
            GetVersionedValue(alpha ? ItemsAlpha.Value : Items.Value, version);

        public static IReadOnlyList<ComboItem<Unlockable>> GetBlueprints(GameVersion version, bool alpha) =>
            GetVersionedValue(alpha ? BlueprintsAlpha.Value : Blueprints.Value, version);

        public static IReadOnlyList<ComboItem<EncyclopediaEntry>> GetEncyclopediaEntries(GameVersion version, bool alpha) =>
            GetVersionedValue(alpha ? EncyclopediaEntriesAlpha.Value : EncyclopediaEntries.Value, version);

        public static IReadOnlyList<ComboItem<Biome>> GetBiomes(GameVersion version, bool alpha) =>
            GetVersionedValue(alpha ? BiomesAlpha.Value : Biomes.Value, version);

        public static IReadOnlyList<ComboItem<Craftable>> GetCraftables(GameVersion version, bool alpha) =>
            GetVersionedValue(alpha ? CraftablesAlpha.Value : Craftables.Value, version);

        public static IReadOnlyList<ComboItem<Craftable>> GetBuildables(GameVersion version, bool alpha) =>
            GetVersionedValue(alpha ? BuildablesAlpha.Value : Buildables.Value, version);

        public static void SetActiveVersion(GameVersion version)
        {
            ActiveVersion = Enum.IsDefined(typeof(GameVersion), version)
                ? version
                : BaselineVersion;
        }

        public static bool IsAvailable(GameVersion version, InventoryItem item) => true;

        public static bool IsAvailable(GameVersion version, Unlockable blueprint)
        {
            if (version == GameVersion.Aug2021 && Aug2021MissingBlueprints.Contains(blueprint))
                return false;

            return true;
        }

        public static bool IsAvailable(GameVersion version, EncyclopediaEntry entry)
        {
            if (version == GameVersion.Aug2021 && Aug2021MissingEncyclopediaEntries.Contains(entry))
                return false;

            return true;
        }

        public static bool IsAvailable(GameVersion version, Biome biome) => true;

        public static bool IsAvailable(GameVersion version, Craftable craftable)
        {
            if (version == GameVersion.Aug2021 && Aug2021MissingCraftables.Contains(craftable))
                return false;

            return true;
        }

        public static bool IsCraftable(GameVersion version, Craftable craftable)
        {
            if (!IsAvailable(version, craftable))
                return false;

            return !BuilderBuildables.Contains(craftable) && !PrefabOnlyCraftables.Contains(craftable);
        }

        public static bool IsBuildable(GameVersion version, Craftable craftable)
        {
            if (!IsAvailable(version, craftable))
                return false;

            return BuilderBuildables.Contains(craftable);
        }

        private static IReadOnlyList<ComboItem<TEnum>> GetVersionedValue<TEnum>(
            IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<TEnum>>> values,
            GameVersion version)
        {
            if (values.TryGetValue(version, out var result))
                return result;

            return values[BaselineVersion];
        }

        private static IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<TEnum>>> BuildVersionMap<TEnum>(
            int skip,
            Func<GameVersion, TEnum, bool> isAvailable) where TEnum : struct
        {
            var values = Enum.GetValues(typeof(GameVersion))
                .Cast<GameVersion>()
                .Distinct()
                .ToDictionary(version => version, version => (IReadOnlyList<ComboItem<TEnum>>)BuildEnumList(version, skip, isAvailable));

            return values;
        }

        private static IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<TEnum>>> BuildAlphaMap<TEnum>(
            IReadOnlyDictionary<GameVersion, IReadOnlyList<ComboItem<TEnum>>> source)
        {
            return source.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<ComboItem<TEnum>>)pair.Value
                    .OrderBy(item => item.Display ?? string.Empty, AlphaComparer)
                    .ToList());
        }

        private static List<ComboItem<TEnum>> BuildEnumList<TEnum>(
            GameVersion version,
            int skip,
            Func<GameVersion, TEnum, bool> isAvailable) where TEnum : struct
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Skip(skip)
                .Where(value => isAvailable(version, value))
                .Select(value => new ComboItem<TEnum>
                {
                    Value = value,
                    Display = typeof(TEnum) == typeof(Biome)
                        ? value.ToString()
                        : Localization.GetDisplayName(value),
                })
                .ToList();
        }
    }
}
