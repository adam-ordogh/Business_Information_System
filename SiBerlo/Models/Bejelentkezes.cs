namespace SiBerlo.Models
{
    public class Bejelentkezes(long Id, long AlkalmazottId, string Erkezes, string? Tavozas)
    {
        public long Id { get; init; } = Id;
        public long AlkalmazottId { get; init; } = AlkalmazottId; // Foreign key to Alkalmazottak table
        public string Erkezes { get; init; } = Erkezes; // ISO 8601 format (YYYY-MM-DD HH:MM:SS)
        public string? Tavozas { get; init; } = Tavozas; // ISO 8601 format (YYYY-MM-DD HH:MM:SS), can be null if not yet left
    }
}
