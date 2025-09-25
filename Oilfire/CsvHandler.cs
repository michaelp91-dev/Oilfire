using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Oilfire
{
    /// <summary>
    /// Handles reading and parsing of propellant performance data from a CSV file.
    /// </summary>
    public class CsvHandler
    {
        /// <summary>
        /// Loads and parses the specified CSV file.
        /// </summary>
        /// <param name="filePath">The path to the CSV file.</param>
        /// <returns>A list of PropellantData objects.</returns>
        public List<PropellantData> Load(string filePath)
        {
            var data = new List<PropellantData>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The data file was not found.", filePath);
            }

            // Read all lines, but skip the header row using LINQ.
            string[] lines = File.ReadAllLines(filePath).Skip(1).ToArray();

            if (lines.Length == 0)
            {
                Console.WriteLine("Warning: The data file is empty or contains only a header.");
                return data;
            }

            int lineNumber = 1;
            foreach (string line in lines)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = line.Split(',');

                if (values.Length != 7)
                {
                    Console.WriteLine($"Warning: Skipping malformed line #{lineNumber}. Expected 7 values, but found {values.Length}.");
                    continue;
                }

                try
                {
                    var record = new PropellantData
                    {
                        OfRatio = float.Parse(values[0]),
                        ChamberTempK = float.Parse(values[1]),
                        GammaChamber = float.Parse(values[2]),
                        GammaThroat = float.Parse(values[3]),
                        MolarMassGmol = float.Parse(values[4]),
                        VacIspS = float.Parse(values[5]),
                        CstarMS = float.Parse(values[6])
                    };
                    data.Add(record);
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Warning: Skipping line #{lineNumber} due to a data format error. Details: {ex.Message}");
                }
            }
            return data;
        }
    }
}