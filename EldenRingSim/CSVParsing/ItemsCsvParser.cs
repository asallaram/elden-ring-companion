using System;
using System.Collections.Generic;
using System.Text.Json;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class ItemsCsvParser : CsvParserBase<Items>
    {
        public ItemsCsvParser(string filePath) : base(filePath) { }

        protected override Items? ParseRow(string[] columns)
        {
            if (columns.Length < 7) return null;

            var name = columns[1]?.Trim() ?? "Unknown Item";

            var item = new Items
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No description provided",
                Type = columns[4]?.Trim() ?? "Unknown Type",
                Effect = ParseFlexibleList<ItemsEffectEntry>(columns[5]),
                ObtainedFrom = columns[6]?.Trim() ?? "Unknown Source"
            };

            return item;
        }

        private static List<T> ParseFlexibleList<T>(string input) where T : new()
        {
            var result = new List<T>();
            if (string.IsNullOrWhiteSpace(input)) return result;

            input = input.Trim();
            if (input == "[]" || input.Equals("['None']", StringComparison.OrdinalIgnoreCase)) return result;

            try
            {
                var json = input.Replace('\'', '"');
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in root.EnumerateArray())
                    {
                        var inst = new T();
                        var pi = typeof(T).GetProperty("Name", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        if (pi != null && pi.PropertyType == typeof(string))
                        {
                            if (el.ValueKind == JsonValueKind.String)
                                pi.SetValue(inst, el.GetString());
                            else if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("name", out var n))
                                pi.SetValue(inst, n.GetString() ?? string.Empty);
                        }

                        result.Add(inst);
                    }

                    if (result.Count > 0) return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing flexible list: {ex.Message}");
            }

            var stripped = input.Trim().TrimStart('[').TrimEnd(']').Trim('\'', '"');
            if (!string.IsNullOrEmpty(stripped))
            {
                var inst = new T();
                var pi = typeof(T).GetProperty("Name", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (pi != null && pi.PropertyType == typeof(string))
                    pi.SetValue(inst, stripped);
                result.Add(inst);
            }

            return result;
        }
    }
}
