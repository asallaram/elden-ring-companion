using System.Collections.Generic;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class BossesCsvParser : CsvParserBase<Bosses>
    {
        public BossesCsvParser(string filePath) : base(filePath) { }

        protected override Bosses? ParseRow(string[] columns)
        {
            if (columns.Length < 5) return null;

            var name = columns[1]?.Trim() ?? "Unknown Boss";

            var boss = new Bosses
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[4]?.Trim() ?? "No description provided",  
                Region = columns[3]?.Trim() ?? "Unknown",  // Column 3 is region
                Location = columns.Length > 5 ? columns[5]?.Trim() ?? string.Empty : string.Empty,
                Drops = columns.Length > 6 ? ParseJsonColumn<BossesDropEntry>(columns[6]) : new List<BossesDropEntry>(),
                HealthPoints = columns.Length > 7 ? columns[7]?.Trim() ?? "Unknown" : "Unknown"
            };

            return boss;
        }
    }
}
