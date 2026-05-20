using System;
using System.Windows.Input;
using System.Collections.ObjectModel;

using SiBerlo.Models;
using SiBerlo.DatabaseAccess;
using System.Diagnostics;

namespace SiBerlo.ViewModels
{
    class UgyfelekViewModel : ViewModelBase
    {
        private readonly DATABASE _database;
        private string _newCustomerName;
        private string? _newCustomerEmail;
        private string? _newCustomerPhone;
        private string? _newCustomerAddress;
        private int? _newCustomerDiscount;

        public string FormTitle => SelectedCustomer == null ? "Új ügyfél regisztrálása" : "Ügyfél módosítása";

        public ObservableCollection<Ugyfel> Customers { get; } = new ObservableCollection<Ugyfel>();
        private Ugyfel _selectedCustomer;
        public Ugyfel SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetField(ref _selectedCustomer, value))
                {
                    if (value != null)
                    {
                        NewCustomerName = value.Nev;
                        NewCustomerEmail = value.Email;
                        NewCustomerPhone = value.Telefon;
                        NewCustomerAddress = value.Cim;
                        NewCustomerDiscount = value.Kedvezmeny;
                    }
                    OnFormChanged();
                }
            }
        }

        public string NewCustomerName
        {
            get => _newCustomerName;
            set
            {
                if (SetField(ref _newCustomerName, value))
                {
                    OnFormChanged();
                }
            }
        }

        public string? NewCustomerEmail
        {
            get => _newCustomerEmail;
            set
            {
                if (SetField(ref _newCustomerEmail, value))
                {
                    OnFormChanged();
                }
            }
        }

        public string? NewCustomerPhone
        {
            get => _newCustomerPhone;
            set
            {
                if (SetField(ref _newCustomerPhone, value))
                {
                    OnFormChanged();
                }
            }
        }

        public string? NewCustomerAddress
        {
            get => _newCustomerAddress;
            set
            {
                if (SetField(ref _newCustomerAddress, value))
                {
                    OnFormChanged();
                }
            }
        }

        public int? NewCustomerDiscount
        {
            get => _newCustomerDiscount;
            set
            {
                if (SetField(ref _newCustomerDiscount, value))
                {
                    OnFormChanged();
                }
            }
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand UpdateCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand CancelEditCommand { get; }

        public UgyfelekViewModel(DATABASE db)
        {
            _database = db;

            AddCustomerCommand = new RelayCommand(AddCustomer, CanAddCustomer);
            UpdateCustomerCommand = new RelayCommand(UpdateCustomer, CanUpdateCustomer);
            DeleteCustomerCommand = new RelayCommand(DeleteCustomer, CanDeleteCustomer);
            CancelEditCommand = new RelayCommand(CancelEdit, CanCancelEdit);

            LoadCustomers();
        }

        public void OnFormChanged()
        {
            OnPropertyChanged(nameof(FormTitle));

            ((RelayCommand)AddCustomerCommand).RaiseCanExecuteChanged();
            ((RelayCommand)UpdateCustomerCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteCustomerCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged();
        }

        public void ResetFields()
        {
            SelectedCustomer = null;
            NewCustomerName = string.Empty;
            NewCustomerEmail = null;
            NewCustomerPhone = null;
            NewCustomerAddress = null;
            NewCustomerDiscount = null;

            OnFormChanged();
        }

        //Data loading and manipulation methods
        private void LoadCustomers()
        {
            Customers.Clear();
            var customers = _database.GetAllUgyfelek();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
        }
        public void AddCustomer()
        {
            if (CanAddCustomer())
            {
                var newCustomer = new Ugyfel(
                    Id: 0,
                    Nev: NewCustomerName,
                    Email: NewCustomerEmail,
                    Telefon: NewCustomerPhone,
                    Cim: NewCustomerAddress,
                    RegisztracioDatuma: DateTime.Now.ToString("yyyy-MM-dd"),
                    Kedvezmeny: NewCustomerDiscount ?? 0);
                _database.InsertUgyfel(newCustomer);
                Customers.Add(newCustomer);
                SelectedCustomer = null; // Reset the form

                ResetFields();
                LoadCustomers();
            }
        }
        public bool CanAddCustomer()
        {            
            return SelectedCustomer == null &&
                    !string.IsNullOrWhiteSpace(NewCustomerName) &&
                    !string.IsNullOrWhiteSpace(NewCustomerEmail) &&
                    !string.IsNullOrWhiteSpace(NewCustomerPhone) &&
                    !string.IsNullOrWhiteSpace(NewCustomerAddress) &&
                    NewCustomerDiscount  >= 0 &&
                    NewCustomerDiscount  <= 100;            

        }

        public void DeleteCustomer()
        {
            if (SelectedCustomer != null)
            {
                _database.DeleteUgyfel(SelectedCustomer.Id);
                Customers.Remove(SelectedCustomer);
                SelectedCustomer = null; // Reset the form

                ResetFields();
            }
        }

        public bool CanDeleteCustomer()
        {
            return SelectedCustomer != null;
        }

        public void UpdateCustomer()
        {
            if (SelectedCustomer != null && CanUpdateCustomer())
            {
                var updatedCustomer = new Ugyfel(
                    Id: SelectedCustomer.Id,
                    Nev: NewCustomerName,
                    Email: NewCustomerEmail,
                    Telefon: NewCustomerPhone,
                    Cim: NewCustomerAddress,
                    RegisztracioDatuma: SelectedCustomer.RegisztracioDatuma,
                    Kedvezmeny: NewCustomerDiscount ?? 0);
                _database.UpdateUgyfel(updatedCustomer);
                int index = Customers.IndexOf(SelectedCustomer);
                Customers[index] = updatedCustomer;
                SelectedCustomer = null; // Reset the form

                ResetFields();
            }
        }

        public bool CanUpdateCustomer()
        {
            return SelectedCustomer != null &&
                   !string.IsNullOrWhiteSpace(NewCustomerName) &&
                   !string.IsNullOrWhiteSpace(NewCustomerEmail) &&
                   !string.IsNullOrWhiteSpace(NewCustomerPhone) &&
                   !string.IsNullOrWhiteSpace(NewCustomerAddress) &&
                   NewCustomerDiscount >= 0 && NewCustomerDiscount <= 100;
        }

        public void CancelEdit()
        {
            ResetFields();
        }
        public bool CanCancelEdit()
        {
            return SelectedCustomer != null ||
                   !string.IsNullOrWhiteSpace(NewCustomerName) ||
                   !string.IsNullOrWhiteSpace(NewCustomerEmail) ||
                   !string.IsNullOrWhiteSpace(NewCustomerPhone) ||
                   !string.IsNullOrWhiteSpace(NewCustomerAddress) ||
                   NewCustomerDiscount > 0;
        }
    }
}
