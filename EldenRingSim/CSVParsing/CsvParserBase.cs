using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{

    public abstract class CsvParserBase<T> where T : GameEntity, new()
    {
        protected string FilePath;
        private static HashSet<string> _usedIds = new HashSet<string>();

        protected CsvParserBase(string filePath)
        {
            FilePath = filePath;
        }
        
        public List<T> Parse()
        {
            var list = new List<T>();
            using var reader = new StreamReader(FilePath);
            string? headerLine = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var columns = SplitCsvLine(line);
                if (columns.Length == 0) continue;

                try
                {
                    var item = ParseRow(columns);
                    if (item != null)
                        list.Add(item);
                }
                catch (Exception)
                {
                }
            }

            return list;
        }

        protected abstract T? ParseRow(string[] columns);

        protected static string[] SplitCsvLine(string line)
        {
            var values = new List<string>();
            bool inQuotes = false;
            string current = "";

            foreach (char c in line)
            {
                if (c == '"') inQuotes = !inQuotes;
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            values.Add(current);
            return values.ToArray();
        }

        protected static string SlugifyName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "unknown";

            var baseSlug = name
                .ToLower()                      
                .Trim()                         
                .Replace(" ", "-")              
                .Replace("'", "")               
                .Replace("&", "and")            
                .Replace(".", "")               
                .Replace(",", "")               
                .Replace("(", "")               
                .Replace(")", "")               
                .Replace("/", "-")              
                .Replace("\\", "-")             
                .Replace("--", "-")             
                .Trim('-');                     

            var slug = baseSlug;
            int counter = 1;
            
            while (_usedIds.Contains(slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }
            
            _usedIds.Add(slug);
            return slug;
        }

        
        protected static List<TEntry> ParseJsonColumn<TEntry>(string jsonLike) where TEntry : class, new()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonLike)) return new List<TEntry>();

                string json = jsonLike.Replace('\'', '"');

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var list = JsonSerializer.Deserialize<List<TEntry>>(json, options) ?? new List<TEntry>();

                return list
                    .Where(e =>
                    {
                        if (e == null) return false;

                        var prop = typeof(TEntry).GetProperty("Name");
                        if (prop != null)
                        {
                            var val = prop.GetValue(e) as string;
                            if (string.IsNullOrWhiteSpace(val)) return false;

                            prop.SetValue(e, val.Trim());
                        }

                        return true;
                    })
                    .ToList();
            }
            catch (Exception)
            {
                return new List<TEntry>();
            }
        }
    }
}
