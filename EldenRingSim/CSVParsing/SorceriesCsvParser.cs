using System;
using System.Collections.Generic;
using System.Text.Json;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class SorceriesCsvParser : CsvParserBase<Sorceries>
    {
        public SorceriesCsvParser(string filePath) : base(filePath) { }

        protected override Sorceries? ParseRow(string[] columns)
        {
            if (columns.Length < 9) return null;

            var name = columns[1]?.Trim() ?? "Unknown Sorcery";

            return new Sorceries
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No Description",
                Type = columns[4]?.Trim() ?? "Unknown Type",
                Cost = int.TryParse(columns[5], out var cost) ? cost : 0,
                Slots = int.TryParse(columns[6], out var slots) ? slots : 0,
                Effects = ParseEffectList(columns[7]),
                Requires = ParseRequirementList(columns[8])
            };
        }

        private static List<SorceriesEffectEntry> ParseEffectList(string input)
        {
            var result = new List<SorceriesEffectEntry>();
            if (string.IsNullOrWhiteSpace(input)) return result;

            input = input.Trim();
            try
            {
                var json = input.Replace('\'', '"');
                using var doc = JsonDocument.Parse(json);

                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    if (el.ValueKind == JsonValueKind.Object)
                    {
                        result.Add(new SorceriesEffectEntry
                        {
                            Name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "Unknown" : "Unknown",
                            Amount = el.TryGetProperty("amount", out var a) ? a.GetString() ?? "0" : "0"
                        });
                    }
                    else if (el.ValueKind == JsonValueKind.String)
                    {
                        result.Add(new SorceriesEffectEntry
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
                var stripped = input.Trim().TrimStart('[').TrimEnd(']').Trim('\'', '"');
                if (!string.IsNullOrEmpty(stripped))
                {
                    result.Add(new SorceriesEffectEntry
                    {
                        Name = stripped,
                        Amount = "0"
                    });
                }
            }

            return result;
        }

        private static List<SorceriesRequirementEntry> ParseRequirementList(string input)
        {
            var result = new List<SorceriesRequirementEntry>();
            if (string.IsNullOrWhiteSpace(input)) return result;

            input = input.Trim();
            try
            {
                var json = input.Replace('\'', '"');
                using var doc = JsonDocument.Parse(json);

                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    result.Add(new SorceriesRequirementEntry
                    {
                        Name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "Unknown" : "Unknown",
                        Amount = el.TryGetProperty("amount", out var a) ? a.GetString() ?? "0" : "0"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing requirement list: {ex.Message}");
                var stripped = input.Trim().TrimStart('[').TrimEnd(']').Trim('\'', '"');
                if (!string.IsNullOrEmpty(stripped))
                {
                    result.Add(new SorceriesRequirementEntry
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
