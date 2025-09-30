using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RocketEngineCalculator
{
    /// <summary>
    /// Represents a single row of data from the N2O_Ethanol.csv file.
    /// This class holds the thermodynamic properties for a given O/F ratio.
    /// </summary>
    public class PropellantData
    {
        public float OF_Ratio { get; set; }
        public float Chamber_Pressure { get; set; }  // Pc (psia)
        public float Chamber_Temp_R { get; set; }    // Tc (R)
        public float Throat_Temp_R { get; set; }     // Tt (R)
        public float Gamma_Chamber { get; set; }     // gamma
        public float Isp { get; set; }              // Isp (s)
        public float Expansion_Ratio { get; set; }   
    }

    /// <summary>
    /// Manages loading and providing access to the propellant data from the CSV.
    /// This class replaces the need for XLOOKUP in the C# code.
    /// </summary>
    public class PropellantDataService
    {
        private readonly List<PropellantData> _data;

        public PropellantDataService(string csvFilePath)
        {
            _data = LoadDataFromCsv(csvFilePath);
        }

        /// <summary>
        /// Loads the CSV file into a list of PropellantData objects.
        /// </summary>
        private List<PropellantData> LoadDataFromCsv(string filePath)
        {
            var dataList = new List<PropellantData>();
            var lines = File.ReadAllLines(filePath).Skip(1); // Skip header

            int lineNumber = 1;
            foreach (var line in lines)
            {
                lineNumber++;
                var values = line.Split(',').Select(v => v.Trim()).ToArray();
                if (values.Length >= 8)
                {
                    // Use TryParse for more robust error handling
                    if (float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float ofRatio) &&
                        float.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float pressure) &&
                        float.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float isp) &&
                        float.TryParse(values[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float chamberTemp) &&
                        float.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out float throatTemp) &&
                        float.TryParse(values[7], NumberStyles.Any, CultureInfo.InvariantCulture, out float gamma) &&
                        float.TryParse(values[8], NumberStyles.Any, CultureInfo.InvariantCulture, out float expansionRatio)
                        )
                    {
                        dataList.Add(new PropellantData
                        {
                            OF_Ratio = ofRatio,
                            Chamber_Pressure = pressure,
                            Chamber_Temp_R = chamberTemp,
                            Throat_Temp_R = throatTemp,
                            Gamma_Chamber = gamma,
                            Isp = isp,
                            Expansion_Ratio = expansionRatio
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Could not parse line {lineNumber} in {filePath}. Skipping.");
                    }
                }
            }
            return dataList;
        }

        /// <summary>
        /// Finds the closest data point for a given O/F ratio.
        /// This is the C# equivalent of the XLOOKUP function used in the spreadsheet.
        /// </summary>
        public PropellantData FindClosest(float targetOfRatio, float targetPressure)
        {
            if (_data == null || !_data.Any()) return null;

            // First, find all entries with the closest O/F ratio
            var ofMatches = _data
                .GroupBy(d => Math.Abs(d.OF_Ratio - (float)targetOfRatio))
                .OrderBy(g => g.Key)
                .First();

            // Then from those, find the one with the closest chamber pressure
            return ofMatches
                .OrderBy(d => Math.Abs(d.Chamber_Pressure - (float)targetPressure))
                .FirstOrDefault();
        }
    }

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

            // Assign properties from the looked-up data
            m_fOFRatio = propellantData.OF_Ratio;
            m_fChamberPressure_PSI = propellantData.Chamber_Pressure;
            m_fChamberTemp_R = propellantData.Chamber_Temp_R;
            m_fThroatTemp_R = propellantData.Throat_Temp_R;
            m_fGammaChamber = propellantData.Gamma_Chamber;
            m_fIsp = propellantData.Isp;
            m_fExpansionRatio = propellantData.Expansion_Ratio;
        }

        public void Calculate()
        {
            m_fTotalWeightFlow = fThrust / m_fIsp;
            m_fFuelWeightFlow = m_fTotalWeightFlow / (1.0f + m_fOFRatio);
            m_fOxWeightFlow = m_fTotalWeightFlow - m_fFuelWeightFlow;
            m_fThroatPressure_PSI = m_fChamberPressure_PSI * 0.564f;
            m_fThroatArea_in2 = (m_fTotalWeightFlow / m_fThroatPressure_PSI) * (float)Math.Sqrt((fGasConstant * m_fThroatTemp_R) / (fGravityConstant * m_fGammaChamber));
            m_fThroatDiameter_in = (float)Math.Sqrt((m_fThroatArea_in2 * 4.0f) / Math.PI);
            m_fExitArea_in2 = m_fThroatArea_in2 * m_fExpansionRatio;
            m_fExitDiameter_in = (float)Math.Sqrt((m_fExitArea_in2 * 4.0f) / Math.PI);
            m_fChamberVolume = fLStar * m_fThroatArea_in2;
            m_fChamberDiameter_in = m_fThroatArea_in2 * fChamberToThroatRatio;
            m_fChamberArea_in2 = (float)(Math.PI * Math.Pow(m_fChamberDiameter_in / 2.0f, 2.0f));
            m_fChamberLength_in = m_fVolumeChamber / (1.1f * m_fChamberArea_in2);
            m_fMinimumWallThickness_in = (m_fChamberPressure_PSI * m_fChamberDiameter_in) / (2.0f * fCopperStress);
            m_fWallThickness = m_fMinimumWallThickness_in * 3.0f;
            m_fConvergentLength_in = ((m_fChamberDiameter_in / 2) - (m_fThroatDiameter_in / 2)) / (float)Math.Tan(fConvergentAngle_deg * ((float)Math.PI / 180.0f));
            m_fDivergentLength_in = ((m_fExitDiameter_in / 2) - (m_fThroatDiameter_in / 2)) / (float)Math.Tan(fDivergentAngle_deg * ((float)Math.PI / 180.0f));
            m_fChamberSurfaceArea = 2 * (float)Math.PI * ((m_fChamberDiameter_in + m_fWallThickness) / 2) * m_fChamberLength_in + 2 * (float)Math.PI * (float)Math.Pow((m_fChamberDiameter_in + m_fWallThickness) / 2, 2.0f);
            m_fConvergentSurfaceArea = (float)Math.PI * (float)Math.Pow(((m_fChamberDiameter_in / 2.0f) + m_fWallThickness), 2.0) + (float)Math.PI * (float)Math.Pow(((m_fThroatDiameter_in / 2.0f) + m_fWallThickness), 2.0) + (float)Math.PI * ((((m_fChamberDiameter_in / 2.0f) + m_fWallThickness) + ((m_fThroatDiameter_in / 2.0f) + m_fWallThickness))) * (float)Math.Sqrt((float)Math.Pow(m_fConvergentLength_in, 2.0) + (float)Math.Pow((((m_fThroatDiameter_in / 2.0f) + m_fWallThickness) - ((m_fChamberDiameter_in / 2.0f) + m_fWallThickness)), 2.0));
            m_fDivergentSurfaceArea = (float)Math.PI * (float)Math.Pow(((m_fExitDiameter_in / 2.0f) + m_fWallThickness), 2.0) + (float)Math.PI * (float)Math.Pow(((m_fThroatDiameter_in / 2.0f) + m_fWallThickness), 2.0) + (float)Math.PI * ((((m_fExitDiameter_in / 2.0f) + m_fWallThickness) + ((m_fThroatDiameter_in / 2.0f) + m_fWallThickness))) * (float)Math.Sqrt((float)Math.Pow(m_fDivergentLength_in, 2.0) + (float)Math.Pow((((m_fThroatDiameter_in / 2.0f) + m_fWallThickness) - ((m_fExitDiameter_in / 2.0f) + m_fWallThickness)), 2.0));
            m_fTotalSurfaceArea = m_fChamberSurfaceArea + m_fConvergentSurfaceArea + m_fDivergentSurfaceArea;
            m_fHeatTransferred = m_fTotalSurfaceArea * fHeatTransferRate;
            m_fWaterFlowWeight = m_fHeatTransferred / fWaterTemperatureRise;
            m_fChamberOuterDiameter_in = m_fChamberDiameter_in + (2.0f * m_fWallThickness);
            m_fChamberOuterDiameter_ft = m_fChamberOuterDiameter_in / 12.0f;
            m_fWaterJacketDiameter_ft = (float)Math.Sqrt((4 * m_fWaterFlowWeight) / (fWaterVelocity * fWaterRHO * (float)Math.PI) + m_fChamberOuterDiameter_ft * m_fChamberOuterDiameter_ft);
            m_fWaterJacketDiameter_in = m_fWaterJacketDiameter_ft * 12.0f;
            m_fWaterFlowGap = m_fWaterJacketDiameter_in - m_fChamberOuterDiameter_in;
        }
        private float fThrust;
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
        private float m_fVolumeChamber;
        private float m_fWallThickness;
        private float m_fHeatTransferred;
        private float m_fWaterFlowWeight;
        private float m_fChamberOuterDiameter_in;
        private float m_fChamberOuterDiameter_ft;
        private float m_fWaterJacketDiameter_ft;
        private float m_fWaterJacketDiameter_in;
        private float m_fWaterFlowGap;
    }
}
