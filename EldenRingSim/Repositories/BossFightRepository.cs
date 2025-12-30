using EldenRingSim.DB;
using EldenRingSim.Models;
using EldenRingSim.Services;
using Microsoft.EntityFrameworkCore;

namespace EldenRingSim.Repositories
{
    public class BossFightRepository : IBossFightRepository
    {
        private readonly EldenRingContext _context;
        private readonly IRedisCacheService _cache;
        private readonly ILogger<BossFightRepository> _logger;

        public BossFightRepository(
            EldenRingContext context,
            IRedisCacheService cache,
            ILogger<BossFightRepository> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<BossFightSession> StartSessionAsync(string playerProgressId, string bossId, string bossName)
        {
            var existing = await GetActiveSessionAsync(playerProgressId, bossId);
            if (existing != null) return existing;

            var session = new BossFightSession
            {
                PlayerProgressId = playerProgressId,
                BossId = bossId,
                BossName = bossName,
                StartedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.BossFightSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<BossFightSession?> GetActiveSessionAsync(string playerProgressId, string bossId)
        {
            return await _context.BossFightSessions
                .FirstOrDefaultAsync(s =>
                    s.PlayerProgressId == playerProgressId &&
                    s.BossId == bossId &&
                    s.IsActive);
        }

        public async Task<BossFightSession?> GetSessionByIdAsync(string sessionId)
        {
            var cached = await _cache.GetAsync<BossFightSession>($"session:{sessionId}");
            if (cached != null) return cached;

            var session = await _context.BossFightSessions.FindAsync(sessionId);
            if (session != null)
                await _cache.SetAsync($"session:{sessionId}", session, TimeSpan.FromMinutes(30));

            return session;
        }

        public async Task<BossFightSession> EndSessionAsync(string sessionId, bool victory)
        {
            var session = await _context.BossFightSessions.FindAsync(sessionId);
            if (session == null) throw new ArgumentException("Session not found");

            session.IsActive = false;
            session.EndedAt = DateTime.UtcNow;
            session.Victory = victory;

            await _context.SaveChangesAsync();
            await _cache.RemoveAsync($"session:{sessionId}");
            return session;
        }

        public async Task<BossFightAttempt> RecordAttemptAsync(BossFightAttempt attempt)
        {
            var session = await GetActiveSessionAsync(attempt.PlayerProgressId, attempt.BossId);
            if (session == null) throw new InvalidOperationException("No active session");

            attempt.AttemptNumber = session.TotalAttempts + 1;
            attempt.BossFightSessionId = session.Id;

            _context.BossFightAttempts.Add(attempt);

            session.TotalAttempts++;
            session.TotalTimeSpentSeconds += attempt.TimeSpentSeconds;

            if (!session.WeaponsTriedIds.Contains(attempt.WeaponUsedId))
                session.WeaponsTriedIds.Add(attempt.WeaponUsedId);

            if (attempt.Victory)
            {
                session.Victory = true;
                session.IsActive = false;
                session.EndedAt = DateTime.UtcNow;
                session.VictoryWeaponId = attempt.WeaponUsedId;
                session.VictoryWeaponName = attempt.WeaponUsedName;
                session.VictoryAttemptNumber = attempt.AttemptNumber;
            }

            await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task<List<BossFightAttempt>> GetSessionAttemptsAsync(string sessionId)
        {
            return await _context.BossFightAttempts
                .Where(a => a.BossFightSessionId == sessionId)
                .OrderBy(a => a.AttemptNumber)
                .ToListAsync();
        }

        public async Task<List<BossFightAttempt>> GetAllAttemptsForBossAsync(string playerProgressId, string bossId)
        {
            return await _context.BossFightAttempts
                .Where(a => a.PlayerProgressId == playerProgressId && a.BossId == bossId)
                .OrderBy(a => a.AttemptedAt)
                .ToListAsync();
        }

        public async Task<List<BossFightSession>> GetPlayerSessionsAsync(string playerProgressId)
        {
            return await _context.BossFightSessions
                .Where(s => s.PlayerProgressId == playerProgressId)
                .OrderByDescending(s => s.StartedAt)
                .ToListAsync();
        }
    }
}
