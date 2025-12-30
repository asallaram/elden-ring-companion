using System;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class NPCsCsvParser : CsvParserBase<NPCs>
    {
        public NPCsCsvParser(string filePath) : base(filePath) { }

        protected override NPCs? ParseRow(string[] columns)
        {
            if (columns.Length < 5) return null; 

            var name = columns[1]?.Trim() ?? "Unknown NPC";

            return new NPCs
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = "No description provided", 
                Quote = columns[3]?.Trim() ?? "No quote provided",
                Location = columns[4]?.Trim() ?? "Unknown Location",
                Role = columns.Length > 5 ? columns[5]?.Trim() ?? "Unknown Role" : "Unknown Role"
            };
        }
    }
}
