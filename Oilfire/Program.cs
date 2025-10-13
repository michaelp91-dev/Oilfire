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
        float fThrust = 100.0f;
        float f_OFRatio = 4.5f;
        float fChamberPressure = 300.0f;
        PropellantDataService propellantDataService = new("N2O_C2H5OH.csv");

        CombustionChamber oChamber = new CombustionChamber(fThrust, f_OFRatio, fChamberPressure, propellantDataService);
        oChamber.Calculate();
        oChamber.CreateCombustionChamber();
        Voxels voxCombustionChamber = oChamber.voxGetCombustionChamber();
        voxCombustionChamber.mshAsMesh().SaveToStlFile("CombustionChamber.stl");
    }
}
catch (Exception e)
{
    Console.Write(e.ToString());
}
