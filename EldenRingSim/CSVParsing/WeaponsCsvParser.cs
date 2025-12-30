using System;
using System.Collections.Generic;
using System.Text.Json;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class WeaponsCsvParser : CsvParserBase<Weapons>
    {
        public WeaponsCsvParser(string filePath) : base(filePath) { }

        protected override Weapons? ParseRow(string[] columns)
        {
            if (columns.Length < 10) return null;

            var name = columns[1]?.Trim() ?? "Unknown Weapon";

            return new Weapons
            {
                Id = SlugifyName(name), 
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No Description",
                Attack = ParseFlexibleList<NegationEntry>(columns[4]),
                Defence = ParseFlexibleList<NegationEntry>(columns[5]),
                ScalesWith = ParseFlexibleList<ScalingEntry>(columns[6], "None"),
                RequiredAttributes = ParseFlexibleList<RequirementEntry>(columns[7]),
                Category = columns[8]?.Trim() ?? "Unknown",
                Weight = double.TryParse(columns[9], out var w) ? w : 0.0
            };
        }

        private static List<T> ParseFlexibleList<T>(string input, string defaultScaling = "") where T : new()
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

                        if (el.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var prop in el.EnumerateObject())
                            {
                                var pi = typeof(T).GetProperty(prop.Name,
                                    System.Reflection.BindingFlags.IgnoreCase |
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.Instance);
                                if (pi == null) continue;

                                if (pi.PropertyType == typeof(string))
                                {
                                    var val = prop.Value.GetString() ?? "";
                                    // For ScalingEntry, default to provided defaultScaling
                                    if (typeof(T) == typeof(ScalingEntry) && prop.Name.Equals("scaling", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(val))
                                        val = defaultScaling;
                                    pi.SetValue(inst, val);
                                }
                                else if (pi.PropertyType == typeof(int) && prop.Value.TryGetInt32(out var intVal))
                                    pi.SetValue(inst, intVal);
                                else if (pi.PropertyType == typeof(double) && prop.Value.TryGetDouble(out var dblVal))
                                    pi.SetValue(inst, dblVal);
                            }

                            if (inst is ScalingEntry se && string.IsNullOrEmpty(se.Scaling))
                                se.Scaling = defaultScaling;
                        }

                        result.Add(inst);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing flexible list: {ex.Message}");
                var stripped = input.Trim().TrimStart('[').TrimEnd(']').Trim('\'', '"');
                if (!string.IsNullOrEmpty(stripped))
                {
                    var inst = new T();
                    var pi = typeof(T).GetProperty("Name", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (pi != null && pi.PropertyType == typeof(string))
                        pi.SetValue(inst, stripped);

                    if (inst is ScalingEntry se)
                        se.Scaling = defaultScaling;

                    result.Add(inst);
                }
            }

            return result;
        }
    }
}
