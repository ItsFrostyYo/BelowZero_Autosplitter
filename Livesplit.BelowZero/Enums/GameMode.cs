namespace LiveSplit.BelowZero
{
    // Shared legacy mode enum present in both target patches.
    // The game's obsolete message changed between versions, so this mirrors the values only.
    public enum GameMode
    {
        Survival = 0,
        Freedom = 1,
        Hardcore = 2,
        Creative = 3,
        None = 4
    }
}
