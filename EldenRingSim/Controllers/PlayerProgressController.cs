using EldenRingSim.Models;
using EldenRingSim.Repositories;
using EldenRingSim.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

// Player character progress endpoints for managing character creation, boss/weapon tracking, and detailed progression statistics with user authentication

namespace EldenRingSim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlayerProgressController : ControllerBase
    {
        private readonly IPlayerProgressRepository _progressRepo;
        private readonly IBossStatsRepository _bossStatsRepo;
        private readonly IWeaponRepository _weaponRepo;
        private readonly IBossRepository _bossRepo;
        private readonly IAnalysisEngine _analysisEngine;

        public PlayerProgressController(
            IPlayerProgressRepository progressRepo,
            IBossStatsRepository bossStatsRepo,
            IWeaponRepository weaponRepo,
            IBossRepository bossRepo,
            IAnalysisEngine analysisEngine)
        {
            _progressRepo = progressRepo;
            _bossStatsRepo = bossStatsRepo;
            _weaponRepo = weaponRepo;
            _bossRepo = bossRepo;
            _analysisEngine = analysisEngine;
        }

        [HttpPost]
        public async Task<ActionResult<PlayerProgress>> CreateProgress([FromBody] CreateProgressRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var progress = new PlayerProgress
            {
                UserId = userId,
                PlayerName = request.PlayerName,
                PSN_ID = request.PSN_ID,
                CurrentLevel = request.StartingLevel
            };

            var created = await _progressRepo.CreateAsync(progress);
            return CreatedAtAction(nameof(GetProgress), new { id = created.Id }, created);
        }

        [HttpGet("my-characters")]
        public async Task<ActionResult<List<PlayerProgress>>> GetMyCharacters()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var allProgress = await _progressRepo.GetAllAsync();
            var myProgress = allProgress.Where(p => p.UserId == userId).ToList();

            return Ok(myProgress);
        }

        [HttpGet("by-name/{playerName}")]
        public async Task<ActionResult<PlayerProgress>> GetProgressByName(string playerName)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var progress = await _progressRepo.GetByPlayerNameAsync(playerName);
            if (progress == null)
                return NotFound($"Player '{playerName}' not found");

            if (progress.UserId != userId)
                return Forbid();

            return Ok(progress);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerProgress>> GetProgress(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var progress = await _progressRepo.GetByIdAsync(id);
            if (progress == null)
                return NotFound($"Player progress '{id}' not found");

            if (progress.UserId != userId)
                return Forbid();

            return Ok(progress);
        }

    
        [HttpGet("{id}/detailed")]
        public async Task<ActionResult<DetailedProgressResponse>> GetDetailedProgress(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var progress = await _progressRepo.GetByIdAsync(id);
            if (progress == null)
                return NotFound($"Player progress '{id}' not found");

            if (progress.UserId != userId)
                return Forbid();

            // Get all weapons this player has collected
            var allWeapons = await _weaponRepo.GetAllAsync();
            var collectedWeapons = allWeapons
                .Where(w => progress.ObtainedWeaponIds.Contains(w.Id))
                .Select(w => new WeaponInfo
                {
                    Id = w.Id,
                    Name = w.Name,
                    Image = w.Image,
                    Category = w.Category,
                    Weight = w.Weight,
                    RequiredAttributes = w.RequiredAttributes.Select(r => new RequirementInfo
                    {
                        Name = r.Name,
                        Amount = r.Amount
                    }).ToList(),
                    Attack = w.Attack.Select(a => new StatInfo
                    {
                        Name = a.Name,
                        Amount = a.Amount
                    }).ToList()
                })
                .ToList();

            var allBosses = await _bossRepo.GetAllAsync();
            var defeatedBosses = allBosses
                .Where(b => progress.DefeatedBossIds.Contains(b.Id))
                .Select(b => new BossInfo
                {
                    Id = b.Id,
                    Name = b.Name,
                    Image = b.Image,
                    Region = b.Region,
                    Location = b.Location,
                    HealthPoints = b.HealthPoints
                })
                .ToList();

            return Ok(new DetailedProgressResponse
            {
                PlayerId = progress.Id,
                PlayerName = progress.PlayerName,
                PSN_ID = progress.PSN_ID,
                CurrentLevel = progress.CurrentLevel,
                
                TotalBossesDefeated = progress.DefeatedBossIds.Count,
                TotalWeaponsCollected = progress.ObtainedWeaponIds.Count,
                TotalLocationsVisited = progress.VisitedLocationIds.Count,
                
                DefeatedBosses = defeatedBosses,
                CollectedWeapons = collectedWeapons,
                
                TotalPlaytimeHours = progress.PlaytimeHours,
                TotalDeaths = progress.TotalDeaths,
                
                BossCompletionPercentage = Math.Round((progress.DefeatedBossIds.Count / 106.0) * 100, 1),
                WeaponCompletionPercentage = Math.Round((progress.ObtainedWeaponIds.Count / 306.0) * 100, 1)
            });
        }

        [HttpGet("{id}/death-stats")]
        public async Task<ActionResult<DeathStatisticsResponse>> GetDeathStatistics(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var progress = await _progressRepo.GetByIdAsync(id);
            if (progress == null)
                return NotFound($"Player progress '{id}' not found");

            if (progress.UserId != userId)
                return Forbid();

            // TODO: This needs BossFightAttempt tracking 
            return Ok(new DeathStatisticsResponse
            {
                TotalDeaths = progress.TotalDeaths,
                DeathsByBoss = new List<BossDeathStat>(),
                MostDifficultBoss = "Not yet tracked - enable boss fight tracking"
            });
        }

        [HttpGet("{id}/recommended-weapons/{bossName}")]
        public async Task<ActionResult<PlayerWeaponRecommendations>> GetRecommendedWeapons(
            string id, 
            string bossName)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var progress = await _progressRepo.GetByIdAsync(id);
            if (progress == null)
                return NotFound($"Player progress '{id}' not found");

            if (progress.UserId != userId)
                return Forbid();

            var bossStats = await _bossStatsRepo.GetByBossNameAsync(bossName);
            if (bossStats == null)
                return NotFound($"Boss '{bossName}' not found");

            var recommendations = await _analysisEngine.GetRecommendedWeaponsForPlayerAsync(bossStats, progress);

            return Ok(new PlayerWeaponRecommendations
            {
                BossName = bossStats.BossName,
                PlayerName = progress.PlayerName,
                TotalWeaponsOwned = progress.ObtainedWeaponIds.Count,
                Recommendations = recommendations.Take(10).ToList()
            });
        }

        [HttpPost("{id}/visit-location")]
        public async Task<ActionResult> VisitLocation(string id, [FromBody] LocationVisitRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var progress = await _progressRepo.GetByIdAsync(id);
            if (progress == null)
                return NotFound($"Player progress '{id}' not found");

            if (progress.UserId != userId)
                return Forbid();

            await _progressRepo.AddVisitedLocationAsync(id, request.LocationId);
            return Ok(new { message = $"Location '{request.LocationId}' marked as visited" });
        }

        [HttpPost("{id}/defeat-boss")]
        public async Task<ActionResult> DefeatBoss(string id, [FromBody] BossDefeatRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var progress = await _progressRepo.GetByIdAsync(id);
            if (progress == null)
                return NotFound($"Player progress '{id}' not found");

            if (progress.UserId != userId)
                return Forbid();

            await _progressRepo.AddDefeatedBossAsync(id, request.BossId);
            return Ok(new { message = $"Boss '{request.BossId}' marked as defeated" });
        }

        [HttpPost("{id}/obtain-weapon")]
        public async Task<ActionResult> ObtainWeapon(string id, [FromBody] WeaponObtainRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var progress = await _progressRepo.GetByIdAsync(id);
            if (progress == null)
                return NotFound($"Player progress '{id}' not found");

            if (progress.UserId != userId)
                return Forbid();

            await _progressRepo.AddObtainedWeaponAsync(id, request.WeaponId);
            return Ok(new { message = $"Weapon '{request.WeaponId}' marked as obtained" });
        }


        [HttpDelete("{id}/weapons/{weaponId}")]
        public async Task<ActionResult> RemoveWeapon(string id, string weaponId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var progress = await _progressRepo.GetByIdAsync(id);
            if (progress == null)
                return NotFound($"Player progress '{id}' not found");

            if (progress.UserId != userId)
                return Forbid();

            await _progressRepo.RemoveObtainedWeaponAsync(id, weaponId);
            return Ok(new { message = $"Weapon '{weaponId}' removed from collection" });
        }

        [HttpDelete("{id}/bosses/{bossId}")]
        public async Task<ActionResult> RemoveBoss(string id, string bossId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var progress = await _progressRepo.GetByIdAsync(id);
            if (progress == null)
                return NotFound($"Player progress '{id}' not found");

            if (progress.UserId != userId)
                return Forbid();

            await _progressRepo.RemoveDefeatedBossAsync(id, bossId);
            return Ok(new { message = $"Boss '{bossId}' removed from defeated list" });
        }
    }

    public class CreateProgressRequest
    {
        public string PlayerName { get; set; } = string.Empty;
        public string? PSN_ID { get; set; }
        public int StartingLevel { get; set; } = 1;
    }

    public class LocationVisitRequest
    {
        public string LocationId { get; set; } = string.Empty;
    }

    public class BossDefeatRequest
    {
        public string BossId { get; set; } = string.Empty;
    }

    public class WeaponObtainRequest
    {
        public string WeaponId { get; set; } = string.Empty;
    }

    // Response models
    public class PlayerWeaponRecommendations
    {
        public string BossName { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public int TotalWeaponsOwned { get; set; }
        public List<WeaponRecommendation> Recommendations { get; set; } = new();
    }

    public class DetailedProgressResponse
    {
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string? PSN_ID { get; set; }
        public int CurrentLevel { get; set; }
        
        // Counts
        public int TotalBossesDefeated { get; set; }
        public int TotalWeaponsCollected { get; set; }
        public int TotalLocationsVisited { get; set; }
        
        // Full detailed lists
        public List<BossInfo> DefeatedBosses { get; set; } = new();
        public List<WeaponInfo> CollectedWeapons { get; set; } = new();
        
        // Statistics
        public double TotalPlaytimeHours { get; set; }
        public int TotalDeaths { get; set; }
        
        // Completion percentages
        public double BossCompletionPercentage { get; set; }
        public double WeaponCompletionPercentage { get; set; }
    }

    public class BossInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string HealthPoints { get; set; } = string.Empty;
    }

    public class WeaponInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Weight { get; set; }
        public List<RequirementInfo> RequiredAttributes { get; set; } = new();
        public List<StatInfo> Attack { get; set; } = new();
    }

    public class RequirementInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Amount { get; set; }
    }

    public class StatInfo
    {
        public string Name { get; set; } = string.Empty;
        public double Amount { get; set; }
    }

    public class DeathStatisticsResponse
    {
        public int TotalDeaths { get; set; }
        public List<BossDeathStat> DeathsByBoss { get; set; } = new();
        public string MostDifficultBoss { get; set; } = string.Empty;
    }

    public class BossDeathStat
    {
        public string BossName { get; set; } = string.Empty;
        public int DeathCount { get; set; }
        public int AttemptsBeforeVictory { get; set; }
        public double AverageFightDurationSeconds { get; set; }
    }
}