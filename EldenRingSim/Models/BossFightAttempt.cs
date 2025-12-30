namespace EldenRingSim.Models
{
    // Fighting boss attempt 
    public class BossFightAttempt
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Links
        public string BossFightSessionId { get; set; } = string.Empty;
        public string PlayerProgressId { get; set; } = string.Empty;
        public string BossId { get; set; } = string.Empty;
        public string BossName { get; set; } = string.Empty;

        // Attempt Info
        public int AttemptNumber { get; set; }
        public string WeaponUsedId { get; set; } = string.Empty;
        public string WeaponUsedName { get; set; } = string.Empty;

        // Result
        public bool Victory { get; set; }
        public int TimeSpentSeconds { get; set; }

        // Optional Stats
        public int? DamageTaken { get; set; }
        public int? PlayerLevel { get; set; }

        // Timestamps
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        // Notes
        public string? Notes { get; set; }
    }
}
