using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EldenRingSim.DB
{
    // ---------------------- Base Class ----------------------
    public abstract class GameEntity
    {
        [Key]
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Image { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    // ---------------------- Ammo ----------------------
    public class Ammo : GameEntity
    {
        public string Type { get; set; } = null!;
        public List<AttackPowerEntry> AttackPower { get; set; } = new();
        public string Passive { get; set; } = null!;
    }
    public class AttackPowerEntry
    {
        public string Name { get; set; } = null!;
        public int Amount { get; set; }
    }

    // ---------------------- Armor ----------------------
    public class Armor : GameEntity
    {
        public string Category { get; set; } = null!;
        public List<NegationEntry> DmgNegation { get; set; } = new();
        public List<ResistanceEntry> Resistance { get; set; } = new();
        public double Weight { get; set; }
    }
    public class NegationEntry
    {
        public string Name { get; set; } = null!;
        public double Amount { get; set; }
    }
    public class ResistanceEntry
    {
        public string Name { get; set; } = null!;
        public double Amount { get; set; }
    }

    // ---------------------- Ashes of War ----------------------
    public class AshOfWar : GameEntity
    {
        public string Affinity { get; set; } = null!;
        public string Skill { get; set; } = null!;
        public AshOfWarDescription DescriptionDetails { get; set; } = null!;
    }
    public class AshOfWarDescription
    {
        public string DescriptionText { get; set; } = null!;
        public string Effect { get; set; } = null!;
    }

    // ---------------------- Bosses ----------------------
    public class Bosses : GameEntity
    {
        public string Region { get; set; } = null!;
        public string Location { get; set; } = null!;
        public List<BossesDropEntry> Drops { get; set; } = new();
        public string HealthPoints { get; set; } = null!;
    }
    public class BossesDropEntry
    {
        public string Name { get; set; } = null!;
        public string Amount { get; set; } = null!;
    }

    // ---------------------- Boss Stats (Enhanced Combat Data) ----------------------
public class BossStats : GameEntity
{
    public string BossName { get; set; } = null!;
    
    public int HealthPoints { get; set; }
    
    public double PhysicalResist { get; set; }
    public double MagicResist { get; set; }
    public double FireResist { get; set; }
    public double LightningResist { get; set; }
    public double HolyResist { get; set; }
    
    public bool BleedImmune { get; set; }
    public bool PoisonImmune { get; set; }
    public bool FrostImmune { get; set; }
    public bool ScarletRotImmune { get; set; }
    public bool MadnessImmune { get; set; }
    public bool SleepImmune { get; set; }
    
    public string Weakness { get; set; } = string.Empty; // e.g., "Strike, Fire"
    
    public int Tier { get; set; }
    
    public int AverageDamage { get; set; }
}

    // ---------------------- Classes ----------------------
    public class Classes : GameEntity
    {
        public List<StatEntry> Stats { get; set; } = new();
    }
    public class StatEntry
    {
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    // ---------------------- Creatures ----------------------
    public class Creatures : GameEntity
    {
        public string Location { get; set; } = null!;
        public List<CreaturesDropEntry> Drops { get; set; } = new();
    }
    public class CreaturesDropEntry
    {
        public string Name { get; set; } = null!;
        public string Amount { get; set; } = null!;
    }

    // ---------------------- Incantations ----------------------
    public class Incantations : GameEntity
    {
        public string Type { get; set; } = null!;
        public int Cost { get; set; }
        public int Slots { get; set; }
        public List<IncantationsEffectEntry> Effects { get; set; } = new();
        public List<IncantationsRequirementEntry> Requires { get; set; } = new();
    }
    public class IncantationsEffectEntry
    {
        public string Name { get; set; } = null!;
        public int Amount { get; set; }
    }
    public class IncantationsRequirementEntry
    {
        public string Name { get; set; } = null!;
        public int Amount { get; set; }
    }

    // ---------------------- Items ----------------------
    public class Items : GameEntity
    {
        public string Type { get; set; } = null!;
        public List<ItemsEffectEntry> Effect { get; set; } = new();
        public string ObtainedFrom { get; set; } = null!;
    }
    public class ItemsEffectEntry
    {
        public string Name { get; set; } = null!;
        public string Amount { get; set; } = string.Empty;

    }

    // ---------------------- Locations ----------------------
    public class Locations : GameEntity
    {
        public string Region { get; set; } = null!;
        public List<SubLocationEntry> SubLocations { get; set; } = new();
    }
    public class SubLocationEntry
    {
        public string Name { get; set; } = null!;
    }

    // ---------------------- NPCs ----------------------
    public class NPCs : GameEntity
    {
        public string Quote { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Role { get; set; } = null!;
    }

    // ---------------------- Shields ----------------------
    public class Shields : GameEntity
    {
        public List<NegationEntry> Attack { get; set; } = new();
        public List<NegationEntry> Defence { get; set; } = new();
        public List<ScalingEntry> ScalesWith { get; set; } = new();
        public List<RequirementEntry> RequiredAttributes { get; set; } = new();
        public string Category { get; set; } = null!;
        public double Weight { get; set; }
    }
    public class ScalingEntry
    {
        public string Name { get; set; } = null!;
        public string Scaling { get; set; } = null!;
    }
    public class RequirementEntry
    {
        public string Name { get; set; } = null!;
        public int Amount { get; set; }
    }

    // ---------------------- Sorceries ----------------------
    public class Sorceries : GameEntity
    {
        public string Type { get; set; } = null!;
        public int Cost { get; set; }
        public int Slots { get; set; }
        public List<SorceriesEffectEntry> Effects { get; set; } = new();
        public List<SorceriesRequirementEntry> Requires { get; set; } = new();
    }
    public class SorceriesEffectEntry
    {
        public string Name { get; set; } = null!;
        public string Amount { get; set; } = null!;
    }
    public class SorceriesRequirementEntry
    {
        public string Name { get; set; } = null!;
        public string Amount { get; set; } = null!;
    }

    // ---------------------- Spirits ----------------------
    public class Spirits : GameEntity
    {
        public int FpCost { get; set; }
        public int HpCost { get; set; }
        public List<SpiritEffectEntry> Effect { get; set; } = new();
    }
    public class SpiritEffectEntry
    {
        public string Name { get; set; } = null!;
        public string Amount { get; set; } = null!;
    }

    // ---------------------- Talismans ----------------------
    public class Talismans : GameEntity
    {
        public string Effect { get; set; } = null!;
    }

    // ---------------------- Weapons ----------------------
    public class Weapons : GameEntity
    {
        public List<NegationEntry> Attack { get; set; } = new();
        public List<NegationEntry> Defence { get; set; } = new();
        public List<ScalingEntry> ScalesWith { get; set; } = new();
        public List<RequirementEntry> RequiredAttributes { get; set; } = new();
        public string Category { get; set; } = null!;
        public double Weight { get; set; }
    }
}

// This file contains all the database models for the Elden Ring Simulator application.
// Each class represents a different entity in the game, with properties corresponding to the attributes of those entities. 
// Complex types are represented as separate classes and are used in lists to allow for multiple entries where applicable.
