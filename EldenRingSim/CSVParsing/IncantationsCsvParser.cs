using System;
using System.Collections.Generic;
using System.Text.Json;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class IncantationsCsvParser : CsvParserBase<Incantations>
    {
        public IncantationsCsvParser(string filePath) : base(filePath) { }

        protected override Incantations? ParseRow(string[] columns)
        {
            if (columns.Length < 9) return null;

            var name = columns[1]?.Trim() ?? "Unknown Incantation";

            var incantation = new Incantations
            {
                Id = SlugifyName(name), 
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No description",
                Type = columns[4]?.Trim() ?? "Unknown Type",
                Cost = int.TryParse(columns[5], out var cost) ? cost : 0,
                Slots = int.TryParse(columns[6], out var slots) ? slots : 0,
                Effects = ParseEffectsColumn(columns[7]),
                Requires = ParseRequiresColumn(columns[8])
            };

            return incantation;
        }

        private List<IncantationsEffectEntry> ParseEffectsColumn(string raw)
        {
            var list = new List<IncantationsEffectEntry>();
            if (string.IsNullOrWhiteSpace(raw)) return list;

            var stripped = raw.Trim('\'', '"').Trim();
            if (!string.IsNullOrEmpty(stripped))
                list.Add(new IncantationsEffectEntry { Name = stripped, Amount = 0 });

            return list;
        }

        private List<IncantationsRequirementEntry> ParseRequiresColumn(string raw)
        {
            var list = new List<IncantationsRequirementEntry>();
            if (string.IsNullOrWhiteSpace(raw)) return list;

            try
            {
                var normalized = raw.Replace('\'', '"');
                var doc = JsonDocument.Parse(normalized);
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    var name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "Unknown" : "Unknown";
                    var amount = el.TryGetProperty("amount", out var a) && a.TryGetInt32(out var val) ? val : 0;
                    list.Add(new IncantationsRequirementEntry { Name = name, Amount = amount });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing requires column: {ex.Message}");
                var stripped = raw.Trim('[', ']', '"', '\'').Trim();
                if (!string.IsNullOrWhiteSpace(stripped))
                    list.Add(new IncantationsRequirementEntry { Name = stripped, Amount = 0 });
            }

            return list;
        }
    }
}
