using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class BossStatsCsvParser : CsvParserBase<BossStats>
    {
        public BossStatsCsvParser(string filePath) : base(filePath) { }

        protected override BossStats? ParseRow(string[] columns)
        {
            if (columns.Length < 20) return null;

            try
            {
                var name = columns[1].Trim();
                var bossName = columns[3].Trim();

                return new BossStats
                {
                    Id = SlugifyName(name),  
                    Name = name,
                    Image = columns[2]?.Trim() ?? string.Empty,
                    BossName = bossName,
                    Description = columns[4]?.Trim() ?? string.Empty,
                    HealthPoints = int.TryParse(columns[5], out int hp) ? hp : 0,
                    PhysicalResist = double.TryParse(columns[6], out double pr) ? pr : 0.0,
                    MagicResist = double.TryParse(columns[7], out double mr) ? mr : 0.0,
                    FireResist = double.TryParse(columns[8], out double fr) ? fr : 0.0,
                    LightningResist = double.TryParse(columns[9], out double lr) ? lr : 0.0,
                    HolyResist = double.TryParse(columns[10], out double hr) ? hr : 0.0,
                    BleedImmune = bool.TryParse(columns[11], out bool bi) ? bi : false,
                    PoisonImmune = bool.TryParse(columns[12], out bool pi) ? pi : false,
                    FrostImmune = bool.TryParse(columns[13], out bool fi) ? fi : false,
                    ScarletRotImmune = bool.TryParse(columns[14], out bool si) ? si : false,
                    MadnessImmune = bool.TryParse(columns[15], out bool mi) ? mi : false,
                    SleepImmune = bool.TryParse(columns[16], out bool sli) ? sli : false,
                    Weakness = columns[17]?.Trim() ?? string.Empty,
                    Tier = int.TryParse(columns[18], out int tier) ? tier : 1,
                    AverageDamage = int.TryParse(columns[19], out int ad) ? ad : 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing boss stats row: {ex.Message}");
                return null;
            }
        }
    }
}

