using System;
using System.Numerics;
using PicoGK;

namespace Oilfire.Geometry
{
    /// <summary>
    /// Generates the 3D geometry for the Thrust Chamber Assembly (TCA),
    /// which includes the combustion chamber and the nozzle.
    /// </summary>
    public class ThrustChamber : EngineComponent
    {
        private readonly EngineDesigner _engine;

        public ThrustChamber(EngineDesigner engine)
        {
            _engine = engine;
        }

        public override Voxels Build()
        {
            // Convert all design dimensions from meters to millimeters for PicoGK
            float chamberRadiusMm     = _engine.ChamberDiameterM * 500f;
            float throatRadiusMm      = _engine.NozzleThroatDiameterM * 500f;
            float exitRadiusMm        = _engine.NozzleExitDiameterM * 500f;
            float chamberLengthMm     = _engine.ChamberLengthM * 1000f;
            float wallThicknessMm     = _engine.PracticalChamberWallThicknessM * 1000f;
            
            // Define the nozzle angles from the reference PDF
            float convergenceAngle    = 60f * MathF.PI / 180f; // 60 degrees in radians
            float divergenceAngle     = 15f * MathF.PI / 180f; // 15 degrees in radians

            // Calculate the lengths of the nozzle sections based on the angles
            float convergenceLengthMm = (chamberRadiusMm - throatRadiusMm) / MathF.Tan(convergenceAngle);
            float divergenceLengthMm  = (exitRadiusMm - throatRadiusMm) / MathF.Tan(divergenceAngle);

            // Define Z coordinates for each section boundary
            float zChamberStart       = 0f;
            float zConvergenceStart   = -chamberLengthMm;
            float zThroat             = zConvergenceStart - convergenceLengthMm;
            float zExit               = zThroat - divergenceLengthMm;

            // --- Create the Outer Surface using a Lattice ---
            Lattice latOutside = new();
            // 1. Chamber Section
            latOutside.AddBeam( new Vector3(0, 0, zChamberStart),
                                new Vector3(0, 0, zConvergenceStart),
                                chamberRadiusMm + wallThicknessMm,
                                chamberRadiusMm + wallThicknessMm,
                                false);
            // 2. Convergent Section
            latOutside.AddBeam( new Vector3(0, 0, zConvergenceStart),
                                new Vector3(0, 0, zThroat),
                                chamberRadiusMm + wallThicknessMm,
                                throatRadiusMm + wallThicknessMm,
                                false);
            // 3. Divergent Section
            latOutside.AddBeam( new Vector3(0, 0, zThroat),
                                new Vector3(0, 0, zExit),
                                throatRadiusMm + wallThicknessMm,
                                exitRadiusMm + wallThicknessMm,
                                false);
            Voxels voxOutside = new(latOutside);

            // --- Create the Inner Surface (the empty volume to be subtracted) ---
            Lattice latInside = new();
            // 1. Chamber Section
            latInside.AddBeam(  new Vector3(0, 0, zChamberStart),
                                new Vector3(0, 0, zConvergenceStart),
                                chamberRadiusMm,
                                chamberRadiusMm,
                                false);
            // 2. Convergent Section
            latInside.AddBeam(  new Vector3(0, 0, zConvergenceStart),
                                new Vector3(0, 0, zThroat),
                                chamberRadiusMm,
                                throatRadiusMm,
                                false);
            // 3. Divergent Section
            latInside.AddBeam(  new Vector3(0, 0, zThroat),
                                new Vector3(0, 0, zExit),
                                throatRadiusMm,
                                exitRadiusMm,
                                false);
            Voxels voxInside = new(latInside);

            // The final solid part is the outer volume with the inner volume subtracted
            voxOutside.BoolSubtract(voxInside);
            
            return voxOutside;
        }
    }
}

