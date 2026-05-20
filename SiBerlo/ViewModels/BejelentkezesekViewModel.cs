using SiBerlo.Models;
using SiBerlo.DatabaseAccess;
using SiBerlo.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;
using Microsoft.Windows.Themes;
using System.Diagnostics;
using System.Windows;
using System.Globalization;

namespace SiBerlo.ViewModels
{
    class BejelentkezesekViewModel : ViewModelBase
    {
        DATABASE _database;
        WageService wageService;
        private bool _left = false;

        public string ArrivedOrLeft => _left == false ? "Bejelentkezés" : "Kijelentkezés";
        public ObservableCollection<Alkalmazott> Employees { get; } = new ObservableCollection<Alkalmazott>();

        private Alkalmazott _selectedEmployee;

        public bool Left
        {
            get => _left;
            set
            {
                if (SetField(ref _left, value))
                {
                    OnFromChanged();
                }
            }
        }

        public Alkalmazott SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (SetField(ref _selectedEmployee, value))
                {
                    if (value != null)
                    {
                        _left = _database.GetBejelentkezesByAlkalmazottAndDate(value.Id, DateTime.Now) != null;
                    }
                    else
                    {
                        _left = false;
                    }
                    OnFromChanged();
                }
            }
        }

        public ICommand CheckinOrOutCommand { get; }
        public BejelentkezesekViewModel(DATABASE database)
        {
            _database = database;
            wageService = new WageService(_database);
            LoadEmployees();

            CheckinOrOutCommand = new RelayCommand(CheckinOrOut, CanCheckinOrOut);
        }

        private void OnFromChanged()
        {
            OnPropertyChanged(nameof(ArrivedOrLeft));
            ((RelayCommand)CheckinOrOutCommand).RaiseCanExecuteChanged();
        }

        private void LoadEmployees()
        {
            Employees.Clear();
            foreach (var employee in _database.GetAllAlkalmazottak())
            {
                Employees.Add(employee);
            }
        }

        private bool CanCheckinOrOut()
        {
            return SelectedEmployee != null;
        }

        private void CheckinOrOut()
        {
            Bejelentkezes bejelentkezes = _database.GetBejelentkezesByAlkalmazottAndDate(SelectedEmployee.Id, DateTime.Now);
            if (bejelentkezes == null)
            {                
                _database.InsertBejelenkezes(new Bejelentkezes(
                    Id: 0,
                    AlkalmazottId: SelectedEmployee.Id,
                    Erkezes: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Tavozas: null
                    ));
            }
            else if (bejelentkezes.Tavozas == null)
            {
                var updatedBejelentkezes = new Bejelentkezes(
                    Id: bejelentkezes.Id,
                    AlkalmazottId: bejelentkezes.AlkalmazottId,
                    Erkezes: bejelentkezes.Erkezes,
                    Tavozas: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                );

                _database.UpdateBejelenkezes(updatedBejelentkezes);

                DateTime erkezes = DateTime.ParseExact(updatedBejelentkezes.Erkezes, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime tavozas = DateTime.ParseExact(updatedBejelentkezes.Tavozas, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                TimeSpan workedTime = tavozas - erkezes;

                double totalWorkedHours = workedTime.TotalHours;
                bool isWeekend = (erkezes.DayOfWeek == DayOfWeek.Saturday || erkezes.DayOfWeek == DayOfWeek.Sunday);

                if (totalWorkedHours > 8)
                {
                    double overtimeHours = totalWorkedHours - 8;
                    wageService.UpdateOvertimeForEmployee(SelectedEmployee, totalWorkedHours, overtimeHours, isWeekend);
                }
            }
            else
            {
                MessageBox.Show("Ez az alkalmazott már kijelentkezett!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
                OnPropertyChanged(nameof(ArrivedOrLeft));
            LoadEmployees();

            SelectedEmployee = null;
        }
    }
}
