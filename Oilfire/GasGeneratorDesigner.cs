using System;

namespace Oilfire
{
    /// <summary>
    /// Performs preliminary design calculations for the gas generator.
    /// </summary>
    public class GasGeneratorDesigner
    {
        // --- Design Choices & Assumptions for the GG cycle ---
        public float TurbineInletTempK { get; set; } = 950f; // A safe temperature for uncooled turbine blades
        public float TurbineEfficiency { get; set; } = 0.75f; // 75% efficiency
        public float TurbinePressureRatio { get; set; } = 15.0f; // P_in / P_out
        
        // --- Properties of fuel-rich GG exhaust gas (approximations) ---
        public float GgExhaustGamma { get; set; } = 1.25f;
        public float GgExhaustSpecificHeatCp { get; set; } = 2500f; // J/(kg·K)

        // --- Inputs ---
        private readonly TurbopumpDesigner _turbopump;

        // --- Calculated Results ---
        public float RequiredGgMassFlowKgps { get; private set; }
        public float GgFuelMassFlowKgps { get; private set; }
        public float GgOxidizerMassFlowKgps { get; private set; }

        public GasGeneratorDesigner(TurbopumpDesigner turbopump)
        {
            _turbopump = turbopump ?? throw new ArgumentNullException(nameof(turbopump));
        }

        public void Calculate()
        {
            // To find the mass flow, we use the turbine power equation:
            // P_turbine = ṁ_gg * c_p * ΔT * η_turbine
            // We need to find ΔT (the temperature drop across the turbine) first.
            
            // Calculate isentropic temperature drop using the pressure ratio
            float gammaExponent = (GgExhaustGamma - 1) / GgExhaustGamma;
            float turbineExitTempK = TurbineInletTempK * (1 / MathF.Pow(TurbinePressureRatio, gammaExponent));
            float deltaTempK = TurbineInletTempK - turbineExitTempK;

            // Now we can solve for the required mass flow rate for the gas generator
            // ṁ_gg = P_turbine / (c_p * ΔT * η_turbine)
            RequiredGgMassFlowKgps = _turbopump.RequiredTurbinePowerW / 
                (GgExhaustSpecificHeatCp * deltaTempK * TurbineEfficiency);

            // We assume the gas generator uses the same propellants, but at a specific O/F ratio
            // chosen to produce the desired turbine inlet temperature. For simplicity, we can
            // assume a fuel-rich O/F ratio, e.g., 1.0.
            float ggOfRatio = 1.0f;
            GgFuelMassFlowKgps = RequiredGgMassFlowKgps / (ggOfRatio + 1);
            GgOxidizerMassFlowKgps = RequiredGgMassFlowKgps - GgFuelMassFlowKgps;
        }
    }
}
