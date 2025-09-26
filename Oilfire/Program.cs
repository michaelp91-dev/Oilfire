using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RocketEngineCalculator
{
    /// <summary>
    /// Represents a single row of data from the N2O_Ethanol.csv file.
    /// This class holds the thermodynamic properties for a given O/F ratio.
    /// </summary>
    public class PropellantData
    {
        public double OF_Ratio { get; set; }
        public double Chamber_Temp_R { get; set; }
        public double Throat_Temp_R { get; set; }
        public double Gamma_Chamber { get; set; }
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
                var values = line.Split(',');
                if (values.Length >= 8)
                {
                    // Use TryParse for more robust error handling
                    if (double.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double ofRatio) &&
                        double.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double chamberTemp) &&
                        double.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double throatTemp) &&
                        double.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double gamma))
                    {
                        dataList.Add(new PropellantData
                        {
                            OF_Ratio = ofRatio,
                            Chamber_Temp_R = chamberTemp,
                            Throat_Temp_R = throatTemp,
                            Gamma_Chamber = gamma,
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
        public PropellantData FindClosest(double targetOfRatio)
        {
            if (_data == null || !_data.Any()) return null;
            return _data.OrderBy(d => Math.Abs(d.OF_Ratio - targetOfRatio)).FirstOrDefault();
        }
    }

    /// <summary>
    /// Main class to perform the rocket engine calculations.
    /// Each method in this class corresponds to a formula from the spreadsheet.
    /// </summary>
    public class RocketEngine
    {
        // --- INPUTS (Corresponds to cells B1-B8 in the spreadsheet) ---
        public double Thrust { get; set; }
        public double OF_Ratio { get; set; }
        public double Isp { get; set; }
        public double ChamberPressure { get; set; }
        
        // --- CONSTANTS ---
        public const double GasConstantR = 65.5; // ft-lb/lb*R
        public const double GravityConstantGc = 32.2; // ft/sec^2

        // --- LOOKUP DATA ---
        private readonly PropellantData _propellantData;
        
        public RocketEngine(PropellantData propellantData)
        {
            _propellantData = propellantData ?? throw new ArgumentNullException(nameof(propellantData));
        }

        // --- CALCULATIONS (Each method matches a formula in Equations.txt) ---
        
        public double GetGamma() => _propellantData.Gamma_Chamber;
        
        public double CalculateTotalWeightFlow() => Thrust / Isp;

        public double CalculateFuelWeightFlow() => CalculateTotalWeightFlow() / (OF_Ratio + 1);

        public double CalculateOxidizerWeightFlow() => CalculateTotalWeightFlow() - CalculateFuelWeightFlow();

        public double GetThroatTemperature() => _propellantData.Throat_Temp_R;

        public double CalculateThroatPressure() => ChamberPressure * 0.564;

        public double CalculateThroatArea()
        {
            double wt = CalculateTotalWeightFlow();
            double pt = CalculateThroatPressure();
            double tt = GetThroatTemperature();
            double gamma = GetGamma();
            return (wt / pt) * Math.Sqrt((GasConstantR * tt) / (gamma * GravityConstantGc));
        }

        public double CalculateThroatDiameter() => Math.Sqrt(4 * CalculateThroatArea() / Math.PI);

        public double CalculateChamberVolume(double lStar = 60) => lStar * CalculateThroatArea();
        
        public double CalculateChamberDiameter() => CalculateThroatDiameter() * 5;

        public double CalculateChamberArea() => Math.PI * Math.Pow(CalculateChamberDiameter() / 2, 2);

        public double CalculateChamberLength(double lStar = 60) => CalculateChamberVolume(lStar) / (1.1 * CalculateChamberArea());
        
        public double CalculateWallThickness() => (ChamberPressure * CalculateChamberDiameter() / 16000) * 3;
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Rocket Engine Design Calculator (C# Version) ---");

            try
            {
                var dataService = new PropellantDataService("N2O_Ethanol.csv");

                double targetOfRatio = 4.5;
                var propellantData = dataService.FindClosest(targetOfRatio);
                if (propellantData == null)
                {
                    Console.WriteLine($"Error: Could not find data for O/F Ratio near {targetOfRatio}.");
                    return;
                }
                
                var engine = new RocketEngine(propellantData)
                {
                     Thrust = 20,
                     ChamberPressure = 300,
                     OF_Ratio = targetOfRatio,
                     Isp = 302.84
                };

                Console.WriteLine("\n--- INPUTS ---");
                Console.WriteLine($"Target O/F Ratio: \t{engine.OF_Ratio}");
                Console.WriteLine($"Thrust: \t\t{engine.Thrust} lbs");
                Console.WriteLine($"Chamber Pressure: \t{engine.ChamberPressure} psi");
                Console.WriteLine($"Specific Impulse (Isp): {engine.Isp} sec");

                Console.WriteLine("\n--- LOOKUP VALUES ---");
                Console.WriteLine($"Closest O/F Ratio Found:{propellantData.OF_Ratio}");
                Console.WriteLine($"Gamma: \t\t\t{engine.GetGamma():F4}");
                Console.WriteLine($"Throat Temp (Tt): \t{engine.GetThroatTemperature():F2} R");

                Console.WriteLine("\n--- CALCULATED VALUES ---");
                Console.WriteLine($"Total Weight Flow (Wt): \t{engine.CalculateTotalWeightFlow():F5} lb/sec");
                Console.WriteLine($"Fuel Weight Flow (Wf): \t\t{engine.CalculateFuelWeightFlow():F5} lb/sec");
                Console.WriteLine($"Oxidizer Weight Flow (Wo): \t{engine.CalculateOxidizerWeightFlow():F5} lb/sec");
                Console.WriteLine($"Throat Pressure (Pt): \t\t{engine.CalculateThroatPressure():F2} psi");
                Console.WriteLine($"Throat Area (At): \t\t{engine.CalculateThroatArea():F5} in^2");
                Console.WriteLine($"Throat Diameter (Dt): \t\t{engine.CalculateThroatDiameter():F5} in");
                Console.WriteLine($"Chamber Diameter (Dc): \t\t{engine.CalculateChamberDiameter():F5} in");
                Console.WriteLine($"Chamber Area (Ac): \t\t{engine.CalculateChamberArea():F5} in^2");
                Console.WriteLine($"Chamber Length (Lc): \t\t{engine.CalculateChamberLength():F5} in");
                Console.WriteLine($"Wall Thickness (Tw): \t\t{engine.CalculateWallThickness():F5} in");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("ERROR: The file 'N2O_Ethanol.csv' was not found. Please place it in the same directory as the program.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}

