namespace SiBerlo.Models
{
    public class Keszletmozgas(long Id, long FelszerelesId, long AlkalmazottId, string Datum, string Tipus, int Mennyiseg, string? Megjegyzes)
    {
        public long Id { get; init; } = Id;
        public long FelszerelesId { get; init; } = FelszerelesId; // Foreign key to Felszerelesek table
        public long AlkalmazottId { get; init; } = AlkalmazottId; // Foreign key to Alkalmazottak table
        public string Datum { get; init; } = Datum; // ISO 8601 format (YYYY-MM-DD HH:MM:SS)
        public string Tipus { get; init; } = Tipus; // e.g. "Beérkezés", "Selejtezés", "Áthelyezés"
        public int Mennyiseg { get; init; } = Mennyiseg; // Quantity of the equipment moved
        public string? Megjegyzes { get; init; } = Megjegyzes; // Optional notes about the movement
    }
}
