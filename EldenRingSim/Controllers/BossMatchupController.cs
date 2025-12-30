using EldenRingSim.DB;
using EldenRingSim.Models;
using EldenRingSim.Repositories;
using EldenRingSim.Services;
using Microsoft.AspNetCore.Mvc;

// Boss-weapon matchup analysis endpoints for calculating win probability and weapon effectiveness against specific bosses

namespace EldenRingSim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BossMatchupController : ControllerBase
    {
        private readonly IBossStatsRepository _bossStatsRepo;
        private readonly IWeaponRepository _weaponRepo;
        private readonly IAnalysisEngine _analysisEngine;

        public BossMatchupController(
            IBossStatsRepository bossStatsRepo,
            IWeaponRepository weaponRepo,
            IAnalysisEngine analysisEngine)
        {
            _bossStatsRepo = bossStatsRepo;
            _weaponRepo = weaponRepo;
            _analysisEngine = analysisEngine;
        }

    
        [HttpGet("calculate/{bossName}")]
        public async Task<ActionResult<BossMatchupResult>> CalculateMatchup(
            string bossName,
            [FromQuery] string weaponName,
            [FromQuery] int playerLevel = 150,
            [FromQuery] int vigor = 40,
            [FromQuery] int strength = 50,
            [FromQuery] int dexterity = 50,
            [FromQuery] int intelligence = 10,
            [FromQuery] int faith = 10)
        {
            var bossStats = await _bossStatsRepo.GetByBossNameAsync(bossName);
            if (bossStats == null)
                return NotFound($"Boss '{bossName}' not found");

            var weapon = await _weaponRepo.GetByNameAsync(weaponName);
            if (weapon == null)
                return NotFound($"Weapon '{weaponName}' not found");

            var build = new PlayerBuild
            {
                Level = playerLevel,
                Vigor = vigor,
                Mind = 20,
                Endurance = 30,
                Strength = strength,
                Dexterity = dexterity,
                Intelligence = intelligence,
                Faith = faith,
                Arcane = 10,
                WeaponId = weapon.Id
            };

            var winProbability = _analysisEngine.CalculateWinProbability(build, bossStats, weapon);
            var recommendedWeapons = await _analysisEngine.GetBestWeaponsForBossAsync(bossStats);

            return Ok(new BossMatchupResult
            {
                BossName = bossStats.BossName,
                WeaponName = weapon.Name,
                PlayerLevel = playerLevel,
                WinProbability = Math.Round(winProbability * 100, 2),
                BossTier = bossStats.Tier,
                BossHP = bossStats.HealthPoints,
                BossWeaknesses = bossStats.Weakness,
                RecommendedWeapons = recommendedWeapons.Take(5).ToList()
            });
        }

        [HttpGet("best-weapons/{bossName}")]
        public async Task<ActionResult<List<WeaponEffectiveness>>> GetBestWeapons(string bossName)
        {
            var bossStats = await _bossStatsRepo.GetByBossNameAsync(bossName);
            if (bossStats == null)
                return NotFound($"Boss '{bossName}' not found");

            var recommendations = await _analysisEngine.GetBestWeaponsForBossAsync(bossStats);
            return Ok(recommendations.Take(10));
        }

        [HttpGet("tier/{tier}")]
        public async Task<ActionResult<List<BossStats>>> GetBossesByTier(int tier)
        {
            if (tier < 1 || tier > 5)
                return BadRequest("Tier must be between 1 and 5");

            var bosses = await _bossStatsRepo.GetByTierAsync(tier);
            return Ok(bosses);
        }

        [HttpGet("weakness/{damageType}")]
        public async Task<ActionResult<List<BossStats>>> GetBossesByWeakness(string damageType)
        {
            var bosses = await _bossStatsRepo.GetByWeaknessAsync(damageType);
            return Ok(bosses);
        }

        [HttpGet("stats/{bossName}")]
        public async Task<ActionResult<BossStats>> GetBossStats(string bossName)
        {
            var bossStats = await _bossStatsRepo.GetByBossNameAsync(bossName);
            if (bossStats == null)
                return NotFound($"Boss '{bossName}' not found");

            return Ok(bossStats);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<BossStats>>> GetAllBosses()
        {
            var bosses = await _bossStatsRepo.GetAllAsync();
            return Ok(bosses.OrderBy(b => b.Tier).ThenBy(b => b.BossName));
        }
    }

    public class BossMatchupResult
    {
        public string BossName { get; set; } = string.Empty;
        public string WeaponName { get; set; } = string.Empty;
        public int PlayerLevel { get; set; }
        public double WinProbability { get; set; }
        public int BossTier { get; set; }
        public int BossHP { get; set; }
        public string BossWeaknesses { get; set; } = string.Empty;
        public List<WeaponEffectiveness> RecommendedWeapons { get; set; } = new();
    }
}