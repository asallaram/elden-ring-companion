using System;
using System.Collections.Generic;
using System.Text.Json;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class CreaturesCsvParser : CsvParserBase<Creatures>
    {
        public CreaturesCsvParser(string filePath) : base(filePath) { }

        protected override Creatures? ParseRow(string[] columns)
        {
            if (columns.Length < 6) return null;

            var name = !string.IsNullOrWhiteSpace(columns[1]) ? columns[1].Trim() : "Unknown Creature";

            var creature = new Creatures
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No description provided",
                Location = columns[4]?.Trim() ?? "Unknown Location",
                Drops = ParseDropsColumn(columns[5])
            };

            return creature;
        }

        private List<CreaturesDropEntry> ParseDropsColumn(string raw)
        {
            var drops = new List<CreaturesDropEntry>();
            if (string.IsNullOrWhiteSpace(raw) || raw.Contains("None", StringComparison.OrdinalIgnoreCase))
                return drops;

            try
            {
                var normalized = raw.Replace('\'', '"');
                var dropNames = JsonSerializer.Deserialize<List<string>>(normalized);
                if (dropNames != null)
                {
                    foreach (var name in dropNames)
                    {
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            drops.Add(new CreaturesDropEntry { Name = name.Trim(), Amount = "1" });
                        }
                    }
                    return drops;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing drops column: {ex.Message}");
            }

            var stripped = raw.Trim('\'', '"').Trim();
            if (!string.IsNullOrEmpty(stripped))
            {
                drops.Add(new CreaturesDropEntry { Name = stripped, Amount = "1" });
            }

            return drops;
        }
    }
}
