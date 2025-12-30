using EldenRingSim.Models;
using EldenRingSim.Repositories;
using Microsoft.AspNetCore.Mvc;


// Boss fight session and attempt tracking endpoints for managing combat encounters and player performance history

namespace EldenRingSim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BossFightController : ControllerBase
    {
        private readonly IBossFightRepository _fightRepo;
        private readonly IPlayerProgressRepository _progressRepo;

        public BossFightController(
            IBossFightRepository fightRepo,
            IPlayerProgressRepository progressRepo)
        {
            _fightRepo = fightRepo;
            _progressRepo = progressRepo;
        }

      
        [HttpPost("start")]
        public async Task<ActionResult<BossFightSession>> StartFight([FromBody] StartFightRequest request)
        {
            var session = await _fightRepo.StartSessionAsync(
                request.PlayerProgressId, 
                request.BossId, 
                request.BossName);

            return Ok(session);
        }

       
        [HttpPost("attempt")]
        public async Task<ActionResult<BossFightAttempt>> RecordAttempt([FromBody] RecordAttemptRequest request)
        {
            var session = await _fightRepo.GetActiveSessionAsync(
                request.PlayerProgressId, 
                request.BossId);

            if (session == null)
                return BadRequest("No active session found. Start a session first.");

            var attempt = new BossFightAttempt
{
    PlayerProgressId = request.PlayerProgressId,
    BossId = request.BossId,
    BossName = request.BossName,
    WeaponUsedId = request.WeaponUsedId,
    WeaponUsedName = request.WeaponUsedName,
    Victory = request.Victory,
    TimeSpentSeconds = request.TimeSpentSeconds,
    DamageTaken = request.DamageTaken,
    PlayerLevel = request.PlayerLevel,
    Notes = request.Notes
};


            var recorded = await _fightRepo.RecordAttemptAsync(attempt);

            // If victory, end the session
            if (request.Victory)
            {
                await _fightRepo.EndSessionAsync(session.Id, true);
                
                // Mark boss as defeated in player progress
                await _progressRepo.AddDefeatedBossAsync(request.PlayerProgressId, request.BossId);
            }

            return Ok(recorded);
        }

       
        [HttpPost("{sessionId}/end")]
        public async Task<ActionResult<BossFightSession>> EndSession(string sessionId)
        {
            var session = await _fightRepo.EndSessionAsync(sessionId, false);
            return Ok(session);
        }

       
        [HttpGet("session/{sessionId}/attempts")]
        public async Task<ActionResult<List<BossFightAttempt>>> GetSessionAttempts(string sessionId)
        {
            var attempts = await _fightRepo.GetSessionAttemptsAsync(sessionId);
            return Ok(attempts);
        }

       
        [HttpGet("history/{playerProgressId}/{bossId}")]
        public async Task<ActionResult<BossFightHistory>> GetFightHistory(
            string playerProgressId, 
            string bossId)
        {
            var attempts = await _fightRepo.GetAllAttemptsForBossAsync(playerProgressId, bossId);
            var sessions = await _fightRepo.GetPlayerSessionsAsync(playerProgressId);
            var bossSessions = sessions.Where(s => s.BossId == bossId).ToList();

            var history = new BossFightHistory
            {
                PlayerProgressId = playerProgressId,
                BossId = bossId,
                TotalAttempts = attempts.Count,
                TotalSessions = bossSessions.Count,
                Victories = bossSessions.Count(s => s.Victory),
                Attempts = attempts,
                Sessions = bossSessions
            };

            return Ok(history);
        }

       
        [HttpGet("player/{playerProgressId}/sessions")]
        public async Task<ActionResult<List<BossFightSession>>> GetPlayerSessions(string playerProgressId)
        {
            var sessions = await _fightRepo.GetPlayerSessionsAsync(playerProgressId);
            return Ok(sessions);
        }
    }

    public class StartFightRequest
    {
        public string PlayerProgressId { get; set; } = string.Empty;
        public string BossId { get; set; } = string.Empty;
        public string BossName { get; set; } = string.Empty;
    }

    public class RecordAttemptRequest
    {
        public string PlayerProgressId { get; set; } = string.Empty;
        public string BossId { get; set; } = string.Empty;
        public string BossName { get; set; } = string.Empty;
        public string WeaponUsedId { get; set; } = string.Empty;
        public string WeaponUsedName { get; set; } = string.Empty;
        public bool Victory { get; set; }
        public int TimeSpentSeconds { get; set; }
        public int? DamageTaken { get; set; }
        public int? PlayerLevel { get; set; }
        public string? Notes { get; set; }
    }

    public class BossFightHistory
    {
        public string PlayerProgressId { get; set; } = string.Empty;
        public string BossId { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public int TotalSessions { get; set; }
        public int Victories { get; set; }
        public List<BossFightAttempt> Attempts { get; set; } = new();
        public List<BossFightSession> Sessions { get; set; } = new();
    }
}