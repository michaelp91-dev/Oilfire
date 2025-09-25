using System;

namespace Oilfire
{
    /// <summary>
    /// Performs preliminary design calculations for the fuel and oxidizer turbopumps.
    /// </summary>
    public class TurbopumpDesigner
    {
        // --- Design Choices & Assumptions ---
        public float PumpEfficiency { get; set; } = 0.65f; // 65% is reasonable for small centrifugal pumps
        public float GearboxEfficiency { get; set; } = 0.98f; // Assuming a simple gearbox or direct drive
        public float TankPressurePa { get; set; } = 200000f; // ~2 bar tank pressure

        // --- Inputs from other designers ---
        private readonly EngineDesigner _engine;
        private readonly InjectorDesigner _injector;

        // --- Calculated Results ---
        public float FuelPumpHeadPa { get; private set; } // Pressure rise needed from pump
        public float OxidizerPumpHeadPa { get; private set; }
        public float FuelPumpHydraulicPowerW { get; private set; }
        public float OxidizerPumpHydraulicPowerW { get; private set; }
        public float FuelPumpShaftPowerW { get; private set; } // Power required at the shaft
        public float OxidizerPumpShaftPowerW { get; private set; }
        public float RequiredTurbinePowerW { get; private set; } // Total power turbine must generate

        public TurbopumpDesigner(EngineDesigner engine, InjectorDesigner injector)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _injector = injector ?? throw new ArgumentNullException(nameof(injector));
        }

        public void Calculate()
        {
            // The pump "head" is the pressure rise it must provide.
            FuelPumpHeadPa = _injector.RequiredFuelPressurePa - TankPressurePa;
            OxidizerPumpHeadPa = _injector.RequiredOxidizerPressurePa - TankPressurePa;

            // Hydraulic Power = (ṁ * ΔP) / ρ
            // This is the ideal power transferred to the fluid.
            FuelPumpHydraulicPowerW = (_engine.FuelMassFlowRateKgps * FuelPumpHeadPa) / 789f; // Ethanol density
            OxidizerPumpHydraulicPowerW = (_engine.OxidizerMassFlowRateKgps * OxidizerPumpHeadPa) / 1220f; // N2O density

            // Shaft Power = Hydraulic Power / Pump Efficiency
            // This is the actual mechanical power needed to drive the pump.
            FuelPumpShaftPowerW = FuelPumpHydraulicPowerW / PumpEfficiency;
            OxidizerPumpShaftPowerW = OxidizerPumpHydraulicPowerW / PumpEfficiency;

            // The turbine must provide enough power for both pumps, accounting for gearbox losses.
            RequiredTurbinePowerW = (FuelPumpShaftPowerW + OxidizerPumpShaftPowerW) / GearboxEfficiency;
        }
    }
}
