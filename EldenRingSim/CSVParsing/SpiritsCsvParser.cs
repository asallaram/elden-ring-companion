using System;
using System.Collections.Generic;
using System.Text.Json;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class SpiritsCsvParser : CsvParserBase<Spirits>
    {
        public SpiritsCsvParser(string filePath) : base(filePath) { }

        protected override Spirits? ParseRow(string[] columns)
        {
            if (columns.Length < 7) return null;

            var name = columns[1]?.Trim() ?? "Unknown Spirit";

            return new Spirits
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No description provided",
                FpCost = int.TryParse(columns[4], out var fp) ? fp : 0,
                HpCost = int.TryParse(columns[5], out var hp) ? hp : 0,
                Effect = ParseEffectList(columns[6])
            };
        }

        private static List<SpiritEffectEntry> ParseEffectList(string input)
        {
            var result = new List<SpiritEffectEntry>();
            if (string.IsNullOrWhiteSpace(input)) return result;

            input = input.Trim();
            if (input == "[]" || input.Equals("['None']", StringComparison.OrdinalIgnoreCase)) return result;

            try
            {
                string json = input;
                if (!input.StartsWith("["))
                    json = $"[\"{input.Replace("\"", "\\\"")}\"]"; // wrap plain string

                using var doc = JsonDocument.Parse(json);
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    if (el.ValueKind == JsonValueKind.Object)
                    {
                        var entry = new SpiritEffectEntry
                        {
                            Name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "Unknown" : "Unknown",
                            Amount = el.TryGetProperty("amount", out var a) ? a.GetString() ?? "0" : "0"
                        };
                        result.Add(entry);
                    }
                    else if (el.ValueKind == JsonValueKind.String)
                    {
                        result.Add(new SpiritEffectEntry
                        {
                            Name = el.GetString() ?? "Unknown",
                            Amount = "0"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing effect list: {ex.Message}");
                var stripped = input.Trim('\'', '"');
                if (!string.IsNullOrEmpty(stripped))
                {
                    result.Add(new SpiritEffectEntry
                    {
                        Name = stripped,
                        Amount = "0"
                    });
                }
            }

            return result;
        }
    }
}
