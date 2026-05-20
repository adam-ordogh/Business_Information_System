using SiBerlo.Models;
using SiBerlo.Services;
using SiBerlo.DatabaseAccess;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;

namespace SiBerlo.ViewModels
{
    class AlkalmazottakViewModel : ViewModelBase
    {
        private readonly DATABASE _database;
        private WageService _wageService;

        private string _newEmployeeName;
        private string _newEmployeePosition;
        private double? _newEmployeeBaseSalary;
        private double? _newEmployeeBonusPercentage;
        private bool _newEmployeeActive;

        public string FormTitle => SelectedEmployee == null ? "Új alkalmazott felvétele" : "Alkalmazott módosítása";

        public List<string> WorkPositionTypes { get; } = new List<string>
        {
            "Pultos",
            "Raktáros",
            "Karbantartó",
            "Könyvelő",
            "Menedzser"
        };

        public ObservableCollection<Alkalmazott> Employees { get; } = new ObservableCollection<Alkalmazott>();

        private Alkalmazott _selectedEmployee;
        public Alkalmazott SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (SetField(ref _selectedEmployee, value))
                {
                    if (value != null)
                    {
                        NewEmployeeName = value.Nev;
                        NewEmployeePosition = value.Beosztas;
                        NewEmployeeBaseSalary = value.Alapber;
                        NewEmployeeBonusPercentage = value.JutalekSzazalek;
                        NewEmployeeActive = value.Aktiv;
                    }

                    OnFormChanged();
                }
            }
        }

        public string NewEmployeeName
        {
            get => _newEmployeeName;
            set
            {
                if (SetField(ref _newEmployeeName, value))
                {
                    OnFormChanged();
                }
            }
        }

        public string NewEmployeePosition
        {
            get => _newEmployeePosition;
            set
            {
                if (SetField(ref _newEmployeePosition, value))
                {
                    OnFormChanged();
                }
            }
        }

        public double? NewEmployeeBaseSalary
        {
            get => _newEmployeeBaseSalary;
            set
            {
                if (SetField(ref _newEmployeeBaseSalary, value))
                {
                    OnFormChanged();
                }
            }
        }

        public double? NewEmployeeBonusPercentage
        {
            get => _newEmployeeBonusPercentage;
            set => SetField(ref _newEmployeeBonusPercentage, value);
        }

        public bool NewEmployeeActive
        {
            get => _newEmployeeActive;
            set => SetField(ref _newEmployeeActive, value);
        }

        public ICommand AddEmployeeCommand { get; }
        public ICommand UpdateEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand CancelEditCommand { get; }

        public AlkalmazottakViewModel(DATABASE db)
        {
            _database = db;
            _wageService = new WageService(_database);

            NewEmployeeActive = true;

            AddEmployeeCommand = new RelayCommand(AddEmployee, CanAddEmployee);
            UpdateEmployeeCommand = new RelayCommand(UpdateEmployee, CanUpdateEmployee);
            DeleteEmployeeCommand = new RelayCommand(DeleteEmployee, CanDeleteEmployee);
            CancelEditCommand = new RelayCommand(CancelEdit, CanCancelEdit);

            LoadEmployees();
        }

        private void OnFormChanged()
        {
            OnPropertyChanged(nameof(FormTitle));
            ((RelayCommand)AddEmployeeCommand).RaiseCanExecuteChanged();
            ((RelayCommand)UpdateEmployeeCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteEmployeeCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CancelEditCommand).RaiseCanExecuteChanged();
        }

        private void ResetFields()
        {
            NewEmployeeName = string.Empty;
            NewEmployeePosition = null;
            NewEmployeeBaseSalary = null;
            NewEmployeeBonusPercentage = null;
            NewEmployeeActive = true;

            OnFormChanged();
        }

        private void LoadEmployees()
        {
            Employees.Clear();
            var employees = _database.GetAllAlkalmazottak();
            foreach (var employee in employees)
            {
                Employees.Add(employee);
            }
        }

        //Hozzáadás
        private bool CanAddEmployee()
        {
            return !string.IsNullOrWhiteSpace(NewEmployeeName) &&
                   !string.IsNullOrWhiteSpace(NewEmployeePosition) &&
                   NewEmployeeBaseSalary > 0 &&
                   SelectedEmployee == null;            
        }
        private void AddEmployee()
        {
            if (string.IsNullOrWhiteSpace(NewEmployeeName)) return;

            var newEmployee = new Alkalmazott(
                Id: 0,
                Nev: NewEmployeeName,
                Beosztas: NewEmployeePosition,
                BelepesDatuma: DateTime.Now.ToString("yyyy-MM-dd"),
                Alapber: NewEmployeeBaseSalary ?? 0,
                JutalekSzazalek: NewEmployeeBonusPercentage ?? 0,
                Aktiv: NewEmployeeActive
            );

            long employeeId = _database.InsertAlkalmazott(newEmployee);
            _wageService.AddWageToNewEmployee(newEmployee, employeeId);

            LoadEmployees();
            ResetFields();
        }        

        //Módosítás és törlés
        private bool CanUpdateEmployee() => SelectedEmployee != null;

        private void UpdateEmployee()
        {
            if (SelectedEmployee == null) return;

            var updated = new Alkalmazott(
                Id: SelectedEmployee.Id,
                Nev: NewEmployeeName,
                Beosztas: NewEmployeePosition,
                BelepesDatuma: SelectedEmployee.BelepesDatuma,
                Alapber: NewEmployeeBaseSalary ?? 0,
                JutalekSzazalek: NewEmployeeBonusPercentage ?? 0,
                Aktiv: NewEmployeeActive
            );

            _database.UpdateAlkalmazott(updated);
            _wageService.UpdateWageForEmployee(updated);

            LoadEmployees();
            ResetFields();
        }

        private bool CanDeleteEmployee() => SelectedEmployee != null;

        private void DeleteEmployee()
        {
            if (SelectedEmployee == null) return;

            _database.DeleteAlkalmazott(SelectedEmployee.Id);
            LoadEmployees();

            ResetFields();
        }

        private void CancelEdit()
        {
            SelectedEmployee = null;

            ResetFields();
        }
        private bool CanCancelEdit()
        {
            return SelectedEmployee != null ||
                   !string.IsNullOrWhiteSpace(NewEmployeeName) ||
                   !string.IsNullOrWhiteSpace(NewEmployeePosition) ||
                   NewEmployeeBaseSalary.HasValue ||
                   NewEmployeeBonusPercentage.HasValue ||
                   !NewEmployeeActive;
        }
    }
}