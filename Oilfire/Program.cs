using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PicoGK;
using RocketEngineBuilder;

try
{
    using (PicoGK.Library oLibrary = new(0.5f))
    {
    
        float fThrust = 1000.0f;
        float f_OFRatio = 4.0f;
        float fChamberPressure = 750.0f;
        String Ethanol_N2O_CSV = "N2O_C2H5OH.csv";
        String Isopropanol_N2O_CSV = "N2O_C3H8O.csv";
        PropellantDataService propellantDataService = new(Isopropanol_N2O_CSV);

        CombustionChamber oChamber = new CombustionChamber(fThrust, f_OFRatio, fChamberPressure, propellantDataService);
        oChamber.Calculate();
        oChamber.CreateCombustionChamber();
        Voxels voxCombustionChamber = oChamber.voxGetCombustionChamber();
        voxCombustionChamber.mshAsMesh().SaveToStlFile("CombustionChamber.stl");
        /*
        WaterTurbopump oTurbopump = new WaterTurbopump();
        oTurbopump.Calculate();
        oTurbopump.CreateWaterTurbopumpShell();
        Voxels voxShell = oTurbopump.voxGetShell();
        voxShell.mshAsMesh().SaveToStlFile("WaterTurbopumpShell.stl");
        */
    }
}
catch (Exception e)
{
    Console.Write(e.ToString());
}
