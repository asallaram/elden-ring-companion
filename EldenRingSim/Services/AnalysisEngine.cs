using EldenRingSim.DB;
using EldenRingSim.Models;
using EldenRingSim.Repositories;

namespace EldenRingSim.Services
{
    /// <summary>
    /// Unified Analysis Engine - handles weapon analysis AND boss matchup analysis
    /// </summary>
    public class AnalysisEngine : IAnalysisEngine
    {
        private readonly IWeaponRepository _weaponRepo;

        public AnalysisEngine(IWeaponRepository weaponRepo)
        {
            _weaponRepo = weaponRepo;
        }

        // ============= EXISTING WEAPON ANALYSIS METHODS (UNCHANGED) =============

        public async Task<WeaponEffectiveness> AnalyzeWeaponAsync(string weaponId, PlayerBuild build)
        {
            var weapon = await _weaponRepo.GetByIdAsync(weaponId);
            if (weapon == null)
            {
                throw new ArgumentException($"Weapon with ID {weaponId} not found");
            }

            var effectiveness = new WeaponEffectiveness
            {
                WeaponId = weapon.Id,
                WeaponName = weapon.Name
            };

            CheckRequirements(weapon, build, effectiveness);

            if (!effectiveness.MeetsRequirements)
            {
                effectiveness.EffectivenessScore = 0;
                effectiveness.Recommendation = $"Cannot use - Missing: {string.Join(", ", effectiveness.MissingRequirements)}";
                return effectiveness;
            }

            CalculateTotalDamage(weapon, build, effectiveness);
            effectiveness.EffectivenessScore = CalculateScore(effectiveness.TotalDamage);
            GenerateRecommendation(effectiveness);

            return effectiveness;
        }

        public async Task<List<WeaponEffectiveness>> FindBestWeaponsAsync(PlayerBuild build, int topCount = 10)
        {
            var usableWeapons = await _weaponRepo.GetWeaponsMeetingRequirementsAsync(
                build.Strength, build.Dexterity, build.Intelligence, build.Faith);

            var analyses = new List<WeaponEffectiveness>();
            
            foreach (var weapon in usableWeapons)
            {
                var analysis = await AnalyzeWeaponAsync(weapon.Id, build);
                analyses.Add(analysis);
            }

            return analyses
                .Where(a => a.MeetsRequirements)
                .OrderByDescending(a => a.TotalDamage)
                .Take(topCount)
                .ToList();
        }

        public async Task<List<WeaponEffectiveness>> CompareWeaponsAsync(List<string> weaponIds, PlayerBuild build)
        {
            var comparisons = new List<WeaponEffectiveness>();
            
            foreach (var weaponId in weaponIds)
            {
                var analysis = await AnalyzeWeaponAsync(weaponId, build);
                comparisons.Add(analysis);
            }

            return comparisons.OrderByDescending(c => c.TotalDamage).ToList();
        }

        // ============= NEW BOSS MATCHUP METHODS =============

        public double CalculateWinProbability(PlayerBuild build, BossStats boss, Weapons weapon)
        {
            double probability = 0.5;

            // Player level vs boss tier
            int expectedLevel = boss.Tier * 30;
            double levelAdvantage = (build.Level - expectedLevel) / 100.0;
            probability += levelAdvantage * 0.2;

            // Weapon effectiveness
            double weaponScore = CalculateWeaponEffectivenessAgainstBoss(weapon, boss);
            probability += (weaponScore - 50) / 100.0;

            // Player stats
            double statBonus = CalculateStatBonus(build);
            probability += statBonus * 0.1;

            return Math.Max(0.05, Math.Min(0.95, probability));
        }

        public double CalculateWeaponEffectivenessAgainstBoss(Weapons weapon, BossStats boss)
        {
            double score = 50;

            string attackType = DetermineAttackType(weapon.Category);
            string weakness = boss.Weakness.ToLower();
            
            // Attack type match
            if (weakness.Contains("slash") && attackType == "Slash")
                score += 20;
            if (weakness.Contains("strike") && attackType == "Strike")
                score += 20;
            if (weakness.Contains("pierce") && attackType == "Pierce")
                score += 20;

            // Elemental damage
            foreach (var attack in weapon.Attack)
            {
                string dmgType = attack.Name.ToLower();
                
                if (weakness.Contains("fire") && dmgType == "fire" && attack.Amount > 0)
                    score += 15;
                if (weakness.Contains("magic") && (dmgType == "mag" || dmgType == "magic") && attack.Amount > 0)
                    score += 15;
                if (weakness.Contains("lightning") && (dmgType == "ligt" || dmgType == "lightning") && attack.Amount > 0)
                    score += 15;
                if (weakness.Contains("holy") && dmgType == "holy" && attack.Amount > 0)
                    score += 15;
            }

            // Status effects
            if (weakness.Contains("bleed"))
                score += 12;
            if (weakness.Contains("frost"))
                score += 12;
            if (weakness.Contains("rot") || weakness.Contains("scarlet"))
                score += 12;

            // Boss immunities
            if (boss.BleedImmune && weakness.Contains("bleed"))
                score -= 20;
            if (boss.FrostImmune && weakness.Contains("frost"))
                score -= 20;
            if (boss.PoisonImmune && weakness.Contains("poison"))
                score -= 15;
            if (boss.ScarletRotImmune && weakness.Contains("rot"))
                score -= 20;

            // Resistances
            double avgResistance = (boss.PhysicalResist + boss.MagicResist + 
                                   boss.FireResist + boss.LightningResist + 
                                   boss.HolyResist) / 5.0;
            score -= avgResistance * 0.3;

            // High base damage bonus
            foreach (var attack in weapon.Attack)
            {
                if (attack.Name.ToLower() == "phy" || attack.Name.ToLower() == "physical")
                {
                    if (attack.Amount > 120)
                        score += 10;
                    else if (attack.Amount > 100)
                        score += 5;
                    break;
                }
            }

            return Math.Max(0, Math.Min(100, score));
        }

        public async Task<List<WeaponEffectiveness>> GetBestWeaponsForBossAsync(BossStats boss)
        {
            var allWeapons = await _weaponRepo.GetAllAsync();
            var effectiveness = new List<WeaponEffectiveness>();

            foreach (var weapon in allWeapons)
            {
                double score = CalculateWeaponEffectivenessAgainstBoss(weapon, boss);
                
                effectiveness.Add(new WeaponEffectiveness
                {
                    WeaponId = weapon.Id,
                    WeaponName = weapon.Name,
                    EffectivenessScore = score,
                    Recommendation = GenerateReasoningForBoss(weapon, boss, score)
                });
            }

            return effectiveness.OrderByDescending(w => w.EffectivenessScore).ToList();
        }

        // ============= EXISTING PRIVATE METHODS (UNCHANGED) =============

        private void CheckRequirements(Weapons weapon, PlayerBuild build, WeaponEffectiveness effectiveness)
        {
            effectiveness.MeetsRequirements = true;

            foreach (var req in weapon.RequiredAttributes)
            {
                var playerStat = GetPlayerStat(req.Name, build);
                
                if (playerStat < req.Amount)
                {
                    effectiveness.MeetsRequirements = false;
                    effectiveness.MissingRequirements.Add($"{req.Name} {req.Amount}");
                }
            }
        }

        private void CalculateTotalDamage(Weapons weapon, PlayerBuild build, WeaponEffectiveness effectiveness)
        {
            foreach (var attack in weapon.Attack)
            {
                switch (attack.Name.ToLower())
                {
                    case "phy":
                    case "physical":
                        effectiveness.PhysicalDamage = attack.Amount;
                        break;
                    case "mag":
                    case "magic":
                        effectiveness.MagicDamage = attack.Amount;
                        break;
                    case "fire":
                        effectiveness.FireDamage = attack.Amount;
                        break;
                    case "ligt":
                    case "lightning":
                        effectiveness.LightningDamage = attack.Amount;
                        break;
                    case "holy":
                        effectiveness.HolyDamage = attack.Amount;
                        break;
                }
            }

            foreach (var scaling in weapon.ScalesWith)
            {
                var stat = GetPlayerStat(scaling.Name, build);
                var multiplier = GetScalingMultiplier(scaling.Scaling);
                
                var scalingPercent = multiplier * 100;
                switch (scaling.Name.ToLower())
                {
                    case "str":
                    case "strength":
                        effectiveness.StrengthScaling = scalingPercent;
                        break;
                    case "dex":
                    case "dexterity":
                        effectiveness.DexterityScaling = scalingPercent;
                        break;
                    case "int":
                    case "intelligence":
                        effectiveness.IntelligenceScaling = scalingPercent;
                        break;
                    case "fai":
                    case "faith":
                        effectiveness.FaithScaling = scalingPercent;
                        break;
                    case "arc":
                    case "arcane":
                        effectiveness.ArcaneScaling = scalingPercent;
                        break;
                }

                var bonusDamage = (stat / 100.0) * effectiveness.PhysicalDamage * multiplier;
                
                switch (scaling.Name.ToLower())
                {
                    case "str":
                    case "strength":
                    case "dex":
                    case "dexterity":
                        effectiveness.PhysicalDamage += bonusDamage;
                        break;
                    case "int":
                    case "intelligence":
                        effectiveness.MagicDamage += bonusDamage;
                        break;
                    case "fai":
                    case "faith":
                        effectiveness.HolyDamage += bonusDamage;
                        break;
                    case "arc":
                    case "arcane":
                        effectiveness.PhysicalDamage += bonusDamage * 0.5;
                        break;
                }
            }

            effectiveness.TotalDamage = effectiveness.PhysicalDamage +
                                       effectiveness.MagicDamage +
                                       effectiveness.FireDamage +
                                       effectiveness.LightningDamage +
                                       effectiveness.HolyDamage;
        }

        private double CalculateScore(double totalDamage)
        {
            return Math.Min(100, (totalDamage / 400.0) * 100);
        }

        private void GenerateRecommendation(WeaponEffectiveness effectiveness)
        {
            var damage = effectiveness.TotalDamage;
            var score = effectiveness.EffectivenessScore;

            if (score >= 75)
                effectiveness.Recommendation = $"⭐ Top tier! {damage:F0} damage - one of the best weapons you can use.";
            else if (score >= 60)
                effectiveness.Recommendation = $"✓ Excellent weapon. {damage:F0} damage - highly effective.";
            else if (score >= 45)
                effectiveness.Recommendation = $"Good choice. {damage:F0} damage - solid performance.";
            else if (score >= 30)
                effectiveness.Recommendation = $"Decent option. {damage:F0} damage - works but not optimal.";
            else
                effectiveness.Recommendation = $"Weak weapon. Only {damage:F0} damage - look for better options.";
        }

        private int GetPlayerStat(string statName, PlayerBuild build)
        {
            return statName.ToLower() switch
            {
                "str" or "strength" => build.Strength,
                "dex" or "dexterity" => build.Dexterity,
                "int" or "intelligence" => build.Intelligence,
                "fai" or "faith" => build.Faith,
                "arc" or "arcane" => build.Arcane,
                _ => 0
            };
        }

        private double GetScalingMultiplier(string grade)
        {
            return grade.ToUpper() switch
            {
                "S" => 1.75,
                "A" => 1.40,
                "B" => 1.15,
                "C" => 0.90,
                "D" => 0.65,
                "E" => 0.25,
                "-" => 0.0,
                _ => 0.5
            };
        }

        // ============= NEW BOSS HELPER METHODS =============

        private double CalculateStatBonus(PlayerBuild build)
        {
            double totalStats = build.Strength + build.Dexterity + build.Intelligence + build.Faith;
            double expectedStats = build.Level * 0.6;
            return (totalStats - expectedStats) / expectedStats;
        }

        private string DetermineAttackType(string category)
        {
            return category.ToLower() switch
            {
                "dagger" => "Pierce",
                "straight sword" => "Slash",
                "greatsword" => "Slash",
                "colossal sword" => "Slash",
                "katana" => "Slash",
                "curved sword" => "Slash",
                "curved greatsword" => "Slash",
                "thrusting sword" => "Pierce",
                "heavy thrusting sword" => "Pierce",
                "axe" => "Slash",
                "greataxe" => "Slash",
                "hammer" => "Strike",
                "great hammer" => "Strike",
                "flail" => "Strike",
                "spear" => "Pierce",
                "great spear" => "Pierce",
                "halberd" => "Slash",
                "reaper" => "Slash",
                "whip" => "Strike",
                "fist" => "Strike",
                "claw" => "Slash",
                _ => "Standard"
            };
        }

        private string GenerateReasoningForBoss(Weapons weapon, BossStats boss, double score)
        {
            var reasons = new List<string>();
            string weakness = boss.Weakness.ToLower();
            string attackType = DetermineAttackType(weapon.Category);
            
            if (weakness.Contains(attackType.ToLower()))
                reasons.Add($"{attackType} damage effective");
            
            foreach (var attack in weapon.Attack)
            {
                string dmgType = attack.Name.ToLower();
                
                if (weakness.Contains("fire") && dmgType == "fire" && attack.Amount > 0)
                    reasons.Add($"Fire damage ({attack.Amount})");
                if (weakness.Contains("magic") && (dmgType == "mag" || dmgType == "magic") && attack.Amount > 0)
                    reasons.Add($"Magic damage ({attack.Amount})");
                if (weakness.Contains("lightning") && (dmgType == "ligt" || dmgType == "lightning") && attack.Amount > 0)
                    reasons.Add($"Lightning damage ({attack.Amount})");
                if (weakness.Contains("holy") && dmgType == "holy" && attack.Amount > 0)
                    reasons.Add($"Holy damage ({attack.Amount})");
            }

            if (weakness.Contains("bleed"))
                reasons.Add("Bleed weakness");
            if (weakness.Contains("frost"))
                reasons.Add("Frost weakness");

            foreach (var attack in weapon.Attack)
            {
                if (attack.Name.ToLower() == "phy" || attack.Name.ToLower() == "physical")
                {
                    if (attack.Amount > 120)
                        reasons.Add($"High damage ({attack.Amount})");
                    break;
                }
            }

            if (reasons.Count == 0)
                reasons.Add(score >= 60 ? "Good option" : "Usable");

            return string.Join(", ", reasons);
        }
        public async Task<List<WeaponRecommendation>> GetRecommendedWeaponsForPlayerAsync(
    BossStats boss, 
    PlayerProgress playerProgress)
{
    // Get all weapons effective against this boss
    var allRecommendations = await GetBestWeaponsForBossAsync(boss);
    
    var playerRecommendations = new List<WeaponRecommendation>();
    
    foreach (var weaponEff in allRecommendations)
    {
        // Check if player already owns this weapon
        bool alreadyOwned = playerProgress.ObtainedWeaponIds.Contains(weaponEff.WeaponId);
        
        playerRecommendations.Add(new WeaponRecommendation
        {
            WeaponId = weaponEff.WeaponId,
            WeaponName = weaponEff.WeaponName,
            WeaponCategory = "Unknown", // We'll get this from weapon later
            EffectivenessScore = weaponEff.EffectivenessScore,
            Reasoning = weaponEff.Recommendation,
            AlreadyOwned = alreadyOwned,
            MeetsRequirements = weaponEff.MeetsRequirements
        });
    }
    
    // Sort: owned weapons first, then by effectiveness
    return playerRecommendations
        .OrderByDescending(w => w.AlreadyOwned)
        .ThenByDescending(w => w.EffectivenessScore)
        .ToList();
}
    }
    
}