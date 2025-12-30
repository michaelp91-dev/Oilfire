using System;
using System.Collections.Generic;
using PicoGK;

namespace RocketEngineBuilder
{
    /// <summary>
    /// Represents a combustion chamber geometry with propellant data and geometry creation.
    /// Calculation logic has been moved to CombustionChamberCalculations in Calculations.cs.
    /// </summary>
    public class CombustionChamber
    {
        public CombustionChamber(Dictionary<string, float> dictInputs)
        {
            m_dictInputs = dictInputs;
        }

        public void CreateCombustionChamber()
        {
            float fChamberLength_mm = m_dictInputs["ChamberLength_mm"];
            float fConvergentLength_mm = m_dictInputs["ConvergentLength_mm"];
            float fDivergentLength_mm = m_dictInputs["DivergentLength_mm"];
            float fChamberRadius_mm = m_dictInputs["ChamberRadius_mm"];
            float fThroatRadius_mm = m_dictInputs["ThroatRadius_mm"];
            float fExitRadius_mm = m_dictInputs["ExitRadius_mm"];
            float fWallThickness_mm = m_dictInputs["WallThickness_mm"];

            float fLengthToThroat = fChamberLength_mm + fConvergentLength_mm;
            float fLengthToExit = fLengthToThroat + fDivergentLength_mm;

            Lattice latInside = new Lattice();
            latInside.AddBeam(new(0, 0, 0),
                                new(0, 0, -fChamberLength_mm),
                                fChamberRadius_mm,
                                fChamberRadius_mm,
                                false);
            latInside.AddBeam(new(0, 0, -fChamberLength_mm),
                                new(0, 0, -fLengthToThroat),
                                fChamberRadius_mm,
                                fThroatRadius_mm,
                                false);
            latInside.AddBeam(new(0, 0, -fLengthToThroat),
                                new(0, 0, -fLengthToExit),
                                fThroatRadius_mm,
                                fExitRadius_mm,
                                false);

            Lattice latOutside = new Lattice();
            latOutside.AddBeam(new(0, 0, 0),
                                new(0, 0, -fChamberLength_mm),
                                fChamberRadius_mm + fWallThickness_mm,
                                fChamberRadius_mm + fWallThickness_mm,
                                false);
            latOutside.AddBeam(new(0, 0, -fChamberLength_mm),
                                new(0, 0, -fLengthToThroat),
                                fChamberRadius_mm + fWallThickness_mm,
                                fThroatRadius_mm + fWallThickness_mm,
                                false);
            latOutside.AddBeam(new(0, 0, -fLengthToThroat),
                                new(0, 0, -fLengthToExit),
                                fThroatRadius_mm + fWallThickness_mm,
                                fExitRadius_mm + fWallThickness_mm,
                                false);

            Voxels voxInside = new(latInside);
            Voxels voxOutside = new(latOutside);
            voxOutside.BoolSubtract(voxInside);
            m_voxCombustionChamber = voxOutside;
        }

        public void CreateFlange(
            float fGrooveThicknessMM,
            float fGrooveDepthMM,
            float fDrillSpaceMM,
            int iNumberOfHoles,
            float fBoltDiameterMM)
        {
            float fChamberDiameterMM = m_dictInputs["ChamberDiameter_mm"];
            float fWallThicknessMM = m_dictInputs["WallThickness_mm"];
            m_fFlangeOuterRadius = (fChamberDiameterMM / 2f) + fWallThicknessMM + fGrooveThicknessMM + fDrillSpaceMM;
            m_fFlangeInnerRadius = (fChamberDiameterMM / 2f) + fWallThicknessMM + fGrooveThicknessMM;
            m_fRadiusToGroove = (fChamberDiameterMM / 2f) + fWallThicknessMM + fGrooveThicknessMM;
            m_fFlangeDepthWithGroove = fGrooveDepthMM + fWallThicknessMM;
            m_fFlangeDepthWOGroove = fWallThicknessMM;

            Lattice latFlangeWithGroove = new();
            latFlangeWithGroove.AddBeam(
                new(0, 0, 0),
                new(0, 0, -m_fFlangeDepthWithGroove),
                m_fFlangeOuterRadius,
                m_fFlangeOuterRadius,
                false);

            Lattice latFlangeWOGroove = new();
            latFlangeWOGroove.AddBeam(
                new(0, 0, 0),
                new(0, 0, m_fFlangeDepthWOGroove),
                m_fFlangeOuterRadius,
                m_fFlangeOuterRadius,
                false);

            Lattice latFlangeGrooveRemoval = new();
            latFlangeGrooveRemoval.AddBeam(
                new(0, 0, 0),
                new(0, 0, -fGrooveDepthMM),
                m_fRadiusToGroove,
                m_fRadiusToGroove,
                false);

            Lattice latFlangeChamberRemoval = new();
            latFlangeChamberRemoval.AddBeam(
                new(0, 0, 100),
                new(0, 0, -100),
                (fChamberDiameterMM / 2f),
                (fChamberDiameterMM / 2f),
                false);

            Lattice latDrillRemoval = new();
            m_fDrillMiddleLine = (m_fFlangeOuterRadius + m_fFlangeInnerRadius) / 2;
            for (float angle = 0; angle < 360; angle += (360 / iNumberOfHoles))
            {
                float angleRAD = angle * ((float)Math.PI / 180.0f);
                float x = m_fDrillMiddleLine * (float)Math.Cos(angleRAD);
                float y = m_fDrillMiddleLine * (float)Math.Sin(angleRAD);
                latDrillRemoval.AddBeam(
                    new(x, y, 30),
                    new(x, y, -30),
                    fBoltDiameterMM / 2f,
                    fBoltDiameterMM / 2f,
                    false
                );
            }

            Voxels voxFlangeWithGroove = new(latFlangeWithGroove);
            Voxels voxFlangeWOGroove = new(latFlangeWOGroove);
            Voxels voxFlangeGrooveRemoval = new(latFlangeGrooveRemoval);
            Voxels voxFlangeChamberRemoval = new(latFlangeChamberRemoval);
            Voxels voxFlangeDrillRemoval = new(latDrillRemoval);

            voxFlangeWithGroove.BoolSubtract(voxFlangeChamberRemoval);
            voxFlangeWithGroove.BoolSubtract(voxFlangeGrooveRemoval);
            voxFlangeWithGroove.BoolSubtract(voxFlangeDrillRemoval);

            voxFlangeWOGroove.BoolSubtract(voxFlangeChamberRemoval);
            voxFlangeWOGroove.BoolSubtract(voxFlangeDrillRemoval);

            m_voxFlangeWithGroove = voxFlangeWithGroove;
            m_voxFlangeWOGroove = voxFlangeWOGroove;
        }

        public Voxels voxGetCombustionChamber()
        {
            return m_voxCombustionChamber;
        }

        public Voxels voxGetFlangeWithGroove()
        {
            return m_voxFlangeWithGroove;
        }

        public Voxels voxGetFlangeWOGroove()
        {
            return m_voxFlangeWOGroove;
        }

        public Voxels voxGetCombustionChamberFlangeAssembly()
        {
            Voxels voxAssembly = new(m_voxCombustionChamber);
            voxAssembly.BoolAdd(m_voxFlangeWithGroove);
            m_voxAssembly = voxAssembly;
            return voxAssembly;
        }
        public float fGetFlangeOuterRadius()
        {
            return m_fFlangeOuterRadius;
        }
        public float fGetDrillLine()
        {
            return m_fDrillMiddleLine;
        }
        public void Run()
        {
            Library.oViewer().Add(m_voxAssembly);
        }
        private float m_fFlangeOuterRadius;
        private float m_fFlangeInnerRadius;
        private float m_fRadiusToGroove;
        private float m_fFlangeDepthWithGroove;
        private float m_fFlangeDepthWOGroove;
        private float m_fDrillMiddleLine;
        private Voxels m_voxCombustionChamber;
        private Voxels m_voxFlangeWithGroove;
        private Voxels m_voxFlangeWOGroove;
        private Voxels m_voxAssembly;
        private Dictionary<string, float> m_dictInputs;
    }
}
