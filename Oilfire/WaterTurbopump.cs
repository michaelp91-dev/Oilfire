using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PicoGK;
using Leap71.ShapeKernel;

namespace RocketEngineBuilder
{
    public class WaterTurbopump
    {
        // --- CONSTANT DESIGN TARGETS ---
        float fTargetPressureRise_PSI = 40.0f;
        float fTargetFlowRate_GPM = 10.0f;
        float fRotationalSpeed_RPM = 15000.0f;
        float fWaterDensity_LBFT3 = 62.4f;
        float fPumpEfficiency = 0.7f;
        float fGravitationalConstant_FTSEC2 = 32.174f;
        float fInletVelocity_FTPSEC = 20.0f;
        float fDischargePipeVelocity_FTPSEC = 10.0f;
        float fOutletRadialVelocity_PERCENT = 0.2f;
        float fShellWallThickness_IN = 0.118f;
        float fBladeThickness_IN = 0.039f;
        float fClearance_IN = 0.039f;
        float fImpellerBase = 0.079f;

        // --- CONVERSION CONSTANTS ---
        const float GPM_TO_FT3S = 1.0f / 448.831f;
        const float PSI_TO_PSF = 144.0f;
        const float HP_CONVERSION_FACTOR = 1714.0f; // For PSI*GPM / HP formula

        public void Calculate()
        {
            float fTargetFlowRate_FT3S = fTargetFlowRate_GPM * GPM_TO_FT3S;
            m_fPressureHead = fTargetPressureRise_PSI * PSI_TO_PSF / fWaterDensity_LBFT3;
            m_fMechanicalPower_HP = (fTargetFlowRate_GPM * fTargetPressureRise_PSI) / (HP_CONVERSION_FACTOR * fPumpEfficiency);
            m_fSpecificSpeed = (fRotationalSpeed_RPM * (float)Math.Sqrt(fTargetFlowRate_GPM)) / (float)Math.Pow(m_fPressureHead, 0.75);
            m_fTipSpeed_FTPSEC = (float)Math.Sqrt(m_fPressureHead * fGravitationalConstant_FTSEC2);
            m_fImpellerOuterDiameter_FT = (60.0f * m_fTipSpeed_FTPSEC) / ((float)Math.PI * fRotationalSpeed_RPM);
            m_fImpellerOuterDiameter_IN = m_fImpellerOuterDiameter_FT * 12.0f;
            m_fInletDiameter_FT = (float)Math.Sqrt((4.0f * fTargetFlowRate_FT3S) / ((float)Math.PI * fInletVelocity_FTPSEC));
            m_fInletDiameter_IN = m_fInletDiameter_FT * 12.0f;
            m_fOutletRadialVelocity_FTPSEC = m_fTipSpeed_FTPSEC * fOutletRadialVelocity_PERCENT;
            m_fImpellerWidth_FT = fTargetFlowRate_FT3S / ((float)Math.PI * (m_fImpellerOuterDiameter_FT * m_fOutletRadialVelocity_FTPSEC));
            m_fImpellerWidth_IN = m_fImpellerWidth_FT * 12.0f;
            m_fPortDiameter_FT = (float)Math.Sqrt((4.0f * fTargetFlowRate_FT3S) / ((float)Math.PI * fDischargePipeVelocity_FTPSEC));
            m_fPortDiameter_IN = m_fPortDiameter_FT * 12.0f;
            m_fImpellerHeight_IN = m_fImpellerWidth_IN * fImpellerBase;
            m_fShellInnerDiameter_IN = m_fImpellerOuterDiameter_IN + (2.0f * fClearance_IN);
            m_fShellOuterDiameter_IN = m_fShellInnerDiameter_IN + (2.0f * fShellWallThickness_IN);
            m_fShellInnerHeight_IN = m_fImpellerHeight_IN + (2.0f * fClearance_IN);
            m_fShellOuterHeight_IN = m_fShellInnerHeight_IN + (2.0f * fShellWallThickness_IN);

            m_fImpellerOuterDiameter_MM = m_fImpellerOuterDiameter_IN * 25.4f;
            m_fInletDiameter_MM = m_fInletDiameter_IN * 25.4f;
            m_fImpellerWidth_MM = m_fImpellerWidth_IN * 25.4f;
            m_fPortDiameter_MM = m_fPortDiameter_IN * 25.4f;
            m_fShellInnerDiameter_MM = m_fShellInnerDiameter_IN * 25.4f;
            m_fShellOuterDiameter_MM = m_fShellOuterDiameter_IN * 25.4f;
            m_fShellInnerHeight_MM = m_fShellInnerHeight_IN * 25.4f;
            m_fShellOuterHeight_MM = m_fShellOuterHeight_IN * 25.4f;

            Console.WriteLine("Water Turbopump Design Parameters:");
            Console.WriteLine($"  Impeller Outer Diameter: {m_fImpellerOuterDiameter_IN:F2} in ({m_fImpellerOuterDiameter_MM:F2} mm)");
            Console.WriteLine($"  Inlet Diameter: {m_fInletDiameter_IN:F2} in ({m_fInletDiameter_MM:F2} mm)");
            Console.WriteLine($"  Impeller Width: {m_fImpellerWidth_IN:F2} in ({m_fImpellerWidth_MM:F2} mm)");
            Console.WriteLine($"  Port Diameter: {m_fPortDiameter_IN:F2} in ({m_fPortDiameter_MM:F2} mm)");
            Console.WriteLine($"  Shell Inner Diameter: {m_fShellInnerDiameter_IN:F2} in ({m_fShellInnerDiameter_MM:F2} mm)");
            Console.WriteLine($"  Shell Outer Diameter: {m_fShellOuterDiameter_IN:F2} in ({m_fShellOuterDiameter_MM:F2} mm)");
            Console.WriteLine($"  Shell Inner Height: {m_fShellInnerHeight_IN:F2} in ({m_fShellInnerHeight_MM:F2} mm)");
            Console.WriteLine($"  Shell Outer Height: {m_fShellOuterHeight_IN:F2} in ({m_fShellOuterHeight_MM:F2} mm)");
        }

        public void CreateWaterTurbopumpShell()
        {
            Lattice latShellExterior = new();
            latShellExterior.AddBeam(new(0, 0, 0),
                                        new(0, 0, m_fShellOuterHeight_MM),
                                        m_fShellOuterDiameter_MM / 2.0f,
                                        m_fShellOuterDiameter_MM / 2.0f,
                                        false);
            m_voxShell = new Voxels(latShellExterior);

            Lattice latShellInterior = new();
            latShellInterior.AddBeam(new(0, 0, fShellWallThickness_IN * 25.4f),
                                        new(0, 0, (m_fShellOuterHeight_IN - fShellWallThickness_IN) * 25.4f),
                                        m_fShellInnerDiameter_MM / 2.0f,
                                        m_fShellInnerDiameter_MM / 2.0f,
                                        false);
            latShellInterior.AddBeam(new(m_fShellInnerDiameter_MM / 2.0f, 0, m_fShellOuterHeight_MM),
                                        new(m_fShellInnerDiameter_MM / 2.0f, 50.0f, m_fShellOuterHeight_MM),
                                        m_fPortDiameter_MM / 2.0f,
                                        m_fPortDiameter_MM / 2.0f,
                                        false);
            Voxels voxShellInterior = new Voxels(latShellInterior);
            m_voxShell.BoolSubtract(voxShellInterior);

            Lattice latInletHole = new();
            latInletHole.AddBeam(new(0, 0, fShellWallThickness_IN * 25.4f),
                                    new(0, 0, m_fShellOuterHeight_MM),
                                    m_fInletDiameter_MM / 2.0f,
                                    m_fInletDiameter_MM / 2.0f,
                                    false);
            Voxels voxInletHole = new Voxels(latInletHole);
            m_voxShell.BoolSubtract(voxInletHole);
        }

        public void CreateWaterTurbopumpImpeller()
        {
            Lattice latImpellerBase = new();
            latImpellerBase.AddBeam(new(0, 0, 0),
                                        new(0, 0, m_fImpellerHeight_MM),
                                        m_fImpellerOuterDiameter_MM / 2.0f,
                                        m_fImpellerOuterDiameter_MM / 2.0f,
                                        false);
            m_voxImpeller = new Voxels(latImpellerBase);
        }

        public Voxels voxGetShell()
        {
            return m_voxShell;
        }

        public Voxels voxGetImpeller()
        {
            return m_voxImpeller;
        }

        float m_fPressureHead;
        float m_fMechanicalPower_HP;
        float m_fSpecificSpeed;
        float m_fTipSpeed_FTPSEC;
        float m_fImpellerOuterDiameter_FT;
        float m_fImpellerOuterDiameter_IN;
        float m_fInletDiameter_FT;
        float m_fInletDiameter_IN;
        float m_fOutletRadialVelocity_FTPSEC;
        float m_fImpellerWidth_FT;
        float m_fImpellerWidth_IN;
        float m_fPortDiameter_FT;
        float m_fPortDiameter_IN;
        float m_fShellInnerDiameter_IN;
        float m_fShellOuterDiameter_IN;
        float m_fShellInnerHeight_IN;
        float m_fShellOuterHeight_IN;
        float m_fImpellerHeight_IN;
        float m_fImpellerOuterDiameter_MM;
        float m_fInletDiameter_MM;
        float m_fImpellerWidth_MM;
        float m_fPortDiameter_MM;
        float m_fShellInnerDiameter_MM;
        float m_fShellOuterDiameter_MM;
        float m_fShellInnerHeight_MM;
        float m_fShellOuterHeight_MM;
        float m_fImpellerHeight_MM;
        Voxels m_voxShell;
        Voxels m_voxImpeller;
    }
}