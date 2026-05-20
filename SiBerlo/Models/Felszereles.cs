namespace SiBerlo.Models
{
    public class Felszereles(long Id, string Tipus, string? Meret, int Allapot, double BeszerzesiAr, double NapiBerletiDij, string? RaktariHely, bool Selejt)
    {
        public long Id { get; init; } = Id;
        public string Tipus { get; init; } = Tipus;
        public string? Meret { get; init; } = Meret;
        public int Allapot { get; init; } = Allapot; // 1-5, 1 legjobb 5 legrosszabb állapot
        public double BeszerzesiAr { get; init; } = BeszerzesiAr; // €
        public double NapiBerletiDij { get; init; } = NapiBerletiDij; // €
        public string? RaktariHely { get; init; } = RaktariHely; // pl. "A1", "B2"
        public bool Selejt { get; init; } = Selejt; 

        public override string ToString()
        {
            string _allapot;
            switch(Allapot)
            {
                case 1: _allapot = "Tökéletes"; break;
                case 2: _allapot = "Jó"; break;
                case 3: _allapot = "Elfogadható"; break;
                case 4: _allapot = "Használt"; break;
                case 5: _allapot = "Rossz"; break;
                default: _allapot = "Ismeretlen állapot"; break;
            }
            return $"Méret: {Meret}; Állapot: {_allapot}; Napi bérleti díj: {NapiBerletiDij}€; ID: {Id}";
        }
    }   
}
