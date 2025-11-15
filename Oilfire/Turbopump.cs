using System;
using PicoGK;
using Leap71.ShapeKernel;

namespace RocketEngineBuilder
{
    /// <summary>
    /// Represents a turbopump geometry with impeller creation.
    /// Calculation logic has been moved to TurbopumpCalculations in Calculations.cs.
    /// </summary>
    public class Turbopump
    {
        private float m_fMassFlowRate_LBM_S;
        private float m_fDischargePressure_PSI;
        private float m_fInletPressure_PSI;
        private float m_fFluidDensity_LBM_FT3;
        private float m_fPumpEfficiency;
        private float m_fKinematicViscosity_CST;
        private float m_fVaporPressure_PSI;

        // Calculations object
        private TurbopumpCalculations m_calculations;
        
        private Voxels m_voxBase;

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

            // Initialize calculations
            m_calculations = new TurbopumpCalculations();
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
            m_calculations.Calculate(m_fMassFlowRate_LBM_S, m_fDischargePressure_PSI,
                                    m_fInletPressure_PSI, m_fFluidDensity_LBM_FT3,
                                    m_fPumpEfficiency, m_fKinematicViscosity_CST,
                                    m_fVaporPressure_PSI);
        }

        public void CreateImpeller()
        {
            LocalFrame oBaseFrame = new(new(0, 0, 0));
            LocalFrame oHubFrame = new(new(0, 0, m_calculations.ImpellerOutletWidth_MM / 2.0f));
            BaseCylinder oBase = new(oBaseFrame, m_calculations.ImpellerOutletWidth_MM / 2.0f, m_calculations.ImpellerDiameter_MM / 2.0f);
            BaseCylinder oHub = new(oHubFrame, m_calculations.InletBladeHeight_MM, m_calculations.InletHubDiameter_MM / 2.0f);
            BaseBox oBlade = new(oHubFrame, m_calculations.InletBladeHeight_MM, m_calculations.ImpellerDiameter_MM, m_calculations.BladeThickness_MM);
            Voxels voxBase = oBase.voxConstruct();
            Voxels voxHub = oHub.voxConstruct();
            Voxels voxBlade = oBlade.voxConstruct();
            voxBase.BoolAdd(voxHub);
            voxBase.BoolAdd(voxBlade);
            m_voxBase = voxBase;
        }

        public Voxels voxGetImpeller()
        {
            return m_voxBase;
        }

        /// <summary>
        /// Print all results from calculations.
        /// </summary>
        public void PrintResults()
        {
            m_calculations.PrintResults();
        }
    }
}
        