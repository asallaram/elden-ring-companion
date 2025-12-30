namespace EldenRingSim.Models
{
    
    public class PlayerBuild
    {
        public int Level { get; set; }
        
        public int Vigor { get; set; }      // HP
        public int Mind { get; set; }       // FP (magic points)
        public int Endurance { get; set; }  // Stamina
        public int Strength { get; set; }   // STR scaling
        public int Dexterity { get; set; }  // DEX scaling
        public int Intelligence { get; set; } // INT scaling
        public int Faith { get; set; }      // FAI scaling
        public int Arcane { get; set; }     // ARC scaling (bleed/discovery)
        
        public string WeaponId { get; set; } = string.Empty;
        public string? OffhandWeaponId { get; set; }  // Optional second weapon
        
        public string? HelmetId { get; set; }
        public string? ChestArmorId { get; set; }
        public string? GauntletsId { get; set; }
        public string? LeggingsId { get; set; }
        
        public List<string> TalismanIds { get; set; } = new();
    }
}