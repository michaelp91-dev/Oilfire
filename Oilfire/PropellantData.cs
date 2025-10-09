using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PicoGK;

namespace RocketEngineBuilder
{
    /// <summary>
    /// Represents a single row of data from the N2O_Ethanol.csv file.
    /// This class holds the thermodynamic properties for a given O/F ratio.
    /// </summary>
    public class PropellantData
    {
        public float OF_Ratio { get; set; }
        public float Chamber_Pressure { get; set; }  // Pc (psia)
        public float Chamber_Temp_R { get; set; }    // Tc (R)
        public float Throat_Temp_R { get; set; }     // Tt (R)
        public float Gamma_Chamber { get; set; }     // gamma
        public float Isp { get; set; }              // Isp (s)
        public float Expansion_Ratio { get; set; }   
    }

    /// <summary>
    /// Manages loading and providing access to the propellant data from the CSV.
    /// This class replaces the need for XLOOKUP in the C# code.
    /// </summary>
    public class PropellantDataService
    {
        private readonly List<PropellantData> _data;

        public PropellantDataService(string csvFilePath)
        {
            _data = LoadDataFromCsv(csvFilePath);
        }

        /// <summary>
        /// Loads the CSV file into a list of PropellantData objects.
        /// </summary>
        private List<PropellantData> LoadDataFromCsv(string filePath)
        {
            var dataList = new List<PropellantData>();
            var lines = File.ReadAllLines(filePath).Skip(1); // Skip header

            int lineNumber = 1;
            foreach (var line in lines)
            {
                lineNumber++;
                var values = line.Split(',').Select(v => v.Trim()).ToArray();
                if (values.Length >= 8)
                {
                    // Use TryParse for more robust error handling
                    if (float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float ofRatio) &&
                        float.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float pressure) &&
                        float.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float isp) &&
                        float.TryParse(values[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float chamberTemp) &&
                        float.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out float throatTemp) &&
                        float.TryParse(values[7], NumberStyles.Any, CultureInfo.InvariantCulture, out float gamma) &&
                        float.TryParse(values[8], NumberStyles.Any, CultureInfo.InvariantCulture, out float expansionRatio)
                        )
                    {
                        dataList.Add(new PropellantData
                        {
                            OF_Ratio = ofRatio,
                            Chamber_Pressure = pressure,
                            Chamber_Temp_R = chamberTemp,
                            Throat_Temp_R = throatTemp,
                            Gamma_Chamber = gamma,
                            Isp = isp,
                            Expansion_Ratio = expansionRatio
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Could not parse line {lineNumber} in {filePath}. Skipping.");
                    }
                }
            }
            return dataList;
        }

        /// <summary>
        /// Finds the closest data point for a given O/F ratio.
        /// This is the C# equivalent of the XLOOKUP function used in the spreadsheet.
        /// </summary>
        public PropellantData FindClosest(float targetOfRatio, float targetPressure)
        {
            if (_data == null || !_data.Any()) return null;

            // First, find all entries with the closest O/F ratio
            var ofMatches = _data
                .GroupBy(d => Math.Abs(d.OF_Ratio - (float)targetOfRatio))
                .OrderBy(g => g.Key)
                .First();

            // Then from those, find the one with the closest chamber pressure
            return ofMatches
                .OrderBy(d => Math.Abs(d.Chamber_Pressure - (float)targetPressure))
                .FirstOrDefault();
        }
    }
}