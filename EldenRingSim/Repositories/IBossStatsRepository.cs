using EldenRingSim.DB;

namespace EldenRingSim.Repositories
{
    public interface IBossStatsRepository : IRepository<BossStats>
    {
        Task<BossStats?> GetByBossNameAsync(string bossName);
        Task<List<BossStats>> GetByTierAsync(int tier);
        Task<List<BossStats>> GetByWeaknessAsync(string weakness);
    }
}