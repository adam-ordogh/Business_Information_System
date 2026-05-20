using System.Web;

namespace SiBerlo.Models
{
    public class Szamla(long Id, long BerlesId, string KiallitasDatuma, string? FizetesiHatarido, bool Fizetve, double Osszeg, string Szamalszam, bool Visszavonva)
    {
        public long Id { get; init; } = Id;
        public long BerlesId { get; init; } = BerlesId; // Foreign key to the rental
        public string KiallitasDatuma { get; init; } = KiallitasDatuma; // ISO 8601 format (YYYY-MM-DD)
        public string? FizetesiHatarido { get; init; } = FizetesiHatarido; // ISO 8601 format (YYYY-MM-DD)
        public bool Fizetve { get; init; } = Fizetve; // true if the invoice is paid
        public double Osszeg { get; init; } = Osszeg; // in HUF
        public string Szamlaszam { get; init; } = Szamalszam;
        public bool Visszavonva { get; set; } = Visszavonva; // true if the invoice is cancelled
    }
}
