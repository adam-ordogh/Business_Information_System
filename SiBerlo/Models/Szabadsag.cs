namespace SiBerlo.Models
{
    public class Szabadsag(long Id, long AlkalmazottId, string KezdoDatum, string VegDatum, string Tipus, bool Jovahagyva)
    {
        private static readonly HashSet<string> ValidTipusValues = new()
        {
            "Fizetett", "Fizetetlen", "Betegszabadság"
        };

        public long Id { get; init; } = Id;
        public long AlkalmazottId { get; init; } = AlkalmazottId; // Foreign key to Alkalmazottak table
        public string KezdoDatum { get; init; } = KezdoDatum; // ISO 8601 format (YYYY-MM-DD)
        public string VegDatum { get; init; } = VegDatum; // ISO 8601 format (YYYY-MM-DD)

        private string _tipus = Tipus;//public string Tipus { get; init; } = _tipus; // e.g. "Szabadsag", "Betegszabadsag", "Szuletesnap"
        public string Tipus //Check if the value is one of the valid types
        {
            get => _tipus;
            init
            {
                if (!ValidTipusValues.Contains(value))
                    throw new ArgumentException("Invalid Tipus value");
                _tipus = value;
            }
        }
        public bool Jovahagyva { get; init; } = Jovahagyva; // true if the leave is approved, false otherwise

    }
}
