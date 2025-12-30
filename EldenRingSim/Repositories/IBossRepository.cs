using EldenRingSim.DB;

namespace EldenRingSim.Repositories
{
    /// <summary>
    /// Boss-specific repository interface
    /// </summary>
    public interface IBossRepository : IRepository<Bosses>
    {
        Task<IEnumerable<Bosses>> GetByRegionAsync(string region);
        
        Task<IEnumerable<Bosses>> GetByLocationAsync(string location);
        
        Task<IEnumerable<Bosses>> GetByDifficultyAsync(bool ascending = false);
        
        Task<IEnumerable<Bosses>> GetBossesWithDropAsync(string itemName);
    }
}