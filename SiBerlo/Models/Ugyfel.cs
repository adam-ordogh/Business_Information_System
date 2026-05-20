namespace SiBerlo.Models
{
    public class Ugyfel(long Id, string Nev, string? Email, string? Telefon, string? Cim, string RegisztracioDatuma, int Kedvezmeny)
    {
        public long Id { get; init; } = Id;
        public string Nev { get; init; } = Nev;
        public string? Email { get; init; } = Email;
        public string? Telefon { get; init; } = Telefon;
        public string? Cim { get; init; } = Cim;
        public string RegisztracioDatuma { get; init; } = RegisztracioDatuma; // ISO 8601 format (YYYY-MM-DD)
        public int Kedvezmeny { get; init; } = Kedvezmeny; // 0-100, ahol 0 nincs kedvezmény, 100 teljes kedvezmény

        public override string ToString()
        {
            return $"{Nev}; Cím: {Cim ?? "N/A"}; Telefon: {Telefon ?? "N/A"}; Email: {Email ?? "N/A"}; ID: {Id}";
        }
    }
}
