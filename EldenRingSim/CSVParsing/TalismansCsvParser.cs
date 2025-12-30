using System;
using System.Collections.Generic;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class TalismansCsvParser : CsvParserBase<Talismans>
    {
        public TalismansCsvParser(string filePath) : base(filePath) { }

        protected override Talismans? ParseRow(string[] columns)
        {
            if (columns.Length < 5) return null;

            var name = columns[1]?.Trim() ?? "Unknown Talisman";

            return new Talismans
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No Description",
                Effect = columns[4]?.Trim() ?? "No Effect"
            };
        }
    }
}
