using System;

namespace Oilfire
{
    /// <summary>
    /// Designs the fuel and oxidizer injector orifices based on required flow rates and pressure drop.
    /// </summary>
    public class InjectorDesigner
    {
        // --- Design Choices & Assumptions ---
        public float InjectionPressureDropFraction { get; set; } = 0.20f; // 20% of chamber pressure
        public float DischargeCoefficient { get; set; } = 0.7f; // For a well-made orifice
        public int FuelOrificeCount { get; set; } = 8;
        public int OxidizerOrificeCount { get; set; } = 16;

        // --- Propellant Properties (SI Units) ---
        private const float DENSITY_ETHANOL_KG_M3 = 789f;
        private const float DENSITY_N2O_LIQUID_KG_M3 = 1220f;

        // --- Inputs from Main Engine Design ---
        private readonly EngineDesigner _engine;

        // --- Calculated Results ---
        public float PressureDropPa { get; private set; }
        public float RequiredFuelPressurePa { get; private set; }
        public float RequiredOxidizerPressurePa { get; private set; }
        public float TotalFuelOrificeAreaM2 { get; private set; }
        public float SingleFuelOrificeDiameterM { get; private set; }
        public float TotalOxidizerOrificeAreaM2 { get; private set; }
        public float SingleOxidizerOrificeDiameterM { get; private set; }

        public InjectorDesigner(EngineDesigner engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        /// <summary>
        /// Calculates the required injector parameters.
        /// </summary>
        public void Calculate()
        {
            // The pressure drop is a critical design parameter, typically 15-25% of chamber pressure.
            PressureDropPa = _engine.ChamberPressurePa * InjectionPressureDropFraction;

            // The pump must supply the chamber pressure PLUS the pressure drop.
            RequiredFuelPressurePa = _engine.ChamberPressurePa + PressureDropPa;
            RequiredOxidizerPressurePa = _engine.ChamberPressurePa + PressureDropPa;

            // Calculate orifice areas using the standard orifice flow equation:
            // ṁ = Cd * A * sqrt(2 * ρ * ΔP)  =>  A = ṁ / (Cd * sqrt(2 * ρ * ΔP))

            // For Fuel (Ethanol)
            TotalFuelOrificeAreaM2 = _engine.FuelMassFlowRateKgps / 
                (DischargeCoefficient * MathF.Sqrt(2 * DENSITY_ETHANOL_KG_M3 * PressureDropPa));
            float singleFuelOrificeAreaM2 = TotalFuelOrificeAreaM2 / FuelOrificeCount;
            SingleFuelOrificeDiameterM = MathF.Sqrt(4 * singleFuelOrificeAreaM2 / MathF.PI);

            // For Oxidizer (N2O)
            TotalOxidizerOrificeAreaM2 = _engine.OxidizerMassFlowRateKgps /
                (DischargeCoefficient * MathF.Sqrt(2 * DENSITY_N2O_LIQUID_KG_M3 * PressureDropPa));
            float singleOxidizerOrificeAreaM2 = TotalOxidizerOrificeAreaM2 / OxidizerOrificeCount;
            SingleOxidizerOrificeDiameterM = MathF.Sqrt(4 * singleOxidizerOrificeAreaM2 / MathF.PI);
        }
    }
}
