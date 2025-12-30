using EldenRingSim.DB;
using EldenRingSim.Models;
using EldenRingSim.Services;
using Microsoft.EntityFrameworkCore;

namespace EldenRingSim.Repositories
{
    public class PlayerProgressRepository : IPlayerProgressRepository
    {
        private readonly EldenRingContext _context;
        private readonly IRedisCacheService _cache;
        private readonly ILogger<PlayerProgressRepository> _logger;

        public PlayerProgressRepository(
            EldenRingContext context,
            IRedisCacheService cache,
            ILogger<PlayerProgressRepository> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PlayerProgress?> GetByIdAsync(string id)
        {
            var cacheKey = $"progress:id:{id}";
            
            var cached = await _cache.GetAsync<PlayerProgress>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: player progress '{Id}'", id);
                return cached;
            }

            var progress = await _context.PlayerProgress.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            
            if (progress != null)
            {
                await _cache.SetAsync(cacheKey, progress, TimeSpan.FromMinutes(30));
            }

            return progress;
        }

        public async Task<PlayerProgress?> GetByPlayerNameAsync(string playerName)
        {
            var cacheKey = $"progress:name:{playerName.ToLower()}";
            
            var cached = await _cache.GetAsync<PlayerProgress>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: player '{PlayerName}'", playerName);
                return cached;
            }

            var progress = await _context.PlayerProgress
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlayerName.ToLower() == playerName.ToLower());
            
            if (progress != null)
            {
                await _cache.SetAsync(cacheKey, progress, TimeSpan.FromMinutes(30));
            }

            return progress;
        }

        public async Task<List<PlayerProgress>> GetAllAsync()
        {
            var cacheKey = "progress:all";
            
            var cached = await _cache.GetAsync<List<PlayerProgress>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: all player progress");
                return cached;
            }

            var allProgress = await _context.PlayerProgress.AsNoTracking().ToListAsync();
            
            await _cache.SetAsync(cacheKey, allProgress, TimeSpan.FromMinutes(10));
            
            return allProgress;
        }

        public async Task<PlayerProgress> CreateAsync(PlayerProgress progress)
        {
            progress.CreatedAt = DateTime.UtcNow;
            progress.LastUpdated = DateTime.UtcNow;
            
            _context.PlayerProgress.Add(progress);
            await _context.SaveChangesAsync();
            
            await _cache.RemoveAsync("progress:all");
            
            _logger.LogInformation("✅ Created player progress for '{PlayerName}'", progress.PlayerName);
            return progress;
        }

        public async Task<PlayerProgress> UpdateAsync(PlayerProgress progress)
        {
            progress.LastUpdated = DateTime.UtcNow;
            
            _context.PlayerProgress.Update(progress);
            await _context.SaveChangesAsync();
            
            await _cache.RemoveAsync($"progress:id:{progress.Id}");
            await _cache.RemoveAsync($"progress:name:{progress.PlayerName.ToLower()}");
            await _cache.RemoveAsync("progress:all");
            
            _logger.LogInformation("✅ Updated player progress for '{PlayerName}'", progress.PlayerName);
            return progress;
        }

        public async Task DeleteAsync(string id)
        {
            var progress = await _context.PlayerProgress.FindAsync(id);
            if (progress != null)
            {
                _context.PlayerProgress.Remove(progress);
                await _context.SaveChangesAsync();
                
                await _cache.RemoveAsync($"progress:id:{id}");
                await _cache.RemoveAsync($"progress:name:{progress.PlayerName.ToLower()}");
                await _cache.RemoveAsync("progress:all");
                
                _logger.LogInformation("🗑️  Deleted player progress '{Id}'", id);
            }
        }

        public async Task AddVisitedLocationAsync(string progressId, string locationId)
        {
            // Clear any tracked entities to prevent conflicts
            _context.ChangeTracker.Clear();
            
            var progress = await _context.PlayerProgress
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == progressId);
                
            if (progress == null) return;

            if (!progress.VisitedLocationIds.Contains(locationId))
            {
                progress.VisitedLocationIds.Add(locationId);
                progress.LastUpdated = DateTime.UtcNow;
                
                _context.PlayerProgress.Attach(progress);
                _context.Entry(progress).State = EntityState.Modified;
                
                await _context.SaveChangesAsync();
                
                await _cache.RemoveAsync($"progress:id:{progressId}");
                await _cache.RemoveAsync($"progress:name:{progress.PlayerName.ToLower()}");
                await _cache.RemoveAsync("progress:all");
                
                _logger.LogInformation("📍 Player '{Name}' visited location '{LocationId}'", 
                    progress.PlayerName, locationId);
            }
        }

        public async Task AddDefeatedBossAsync(string progressId, string bossId)
        {
            // Clear any tracked entities to prevent conflicts
            _context.ChangeTracker.Clear();
            
            var progress = await _context.PlayerProgress
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == progressId);
                
            if (progress == null) return;

            if (!progress.DefeatedBossIds.Contains(bossId))
            {
                progress.DefeatedBossIds.Add(bossId);
                progress.LastUpdated = DateTime.UtcNow;
                
                _context.PlayerProgress.Attach(progress);
                _context.Entry(progress).State = EntityState.Modified;
                
                await _context.SaveChangesAsync();
                
                await _cache.RemoveAsync($"progress:id:{progressId}");
                await _cache.RemoveAsync($"progress:name:{progress.PlayerName.ToLower()}");
                await _cache.RemoveAsync("progress:all");
                
                _logger.LogInformation("⚔️  Player '{Name}' defeated boss '{BossId}'", 
                    progress.PlayerName, bossId);
            }
        }

        public async Task AddObtainedWeaponAsync(string progressId, string weaponId)
        {
            // Clear any tracked entities to prevent conflicts
            _context.ChangeTracker.Clear();
            
            var progress = await _context.PlayerProgress
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == progressId);
                
            if (progress == null) return;

            if (!progress.ObtainedWeaponIds.Contains(weaponId))
            {
                progress.ObtainedWeaponIds.Add(weaponId);
                progress.LastUpdated = DateTime.UtcNow;
                
                _context.PlayerProgress.Attach(progress);
                _context.Entry(progress).State = EntityState.Modified;
                
                await _context.SaveChangesAsync();
                
                await _cache.RemoveAsync($"progress:id:{progressId}");
                await _cache.RemoveAsync($"progress:name:{progress.PlayerName.ToLower()}");
                await _cache.RemoveAsync("progress:all");
                
                _logger.LogInformation("🗡️  Player '{Name}' obtained weapon '{WeaponId}'", 
                    progress.PlayerName, weaponId);
            }
        }
        public async Task RemoveDefeatedBossAsync(string progressId, string bossId)
        {
            // Clear any tracked entities to prevent conflicts
            _context.ChangeTracker.Clear();
            
            var progress = await _context.PlayerProgress
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == progressId);
                
            if (progress == null) return;

            if (progress.DefeatedBossIds.Contains(bossId))
            {
                progress.DefeatedBossIds.Remove(bossId);
                progress.LastUpdated = DateTime.UtcNow;
                
                _context.PlayerProgress.Attach(progress);
                _context.Entry(progress).State = EntityState.Modified;
                
                await _context.SaveChangesAsync();
                
                await _cache.RemoveAsync($"progress:id:{progressId}");
                await _cache.RemoveAsync($"progress:name:{progress.PlayerName.ToLower()}");
                await _cache.RemoveAsync("progress:all");
                
                _logger.LogInformation("❌ Player '{Name}' removed boss '{BossId}' from defeated list", 
                    progress.PlayerName, bossId);
            }
        }

        public async Task RemoveObtainedWeaponAsync(string progressId, string weaponId)
        {
            _context.ChangeTracker.Clear();
            
            var progress = await _context.PlayerProgress
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == progressId);
                
            if (progress == null) return;

            if (progress.ObtainedWeaponIds.Contains(weaponId))
            {
                progress.ObtainedWeaponIds.Remove(weaponId);
                progress.LastUpdated = DateTime.UtcNow;
                
                _context.PlayerProgress.Attach(progress);
                _context.Entry(progress).State = EntityState.Modified;
                
                await _context.SaveChangesAsync();
                
                await _cache.RemoveAsync($"progress:id:{progressId}");
                await _cache.RemoveAsync($"progress:name:{progress.PlayerName.ToLower()}");
                await _cache.RemoveAsync("progress:all");
                
                _logger.LogInformation("❌ Player '{Name}' removed weapon '{WeaponId}' from collection", 
                    progress.PlayerName, weaponId);
            }
        }
    }
}