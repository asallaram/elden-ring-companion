using System.Collections.Generic;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class AshOfWarCsvParser : CsvParserBase<AshOfWar>
    {
        public AshOfWarCsvParser(string filePath) : base(filePath) { }

        protected override AshOfWar? ParseRow(string[] columns)
        {
            if (columns.Length < 6) return null;

            var name = columns[1]?.Trim() ?? "Unknown Ash of War";

            var ash = new AshOfWar
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No description provided",
                Affinity = columns[4]?.Trim() ?? "Standard",
                Skill = columns[5]?.Trim() ?? "Unknown Skill",
                DescriptionDetails = new AshOfWarDescription
                {
                    DescriptionText = columns.Length > 6 ? columns[6]?.Trim() ?? string.Empty : string.Empty,
                    Effect = columns.Length > 7 ? columns[7]?.Trim() ?? string.Empty : string.Empty
                }
            };

            return ash;
        }
    }
}
