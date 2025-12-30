namespace EldenRingSim.Models
{
    // Weapon recommendation with ownership status for a player
    public class WeaponRecommendation
    {
        public string WeaponId { get; set; } = string.Empty;
        public string WeaponName { get; set; } = string.Empty;
        public string WeaponCategory { get; set; } = string.Empty;
        public double EffectivenessScore { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        
        public bool AlreadyOwned { get; set; }
        public bool MeetsRequirements { get; set; }
        
    }
}