using System.Collections.Generic;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class AmmoCsvParser : CsvParserBase<Ammo>
    {
        public AmmoCsvParser(string filePath) : base(filePath) { }

        protected override Ammo? ParseRow(string[] columns)
        {
            if (columns.Length < 7) return null;

            var name = columns[1]?.Trim() ?? "Unknown Ammo";

            var ammo = new Ammo
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No description provided",
                Type = columns[4]?.Trim() ?? "Unknown",
                AttackPower = ParseJsonColumn<AttackPowerEntry>(columns[5]) ?? new List<AttackPowerEntry>(),
                Passive = columns[6]?.Trim() ?? string.Empty
            };

            return ammo;
        }
    }
}
