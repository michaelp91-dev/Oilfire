namespace Oilfire
{
    /// <summary>
    /// Represents a single row of data from the propellant performance CSV file.
    /// All properties are floats to match the required data types.
    /// </summary>
    public class PropellantData
    {
        public float OfRatio { get; set; }
        public float ChamberTempK { get; set; }
        public float GammaChamber { get; set; }
        public float GammaThroat { get; set; }
        public float MolarMassGmol { get; set; }
        public float VacIspS { get; set; }
        public float CstarMS { get; set; }
    }
}