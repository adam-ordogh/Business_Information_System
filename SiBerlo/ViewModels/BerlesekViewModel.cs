using SiBerlo.Models;
using SiBerlo.DatabaseAccess;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;
using Microsoft.Windows.Themes;
using System.Diagnostics;
using System.Windows;

namespace SiBerlo.ViewModels
{
    class BerlesekViewModel : ViewModelBase
    {
        private readonly DATABASE _database;

        private long? _selectedCustomerId;
        private string? _selectedEquipmentType;
        private long? _selectedEquipmentId;
        private DateTime? _rentalStartDate;
        private DateTime? _rentalEndDate;
        private double _totalAmountToPay = -1;

        public string FormTitle => SelectedRental == null ? "Új bérlés rögzítése" : "Bérlés módosítása";
        public string TotalAmountToPay => _totalAmountToPay < 0 ? "" : $"{_totalAmountToPay} €";
        public bool CanSelectEquipment => RentalStartDate != null && RentalEndDate != null;

        public ObservableCollection<Ugyfel> Customers { get; } = new ObservableCollection<Ugyfel>();
        public ObservableCollection<Felszereles> Equipment { get; } = new ObservableCollection<Felszereles>();
        public ObservableCollection<Berles> Rentals { get; } = new ObservableCollection<Berles>();
        private Berles _selectedRental;

        public List<string> EquipmentTypes { get; } = new List<string>
        {
            "Síléc",
            "Hódeszka",
            "Sisak",
            "Bakancs",
            "Síbot"
        };

        public Berles SelectedRental
        {
            get => _selectedRental;
            set
            {
                if (SetField(ref _selectedRental, value))
                {
                    if (value != null)
                    {
                        Felszereles felszereles = _database.GetFelszerelesById(value.FelszerelesId);
                        string tipus = felszereles?.Tipus ?? string.Empty;

                        SelectedCustomerId = value.UgyfelId;
                        SelectedEquipmentType = tipus;
                        if (!Equipment.Any(e => e.Id == felszereles.Id))
                        {
                            Equipment.Add(felszereles);
                        }
                        SelectedEquipmentId = value.FelszerelesId;
                        RentalStartDate = DateTime.TryParse(value.KezdoDatum, out DateTime startDate) ? startDate : (DateTime?)null;
                        RentalEndDate = DateTime.TryParse(value.VegDatum, out DateTime endDate) ? endDate : (DateTime?)null;

                    }
                    _totalAmountToPay = CalculateTotalAmountToPay(
                        _rentalStartDate?.ToString("yyyy-MM-dd"),
                        _rentalEndDate?.ToString("yyyy-MM-dd"),
                        _selectedEquipmentId ?? 0
                    );
                    OnFormChanged();
                }
            }
        }

        public long? SelectedCustomerId
        {
            get => _selectedCustomerId;
            set
            {
                if (SetField(ref _selectedCustomerId, value))
                {
                    OnFormChanged();
                }
            }
        }

        public string? SelectedEquipmentType
        {
            get => _selectedEquipmentType;
            set
            {
                if (SetField(ref _selectedEquipmentType, value))
                {
                    if (value != null)
                    {
                        LoadEquipment();
                    }
                    OnFormChanged();
                }
            }
        }

        public long? SelectedEquipmentId
        {
            get => _selectedEquipmentId;
            set
            {
                if (SetField(ref _selectedEquipmentId, value))
                {
                    _totalAmountToPay = CalculateTotalAmountToPay(
                        _rentalStartDate?.ToString("yyyy-MM-dd"),
                        _rentalEndDate?.ToString("yyyy-MM-dd"),
                        _selectedEquipmentId ?? 0
                    );
                    OnFormChanged();
                }
            }
        }

        public DateTime? RentalStartDate
        {
            get => _rentalStartDate;
            set
            {
                if (SetField(ref _rentalStartDate, value))
                {
                    if (SelectedRental != null)
                    {
                        _totalAmountToPay = CalculateTotalAmountToPay(
                            _rentalStartDate?.ToString("yyyy-MM-dd"),
                            _rentalEndDate?.ToString("yyyy-MM-dd"),
                            _selectedEquipmentId ?? 0
                        );
                    }
                    OnFormChanged();
                }
            }
        }

        public DateTime? RentalEndDate
        {
            get => _rentalEndDate;
            set
            {                
                if (SetField(ref _rentalEndDate, value))
                {
                    if (SelectedRental != null)
                    {
                        _totalAmountToPay = CalculateTotalAmountToPay(
                            _rentalStartDate?.ToString("yyyy-MM-dd"),
                            _rentalEndDate?.ToString("yyyy-MM-dd"),
                            _selectedEquipmentId ?? 0
                        );
                    }
                    OnFormChanged();
                }
            }
        }

        public ICommand AddRentalCommand { get; }
        public ICommand UpdateRentalCommand { get; }
        public ICommand DeleteRentalCommand { get; }
        public ICommand CancelEditCommand { get; }

        public BerlesekViewModel(DATABASE database)
        {
            _database = database;
            LoadRentals();
            LoadCustomers();
            AddRentalCommand = new RelayCommand(AddRental, CanAddRental);
            UpdateRentalCommand = new RelayCommand(UpdateRental, CanUpdateRental);
            DeleteRentalCommand = new RelayCommand(DeleteRental, CanDeleteRental);
            CancelEditCommand = new RelayCommand(CancelEdit, CanCancelEdit);
        }
        private void OnFormChanged()
        {
            OnPropertyChanged(nameof(FormTitle));
            OnPropertyChanged(nameof(TotalAmountToPay));
            OnPropertyChanged(nameof(CanSelectEquipment));
            ((RelayCommand)AddRentalCommand).RaiseCanExecuteChanged();
            ((RelayCommand)UpdateRentalCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteRentalCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged();            
        }

        public void ResetFields()
        {
            SelectedRental = null;
            SelectedCustomerId = null;
            SelectedEquipmentType = null;
            SelectedEquipmentId = null;
            RentalEndDate = null;
            RentalStartDate = null;
            OnFormChanged();
        }

        private void LoadRentals()
        {
            Rentals.Clear();
            foreach (var rental in _database.GetAllBerlesek())
            {
                Rentals.Add(rental);
            }
        }
        public void LoadEquipment()
        {
            Equipment.Clear();

            if (_rentalStartDate == null || _rentalEndDate == null)
                return;

            var availableEquipment = _database.GetAvailableFelszerelesByType(_selectedEquipmentType, _rentalStartDate, _rentalEndDate);
            foreach (var item in availableEquipment)
            {
                Equipment.Add(item);
            }
        }
        public void LoadCustomers()
        {
            Customers.Clear();
            var allCustomers = _database.GetAllUgyfelek();
            foreach (var item in allCustomers)
            {
                Customers.Add(item);
            }
        }

        private bool CanAddRental()
        {
            return _selectedRental == null &&
                   _selectedCustomerId != null &&
                   _selectedEquipmentType != null &&
                   _selectedEquipmentId != null &&
                   _rentalEndDate != null &&
                   _rentalStartDate != null &&
                   (_rentalStartDate <= _rentalEndDate);
        }
        private void AddRental()
        {
            if(CanAddRental())
            {
                var newRental = new Berles(
                    Id: 0,
                    UgyfelId: _selectedCustomerId ?? 0,
                    FelszerelesId: _selectedEquipmentId ?? 0,
                    KezdoDatum: _rentalStartDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    VegDatum: _rentalEndDate?.ToString("yyyy-MM-dd") ?? string.Empty
                );
                long rentalId = _database.InsertBerles(newRental);

                double totalAmount = CalculateTotalAmountToPay(newRental.KezdoDatum, newRental.VegDatum, newRental.FelszerelesId);
                string invoiceNumber = _database.GenerateNextSzamlaszam();
                string dueDate = DateTime.Now.AddDays(14).ToString("yyyy-MM-dd");

                var invoice = new Szamla(
                    Id: 0,
                    BerlesId: rentalId,
                    KiallitasDatuma: DateTime.Now.ToString("yyyy-MM-dd"),
                    FizetesiHatarido: DateTime.Now.ToString("yyyy-MM-dd"), /*dueDate,*/
                    Fizetve: true,
                    Osszeg: totalAmount,
                    Szamalszam: invoiceNumber,
                    Visszavonva: false
                );
                _database.InsertSzamla(invoice);

                ResetFields();
                LoadRentals(); 
            }
        }

        private bool CanUpdateRental()
        {            
            return _selectedRental != null &&
                   _selectedCustomerId != null &&
                   _selectedEquipmentType != null &&
                   _selectedEquipmentId != null &&
                   _rentalEndDate != null &&
                   _rentalStartDate != null &&
                   (_rentalStartDate <= _rentalEndDate);
        }
        private void UpdateRental()
        {
            if (CanUpdateRental())
            {
                var newRental = new Berles(
                    Id: SelectedRental.Id,
                    UgyfelId: SelectedCustomerId ?? 0,
                    FelszerelesId: SelectedEquipmentId ?? 0,
                    KezdoDatum: RentalStartDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    VegDatum: RentalEndDate?.ToString("yyyy-MM-dd") ?? string.Empty
                );
                _database.UpdateBerles(newRental);

                var oldInvoice = _database.GetAllSzamlak()
                    .FirstOrDefault(s => s.BerlesId == SelectedRental.Id && !s.Visszavonva);
                if (oldInvoice != null)
                {
                    oldInvoice.Visszavonva = true;
                    _database.UpdateSzamla(oldInvoice);
                }

                double totalAmount = CalculateTotalAmountToPay(SelectedRental.KezdoDatum, SelectedRental.VegDatum, SelectedRental.FelszerelesId);
                string invoiceNumber = _database.GenerateNextSzamlaszam();
                string dueDate = DateTime.Now.AddDays(14).ToString("yyyy-MM-dd");

                var newInvoice = new Szamla(
                    Id: 0,
                    BerlesId: SelectedRental.Id,
                    KiallitasDatuma: DateTime.Now.ToString("yyyy-MM-dd"),
                    FizetesiHatarido: dueDate,
                    Fizetve: true,
                    Osszeg: totalAmount,
                    Szamalszam: invoiceNumber,
                    Visszavonva: false
                );
                _database.InsertSzamla(newInvoice);


                ResetFields();
                LoadRentals();
            }
        }

        private bool CanDeleteRental()
        {
            return SelectedRental != null;
        }
        private void DeleteRental()
        {
            if(CanDeleteRental())
            {
                _database.DeleteBerles(SelectedRental.Id);
                ResetFields();
                LoadRentals();
            }
        }

        private bool CanCancelEdit()
        {
            return _selectedRental != null ||
                   _selectedCustomerId != null ||
                   _selectedEquipmentType != null ||
                   _selectedEquipmentId != null ||
                   _rentalEndDate != null ||
                   _rentalStartDate != null;
        }

        private void CancelEdit()
        {
            ResetFields(); 

        }

        public double CalculateTotalAmountToPay(string startDate, string endDate, long equipmentId)
        {
            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate) || equipmentId == 0)
            {
                return -1;
            }

            try
            {
                DateTime start = DateTime.Parse(startDate);
                DateTime end = DateTime.Parse(endDate);

                if (end < start)
                {
                    return -1;
                }

                int rentalDays = (end - start).Days + 1;
                Felszereles equipment = _database.GetFelszerelesById(equipmentId);

                if (equipment == null)
                {
                    return -1;
                }

                double totalPrice = equipment.NapiBerletiDij * rentalDays;
                Debug.WriteLine($"Total rental price: {totalPrice}");
                return totalPrice;
            }
            catch (FormatException)
            {
                return -1;
            }
        }
    }
}
