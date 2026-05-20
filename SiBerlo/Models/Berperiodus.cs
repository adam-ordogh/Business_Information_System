namespace SiBerlo.Models
{
    public class Berperiodus(long Id, string Nev, string KezdoDatum, string VegDatum, bool Zarolt)
    {
        public long Id { get; init; } = Id; // Unique identifier for the period
        public string Nev { get; init; } = Nev; // Name of the period, e.g. "2023 Q1"
        public string KezdoDatum { get; init; } = KezdoDatum; // Start date in ISO 8601 format (YYYY-MM-DD)
        public string VegDatum { get; init; } = VegDatum; // End date in ISO 8601 format (YYYY-MM-DD)
        public bool Zarolt { get; init; } = Zarolt; // True if the period is closed, false otherwise
    }
}
