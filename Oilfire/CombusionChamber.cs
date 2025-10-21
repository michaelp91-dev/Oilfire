using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PicoGK;

namespace RocketEngineBuilder
{
    public class CombustionChamber
    {
        float fLStar = 60.0f;
        float fGasConstant = 65.5f;
        float fGravityConstant = 32.174f;
        float fHeatTransferRate = 3.0f;
        float fWaterTemperatureRise = 40.0f;
        float fWaterVelocity = 30.0f;
        float fWaterRHO = 62.4f;
        float fChamberToThroatRatio = 3.0f;
        float fCopperStress = 8000.0f;
        float fConvergentAngle_deg = 45.0f;
        float fDivergentAngle_deg = 15.0f;
        public CombustionChamber(float fThrust, float f_OFRatio, float f_ChamberPressure, PropellantDataService propellantDataService)
        {
            // Lookup the propellant data based on O/F ratio and chamber pressure
            var propellantData = propellantDataService.FindClosest(f_OFRatio, f_ChamberPressure);
            if (propellantData == null)
            {
                throw new InvalidOperationException("No matching propellant data found.");
            }

            // Print all propellantData fields to the console
            Console.WriteLine("--- Propellant Data ---");
            Console.WriteLine($"OF_Ratio: {propellantData.OF_Ratio}");
            Console.WriteLine($"Chamber_Pressure: {propellantData.Chamber_Pressure}");
            Console.WriteLine($"Chamber_Temp_R: {propellantData.Chamber_Temp_R}");
            Console.WriteLine($"Throat_Temp_R: {propellantData.Throat_Temp_R}");
            Console.WriteLine($"Gamma_Chamber: {propellantData.Gamma_Chamber}");
            Console.WriteLine($"Isp: {propellantData.Isp}");
            try {
                var expansionRatio = propellantData.GetType().GetProperty("Expansion_Ratio");
                if (expansionRatio != null)
                    Console.WriteLine($"Expansion_Ratio: {expansionRatio.GetValue(propellantData)}");
            } catch {}
            Console.WriteLine("----------------------");

            // Assign properties from the looked-up data
            m_fOFRatio = propellantData.OF_Ratio;
            m_fChamberPressure_PSI = propellantData.Chamber_Pressure;
            m_fChamberTemp_R = propellantData.Chamber_Temp_R;
            m_fThroatTemp_R = propellantData.Throat_Temp_R;
            m_fGammaChamber = propellantData.Gamma_Chamber;
            m_fIsp = propellantData.Isp;
            m_fExpansionRatio = propellantData.Expansion_Ratio;

            m_fThrust = fThrust;
        }

        public void Calculate()
        {
            m_fTotalWeightFlow = m_fThrust / m_fIsp;
            Console.WriteLine($"Total Weight Flow: {m_fTotalWeightFlow}");
            m_fFuelWeightFlow = m_fTotalWeightFlow / (1.0f + m_fOFRatio);
            Console.WriteLine($"Fuel Weight Flow: {m_fFuelWeightFlow}");
            m_fOxWeightFlow = m_fTotalWeightFlow - m_fFuelWeightFlow;
            Console.WriteLine($"Oxidizer Weight Flow: {m_fOxWeightFlow}");
            m_fThroatPressure_PSI = m_fChamberPressure_PSI * 0.564f;
            Console.WriteLine($"Throat Pressure (PSI): {m_fThroatPressure_PSI}");
            m_fThroatArea_in2 = (m_fTotalWeightFlow / m_fThroatPressure_PSI) * (float)Math.Sqrt((fGasConstant * m_fThroatTemp_R) / (fGravityConstant * m_fGammaChamber));
            Console.WriteLine($"Throat Area (in^2): {m_fThroatArea_in2}");
            m_fThroatDiameter_in = (float)Math.Sqrt((m_fThroatArea_in2 * 4.0f) / Math.PI);
            Console.WriteLine($"Throat Diameter (in): {m_fThroatDiameter_in}");
            m_fExitArea_in2 = m_fThroatArea_in2 * m_fExpansionRatio;
            Console.WriteLine($"Exit Area (in^2): {m_fExitArea_in2}");
            m_fExitDiameter_in = (float)Math.Sqrt((m_fExitArea_in2 * 4.0f) / Math.PI);
            Console.WriteLine($"Exit Diameter (in): {m_fExitDiameter_in}");
            m_fChamberVolume = fLStar * m_fThroatArea_in2;
            Console.WriteLine($"Chamber Volume: {m_fChamberVolume}");
            m_fChamberDiameter_in = m_fThroatDiameter_in * fChamberToThroatRatio;
            Console.WriteLine($"Chamber Diameter (in): {m_fChamberDiameter_in}");
            m_fChamberArea_in2 = (float)(Math.PI * Math.Pow(m_fChamberDiameter_in / 2.0f, 2.0f));
            Console.WriteLine($"Chamber Area (in^2): {m_fChamberArea_in2}");
            m_fChamberLength_in = m_fChamberVolume / (1.1f * m_fChamberArea_in2);
            Console.WriteLine($"Chamber Length (in): {m_fChamberLength_in}");
            m_fMinimumWallThickness_in = (m_fChamberPressure_PSI * m_fChamberDiameter_in) / (2.0f * fCopperStress);
            Console.WriteLine($"Minimum Wall Thickness (in): {m_fMinimumWallThickness_in}");
            m_fWallThickness_in = m_fMinimumWallThickness_in * 1.5f;
            Console.WriteLine($"Wall Thickness (in): {m_fWallThickness_in}");
            m_fConvergentLength_in = ((m_fChamberDiameter_in / 2) - (m_fThroatDiameter_in / 2)) / (float)Math.Tan(fConvergentAngle_deg * ((float)Math.PI / 180.0f));
            Console.WriteLine($"Convergent Length (in): {m_fConvergentLength_in}");
            m_fDivergentLength_in = ((m_fExitDiameter_in / 2) - (m_fThroatDiameter_in / 2)) / (float)Math.Tan(fDivergentAngle_deg * ((float)Math.PI / 180.0f));
            Console.WriteLine($"Divergent Length (in): {m_fDivergentLength_in}");
            m_fChamberSurfaceArea = 2 * (float)Math.PI * ((m_fChamberDiameter_in + m_fWallThickness_in) / 2) * m_fChamberLength_in + 2 * (float)Math.PI * (float)Math.Pow((m_fChamberDiameter_in + m_fWallThickness_in) / 2, 2.0f);
            Console.WriteLine($"Chamber Surface Area: {m_fChamberSurfaceArea}");
            m_fConvergentSurfaceArea = (float)Math.PI * (float)Math.Pow(((m_fChamberDiameter_in / 2.0f) + m_fWallThickness_in), 2.0) + (float)Math.PI * (float)Math.Pow(((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in), 2.0) + (float)Math.PI * ((((m_fChamberDiameter_in / 2.0f) + m_fWallThickness_in) + ((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in))) * (float)Math.Sqrt((float)Math.Pow(m_fConvergentLength_in, 2.0) + (float)Math.Pow((((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in) - ((m_fChamberDiameter_in / 2.0f) + m_fWallThickness_in)), 2.0));
            Console.WriteLine($"Convergent Surface Area: {m_fConvergentSurfaceArea}");
            m_fDivergentSurfaceArea = (float)Math.PI * (float)Math.Pow(((m_fExitDiameter_in / 2.0f) + m_fWallThickness_in), 2.0) + (float)Math.PI * (float)Math.Pow(((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in), 2.0) + (float)Math.PI * ((((m_fExitDiameter_in / 2.0f) + m_fWallThickness_in) + ((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in))) * (float)Math.Sqrt((float)Math.Pow(m_fDivergentLength_in, 2.0) + (float)Math.Pow((((m_fThroatDiameter_in / 2.0f) + m_fWallThickness_in) - ((m_fExitDiameter_in / 2.0f) + m_fWallThickness_in)), 2.0));
            Console.WriteLine($"Divergent Surface Area: {m_fDivergentSurfaceArea}");
            m_fTotalSurfaceArea = m_fChamberSurfaceArea + m_fConvergentSurfaceArea + m_fDivergentSurfaceArea;
            Console.WriteLine($"Total Surface Area: {m_fTotalSurfaceArea}");
            m_fHeatTransferred = m_fTotalSurfaceArea * fHeatTransferRate;
            Console.WriteLine($"Heat Transferred: {m_fHeatTransferred}");
            m_fWaterFlowWeight = m_fHeatTransferred / fWaterTemperatureRise;
            Console.WriteLine($"Water Flow Weight: {m_fWaterFlowWeight}");
            m_fChamberOuterDiameter_in = m_fChamberDiameter_in + (2.0f * m_fWallThickness_in);
            Console.WriteLine($"Chamber Outer Diameter (in): {m_fChamberOuterDiameter_in}");
            m_fChamberOuterDiameter_ft = m_fChamberOuterDiameter_in / 12.0f;
            Console.WriteLine($"Chamber Outer Diameter (ft): {m_fChamberOuterDiameter_ft}");
            m_fWaterJacketDiameter_ft = (float)Math.Sqrt((4 * m_fWaterFlowWeight) / (fWaterVelocity * fWaterRHO * (float)Math.PI) + m_fChamberOuterDiameter_ft * m_fChamberOuterDiameter_ft);
            Console.WriteLine($"Water Jacket Diameter (ft): {m_fWaterJacketDiameter_ft}");
            m_fWaterJacketDiameter_in = m_fWaterJacketDiameter_ft * 12.0f;
            Console.WriteLine($"Water Jacket Diameter (in): {m_fWaterJacketDiameter_in}");
            m_fWaterFlowGap_in = m_fWaterJacketDiameter_in - m_fChamberOuterDiameter_in;
            Console.WriteLine($"Water Flow Gap (in): {m_fWaterFlowGap_in}");

            m_fChamberLength_mm = m_fChamberLength_in * 25.4f;
            Console.WriteLine($"Chamber Length (mm): {m_fChamberLength_mm}");
            m_fChamberDiameter_mm = m_fChamberDiameter_in * 25.4f;
            Console.WriteLine($"Chamber Diameter (mm): {m_fChamberDiameter_mm}");
            m_fThroatDiameter_mm = m_fThroatDiameter_in * 25.4f;
            Console.WriteLine($"Throat Diameter (mm): {m_fThroatDiameter_mm}");
            m_fExitDiameter_mm = m_fExitDiameter_in * 25.4f;
            Console.WriteLine($"Exit Diameter (mm): {m_fExitDiameter_mm}");
            m_fConvergentLength_mm = m_fConvergentLength_in * 25.4f;
            Console.WriteLine($"Convergent Length (mm): {m_fConvergentLength_mm}");
            m_fDivergentLength_mm = m_fDivergentLength_in * 25.4f;
            Console.WriteLine($"Divergent Length (mm): {m_fDivergentLength_mm}");
            m_fWallThickness_mm = m_fWallThickness_in * 25.4f;
            Console.WriteLine($"Wall Thickness (mm): {m_fWallThickness_mm}");
            m_fChamberRadius_mm = m_fChamberDiameter_mm / 2.0f;
            Console.WriteLine($"Chamber Radius (mm): {m_fChamberRadius_mm}");
            m_fThroatRadius_mm = m_fThroatDiameter_mm / 2.0f;
            Console.WriteLine($"Throat Radius (mm): {m_fThroatRadius_mm}");
            m_fExitRadius_mm = m_fExitDiameter_mm / 2.0f;
            Console.WriteLine($"Exit Radius (mm): {m_fExitRadius_mm}");

            
        }

        public void CreateCombustionChamber()
        {
            float fLengthToThroat = m_fChamberLength_mm + m_fConvergentLength_mm;
            float fLengthToExit = fLengthToThroat + m_fDivergentLength_mm;

            Lattice latInside = new Lattice();
            latInside.AddBeam(new(0, 0, 0),
                                new(0, 0, -m_fChamberLength_mm),
                                m_fChamberRadius_mm,
                                m_fChamberRadius_mm,
                                false);
            latInside.AddBeam(new(0, 0, -m_fChamberLength_mm),
                                new(0, 0, -fLengthToThroat),
                                m_fChamberRadius_mm,
                                m_fThroatRadius_mm,
                                false);
            latInside.AddBeam(new(0, 0, -fLengthToThroat),
                                new(0, 0, -fLengthToExit),
                                m_fThroatRadius_mm,
                                m_fExitRadius_mm,
                                false);

            Lattice latOutside = new Lattice();
            latOutside.AddBeam(new(0, 0, 0),
                                new(0, 0, -m_fChamberLength_mm),
                                m_fChamberRadius_mm + m_fWallThickness_mm,
                                m_fChamberRadius_mm + m_fWallThickness_mm,
                                false);
            latOutside.AddBeam(new(0, 0, -m_fChamberLength_mm),
                                new(0, 0, -fLengthToThroat),
                                m_fChamberRadius_mm + m_fWallThickness_mm,
                                m_fThroatRadius_mm + m_fWallThickness_mm,
                                false);
            latOutside.AddBeam(new(0, 0, -fLengthToThroat),
                                new(0, 0, -fLengthToExit),
                                m_fThroatRadius_mm + m_fWallThickness_mm,
                                m_fExitRadius_mm + m_fWallThickness_mm,
                                false);

            Voxels voxInside = new(latInside);
            Voxels voxOutside = new(latOutside);
            voxOutside.BoolSubtract(voxInside);
            m_voxCombustionChamber = voxOutside;
        }

        public Voxels voxGetCombustionChamber()
        {
            return m_voxCombustionChamber;
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
        private float m_fThroatArea_in2;
        private float m_fThroatDiameter_in;
        private float m_fExitArea_in2;
        private float m_fExitDiameter_in;
        private float m_fChamberVolume;
        private float m_fChamberDiameter_in;
        private float m_fChamberLength_in;
        private float m_fChamberArea_in2;
        private float m_fMinimumWallThickness_in;
        private float m_fChamberSurfaceArea;
        private float m_fConvergentLength_in;
        private float m_fDivergentLength_in;
        private float m_fConvergentSurfaceArea;
        private float m_fDivergentSurfaceArea;
        private float m_fTotalSurfaceArea;
        private float m_fThroatPressure_PSI;
        private float m_fWallThickness_in;
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
        private Voxels m_voxCombustionChamber;
    }
}