// JelentesekViewModel.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using SiBerlo.DatabaseAccess;
using SiBerlo.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using Microsoft.Win32;
using QuestPdfColors = QuestPDF.Helpers.Colors;

namespace SiBerlo.ViewModels
{
    class JelentesekViewModel : ViewModelBase
    {
        private readonly DATABASE _db;

        public SeriesCollection HavibevetelSorozat { get; private set; }
        public string[] HavibevetelCimkek { get; private set; }

        public SeriesCollection BerlesStatisztikaSorozat { get; private set; }

        // Kimutatási adatok
        public List<BevetelKimutatas> Havibevetelek { get; private set; }
        public List<BerlesStatisztika> BerlesStatisztikak { get; private set; }
        public List<FelszerelesHasznalat> FelszerelesHasznalatok { get; private set; }
        public List<AlkalmazottTeljesitmeny> AlkalmazottTeljesitmenyek { get; private set; }

        // Parancsok
        public ICommand FrissitesParancs { get; }
        public ICommand ExportPdfParancs { get; }

        public JelentesekViewModel(DATABASE db)
        {
            _db = db;
            FrissitesParancs = new RelayCommand(FrissitKimutasok);
            ExportPdfParancs = new RelayCommand(ExportalPdfKent);

            FrissitKimutasok();
        }

        private void FrissitKimutasok()
        {
            // 1. Havi bevétel kimutatás
            Havibevetelek = SzamlakbolHaviBevetel();

            // 2. Bérlési statisztikák
            BerlesStatisztikak = BerlesekbolStatisztika();
            foreach (var stat in BerlesStatisztikak)
                System.Diagnostics.Debug.WriteLine($"{stat.FelszerelesTipus}: {stat.OsszBevetel}");

            // 3. Felszerelés használat
            FelszerelesHasznalatok = FelszerelesHasznalatSzamol();

            // 4. Alkalmazott teljesítmény
            AlkalmazottTeljesitmenyek = AlkalmazottTeljesitmenySzamol();

            // Havi bevétel diagram adatok előkészítése
            HavibevetelSorozat = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Bevétel",
                    Values = new ChartValues<double>(Havibevetelek.Select(x => x.Osszeg))
                }
            };
            HavibevetelCimkek = Havibevetelek.Select(x => $"{x.Ev}.{x.Honap:D2}").ToArray();
            OnPropertyChanged(nameof(HavibevetelSorozat));
            OnPropertyChanged(nameof(HavibevetelCimkek));

            // Bérlési statisztika pie chart adatok
            BerlesStatisztikaSorozat = new SeriesCollection();

            foreach (var stat in BerlesStatisztikak.Where(x => x.OsszBevetel > 0))
            {
                BerlesStatisztikaSorozat.Add(new PieSeries
                {
                    Title = stat.FelszerelesTipus,
                    Values = new ChartValues<double> { stat.OsszBevetel },
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y:N0} €"
                });

                if (!BerlesStatisztikaSorozat.Any())
                {
                    BerlesStatisztikaSorozat.Add(new PieSeries
                    {
                        Title = "Nincs elérhető adat",
                        Values = new ChartValues<double> { 1 },
                        DataLabels = false,
                        Fill = Brushes.LightGray
                    });
                }
            }

            OnPropertyChanged(nameof(BerlesStatisztikaSorozat));

            OnPropertyChanged(nameof(Havibevetelek));
            OnPropertyChanged(nameof(BerlesStatisztikak));
            OnPropertyChanged(nameof(FelszerelesHasznalatok));
            OnPropertyChanged(nameof(AlkalmazottTeljesitmenyek));

            System.Diagnostics.Debug.WriteLine($"Pie sorozat darabszám: {BerlesStatisztikak.Count}");

        }

        private List<BevetelKimutatas> SzamlakbolHaviBevetel()
        {
            var szamlak = _db.GetAllSzamlak();
            return szamlak
                .Where(s => s.Fizetve && !s.Visszavonva)
                .GroupBy(s => new {
                    Ev = DateTime.Parse(s.KiallitasDatuma).Year,
                    Honap = DateTime.Parse(s.KiallitasDatuma).Month
                })
                .Select(g => new BevetelKimutatas
                {
                    Ev = g.Key.Ev,
                    Honap = g.Key.Honap,
                    Osszeg = g.Sum(s => s.Osszeg),
                    Darab = g.Count()
                })
                .OrderByDescending(x => x.Ev)
                .ThenByDescending(x => x.Honap)
                .ToList();
        }

        private List<BerlesStatisztika> BerlesekbolStatisztika()
        {
            var berlesek = _db.GetAllBerlesek();
            var felszerelesek = _db.GetAllFelszerelesek();
            var ugyfelek = _db.GetAllUgyfelek();

            return berlesek
                .Join(felszerelesek, b => b.FelszerelesId, f => f.Id, (b, f) => new { Berles = b, Felszereles = f })
                .Join(ugyfelek, bf => bf.Berles.UgyfelId, u => u.Id, (bf, u) => new { bf.Berles, bf.Felszereles, Ugyfel = u })
                .GroupBy(x => x.Felszereles.Tipus)
                .Select(g => new BerlesStatisztika
                {
                    FelszerelesTipus = g.Key,
                    AtlagosBérletiIdo = g.Average(x =>
                        (DateTime.Parse(x.Berles.VegDatum) - DateTime.Parse(x.Berles.KezdoDatum)).TotalDays),
                    OsszBevetel = g.Sum(x =>
                        (DateTime.Parse(x.Berles.VegDatum) - DateTime.Parse(x.Berles.KezdoDatum)).TotalDays *
                        x.Felszereles.NapiBerletiDij),
                    UgyfelekSzama = g.Select(x => x.Ugyfel.Id).Distinct().Count()
                })
                .OrderByDescending(x => x.OsszBevetel)
                .ToList();
        }

        private List<FelszerelesHasznalat> FelszerelesHasznalatSzamol()
        {
            var berlesek = _db.GetAllBerlesek();
            var felszerelesek = _db.GetAllFelszerelesek();

            return felszerelesek
                .GroupJoin(berlesek, f => f.Id, b => b.FelszerelesId, (f, b) => new { Felszereles = f, Berlesek = b })
                .Select(x => new FelszerelesHasznalat
                {
                    FelszerelesId = x.Felszereles.Id,
                    Tipus = x.Felszereles.Tipus,
                    Meret = x.Felszereles.Meret,
                    BerlesekSzama = x.Berlesek.Count(),
                    OsszesHasznaltNap = x.Berlesek.Sum(b =>
                        (DateTime.Parse(b.VegDatum) - DateTime.Parse(b.KezdoDatum)).TotalDays),
                    Allapot = x.Felszereles.Allapot
                })
                .OrderByDescending(x => x.BerlesekSzama)
                .ToList();
        }

        private List<AlkalmazottTeljesitmeny> AlkalmazottTeljesitmenySzamol()
        {
            var alkalmazottak = _db.GetAllAlkalmazottak();
            var bejelenkezesek = _db.GetAllBejelenkezesek();
            var berek = _db.GetAllBerek();

            return alkalmazottak
                .Select(a => new AlkalmazottTeljesitmeny
                {
                    AlkalmazottId = a.Id,
                    Nev = a.Nev,
                    Beosztas = a.Beosztas,
                    AtlagosNapiDolgozottOra = bejelenkezesek
                        .Where(b => b.AlkalmazottId == a.Id &&
                                   !string.IsNullOrEmpty(b.Erkezes) &&
                                   !string.IsNullOrEmpty(b.Tavozas) &&
                                   DateTime.TryParse(b.Erkezes, out var erkezes) &&
                                   DateTime.TryParse(b.Tavozas, out var tavozas))
                        .Select(b => (DateTime.Parse(b.Tavozas) - DateTime.Parse(b.Erkezes)).TotalHours)
                        .DefaultIfEmpty(0) 
                        .Average(),
                    OsszBér = berek.Where(b => b.AlkalmazottId == a.Id).Sum(b => b.NettoBer),
                    Aktiv = a.Aktiv
                })
                .OrderByDescending(x => x.OsszBér)
                .ToList();
        }

        private void ExportalPdfKent()
        {
            // Mentés párbeszédablak megnyitása
            var dlg = new SaveFileDialog
            {
                FileName = "Jelentes.pdf",
                DefaultExt = ".pdf",
                Filter = "PDF dokumentum (*.pdf)|*.pdf"
            };

            if (dlg.ShowDialog() != true)
                return;

            var filePath = dlg.FileName;

            // PDF dokumentum létrehozása QuestPDF-vel
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);

                    page.Header()
                        .Text("SiBerlo Jelentés")
                        .SemiBold().FontSize(20).FontColor(QuestPdfColors.Blue.Medium);

                    page.Content()
                        .Column(column =>
                        {
                            // Havi bevétel kimutatás
                            column.Item().Text("Havi bevétel kimutatás").Bold().FontSize(16).Underline();
                            column.Item().Table(table =>
                            {
                                // Fejléc
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Év");
                                    header.Cell().Element(CellStyle).Text("Hónap");
                                    header.Cell().Element(CellStyle).Text("Összeg (€)");
                                    header.Cell().Element(CellStyle).Text("Darab");
                                });

                                foreach (var item in Havibevetelek)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Ev.ToString());
                                    table.Cell().Element(CellStyle).Text(item.Honap.ToString("D2"));
                                    table.Cell().Element(CellStyle).Text($"{item.Osszeg:N0}");
                                    table.Cell().Element(CellStyle).Text(item.Darab.ToString());
                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.Padding(5).BorderBottom(1).BorderColor(QuestPdfColors.Grey.Lighten2);
                                }
                            });

                            column.Item().Text("").LineHeight(5);

                            // Bérlési statisztikák
                            column.Item().Text("Bérlési statisztikák").Bold().FontSize(16).Underline();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Típus");
                                    header.Cell().Element(CellStyle).Text("Átlagos bérleti idő (nap)");
                                    header.Cell().Element(CellStyle).Text("Összbevétel (€)");
                                    header.Cell().Element(CellStyle).Text("Ügyfelek");
                                });

                                foreach (var stat in BerlesStatisztikak)
                                {
                                    table.Cell().Element(CellStyle).Text(stat.FelszerelesTipus);
                                    table.Cell().Element(CellStyle).Text($"{stat.AtlagosBérletiIdo:N1}");
                                    table.Cell().Element(CellStyle).Text($"{stat.OsszBevetel:N0}");
                                    table.Cell().Element(CellStyle).Text(stat.UgyfelekSzama.ToString());
                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.Padding(5).BorderBottom(1).BorderColor(QuestPdfColors.Grey.Lighten2);
                                }
                            });

                            column.Item().Text("").LineHeight(5);

                            // Felszerelés használat
                            column.Item().Text("Felszerelés használat").Bold().FontSize(16).Underline();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Típus");
                                    header.Cell().Element(CellStyle).Text("Méret");
                                    header.Cell().Element(CellStyle).Text("Bérlések");
                                    header.Cell().Element(CellStyle).Text("Használt napok");
                                    header.Cell().Element(CellStyle).Text("Állapot");
                                });

                                foreach (var item in FelszerelesHasznalatok)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Tipus);
                                    table.Cell().Element(CellStyle).Text(item.Meret);
                                    table.Cell().Element(CellStyle).Text(item.BerlesekSzama.ToString());
                                    table.Cell().Element(CellStyle).Text($"{item.OsszesHasznaltNap:N1}");
                                    table.Cell().Element(CellStyle).Text(item.Allapot.ToString());
                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.Padding(5).BorderBottom(1).BorderColor(QuestPdfColors.Grey.Lighten2);
                                }
                            });

                            column.Item().Text("").LineHeight(5);

                            // Alkalmazott teljesítmény
                            column.Item().Text("Alkalmazott teljesítmény").Bold().FontSize(16).Underline();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Név");
                                    header.Cell().Element(CellStyle).Text("Beosztás");
                                    header.Cell().Element(CellStyle).Text("Átlagos óra/nap");
                                    header.Cell().Element(CellStyle).Text("Összbér (€)");
                                    header.Cell().Element(CellStyle).Text("Aktív");
                                });

                                foreach (var item in AlkalmazottTeljesitmenyek)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Nev);
                                    table.Cell().Element(CellStyle).Text(item.Beosztas);
                                    table.Cell().Element(CellStyle).Text($"{item.AtlagosNapiDolgozottOra:N1}");
                                    table.Cell().Element(CellStyle).Text($"{item.OsszBér:N0}");
                                    table.Cell().Element(CellStyle).Text(item.Aktiv ? "Igen" : "Nem");
                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.Padding(5).BorderBottom(1).BorderColor(QuestPdfColors.Grey.Lighten2);
                                }
                            });
                        });

                    page.Footer()
                    .AlignCenter()
                    .Text(txt =>
                    {
                        txt.Span("Generálva: ").SemiBold().FontSize(10).FontColor(QuestPdfColors.Grey.Darken1);
                        txt.Span(DateTime.Now.ToString("yyyy.MM.dd HH:mm"));
                    });
                });
            });

            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            document.GeneratePdf(fs);
        }
    }

    public class BevetelKimutatas
    {
        public int Ev { get; set; }
        public int Honap { get; set; }
        public double Osszeg { get; set; }
        public int Darab { get; set; }
    }

    public class BerlesStatisztika
    {
        public string FelszerelesTipus { get; set; }
        public double AtlagosBérletiIdo { get; set; }
        public double OsszBevetel { get; set; }
        public int UgyfelekSzama { get; set; }
    }

    public class FelszerelesHasznalat
    {
        public long FelszerelesId { get; set; }
        public string Tipus { get; set; }
        public string Meret { get; set; }
        public int BerlesekSzama { get; set; }
        public double OsszesHasznaltNap { get; set; }
        public int Allapot { get; set; }
    }

    public class AlkalmazottTeljesitmeny
    {
        public long AlkalmazottId { get; set; }
        public string Nev { get; set; }
        public string Beosztas { get; set; }
        public double AtlagosNapiDolgozottOra { get; set; }
        public double OsszBér { get; set; }
        public bool Aktiv { get; set; }
    }
}