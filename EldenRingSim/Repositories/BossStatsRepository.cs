using EldenRingSim.DB;
using EldenRingSim.Services;
using Microsoft.EntityFrameworkCore;

namespace EldenRingSim.Repositories
{
    public class BossStatsRepository : Repository<BossStats>, IBossStatsRepository
    {
        private readonly IRedisCacheService _cache;
        private readonly ILogger<BossStatsRepository> _logger;

        public BossStatsRepository(
            EldenRingContext context,
            IRedisCacheService cache,
            ILogger<BossStatsRepository> logger) : base(context)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<BossStats?> GetByBossNameAsync(string bossName)
        {
            var cacheKey = $"bossstats:name:{bossName.ToLower()}";
            
            var cached = await _cache.GetAsync<BossStats>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: boss '{BossName}' (skipped database)", bossName);
                return cached;
            }

            _logger.LogDebug("💾 Cache MISS: boss '{BossName}' (querying database)", bossName);
            
            var bossStats = await _context.BossStats
                .FirstOrDefaultAsync(b => b.BossName.ToLower() == bossName.ToLower());

            if (bossStats != null)
            {
                await _cache.SetAsync(cacheKey, bossStats, TimeSpan.FromDays(365));
                _logger.LogDebug("✅ Cached boss '{BossName}' permanently", bossName);
            }

            return bossStats;
        }

        public async Task<List<BossStats>> GetByTierAsync(int tier)
        {
            var cacheKey = $"bossstats:tier:{tier}";
            
            var cached = await _cache.GetAsync<List<BossStats>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: {Count} bosses in tier {Tier}", cached.Count, tier);
                return cached;
            }

            _logger.LogDebug("💾 Cache MISS: Loading tier {Tier} from database", tier);
            
            var bossStats = await _context.BossStats
                .Where(b => b.Tier == tier)
                .OrderBy(b => b.BossName)
                .ToListAsync();

            await _cache.SetAsync(cacheKey, bossStats, TimeSpan.FromDays(365));
            _logger.LogDebug("✅ Cached {Count} bosses for tier {Tier} permanently", bossStats.Count, tier);

            return bossStats;
        }

        public async Task<List<BossStats>> GetByWeaknessAsync(string weakness)
        {
            var cacheKey = $"bossstats:weakness:{weakness.ToLower()}";
            
            var cached = await _cache.GetAsync<List<BossStats>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: {Count} bosses weak to '{Weakness}'", cached.Count, weakness);
                return cached;
            }

            _logger.LogDebug("💾 Cache MISS: Loading bosses weak to '{Weakness}' from database", weakness);
            
            var bossStats = await _context.BossStats
                .Where(b => b.Weakness.ToLower().Contains(weakness.ToLower()))
                .OrderBy(b => b.Tier)
                .ToListAsync();

            await _cache.SetAsync(cacheKey, bossStats, TimeSpan.FromDays(365));
            _logger.LogDebug("✅ Cached {Count} bosses weak to '{Weakness}' permanently", bossStats.Count, weakness);

            return bossStats;
        }
    }
}