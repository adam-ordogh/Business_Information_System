using SiBerlo.Models;
using SiBerlo.DatabaseAccess;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;

namespace SiBerlo.ViewModels
{
    class FelszerelesViewModel : ViewModelBase
    {
        private DATABASE _database;

        private string? _newEquipmentType;
        private string? _newEquipmentSize;
        private int? _newEquipmentState;
        private double? _newEquipmentPurchasePrice;
        private double? _newEquipmentRentalPrice;
        private string? _newEquipmentStoragePlace;
        private bool _newEquipmentScrap;
        private Felszereles _selectedEquipment;

        public ObservableCollection<Felszereles> Equipment { get; } = new ObservableCollection<Felszereles>();

        public string FormTitle => SelectedEquipment == null ? "Új felszerelés felvétele" : "Felszerelés módosítása";

        public List<string> EquipmentTypes { get; } = new List<string>
        {
            "Síléc",
            "Hódeszka",
            "Sisak",
            "Bakancs",
            "Síbot"
        };

        public List<string> EquipmentSizes { get; } = new List<string>
        {
            "XS",
            "S",
            "M",
            "L",
            "XL"
        };

        public Dictionary<int, string> EquipmentStates { get; } = new Dictionary<int, string>
        {
            { 1, "Tökéletes (1)" },
            { 2, "Jó (2)" },
            { 3, "Elfogadható (3)" },
            { 4, "Használt (4)" },
            { 5, "Rossz (5)" }
        };

        public Felszereles SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                if (SetField(ref _selectedEquipment, value))
                {
                    if (value != null)
                    {
                        NewEquipmentType = value.Tipus;
                        NewEquipmentSize = value.Meret;
                        NewEquipmentState = value.Allapot;
                        NewEquipmentPurchasePrice = value.BeszerzesiAr;
                        NewEquipmentRentalPrice = value.NapiBerletiDij;
                        NewEquipmentStoragePlace = value.RaktariHely;
                        NewEquipmentScrap = value.Selejt;
                    }
                    OnFormChanged();
                }
            }
        }

        public string NewEquipmentType
        {
            get => _newEquipmentType;
            set
            {
                if (SetField(ref _newEquipmentType, value))
                {
                    OnFormChanged();
                }
            }
        }

        public string? NewEquipmentSize
        {
            get => _newEquipmentSize;
            set
            {
                if (SetField(ref _newEquipmentSize, value))
                {
                    OnFormChanged();
                }
            }
        }

        public int? NewEquipmentState
        {
            get => _newEquipmentState;
            set
            {
                if (SetField(ref _newEquipmentState, value))
                {
                    OnFormChanged();
                }
            }
        }

        public double? NewEquipmentPurchasePrice
        {
            get => _newEquipmentPurchasePrice;
            set
            {
                if (SetField(ref _newEquipmentPurchasePrice, value))
                {
                    OnFormChanged();
                }
            }
        }

        public double? NewEquipmentRentalPrice
        {
            get => _newEquipmentRentalPrice;
            set
            {
                if (SetField(ref _newEquipmentRentalPrice, value))
                {
                    OnFormChanged();
                }
            }
        }

        public string? NewEquipmentStoragePlace
        {
            get => _newEquipmentStoragePlace;
            set
            {
                if (SetField(ref _newEquipmentStoragePlace, value))
                {
                    OnFormChanged();
                }
            }
        }

        public bool NewEquipmentScrap
        {
            get => _newEquipmentScrap;
            set
            {
                if (SetField(ref _newEquipmentScrap, value))
                {
                    OnFormChanged();
                }
            }
        }

        public ICommand AddEquipmentCommand { get; }
        public ICommand UpdateEquipmentCommand { get; }
        public ICommand DeleteEquipmentCommand { get; }
        public ICommand CancelEditCommand { get; }

        public FelszerelesViewModel(DATABASE database)
        {
            _database = database;
            NewEquipmentScrap = false;

            AddEquipmentCommand = new RelayCommand(AddEquipment, CanAddEquipment);
            UpdateEquipmentCommand = new RelayCommand(UpdateEquipment, CanUpdateEquipment);
            DeleteEquipmentCommand = new RelayCommand(DeleteEquipment, CanDeleteEquipment);
            CancelEditCommand = new RelayCommand(CancelEdit, CanCancelEdit);

            LoadEquipment();
        }

        public void OnFormChanged()
        {
            OnPropertyChanged(nameof(FormTitle));

            ((RelayCommand)AddEquipmentCommand).RaiseCanExecuteChanged();
            ((RelayCommand)UpdateEquipmentCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteEquipmentCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged();
        }

        public void ResetFields()
        {
            SelectedEquipment = null;
            NewEquipmentType = null;
            NewEquipmentSize = null;
            NewEquipmentState = null;
            NewEquipmentPurchasePrice = null;
            NewEquipmentRentalPrice = null;
            NewEquipmentStoragePlace = null;
            NewEquipmentScrap = false;
            OnFormChanged();
        }

        public void LoadEquipment()
        {
            Equipment.Clear();
            var allEquipment = _database.GetAllFelszerelesek();
            foreach (var item in allEquipment)
            {
                Equipment.Add(item);
            }
        }

        private bool CanAddEquipment()
        {
            return SelectedEquipment == null &&
                   !string.IsNullOrWhiteSpace(NewEquipmentType) &&
                   !string.IsNullOrWhiteSpace(NewEquipmentSize) &&
                   NewEquipmentState >= 1 && NewEquipmentState <= 5 &&
                   NewEquipmentPurchasePrice >= 0 &&
                   NewEquipmentRentalPrice >= 0 &&
                   !string.IsNullOrWhiteSpace(NewEquipmentStoragePlace);
        }

        private void AddEquipment()
        {
            if (CanAddEquipment())
            {
                var newEquipment = new Felszereles(
                    Id: 0,
                    Tipus: NewEquipmentType,
                    Meret: NewEquipmentSize,
                    Allapot: NewEquipmentState ?? 1,
                    BeszerzesiAr: NewEquipmentPurchasePrice ?? 0,
                    NapiBerletiDij: NewEquipmentRentalPrice ?? 0,
                    RaktariHely: NewEquipmentStoragePlace,
                    Selejt: NewEquipmentScrap
                );
                _database.InsertFelszereles(newEquipment);
                ResetFields();

                LoadEquipment();
            }
        }

        private bool CanUpdateEquipment()
        {
            return SelectedEquipment != null &&
                   !string.IsNullOrWhiteSpace(NewEquipmentType) &&
                   !string.IsNullOrWhiteSpace(NewEquipmentSize) &&
                   NewEquipmentState >= 1 && NewEquipmentState <= 5 &&
                   NewEquipmentPurchasePrice >= 0 &&
                   NewEquipmentRentalPrice >= 0 &&
                   !string.IsNullOrWhiteSpace(NewEquipmentStoragePlace);
        }

        private void UpdateEquipment()
        {
            if (CanUpdateEquipment())
            {
                var updatedEquipment = new Felszereles(
                    Id: SelectedEquipment.Id,
                    Tipus: NewEquipmentType,
                    Meret: NewEquipmentSize,
                    Allapot: NewEquipmentState ?? 1,
                    BeszerzesiAr: NewEquipmentPurchasePrice ?? 0,
                    NapiBerletiDij: NewEquipmentRentalPrice ?? 0,
                    RaktariHely: NewEquipmentStoragePlace,
                    Selejt: NewEquipmentScrap
                );
                _database.UpdateFelszereles(updatedEquipment);
                ResetFields();
                LoadEquipment();
            }
        }

        private bool CanDeleteEquipment()
        {
            return SelectedEquipment != null;
        }

        private void DeleteEquipment()
        {
            if (CanDeleteEquipment())
            {
                _database.DeleteFelszereles(SelectedEquipment.Id);
                ResetFields();
                LoadEquipment();
            }
        }

        private bool CanCancelEdit()
        {
            return SelectedEquipment != null ||
                   !string.IsNullOrWhiteSpace(NewEquipmentType) ||
                   !string.IsNullOrWhiteSpace(NewEquipmentSize) ||
                   NewEquipmentState != null ||
                   NewEquipmentPurchasePrice != null ||
                   NewEquipmentRentalPrice != null ||
                   !string.IsNullOrWhiteSpace(NewEquipmentStoragePlace) ||
                   NewEquipmentScrap;
        }

        private void CancelEdit()
        {
            ResetFields();
            OnFormChanged();
        }
    }
}
