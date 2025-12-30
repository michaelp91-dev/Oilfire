using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PicoGK;
using RocketEngineBuilder;

float fVoxelSizeMM = 0.5f;

Console.Write("Viewer (1) or Headless (2)? ");
string userInput = Console.ReadLine();

if (userInput == "2")
{
    try
    {
        using (PicoGK.Library oLibrary = new(fVoxelSizeMM))
        {
        
            float fThrust = 1000.0f;
            float f_OFRatio = 4.0f;
            float fChamberPressure = 500.0f;
            float fEthanolDensityLB_FT3 = 49.3f;
            float fN2ODensityLB_FT3 = 53.2f;
            float fIsopropanolDensityLB_FT3 = 53.5f;
            float fFuelInjectorCd = 0.7f;
            float fFuelInjectorDeltaPPSI = 100f;
            int iNumberOfFuelHoles = 8;
            float fOxygenInjectorCd = 0.7f;
            float fOxygenInjectorDeltaPPSI = 100f;
            int iNumberOfOxidizerHoles = 8;
            String Ethanol_N2O_CSV = "N2O_C2H5OH.csv";
            String Isopropanol_N2O_CSV = "N2O_C3H8O.csv";
            PropellantDataService propellantDataService = new(Isopropanol_N2O_CSV);
    
            Calculations oCalculations = new Calculations(fThrust, f_OFRatio, fChamberPressure, propellantDataService);
            oCalculations.CalculateCombustionChamber();
            oCalculations.CalculateInjector(
                fIsopropanolDensityLB_FT3,
                fFuelInjectorCd,
                fFuelInjectorDeltaPPSI,
                iNumberOfFuelHoles,
                fN2ODensityLB_FT3,
                fOxygenInjectorCd,
                fOxygenInjectorDeltaPPSI,
                iNumberOfOxidizerHoles
            );
            Dictionary<string, float> dictResults = oCalculations.DictGetCalculations();
            //oCalculations.PrintResults();
            
            CombustionChamber oChamber = new CombustionChamber(dictResults);
    
            oChamber.CreateCombustionChamber();
    
            float fGrooveThickness = 1.0f;
            float fGrooveDepthMM = 2.0f;
            float fDrillSpaceMM = 20.0f;
            int iNumberOfHoles = 8;
            float fBoltDiameterMM = 6.35f;
            oChamber.CreateFlange(
                fGrooveThickness,
                fGrooveDepthMM,
                fDrillSpaceMM,
                iNumberOfHoles,
                fBoltDiameterMM
            );
            Voxels voxCombustionChamber = oChamber.voxGetCombustionChamber();
            Voxels voxFlangeWithGroove = oChamber.voxGetFlangeWithGroove();
            Voxels voxChamberWithFlange = oChamber.voxGetCombustionChamberFlangeAssembly();
            voxCombustionChamber.mshAsMesh().SaveToStlFile("STLs/CombustionChamber.stl");
            voxFlangeWithGroove.mshAsMesh().SaveToStlFile("STLs/FlangeWithGroove.stl");
            voxChamberWithFlange.mshAsMesh().SaveToStlFile("STLs/ChamberWithFlange.stl");
    
            float fFlangeOuterRadius = oChamber.fGetFlangeOuterRadius();
            float fDrillLine = oChamber.fGetDrillLine();
            Injector oInjector = new Injector(
                dictResults["ChamberDiameter_mm"],
                iNumberOfOxidizerHoles,
                iNumberOfFuelHoles,
                dictResults["OxidizerHoleDiameter_MM"],
                dictResults["FuelHoleDiameter_MM"],
                dictResults["WallThickness_mm"]
            );
            oInjector.CreateInjectorPlate(fFlangeOuterRadius, fBoltDiameterMM, fDrillLine, iNumberOfHoles);
            oInjector.CreateOxidizerInjectorManifold();
            oInjector.CreateFuelInjectorManifold();
            Voxels voxInjectorPlate = oInjector.voxGetInjectorPlate();
            Voxels voxOxidizerManifold = oInjector.voxGetOxidizerManifold();
            Voxels voxOxidizerFlowArea = oInjector.voxGetOxidizerFlowArea();
            Voxels voxFuelManifold = oInjector.voxGetFuelInjectorManifold();
            Voxels voxInjector = oInjector.voxGetInjector();
            voxInjectorPlate.mshAsMesh().SaveToStlFile("STLs/InjectorPlate.stl");
            voxOxidizerManifold.mshAsMesh().SaveToStlFile("STLs/OxidizerManifold.stl");
            voxFuelManifold.mshAsMesh().SaveToStlFile("STLs/FuelManifold.stl");
            voxOxidizerFlowArea.mshAsMesh().SaveToStlFile("STLs/OxidizerFlowArea.stl");
            voxInjector.mshAsMesh().SaveToStlFile("STLs/Injector.stl");
    
            Voxels voxEngineAssembly = new(voxChamberWithFlange);
            Mesh mshEngineAssembly = voxEngineAssembly.mshAsMesh();
            Mesh mshTranslated = Leap71.ShapeKernel.MeshUtility.mshApplyTransformation(
                mshEngineAssembly,
                (vec) => new(vec.X, vec.Y, vec.Z - dictResults["WallThickness_mm"])
            );
            voxEngineAssembly = new(mshTranslated);
            voxEngineAssembly.BoolAdd(voxInjector);
            voxEngineAssembly.mshAsMesh().SaveToStlFile("STLs/EngineAssembly.stl");
    
            /*
            float fFuelWeightFlow = dictResults["FuelWeightFlow"];
            float fDischargePressure = fChamberPressure + 100.0f;
            float fInletPressure = 14.7f;
            float fFluidDensity = 49f;
            float fPumpEfficiency = 0.6f;
    
            Turbopump oTurbopump = new Turbopump(
                fFuelWeightFlow,
                fDischargePressure,
                fInletPressure,
                fFluidDensity,
                fPumpEfficiency
            );
            
            oTurbopump.Calculate();
            oTurbopump.PrintResults();
            oTurbopump.CreateImpeller();
            Voxels voxImpeller = oTurbopump.voxGetImpeller();
            voxImpeller.mshAsMesh().SaveToStlFile("TurbopumpImpeller.stl");
            */
        }
    }
    catch (Exception e)
    {
        Console.Write(e.ToString());
    }
}
else
{
    ﻿try
    {
        float fThrust = 1000.0f;
        float f_OFRatio = 4.0f;
        float fChamberPressure = 500.0f;
        float fEthanolDensityLB_FT3 = 49.3f;
        float fN2ODensityLB_FT3 = 53.2f;
        float fIsopropanolDensityLB_FT3 = 53.5f;
        float fFuelInjectorCd = 0.7f;
        float fFuelInjectorDeltaPPSI = 100f;
        int iNumberOfFuelHoles = 8;
        float fOxygenInjectorCd = 0.7f;
        float fOxygenInjectorDeltaPPSI = 100f;
        int iNumberOfOxidizerHoles = 8;
        String Ethanol_N2O_CSV = "N2O_C2H5OH.csv";
        String Isopropanol_N2O_CSV = "N2O_C3H8O.csv";
        PropellantDataService propellantDataService = new(Isopropanol_N2O_CSV);
        
        Calculations oCalculations = new Calculations(fThrust, f_OFRatio, fChamberPressure, propellantDataService);
        oCalculations.CalculateCombustionChamber();
        oCalculations.CalculateInjector(
            fIsopropanolDensityLB_FT3,
            fFuelInjectorCd,
            fFuelInjectorDeltaPPSI,
            iNumberOfFuelHoles,
            fN2ODensityLB_FT3,
            fOxygenInjectorCd,
            fOxygenInjectorDeltaPPSI,
            iNumberOfOxidizerHoles
        );
        Dictionary<string, float> dictResults = oCalculations.DictGetCalculations();
    
        CombustionChamber oChamber = new CombustionChamber(dictResults);
        oChamber.CreateCombustionChamber();
        
        float fGrooveThickness = 1.0f;
        float fGrooveDepthMM = 2.0f;
        float fDrillSpaceMM = 20.0f;
        int iNumberOfHoles = 8;
        float fBoltDiameterMM = 6.35f;
        oChamber.CreateFlange(
            fGrooveThickness,
            fGrooveDepthMM,
            fDrillSpaceMM,
            iNumberOfHoles,
            fBoltDiameterMM
        );
        PicoGK.Library.Go(fVoxelSizeMM, oChamber.Run);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
