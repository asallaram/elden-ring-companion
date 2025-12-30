using EldenRingSim.DB;
using EldenRingSim.Services;
using Microsoft.EntityFrameworkCore;

namespace EldenRingSim.Repositories
{
    public class BossRepository : Repository<Bosses>, IBossRepository
    {
        private readonly IRedisCacheService _cache;
        private readonly ILogger<BossRepository> _logger;

        public BossRepository(
            EldenRingContext context,
            IRedisCacheService cache,
            ILogger<BossRepository> logger) : base(context)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<IEnumerable<Bosses>> GetByRegionAsync(string region)
        {
            var cacheKey = $"bosses:region:{region.ToLower()}";
            
            var cached = await _cache.GetAsync<List<Bosses>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: {Count} bosses in region '{Region}'", cached.Count, region);
                return cached;
            }

            _logger.LogDebug("💾 Cache MISS: Loading bosses in region '{Region}' from database", region);
            
            var bosses = await _dbSet
                .Include(b => b.Drops)
                .Where(b => b.Region.ToLower().Contains(region.ToLower()))
                .ToListAsync();

            await _cache.SetAsync(cacheKey, bosses, TimeSpan.FromDays(365));
            _logger.LogDebug("✅ Cached {Count} bosses for region '{Region}' permanently", bosses.Count, region);

            return bosses;
        }

        public async Task<IEnumerable<Bosses>> GetByLocationAsync(string location)
        {
            var cacheKey = $"bosses:location:{location.ToLower()}";
            
            var cached = await _cache.GetAsync<List<Bosses>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: {Count} bosses at location '{Location}'", cached.Count, location);
                return cached;
            }

            _logger.LogDebug("💾 Cache MISS: Loading bosses at location '{Location}' from database", location);
            
            var bosses = await _dbSet
                .Include(b => b.Drops)
                .Where(b => b.Location.ToLower().Contains(location.ToLower()))
                .ToListAsync();

            await _cache.SetAsync(cacheKey, bosses, TimeSpan.FromDays(365));
            _logger.LogDebug("✅ Cached {Count} bosses for location '{Location}' permanently", bosses.Count, location);

            return bosses;
        }

        public async Task<IEnumerable<Bosses>> GetByDifficultyAsync(bool ascending = false)
        {
            var cacheKey = $"bosses:difficulty:{(ascending ? "asc" : "desc")}";
            
            var cached = await _cache.GetAsync<List<Bosses>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: {Count} bosses sorted by difficulty ({Order})", 
                    cached.Count, ascending ? "ascending" : "descending");
                return cached;
            }

            _logger.LogDebug("💾 Cache MISS: Loading bosses by difficulty ({Order}) from database",
                ascending ? "ascending" : "descending");
            
            var bosses = await _dbSet
                .Include(b => b.Drops)
                .ToListAsync();

            var sorted = bosses
                .Select(b => new
                {
                    Boss = b,
                    Health = ParseHealthPoints(b.HealthPoints)
                })
                .OrderBy(x => ascending ? x.Health : -x.Health)
                .Select(x => x.Boss)
                .ToList();

            await _cache.SetAsync(cacheKey, sorted, TimeSpan.FromDays(365));
            _logger.LogDebug("✅ Cached {Count} bosses by difficulty ({Order}) permanently", 
                sorted.Count, ascending ? "ascending" : "descending");

            return sorted;
        }

        public async Task<IEnumerable<Bosses>> GetBossesWithDropAsync(string itemName)
        {
            var allBosses = await _dbSet
                .Include(b => b.Drops)
                .ToListAsync();

            return allBosses.Where(boss =>
                boss.Drops.Any(drop => drop.Name.ToLower().Contains(itemName.ToLower()))
            );
        }

        private int ParseHealthPoints(string healthPoints)
        {
            if (string.IsNullOrEmpty(healthPoints))
                return 0;

            var cleaned = healthPoints.Replace(",", "").Trim();
            
            if (int.TryParse(cleaned, out int health))
                return health;

            return 0;
        }
    }
}