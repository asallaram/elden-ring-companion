using EldenRingSim.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EldenRingSim.Controllers

// Boss endpoints for retrieving boss data, stats, regional filtering, and weapon effectiveness recommendations

{
    [ApiController]
    [Route("api/[controller]")]
    public class BossesController : ControllerBase
    {
        private readonly IBossRepository _bossRepo;
        private readonly IBossStatsRepository _bossStatsRepo;
        private readonly IWeaponRepository _weaponRepo;

        public BossesController(
            IBossRepository bossRepo,
            IBossStatsRepository bossStatsRepo,
            IWeaponRepository weaponRepo)
        {
            _bossRepo = bossRepo;
            _bossStatsRepo = bossStatsRepo;
            _weaponRepo = weaponRepo;
        }

        // GET: api/bosses
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bosses = await _bossRepo.GetAllAsync();
            return Ok(bosses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var boss = await _bossRepo.GetByIdAsync(id);
            if (boss == null)
                return NotFound();
            
            return Ok(boss);
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetBossStats(string id)
        {
            var boss = await _bossRepo.GetByIdAsync(id);
            if (boss == null)
                return NotFound("Boss not found");

            Console.WriteLine($"DEBUG: Looking for boss stats with name: '{boss.Name}'");

            var stats = await _bossStatsRepo.GetByBossNameAsync(boss.Name);
            
            Console.WriteLine($"DEBUG: Stats found: {stats != null}");
            
            if (stats == null)
                return NotFound("Boss stats not found");

            return Ok(stats);
        }

        [HttpGet("region/{region}")]
        public async Task<IActionResult> GetByRegion(string region)
        {
            var bosses = await _bossRepo.GetByRegionAsync(region);
            return Ok(bosses);
        }

        [HttpGet("difficulty")]
        public async Task<IActionResult> GetByDifficulty([FromQuery] bool ascending = false)
        {
            var bosses = await _bossRepo.GetByDifficultyAsync(ascending);
            return Ok(bosses);
        }

        [HttpGet("{id}/weapon-recommendations")]
        public async Task<IActionResult> GetWeaponRecommendations(string id)
        {
            var boss = await _bossRepo.GetByIdAsync(id);
            if (boss == null)
                return NotFound("Boss not found");

            var bossStats = await _bossStatsRepo.GetByBossNameAsync(boss.Name);
            
            if (bossStats == null)
            {
                return Ok(new List<WeaponRecommendationDto>());
            }

            var allWeapons = await _weaponRepo.GetAllAsync();
            var recommendations = new List<WeaponRecommendationDto>();

            foreach (var weapon in allWeapons)
            {
                double score = CalculateEffectiveness(weapon, bossStats);
                
                recommendations.Add(new WeaponRecommendationDto
                {
                    WeaponId = weapon.Id,
                    WeaponName = weapon.Name,
                    WeaponImage = weapon.Image,
                    Category = weapon.Category,
                    EffectivenessScore = Math.Round(score, 1),
                    Rating = GetRating(score)
                });
            }

            recommendations = recommendations
                .OrderByDescending(r => r.EffectivenessScore)
                .Take(5)
                .ToList();

            return Ok(recommendations);
        }

        private double CalculateEffectiveness(DB.Weapons weapon, DB.BossStats bossStats)
        {
            double score = 0;

            var physDmg = weapon.Attack.FirstOrDefault(a => a.Name == "Phy")?.Amount ?? 0;
            var magDmg = weapon.Attack.FirstOrDefault(a => a.Name == "Mag")?.Amount ?? 0;
            var fireDmg = weapon.Attack.FirstOrDefault(a => a.Name == "Fire")?.Amount ?? 0;
            var lgtDmg = weapon.Attack.FirstOrDefault(a => a.Name == "Ligt")?.Amount ?? 0;
            var holyDmg = weapon.Attack.FirstOrDefault(a => a.Name == "Holy")?.Amount ?? 0;

            if (bossStats.PhysicalResist < 100)
                score += physDmg * (1 - bossStats.PhysicalResist / 100);
            
            if (bossStats.MagicResist < 100)
                score += magDmg * (1 - bossStats.MagicResist / 100);
            
            if (bossStats.FireResist < 100)
                score += fireDmg * (1 - bossStats.FireResist / 100);
            
            if (bossStats.LightningResist < 100)
                score += lgtDmg * (1 - bossStats.LightningResist / 100);
            
            if (bossStats.HolyResist < 100)
                score += holyDmg * (1 - bossStats.HolyResist / 100);

            return score;
        }

        private string GetRating(double score)
        {
            if (score >= 150) return "Excellent";
            if (score >= 100) return "Good";
            if (score >= 50) return "Average";
            return "Poor";
        }
    }

    public class WeaponRecommendationDto
    {
        public string WeaponId { get; set; } = null!;
        public string WeaponName { get; set; } = null!;
        public string WeaponImage { get; set; } = null!;
        public string Category { get; set; } = null!;
        public double EffectivenessScore { get; set; }
        public string Rating { get; set; } = null!;
    }
}