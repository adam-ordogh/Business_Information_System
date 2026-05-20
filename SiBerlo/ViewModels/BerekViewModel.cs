using SiBerlo.Models;
using SiBerlo.DatabaseAccess;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;
using System.Windows;
using SiBerlo.Views;
using System.Linq;
using SiBerlo.Services;

namespace SiBerlo.ViewModels
{
    class BerekViewModel : ViewModelBase
    {
        private readonly DATABASE _database;
        private readonly WageService _wageService;
        private string? _selectedBerperiodusName;
        private BerDisplay? _selectedBer;

        public ICommand EditWageCommand { get; }
        public ICommand SaveChangesCommand { get; }

        public string FormTitle => _selectedBerperiodus == null
            ? "Válassz bérperiódust"
            : $"Bérek {_selectedBerperiodusName} periódusban";

        public ObservableCollection<Berperiodus> Berperiodusok { get; } = new ObservableCollection<Berperiodus>();
        public ObservableCollection<BerDisplay> BerekPerBerperiodusok { get; } = new ObservableCollection<BerDisplay>();

        private Berperiodus? _selectedBerperiodus;

        public Berperiodus? SelectedBerperiodus
        {
            get => _selectedBerperiodus;
            set
            {
                if (SetField(ref _selectedBerperiodus, value))
                {
                    LoadBerekPerBerperiodusok();
                    if (value != null)
                    {
                        SelectedBerperiodusName = value.Zarolt ? $"a {value.Nev}" : "az aktív";
                    }
                }
            }
        }

        public BerDisplay? SelectedBer
        {
            get => _selectedBer;
            set => SetField(ref _selectedBer, value);
        }

        public string? SelectedBerperiodusName
        {
            get => _selectedBerperiodusName;
            set
            {
                if (SetField(ref _selectedBerperiodusName, value))
                {
                    OnPropertyChanged(nameof(FormTitle));
                }
            }
        }

        public BerekViewModel(DATABASE database)
        {
            _database = database;
            _wageService = new WageService(_database);
            EditWageCommand = new RelayCommand<BerDisplay>(EditWage);
            LoadBerperiodusok();
        }

        private void EditWage(BerDisplay berDisplay)
        {
            if (berDisplay == null) return;

            var ber = _database.GetBerekById(berDisplay.Id);

            var editWindow = new EditWageFieldsWindow(ber.Jutalek, ber.EgyebPotlek, ber.Eloleg);

            if (editWindow.ShowDialog() == true)
            {
                _database.UpdateBerek(new Ber(
                    Id: ber.Id,
                    AlkalmazottId: ber.AlkalmazottId,
                    PeriodusId: ber.PeriodusId,
                    Alapber: ber.Alapber,
                    TuloraBer: ber.TuloraBer,
                    HetvegiBonusz: ber.HetvegiBonusz,
                    Jutalek: editWindow.Jutalek,
                    EgyebPotlek: editWindow.EgyebPotlek,
                    Eloleg: editWindow.Eloleg,
                    Szocho: ber.Szocho,
                    Szja: ber.Szja,
                    NettoBer: ber.NettoBer
                ));

                var employee = _database.GetAlkalmazottById(ber.AlkalmazottId);

                _wageService.UpdateWageForEmployee(employee);

                LoadBerekPerBerperiodusok();
            }
        }

        public void LoadBerperiodusok()
        {
            Berperiodusok.Clear();
            var berperiodusok = _database.GetAllBerperiodusok();
            foreach (var berperiodus in berperiodusok)
            {
                Berperiodusok.Add(berperiodus);
            }
        }

        public void LoadBerekPerBerperiodusok()
        {
            if (SelectedBerperiodus == null) return;

            var allBerek = _database.GetAllBerek();
            var allEmployees = _database.GetAllAlkalmazottak();
            var allPeriods = _database.GetAllBerperiodusok();

            BerekPerBerperiodusok.Clear();
            foreach (var ber in allBerek.Where(b => b.PeriodusId == SelectedBerperiodus.Id))
            {
                var employee = allEmployees.FirstOrDefault(e => e.Id == ber.AlkalmazottId);
                var period = allPeriods.FirstOrDefault(p => p.Id == ber.PeriodusId);

                BerekPerBerperiodusok.Add(new BerDisplay
                {
                    Id = ber.Id,
                    AlkalmazottId = ber.AlkalmazottId,
                    AlkalmazottNev = employee?.Nev ?? "Ismeretlen",
                    PeriodusId = ber.PeriodusId,
                    PeriodusNev = period?.Nev ?? "Ismeretlen",
                    Alapber = ber.Alapber,
                    TuloraBer = ber.TuloraBer,
                    HetvegiBonusz = ber.HetvegiBonusz,
                    Jutalek = ber.Jutalek,
                    EgyebPotlek = ber.EgyebPotlek,
                    Eloleg = ber.Eloleg,
                    Szocho = ber.Szocho,
                    Szja = ber.Szja,
                    NettoBer = ber.NettoBer
                });
            }
        }
    }

    public class BerDisplay
    {
        public long Id { get; set; }
        public long AlkalmazottId { get; set; }
        public string AlkalmazottNev { get; set; }
        public long PeriodusId { get; set; }
        public string PeriodusNev { get; set; }
        public double Alapber { get; set; }
        public double TuloraBer { get; set; }
        public double HetvegiBonusz { get; set; }
        public double Jutalek { get; set; }
        public double EgyebPotlek { get; set; }
        public double Eloleg { get; set; }
        public double Szocho { get; set; }
        public double Szja { get; set; }
        public double NettoBer { get; set; }
    }
}