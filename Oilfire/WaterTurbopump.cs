using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PicoGK;

namespace Test
{
    public class WaterTurbopump
    {
        float fTargetPressureRise_PSI = 40.0f;
        float fTargetFlowRate_GPM = 10.0f;
        float fTargetFlowRate_LBFT3 = fTargetFlowRate_GPM / 448.8f;
        float fRotationalSpeed_RPM = 15000.0f;
        float fWaterDensity_LBFT3 = 62.4f;
        float fPumpEfficiency = 0.7f;
        float fGravitationalConstant_FTSEC2 = 32.174f;
        float fInletVelocity_FTPSEC = 20.0f;
        float fDischargePipeVelocity_FTPSEC = 10.0f;
        float fOutletRadialVelocity_PERCENT = 0.2f;

        public WaterTurbopump()
        {

        }

        public void Calculate()
        {
            m_fPressureHead = (fTargetPressureRise_PSI * 144.0f) / fWaterDensity_LBFT3;
            m_fMechanicalPower = (fTargetFlowRate_LBFT3 * fTargetPressureRise_PSI) / (1714.0f * fPumpEfficiency);
            m_fSpecificSpeed = (fRotationalSpeed_RPM * (float)Math.Sqrt(fTargetFlowRate_GPM)) / ((float)Math.Pow(m_fPressureHead, 0.75f));
            m_fTipSpeed = (float)Math.Sqrt(m_fPressureHead * fGravitationalConstant_FTSEC2);
            m_fImpellerDiameter_FT = (60.0f * m_fTipSpeed) / ((float)Math.PI * fRotationalSpeed_RPM);
            m_fImpellerDiameter_IN = m_fImpellerDiameter_FT * 12.0f;
            m_fInletDiameter_FT = (float)Math.Sqrt((4.0f * fTargetFlowRate_LBFT3) / (Math.PI * fInletVelocity_FTPSEC));
            m_fInletDiameter_IN = m_fInletDiameter_FT * 12.0f;
            m_fOutletRadialVelocity_FTPSEC = fOutletRadialVelocity_PERCENT * m_fTipSpeed;
            m_fOutletDiameter_FT = fTargetFlowRate_GPM / ((float)Math.PI * m_fImpellerDiameter_FT * m_fOutletRadialVelocity_FTPSEC);
            m_fOutletDiameter_IN = m_fOutletDiameter_FT * 12.0f;
            m_fPortDiameter_FT = (float)Math.Sqrt((4.0f * fTargetFlowRate_LBFT3) / (Math.PI * fDischargePipeVelocity_FTPSEC));
            m_fPortDiameter_IN = m_fPortDiameter_FT * 12.0f;
        }

        private float m_fPressureHead;
        private float m_fMechanicalPower;
        private float m_fSpecificSpeed;
        private float m_fTipSpeed;
        private float m_fImpellerDiameter_FT;
        private float m_fImpellerDiameter_IN;
        private float m_fInletDiameter_FT;
        private float m_fInletDiameter_IN;
        private float m_fOutletRadialVelocity_FTPSEC;
        private float m_fOutletDiameter_FT;
        private float m_fOutletDiameter_IN;
        private float m_fPortDiameter_FT;
        private float m_fPortDiameter_IN;
    }
}