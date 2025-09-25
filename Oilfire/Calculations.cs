using System;
using System.Collections.Generic;
using System.Linq;

namespace Oilfire
{
    /// <summary>
    /// Performs rocket engine design calculations in SI units, based on propellant data.
    /// </summary>
    public class EngineDesigner
    {
        // --- INPUT PARAMETERS ---
        public float DesiredThrustN { get; set; }
        public float ChamberPressurePa { get; set; }
        public float SpecificImpulseSec { get; set; }
        public float OfRatio { get; set; }
        public float ChamberTempK { get; set; }
        public float LStarM { get; set; } // L* in meters
        public float ChamberToThroatDiameterRatio { get; set; }
        public float Gamma { get; set; }
        public float CharacteristicVelocityCstarMps { get; private set; }

        // --- SI PHYSICAL CONSTANTS ---
        private const float GRAVITY_g0_SI = 9.80665f; // Standard gravity, m/s^2
        private const float ALLOWABLE_STRESS_COPPER_PA = 55158058f; // 8000 psi in Pascals

        // --- CALCULATED RESULTS (in base SI units: m, kg, s, Pa, N, K) ---
        public float EffectiveExhaustVelocityMps { get; private set; }
        public float TotalMassFlowRateKgps { get; private set; }
        public float FuelMassFlowRateKgps { get; private set; }
        public float OxidizerMassFlowRateKgps { get; private set; }
        public float NozzleThroatAreaM2 { get; private set; }
        public float NozzleThroatDiameterM { get; private set; }
        public float NozzleExitAreaM2 { get; private set; }
        public float NozzleExitDiameterM { get; private set; }
        public float ChamberVolumeM3 { get; private set; }
        public float ChamberDiameterM { get; private set; }
        public float ChamberCrossSectionAreaM2 { get; private set; }
        public float ChamberLengthM { get; private set; }
        public float MinChamberWallThicknessM { get; private set; }
        public float PracticalChamberWallThicknessM { get; private set; }

        // Ae/At ratios from PDF Table III, keyed by Chamber Pressure in Pascals (approximate)
        private static readonly Dictionary<int, float> AeAtRatios = new Dictionary<int, float>
        {
            { 689476, 1.79f },   // 100 psi
            { 1378952, 2.74f },  // 200 psi
            { 2068428, 3.65f },  // 300 psi
            { 2757903, 4.60f },  // 400 psi
            { 3447379, 5.28f }   // 500 psi
        };

        public EngineDesigner(float desiredThrustN, List<PropellantData> propellantData)
        {
            if (propellantData == null || !propellantData.Any())
                throw new ArgumentException("Propellant data cannot be null or empty.");

            var optimalData = propellantData.OrderByDescending(p => p.VacIspS).First();

            DesiredThrustN = desiredThrustN;
            
            // --- Set fixed and optimal parameters in SI ---
            ChamberPressurePa = 3447379f; // 500 psi
            ChamberToThroatDiameterRatio = 3.0f;
            
            // Values from the optimal data point in the CSV
            SpecificImpulseSec = optimalData.VacIspS;
            OfRatio = optimalData.OfRatio;
            ChamberTempK = optimalData.ChamberTempK;
            Gamma = optimalData.GammaThroat;
            CharacteristicVelocityCstarMps = optimalData.CstarMS;

            // L* from PDF (60 inches) converted to meters
            LStarM = 60f * 0.0254f;
        }

        /// <summary>
        /// Executes the full design calculation sequence in SI units.
        /// </summary>
        public void Calculate()
        {
            // Step 1: Calculate Mass Flow Rates
            // F = ṁ * v_e  and  v_e = Isp * g0
            EffectiveExhaustVelocityMps = SpecificImpulseSec * GRAVITY_g0_SI;
            TotalMassFlowRateKgps = DesiredThrustN / EffectiveExhaustVelocityMps;
            FuelMassFlowRateKgps = TotalMassFlowRateKgps / (OfRatio + 1f);
            OxidizerMassFlowRateKgps = TotalMassFlowRateKgps - FuelMassFlowRateKgps;

            // Step 2: Calculate Nozzle Throat Area using C*
            // A_t = (ṁ * C*) / P_c
            NozzleThroatAreaM2 = (TotalMassFlowRateKgps * CharacteristicVelocityCstarMps) / ChamberPressurePa;

            // Step 3: Calculate Nozzle Throat Diameter
            NozzleThroatDiameterM = MathF.Sqrt(4f * NozzleThroatAreaM2 / MathF.PI);

            // Step 4: Calculate Nozzle Exit Area
            // Find the closest pressure key in the dictionary to look up the ratio
            var closestKey = AeAtRatios.Keys.OrderBy(k => Math.Abs(k - ChamberPressurePa)).First();
            float ae_at_ratio = AeAtRatios[closestKey];
            NozzleExitAreaM2 = ae_at_ratio * NozzleThroatAreaM2;

            // Step 5: Calculate Nozzle Exit Diameter
            NozzleExitDiameterM = MathF.Sqrt(4f * NozzleExitAreaM2 / MathF.PI);

            // Step 6: Calculate Combustion Chamber Volume
            // V_c = L* * A_t
            ChamberVolumeM3 = LStarM * NozzleThroatAreaM2;

            // Step 7: Calculate Chamber Dimensions
            ChamberDiameterM = ChamberToThroatDiameterRatio * NozzleThroatDiameterM;
            ChamberCrossSectionAreaM2 = MathF.PI * MathF.Pow(ChamberDiameterM, 2) / 4f;
            // V_c = 1.1 * A_c * L_c  =>  L_c = V_c / (1.1 * A_c)
            ChamberLengthM = ChamberVolumeM3 / (1.1f * ChamberCrossSectionAreaM2);

            // Step 8: Calculate Chamber Wall Thickness using Hoop Stress formula (t = P*D / (2*S))
            MinChamberWallThicknessM = (ChamberPressurePa * ChamberDiameterM) / (2f * ALLOWABLE_STRESS_COPPER_PA);
            // Practical thickness from PDF (3/32 inch) converted to meters
            PracticalChamberWallThicknessM = MinChamberWallThicknessM * 3.0f;
        }
    }
}