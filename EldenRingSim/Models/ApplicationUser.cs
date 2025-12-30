using Microsoft.AspNetCore.Identity;

namespace EldenRingSim.Models
{
    // Making account(auth and profile)
    
    public class ApplicationUser : IdentityUser
    {
        
        public string? PsnId { get; set; }

        
        public string? Playstyle { get; set; }

        
        public string? FavoriteBuild { get; set; }

       
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

       
        public DateTime? LastLoginAt { get; set; }

        
        public ICollection<PlayerProgress> PlayerProgresses { get; set; } = new List<PlayerProgress>();
    }
}