using System;

namespace TurbopumpDesign
{
    public class Turbopump
    {
        // REQUIRED CONSTRUCTOR PARAMETERS (Input)
        private float m_fMassFlowRate_LBM_S;
        private float m_fDischargePressure_PSI;
        private float m_fInletPressure_PSI;
        private float m_fFluidDensity_LBM_FT3;
        private float m_fPumpEfficiency;

        // OPTIONAL PARAMETERS (with defaults)
        private float m_fKinematicViscosity_CST;
        private float m_fVaporPressure_PSI;

        // DESIGN PARAMETERS (calculated)
        private float m_fImpellerRPM;
        private float m_fImpellerDiameter_IN;
        private int m_iNumberOfBlades;
        private float m_fBladeOutletAngle_DEG;
        private float m_fShaftDiameter_IN;

        // CONSTANTS
        private const float G_FT_S2 = 32.2f;
        private const int NUMBER_OF_STAGES = 1;

        // MATERIAL PROPERTIES (typical values)
        private const float SHAFT_MATERIAL_YIELD_PSI = 60000f;
        private const float IMPELLER_MATERIAL_DENSITY_LBM_IN3 = 0.283f;
        private const float IMPELLER_MATERIAL_YIELD_PSI = 40000f;
        private const float CASING_MATERIAL_ALLOWABLE_PSI = 20000f;
        private const float JOINT_EFFICIENCY = 1.0f;

        // GEOMETRY RATIOS (typical design values)
        private const float EYE_DIAMETER_RATIO = 0.35f;
        private const float OUTLET_WIDTH_RATIO = 0.08f;

        // CALCULATED RESULTS (member variables)
        private float m_fVolumetricFlowRate_FT3_S;
        private float m_fVolumetricFlowRate_GPM;
        private float m_fPressureRise_PSI;
        private float m_fTotalHead_FT;
        private float m_fHeadPerStage_FT;
        private float m_fHydraulicPower_FT_LBF_S;
        private float m_fHydraulicPower_HP;
        private float m_fShaftPower_HP;
        private float m_fShaftPower_KW;
        private float m_fSpecificSpeed_US;
        private float m_fSpecificSpeedPerStage;
        private float m_fImpellerTipSpeed_FT_S;
        private float m_fHeadCoefficient;
        private float m_fFlowCoefficient;
        private float m_fInletVelocity_FT_S;
        private float m_fInletPipeDiameter_IN;
        private float m_fNPSH_Available_FT;
        private float m_fSuctionSpecificSpeed;
        private float m_fImpellerDiameter_FT;
        private float m_fReynoldsNumber;
        private float m_fImpellerEyeDiameter_IN;
        private float m_fImpellerOutletWidth_IN;
        private float m_fBladeOutletAngle_RAD;
        private float m_fTangentialVelocity_FT_S;
        private float m_fMeridionalVelocity_FT_S;
        private float m_fTheoreticalHead_FT;
        private float m_fSlipFactor;
        private float m_fActualHeadPerStage_FT;
        private float m_fInletHubDiameter_IN;
        private float m_fInletShroudDiameter_IN;
        private float m_fInletBladeHeight_IN;
        private float m_fInletFlowArea_IN2;
        private float m_fInletMeridionalVelocity_FT_S;
        private float m_fInletBladeAngle_DEG;
        private float m_fInletBladeAngle_RAD;
        private float m_fOutletDiameter_IN;
        private float m_fOutletBladeHeight_IN;
        private float m_fOutletFlowArea_IN2;
        private float m_fOutletMeridionalVelocity_FT_S;
        private float m_fOutletBladeAngle_RAD;
        private float m_fOutletTangentialVelocity_FT_S;
        private float m_fOutletRelativeVelocity_FT_S;
        private float m_fBladeRadialLength_IN;
        private float m_fBladeThickness_IN;
        private float m_fBladeLeadingEdgeChamfer_IN;
        private float m_fBladeTrailingEdgeChamfer_IN;
        private float m_fVoluteThroatArea_IN2;
        private float m_fVoluteThroatWidth_IN;
        private float m_fVoluteThroatHeight_IN;
        private float m_fVoluteBaseCircleDia_IN;
        private float m_fVoluteCutwaterClearance_IN;
        private float m_fDischargeNozzleDiameter_IN;
        private int m_iDiffuserVaneCount;
        private float m_fDiffuserInletDiameter_IN;
        private float m_fDiffuserOutletDiameter_IN;
        private float m_fDiffuserVaneAngle_DEG;
        private float m_fDiffuserPassageWidth_IN;
        private float m_fFrontShroudArea_IN2;
        private float m_fBackShroudArea_IN2;
        private float m_fAxialThrust_LBF;
        private string m_strBalanceHolesRequired;
        private int m_iNumberOfBalanceHoles;
        private float m_fBalanceHoleDiameter_IN;
        private float m_fShaftTorque_LBF_IN;
        private float m_fShaftShearStress_PSI;
        private float m_fShaftSafetyFactor;
        private float m_fBearingLoad_LBF;
        private float m_fImpellerTipStress_PSI;
        private float m_fImpellerStressSafetyFactor;
        private float m_fMaxSafeRPM;
        private float m_fCasingOuterDiameter_IN;
        private float m_fCasingWallThickness_IN;
        private float m_fMinimumWallThickness_IN;
        private float m_fSealDiameter_IN;
        private float m_fSealFaceVelocity_FT_MIN;
        private float m_fSealFacePressure_PSI;
        private string m_strSealType;
        private float m_fRadialClearance_IN;
        private float m_fAxialClearance_IN;
        private float m_fSurfaceFinish_Ra_MICROINCH;
        private float m_fTipClearance_IN;

        // Constructor
        public Turbopump(float fMassFlowRate_LBM_S, float fDischargePressure_PSI, 
                        float fInletPressure_PSI, float fFluidDensity_LBM_FT3, 
                        float fPumpEfficiency)
        {
            m_fMassFlowRate_LBM_S = fMassFlowRate_LBM_S;
            m_fDischargePressure_PSI = fDischargePressure_PSI;
            m_fInletPressure_PSI = fInletPressure_PSI;
            m_fFluidDensity_LBM_FT3 = fFluidDensity_LBM_FT3;
            m_fPumpEfficiency = fPumpEfficiency;

            // Set defaults for optional parameters (typical for IPA)
            m_fKinematicViscosity_CST = 2.4f;
            m_fVaporPressure_PSI = 0.64f;

            // Initialize design parameters
            m_fImpellerRPM = 0f;
            m_fImpellerDiameter_IN = 0f;
            m_iNumberOfBlades = 0;
            m_fBladeOutletAngle_DEG = 0f;
            m_fShaftDiameter_IN = 0f;
        }

        // Optional parameter setters
        public void SetKinematicViscosity_CST(float fValue)
        {
            m_fKinematicViscosity_CST = fValue;
        }

        public void SetVaporPressure_PSI(float fValue)
        {
            m_fVaporPressure_PSI = fValue;
        }

        // Main calculation function
        public void Calculate()
        {
            // CALCULATE DESIGN PARAMETERS
            CalculateDesignParameters();

            // BASIC FLOW CALCULATIONS
            m_fVolumetricFlowRate_FT3_S = m_fMassFlowRate_LBM_S / m_fFluidDensity_LBM_FT3;
            m_fVolumetricFlowRate_GPM = m_fVolumetricFlowRate_FT3_S * 448.831f;
            m_fPressureRise_PSI = m_fDischargePressure_PSI - m_fInletPressure_PSI;

            // HEAD CALCULATIONS
            m_fTotalHead_FT = (m_fPressureRise_PSI * 144f) / (m_fFluidDensity_LBM_FT3 * G_FT_S2);
            m_fHeadPerStage_FT = m_fTotalHead_FT / NUMBER_OF_STAGES;

            // POWER CALCULATIONS
            m_fHydraulicPower_FT_LBF_S = m_fMassFlowRate_LBM_S * m_fTotalHead_FT * G_FT_S2;
            m_fHydraulicPower_HP = m_fHydraulicPower_FT_LBF_S / 550f;
            m_fShaftPower_HP = m_fHydraulicPower_HP / m_fPumpEfficiency;
            m_fShaftPower_KW = m_fShaftPower_HP * 0.7457f;

            // SPECIFIC SPEED
            m_fSpecificSpeed_US = m_fImpellerRPM * (float)Math.Sqrt(m_fVolumetricFlowRate_GPM) / (float)Math.Pow(m_fTotalHead_FT, 0.75);
            m_fSpecificSpeedPerStage = m_fImpellerRPM * (float)Math.Sqrt(m_fVolumetricFlowRate_GPM) / (float)Math.Pow(m_fHeadPerStage_FT, 0.75);

            // IMPELLER TIP SPEED
            m_fImpellerTipSpeed_FT_S = ((float)Math.PI * m_fImpellerDiameter_IN * m_fImpellerRPM) / 720f;

            // HEAD COEFFICIENT
            m_fHeadCoefficient = (G_FT_S2 * m_fHeadPerStage_FT) / (m_fImpellerTipSpeed_FT_S * m_fImpellerTipSpeed_FT_S);

            // FLOW COEFFICIENT
            m_fFlowCoefficient = m_fVolumetricFlowRate_FT3_S / ((m_fImpellerDiameter_IN / 12f) * (m_fImpellerDiameter_IN / 12f) * m_fImpellerTipSpeed_FT_S);

            // NPSH CALCULATIONS
            m_fInletVelocity_FT_S = 10f;
            m_fInletPipeDiameter_IN = (float)Math.Sqrt((m_fVolumetricFlowRate_FT3_S * 183.346f) / ((float)Math.PI * m_fInletVelocity_FT_S));
            m_fNPSH_Available_FT = ((m_fInletPressure_PSI - m_fVaporPressure_PSI) * 144f) / (m_fFluidDensity_LBM_FT3 * G_FT_S2) + (m_fInletVelocity_FT_S * m_fInletVelocity_FT_S) / (2f * G_FT_S2);
            m_fSuctionSpecificSpeed = m_fImpellerRPM * (float)Math.Sqrt(m_fVolumetricFlowRate_GPM) / (float)Math.Pow(m_fNPSH_Available_FT, 0.75);

            // REYNOLDS NUMBER
            m_fImpellerDiameter_FT = m_fImpellerDiameter_IN / 12f;
            m_fReynoldsNumber = (m_fImpellerTipSpeed_FT_S * m_fImpellerDiameter_FT) / (m_fKinematicViscosity_CST * 0.00001076f);

            // ESTIMATED IMPELLER GEOMETRY
            m_fImpellerEyeDiameter_IN = m_fImpellerDiameter_IN * EYE_DIAMETER_RATIO;
            m_fImpellerOutletWidth_IN = m_fImpellerDiameter_IN * OUTLET_WIDTH_RATIO;

            // EULER HEAD (Theoretical)
            m_fBladeOutletAngle_RAD = m_fBladeOutletAngle_DEG * (float)Math.PI / 180f;
            m_fTangentialVelocity_FT_S = m_fImpellerTipSpeed_FT_S;
            m_fMeridionalVelocity_FT_S = m_fVolumetricFlowRate_FT3_S / ((float)Math.PI * (m_fImpellerDiameter_IN / 12f) * (m_fImpellerOutletWidth_IN / 12f));
            m_fTheoreticalHead_FT = (m_fTangentialVelocity_FT_S * m_fTangentialVelocity_FT_S) / G_FT_S2;
            m_fSlipFactor = 1f - (2.0f / m_iNumberOfBlades);
            m_fActualHeadPerStage_FT = m_fTheoreticalHead_FT * m_fSlipFactor;

            // DETAILED IMPELLER BLADE GEOMETRY
            m_fInletHubDiameter_IN = m_fImpellerEyeDiameter_IN * 0.4f;
            m_fInletShroudDiameter_IN = m_fImpellerEyeDiameter_IN;
            m_fInletBladeHeight_IN = (m_fInletShroudDiameter_IN - m_fInletHubDiameter_IN) / 2f;
            m_fInletFlowArea_IN2 = (float)Math.PI * ((m_fInletShroudDiameter_IN * m_fInletShroudDiameter_IN) - (m_fInletHubDiameter_IN * m_fInletHubDiameter_IN)) / 4f;
            m_fInletMeridionalVelocity_FT_S = m_fVolumetricFlowRate_FT3_S / (m_fInletFlowArea_IN2 / 144f);
            m_fInletBladeAngle_DEG = 90f;
            m_fInletBladeAngle_RAD = m_fInletBladeAngle_DEG * (float)Math.PI / 180f;

            // IMPELLER OUTLET BLADE GEOMETRY
            m_fOutletDiameter_IN = m_fImpellerDiameter_IN;
            m_fOutletBladeHeight_IN = m_fImpellerOutletWidth_IN;
            m_fOutletFlowArea_IN2 = (float)Math.PI * m_fOutletDiameter_IN * m_fOutletBladeHeight_IN;
            m_fOutletMeridionalVelocity_FT_S = m_fVolumetricFlowRate_FT3_S / (m_fOutletFlowArea_IN2 / 144f);
            m_fOutletBladeAngle_RAD = m_fBladeOutletAngle_DEG * (float)Math.PI / 180f;
            m_fOutletTangentialVelocity_FT_S = (float)Math.PI * (m_fOutletDiameter_IN / 12f) * m_fImpellerRPM / 60f;
            m_fOutletRelativeVelocity_FT_S = m_fOutletMeridionalVelocity_FT_S / (float)Math.Sin(m_fOutletBladeAngle_RAD);

            // STRAIGHT BLADE PROFILE (SIMPLIFIED)
            m_fBladeRadialLength_IN = (m_fOutletDiameter_IN - m_fInletShroudDiameter_IN) / 2f;
            m_fBladeThickness_IN = 0.06f;
            m_fBladeLeadingEdgeChamfer_IN = 0.03f;
            m_fBladeTrailingEdgeChamfer_IN = 0.03f;

            // VOLUTE/CASING DESIGN
            m_fVoluteThroatArea_IN2 = 1.1f * (m_fVolumetricFlowRate_FT3_S * 144f) / m_fOutletMeridionalVelocity_FT_S;
            m_fVoluteThroatWidth_IN = (float)Math.Sqrt(m_fVoluteThroatArea_IN2 / 1.5f);
            m_fVoluteThroatHeight_IN = m_fVoluteThroatArea_IN2 / m_fVoluteThroatWidth_IN;
            m_fVoluteBaseCircleDia_IN = m_fOutletDiameter_IN + (2f * m_fOutletBladeHeight_IN) + 0.25f;
            m_fVoluteCutwaterClearance_IN = 0.1f * m_fOutletDiameter_IN;
            m_fDischargeNozzleDiameter_IN = (float)Math.Sqrt((m_fVolumetricFlowRate_FT3_S * 183.346f) / ((float)Math.PI * 30f));

            // DIFFUSER DESIGN (Alternative to Volute)
            m_iDiffuserVaneCount = m_iNumberOfBlades + 3;
            m_fDiffuserInletDiameter_IN = m_fOutletDiameter_IN + 0.15f;
            m_fDiffuserOutletDiameter_IN = m_fDiffuserInletDiameter_IN * 1.5f;
            m_fDiffuserVaneAngle_DEG = 12f;
            m_fDiffuserPassageWidth_IN = m_fOutletBladeHeight_IN * 1.1f;

            // AXIAL THRUST BALANCE
            m_fFrontShroudArea_IN2 = (float)Math.PI * (m_fOutletDiameter_IN * m_fOutletDiameter_IN) / 4f;
            m_fBackShroudArea_IN2 = (float)Math.PI * ((m_fOutletDiameter_IN * m_fOutletDiameter_IN) - (m_fInletHubDiameter_IN * m_fInletHubDiameter_IN)) / 4f;
            m_fAxialThrust_LBF = (m_fDischargePressure_PSI * m_fFrontShroudArea_IN2) - (m_fInletPressure_PSI * m_fBackShroudArea_IN2);
            m_strBalanceHolesRequired = m_fAxialThrust_LBF > 100f ? "YES" : "NO";
            m_iNumberOfBalanceHoles = 4;
            m_fBalanceHoleDiameter_IN = 0.125f;

            // MECHANICAL DESIGN PARAMETERS
            m_fShaftTorque_LBF_IN = (m_fShaftPower_HP * 63025f) / m_fImpellerRPM;
            m_fShaftShearStress_PSI = (16f * m_fShaftTorque_LBF_IN) / ((float)Math.PI * (m_fShaftDiameter_IN * m_fShaftDiameter_IN * m_fShaftDiameter_IN));
            m_fShaftSafetyFactor = SHAFT_MATERIAL_YIELD_PSI / m_fShaftShearStress_PSI;
            m_fBearingLoad_LBF = m_fAxialThrust_LBF + (2f * m_fMassFlowRate_LBM_S * m_fOutletTangentialVelocity_FT_S / G_FT_S2);

            // IMPELLER STRESS ANALYSIS
            m_fImpellerTipStress_PSI = IMPELLER_MATERIAL_DENSITY_LBM_IN3 * ((m_fImpellerTipSpeed_FT_S * 12f) * (m_fImpellerTipSpeed_FT_S * 12f)) / (12f * G_FT_S2);
            m_fImpellerStressSafetyFactor = IMPELLER_MATERIAL_YIELD_PSI / m_fImpellerTipStress_PSI;
            m_fMaxSafeRPM = (float)Math.Sqrt((IMPELLER_MATERIAL_YIELD_PSI * 12f * G_FT_S2) / IMPELLER_MATERIAL_DENSITY_LBM_IN3) * 60f / ((float)Math.PI * m_fImpellerDiameter_IN);

            // CASING WALL THICKNESS
            m_fCasingOuterDiameter_IN = m_fVoluteBaseCircleDia_IN + 1.0f;
            m_fCasingWallThickness_IN = (m_fDischargePressure_PSI * m_fCasingOuterDiameter_IN) / (2f * CASING_MATERIAL_ALLOWABLE_PSI * JOINT_EFFICIENCY - m_fDischargePressure_PSI);
            m_fMinimumWallThickness_IN = Math.Max(m_fCasingWallThickness_IN, 0.125f);

            // SEAL DESIGN PARAMETERS
            m_fSealDiameter_IN = m_fShaftDiameter_IN + 0.125f;
            m_fSealFaceVelocity_FT_MIN = ((float)Math.PI * m_fSealDiameter_IN * m_fImpellerRPM) / 12f;
            m_fSealFacePressure_PSI = m_fDischargePressure_PSI;
            m_strSealType = m_fSealFaceVelocity_FT_MIN > 3000f ? "Mechanical Seal" : "Lip Seal";

            // MANUFACTURING TOLERANCES
            m_fRadialClearance_IN = 0.015f;
            m_fAxialClearance_IN = 0.020f;
            m_fSurfaceFinish_Ra_MICROINCH = 32f;
            m_fTipClearance_IN = 0.010f;
        }

        private void CalculateDesignParameters()
        {
            // Calculate specific speed to determine optimal impeller diameter and RPM
            // Target specific speed between 500-2000 for centrifugal pumps
            float fTargetSpecificSpeed = 1200f;

            // Calculate required head
            float fPressureRise = m_fDischargePressure_PSI - m_fInletPressure_PSI;
            float fHeadRequired_FT = (fPressureRise * 144f) / (m_fFluidDensity_LBM_FT3 * G_FT_S2);
            float fFlowRate_GPM = (m_fMassFlowRate_LBM_S / m_fFluidDensity_LBM_FT3) * 448.831f;

            // From Ns = N * sqrt(Q) / H^0.75, solve for N
            m_fImpellerRPM = fTargetSpecificSpeed * (float)Math.Pow(fHeadRequired_FT, 0.75) / (float)Math.Sqrt(fFlowRate_GPM);
            
            // Round to nearest 500 RPM for practical design
            m_fImpellerRPM = (float)Math.Round(m_fImpellerRPM / 500f) * 500f;

            // Calculate impeller diameter from head coefficient
            // Typical head coefficient for centrifugal pumps: 0.4 - 0.8
            float fTargetHeadCoefficient = 0.6f;
            
            // From head coefficient: psi = g*H/U^2, and U = pi*D*N/60
            // Solving for D: D = 60 * sqrt(g*H/psi) / (pi*N)
            float fTipSpeed_FT_S = (float)Math.Sqrt((G_FT_S2 * fHeadRequired_FT) / fTargetHeadCoefficient);
            m_fImpellerDiameter_IN = (fTipSpeed_FT_S * 60f * 12f) / ((float)Math.PI * m_fImpellerRPM);

            // Round to nearest 0.25 inch for manufacturing
            m_fImpellerDiameter_IN = (float)Math.Round(m_fImpellerDiameter_IN * 4f) / 4f;

            // Recalculate specific speed with actual values
            float fActualSpecificSpeed = m_fImpellerRPM * (float)Math.Sqrt(fFlowRate_GPM) / (float)Math.Pow(fHeadRequired_FT, 0.75);

            // Number of blades based on specific speed
            if (fActualSpecificSpeed < 1000f)
                m_iNumberOfBlades = 9;
            else if (fActualSpecificSpeed < 1500f)
                m_iNumberOfBlades = 7;
            else
                m_iNumberOfBlades = 5;

            // Blade outlet angle based on specific speed
            m_fBladeOutletAngle_DEG = 15f + (2000f - fActualSpecificSpeed) / 100f;
            m_fBladeOutletAngle_DEG = Math.Max(15f, Math.Min(35f, m_fBladeOutletAngle_DEG));

            // Shaft diameter from torque requirements
            float fTorque_LBF_IN = (m_fShaftPower_HP * 63025f) / m_fImpellerRPM;
            float fRequiredShaftDiameter = (float)Math.Pow((16f * fTorque_LBF_IN * 4f) / ((float)Math.PI * SHAFT_MATERIAL_YIELD_PSI), 1.0 / 3.0);
            
            // Round up to next 1/8 inch for standard sizing
            m_fShaftDiameter_IN = (float)Math.Ceiling(fRequiredShaftDiameter * 8f) / 8f;
            m_fShaftDiameter_IN = Math.Max(0.375f, m_fShaftDiameter_IN);

            // Recalculate shaft power for shaft calculation
            float fVolumetricFlowRate = m_fMassFlowRate_LBM_S / m_fFluidDensity_LBM_FT3;
            float fHydraulicPower_HP = (m_fMassFlowRate_LBM_S * fHeadRequired_FT * G_FT_S2) / 550f;
            m_fShaftPower_HP = fHydraulicPower_HP / m_fPumpEfficiency;
        }

        // Getter methods for all results
        public float GetVolumetricFlowRate_GPM() { return m_fVolumetricFlowRate_GPM; }
        public float GetTotalHead_FT() { return m_fTotalHead_FT; }
        public float GetShaftPower_HP() { return m_fShaftPower_HP; }
        public float GetImpellerDiameter_IN() { return m_fImpellerDiameter_IN; }
        public float GetImpellerRPM() { return m_fImpellerRPM; }
        public int GetNumberOfBlades() { return m_iNumberOfBlades; }
        public float GetBladeOutletAngle_DEG() { return m_fBladeOutletAngle_DEG; }
        public float GetShaftDiameter_IN() { return m_fShaftDiameter_IN; }
        public float GetAxialThrust_LBF() { return m_fAxialThrust_LBF; }
        public float GetCasingOuterDiameter_IN() { return m_fCasingOuterDiameter_IN; }
        public string GetSealType() { return m_strSealType; }

        // Method to print all results
        public void PrintResults()
        {
            Console.WriteLine("=== TURBOPUMP DESIGN RESULTS ===\n");
            
            Console.WriteLine("INPUT CONDITIONS:");
            Console.WriteLine($"  Mass Flow Rate: {m_fMassFlowRate_LBM_S:F2} lbm/s");
            Console.WriteLine($"  Discharge Pressure: {m_fDischargePressure_PSI:F1} psi");
            Console.WriteLine($"  Inlet Pressure: {m_fInletPressure_PSI:F1} psi");
            Console.WriteLine($"  Fluid Density: {m_fFluidDensity_LBM_FT3:F1} lbm/ft³");
            Console.WriteLine($"  Pump Efficiency: {m_fPumpEfficiency:P0}\n");
            
            Console.WriteLine("BASIC FLOW:");
            Console.WriteLine($"  Volumetric Flow Rate: {m_fVolumetricFlowRate_FT3_S:F4} ft³/s ({m_fVolumetricFlowRate_GPM:F2} GPM)");
            Console.WriteLine($"  Pressure Rise: {m_fPressureRise_PSI:F1} psi\n");
            
            Console.WriteLine("HEAD & POWER:");
            Console.WriteLine($"  Total Head: {m_fTotalHead_FT:F1} ft");
            Console.WriteLine($"  Hydraulic Power: {m_fHydraulicPower_HP:F2} hp");
            Console.WriteLine($"  Shaft Power: {m_fShaftPower_HP:F2} hp ({m_fShaftPower_KW:F2} kW)\n");
            
            Console.WriteLine("IMPELLER:");
            Console.WriteLine($"  Diameter: {m_fImpellerDiameter_IN:F2} in");
            Console.WriteLine($"  RPM: {m_fImpellerRPM:F0}");
            Console.WriteLine($"  Tip Speed: {m_fImpellerTipSpeed_FT_S:F1} ft/s");
            Console.WriteLine($"  Eye Diameter: {m_fImpellerEyeDiameter_IN:F3} in");
            Console.WriteLine($"  Outlet Width: {m_fImpellerOutletWidth_IN:F3} in");
            Console.WriteLine($"  Number of Blades: {m_iNumberOfBlades}\n");
            
            Console.WriteLine("BLADE GEOMETRY (STRAIGHT):");
            Console.WriteLine($"  Radial Length: {m_fBladeRadialLength_IN:F3} in");
            Console.WriteLine($"  Outlet Angle: {m_fBladeOutletAngle_DEG:F1}°");
            Console.WriteLine($"  Thickness: {m_fBladeThickness_IN:F3} in\n");
            
            Console.WriteLine("PERFORMANCE:");
            Console.WriteLine($"  Specific Speed: {m_fSpecificSpeed_US:F1}");
            Console.WriteLine($"  Head Coefficient: {m_fHeadCoefficient:F3}");
            Console.WriteLine($"  Flow Coefficient: {m_fFlowCoefficient:F4}");
            Console.WriteLine($"  NPSH Available: {m_fNPSH_Available_FT:F1} ft");
            Console.WriteLine($"  Suction Specific Speed: {m_fSuctionSpecificSpeed:F1}\n");
            
            Console.WriteLine("VOLUTE CASING:");
            Console.WriteLine($"  Base Circle Diameter: {m_fVoluteBaseCircleDia_IN:F2} in");
            Console.WriteLine($"  Throat Area: {m_fVoluteThroatArea_IN2:F2} in²");
            Console.WriteLine($"  Discharge Nozzle Diameter: {m_fDischargeNozzleDiameter_IN:F2} in");
            Console.WriteLine($"  Casing Outer Diameter: {m_fCasingOuterDiameter_IN:F2} in");
            Console.WriteLine($"  Wall Thickness: {m_fMinimumWallThickness_IN:F3} in\n");
            
            Console.WriteLine("MECHANICAL:");
            Console.WriteLine($"  Shaft Diameter: {m_fShaftDiameter_IN:F2} in");
            Console.WriteLine($"  Shaft Safety Factor: {m_fShaftSafetyFactor:F2}");
            Console.WriteLine($"  Axial Thrust: {m_fAxialThrust_LBF:F1} lbf");
            Console.WriteLine($"  Balance Holes Required: {m_strBalanceHolesRequired}");
            Console.WriteLine($"  Bearing Load: {m_fBearingLoad_LBF:F1} lbf\n");
            
            Console.WriteLine("STRESS ANALYSIS:");
            Console.WriteLine($"  Impeller Tip Stress: {m_fImpellerTipStress_PSI:F0} psi");
            Console.WriteLine($"  Impeller Safety Factor: {m_fImpellerStressSafetyFactor:F2}");
            Console.WriteLine($"  Maximum Safe RPM: {m_fMaxSafeRPM:F0}\n");
            
            Console.WriteLine("SEAL:");
            Console.WriteLine($"  Seal Type: {m_strSealType}");
            Console.WriteLine($"  Seal Diameter: {m_fSealDiameter_IN:F3} in");
        }
    }
}