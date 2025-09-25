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
            Console.WriteLine("--- Rocket Engine System Design Calculator (SI Units) ---");
            
            // --- Load Propellant Data ---
            List<PropellantData> propellantData = null;
            try
            {
                string dataPath = Path.Combine("Data", "ethanol_n2o_data_si.csv");
                Console.WriteLine($"\nLoading propellant data from: {Path.GetFullPath(dataPath)}");
                var csvHandler = new CsvHandler();
                propellantData = csvHandler.Load(dataPath);

                if (!propellantData.Any()) throw new Exception("CSV file loaded, but it contains no data.");

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
                return;
            }

            // --- Full System Design Sequence ---
            try
            {
                // Hardcoded thrust value of 2224 N (equivalent to 500 lbf)
                float thrustN = 2224f;
                
                // 1. Design the main engine chamber and nozzle
                var engineDesigner = new EngineDesigner(thrustN, propellantData);
                engineDesigner.Calculate();
                
                // 2. Design the injector based on the engine's requirements
                var injectorDesigner = new InjectorDesigner(engineDesigner);
                injectorDesigner.Calculate();

                // 3. Design the turbopump based on the required flow rates and pressures
                var turbopumpDesigner = new TurbopumpDesigner(engineDesigner, injectorDesigner);
                turbopumpDesigner.Calculate();
                
                // 4. Design the gas generator needed to power the turbopump
                var ggDesigner = new GasGeneratorDesigner(turbopumpDesigner);
                ggDesigner.Calculate();

                // 5. Print all results
                PrintAllResults(engineDesigner, injectorDesigner, turbopumpDesigner, ggDesigner);

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nAn error occurred during calculation: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\n--- End of Program ---");
        }

        private static void PrintAllResults(EngineDesigner e, InjectorDesigner i, TurbopumpDesigner t, GasGeneratorDesigner g)
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine($"  COMPLETE ENGINE SYSTEM DESIGN REPORT");
            Console.WriteLine($"  Thrust: {e.DesiredThrustN:F0} N ({e.DesiredThrustN * 0.2248:F0} lbf) | Cycle: Gas-Generator");
            Console.WriteLine("==================================================");

            // --- Main Engine ---
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n--- 1. Main Combustion Chamber & Nozzle ---");
            Console.ResetColor();
            Console.WriteLine($"  Chamber Pressure:    {(e.ChamberPressurePa / 1e6):F2} MPa");
            Console.WriteLine($"  O/F Ratio:           {e.OfRatio:F2}");
            Console.WriteLine($"  Throat Diameter:     {(e.NozzleThroatDiameterM * 1000):F2} mm");
            Console.WriteLine($"  Chamber Diameter:    {(e.ChamberDiameterM * 1000):F2} mm");
            Console.WriteLine($"  Chamber Length:      {(e.ChamberLengthM * 1000):F2} mm");
            Console.WriteLine($"  Practical Wall Thickness: {(e.PracticalChamberWallThicknessM * 1000):F2} mm");

            // --- Injector ---
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n--- 2. Injector Design ---");
            Console.ResetColor();
            Console.WriteLine($"  Pressure Drop:       {(i.PressureDropPa / 1e6):F2} MPa (~{i.InjectionPressureDropFraction:P0} of Pc)");
            Console.WriteLine($"  Required Fuel Pressure:   {(i.RequiredFuelPressurePa / 1e6):F2} MPa");
            Console.WriteLine($"  Required Oxidizer Pressure: {(i.RequiredOxidizerPressurePa / 1e6):F2} MPa");
            Console.WriteLine($"  Fuel Orifices:       {i.FuelOrificeCount} x {(i.SingleFuelOrificeDiameterM * 1000):F2} mm holes");
            Console.WriteLine($"  Oxidizer Orifices:   {i.OxidizerOrificeCount} x {(i.SingleOxidizerOrificeDiameterM * 1000):F2} mm holes");

            // --- Turbopump ---
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n--- 3. Turbopump Power Requirements ---");
            Console.ResetColor();
            Console.WriteLine($"  Fuel Pump Power:     {(t.FuelPumpShaftPowerW / 1000):F2} kW");
            Console.WriteLine($"  Oxidizer Pump Power: {(t.OxidizerPumpShaftPowerW / 1000):F2} kW");
            Console.WriteLine($"  --------------------------------------");
            Console.WriteLine($"  TOTAL TURBINE POWER: {(t.RequiredTurbinePowerW / 1000):F2} kW");
            
            // --- Gas Generator ---
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n--- 4. Gas Generator Flow Requirements ---");
            Console.ResetColor();
            Console.WriteLine($"  Turbine Inlet Temp:  {g.TurbineInletTempK:F0} K");
            Console.WriteLine($"  Required GG Mass Flow: {g.RequiredGgMassFlowKgps:F4} kg/s");
            Console.WriteLine($"    -> Fuel:           {g.GgFuelMassFlowKgps:F4} kg/s");
            Console.WriteLine($"    -> Oxidizer:       {g.GgOxidizerMassFlowKgps:F4} kg/s");
            
            // --- System Totals ---
            float totalFuel = e.FuelMassFlowRateKgps + g.GgFuelMassFlowKgps;
            float totalOxidizer = e.OxidizerMassFlowRateKgps + g.GgOxidizerMassFlowKgps;
            float totalFlow = e.TotalMassFlowRateKgps + g.RequiredGgMassFlowKgps;
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n--- SYSTEM TOTAL PROPELLANT FLOW ---");
            Console.ResetColor();
            Console.WriteLine($"  Total Fuel Flow:     {totalFuel:F4} kg/s");
            Console.WriteLine($"  Total Oxidizer Flow: {totalOxidizer:F4} kg/s");
            Console.WriteLine($"  --------------------------------------");
            Console.WriteLine($"  TOTAL SYSTEM FLOW:   {totalFlow:F4} kg/s");
        }
    }
}