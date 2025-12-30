using EldenRingSim.Repositories;
using Microsoft.AspNetCore.Mvc;

// Weapon endpoints for retrieving weapon data, filtering by category/requirements, and calculating boss matchup effectiveness

namespace EldenRingSim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeaponsController : ControllerBase
    {
        private readonly IWeaponRepository _weaponRepo;
        private readonly IBossStatsRepository _bossStatsRepo;
        private readonly IBossRepository _bossRepo;

        public WeaponsController(
            IWeaponRepository weaponRepo,
            IBossStatsRepository bossStatsRepo,
            IBossRepository bossRepo)
        {
            _weaponRepo = weaponRepo;
            _bossStatsRepo = bossStatsRepo;
            _bossRepo = bossRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var weapons = await _weaponRepo.GetAllAsync();
            return Ok(weapons);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var weapon = await _weaponRepo.GetByIdAsync(id);
            if (weapon == null)
                return NotFound();
            
            return Ok(weapon);
        }

        [HttpGet("test-matchup")]
        public async Task<IActionResult> TestMatchup()
        {
            try
            {
                var weapon = await _weaponRepo.GetByIdAsync("zweihander");
                if (weapon == null) return Ok(new { error = "Weapon not found" });
                
                var bosses = await _bossRepo.GetAllAsync();
                
                var testBoss = bosses.FirstOrDefault();
                DB.BossStats? testStats = null;
                if (testBoss != null)
                {
                    testStats = await _bossStatsRepo.GetByBossNameAsync(testBoss.Name);
                }
                
                return Ok(new { 
                    weaponFound = weapon != null,
                    weaponName = weapon?.Name,
                    bossCount = bosses.Count(),
                    firstBossName = testBoss?.Name,
                    statsFound = testStats != null
                });
            }
            catch (Exception ex)
            {
                return Ok(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("{id}/matchups")]
        public async Task<IActionResult> GetWeaponMatchups(string id)
        {
            try
            {
                Console.WriteLine($"DEBUG: Getting matchups for weapon {id}");
                
                var weapon = await _weaponRepo.GetByIdAsync(id);
                if (weapon == null)
                {
                    Console.WriteLine($"DEBUG: Weapon {id} not found");
                    return NotFound("Weapon not found");
                }

                Console.WriteLine($"DEBUG: Weapon found: {weapon.Name}");

                var allBosses = await _bossRepo.GetAllAsync();
                Console.WriteLine($"DEBUG: Found {allBosses.Count()} bosses");
                
                var matchups = new List<BossMatchup>();

                foreach (var boss in allBosses)
                {
                    var bossStats = await _bossStatsRepo.GetByBossNameAsync(boss.Name);
                    
                    if (bossStats != null)
                    {
                        double score = CalculateEffectiveness(weapon, bossStats);
                        
                        matchups.Add(new BossMatchup
                        {
                            BossId = boss.Id,
                            BossName = boss.Name,
                            BossImage = boss.Image,
                            Region = boss.Region,
                            EffectivenessScore = score,
                            Rating = GetRating(score)
                        });
                    }
                }

                Console.WriteLine($"DEBUG: Created {matchups.Count} matchups");

                matchups = matchups.OrderByDescending(m => m.EffectivenessScore).ToList();

                return Ok(matchups);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetWeaponMatchups: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var weapons = await _weaponRepo.GetByCategoryAsync(category);
            return Ok(weapons);
        }

        [HttpGet("weight")]
        public async Task<IActionResult> GetByWeight([FromQuery] double min, [FromQuery] double max)
        {
            var weapons = await _weaponRepo.GetByWeightRangeAsync(min, max);
            return Ok(weapons);
        }

        [HttpGet("usable")]
        public async Task<IActionResult> GetUsableWeapons(
            [FromQuery] int str,
            [FromQuery] int dex,
            [FromQuery] int int_,
            [FromQuery] int fai)
        {
            var weapons = await _weaponRepo.GetWeaponsMeetingRequirementsAsync(str, dex, int_, fai);
            return Ok(weapons);
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

    public class BossMatchup
    {
        public string BossId { get; set; } = null!;
        public string BossName { get; set; } = null!;
        public string BossImage { get; set; } = null!;
        public string Region { get; set; } = null!;
        public double EffectivenessScore { get; set; }
        public string Rating { get; set; } = null!;
    }
}