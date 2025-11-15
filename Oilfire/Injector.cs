using PicoGK;
using System;

namespace RocketEngineBuilder
{
    public class Injector
    {
        float fOxidizerPipeDiameterMM = 0.375f * 25.4f;
        float fFuelPipeDiameterMM = 0.25f * 25.4f;

        public Injector(
            float fChamberDiameterMM,
            int iNumberOfOxidizerHoles,
            int iNumberOfFuelHoles,
            float fOxidizerHoleDiameterMM,
            float fFuelHoleDiameterMM,
            float fWallThicknessMM)
        {
            m_fInjectorRadius = (fChamberDiameterMM / 2f) + fWallThicknessMM;
            m_fOxidizerHoleRadius = fOxidizerHoleDiameterMM / 2f;
            m_fFuelHoleRadius = fFuelHoleDiameterMM / 2f;
            m_fOxidizerPlacementLine = m_fInjectorRadius * 0.333f;
            m_fFuelPlacementLine = m_fInjectorRadius * 0.666f;
            m_iNumberOfOxidizerHoles = iNumberOfOxidizerHoles;
            m_iNumberOfFuelHoles = iNumberOfFuelHoles;
            m_fWallThicknessMM = fWallThicknessMM;
        }

        public void CreateInjectorPlate(float fFlangeOuterRadius, float fBoltDiameterMM, float fDrillLine, int iNumberOfBolts)
        {
            Lattice latInjector = new();
            latInjector.AddBeam(
                new(0, 0, 0),
                new(0, 0, -m_fWallThicknessMM),
                fFlangeOuterRadius,
                fFlangeOuterRadius,
                false);

            Lattice latHoleRemoval = new();
            for (float angle = 0; angle < 360; angle += 360 / m_iNumberOfOxidizerHoles)
            {
                float angleRAD = angle * ((float)Math.PI / 180.0f);
                float x = m_fOxidizerPlacementLine * (float)Math.Cos(angleRAD);
                float y = m_fOxidizerPlacementLine * (float)Math.Sin(angleRAD);
                latHoleRemoval.AddBeam(
                     new(x, y, 30),
                     new(x, y, -30),
                     m_fOxidizerHoleRadius,
                     m_fOxidizerHoleRadius,
                     false
                 );
            }
            for (float angle = 0; angle < 360; angle += 360 / m_iNumberOfFuelHoles)
            {
                float angleRAD = angle * ((float)Math.PI / 180.0f);
                float x = m_fFuelPlacementLine * (float)Math.Cos(angleRAD);
                float y = m_fFuelPlacementLine * (float)Math.Sin(angleRAD);
                latHoleRemoval.AddBeam(
                     new(x, y, 30),
                     new(x, y, -30),
                     m_fFuelHoleRadius,
                     m_fFuelHoleRadius,
                     false
                 );
            }
            for (float angle = 0; angle < 360; angle += 360 / iNumberOfBolts)
            {
                float angleRAD = angle * ((float)Math.PI / 180.0f);
                float x = fDrillLine * (float)Math.Cos(angleRAD);
                float y = fDrillLine * (float)Math.Sin(angleRAD);
                latHoleRemoval.AddBeam(
                     new(x, y, 30),
                     new(x, y, -30),
                     fBoltDiameterMM / 2f,
                     fBoltDiameterMM / 2f,
                     false
                 );
            }

            Voxels voxInjectorPlate = new(latInjector);
            Voxels voxHoleRemoval = new(latHoleRemoval);
            voxInjectorPlate.BoolSubtract(voxHoleRemoval);
            m_voxInjectorPlate = voxInjectorPlate;
        }

        public void CreateOxidizerInjectorManifold()
        {
            Lattice latOxidizerManifold = new();
            latOxidizerManifold.AddBeam(
                new(fOxidizerPipeDiameterMM, 0, 100),
                new(fOxidizerPipeDiameterMM, 0, 150),
                m_fOxidizerHoleRadius + m_fWallThicknessMM / 3f,
                m_fOxidizerHoleRadius + m_fWallThicknessMM / 3f,
                false);
            latOxidizerManifold.AddBeam(
                new(0, 0, 75),
                new(fOxidizerPipeDiameterMM, 0, 100),
                m_fOxidizerHoleRadius + m_fWallThicknessMM / 3f,
                m_fOxidizerHoleRadius + m_fWallThicknessMM / 3f,
                true);
            latOxidizerManifold.AddBeam(
                new(0, 0, 50),
                new(0, 0, 75),
                m_fOxidizerHoleRadius + m_fWallThicknessMM / 3f,
                m_fOxidizerHoleRadius + m_fWallThicknessMM / 3f,
                false);
            for (float angle = 0; angle < 360; angle += 360 / m_iNumberOfOxidizerHoles)
            {
                float angleRAD = angle * ((float)Math.PI / 180.0f);
                float x = m_fOxidizerPlacementLine * (float)Math.Cos(angleRAD);
                float y = m_fOxidizerPlacementLine * (float)Math.Sin(angleRAD);
                latOxidizerManifold.AddBeam(
                     new(0, 0, 50),
                     new(x, y, 10),
                     m_fOxidizerHoleRadius + m_fWallThicknessMM / 3f,
                     m_fOxidizerHoleRadius + m_fWallThicknessMM / 3f,
                     true
                 );
                latOxidizerManifold.AddBeam(
                    new(x, y, 10),
                    new(x, y, -m_fWallThicknessMM / 2f),
                    m_fOxidizerHoleRadius + m_fWallThicknessMM / 3f,
                    m_fOxidizerHoleRadius + m_fWallThicknessMM / 3f,
                    false
                );
            }
            Lattice latOxidzerManifoldHoleRemoval = new();
            latOxidizerManifold.AddBeam(
                new(fOxidizerPipeDiameterMM, 0, 100),
                new(fOxidizerPipeDiameterMM, 0, 150),
                m_fOxidizerHoleRadius,
                m_fOxidizerHoleRadius,
                false);
            latOxidizerManifold.AddBeam(
                new(0, 0, 75),
                new(fOxidizerPipeDiameterMM, 0, 100),
                m_fOxidizerHoleRadius,
                m_fOxidizerHoleRadius,
                true);
            latOxidizerManifold.AddBeam(
                new(0, 0, 50),
                new(0, 0, 75),
                m_fOxidizerHoleRadius,
                m_fOxidizerHoleRadius,
                false);
            for (float angle = 0; angle < 360; angle += 360 / m_iNumberOfOxidizerHoles)
            {
                float angleRAD = angle * ((float)Math.PI / 180.0f);
                float x = m_fOxidizerPlacementLine * (float)Math.Cos(angleRAD);
                float y = m_fOxidizerPlacementLine * (float)Math.Sin(angleRAD);
                latOxidzerManifoldHoleRemoval.AddBeam(
                     new(0, 0, 50),
                     new(x, y, 10),
                     m_fOxidizerHoleRadius,
                     m_fOxidizerHoleRadius,
                     true
                 );
                latOxidzerManifoldHoleRemoval.AddBeam(
                    new(x, y, 10),
                    new(x, y, 0),
                    m_fOxidizerHoleRadius,
                    m_fOxidizerHoleRadius,
                    false
                );

            }
            Voxels voxOxidizerManifoldHoleRemoval = new(latOxidzerManifoldHoleRemoval);
            Lattice latFlattenBottom = new();
            latFlattenBottom.AddBeam(
                new(0, 0, -1),
                new(0, 0, -150),
                100.0f,
                100.0f,
                false
            );
            Voxels voxFlattenBottom = new(latFlattenBottom);
            m_voxOxidizerFlowArea = new(latOxidzerManifoldHoleRemoval);
            m_voxOxidizerManifold = new(latOxidizerManifold);
            m_voxOxidizerManifold.BoolSubtract(voxOxidizerManifoldHoleRemoval);
            m_voxOxidizerManifold.BoolSubtract(voxFlattenBottom);
        }

        public void CreateFuelInjectorManifold()
        {
            Lattice latFuelInjectorManifold = new();
            latFuelInjectorManifold.AddBeam(
                new(0, 0, 150),
                new(0, 0, 125),
                m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                false);
            latFuelInjectorManifold.AddBeam(
                new(0, 0, 125),
                new(0, fOxidizerPipeDiameterMM, 75),
                m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                true);
            latFuelInjectorManifold.AddBeam(
                new(0, 0, 125),
                new(0, -fOxidizerPipeDiameterMM, 75),
                m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                true);
            latFuelInjectorManifold.AddBeam(
                new(0, fOxidizerPipeDiameterMM, 75),
                new(0, fOxidizerPipeDiameterMM, 50),
                m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                true);
            latFuelInjectorManifold.AddBeam(
                new(0, -fOxidizerPipeDiameterMM, 75),
                new(0, -fOxidizerPipeDiameterMM, 50),
                m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                true);
            for (float angle = 0; angle < 180; angle += 360 / m_iNumberOfFuelHoles)
            {
                float angleRAD = angle * ((float)Math.PI / 180.0f);
                float x = m_fFuelPlacementLine * (float)Math.Cos(angleRAD);
                float y = m_fFuelPlacementLine * (float)Math.Sin(angleRAD);
                latFuelInjectorManifold.AddBeam(
                     new(0, fOxidizerPipeDiameterMM, 50),
                     new(x, y, 10),
                     m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                     m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                     true
                );
                latFuelInjectorManifold.AddBeam(
                     new(x, y, 10),
                     new(x, y, -m_fWallThicknessMM / 2f),
                     m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                     m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                     false
                );
            }
            for (float angle = 180; angle < 360; angle += 360 / m_iNumberOfFuelHoles)
            {
                float angleRAD = angle * ((float)Math.PI / 180.0f);
                float x = m_fFuelPlacementLine * (float)Math.Cos(angleRAD);
                float y = m_fFuelPlacementLine * (float)Math.Sin(angleRAD);
                latFuelInjectorManifold.AddBeam(
                     new(0, -fOxidizerPipeDiameterMM, 50),
                     new(x, y, 10),
                     m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                     m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                     true
                );
                latFuelInjectorManifold.AddBeam(
                     new(x, y, 10),
                     new(x, y, -m_fWallThicknessMM / 2f),
                     m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                     m_fFuelHoleRadius + m_fWallThicknessMM / 3f,
                     false
                );
            }

            Lattice latFuelManifoldHoleRemoval = new();
            latFuelManifoldHoleRemoval.AddBeam(
                new(0, 0, 150),
                new(0, 0, 125),
                m_fFuelHoleRadius,
                m_fFuelHoleRadius,
                false);
            latFuelManifoldHoleRemoval.AddBeam(
                new(0, 0, 125),
                new(0, fOxidizerPipeDiameterMM, 75),
                m_fFuelHoleRadius,
                m_fFuelHoleRadius,
                true);
            latFuelManifoldHoleRemoval.AddBeam(
                new(0, 0, 125),
                new(0, -fOxidizerPipeDiameterMM, 75),
                m_fFuelHoleRadius,
                m_fFuelHoleRadius,
                true);
            latFuelManifoldHoleRemoval.AddBeam(
                new(0, fOxidizerPipeDiameterMM, 75),
                new(0, fOxidizerPipeDiameterMM, 50),
                m_fFuelHoleRadius,
                m_fFuelHoleRadius,
                true);
            latFuelManifoldHoleRemoval.AddBeam(
                new(0, -fOxidizerPipeDiameterMM, 75),
                new(0, -fOxidizerPipeDiameterMM, 50),
                m_fFuelHoleRadius,
                m_fFuelHoleRadius,
                true);
            latFuelManifoldHoleRemoval.AddBeam(
                new(0, fOxidizerPipeDiameterMM, 75),
                new(0, fOxidizerPipeDiameterMM, 50),
                m_fFuelHoleRadius,
                m_fFuelHoleRadius,
                true);
            latFuelManifoldHoleRemoval.AddBeam(
                new(0, -fOxidizerPipeDiameterMM, 75),
                new(0, -fOxidizerPipeDiameterMM, 50),
                m_fFuelHoleRadius,
                m_fFuelHoleRadius,
                true);
            for (float angle = 0; angle < 180; angle += 360 / m_iNumberOfFuelHoles)
            {
                float angleRAD = angle * ((float)Math.PI / 180.0f);
                float x = m_fFuelPlacementLine * (float)Math.Cos(angleRAD);
                float y = m_fFuelPlacementLine * (float)Math.Sin(angleRAD);
                latFuelManifoldHoleRemoval.AddBeam(
                     new(0, fOxidizerPipeDiameterMM, 50),
                     new(x, y, 10),
                     m_fFuelHoleRadius,
                     m_fFuelHoleRadius,
                     false
                );
                latFuelManifoldHoleRemoval.AddBeam(
                     new(x, y, 10),
                     new(x, y, 0),
                     m_fFuelHoleRadius,
                     m_fFuelHoleRadius,
                     false
                );
            }
            for (float angle = 180; angle < 360; angle += 360 / m_iNumberOfFuelHoles)
            {
                float angleRAD = angle * ((float)Math.PI / 180.0f);
                float x = m_fFuelPlacementLine * (float)Math.Cos(angleRAD);
                float y = m_fFuelPlacementLine * (float)Math.Sin(angleRAD);
                latFuelManifoldHoleRemoval.AddBeam(
                     new(x, y, 10),
                     new(x, y, 0),
                     m_fFuelHoleRadius,
                     m_fFuelHoleRadius,
                     false
                );
            }
            Voxels voxFuelManifoldHoleRemoval = new(latFuelManifoldHoleRemoval);
            m_voxFuelInjectorManifold = new(latFuelInjectorManifold);
            m_voxFuelInjectorManifold.BoolSubtract(voxFuelManifoldHoleRemoval);
        }

        public Voxels voxGetInjectorPlate()
        {
            return m_voxInjectorPlate;
        }
        public Voxels voxGetOxidizerManifold()
        {
            return m_voxOxidizerManifold;
        }
        public Voxels voxGetFuelInjectorManifold()
        {
            return m_voxFuelInjectorManifold;
        }
        public Voxels voxGetOxidizerFlowArea()
        {
            return m_voxOxidizerFlowArea;
        }
        public Voxels voxGetInjector()
        {
            m_voxInjector = new Voxels(m_voxInjectorPlate);
            m_voxInjector.BoolAdd(m_voxOxidizerManifold);
            m_voxInjector.BoolAdd(m_voxFuelInjectorManifold);
            return m_voxInjector;
        }


        private float m_fInjectorRadius;
        private float m_fOxidizerHoleRadius;
        private float m_fFuelHoleRadius;
        private float m_fOxidizerPlacementLine;
        private float m_fFuelPlacementLine;
        private float m_fFuelWallHeight;
        private float m_fOxidizerWallHeight;
        private float m_fWallThicknessMM;
        private int m_iNumberOfOxidizerHoles;
        private int m_iNumberOfFuelHoles;
        private float m_fFlangeOuterRadius;
        private Voxels m_voxInjectorPlate;
        private Voxels m_voxOxidizerManifold;
        private Voxels m_voxOxidizerFlowArea;
        private Voxels m_voxFuelInjectorManifold;
        private Voxels m_voxFuelFlowArea;
        private Voxels m_voxInjector;
    }
}