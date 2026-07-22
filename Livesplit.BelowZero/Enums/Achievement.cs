using System.ComponentModel;

namespace LiveSplit.BelowZero
{

    public enum Achievement
    {
        None,
        [Description("Xenobiology"), ToolTip("Encounter a Sapient Alien Lifeform")]
        AchievementAlan,
        [Description("Out of Mind"), ToolTip("Construct an Alien Vessel")]
        AchievementBody,
        [Description("Dressed For The Weather"), ToolTip("Construct a Cold Suit")]
        AchievementColdSuitEquipped,
        [Description("Into the Unknown"), ToolTip("Leave 4546b")]
        AchievementEnding,
        [Description("Drop in the Ocean"), ToolTip("Locate your Drop Pod")]
        AchievementEnterLifePod,
        [Description("Finding the Cure"), ToolTip("Use the Antidote")]
        AchievementFrolo,
        [Description("Necessary Repairs"), ToolTip("Repair the Bridge")]
        AchievementGlacialBasinBridgeItemInserted,
        [Description("Jukebox Hero"), ToolTip("Install a Jukebox")]
        AchievementJukeBoxConstructed,
        [Description("Another Survivor"), ToolTip("Find Marguerit's Home")]
        AchievementMarg,
        [Description("Pirate Radio"), ToolTip("Disable Alterra Communications")]
        AchievementRadioTower,
        [Description("Like Riding a Bike"), ToolTip("Ride a Snowfox")]
        AchievementRideSnowFox,
        [Description("Truckin'"), ToolTip("Construct a Seatruck")]
        AchievementSeatruckConstructed,
        [Description("Spy Pengling"), ToolTip("Construct a Spy Pengling")]
        AchievementSpyPenglingConstructed,
    }
}
