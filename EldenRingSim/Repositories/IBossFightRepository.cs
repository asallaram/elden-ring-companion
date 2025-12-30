using EldenRingSim.Models;

namespace EldenRingSim.Repositories
{
    public interface IBossFightRepository
    {
        Task<BossFightSession> StartSessionAsync(string playerProgressId, string bossId, string bossName);
        Task<BossFightSession?> GetActiveSessionAsync(string playerProgressId, string bossId);
        Task<BossFightSession?> GetSessionByIdAsync(string sessionId);
        Task<BossFightSession> EndSessionAsync(string sessionId, bool victory);
        
        Task<BossFightAttempt> RecordAttemptAsync(BossFightAttempt attempt);
        Task<List<BossFightAttempt>> GetSessionAttemptsAsync(string sessionId);
        Task<List<BossFightAttempt>> GetAllAttemptsForBossAsync(string playerProgressId, string bossId);
        
        Task<List<BossFightSession>> GetPlayerSessionsAsync(string playerProgressId);
    }
}