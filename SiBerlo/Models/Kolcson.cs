namespace SiBerlo.Models
{
    public class Kolcson(long Id, long KiadoAlkalmazottId, long FelvevoAlkalmazottId, double Osszeg, string KezdoDatum, string? VisszafizetesDatuma, string? Leiras)
    {
        public long Id { get; init; } = Id; // Unique identifier for the loan
        public long KiadoAlkalmazottId { get; init; } = KiadoAlkalmazottId; // ID of the employee who issued the loan
        public long FelvevoAlkalmazottId { get; init; } = FelvevoAlkalmazottId; // ID of the employee who received the loan
        public double Osszeg { get; init; } = Osszeg; // Amount of the loan in HUF
        public string KezdoDatum { get; init; } = KezdoDatum; // Start date in ISO 8601 format (YYYY-MM-DD)
        public string? VisszafizetesDatuma { get; init; } = VisszafizetesDatuma; // Repayment date in ISO 8601 format (YYYY-MM-DD)
        public string? Leiras { get; init; } = Leiras; // Optional description of the loan
    }
}
