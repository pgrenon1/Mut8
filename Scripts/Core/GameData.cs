namespace Mut8.Scripts
{
    /// <summary>
    /// Contains all game balance constants for tuning gameplay.
    /// </summary>
    internal static class GameData
    {
        // Health related constants
        public const float BaseMaxHP = 100f;
        public const float StoutGeneHPMultiplier = 20f;

        // Combat related constants
        public const float BaseAttackPower = 10f;
        public const float BaseDefense = 5f;
        public const float StrongGeneAttackMultiplier = 5f;
        public const float ResilientGeneDefenseMultiplier = 3f;

        // Gene effectiveness multipliers (for future use)
        public const float QuickGeneSpeedMultiplier = 1f;
        public const float SmartGeneXPMultiplier = 1f;
        public const float StealthyGeneDetectionMultiplier = 1f;
    }
}
