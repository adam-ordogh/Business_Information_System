using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiBerlo.DatabaseAccess;
using SiBerlo.Models;

namespace SiBerlo.Services
{
    class WageService
    {
        private double _szocho = 0.094;
        private double _szja = 0.19;
        private double _tuloraSzorzoszam = 15; //Lehet át kell gondolni ha lesz idő
        double _hetvegiBonuszSzorzo = 5; //Lehet át kell gondolni ha lesz idő

        DATABASE _database;
        public WageService(DATABASE database)
        {
            _database = database;
        }

        public void AddWageToNewEmployee(Alkalmazott employee, long employeeId)
        {
            Berperiodus? currentPeriod = _database.GetCurrentBerperiodus();

            double szocho = employee.Alapber * _szocho;
            double afterSzocho = employee.Alapber - szocho;
            double szja = afterSzocho * _szja;
            double nettoBer = afterSzocho - szja;

            Ber ber = new Ber(
                Id: 0,
                AlkalmazottId: employeeId,
                PeriodusId: currentPeriod.Id,
                Alapber: employee.Alapber,
                TuloraBer: 0,
                HetvegiBonusz: 0,
                Jutalek: 0,
                EgyebPotlek: 0,
                Eloleg: 0,
                Szocho: szocho,
                Szja: szja,
                NettoBer: nettoBer
            );

            _database.InsertBerek(ber);
        }

        public void UpdateWageForEmployee(Alkalmazott employee)
        {
            Berperiodus? currentPeriod = _database.GetCurrentBerperiodus();
            Ber employeeCurrentPay = _database.GetWageForEmployeeInCurrentPeriod(employee.Id, currentPeriod.Id);

            double gross = employee.Alapber
                         + employeeCurrentPay.TuloraBer
                         + employeeCurrentPay.HetvegiBonusz
                         + employeeCurrentPay.Jutalek
                         + employeeCurrentPay.EgyebPotlek;

            double szocho = gross * _szocho;
            double afterSzocho = gross - szocho;
            double szja = afterSzocho * _szja;
            double nettoBer = afterSzocho - szja - employeeCurrentPay.Eloleg;


            Ber updatedPay = new Ber(
                Id: employeeCurrentPay.Id,
                AlkalmazottId: employee.Id,
                PeriodusId: currentPeriod.Id,
                Alapber: employee.Alapber,
                TuloraBer: employeeCurrentPay.TuloraBer,
                HetvegiBonusz: employeeCurrentPay.HetvegiBonusz,
                Jutalek: employeeCurrentPay.Jutalek,
                EgyebPotlek: employeeCurrentPay.EgyebPotlek,
                Eloleg: employeeCurrentPay.Eloleg,
                Szocho: szocho,
                Szja: szja,
                NettoBer: nettoBer
            );

            _database.UpdateBerek(updatedPay);
        }

        public void UpdateOvertimeForEmployee(Alkalmazott employee, double oraSzam, double tuloraSzam, bool isWeekend)
        {
            Berperiodus? currentPeriod = _database.GetCurrentBerperiodus();
            Ber employeeCurrentPay = _database.GetWageForEmployeeInCurrentPeriod(employee.Id, currentPeriod.Id);

            double tuloraBer = tuloraSzam * _tuloraSzorzoszam;
            double hetvegiBonusz = isWeekend ? oraSzam * _hetvegiBonuszSzorzo : 0;

            double gross = employee.Alapber
                         + tuloraBer
                         + hetvegiBonusz
                         + employeeCurrentPay.Jutalek
                         + employeeCurrentPay.EgyebPotlek;

            double szocho = gross * _szocho;
            double afterSzocho = gross - szocho;
            double szja = afterSzocho * _szja;
            double nettoBer = afterSzocho - szja - employeeCurrentPay.Eloleg;


            Ber updatedPay = new Ber(
                Id: employeeCurrentPay.Id,
                AlkalmazottId: employee.Id,
                PeriodusId: currentPeriod.Id,
                Alapber: employee.Alapber,
                TuloraBer: tuloraBer,
                HetvegiBonusz: hetvegiBonusz, 
                Jutalek: employeeCurrentPay.Jutalek,
                EgyebPotlek: employeeCurrentPay.EgyebPotlek,
                Eloleg: employeeCurrentPay.Eloleg,
                Szocho: szocho,
                Szja: szja,
                NettoBer: nettoBer
            );

            _database.UpdateBerek(updatedPay);
        }

    }


}
