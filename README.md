# SiBerlo – Ski Pass Rental Business Information System

A complete WPF desktop application for managing ski equipment rentals, customers, employees, payroll, and financial reports. Built with C#, .NET, SQLite, and MVVM architecture.

## Features

### Core Modules
- **Customer Management** – Add, edit, delete, and list customers with discount handling.
- **Equipment Management** – Manage ski equipment inventory (type, size, condition, rental price, storage location).
- **Rental Management** – Create and modify rentals with automatic invoice generation (unique invoice numbers per year).
- **Availability Check** – Filter available equipment by type and date range when creating a rental.
- **Employee Management** – Track employees, positions, base salary, commission rates, and active status.

### Attendance & Payroll
- **Check-in / Check-out** – Record employee working hours with automatic overtime calculation.
- **Pay Periods** – Define and close pay periods.
- **Salary Calculation** – Automatic calculation of gross salary, social security (9.4%), personal income tax (19%), overtime pay, weekend bonuses, commissions, and advances.
- **Manual Adjustments** – Edit commission, other allowances, or advance payments per employee per period.

### Reports & Analytics
- **Monthly Revenue** – Chart and table of paid invoices grouped by month/year.
- **Rental Statistics** – Pie chart and table showing revenue and average rental days by equipment type.
- **Equipment Usage** – List of each equipment item with total rentals and total days used.
- **Employee Performance** – Average daily hours worked and total net salary per employee.
- **PDF Export** – Generate a complete report (all tables) as a PDF file using QuestPDF.

### Database
- **SQLite** – Local database file (`DATABASE.db`) created automatically on first run.
- **Schema** – 12+ tables with foreign keys, constraints, and triggers (via application logic).
- **Event Log** – Optional table for tracking changes (insert/update/delete).

## Technologies
| Technology | Purpose |
|:---|:---|
| **C# .NET** (8.0) | Backend logic, MVVM, data access |
| **WPF** | Desktop UI with XAML |
| **SQLite** (Microsoft.Data.Sqlite) | Embedded database |
| **MVVM Pattern** | ViewModels, RelayCommand, INotifyPropertyChanged |
| **LiveCharts** | Monthly revenue chart, rental statistics pie chart |
| **QuestPDF** | PDF report generation |
| **Git** | Version control |

## How to Run

### Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (or newer) with **.NET Desktop Development** workload
- .NET 6.0 or 8.0 SDK

### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/adam-ordogh/Business_Information_System.git
2. Open SiBerlo.sln in Visual Studio.
3. Build the solution (Ctrl+Shift+B) – NuGet packages (Microsoft.Data.Sqlite, LiveCharts.Wpf, QuestPDF) will be restored automatically.
4. Run the application (F5). The database file DATABASE.db will be created automatically in the output folder (bin/Debug/...).

Note: No external database server is required. The application uses a local SQLite file.

## Project Structure
```bash
SiBerlo/
├── DatabaseAccess/
│   └── DATABASE.cs          # SQLite CRUD operations (12+ tables)
├── Models/                  # Entity classes (Ugyfel, Alkalmazott, Berles, etc.)
├── ViewModels/              # MVVM ViewModels (commands, data binding)
│   ├── MainWindowViewModel.cs
│   ├── UgyfelekViewModel.cs
│   ├── AlkalmazottakViewModel.cs
│   ├── BerlesekViewModel.cs
│   ├── FelszerelesViewModel.cs
│   ├── BejelentkezesekViewModel.cs
│   ├── BerperiodusokViewModel.cs
│   ├── BerekViewModel.cs
│   ├── JelentesekViewModel.cs (LiveCharts + PDF export)
│   └── RelayCommand.cs
├── Views/                   # XAML windows and user controls
│   ├── MainWindow.xaml
│   ├── UgyfelekView.xaml
│   └── ...
├── Services/
│   └── WageService.cs       # Payroll calculation logic
└── DATABASE.db              # Created at runtime (not in repo)
```

## Sample Usage
1. Register a customer – Navigate to "Customers" tab, enter name, email, phone, address, discount.
2. Add equipment – Go to "Equipment" tab, select type (ski, snowboard, helmet, boots, poles), size, condition (1–5), daily rental price.
3. Create a rental – Under "Rentals", select customer, equipment type, pick start/end dates (system shows available equipment automatically). Confirm – an invoice is generated.
4. Employee check-in – "Attendance" tab, select employee, click "Check-in" (arrival time), later "Check-out" (departure). Overtime (>8h/day) is calculated automatically.
5. Payroll – Create pay periods (e.g., "2025 Q1"), then view "Salaries" tab to see net salaries for each employee. Use "Edit" to adjust commission, other allowances, or advance payments.
6. Reports – "Reports" tab shows charts and tables. Click "Export PDF" to save a complete summary.

## Author
Ádám Ördögh - MSc in Applied Computer Science – Selye János University, Komárno, Slovakia
