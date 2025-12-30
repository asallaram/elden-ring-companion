using System;
using System.Collections.Generic;
using EldenRingSim.DB;

namespace EldenRingSim.CSVParsing
{
    public class LocationsCsvParser : CsvParserBase<Locations>
    {
        public LocationsCsvParser(string filePath) : base(filePath) { }

        protected override Locations? ParseRow(string[] columns)
        {
            if (columns.Length < 5) return null;

            var name = columns[1]?.Trim() ?? "Unknown Location";

            var location = new Locations
            {
                Id = SlugifyName(name), 
                Name = name,
                Image = columns[2]?.Trim() ?? string.Empty,
                Region = columns[3]?.Trim() ?? "Unknown Region",
                Description = columns[4]?.Trim() ?? "No description provided",
                SubLocations = new List<SubLocationEntry>() 
            };

            return location;
        }
    }
}
