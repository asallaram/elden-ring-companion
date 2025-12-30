using System.Collections.Generic;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class ArmorCsvParser : CsvParserBase<Armor>
    {
        public ArmorCsvParser(string filePath) : base(filePath) { }

        protected override Armor? ParseRow(string[] columns)
        {
            if (columns.Length < 8) return null;

            var name = columns[1]?.Trim() ?? "Unknown Armor";

            var armor = new Armor
            {
                Id = SlugifyName(name),  
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Description = columns[3]?.Trim() ?? "No description provided",
                Category = columns[4]?.Trim() ?? "Unknown",
                DmgNegation = ParseJsonColumn<NegationEntry>(columns[5]),
                Resistance = ParseJsonColumn<ResistanceEntry>(columns[6]),
                Weight = double.TryParse(columns[7], out double weight) ? weight : 0.0
            };

            return armor;
        }
    }
}

