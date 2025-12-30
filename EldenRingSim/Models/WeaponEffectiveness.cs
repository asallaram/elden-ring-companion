namespace EldenRingSim.Models
{
    /// Analysis result showing how effective a weapon is for a specific build
    public class WeaponEffectiveness
    {
        public string WeaponId { get; set; } = string.Empty;
        public string WeaponName { get; set; } = string.Empty;
        
        // Can the player even use this weapon?
        public bool MeetsRequirements { get; set; }
        public List<string> MissingRequirements { get; set; } = new();
        
        public double PhysicalDamage { get; set; }
        public double MagicDamage { get; set; }
        public double FireDamage { get; set; }
        public double LightningDamage { get; set; }
        public double HolyDamage { get; set; }
        public double TotalDamage { get; set; }
        
        public double StrengthScaling { get; set; }
        public double DexterityScaling { get; set; }
        public double IntelligenceScaling { get; set; }
        public double FaithScaling { get; set; }
        public double ArcaneScaling { get; set; }
        
        public double EffectivenessScore { get; set; }
        
        public string Recommendation { get; set; } = string.Empty;
    }
}