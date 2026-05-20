using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SiBerlo.DatabaseAccess;
using SiBerlo.ViewModels;
using SiBerlo.Views;

namespace SiBerlo.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        DATABASE db = new DATABASE();

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetField(ref _currentView, value);
        }

        public ICommand NavigateCommand { get; }

        public MainWindowViewModel()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            db.InitializeDatabase();
            NavigateCommand = new RelayCommand<string>(param =>
            {
                CurrentView = param switch
                {
                    "AlkalmazottakView" => new AlkalmazottakView { DataContext = new AlkalmazottakViewModel(db) },
                    "UgyfelekView" => new UgyfelekView { DataContext = new UgyfelekViewModel(db) },
                    "FelszerelesView" => new FelszerelesView { DataContext = new FelszerelesViewModel(db) },
                    "BerlesekView" => new BerlesekView { DataContext = new BerlesekViewModel(db) },
                    "BejelentkezesekView" => new BejelentkezesekView { DataContext = new  BejelentkezesekViewModel(db) },
                    "BerperiodusokView" => new BerperiodusokView { DataContext = new BerperiodusokViewModel(db) },
                    "BerekView" => new BerekView { DataContext = new BerekViewModel(db) },
                    "JelentesekView" => new JelentesekView { DataContext = new JelentesekViewModel(db) },
                    _ => throw new ArgumentException()
                };
            });
            
            CurrentView = null;
        }
    }
}
