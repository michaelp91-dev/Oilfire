using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Oilfire
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Rocket Engine Design Calculator (SI Units) ---");
            Console.WriteLine("Based on 'How to design, build and test small liquid-fuel rocket engines'");

            // --- Load Propellant Data ---
            List<PropellantData> propellantData = null;
            try
            {
                string dataPath = Path.Combine("Data", "ethanol_n2o_data_si.csv");
                Console.WriteLine($"\nLoading propellant data from: {Path.GetFullPath(dataPath)}");
                var csvHandler = new CsvHandler();
                propellantData = csvHandler.Load(dataPath);

                if (!propellantData.Any())
                {
                    throw new Exception("CSV file loaded, but it contains no data.");
                }

                var optimalData = propellantData.OrderByDescending(p => p.VacIspS).First();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Data loaded successfully.");
                Console.WriteLine($"Optimal performance found at O/F Ratio: {optimalData.OfRatio:F2} with Isp: {optimalData.VacIspS:F2}s");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nFATAL ERROR: Could not load propellant data. {ex.Message}");
                Console.WriteLine("Please ensure 'ethanol_n2o_data_si.csv' is in the 'Data' folder.");
                Console.ResetColor();
                Console.WriteLine("\n--- End of Program ---");
                return; // Exit if data cannot be loaded
            }

            // --- Calculate Engine with Hardcoded Thrust ---
            try
            {
                // Hardcoded thrust value of 2224 N (equivalent to 500 lbf)
                float thrustN = 20000f;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n--- Calculating Design for a {thrustN} N Engine ---");
                Console.ResetColor();

                var designer = new EngineDesigner(thrustN, propellantData);
                designer.Calculate();
                PrintDesignResults(designer);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred during calculation: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\n--- End of Program ---");
        }

        /// <summary>
        /// Helper method to print the design parameters from an EngineDesigner instance in SI units.
        /// </summary>
        private static void PrintDesignResults(EngineDesigner designer)
        {
            Console.WriteLine("\n--- Engine Design Parameters (SI Units) ---");
            Console.WriteLine($"INPUTS (Fixed & Optimal):");
            Console.WriteLine($"  Desired Thrust:      {designer.DesiredThrustN:F2} N");
            Console.WriteLine($"  Chamber Pressure:    {(designer.ChamberPressurePa / 1e6):F2} MPa");
            Console.WriteLine($"  Specific Impulse:    {designer.SpecificImpulseSec:F2} s");
            Console.WriteLine($"  O/F Ratio:           {designer.OfRatio:F2}");
            Console.WriteLine($"  Gamma (Throat):      {designer.Gamma:F4}");
            Console.WriteLine($"  Chamber Temp:        {designer.ChamberTempK:F1} K");
            Console.WriteLine($"----------------------------------");
            Console.WriteLine("RESULTS:");
            Console.WriteLine("--- Propellant Flow ---");
            Console.WriteLine($"  Total Mass Flow:     {designer.TotalMassFlowRateKgps:F4} kg/s");
            Console.WriteLine($"  Fuel Mass Flow:      {designer.FuelMassFlowRateKgps:F4} kg/s");
            Console.WriteLine($"  Oxidizer Mass Flow:  {designer.OxidizerMassFlowRateKgps:F4} kg/s");
            Console.WriteLine("\n--- Nozzle Dimensions (mm) ---");
            Console.WriteLine($"  Throat Diameter:     {(designer.NozzleThroatDiameterM * 1000):F2} mm");
            Console.WriteLine($"  Throat Area:         {(designer.NozzleThroatAreaM2 * 1e6):F2} mm^2");
            Console.WriteLine($"  Exit Diameter:       {(designer.NozzleExitDiameterM * 1000):F2} mm");
            Console.WriteLine($"  Exit Area:           {(designer.NozzleExitAreaM2 * 1e6):F2} mm^2");
            Console.WriteLine("\n--- Combustion Chamber (mm) ---");
            Console.WriteLine($"  Chamber Diameter:    {(designer.ChamberDiameterM * 1000):F2} mm");
            Console.WriteLine($"  Chamber Length:      {(designer.ChamberLengthM * 1000):F2} mm");
            Console.WriteLine($"  Chamber Volume:      {(designer.ChamberVolumeM3 * 1e6):F1} cm^3");
            Console.WriteLine($"  Wall Thickness (Min):{(designer.MinChamberWallThicknessM * 1000):F3} mm");
            Console.WriteLine($"  Wall Thickness (Set):{(designer.PracticalChamberWallThicknessM * 1000):F3} mm");
            Console.WriteLine("----------------------------------");
        }
    }
}