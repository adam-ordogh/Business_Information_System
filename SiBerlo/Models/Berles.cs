namespace SiBerlo.Models
{
    public class Berles(long Id, long UgyfelId, long FelszerelesId, string KezdoDatum, string VegDatum)
    {
        public long Id { get; init; } = Id;
        public long UgyfelId { get; init; } = UgyfelId;
        public long FelszerelesId { get; init; } = FelszerelesId;
        public string KezdoDatum { get; init; } = KezdoDatum; // ISO 8601 format (YYYY-MM-DD)
        public string VegDatum { get; init; } = VegDatum; // ISO 8601 format (YYYY-MM-DD)
    }
}
