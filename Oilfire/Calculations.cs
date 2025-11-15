using System;
using System.Collections.Generic;

namespace RocketEngineBuilder
{
    public class Calculations
    {
        float fGravitationalConstantFT_S2 = 32.174f;
        public Calculations(
            float fThrust,
            float fOFRatio,
            float fChamberPressure_PSI,
            PropellantDataService propellantDataService
        )
        {
            var propellantData = propellantDataService.FindClosest(fOFRatio, fChamberPressure_PSI);
            m_fThrust = fThrust;
            m_fOFRatio = fOFRatio;
            m_fChamberPressure_PSI = fChamberPressure_PSI;
            m_fChamberTemp_R = propellantData.Chamber_Temp_R;
            m_fThroatTemp_R = propellantData.Throat_Temp_R;
            m_fGammaChamber = propellantData.Gamma_Chamber;
            m_fIsp = propellantData.Isp;
            m_fExpansionRatio = propellantData.Expansion_Ratio;

            m_dictInputs = new Dictionary<string, float>
            {
                { "Thrust", m_fThrust },
                { "OFRatio", m_fOFRatio },
                { "ChamberPressure_PSI", m_fChamberPressure_PSI },
                { "ChamberTemp_R", m_fChamberTemp_R },
                { "ThroatTemp_R", m_fThroatTemp_R },
                { "GammaChamber", m_fGammaChamber },
                { "Isp", m_fIsp },
                { "ExpansionRatio", m_fExpansionRatio }
            };
        }

        public void CalculateCombustionChamber()
        {
            float LSTAR = 60.0f;
            float GAS_CONSTANT = 65.5f;
            float GRAVITY_CONSTANT = 32.174f;
            float HEAT_TRANSFER_RATE = 3.0f;
            float WATER_TEMPERATURE_RISE = 40.0f;
            float WATER_VELOCITY = 30.0f;
            float WATER_RHO = 62.4f;
            float CHAMBER_TO_THROAT_RATIO = 3.0f;
            float COPPER_STRESS = 8000.0f;
            float CONVERGENT_ANGLE_DEG = 45.0f;
            float DIVERGENT_ANGLE_DEG = 15.0f;

            m_fTotalWeightFlow = m_fThrust / m_fIsp;
            m_fFuelWeightFlow = m_fTotalWeightFlow / (1.0f + m_fOFRatio);
            m_fOxWeightFlow = m_fTotalWeightFlow - m_fFuelWeightFlow;
            m_fThroatPressure_PSI = m_fChamberPressure_PSI * 0.564f;
            m_fThroatArea_in2 = (m_fTotalWeightFlow / m_fThroatPressure_PSI) * (float)Math.Sqrt((GAS_CONSTANT * m_fThroatTemp_R) / (GRAVITY_CONSTANT * m_fGammaChamber));
            m_fThroatDiameter_in = (float)Math.Sqrt((m_fThroatArea_in2 * 4.0f) / Math.PI);
            m_fExitArea_in2 = m_fThroatArea_in2 * m_fExpansionRatio;
            m_fExitDiameter_in = (float)Math.Sqrt((m_fExitArea_in2 * 4.0f) / Math.PI);
            m_fChamberVolume = LSTAR * m_fThroatArea_in2;
            m_fChamberDiameter_in = m_fThroatDiameter_in * CHAMBER_TO_THROAT_RATIO;
            m_fChamberArea_in2 = (float)(Math.PI * Math.Pow(m_fChamberDiameter_in / 2.0f, 2.0f));
            m_fChamberLength_in = m_fChamberVolume / (1.1f * m_fChamberArea_in2);
            m_fMinimumWallThickness_in = (m_fChamberPressure_PSI * m_fChamberDiameter_in) / (2.0f * COPPER_STRESS);
            m_fWallThickness_in = m_fMinimumWallThickness_in * 1.5f;
            m_fConvergentLength_in = ((m_fChamberDiameter_in / 2) - (m_fThroatDiameter_in / 2)) / (float)Math.Tan(CONVERGENT_ANGLE_DEG * ((float)Math.PI / 180.0f));
            m_fDivergentLength_in = ((m_fExitDiameter_in / 2) - (m_fThroatDiameter_in / 2)) / (float)Math.Tan(DIVERGENT_ANGLE_DEG * ((float)Math.PI / 180.0f));
            m_fChamberSurfaceArea = 2 * (float)Math.PI * ((m_fChamberDiameter_in + m_fWallThickness_in) / 2) * m_fChamberLength_in + 2 * (float)Math.PI * (float)Math.Pow((m_fChamberDiameter_in + m_fWallThickness_in) / 2, 2.0f);
            m_fConvergentSurfaceArea = (float)Math.PI * (float)Math.Pow(((m_fChamberDiameter_in / 2.0f) + m_fWallThickness_in), 2.0) + (float)Math.PI * (float)Math.Pow(((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in), 2.0) + (float)Math.PI * ((((m_fChamberDiameter_in / 2.0f) + m_fWallThickness_in) + ((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in))) * (float)Math.Sqrt((float)Math.Pow(m_fConvergentLength_in, 2.0) + (float)Math.Pow((((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in) - ((m_fChamberDiameter_in / 2.0f) + m_fWallThickness_in)), 2.0));
            m_fDivergentSurfaceArea = (float)Math.PI * (float)Math.Pow(((m_fExitDiameter_in / 2.0f) + m_fWallThickness_in), 2.0) + (float)Math.PI * (float)Math.Pow(((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in), 2.0) + (float)Math.PI * ((((m_fExitDiameter_in / 2.0f) + m_fWallThickness_in) + ((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in))) * (float)Math.Sqrt((float)Math.Pow(m_fDivergentLength_in, 2.0) + (float)Math.Pow((((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in) - ((m_fExitDiameter_in / 2.0f) + m_fWallThickness_in)), 2.0));
            m_fTotalSurfaceArea = m_fChamberSurfaceArea + m_fConvergentSurfaceArea + m_fDivergentSurfaceArea;
            m_fHeatTransferred = m_fTotalSurfaceArea * HEAT_TRANSFER_RATE;
            m_fWaterFlowWeight = m_fHeatTransferred / WATER_TEMPERATURE_RISE;
            m_fChamberOuterDiameter_in = m_fChamberDiameter_in + (2.0f * m_fWallThickness_in);
            m_fChamberOuterDiameter_ft = m_fChamberOuterDiameter_in / 12.0f;
            m_fWaterJacketDiameter_ft = (float)Math.Sqrt((4 * m_fWaterFlowWeight) / (WATER_VELOCITY * WATER_RHO * (float)Math.PI) + m_fChamberOuterDiameter_ft * m_fChamberOuterDiameter_ft);
            m_fWaterJacketDiameter_in = m_fWaterJacketDiameter_ft * 12.0f;
            m_fWaterFlowGap_in = m_fWaterJacketDiameter_in - m_fChamberOuterDiameter_in;

            m_fChamberLength_mm = m_fChamberLength_in * 25.4f;
            m_fChamberDiameter_mm = m_fChamberDiameter_in * 25.4f;
            m_fThroatDiameter_mm = m_fThroatDiameter_in * 25.4f;
            m_fExitDiameter_mm = m_fExitDiameter_in * 25.4f;
            m_fConvergentLength_mm = m_fConvergentLength_in * 25.4f;
            m_fDivergentLength_mm = m_fDivergentLength_in * 25.4f;
            m_fWallThickness_mm = m_fWallThickness_in * 25.4f;
            m_fChamberRadius_mm = m_fChamberDiameter_mm / 2.0f;
            m_fThroatRadius_mm = m_fThroatDiameter_mm / 2.0f;
            m_fExitRadius_mm = m_fExitDiameter_mm / 2.0f;

            m_dictResults = new Dictionary<string, float>
            {
                { "TotalWeightFlow", m_fTotalWeightFlow },
                { "FuelWeightFlow", m_fFuelWeightFlow },
                { "OxWeightFlow", m_fOxWeightFlow },
                { "ThroatPressure_PSI", m_fThroatPressure_PSI },
                { "ThroatArea_in2", m_fThroatArea_in2 },
                { "ThroatDiameter_in", m_fThroatDiameter_in },
                { "ExitArea_in2", m_fExitArea_in2 },
                { "ExitDiameter_in", m_fExitDiameter_in },
                { "ChamberVolume", m_fChamberVolume },
                { "ChamberDiameter_in", m_fChamberDiameter_in },
                { "ChamberLength_in", m_fChamberLength_in },
                { "ChamberArea_in2", m_fChamberArea_in2 },
                { "MinimumWallThickness_in", m_fMinimumWallThickness_in },
                { "WallThickness_in", m_fWallThickness_in },
                { "ConvergentLength_in", m_fConvergentLength_in },
                { "DivergentLength_in", m_fDivergentLength_in },
                { "ChamberSurfaceArea", m_fChamberSurfaceArea },
                { "ConvergentSurfaceArea", m_fConvergentSurfaceArea },
                { "DivergentSurfaceArea", m_fDivergentSurfaceArea },
                { "TotalSurfaceArea", m_fTotalSurfaceArea },
                { "HeatTransferred", m_fHeatTransferred },
                { "WaterFlowWeight", m_fWaterFlowWeight },
                { "ChamberOuterDiameter_in", m_fChamberOuterDiameter_in },
                { "ChamberOuterDiameter_ft", m_fChamberOuterDiameter_ft },
                { "WaterJacketDiameter_ft", m_fWaterJacketDiameter_ft },
                { "WaterJacketDiameter_in", m_fWaterJacketDiameter_in },
                { "WaterFlowGap_in", m_fWaterFlowGap_in },
                { "ChamberLength_mm", m_fChamberLength_mm },
                { "ChamberDiameter_mm", m_fChamberDiameter_mm },
                { "ThroatDiameter_mm", m_fThroatDiameter_mm },
                { "ExitDiameter_mm", m_fExitDiameter_mm },
                { "ConvergentLength_mm", m_fConvergentLength_mm },
                { "DivergentLength_mm", m_fDivergentLength_mm },
                { "WallThickness_mm", m_fWallThickness_mm },
                { "ChamberRadius_mm", m_fChamberRadius_mm },
                { "ThroatRadius_mm", m_fThroatRadius_mm },
                { "ExitRadius_mm", m_fExitRadius_mm }
            };
        }

        public Dictionary<string, float> DictGetCalculations()
        {
            return m_dictResults;
        }

        public void PrintResults()
        {
            Console.WriteLine("Combustion Chamber Calculations:");
            foreach (var entry in m_dictResults)
            {
                Console.WriteLine($"{entry.Key}: {entry.Value}");
            }
        }

        private float m_fThrust;
        private float m_fOFRatio;
        private float m_fChamberPressure_PSI;
        private float m_fChamberTemp_R;
        private float m_fThroatTemp_R;
        private float m_fGammaChamber;
        private float m_fIsp;
        private float m_fExpansionRatio;
        private float m_fTotalWeightFlow;
        private float m_fFuelWeightFlow;
        private float m_fOxWeightFlow;
        private float m_fThroatPressure_PSI;
        private float m_fThroatArea_in2;
        private float m_fThroatDiameter_in;
        private float m_fExitArea_in2;
        private float m_fExitDiameter_in;
        private float m_fChamberVolume;
        private float m_fChamberDiameter_in;
        private float m_fChamberLength_in;
        private float m_fChamberArea_in2;
        private float m_fMinimumWallThickness_in;
        private float m_fWallThickness_in;
        private float m_fConvergentLength_in;
        private float m_fDivergentLength_in;
        private float m_fChamberSurfaceArea;
        private float m_fConvergentSurfaceArea;
        private float m_fDivergentSurfaceArea;
        private float m_fTotalSurfaceArea;
        private float m_fHeatTransferred;
        private float m_fWaterFlowWeight;
        private float m_fChamberOuterDiameter_in;
        private float m_fChamberOuterDiameter_ft;
        private float m_fWaterJacketDiameter_ft;
        private float m_fWaterJacketDiameter_in;
        private float m_fWaterFlowGap_in;
        private float m_fChamberLength_mm;
        private float m_fChamberDiameter_mm;
        private float m_fThroatDiameter_mm;
        private float m_fExitDiameter_mm;
        private float m_fConvergentLength_mm;
        private float m_fDivergentLength_mm;
        private float m_fWallThickness_mm;
        private float m_fChamberRadius_mm;
        private float m_fThroatRadius_mm;
        private float m_fExitRadius_mm;

        private Dictionary<string, float> m_dictResults;
        private Dictionary<string, float> m_dictInputs;

        public void CalculateInjector(float fFuelDensityLB_FT3, float fFuelInjectorCd, float fFuelInjectorDeltaPPSI, int iNumFuelInjectorHoles,
            float fOxygenDensityLB_FT3, float fOxygenInjectorCd, float fOxygenInjectorDeltaPPSI, int iNumOxygenInjectorHoles)
        {
            m_fFuelFlowAreaFT2 = m_fFuelWeightFlow / (fFuelInjectorCd * ((float)Math.Pow(2 * fGravitationalConstantFT_S2 * fFuelDensityLB_FT3 * fFuelInjectorDeltaPPSI, 0.5) * 12));
            m_fFuelHoleAreaFT2 = m_fFuelFlowAreaFT2 / iNumFuelInjectorHoles;
            m_fFuelHoleDiameterFT = (float)Math.Pow((4 * m_fFuelHoleAreaFT2) / Math.PI, 0.5);

            m_fOxygenFlowAreaFT2 = m_fOxWeightFlow / (fOxygenInjectorCd * ((float)Math.Pow(2 * fGravitationalConstantFT_S2 * fOxygenDensityLB_FT3 * fOxygenInjectorDeltaPPSI, 0.5) * 12));
            m_fOxygenHoleAreaFT2 = m_fOxygenFlowAreaFT2 / iNumOxygenInjectorHoles;
            m_fOxygenHoleDiameterFT = (float)Math.Pow((4 * m_fOxygenHoleAreaFT2) / Math.PI, 0.5);

            m_fFuelFlowAreaM2_OUT = m_fFuelFlowAreaFT2 * 0.092903f;
            m_fFuelHoleAreaM2_OUT = m_fFuelHoleAreaFT2 * 0.092903f;
            m_fFuelHoleDiameterMM_OUT = m_fFuelHoleDiameterFT * 0.3048f * 1000f;
            m_fOxygenFlowAreaM2_OUT = m_fOxygenFlowAreaFT2 * 0.092903f;
            m_fOxygenHoleAreaM2_OUT = m_fOxygenHoleAreaFT2 * 0.092903f;
            m_fOxygenHoleDiameterMM_OUT = m_fOxygenHoleDiameterFT * 0.3048f * 1000f;

            m_dictResults.Add("FuelFlowArea_FT2", m_fFuelFlowAreaFT2);
            m_dictResults.Add("OxidizerFlowArea_FT2", m_fOxygenFlowAreaFT2);
            m_dictResults.Add("FuelHoleArea_FT2", m_fFuelHoleAreaFT2);
            m_dictResults.Add("OxidizerHoleArea_FT2", m_fOxygenHoleAreaFT2);
            m_dictResults.Add("FuelHoleDiameter_FT", m_fFuelHoleDiameterFT);
            m_dictResults.Add("OxidizerHoleDiameter_FT", m_fOxygenHoleDiameterFT);
            m_dictResults.Add("FuelFlowArea_M2", m_fFuelFlowAreaM2_OUT);
            m_dictResults.Add("OxidizerFlowArea_M2", m_fOxygenFlowAreaM2_OUT);
            m_dictResults.Add("FuelHoleArea_M2", m_fFuelHoleAreaM2_OUT);
            m_dictResults.Add("OxidizerHoleArea_M2", m_fOxygenHoleAreaM2_OUT);
            m_dictResults.Add("FuelHoleDiameter_MM", m_fFuelHoleDiameterMM_OUT);
            m_dictResults.Add("OxidizerHoleDiameter_MM", m_fOxygenHoleDiameterMM_OUT);
        }

        private float m_fFuelFlowAreaFT2;
        private float m_fFuelHoleAreaFT2;
        private float m_fFuelHoleDiameterFT;
        private float m_fOxygenFlowAreaFT2;
        private float m_fOxygenHoleAreaFT2;
        private float m_fOxygenHoleDiameterFT;
        private float m_fFuelFlowAreaM2_OUT;
        private float m_fFuelHoleAreaM2_OUT;
        private float m_fFuelHoleDiameterMM_OUT;
        private float m_fOxygenFlowAreaM2_OUT;
        private float m_fOxygenHoleAreaM2_OUT;
        private float m_fOxygenHoleDiameterMM_OUT;
    }
    
}

/// <summary>
/// Encapsulates all calculations for the Turbopump.
/// </summary>
public class TurbopumpCalculations
{
    // MATERIAL PROPERTIES (typical values)
    private const float SHAFT_MATERIAL_YIELD_PSI = 60000f;
    private const float IMPELLER_MATERIAL_DENSITY_LBM_IN3 = 0.283f;
    private const float IMPELLER_MATERIAL_YIELD_PSI = 40000f;
    private const float CASING_MATERIAL_ALLOWABLE_PSI = 20000f;
    private const float JOINT_EFFICIENCY = 1.0f;

    // GEOMETRY RATIOS (typical design values)
    private const float EYE_DIAMETER_RATIO = 0.35f;
    private const float OUTLET_WIDTH_RATIO = 0.08f;

    private const float G_FT_S2 = 32.2f;
    private const int NUMBER_OF_STAGES = 1;

    // Calculated results - all public properties
    public float VolumetricFlowRate_FT3_S { get; set; }
    public float VolumetricFlowRate_GPM { get; set; }
    public float PressureRise_PSI { get; set; }
    public float TotalHead_FT { get; set; }
    public float HeadPerStage_FT { get; set; }
    public float HydraulicPower_FT_LBF_S { get; set; }
    public float HydraulicPower_HP { get; set; }
    public float ShaftPower_HP { get; set; }
    public float ShaftPower_KW { get; set; }
    public float SpecificSpeed_US { get; set; }
    public float SpecificSpeedPerStage { get; set; }
    public float ImpellerTipSpeed_FT_S { get; set; }
    public float HeadCoefficient { get; set; }
    public float FlowCoefficient { get; set; }
    public float InletVelocity_FT_S { get; set; }
    public float InletPipeDiameter_IN { get; set; }
    public float NPSH_Available_FT { get; set; }
    public float SuctionSpecificSpeed { get; set; }
    public float ImpellerDiameter_FT { get; set; }
    public float ReynoldsNumber { get; set; }
    public float ImpellerEyeDiameter_IN { get; set; }
    public float ImpellerOutletWidth_IN { get; set; }
    public float BladeOutletAngle_RAD { get; set; }
    public float TangentialVelocity_FT_S { get; set; }
    public float MeridionalVelocity_FT_S { get; set; }
    public float TheoreticalHead_FT { get; set; }
    public float SlipFactor { get; set; }
    public float ActualHeadPerStage_FT { get; set; }
    public float InletHubDiameter_IN { get; set; }
    public float InletShroudDiameter_IN { get; set; }
    public float InletBladeHeight_IN { get; set; }
    public float InletFlowArea_IN2 { get; set; }
    public float InletMeridionalVelocity_FT_S { get; set; }
    public float InletBladeAngle_DEG { get; set; }
    public float InletBladeAngle_RAD { get; set; }
    public float OutletDiameter_IN { get; set; }
    public float OutletBladeHeight_IN { get; set; }
    public float OutletFlowArea_IN2 { get; set; }
    public float OutletMeridionalVelocity_FT_S { get; set; }
    public float OutletBladeAngle_RAD { get; set; }
    public float OutletTangentialVelocity_FT_S { get; set; }
    public float OutletRelativeVelocity_FT_S { get; set; }
    public float BladeRadialLength_IN { get; set; }
    public float BladeThickness_IN { get; set; }
    public float BladeLeadingEdgeChamfer_IN { get; set; }
    public float BladeTrailingEdgeChamfer_IN { get; set; }
    public float VoluteThroatArea_IN2 { get; set; }
    public float VoluteThroatWidth_IN { get; set; }
    public float VoluteThroatHeight_IN { get; set; }
    public float VoluteBaseCircleDia_IN { get; set; }
    public float VoluteCutwaterClearance_IN { get; set; }
    public float DischargeNozzleDiameter_IN { get; set; }
    public int DiffuserVaneCount { get; set; }
    public float DiffuserInletDiameter_IN { get; set; }
    public float DiffuserOutletDiameter_IN { get; set; }
    public float DiffuserVaneAngle_DEG { get; set; }
    public float DiffuserPassageWidth_IN { get; set; }
    public float FrontShroudArea_IN2 { get; set; }
    public float BackShroudArea_IN2 { get; set; }
    public float AxialThrust_LBF { get; set; }
    public string BalanceHolesRequired { get; set; }
    public int NumberOfBalanceHoles { get; set; }
    public float BalanceHoleDiameter_IN { get; set; }
    public float ShaftTorque_LBF_IN { get; set; }
    public float ShaftShearStress_PSI { get; set; }
    public float ShaftSafetyFactor { get; set; }
    public float BearingLoad_LBF { get; set; }
    public float ImpellerTipStress_PSI { get; set; }
    public float ImpellerStressSafetyFactor { get; set; }
    public float MaxSafeRPM { get; set; }
    public float CasingOuterDiameter_IN { get; set; }
    public float CasingWallThickness_IN { get; set; }
    public float MinimumWallThickness_IN { get; set; }
    public float SealDiameter_IN { get; set; }
    public float SealFaceVelocity_FT_MIN { get; set; }
    public float SealFacePressure_PSI { get; set; }
    public string SealType { get; set; }
    public float RadialClearance_IN { get; set; }
    public float AxialClearance_IN { get; set; }
    public float SurfaceFinish_Ra_MICROINCH { get; set; }
    public float TipClearance_IN { get; set; }

    // Design parameters
    public float ImpellerRPM { get; set; }
    public float ImpellerDiameter_IN { get; set; }
    public int NumberOfBlades { get; set; }
    public float BladeOutletAngle_DEG { get; set; }
    public float ShaftDiameter_IN { get; set; }

    // MM Dimensions
    public float ImpellerDiameter_MM { get; set; }
    public float ImpellerEyeDiameter_MM { get; set; }
    public float ImpellerOutletWidth_MM { get; set; }
    public float BladeRadialLength_MM { get; set; }
    public float InletBladeHeight_MM { get; set; }
    public float OutletBladeHeight_MM { get; set; }
    public float BladeThickness_MM { get; set; }
    public float BladeLeadingEdgeChamfer_MM { get; set; }
    public float BladeTrailingEdgeChamfer_MM { get; set; }
    public float InletHubDiameter_MM { get; set; }
    public float InletShroudDiameter_MM { get; set; }
    public float InletPipeDiameter_MM { get; set; }
    public float OutletDiameter_MM { get; set; }
    public float VoluteBaseCircleDia_MM { get; set; }
    public float VoluteThroatWidth_MM { get; set; }
    public float VoluteThroatHeight_MM { get; set; }
    public float VoluteCutwaterClearance_MM { get; set; }
    public float DischargeNozzleDiameter_MM { get; set; }
    public float CasingOuterDiameter_MM { get; set; }
    public float MinimumWallThickness_MM { get; set; }
    public float ShaftDiameter_MM { get; set; }
    public float SealDiameter_MM { get; set; }
    public float BalanceHoleDiameter_MM { get; set; }
    public float DiffuserInletDiameter_MM { get; set; }
    public float DiffuserOutletDiameter_MM { get; set; }
    public float DiffuserPassageWidth_MM { get; set; }
    public float RadialClearance_MM { get; set; }
    public float AxialClearance_MM { get; set; }
    public float TipClearance_MM { get; set; }

    /// <summary>
    /// Perform all turbopump calculations.
    /// </summary>
    public void Calculate(float massFlowRate_LBM_S, float dischargePressure_PSI,
                         float inletPressure_PSI, float fluidDensity_LBM_FT3,
                         float pumpEfficiency, float kinematicViscosity_CST,
                         float vaporPressure_PSI)
    {
        CalculateDesignParameters(massFlowRate_LBM_S, dischargePressure_PSI, inletPressure_PSI,
                                 fluidDensity_LBM_FT3, pumpEfficiency);

        VolumetricFlowRate_FT3_S = massFlowRate_LBM_S / fluidDensity_LBM_FT3;
        VolumetricFlowRate_GPM = VolumetricFlowRate_FT3_S * 448.831f;
        PressureRise_PSI = dischargePressure_PSI - inletPressure_PSI;

        TotalHead_FT = (PressureRise_PSI * 144f) / (fluidDensity_LBM_FT3 * G_FT_S2);
        HeadPerStage_FT = TotalHead_FT / NUMBER_OF_STAGES;

        HydraulicPower_FT_LBF_S = massFlowRate_LBM_S * TotalHead_FT * G_FT_S2;
        HydraulicPower_HP = HydraulicPower_FT_LBF_S / 550f;
        ShaftPower_HP = HydraulicPower_HP / pumpEfficiency;
        ShaftPower_KW = ShaftPower_HP * 0.7457f;

        SpecificSpeed_US = ImpellerRPM * (float)Math.Sqrt(VolumetricFlowRate_GPM) / (float)Math.Pow(TotalHead_FT, 0.75);
        SpecificSpeedPerStage = ImpellerRPM * (float)Math.Sqrt(VolumetricFlowRate_GPM) / (float)Math.Pow(HeadPerStage_FT, 0.75);

        ImpellerTipSpeed_FT_S = ((float)Math.PI * ImpellerDiameter_IN * ImpellerRPM) / 720f;

        HeadCoefficient = (G_FT_S2 * HeadPerStage_FT) / (ImpellerTipSpeed_FT_S * ImpellerTipSpeed_FT_S);

        FlowCoefficient = VolumetricFlowRate_FT3_S / ((ImpellerDiameter_IN / 12f) * (ImpellerDiameter_IN / 12f) * ImpellerTipSpeed_FT_S);

        InletVelocity_FT_S = 10f;
        InletPipeDiameter_IN = (float)Math.Sqrt((VolumetricFlowRate_FT3_S * 183.346f) / ((float)Math.PI * InletVelocity_FT_S));
        NPSH_Available_FT = ((inletPressure_PSI - vaporPressure_PSI) * 144f) / (fluidDensity_LBM_FT3 * G_FT_S2) + (InletVelocity_FT_S * InletVelocity_FT_S) / (2f * G_FT_S2);
        SuctionSpecificSpeed = ImpellerRPM * (float)Math.Sqrt(VolumetricFlowRate_GPM) / (float)Math.Pow(NPSH_Available_FT, 0.75);

        ImpellerDiameter_FT = ImpellerDiameter_IN / 12f;
        ReynoldsNumber = (ImpellerTipSpeed_FT_S * ImpellerDiameter_FT) / (kinematicViscosity_CST * 0.00001076f);

        ImpellerEyeDiameter_IN = ImpellerDiameter_IN * EYE_DIAMETER_RATIO;
        ImpellerOutletWidth_IN = ImpellerDiameter_IN * OUTLET_WIDTH_RATIO;

        BladeOutletAngle_RAD = BladeOutletAngle_DEG * (float)Math.PI / 180f;
        TangentialVelocity_FT_S = ImpellerTipSpeed_FT_S;
        MeridionalVelocity_FT_S = VolumetricFlowRate_FT3_S / ((float)Math.PI * (ImpellerDiameter_IN / 12f) * (ImpellerOutletWidth_IN / 12f));
        TheoreticalHead_FT = (TangentialVelocity_FT_S * TangentialVelocity_FT_S) / G_FT_S2;
        SlipFactor = 1f - (2.0f / NumberOfBlades);
        ActualHeadPerStage_FT = TheoreticalHead_FT * SlipFactor;

        InletHubDiameter_IN = ImpellerEyeDiameter_IN * 0.4f;
        InletShroudDiameter_IN = ImpellerEyeDiameter_IN;
        InletBladeHeight_IN = (InletShroudDiameter_IN - InletHubDiameter_IN) / 2f;
        InletFlowArea_IN2 = (float)Math.PI * ((InletShroudDiameter_IN * InletShroudDiameter_IN) - (InletHubDiameter_IN * InletHubDiameter_IN)) / 4f;
        InletMeridionalVelocity_FT_S = VolumetricFlowRate_FT3_S / (InletFlowArea_IN2 / 144f);
        InletBladeAngle_DEG = 90f;
        InletBladeAngle_RAD = InletBladeAngle_DEG * (float)Math.PI / 180f;

        OutletDiameter_IN = ImpellerDiameter_IN;
        OutletBladeHeight_IN = ImpellerOutletWidth_IN;
        OutletFlowArea_IN2 = (float)Math.PI * OutletDiameter_IN * OutletBladeHeight_IN;
        OutletMeridionalVelocity_FT_S = VolumetricFlowRate_FT3_S / (OutletFlowArea_IN2 / 144f);
        OutletBladeAngle_RAD = BladeOutletAngle_DEG * (float)Math.PI / 180f;
        OutletTangentialVelocity_FT_S = (float)Math.PI * (OutletDiameter_IN / 12f) * ImpellerRPM / 60f;
        OutletRelativeVelocity_FT_S = OutletMeridionalVelocity_FT_S / (float)Math.Sin(OutletBladeAngle_RAD);

        BladeRadialLength_IN = (OutletDiameter_IN - InletShroudDiameter_IN) / 2f;
        BladeThickness_IN = 0.06f;
        BladeLeadingEdgeChamfer_IN = 0.03f;
        BladeTrailingEdgeChamfer_IN = 0.03f;

        VoluteThroatArea_IN2 = 1.1f * (VolumetricFlowRate_FT3_S * 144f) / OutletMeridionalVelocity_FT_S;
        VoluteThroatWidth_IN = (float)Math.Sqrt(VoluteThroatArea_IN2 / 1.5f);
        VoluteThroatHeight_IN = VoluteThroatArea_IN2 / VoluteThroatWidth_IN;
        VoluteBaseCircleDia_IN = OutletDiameter_IN + (2f * OutletBladeHeight_IN) + 0.25f;
        VoluteCutwaterClearance_IN = 0.1f * OutletDiameter_IN;
        DischargeNozzleDiameter_IN = (float)Math.Sqrt((VolumetricFlowRate_FT3_S * 183.346f) / ((float)Math.PI * 30f));

        DiffuserVaneCount = NumberOfBlades + 3;
        DiffuserInletDiameter_IN = OutletDiameter_IN + 0.15f;
        DiffuserOutletDiameter_IN = DiffuserInletDiameter_IN * 1.5f;
        DiffuserVaneAngle_DEG = 12f;
        DiffuserPassageWidth_IN = OutletBladeHeight_IN * 1.1f;

        FrontShroudArea_IN2 = (float)Math.PI * (OutletDiameter_IN * OutletDiameter_IN) / 4f;
        BackShroudArea_IN2 = (float)Math.PI * ((OutletDiameter_IN * OutletDiameter_IN) - (InletHubDiameter_IN * InletHubDiameter_IN)) / 4f;
        AxialThrust_LBF = (dischargePressure_PSI * FrontShroudArea_IN2) - (inletPressure_PSI * BackShroudArea_IN2);
        BalanceHolesRequired = AxialThrust_LBF > 100f ? "YES" : "NO";
        NumberOfBalanceHoles = 4;
        BalanceHoleDiameter_IN = 0.125f;

        ShaftTorque_LBF_IN = (ShaftPower_HP * 63025f) / ImpellerRPM;
        ShaftShearStress_PSI = (16f * ShaftTorque_LBF_IN) / ((float)Math.PI * (ShaftDiameter_IN * ShaftDiameter_IN * ShaftDiameter_IN));
        ShaftSafetyFactor = SHAFT_MATERIAL_YIELD_PSI / ShaftShearStress_PSI;
        BearingLoad_LBF = AxialThrust_LBF + (2f * massFlowRate_LBM_S * OutletTangentialVelocity_FT_S / G_FT_S2);

        ImpellerTipStress_PSI = IMPELLER_MATERIAL_DENSITY_LBM_IN3 * ((ImpellerTipSpeed_FT_S * 12f) * (ImpellerTipSpeed_FT_S * 12f)) / (12f * G_FT_S2);
        ImpellerStressSafetyFactor = IMPELLER_MATERIAL_YIELD_PSI / ImpellerTipStress_PSI;
        MaxSafeRPM = (float)Math.Sqrt((IMPELLER_MATERIAL_YIELD_PSI * 12f * G_FT_S2) / IMPELLER_MATERIAL_DENSITY_LBM_IN3) * 60f / ((float)Math.PI * ImpellerDiameter_IN);

        CasingOuterDiameter_IN = VoluteBaseCircleDia_IN + 1.0f;
        CasingWallThickness_IN = (dischargePressure_PSI * CasingOuterDiameter_IN) / (2f * CASING_MATERIAL_ALLOWABLE_PSI * JOINT_EFFICIENCY - dischargePressure_PSI);
        MinimumWallThickness_IN = Math.Max(CasingWallThickness_IN, 0.125f);

        SealDiameter_IN = ShaftDiameter_IN + 0.125f;
        SealFaceVelocity_FT_MIN = ((float)Math.PI * SealDiameter_IN * ImpellerRPM) / 12f;
        SealFacePressure_PSI = dischargePressure_PSI;
        SealType = SealFaceVelocity_FT_MIN > 3000f ? "Mechanical Seal" : "Lip Seal";

        RadialClearance_IN = 0.015f;
        AxialClearance_IN = 0.020f;
        SurfaceFinish_Ra_MICROINCH = 32f;
        TipClearance_IN = 0.010f;

        ConvertDimensionsToMM();
    }

    private void CalculateDesignParameters(float massFlowRate_LBM_S, float dischargePressure_PSI,
                                         float inletPressure_PSI, float fluidDensity_LBM_FT3,
                                         float pumpEfficiency)
    {
        float fTargetSpecificSpeed = 1200f;
        float fPressureRise = dischargePressure_PSI - inletPressure_PSI;
        float fHeadRequired_FT = (fPressureRise * 144f) / (fluidDensity_LBM_FT3 * G_FT_S2);
        float fFlowRate_GPM = (massFlowRate_LBM_S / fluidDensity_LBM_FT3) * 448.831f;

        ImpellerRPM = fTargetSpecificSpeed * (float)Math.Pow(fHeadRequired_FT, 0.75) / (float)Math.Sqrt(fFlowRate_GPM);
        ImpellerRPM = (float)Math.Round(ImpellerRPM / 500f) * 500f;

        float fTargetHeadCoefficient = 0.6f;
        float fTipSpeed_FT_S = (float)Math.Sqrt((G_FT_S2 * fHeadRequired_FT) / fTargetHeadCoefficient);
        ImpellerDiameter_IN = (fTipSpeed_FT_S * 60f * 12f) / ((float)Math.PI * ImpellerRPM);
        ImpellerDiameter_IN = (float)Math.Round(ImpellerDiameter_IN * 4f) / 4f;

        float fActualSpecificSpeed = ImpellerRPM * (float)Math.Sqrt(fFlowRate_GPM) / (float)Math.Pow(fHeadRequired_FT, 0.75);

        if (fActualSpecificSpeed < 1000f)
            NumberOfBlades = 9;
        else if (fActualSpecificSpeed < 1500f)
            NumberOfBlades = 7;
        else
            NumberOfBlades = 5;

        BladeOutletAngle_DEG = 15f + (2000f - fActualSpecificSpeed) / 100f;
        BladeOutletAngle_DEG = Math.Max(15f, Math.Min(35f, BladeOutletAngle_DEG));

        float fTorque_LBF_IN = (ShaftPower_HP * 63025f) / ImpellerRPM;
        float fRequiredShaftDiameter = (float)Math.Pow((16f * fTorque_LBF_IN * 4f) / ((float)Math.PI * SHAFT_MATERIAL_YIELD_PSI), 1.0 / 3.0);

        ShaftDiameter_IN = (float)Math.Ceiling(fRequiredShaftDiameter * 8f) / 8f;
        ShaftDiameter_IN = Math.Max(0.375f, ShaftDiameter_IN);

        float fVolumetricFlowRate = massFlowRate_LBM_S / fluidDensity_LBM_FT3;
        float fHydraulicPower_HP = (massFlowRate_LBM_S * fHeadRequired_FT * G_FT_S2) / 550f;
        ShaftPower_HP = fHydraulicPower_HP / pumpEfficiency;
    }

    private void ConvertDimensionsToMM()
    {
        const float IN_TO_MM = 25.4f;

        ImpellerDiameter_MM = ImpellerDiameter_IN * IN_TO_MM;
        ImpellerEyeDiameter_MM = ImpellerEyeDiameter_IN * IN_TO_MM;
        ImpellerOutletWidth_MM = ImpellerOutletWidth_IN * IN_TO_MM;
        BladeRadialLength_MM = BladeRadialLength_IN * IN_TO_MM;
        InletBladeHeight_MM = InletBladeHeight_IN * IN_TO_MM;
        OutletBladeHeight_MM = OutletBladeHeight_IN * IN_TO_MM;
        BladeThickness_MM = BladeThickness_IN * IN_TO_MM;
        BladeLeadingEdgeChamfer_MM = BladeLeadingEdgeChamfer_IN * IN_TO_MM;
        BladeTrailingEdgeChamfer_MM = BladeTrailingEdgeChamfer_IN * IN_TO_MM;
        InletHubDiameter_MM = InletHubDiameter_IN * IN_TO_MM;
        InletShroudDiameter_MM = InletShroudDiameter_IN * IN_TO_MM;
        InletPipeDiameter_MM = InletPipeDiameter_IN * IN_TO_MM;
        OutletDiameter_MM = OutletDiameter_IN * IN_TO_MM;
        VoluteBaseCircleDia_MM = VoluteBaseCircleDia_IN * IN_TO_MM;
        VoluteThroatWidth_MM = VoluteThroatWidth_IN * IN_TO_MM;
        VoluteThroatHeight_MM = VoluteThroatHeight_IN * IN_TO_MM;
        VoluteCutwaterClearance_MM = VoluteCutwaterClearance_IN * IN_TO_MM;
        DischargeNozzleDiameter_MM = DischargeNozzleDiameter_IN * IN_TO_MM;
        CasingOuterDiameter_MM = CasingOuterDiameter_IN * IN_TO_MM;
        MinimumWallThickness_MM = MinimumWallThickness_IN * IN_TO_MM;
        ShaftDiameter_MM = ShaftDiameter_IN * IN_TO_MM;
        SealDiameter_MM = SealDiameter_IN * IN_TO_MM;
        BalanceHoleDiameter_MM = BalanceHoleDiameter_IN * IN_TO_MM;
        DiffuserInletDiameter_MM = DiffuserInletDiameter_IN * IN_TO_MM;
        DiffuserOutletDiameter_MM = DiffuserOutletDiameter_IN * IN_TO_MM;
        DiffuserPassageWidth_MM = DiffuserPassageWidth_IN * IN_TO_MM;
        RadialClearance_MM = RadialClearance_IN * IN_TO_MM;
        AxialClearance_MM = AxialClearance_IN * IN_TO_MM;
        TipClearance_MM = TipClearance_IN * IN_TO_MM;
    }

    /// <summary>
    /// Print all calculated results to the console.
    /// </summary>
    public void PrintResults()
    {
        Console.WriteLine("=== TURBOPUMP DESIGN RESULTS ===\n");
        Console.WriteLine("BASIC FLOW:");
        Console.WriteLine($"  Volumetric Flow Rate: {VolumetricFlowRate_FT3_S:F4} ft³/s ({VolumetricFlowRate_GPM:F2} GPM)");
        Console.WriteLine($"  Pressure Rise: {PressureRise_PSI:F1} psi\n");
        Console.WriteLine("HEAD & POWER:");
        Console.WriteLine($"  Total Head: {TotalHead_FT:F1} ft");
        Console.WriteLine($"  Hydraulic Power: {HydraulicPower_HP:F2} hp");
        Console.WriteLine($"  Shaft Power: {ShaftPower_HP:F2} hp ({ShaftPower_KW:F2} kW)\n");
        Console.WriteLine("IMPELLER:");
        Console.WriteLine($"  Diameter: {ImpellerDiameter_IN:F2} in ({ImpellerDiameter_MM:F2} mm)");
        Console.WriteLine($"  RPM: {ImpellerRPM:F0}");
        Console.WriteLine($"  Tip Speed: {ImpellerTipSpeed_FT_S:F1} ft/s");
        Console.WriteLine($"  Number of Blades: {NumberOfBlades}\n");
    }
}

