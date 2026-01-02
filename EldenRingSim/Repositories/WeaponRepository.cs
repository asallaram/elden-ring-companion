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
            _logger.LogDebug("Loading weapon '{Id}' from database", id);
            
            var weapon = await _dbSet
                .Include(w => w.Attack)
                .Include(w => w.Defence)
                .Include(w => w.ScalesWith)
                .Include(w => w.RequiredAttributes)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id);

            return weapon;
        }

        public async Task<Weapons?> GetByNameAsync(string name)
        {
            _logger.LogDebug("Loading weapon '{Name}' from database", name);
            
            var weapon = await _dbSet
                .Include(w => w.Attack)
                .Include(w => w.Defence)
                .Include(w => w.ScalesWith)
                .Include(w => w.RequiredAttributes)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Name.ToLower() == name.ToLower());

            return weapon;
        }

        public async Task<IEnumerable<Weapons>> GetByCategoryAsync(string category)
        {
            _logger.LogDebug("Loading category '{Category}' from database", category);
            
            var weapons = await _dbSet
                .Include(w => w.Attack)
                .Include(w => w.Defence)
                .Include(w => w.ScalesWith)
                .Include(w => w.RequiredAttributes)
                .AsNoTracking()
                .Where(w => w.Category == category)
                .ToListAsync();

            return weapons;
        }

        public async Task<IEnumerable<Weapons>> GetByWeightRangeAsync(double minWeight, double maxWeight)
        {
            return await _dbSet
                .Include(w => w.Attack)
                .Include(w => w.ScalesWith)
                .AsNoTracking()
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
                .AsNoTracking()
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