using EldenRingSim.DB;
using EldenRingSim.Services;
using Microsoft.EntityFrameworkCore;

namespace EldenRingSim.Repositories
{
    public class WeaponRepository : Repository<Weapons>, IWeaponRepository
    {
        private readonly IRedisCacheService _cache;
        private readonly ILogger<WeaponRepository> _logger;

        public WeaponRepository(
            EldenRingContext context,
            IRedisCacheService cache,
            ILogger<WeaponRepository> logger) : base(context)
        {
            _cache = cache;
            _logger = logger;
        }

        public new async Task<List<Weapons>> GetAllAsync()
        {
            _logger.LogDebug("Loading all weapons from database (no cache)");
            
            var weapons = await _dbSet
                .Include(w => w.Attack)
                .Include(w => w.Defence)
                .Include(w => w.ScalesWith)
                .Include(w => w.RequiredAttributes)
                .AsNoTracking()
                .ToListAsync();

            _logger.LogDebug("Loaded {Count} weapons", weapons.Count);

            return weapons;
        }

        public new async Task<Weapons?> GetByIdAsync(string id)
        {
            var cacheKey = $"weapon:id:{id.ToLower()}";
            
            var cached = await _cache.GetAsync<Weapons>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: weapon ID '{Id}'", id);
                return cached;
            }

            _logger.LogDebug("💾 Cache MISS: Loading weapon '{Id}' from database", id);
            
            var weapon = await _dbSet
                .Include(w => w.Attack)
                .Include(w => w.Defence)
                .Include(w => w.ScalesWith)
                .Include(w => w.RequiredAttributes)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (weapon != null)
            {
                await _cache.SetAsync(cacheKey, weapon, TimeSpan.FromHours(1));
                _logger.LogDebug("✅ Cached weapon '{Id}'", id);
            }

            return weapon;
        }

        public async Task<Weapons?> GetByNameAsync(string name)
        {
            var cacheKey = $"weapon:name:{name.ToLower()}";
            
            var cached = await _cache.GetAsync<Weapons>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: weapon '{Name}' (skipped database)", name);
                return cached;
            }

            _logger.LogDebug("💾 Cache MISS: weapon '{Name}' (querying database)", name);
            
            var weapon = await _dbSet
                .Include(w => w.Attack)
                .Include(w => w.Defence)
                .Include(w => w.ScalesWith)
                .Include(w => w.RequiredAttributes)
                .FirstOrDefaultAsync(w => w.Name.ToLower() == name.ToLower());

            if (weapon != null)
            {
                await _cache.SetAsync(cacheKey, weapon, TimeSpan.FromHours(1));
                _logger.LogDebug("✅ Cached weapon '{Name}' for 1 hour", name);
            }

            return weapon;
        }

        public async Task<IEnumerable<Weapons>> GetByCategoryAsync(string category)
        {
            var cacheKey = $"weapons:category:{category.ToLower()}";
            
            var cached = await _cache.GetAsync<List<Weapons>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("⚡ Cache HIT: {Count} weapons in category '{Category}'", cached.Count, category);
                return cached;
            }

            _logger.LogDebug("💾 Cache MISS: Loading category '{Category}' from database", category);
            
            var weapons = await _dbSet
                .Include(w => w.Attack)
                .Include(w => w.Defence)
                .Include(w => w.ScalesWith)
                .Include(w => w.RequiredAttributes)
                .Where(w => w.Category == category)
                .ToListAsync();

            await _cache.SetAsync(cacheKey, weapons, TimeSpan.FromHours(1));
            _logger.LogDebug("✅ Cached {Count} weapons for category '{Category}'", weapons.Count, category);

            return weapons;
        }

        public async Task<IEnumerable<Weapons>> GetByWeightRangeAsync(double minWeight, double maxWeight)
        {
            return await _dbSet
                .Include(w => w.Attack)
                .Include(w => w.ScalesWith)
                .Where(w => w.Weight >= minWeight && w.Weight <= maxWeight)
                .OrderBy(w => w.Weight)
                .ToListAsync();
        }

        public async Task<IEnumerable<Weapons>> GetWeaponsMeetingRequirementsAsync(
            int strength, int dexterity, int intelligence, int faith)
        {
            var allWeapons = await _dbSet
                .Include(w => w.Attack)
                .Include(w => w.ScalesWith)
                .Include(w => w.RequiredAttributes)
                .ToListAsync();

            return allWeapons.Where(weapon =>
            {
                foreach (var req in weapon.RequiredAttributes)
                {
                    var requiredAmount = req.Amount;
                    var playerStat = req.Name.ToLower() switch
                    {
                        "str" or "strength" => strength,
                        "dex" or "dexterity" => dexterity,
                        "int" or "intelligence" => intelligence,
                        "fai" or "faith" => faith,
                        _ => 0
                    };

                    if (playerStat < requiredAmount)
                        return false;
                }
                return true;
            });
        }
    }
}