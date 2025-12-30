using EldenRingSim.Models;

namespace EldenRingSim.Repositories
{
    public interface IPlayerProgressRepository
    {
        Task<PlayerProgress?> GetByIdAsync(string id);
        Task<PlayerProgress?> GetByPlayerNameAsync(string playerName);
        Task<List<PlayerProgress>> GetAllAsync(); // NEW: Get all progress records
        Task<PlayerProgress> CreateAsync(PlayerProgress progress);
        Task<PlayerProgress> UpdateAsync(PlayerProgress progress);
        Task DeleteAsync(string id);
        
        Task AddVisitedLocationAsync(string progressId, string locationId);
        Task AddDefeatedBossAsync(string progressId, string bossId);
        Task AddObtainedWeaponAsync(string progressId, string weaponId);
        Task RemoveDefeatedBossAsync(string progressId, string bossId);
        Task RemoveObtainedWeaponAsync(string progressId, string weaponId);
    }
}