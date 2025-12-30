using System.ComponentModel.DataAnnotations;

namespace EldenRingSim.Models
{
    //Need to work on the map, could get from official wiki..
    public class PlayerProgress
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string PlayerName { get; set; } = string.Empty;
        public string? PSN_ID { get; set; }
        public int CurrentLevel { get; set; }
        public int CurrentRunes { get; set; }
        
        public List<string> VisitedLocationIds { get; set; } = new();
        public List<string> DefeatedBossIds { get; set; } = new();
        public List<string> ObtainedWeaponIds { get; set; } = new();
        public List<string> DiscoveredGraceIds { get; set; } = new(); 
        public List<string> UnlockedRegions { get; set; } = new(); 
        
        public int TotalDeaths { get; set; }
        public double PlaytimeHours { get; set; }
        public int GreatRunesCollected { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        public string? CurrentBuildId { get; set; }
        
        //These are useless for now(need to get all 22 main bossses ids)
        public bool HasBeatenMargit { get; set; }
        public bool HasBeatenGodrick { get; set; }
        public bool HasBeatenRadahn { get; set; }
        public bool HasBeatenMorgott { get; set; }
        public bool HasBeatenFireGiant { get; set; }
        public bool HasBeatenMalenia { get; set; }
        public bool HasBeatenMaliketh { get; set; }
        public bool HasBeatenEldenBeast { get; set; }

        
        public string? UserId { get; set; }

       
        public ApplicationUser? User { get; set; }
    }
}