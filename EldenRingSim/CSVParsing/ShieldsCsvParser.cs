using System;
using System.Collections.Generic;
using System.Text.Json;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class ShieldsCsvParser : CsvParserBase<Shields>
    {
        public ShieldsCsvParser(string filePath) : base(filePath) { }

        protected override Shields? ParseRow(string[] columns)
        {
            if (columns.Length < 10) return null;

            var name = columns[1]?.Trim() ?? "Unknown Shield";

            return new Shields
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No description provided",
                Attack = ParseNegationList(columns[4]),
                Defence = ParseNegationList(columns[5]),
                ScalesWith = ParseScalingList(columns[6]),
                RequiredAttributes = ParseRequirementList(columns[7]),
                Category = columns[8]?.Trim() ?? "Unknown Category",
                Weight = double.TryParse(columns[9], out var w) ? w : 0
            };
        }

        private static List<NegationEntry> ParseNegationList(string input)
        {
            var result = new List<NegationEntry>();
            if (string.IsNullOrWhiteSpace(input)) return result;

            input = input.Replace('\'', '"');
            try
            {
                using var doc = JsonDocument.Parse(input);
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    var entry = new NegationEntry
                    {
                        Name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "Unknown" : "Unknown",
                        Amount = el.TryGetProperty("amount", out var a) && a.TryGetDouble(out var val) ? val : 0
                    };
                    result.Add(entry);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing negation list: {ex.Message}");
            }
            return result;
        }

        private static List<ScalingEntry> ParseScalingList(string input)
        {
            var result = new List<ScalingEntry>();
            if (string.IsNullOrWhiteSpace(input)) return result;

            input = input.Replace('\'', '"');
            try
            {
                using var doc = JsonDocument.Parse(input);
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    var entry = new ScalingEntry
                    {
                        Name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "Unknown" : "Unknown",
                        Scaling = el.TryGetProperty("scaling", out var s) ? s.GetString() ?? string.Empty : string.Empty
                    };
                    result.Add(entry);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing scaling list: {ex.Message}");
            }
            return result;
        }

        private static List<RequirementEntry> ParseRequirementList(string input)
        {
            var result = new List<RequirementEntry>();
            if (string.IsNullOrWhiteSpace(input)) return result;

            input = input.Replace('\'', '"');
            try
            {
                using var doc = JsonDocument.Parse(input);
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    var entry = new RequirementEntry
                    {
                        Name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "Unknown" : "Unknown",
                        Amount = el.TryGetProperty("amount", out var a) && a.TryGetInt32(out var val) ? val : 0
                    };
                    result.Add(entry);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing requirement list: {ex.Message}");
            }
            return result;
        }
    }
}

