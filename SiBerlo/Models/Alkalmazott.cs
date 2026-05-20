namespace SiBerlo.Models
{
    public class Alkalmazott(long Id, string Nev, string Beosztas, string BelepesDatuma, double Alapber, double JutalekSzazalek, bool Aktiv)
    {
        public long Id { get; init; } = Id;
        public string Nev { get; init; } = Nev;
        public string Beosztas { get; init; } = Beosztas; // e.g. "Admin", "Raktaros", "Pultos"
        public string BelepesDatuma { get; init; } = BelepesDatuma; // ISO 8601 format (YYYY-MM-DD)
        public double Alapber { get; init; } = Alapber; // in HUF
        public double JutalekSzazalek { get; init; } = JutalekSzazalek; // percentage of sales
        public bool Aktiv { get; init; } = Aktiv; // true if the employee is currently active

        public override string ToString()
        {
            return $"{Nev} - {Beosztas ?? "N/A"} - ID: {Id}";
        }
    }
}
