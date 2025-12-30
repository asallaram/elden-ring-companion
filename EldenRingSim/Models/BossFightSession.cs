namespace EldenRingSim.Models
{
    /// Tracks an entire boss fight session (all attempts until victory or giving up)
    public class BossFightSession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        // Links
        public string PlayerProgressId { get; set; } = string.Empty;
        public string BossId { get; set; } = string.Empty;
        public string BossName { get; set; } = string.Empty;
        
        // Session Info
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool Victory { get; set; } = false;
        
        // Statistics
        public int TotalAttempts { get; set; } = 0;
        public List<string> WeaponsTriedIds { get; set; } = new();
        public int TotalTimeSpentSeconds { get; set; } = 0;
        
        // Notes
        public string? SessionNotes { get; set; }
        
        // Victory Details
        public string? VictoryWeaponId { get; set; }
        public string? VictoryWeaponName { get; set; }
        public int? VictoryAttemptNumber { get; set; }
    }
}