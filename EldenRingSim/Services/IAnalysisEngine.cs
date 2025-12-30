using EldenRingSim.DB;
using EldenRingSim.Models;

namespace EldenRingSim.Services
{
    /// <summary>
    /// Unified Analysis Engine interface - handles weapon analysis AND boss matchup analysis
    /// </summary>
    public interface IAnalysisEngine
    {
        // ============= EXISTING WEAPON ANALYSIS METHODS =============
        
        /// <summary>
        /// Analyzes how effective a specific weapon is for a given build
        /// </summary>
        Task<WeaponEffectiveness> AnalyzeWeaponAsync(string weaponId, PlayerBuild build);
        
        /// <summary>
        /// Finds the best weapons for a given build (top N results)
        /// </summary>
        Task<List<WeaponEffectiveness>> FindBestWeaponsAsync(PlayerBuild build, int topCount = 10);
        
        /// <summary>
        /// Compares multiple weapons side-by-side
        /// </summary>
        Task<List<WeaponEffectiveness>> CompareWeaponsAsync(List<string> weaponIds, PlayerBuild build);

        Task<List<WeaponRecommendation>> GetRecommendedWeaponsForPlayerAsync(
        BossStats boss, 
        PlayerProgress playerProgress);

        // ============= NEW BOSS MATCHUP METHODS =============
        
        /// <summary>
        /// Calculates the win probability for a player build against a specific boss
        /// Pass the weapon separately since PlayerBuild stores WeaponId as string
        /// </summary>
        double CalculateWinProbability(PlayerBuild build, BossStats boss, Weapons weapon);
        
        /// <summary>
        /// Gets the best weapons to use against a specific boss
        /// </summary>
        Task<List<WeaponEffectiveness>> GetBestWeaponsForBossAsync(BossStats boss);
        
        /// <summary>
        /// Analyzes a specific weapon's effectiveness against a boss (returns 0-100 score)
        /// </summary>
        double CalculateWeaponEffectivenessAgainstBoss(Weapons weapon, BossStats boss);
    }
}