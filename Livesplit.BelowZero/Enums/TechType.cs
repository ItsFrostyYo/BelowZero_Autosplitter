namespace LiveSplit.BelowZero
{
    using System;

    public enum TechType
    {
    	None = 0,
    	Quartz = 1,
    	ScrapMetal = 2,
    	FiberMesh = 3,
    	LimestoneChunk = 4,
    	CalciteOld = 5,
    	DolomiteOld = 6,
    	Copper = 7,
    	Lead = 8,
    	Salt = 9,
    	FlintOld = 10,
    	EmeryOld = 11,
    	[Obsolete]
    	MercuryOre = 12,
    	CalciumChunk = 13,
    	Placeholder = 14,
    	Glass = 15,
    	Titanium = 16,
    	Silicone = 17,
    	CarbonOld = 18,
    	EthanolOld = 19,
    	EthyleneOld = 20,
    	Gold = 21,
    	[Obsolete]
    	Magnesium = 22,
    	Sulphur = 23,
    	HydrogenOld = 24,
    	Lodestone = 25,
    	[Obsolete]
    	SandLoot = 26,
    	[Obsolete]
    	Bleach = 27,
    	Silver = 28,
    	BatteryAcidOld = 29,
    	TitaniumIngot = 30,
    	[Obsolete]
    	SandstoneChunk = 31,
    	CopperWire = 32,
    	WiringKit = 33,
    	AdvancedWiringKit = 34,
    	CrashPowder = 35,
    	Diamond = 36,
    	[Obsolete]
    	BasaltChunk = 37,
    	[Obsolete]
    	ShaleChunk = 38,
    	ObsidianChunk = 39,
    	Lithium = 40,
    	PlasteelIngot = 41,
    	EnameledGlass = 42,
    	PowerCell = 43,
    	ComputerChip = 44,
    	Fiber = 45,
    	Enamel = 46,
    	AcidOld = 47,
    	VesselOld = 48,
    	CombustibleOld = 49,
    	OpalGem = 50,
    	[Obsolete]
    	Uranium = 51,
    	AluminumOxide = 52,
    	HydrochloricAcid = 53,
    	Magnetite = 54,
    	AminoAcids = 55,
    	Polyaniline = 56,
    	AramidFibers = 57,
    	Graphene = 58,
    	Aerogel = 59,
    	Nanowires = 60,
    	Benzene = 61,
    	Lubricant = 62,
    	UraniniteCrystal = 63,
    	ReactorRod = 64,
    	DepletedReactorRod = 65,
    	PrecursorIonCrystal = 66,
    	[Obsolete]
    	PrecursorIonCrystalMatrix = 67,
    	Kyanite = 68,
    	Nickel = 69,
    	DrillableSalt = 70,
    	DrillableQuartz = 71,
    	DrillableCopper = 72,
    	DrillableTitanium = 73,
    	DrillableLead = 74,
    	DrillableSilver = 75,
    	DrillableDiamond = 76,
    	DrillableGold = 77,
    	DrillableMagnetite = 78,
    	DrillableLithium = 79,
    	DrillableMercury = 80,
    	DrillableUranium = 81,
    	DrillableAluminiumOxide = 82,
    	DrillableNickel = 83,
    	DrillableSulphur = 84,
    	DrillableKyanite = 85,
    	BreakableLead = 100,
    	BreakableSilver = 101,
    	BreakableGold = 102,
    	DiveSuit = 500,
    	[Obsolete]
    	ShipComputerOld = 501,
    	Fins = 502,
    	Tank = 503,
    	Battery = 504,
    	Knife = 505,
    	Drill = 506,
    	Flashlight = 507,
    	Beacon = 508,
    	Builder = 509,
    	PDA = 510,
    	EscapePod = 511,
    	Compass = 512,
    	AirBladder = 513,
    	[Obsolete]
    	Terraformer = 514,
    	Pipe = 515,
    	[Obsolete]
    	Thermometer = 516,
    	DiveReel = 517,
    	Rebreather = 518,
    	[Obsolete]
    	RadiationSuit = 519,
    	[Obsolete]
    	RadiationHelmet = 520,
    	[Obsolete]
    	RadiationGloves = 521,
    	ReinforcedDiveSuit = 522,
    	Scanner = 523,
    	[Obsolete]
    	FireExtinguisher = 524,
    	MapRoomHUDChip = 525,
    	PipeSurfaceFloater = 526,
    	[Obsolete]
    	CyclopsDecoy = 527,
    	DoubleTank = 528,
    	ReinforcedGloves = 529,
    	Thumper = 530,
    	TeleportationTool = 531,
    	MetalDetector = 532,
    	FlashlightHelmet = 533,
    	Welder = 750,
    	Seaglide = 751,
    	Constructor = 752,
    	[Obsolete]
    	Transfuser = 753,
    	Flare = 754,
    	[Obsolete]
    	StasisRifle = 755,
    	BuildBot = 756,
    	PropulsionCannon = 757,
    	Gravsphere = 758,
    	SmallStorage = 759,
    	StasisSphere = 760,
    	LaserCutter = 761,
    	LEDLight = 762,
    	[Obsolete]
    	HoverBoard = 763,
    	[Obsolete]
    	DeployableDrill = 764,
    	[Obsolete]
    	DiamondBlade = 800,
    	HeatBlade = 801,
    	[Obsolete]
    	LithiumIonBattery = 802,
    	PlasteelTank = 803,
    	HighCapacityTank = 804,
    	UltraGlideFins = 805,
    	SwimChargeFins = 806,
    	[Obsolete]
    	RepulsionCannon = 807,
    	WaterFiltrationSuit = 808,
    	[Obsolete]
    	PowerGlide = 809,
    	ColdSuit = 810,
    	ColdSuitGloves = 811,
    	ColdSuitHelmet = 812,
    	SuitBoosterTank = 813,
    	[Obsolete]
    	CompostCreepvine = 900,
    	[Obsolete]
    	ProcessUranium = 901,
    	PrecursorIonEnergyBlueprint = 950,
    	FabricatorBlueprintOld = 1000,
    	ConstructorBlueprint = 1001,
    	CyclopsBlueprint = 1002,
    	FragmentAnalyzerBlueprintOld = 1003,
    	LockerBlueprint = 1004,
    	SpecialHullPlateBlueprintOld = 1005,
    	BikemanHullPlateBlueprintOld = 1006,
    	EatMyDictionHullPlateBlueprintOld = 1007,
    	DevTestItemBlueprintOld = 1008,
    	SeamothBlueprint = 1009,
    	StasisRifleBlueprint = 1010,
    	ExosuitBlueprint = 1011,
    	TransfuserBlueprint = 1012,
    	TerraformerBlueprint = 1013,
    	ReinforceHullBlueprint = 1014,
    	WorkbenchBlueprint = 1015,
    	PropulsionCannonBlueprint = 1016,
    	SpecimenAnalyzerBlueprint = 1017,
    	BioreactorBlueprint = 1018,
    	ThermalPlantBlueprint = 1019,
    	NuclearReactorBlueprint = 1020,
    	MoonpoolBlueprint = 1021,
    	FiltrationMachineBlueprint = 1022,
    	TechlightBlueprint = 1023,
    	LEDLightBlueprint = 1024,
    	CyclopsHullBlueprint = 1025,
    	CyclopsBridgeBlueprint = 1026,
    	CyclopsEngineBlueprint = 1027,
    	CyclopsDockingBayBlueprint = 1028,
    	SpotlightBlueprint = 1029,
    	RadioBlueprint = 1030,
    	StarshipCargoCrateBlueprint = 1031,
    	StarshipCircuitBoxBlueprint = 1032,
    	StarshipDeskBlueprint = 1033,
    	StarshipChairBlueprint = 1034,
    	StarshipMonitorBlueprint = 1035,
    	SolarPanelBlueprint = 1036,
    	PowerTransmitterBlueprint = 1037,
    	BaseUpgradeConsoleBlueprint = 1038,
    	BaseObservatoryBlueprint = 1039,
    	BaseWaterParkBlueprint = 1040,
    	PictureFrameBlueprint = 1041,
    	BaseRoomBlueprint = 1042,
    	BaseBulkheadBlueprint = 1043,
    	SeaglideBlueprint = 1044,
    	BatteryChargerBlueprint = 1045,
    	PowerCellChargerBlueprint = 1046,
    	FarmingTrayBlueprint = 1047,
    	SignBlueprint = 1048,
    	BenchBlueprint = 1049,
    	PlanterPotBlueprint = 1050,
    	PlanterBoxBlueprint = 1051,
    	PlanterShelfBlueprint = 1052,
    	AquariumBlueprint = 1053,
    	ReinforcedDiveSuitBlueprint = 1054,
    	RadiationSuitBlueprint = 1055,
    	StillsuitBlueprint = 1056,
    	ScannerRoomBlueprint = 1057,
    	BasePlanterBlueprint = 1058,
    	PlanterPot2Blueprint = 1059,
    	PlanterPot3Blueprint = 1060,
    	MedicalCabinetBlueprint = 1061,
    	BaseMapRoomBlueprint = 1062,
    	[Obsolete]
    	SeamothFragment = 1100,
    	[Obsolete]
    	StasisRifleFragment = 1101,
    	ExosuitFragment = 1102,
    	[Obsolete]
    	TransfuserFragment = 1103,
    	TerraformerFragment = 1104,
    	[Obsolete]
    	ReinforceHullFragment = 1105,
    	WorkbenchFragment = 1106,
    	PropulsionCannonFragment = 1107,
    	BioreactorFragment = 1108,
    	ThermalPlantFragment = 1109,
    	NuclearReactorFragment = 1110,
    	[Obsolete]
    	MoonpoolFragment = 1111,
    	[Obsolete]
    	BaseFiltrationMachineFragment = 1112,
    	[Obsolete]
    	CyclopsHullFragment = 1113,
    	[Obsolete]
    	CyclopsBridgeFragment = 1114,
    	[Obsolete]
    	CyclopsEngineFragment = 1115,
    	[Obsolete]
    	CyclopsDockingBayFragment = 1116,
    	SeaglideFragment = 1117,
    	ConstructorFragment = 1118,
    	[Obsolete]
    	SolarPanelFragment = 1119,
    	[Obsolete]
    	PowerTransmitterFragment = 1120,
    	[Obsolete]
    	BaseUpgradeConsoleFragment = 1121,
    	[Obsolete]
    	BaseObservatoryFragment = 1122,
    	[Obsolete]
    	BaseWaterParkFragment = 1123,
    	[Obsolete]
    	RadioFragment = 1124,
    	[Obsolete]
    	BaseRoomFragment = 1125,
    	[Obsolete]
    	BaseBulkheadFragment = 1126,
    	[Obsolete]
    	BatteryChargerFragment = 1127,
    	PowerCellChargerFragment = 1128,
    	ScannerRoomFragment = 1129,
    	[Obsolete]
    	SpecimenAnalyzerFragment = 1130,
    	[Obsolete]
    	FarmingTrayFragment = 1131,
    	[Obsolete]
    	SignFragment = 1132,
    	[Obsolete]
    	PictureFrameFragment = 1133,
    	[Obsolete]
    	BenchFragment = 1134,
    	[Obsolete]
    	PlanterPotFragment = 1135,
    	[Obsolete]
    	PlanterBoxFragment = 1136,
    	[Obsolete]
    	PlanterShelfFragment = 1137,
    	[Obsolete]
    	AquariumFragment = 1138,
    	ReinforcedDiveSuitFragment = 1139,
    	RadiationSuitFragment = 1140,
    	StillsuitFragment = 1141,
    	BuilderFragment = 1142,
    	LEDLightFragment = 1143,
    	[Obsolete]
    	TechlightFragment = 1144,
    	SpotlightFragment = 1145,
    	[Obsolete]
    	BaseMapRoomFragment = 1146,
    	[Obsolete]
    	BaseBioReactorFragment = 1147,
    	BaseNuclearReactorFragment = 1148,
    	LaserCutterFragment = 1149,
    	[Obsolete]
    	BeaconFragment = 1150,
    	GravSphereFragment = 1151,
    	[Obsolete]
    	DeployableDrillFragment = 1152,
    	SeaTruckFragment = 1153,
    	SeaTruckStorageModuleFragment = 1154,
    	SeaTruckFabricatorModuleFragment = 1155,
    	SeaTruckAquariumModuleFragment = 1156,
    	HoverpadFragment = 1157,
    	HoverbikeFragment = 1158,
    	SeaTruckSleeperModuleFragment = 1159,
    	SeaTruckDockingModuleFragment = 1160,
    	JukeboxFragment = 1161,
    	BaseLargeRoomFragment = 1162,
    	BaseLargeGlassDomeFragment = 1163,
    	BaseGlassDomeFragment = 1164,
    	MetalDetectorFragment = 1165,
    	[Obsolete]
    	SeaTruckPlanterModuleFragment = 1166,
    	RepulsionCannonFragment = 1167,
    	SeaTruckUpgradeAfterburnerFragment = 1168,
    	SeaTruckUpgradeHorsePowerFragment = 1169,
    	HighCapacityTankFragment = 1170,
    	ExosuitThermalReactorModuleFragment = 1171,
    	[Obsolete]
    	SafeShallowsEgg = 1250,
    	[Obsolete]
    	KelpForestEgg = 1251,
    	[Obsolete]
    	GrassyPlateausEgg = 1252,
    	[Obsolete]
    	GrandReefsEgg = 1253,
    	[Obsolete]
    	MushroomForestEgg = 1254,
    	[Obsolete]
    	KooshZoneEgg = 1255,
    	[Obsolete]
    	TwistyBridgesEgg = 1256,
    	LavaZoneEgg = 1257,
    	[Obsolete]
    	StalkerEgg = 1258,
    	[Obsolete]
    	ReefbackEgg = 1259,
    	[Obsolete]
    	SpadefishEgg = 1260,
    	[Obsolete]
    	RabbitrayEgg = 1261,
    	[Obsolete]
    	MesmerEgg = 1262,
    	[Obsolete]
    	JumperEgg = 1263,
    	[Obsolete]
    	SandsharkEgg = 1264,
    	[Obsolete]
    	JellyrayEgg = 1265,
    	[Obsolete]
    	BonesharkEgg = 1266,
    	[Obsolete]
    	CrabsnakeEgg = 1267,
    	ShockerEgg = 1268,
    	[Obsolete]
    	GasopodEgg = 1269,
    	[Obsolete]
    	RabbitrayEggUndiscovered = 1270,
    	[Obsolete]
    	JellyrayEggUndiscovered = 1271,
    	[Obsolete]
    	StalkerEggUndiscovered = 1272,
    	[Obsolete]
    	ReefbackEggUndiscovered = 1273,
    	[Obsolete]
    	JumperEggUndiscovered = 1274,
    	[Obsolete]
    	BonesharkEggUndiscovered = 1275,
    	[Obsolete]
    	GasopodEggUndiscovered = 1276,
    	[Obsolete]
    	MesmerEggUndiscovered = 1277,
    	[Obsolete]
    	SandsharkEggUndiscovered = 1278,
    	ShockerEggUndiscovered = 1279,
    	GenericEgg = 1280,
    	[Obsolete]
    	CrashEgg = 1281,
    	[Obsolete]
    	CrashEggUndiscovered = 1282,
    	[Obsolete]
    	CrabsquidEgg = 1283,
    	[Obsolete]
    	CrabsquidEggUndiscovered = 1284,
    	[Obsolete]
    	CutefishEgg = 1285,
    	CutefishEggUndiscovered = 1286,
    	[Obsolete]
    	LavaLizardEgg = 1287,
    	[Obsolete]
    	LavaLizardEggUndiscovered = 1288,
    	[Obsolete]
    	CrabsnakeEggUndiscovered = 1289,
    	[Obsolete]
    	SpadefishEggUndiscovered = 1290,
    	SeaMonkeyEgg = 1291,
    	ArcticRayEgg = 1292,
    	ArcticRayEggUndiscovered = 1293,
    	BruteSharkEgg = 1294,
    	BruteSharkEggUndiscovered = 1295,
    	LilyPaddlerEgg = 1296,
    	LilyPaddlerEggUndiscovered = 1297,
    	PinnacaridEgg = 1298,
    	PinnacaridEggUndiscovered = 1299,
    	ReefbackShell = 1300,
    	ReefbackTissue = 1301,
    	ReefbackAdvancedStructure = 1302,
    	SquidSharkEgg = 1310,
    	SquidSharkEggUndiscovered = 1311,
    	TitanHolefishEgg = 1312,
    	TitanHolefishEggUndiscovered = 1313,
    	TrivalveBlueEgg = 1314,
    	TrivalveYellowEgg = 1315,
    	TrivalveBlueEggUndiscovered = 1316,
    	TrivalveYellowEggUndiscovered = 1317,
    	BrinewingEgg = 1318,
    	BrinewingEggUndiscovered = 1319,
    	CryptosuchusEgg = 1320,
    	CryptosuchusEggUndiscovered = 1321,
    	GlowWhaleEgg = 1322,
    	GlowWhaleEggUndiscovered = 1323,
    	JellyfishEgg = 1324,
    	JellyfishEggUndiscovered = 1325,
    	PenguinEgg = 1326,
    	PenguinEggUndiscovered = 1327,
    	RockPuncherEgg = 1328,
    	RockPuncherEggUndiscovered = 1329,
    	ReefbackDNA = 1400,
    	Workbench = 1500,
    	[Obsolete]
    	HullReinforcementModule = 1501,
    	Fabricator = 1502,
    	Aquarium = 1503,
    	Locker = 1504,
    	Spotlight = 1505,
    	DiveHatch = 1506,
    	CurrentGenerator = 1507,
    	[Obsolete]
    	FragmentAnalyzer = 1508,
    	[Obsolete]
    	SpecialHullPlate = 1509,
    	[Obsolete]
    	BikemanHullPlate = 1510,
    	[Obsolete]
    	EatMyDictionHullPlate = 1511,
    	[Obsolete]
    	DevTestItem = 1512,
    	[Obsolete]
    	SpecimenAnalyzer = 1513,
    	[Obsolete]
    	HullReinforcementModule2 = 1514,
    	[Obsolete]
    	HullReinforcementModule3 = 1515,
    	[Obsolete]
    	PowerUpgradeModule = 1516,
    	SolarPanel = 1517,
    	Sign = 1518,
    	PowerTransmitter = 1519,
    	[Obsolete]
    	Accumulator = 1520,
    	[Obsolete]
    	Bioreactor = 1521,
    	ThermalPlant = 1522,
    	[Obsolete]
    	NuclearReactor = 1523,
    	SmallLocker = 1524,
    	Bench = 1525,
    	PictureFrame = 1526,
    	PlanterPot = 1527,
    	PlanterBox = 1528,
    	PlanterShelf = 1529,
    	FarmingTray = 1530,
    	FiltrationMachine = 1531,
    	Techlight = 1532,
    	[Obsolete]
    	Radio = 1533,
    	PlanterPot2 = 1534,
    	PlanterPot3 = 1535,
    	[Obsolete]
    	MedicalCabinet = 1536,
    	[Obsolete]
    	CyclopsHullModule1 = 1537,
    	[Obsolete]
    	CyclopsHullModule2 = 1538,
    	SingleWallShelf = 1539,
    	WallShelves = 1540,
    	Bed1 = 1541,
    	Bed2 = 1542,
    	NarrowBed = 1543,
    	BatteryCharger = 1544,
    	PowerCellCharger = 1545,
    	[Obsolete]
    	Incubator = 1546,
    	[Obsolete]
    	HatchingEnzymes = 1547,
    	[Obsolete]
    	EnyzmeCloud = 1548,
    	[Obsolete]
    	EnzymeCureBall = 1549,
    	[Obsolete]
    	Centrifuge = 1550,
    	[Obsolete]
    	CyclopsShieldModule = 1551,
    	[Obsolete]
    	CyclopsSonarModule = 1552,
    	[Obsolete]
    	CyclopsSeamothRepairModule = 1553,
    	[Obsolete]
    	CyclopsDecoyModule = 1554,
    	[Obsolete]
    	CyclopsFireSuppressionModule = 1555,
    	[Obsolete]
    	CyclopsFabricator = 1556,
    	[Obsolete]
    	CyclopsThermalReactorModule = 1557,
    	[Obsolete]
    	CyclopsHullModule3 = 1558,
    	[Obsolete]
    	PhaseGate = 1559,
    	[Obsolete]
    	WeatherStation = 1560,
    	[Obsolete]
    	WeatherPanel = 1561,
    	[Obsolete]
    	WeatherBalloon = 1562,
    	BaseColorCustomizer = 1563,
    	Jukebox = 1564,
    	SeaTruckFabricator = 1565,
    	Speaker = 1566,
    	QuantumLocker = 1567,
    	Recyclotron = 1568,
    	[Obsolete]
    	StarshipCargoCrate = 1800,
    	[Obsolete]
    	StarshipCircuitBox = 1801,
    	StarshipDesk = 1802,
    	StarshipChair = 1803,
    	[Obsolete]
    	StarshipMonitor = 1804,
    	StarshipChair2 = 1805,
    	StarshipChair3 = 1806,
    	[Obsolete]
    	LuggageBag = 1807,
    	[Obsolete]
    	ArcadeGorgetoy = 1808,
    	[Obsolete]
    	LabEquipment1 = 1809,
    	[Obsolete]
    	LabEquipment2 = 1810,
    	[Obsolete]
    	LabEquipment3 = 1811,
    	CoffeeVendingMachine = 1812,
    	BarTable = 1813,
    	[Obsolete]
    	Cap1 = 1814,
    	[Obsolete]
    	Cap2 = 1815,
    	LabContainer = 1816,
    	LabContainer2 = 1817,
    	LabContainer3 = 1818,
    	Trashcans = 1819,
    	LabTrashcan = 1820,
    	VendingMachine = 1821,
    	LabCounter = 1822,
    	[Obsolete]
    	StarshipSouvenir = 1823,
    	PosterAurora = 1824,
    	PosterExoSuit1 = 1825,
    	PosterExoSuit2 = 1826,
    	PosterKitty = 1827,
    	[Obsolete]
    	ToyCar = 1828,
    	Snowman = 1829,
    	PosterSpyPenguin = 1830,
    	Fridge = 1831,
    	Shower = 1832,
    	Sink = 1833,
    	SmallStove = 1834,
    	Toilet = 1835,
    	RocketBaseWorldMap = 1836,
    	PosterMotivational = 1837,
    	PosterSeatruck = 1838,
    	[Obsolete]
    	PosterJeremiahBirds = 1839,
    	PosterLilArchitect = 1840,
    	PictureLilKids1 = 1841,
    	PosterJeremiahNoBirds = 1842,
    	PictureFredPengling = 1843,
    	PictureVinhPostcard = 1844,
    	PictureSamPotato = 1845,
    	EmmanuelPendulum = 1846,
    	FredShavingKit = 1847,
    	PosterSeatruck2 = 1848,
    	PosterMercury = 1849,
    	PicturePotatoPortrait = 1850,
    	PictureLilKids2 = 1851,
    	PosterMotivational2 = 1852,
    	PosterMotivational3 = 1853,
    	[Obsolete]
    	PosterVesper = 1854,
    	PictureSamDanielleHappy = 1855,
    	AromatherapyLamp = 1856,
    	PictureVinhBiologyArt = 1857,
    	PictureDanielleAbstractArt = 1858,
    	SamNecklace = 1859,
    	BedDanielle = 1860,
    	BedEmmanuel = 1861,
    	BedFred = 1862,
    	BedJeremiah = 1863,
    	BedSam = 1864,
    	BedZeta = 1865,
    	ExecutiveDesk = 1866,
    	BedParvan = 1867,
    	PictureSamHand = 1868,
    	[Obsolete]
    	Seamoth = 2000,
    	Exosuit = 2001,
    	CrashedShip = 2002,
    	[Obsolete]
    	Cyclops = 2003,
    	[Obsolete]
    	Audiolog = 2004,
    	Signal = 2005,
    	Hoverbike = 2006,
    	SeaTruck = 2007,
    	SeaTruckFabricatorModule = 2008,
    	[Obsolete]
    	SeaTruckPlanterModule = 2009,
    	SeaTruckStorageModule = 2010,
    	SeaTruckAquariumModule = 2011,
    	SeaTruckDockingModule = 2012,
    	SeaTruckSleeperModule = 2013,
    	SeaTruckTeleportationModule = 2014,
    	[Obsolete]
    	SeamothReinforcementModule = 2100,
    	[Obsolete]
    	VehiclePowerUpgradeModule = 2101,
    	[Obsolete]
    	SeamothSolarCharge = 2102,
    	VehicleStorageModule = 2103,
    	[Obsolete]
    	SeamothElectricalDefense = 2104,
    	[Obsolete]
    	VehicleArmorPlating = 2105,
    	LootSensorMetal = 2106,
    	LootSensorLithium = 2107,
    	LootSensorFragment = 2108,
    	[Obsolete]
    	SeamothTorpedoModule = 2109,
    	[Obsolete]
    	SeamothSonarModule = 2110,
    	WhirlpoolTorpedo = 2111,
    	[Obsolete]
    	VehicleHullModule1 = 2112,
    	[Obsolete]
    	VehicleHullModule2 = 2113,
    	[Obsolete]
    	VehicleHullModule3 = 2114,
    	ExosuitJetUpgradeModule = 2115,
    	ExosuitDrillArmModule = 2116,
    	ExosuitThermalReactorModule = 2117,
    	ExosuitClawArmModule = 2118,
    	GasTorpedo = 2119,
    	ExosuitPropulsionArmModule = 2120,
    	ExosuitGrapplingArmModule = 2121,
    	ExosuitTorpedoArmModule = 2122,
    	ExosuitDrillArmFragment = 2123,
    	ExosuitPropulsionArmFragment = 2124,
    	ExosuitGrapplingArmFragment = 2125,
    	ExosuitTorpedoArmFragment = 2126,
    	[Obsolete]
    	ExosuitClawArmFragment = 2127,
    	ExoHullModule1 = 2128,
    	ExoHullModule2 = 2129,
    	HoverbikeJumpModule = 2130,
    	SeaTruckUpgradeAfterburner = 2131,
    	[Obsolete]
    	SeaTruckUpgradeThruster = 2132,
    	SeaTruckUpgradeEnergyEfficiency = 2133,
    	SeaTruckUpgradePerimeterDefense = 2134,
    	SeaTruckUpgradeHorsePower = 2135,
    	SeaTruckUpgradeHull1 = 2136,
    	SeaTruckUpgradeHull2 = 2137,
    	SeaTruckUpgradeHull3 = 2138,
    	HoverbikeIceWormReductionModule = 2139,
    	MapRoomUpgradeScanRange = 2250,
    	MapRoomUpgradeScanSpeed = 2251,
    	Creepvine = 2500,
    	[Obsolete]
    	HoleFish = 2501,
    	[Obsolete]
    	Jumper = 2502,
    	CreepvineSeedCluster = 2503,
    	[Obsolete]
    	Peeper = 2504,
    	[Obsolete]
    	Oculus = 2505,
    	[Obsolete]
    	RabbitRay = 2506,
    	[Obsolete]
    	GarryFish = 2507,
    	Slime = 2508,
    	Crash = 2509,
    	Boomerang = 2510,
    	[Obsolete]
    	LavaLarva = 2511,
    	[Obsolete]
    	Stalker = 2512,
    	[Obsolete]
    	Eyeye = 2513,
    	[Obsolete]
    	Bloom = 2514,
    	Bladderfish = 2515,
    	[Obsolete]
    	Hoverfish = 2516,
    	[Obsolete]
    	Jellyray = 2517,
    	[Obsolete]
    	Reefback = 2518,
    	[Obsolete]
    	Reginald = 2519,
    	[Obsolete]
    	Spadefish = 2520,
    	Grabcrab = 2521,
    	[Obsolete]
    	Floater = 2522,
    	[Obsolete]
    	Gasopod = 2523,
    	[Obsolete]
    	Sandshark = 2524,
    	Player = 2525,
    	[Obsolete]
    	Bleeder = 2526,
    	Rockgrub = 2527,
    	CrashHome = 2528,
    	CreepvinePiece = 2529,
    	[Obsolete]
    	GasPod = 2530,
    	Hoopfish = 2531,
    	HoopfishSchool = 2532,
    	RockPuncher = 2533,
    	[Obsolete]
    	BoneShark = 2534,
    	[Obsolete]
    	Mesmer = 2535,
    	[Obsolete]
    	SeaTreader = 2536,
    	[Obsolete]
    	SeaEmperor = 2537,
    	[Obsolete]
    	Cutefish = 2538,
    	[Obsolete]
    	Crabsnake = 2539,
    	[Obsolete]
    	ReaperLeviathan = 2540,
    	[Obsolete]
    	CaveCrawler = 2541,
    	Skyray = 2542,
    	[Obsolete]
    	Biter = 2543,
    	SkyrayNonRoosting = 2544,
    	[Obsolete]
    	Shocker = 2545,
    	Spinefish = 2546,
    	[Obsolete]
    	Shuttlebug = 2547,
    	[Obsolete]
    	Blighter = 2548,
    	[Obsolete]
    	Warper = 2549,
    	[Obsolete]
    	CrabSquid = 2550,
    	[Obsolete]
    	LavaLizard = 2551,
    	[Obsolete]
    	SpineEel = 2552,
    	[Obsolete]
    	SeaDragon = 2553,
    	[Obsolete]
    	LavaBoomerang = 2554,
    	[Obsolete]
    	LavaEyeye = 2555,
    	[Obsolete]
    	SeaEmperorBaby = 2556,
    	[Obsolete]
    	WarperSpawner = 2557,
    	[Obsolete]
    	GhostRayBlue = 2558,
    	[Obsolete]
    	GhostRayRed = 2559,
    	[Obsolete]
    	ReefbackBaby = 2560,
    	PrecursorDroid = 2561,
    	[Obsolete]
    	GhostLeviathan = 2562,
    	SeaEmperorLeviathan = 2563,
    	SeaEmperorJuvenile = 2564,
    	[Obsolete]
    	GhostLeviathanJuvenile = 2565,
    	Penguin = 2566,
    	GlowWhale = 2567,
    	IceWormSpawner = 2568,
    	Pinnacarid = 2569,
    	LilyPaddler = 2570,
    	SpinnerFish = 2571,
    	ArcticRay = 2572,
    	Symbiote = 2573,
    	TitanHolefish = 2574,
    	PenguinBaby = 2575,
    	BruteShark = 2576,
    	TrivalveBlue = 2577,
    	ArcticPeeper = 2578,
    	ArrowRay = 2579,
    	SeaMonkey = 2580,
    	BladderFishSchool = 2581,
    	BoomerangFishSchool = 2582,
    	[Obsolete]
    	HoleFishSchool = 2583,
    	SpinefishSchool = 2584,
    	NootFish = 2585,
    	Brinewing = 2586,
    	Triops = 2587,
    	SquidShark = 2588,
    	SeaMonkeyBaby = 2589,
    	Chelicerate = 2590,
    	SnowStalker = 2591,
    	SnowStalkerBaby = 2592,
    	FeatherFish = 2593,
    	FeatherFishRed = 2594,
    	ShadowLeviathan = 2595,
    	LargeVentGarden = 2596,
    	SmallVentGarden = 2597,
    	TrivalveYellow = 2598,
    	Jellyfish = 2599,
    	DiscusFish = 2600,
    	IceWorm = 2601,
    	Cryptosuchus = 2602,
    	HoleFishAnalysis = 2700,
    	[Obsolete]
    	PeeperAnalysis = 2701,
    	BladderfishAnalysis = 2702,
    	[Obsolete]
    	GarryFishAnalysis = 2703,
    	[Obsolete]
    	HoverfishAnalysis = 2704,
    	[Obsolete]
    	ReginaldAnalysis = 2705,
    	[Obsolete]
    	SpadefishAnalysis = 2706,
    	BoomerangAnalysis = 2707,
    	[Obsolete]
    	EyeyeAnalysis = 2708,
    	[Obsolete]
    	OculusAnalysis = 2709,
    	HoopfishAnalysis = 2710,
    	AnalysisTreeOld = 2711,
    	SpinefishAnalysis = 2712,
    	PlantPlaceholder = 3000,
    	BallClusters = 3001,
    	BarnacleSuckers = 3002,
    	BlueBarnacle = 3003,
    	BlueBarnacleCluster = 3004,
    	BlueCoralTubes = 3005,
    	RedGrass = 3006,
    	GreenGrass = 3007,
    	Mohawk = 3008,
    	GreenReeds = 3009,
    	JellyPlant = 3010,
    	BlueJeweledDisk = 3011,
    	GreenJeweledDisk = 3012,
    	PurpleJeweledDisk = 3013,
    	RedJeweledDisk = 3014,
    	[Obsolete]
    	SmallKoosh = 3015,
    	[Obsolete]
    	MediumKoosh = 3016,
    	[Obsolete]
    	LargeKoosh = 3017,
    	[Obsolete]
    	HugeKoosh = 3018,
    	[Obsolete]
    	MembrainTree = 3019,
    	[Obsolete]
    	PurpleFan = 3020,
    	[Obsolete]
    	AcidMushroom = 3021,
    	PurpleTentacle = 3022,
    	RedSeaweed = 3023,
    	CoralOldPlaceholder = 3024,
    	CoralShellPlate = 3025,
    	[Obsolete]
    	SmallFan = 3026,
    	[Obsolete]
    	SmallFanCluster = 3027,
    	BigCoralTubes = 3028,
    	TreeMushroom = 3029,
    	BlueCluster = 3030,
    	BrownTubes = 3031,
    	BloodGrass = 3032,
    	HeatArea = 3033,
    	[Obsolete]
    	BloodOil = 3034,
    	[Obsolete]
    	WhiteMushroom = 3035,
    	BloodRoot = 3036,
    	BloodVine = 3037,
    	PinkFlower = 3038,
    	[Obsolete]
    	PinkMushroom = 3039,
    	[Obsolete]
    	PurpleRattle = 3040,
    	[Obsolete]
    	BulboTree = 3041,
    	[Obsolete]
    	PurpleVasePlant = 3042,
    	[Obsolete]
    	OrangeMushroom = 3043,
    	FernPalm = 3044,
    	HangingFruitTree = 3045,
    	PurpleVegetablePlant = 3046,
    	MelonPlant = 3047,
    	[Obsolete]
    	BluePalm = 3048,
    	[Obsolete]
    	GabeSFeather = 3049,
    	[Obsolete]
    	SeaCrown = 3050,
    	OrangePetalsPlant = 3051,
    	EyesPlant = 3052,
    	RedGreenTentacle = 3053,
    	PurpleStalk = 3054,
    	RedBasketPlant = 3055,
    	RedBush = 3056,
    	[Obsolete]
    	RedConePlant = 3057,
    	ShellGrass = 3058,
    	SpottedLeavesPlant = 3059,
    	[Obsolete]
    	RedRollPlant = 3060,
    	PurpleBranches = 3061,
    	[Obsolete]
    	SnakeMushroom = 3062,
    	[Obsolete]
    	SeaTreaderPoop = 3063,
    	GenericJeweledDisk = 3064,
    	FloatingStone = 3065,
    	BlueAmoeba = 3066,
    	RedTipRockThings = 3067,
    	BlueTipLostRiverPlant = 3068,
    	BlueLostRiverLilly = 3069,
    	[Obsolete]
    	LargeFloater = 3070,
    	GenericArmored = 3071,
    	GenericBowl = 3072,
    	GenericBulbStalk = 3073,
    	GenericCage = 3074,
    	GenericSpine = 3075,
    	GenericShellDouble = 3076,
    	GenericShellSingle = 3077,
    	GenericSpiral = 3078,
    	GenericRibbon = 3079,
    	SeaMonkeyNest1 = 3080,
    	SeaMonkeyNest2 = 3081,
    	SeaMonkeyNest3 = 3082,
    	ThermalLily = 3083,
    	OxygenPlant = 3084,
    	[Obsolete]
    	SeaMonkeyNestRoof = 3085,
    	SeaMonkeyNest4 = 3086,
    	SeaMonkeyNest5 = 3087,
    	HeatFruitPlant = 3088,
    	SnowPlant = 3089,
    	GlowFlower = 3090,
    	KelpRoot = 3091,
    	KelpRootPustule = 3092,
    	SeaMonkeyNest6 = 3093,
    	SeaMonkeyNest7 = 3094,
    	BlastingVent = 3095,
    	TemperateCrystal = 3096,
    	TwistyBridgesCoralShelf = 3097,
    	TwistyBridgesLargePlant = 3098,
    	TwistyBridgesMushroom = 3099,
    	BlueFurPlant = 3100,
    	GenericBigPlant1 = 3101,
    	GenericBigPlant2 = 3102,
    	GenericCrystal = 3103,
    	CaveFlower = 3104,
    	LeafyFruitPlant = 3105,
    	IceFruitPlant = 3106,
    	DeepLilyShroom = 3107,
    	HivePlant = 3108,
    	DeepTwistyBridgesLargePlant = 3109,
    	CyanFlower = 3110,
    	TapePlant = 3111,
    	LilyPadStage1 = 3112,
    	LilyPadFallen = 3113,
    	LilyPadMature = 3114,
    	LilyPadRoot = 3115,
    	TrianglePlant = 3116,
    	SmallMaroonPlant = 3117,
    	LilyPadStage2 = 3118,
    	LilyPadStage3 = 3119,
    	[Obsolete]
    	FrozenRiverPlant1 = 3120,
    	FrozenRiverPlant2 = 3121,
    	GlacialTree = 3122,
    	GlacialBulb = 3123,
    	BoneYard = 3124,
    	CavePlant = 3125,
    	TwistyBridge = 3126,
    	TwistyBridgeCliffPlant = 3127,
    	TwistyBridgeCoralLong = 3128,
    	MohawkPlant = 3129,
    	CrystalCavernBigCrystal = 3130,
    	FabricatorCavernBigCrystal = 3131,
    	SmallMaroonPlantFruit = 3132,
    	SnowStalkerFruit = 3133,
    	SnowStalkerPlant = 3134,
    	SnowStalkerPlantLeaf = 3135,
    	HoneyCombPlant = 3136,
    	ThermalSpire = 3137,
    	ThermalSpireBarnacle = 3138,
    	TreeSpire = 3139,
    	TreeSpireMushroom = 3140,
    	IceCrystal = 3141,
    	GlacialPouchBulb = 3142,
    	DeepLilyPadsLanternPlant = 3143,
    	TallShootsPlant = 3144,
    	TornadoPlates = 3145,
    	PurpleVent = 3146,
    	GeothermalVent = 3147,
    	PiecePlaceholder = 3500,
    	JeweledDiskPiece = 3501,
    	[Obsolete]
    	CoralChunk = 3502,
    	[Obsolete]
    	KooshChunk = 3503,
    	[Obsolete]
    	StalkerTooth = 3504,
    	TreeMushroomPiece = 3505,
    	[Obsolete]
    	BulboTreePiece = 3506,
    	[Obsolete]
    	OrangeMushroomSpore = 3507,
    	[Obsolete]
    	PurpleVasePlantSeed = 3508,
    	[Obsolete]
    	AcidMushroomSpore = 3509,
    	[Obsolete]
    	WhiteMushroomSpore = 3510,
    	[Obsolete]
    	PinkMushroomSpore = 3511,
    	[Obsolete]
    	PurpleRattleSpore = 3512,
    	HangingFruit = 3513,
    	PurpleVegetable = 3514,
    	SmallMelon = 3515,
    	Melon = 3516,
    	MelonSeed = 3517,
    	[Obsolete]
    	PurpleBrainCoralPiece = 3518,
    	[Obsolete]
    	SpikePlantSeed = 3519,
    	[Obsolete]
    	BluePalmSeed = 3520,
    	[Obsolete]
    	PurpleFanSeed = 3521,
    	[Obsolete]
    	SmallFanSeed = 3522,
    	[Obsolete]
    	PurpleTentacleSeed = 3523,
    	JellyPlantSeed = 3524,
    	[Obsolete]
    	GabeSFeatherSeed = 3525,
    	[Obsolete]
    	SeaCrownSeed = 3526,
    	[Obsolete]
    	MembrainTreeSeed = 3527,
    	[Obsolete]
    	PinkFlowerSeed = 3528,
    	[Obsolete]
    	FernPalmSeed = 3529,
    	[Obsolete]
    	OrangePetalsPlantSeed = 3530,
    	[Obsolete]
    	EyesPlantSeed = 3531,
    	[Obsolete]
    	RedGreenTentacleSeed = 3532,
    	PurpleStalkSeed = 3533,
    	[Obsolete]
    	RedBasketPlantSeed = 3534,
    	RedBushSeed = 3535,
    	[Obsolete]
    	RedConePlantSeed = 3536,
    	[Obsolete]
    	ShellGrassSeed = 3537,
    	[Obsolete]
    	SpottedLeavesPlantSeed = 3538,
    	[Obsolete]
    	RedRollPlantSeed = 3539,
    	[Obsolete]
    	PurpleBranchesSeed = 3540,
    	[Obsolete]
    	SnakeMushroomSpore = 3541,
    	HeatFruit = 3542,
    	SnowStalkerFur = 3543,
    	GenericRibbonSeed = 3544,
    	[Obsolete]
    	KelpRootPustuleSeed = 3545,
    	LeafyFruit = 3546,
    	IceFruit = 3547,
    	TwistyBridgesMushroomChunk = 3548,
    	FrozenRiverPlant2Seeds = 3549,
    	GenericSpiralChunk = 3550,
    	[Obsolete]
    	DeepLilyShroomSeed = 3551,
    	SmallMaroonPlantSeed = 3552,
    	[Obsolete]
    	DeepLilyFlower = 3553,
    	EnvironmentPlaceholder = 4000,
    	Boulder = 4001,
    	[Obsolete]
    	PurpleBrainCoral = 4002,
    	HangingStinger = 4003,
    	[Obsolete]
    	SpikePlant = 4004,
    	BrainCoral = 4005,
    	CoveTree = 4006,
    	MonsterSkeleton = 4007,
    	SeaDragonSkeleton = 4008,
    	ReaperSkeleton = 4009,
    	[Obsolete]
    	CaveSkeleton = 4010,
    	HugeSkeleton = 4011,
    	Brinicle = 4012,
    	SpikeyTrap = 4013,
    	FrozenCreature = 4014,
    	FrozenCreature_Head = 4015,
    	FrozenCreature_Horns = 4016,
    	FrozenCreature_Teeth = 4017,
    	FrozenCreature_Pustules = 4018,
    	FrozenCreature_Claws = 4019,
    	SnowBall = 4020,
    	IceBubble = 4021,
    	[Obsolete]
    	PrecursorKey_Red = 4200,
    	[Obsolete]
    	PrecursorKey_Blue = 4201,
    	[Obsolete]
    	PrecursorKey_Orange = 4202,
    	[Obsolete]
    	PrecursorKey_White = 4203,
    	[Obsolete]
    	PrecursorKey_Purple = 4204,
    	[Obsolete]
    	PrecursorKey_PurpleFragment = 4205,
    	[Obsolete]
    	PrecursorKeyTerminal = 4206,
    	PrecursorTeleporter = 4207,
    	PrecursorEnergyCore = 4208,
    	PrecursorIonPowerCell = 4209,
    	PrecursorIonBattery = 4210,
    	PrecursorThermalPlant = 4211,
    	[Obsolete]
    	PrecursorWarper = 4212,
    	[Obsolete]
    	PrecursorFishSkeleton = 4213,
    	[Obsolete]
    	PrecursorScanner = 4214,
    	[Obsolete]
    	PrecursorLabCacheContainer1 = 4215,
    	[Obsolete]
    	PrecursorLabCacheContainer2 = 4216,
    	[Obsolete]
    	PrecursorLabTable = 4217,
    	[Obsolete]
    	PrecursorSeaDragonSkeleton = 4218,
    	[Obsolete]
    	PrecursorSensor = 4219,
    	[Obsolete]
    	PrecursorPrisonArtifact1 = 4220,
    	[Obsolete]
    	PrecursorPrisonArtifact2 = 4221,
    	[Obsolete]
    	PrecursorPrisonArtifact3 = 4222,
    	[Obsolete]
    	PrecursorPrisonArtifact4 = 4223,
    	[Obsolete]
    	PrecursorPrisonArtifact5 = 4224,
    	[Obsolete]
    	PrecursorPrisonArtifact6 = 4225,
    	[Obsolete]
    	PrecursorPrisonArtifact7 = 4226,
    	[Obsolete]
    	PrecursorPrisonArtifact8 = 4227,
    	[Obsolete]
    	PrecursorPrisonArtifact9 = 4228,
    	[Obsolete]
    	PrecursorPrisonArtifact10 = 4229,
    	[Obsolete]
    	PrecursorPrisonArtifact11 = 4230,
    	[Obsolete]
    	PrecursorPrisonArtifact12 = 4231,
    	[Obsolete]
    	PrecursorPipeRoomIncomingPipe = 4232,
    	[Obsolete]
    	PrecursorPipeRoomOutgoingPipe = 4233,
    	[Obsolete]
    	PrecursorPrisonLabEmperorFetus = 4234,
    	[Obsolete]
    	PrecursorPrisonLabEmperorEgg = 4235,
    	[Obsolete]
    	PrecursorPrisonAquariumPipe = 4236,
    	[Obsolete]
    	PrecursorPrisonAquariumFinalTeleporter = 4237,
    	[Obsolete]
    	PrecursorPrisonAquariumIncubatorEggs = 4238,
    	[Obsolete]
    	PrecursorPrisonAquariumIncubator = 4239,
    	[Obsolete]
    	PrecursorSurfacePipe = 4240,
    	[Obsolete]
    	PrecursorPrisonArtifact13 = 4241,
    	[Obsolete]
    	PrecursorPrisonIonGenerator = 4242,
    	[Obsolete]
    	PrecursorPrisonOutposts = 4243,
    	PrecursorSanctuaryCube = 4244,
    	PrecursorCacheCrystal = 4245,
    	PrecursorCable = 4246,
    	ObservatoryOld = 4250,
    	PrecursorLostRiverBrokenAnchor = 4300,
    	PrecursorLostRiverLabRays = 4301,
    	PrecursorLostRiverLabBones = 4302,
    	PrecursorLostRiverLabEgg = 4303,
    	PrecursorLostRiverProductionLine = 4304,
    	PrecursorLostRiverWarperParts = 4305,
    	FilteredWater = 4500,
    	DisinfectedWater = 4501,
    	[Obsolete]
    	CookedPeeper = 4502,
    	[Obsolete]
    	CookedHoleFish = 4503,
    	[Obsolete]
    	CookedGarryFish = 4504,
    	[Obsolete]
    	CookedReginald = 4505,
    	CookedBladderfish = 4506,
    	[Obsolete]
    	CookedHoverfish = 4507,
    	[Obsolete]
    	CookedSpadefish = 4508,
    	CookedBoomerang = 4509,
    	[Obsolete]
    	CookedEyeye = 4510,
    	[Obsolete]
    	CookedOculus = 4511,
    	CookedHoopfish = 4512,
    	NutrientBlock = 4513,
    	FirstAidKit = 4514,
    	WaterFiltrationSuitWater = 4515,
    	BigFilteredWater = 4516,
    	CookedSpinefish = 4517,
    	[Obsolete]
    	CookedLavaEyeye = 4518,
    	[Obsolete]
    	CookedLavaBoomerang = 4519,
    	[Obsolete]
    	Snack1 = 4520,
    	[Obsolete]
    	Snack2 = 4521,
    	[Obsolete]
    	Snack3 = 4522,
    	Coffee = 4523,
    	[Obsolete]
    	HeatingGel = 4524,
    	CookedSpinnerfish = 4525,
    	CookedSymbiote = 4526,
    	CookedArcticPeeper = 4527,
    	CookedArrowRay = 4528,
    	CookedNootFish = 4529,
    	CookedTriops = 4530,
    	CookedFeatherFish = 4531,
    	CookedFeatherFishRed = 4532,
    	CookedDiscusFish = 4533,
    	WaterPurificationTablet = 4534,
    	SpicyFruitSalad = 4535,
    	[Obsolete]
    	CuredPeeper = 4600,
    	[Obsolete]
    	CuredHoleFish = 4601,
    	[Obsolete]
    	CuredGarryFish = 4602,
    	[Obsolete]
    	CuredReginald = 4603,
    	CuredBladderfish = 4604,
    	[Obsolete]
    	CuredHoverfish = 4605,
    	[Obsolete]
    	CuredSpadefish = 4606,
    	CuredBoomerang = 4607,
    	[Obsolete]
    	CuredEyeye = 4608,
    	[Obsolete]
    	CuredOculus = 4609,
    	CuredHoopfish = 4610,
    	CuredSpinefish = 4611,
    	[Obsolete]
    	CuredLavaEyeye = 4612,
    	[Obsolete]
    	CuredLavaBoomerang = 4613,
    	CuredSpinnerfish = 4614,
    	CuredSymbiote = 4615,
    	CuredArcticPeeper = 4616,
    	CuredArrowRay = 4617,
    	CuredNootFish = 4618,
    	CuredTriops = 4619,
    	CuredFeatherFish = 4620,
    	CuredFeatherFishRed = 4621,
    	CuredDiscusFish = 4622,
    	MembraneOld = 5000,
    	Unobtanium = 5001,
    	BaseRoom = 5500,
    	BaseHatch = 5501,
    	BaseWall = 5502,
    	BaseDoor = 5503,
    	BaseLadder = 5504,
    	BaseWindow = 5505,
    	[Obsolete]
    	PowerGeneratorOld = 5506,
    	UnusedOld = 5507,
    	BaseCorridor = 5508,
    	BaseFoundation = 5509,
    	BaseCorridorI = 5510,
    	BaseCorridorL = 5511,
    	BaseCorridorT = 5512,
    	BaseCorridorX = 5513,
    	BaseReinforcement = 5514,
    	BaseBulkhead = 5515,
    	BaseCorridorGlassI = 5516,
    	BaseCorridorGlassL = 5517,
    	BaseObservatory = 5518,
    	BaseConnector = 5519,
    	BaseMoonpool = 5520,
    	BaseCorridorGlass = 5521,
    	BaseUpgradeConsole = 5522,
    	BasePlanter = 5523,
    	BaseFiltrationMachine = 5524,
    	BaseWaterPark = 5525,
    	BaseMapRoom = 5526,
    	MapRoomCamera = 5527,
    	BaseBioReactor = 5528,
    	BaseNuclearReactor = 5529,
    	BasePipeConnector = 5530,
    	BaseRechargePlatform = 5531,
    	BaseControlRoom = 5532,
    	BaseWallFoundation = 5533,
    	BaseLargeRoom = 5534,
    	OldBasePartitionI = 5535,
    	OldBasePartitionL = 5536,
    	OldBasePartitionT = 5537,
    	OldBasePartitionX = 5538,
    	BasePartitionDoor = 5539,
    	BaseGlassDome = 5540,
    	BasePartition = 5541,
    	BaseLargeGlassDome = 5542,
    	[Obsolete]
    	RocketBase = 5900,
    	RocketBaseLadder = 5901,
    	[Obsolete]
    	RocketStage1 = 5902,
    	[Obsolete]
    	RocketStage2 = 5903,
    	[Obsolete]
    	RocketStage3 = 5904,
    	[Obsolete]
    	TimeCapsule = 5905,
    	[Obsolete]
    	DioramaHullPlate = 6000,
    	[Obsolete]
    	MarkiplierHullPlate = 6001,
    	[Obsolete]
    	MuyskermHullPlate = 6002,
    	[Obsolete]
    	LordMinionHullPlate = 6003,
    	[Obsolete]
    	JackSepticEyeHullPlate = 6004,
    	Poster = 6005,
    	[Obsolete]
    	IGPHullPlate = 6006,
    	[Obsolete]
    	GilathissHullPlate = 6007,
    	[Obsolete]
    	Marki1 = 6008,
    	[Obsolete]
    	Marki2 = 6009,
    	[Obsolete]
    	JackSepticEye = 6010,
    	[Obsolete]
    	EatMyDiction = 6011,
    	RadiationLeakPoint = 7000,
    	Exchanger = 8000,
    	KharaaSampleSkeleton = 8001,
    	[Obsolete]
    	KharaaSample = 8002,
    	ReinforcedGlass = 8003,
    	[Obsolete]
    	TwistyBridgeResource = 8004,
    	LilyPadResource = 8005,
    	[Obsolete]
    	DeepArcticResource = 8006,
    	[Obsolete]
    	IceBergResource = 8007,
    	[Obsolete]
    	HoverZoneResource1 = 8008,
    	[Obsolete]
    	HoverZoneResource2 = 8009,
    	SupplyDrop = 8010,
    	[Obsolete]
    	PrecursorFabricator = 8011,
    	[Obsolete]
    	PrecursorNPCPowerSource = 8013,
    	PrecursorNPCOrgans = 8014,
    	PrecursorNPCTissue = 8015,
    	PrecursorNPCSkeleton = 8016,
    	[Obsolete]
    	PrecursorNPCComputerCore = 8017,
    	[Obsolete]
    	PrecursorNPCPowerSourceFragment = 8018,
    	PrecursorNPCOrgansFragment = 8019,
    	PrecursorNPCTissueFragment = 8020,
    	PrecursorNPCSkeletonFragment = 8021,
    	[Obsolete]
    	ColleagueDiveSuit = 8022,
    	AlterraPrecursorResearchNotes = 8023,
    	ColleaguePDA1 = 8024,
    	Hoverpad = 8025,
    	[Obsolete]
    	PrecursorNPCFakeComputerCore = 8026,
    	PrecursorTechGeneric = 8027,
    	[Obsolete]
    	BioScanner = 8028,
    	[Obsolete]
    	BioScannerSample = 8029,
    	SpyPenguin = 8030,
    	ResearchBaseExo = 8031,
    	OreVein = 8032,
    	SpyPenguinRemote = 8033,
    	SpyPenguinFragment = 8034,
    	SpyPenguinRemoteFragment = 8035,
    	PosterAlterraBounty = 8036,
    	[Obsolete]
    	RocketRadioTower = 8037,
    	RadioTowerPPUFragment = 8038,
    	RadioTowerPPU = 8039,
    	[Obsolete]
    	RadioTower = 8040,
    	Marguerit = 8041,
    	[Obsolete]
    	AlterraShuttle = 8042,
    	MargueritExosuit = 8043,
    	HydraulicFluid = 8044,
    	RadioTowerTOM = 8045,
    	RadioTowerTOMFragment = 8046,
    	PosterParvan = 8047,
    	ColdSuitFragment = 8048,
    	FrozenCreatureAntidote = 8049,
    	PosterBunkerCommunity = 8050,
    	SpyPenguinMap = 8051,
    	PosterZetaRollerDerby = 8052,
    	PosterSpyPenguinConcepts = 8053,
    	PosterBoardgame = 8054,
    	PosterHangInThere = 8055,
    	PosterSpyPenguinBlueprint = 8056,
    	HoverbikeBaseZetaNamePlate = 8057,
    	HoverbikeBaseSamNamePlate = 8058,
    	BunkerParvanNamePlate = 8059,
    	PosterParvanBiome = 8060,
    	OutpostOmegaDanielleNamePlate = 8061,
    	OutpostOmegaVinhNamePlate = 8062,
    	DeltaStationFredNamePlate = 8063,
    	DeltaStationEmmanuelNamePlate = 8064,
    	DeltaStationJeremiahNamePlate = 8065,
    	OutpostZeroLillianNamePlate = 8066,
    	HydraulicFluidFragment = 8067,
    	SpyPenguinResearchSite = 8068,
    	JukeboxDisksAll = 8069,
    	SomethingPlaceholder = 10000,
    	Fragment = 10001,
    	Wreck = 10002,
    	CountOld = 10003,
    	Databox = 10004,
    	DataChip = 10005,
    	Anomaly = 10006,
    	BaseMoonpoolExpansion = 10007
    }
}
