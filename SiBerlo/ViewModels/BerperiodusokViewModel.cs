using SiBerlo.Models;
using SiBerlo.Services;
using SiBerlo.DatabaseAccess;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;
using Microsoft.Windows.Themes;
using System.Diagnostics;
using System.Windows;

namespace SiBerlo.ViewModels
{
    class BerperiodusokViewModel : ViewModelBase
    {
        DATABASE _database;
        WageService wageService;

        private string _newBerperiodusNev;
        private DateTime? _newBerperiodusKezdete;
        private DateTime? _newBerperiodusVege;
        private bool _berperiodusZarolt;

        public string FormTitle => "Bérperiódusok kezelése";

        public ObservableCollection<Berperiodus> Berperiodusok { get; } = new ObservableCollection<Berperiodus>();
        private Berperiodus _selectedBerperiodus;

        public Berperiodus SelectedBerperiodus
        {
            get => _selectedBerperiodus;
            set
            {
                if (SetField(ref _selectedBerperiodus, value))
                {
                    if (value != null)
                    {
                        NewBerperiodusNev = value.Nev;
                        NewBerperiodusKezdete = DateTime.TryParse(value.KezdoDatum, out DateTime startDate) ? startDate : (DateTime?)null;
                        NewBerperiodusVege = DateTime.TryParse(value.VegDatum, out DateTime endDate) ? endDate : (DateTime?)null;
                        BerperiodusZarolt = value.Zarolt;

                    }
                    OnFormChanged();
                }
            }
        }

        public string NewBerperiodusNev
        {
            get => _newBerperiodusNev;
            set
            {
                if (SetField(ref _newBerperiodusNev, value))
                {
                    OnFormChanged();
                }
            }
        }

        public DateTime? NewBerperiodusKezdete
        {
            get => _newBerperiodusKezdete;
            set
            {
                if (SetField(ref _newBerperiodusKezdete, value))
                {
                    OnFormChanged();
                }
            }
        }

        public DateTime? NewBerperiodusVege
        {
            get => _newBerperiodusVege;
            set
            {
                if (SetField(ref _newBerperiodusVege, value))
                {
                    OnFormChanged();
                }
            }
        }

        public bool BerperiodusZarolt
        {
            get => _berperiodusZarolt;
            set
            {
                if (SetField(ref _berperiodusZarolt, value))
                {
                    OnFormChanged();
                }
            }
        }

        public ICommand AddBerperiodusCommand { get; }
        public ICommand UpdateBerperiodusCommand { get; }
        public ICommand DeleteBerperiodusCommand { get; }
        public ICommand CancelEditCommand { get; }

        public BerperiodusokViewModel(DATABASE database)
        {
            _database = database;
            wageService = new WageService(database);

            AddBerperiodusCommand = new RelayCommand(AddBerperiodus, CanAddBerperiodus);
            UpdateBerperiodusCommand = new RelayCommand(UpdateBerperiodus, CanupdateBerperiodus);
            DeleteBerperiodusCommand = new RelayCommand(DeleteBerperiodus, CandeleteBerperiodus);
            CancelEditCommand = new RelayCommand(CancelEdit, CanCancelEdit);

            LoadBerperiodusok();
        }

        private void OnFormChanged()
        {
            ((RelayCommand)AddBerperiodusCommand).RaiseCanExecuteChanged();
            ((RelayCommand)UpdateBerperiodusCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteBerperiodusCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged();
        }

        private void ResetFields()
        {
            SelectedBerperiodus = null;
            NewBerperiodusNev = string.Empty;
            NewBerperiodusKezdete = null;
            NewBerperiodusVege = null;
            BerperiodusZarolt = false;
        }

        private void LoadBerperiodusok()
        {
            Berperiodusok.Clear();
            var berperiodusok = _database.GetAllBerperiodusok();
            foreach (var period in berperiodusok)
            {
                Berperiodusok.Add(period);
            }
        }

        private bool CanAddBerperiodus()
        {
            return _selectedBerperiodus == null &&
                   !string.IsNullOrWhiteSpace(NewBerperiodusNev) &&
                   NewBerperiodusKezdete.HasValue &&
                   NewBerperiodusVege.HasValue &&
                   NewBerperiodusKezdete < NewBerperiodusVege;
        }

        private void AddBerperiodus()
        {
            if (CanAddBerperiodus())
            {
                var newPeriod = new Berperiodus(
                    Id: 0,
                    Nev: NewBerperiodusNev,
                    KezdoDatum: NewBerperiodusKezdete.Value.ToString("yyyy-MM-dd"),
                    VegDatum: NewBerperiodusVege.Value.ToString("yyyy-MM-dd"),
                    Zarolt: false
                );
                _database.InsertBerperiodus(newPeriod);

                LoadBerperiodusok();
                ResetFields();
            }
        }

        private bool CanupdateBerperiodus()
        {
            return _selectedBerperiodus != null &&
                   !string.IsNullOrWhiteSpace(NewBerperiodusNev) &&
                   NewBerperiodusKezdete.HasValue &&
                   NewBerperiodusVege.HasValue &&
                   NewBerperiodusKezdete < NewBerperiodusVege;
        }

        private void UpdateBerperiodus()
        {
            if (CanupdateBerperiodus())
            {
                var updatedPeriod = new Berperiodus(
                    Id: _selectedBerperiodus.Id,
                    Nev: NewBerperiodusNev,
                    KezdoDatum: NewBerperiodusKezdete.Value.ToString("yyyy-MM-dd"),
                    VegDatum: NewBerperiodusVege.Value.ToString("yyyy-MM-dd"),
                    Zarolt: BerperiodusZarolt
                );
                _database.UpdateBerperiodus(updatedPeriod);
                LoadBerperiodusok();
                ResetFields();
            }
        }

        private bool CandeleteBerperiodus()
        {
            return _selectedBerperiodus != null;
        }

        private void DeleteBerperiodus()
        {
            if (CandeleteBerperiodus())
            {
                _database.DeleteBerperiodus(_selectedBerperiodus.Id);
                Berperiodusok.Remove(_selectedBerperiodus);
                ResetFields();
            }
        }

        private bool CanCancelEdit()
        {
            return _selectedBerperiodus != null ||
                   !string.IsNullOrWhiteSpace(NewBerperiodusNev) ||
                   NewBerperiodusKezdete.HasValue ||
                   NewBerperiodusVege.HasValue;
        }

        private void CancelEdit()
        {
            ResetFields();
            OnFormChanged();
        }
    }
}
