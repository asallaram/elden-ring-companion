using System;
using System.Collections.Generic;
using System.Text.Json;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class ClassesCsvParser : CsvParserBase<Classes>
    {
        public ClassesCsvParser(string filePath) : base(filePath) { }

        protected override Classes? ParseRow(string[] columns)
        {
            if (columns.Length < 5) return null;

            var name = columns[1]?.Trim() ?? "Unknown Class";

            var playerClass = new Classes
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No description provided",
                Stats = new List<StatEntry>()
            };

            try
            {
                var statsDict = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    columns[4].Replace('\'', '"')
                ) ?? new Dictionary<string, string>();

                foreach (var kv in statsDict)
                {
                    playerClass.Stats.Add(new StatEntry
                    {
                        Name = kv.Key.Trim(),
                        Value = kv.Value.Trim()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing stats for class {name}: {ex.Message}");
                // Leave Stats empty if parsing fails
            }

            return playerClass;
        }
    }
}
