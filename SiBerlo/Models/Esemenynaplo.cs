namespace SiBerlo.Models
{
    public class Esemenynaplo(long Id, long AlkalmazottId, string DatumIdo, string Tipus, int EntitasId, string EntitasTipus, string Leiras)
    {
        public long Id { get; init; } = Id; // Unique identifier for the event log entry
        public long AlkalmazottId { get; init; } = AlkalmazottId; // ID of the employee associated with the event
        public string DatumIdo { get; init; } = DatumIdo; // Date and time of the event in ISO 8601 format (YYYY-MM-DD HH:MM:SS)
        public string Tipus { get; init; } = Tipus; // Type of the event (e.g., "Beérkezés", "Selejtezés", "Áthelyezés")
        public int EntitasId { get; init; } = EntitasId; // ID of the entity related to the event
        public string EntitasTipus { get; init; } = EntitasTipus; // Type of the entity (e.g., "Felszereles", "Alkalmazott")
        public string Leiras { get; init; } = Leiras; // Description of the event, providing additional context or details
    }
}
