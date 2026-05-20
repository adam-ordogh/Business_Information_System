namespace SiBerlo.Models
{
    public class Ber(long Id, long AlkalmazottId, long PeriodusId, double Alapber, double TuloraBer, double HetvegiBonusz, double Jutalek, double EgyebPotlek, double Eloleg, double Szocho, double Szja, double NettoBer)
    {
        public long Id { get; init; } = Id;
        public long AlkalmazottId { get; init; } = AlkalmazottId; // Foreign key to Alkalmazottak table
        public long PeriodusId { get; init; } = PeriodusId; // Foreign key to BerPeriodusok table
        public double Alapber { get; init; } = Alapber; // Base salary in HUF
        public double TuloraBer { get; init; } = TuloraBer; // Overtime pay in HUF
        public double HetvegiBonusz { get; init; } = HetvegiBonusz; // Weekend bonus in HUF
        public double Jutalek { get; init; } = Jutalek; // Commission in HUF
        public double EgyebPotlek { get; init; } = EgyebPotlek; // Other allowances in HUF
        public double Eloleg { get; init; } = Eloleg; // Advance payment in HUF
        public double Szocho { get; init; } = Szocho; // Social security contribution in HUF
        public double Szja { get; init; } = Szja; // Personal income tax in HUF
        public double NettoBer { get; init; } = NettoBer; // Net salary in HUF
    }
}
