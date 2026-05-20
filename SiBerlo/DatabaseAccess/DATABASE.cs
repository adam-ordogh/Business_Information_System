using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SiBerlo.Models;

namespace SiBerlo.DatabaseAccess
{
    class DATABASE
    {
        public void InitializeDatabase()
        {
            string path = "DATABASE.db";
            bool isNewDB = !File.Exists(path);

            using (var conn = new SqliteConnection($"Data Source={path};Foreign Keys=True;"))
            {
                conn.Open();
                if (isNewDB)
                {
                    string[] tables = {
                    @"CREATE TABLE Ugyfelek (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nev TEXT NOT NULL,
                        Email TEXT UNIQUE,
                        Telefon TEXT,
                        Cim TEXT,
                        RegisztracioDatuma TEXT DEFAULT (date('now')),
                        Kedvezmeny INTEGER DEFAULT 0
                    )",
                    @"CREATE TABLE Felszerelesek(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Tipus TEXT NOT NULL,                   
                        Meret TEXT,
                        Allapot INTEGER DEFAULT 5 CHECK(Allapot BETWEEN 1 AND 5),
                        BeszerzesiAr REAL,
                        NapiBerletiDij REAL NOT NULL,
                        RaktariHely TEXT,
                        Selejt BOOLEAN DEFAULT FALSE
                    )",
                    @"CREATE TABLE Berlesek(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UgyfelId INTEGER,
                        FelszerelesId INTEGER,
                        KezdoDatum TEXT NOT NULL,
                        VegDatum TEXT NOT NULL,
                        FOREIGN KEY(UgyfelId) REFERENCES Ugyfelek(Id) ON DELETE CASCADE,
                        FOREIGN KEY(FelszerelesId) REFERENCES Felszerelesek(Id) ON DELETE CASCADE
                    )",
                    @"CREATE TABLE Alkalmazottak(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nev TEXT NOT NULL,
                        Beosztas TEXT NOT NULL,
                        BelepesDatuma TEXT DEFAULT(date('now')),
                        Alapber REAL NOT NULL,
                        JutalekSzazalek REAL DEFAULT 0,
                        Aktiv BOOLEAN DEFAULT FALSE
                    )",
                    @"CREATE TABLE Bejelenkezesek(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AlkalmazottId INTEGER,
                        Erkezes TEXT NOT NULL,                  
                        Tavozas TEXT,
                        FOREIGN KEY(AlkalmazottId) REFERENCES Alkalmazottak(Id) ON DELETE CASCADE
                    )",
                    @"CREATE TABLE Szabadsagok(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AlkalmazottId INTEGER,
                        KezdoDatum TEXT NOT NULL,
                        VegDatum TEXT NOT NULL,
                        Tipus TEXT CHECK(Tipus IN('Fizetett', 'Fizetetlen', 'Betegszabadság')),
                        Jovahagyva BOOLEAN DEFAULT FALSE,
                        FOREIGN KEY(AlkalmazottId) REFERENCES Alkalmazottak(Id) ON DELETE CASCADE
                    )",
                    @"CREATE TABLE BerPeriodusok(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nev TEXT NOT NULL,
                        KezdoDatum TEXT NOT NULL,
                        VegDatum TEXT NOT NULL,
                        Zarolt BOOLEAN DEFAULT FALSE
                    )",
                    @"CREATE TABLE Berek(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AlkalmazottId INTEGER,
                        PeriodusId INTEGER NULL,
                        Alapber REAL NOT NULL,
                        TuloraBer REAL DEFAULT 0,
                        HetvegiBonusz REAL DEFAULT 0,
                        Jutalek REAL DEFAULT 0,
                        EgyebPotlek REAL DEFAULT 0,
                        Eloleg REAL DEFAULT 0,
                        Szocho REAL NOT NULL,
                        Szja REAL NOT NULL,
                        NettoBer REAL NOT NULL,
                        FOREIGN KEY(AlkalmazottId) REFERENCES Alkalmazottak(Id) ON DELETE CASCADE,
                        FOREIGN KEY(PeriodusId) REFERENCES BerPeriodusok(Id) ON DELETE SET NULL
                    )",
                    @"CREATE TABLE Szamlak(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        BerlesId INTEGER,
                        KiallitasDatuma TEXT DEFAULT (date('now')),
                        FizetesiHatarido TEXT,
                        Fizetve BOOLEAN DEFAULT FALSE,
                        Osszeg REAL NOT NULL,
                        Szamlaszam TEXT UNIQUE,
                        Visszavonva BOOLEAN DEFAULT FALSE,
                        FOREIGN KEY(BerlesId) REFERENCES Berlesek(Id) ON DELETE RESTRICT
                    )",
                    @"CREATE TABLE Kolcsonok(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        KiadoAlkalmazottId INTEGER,
                        FelvevoAlkalmazottId INTEGER,
                        Osszeg REAL NOT NULL,
                        KezdoDatum TEXT NOT NULL,
                        VisszafizetesDatuma TEXT,
                        Leiras TEXT,
                        FOREIGN KEY(KiadoAlkalmazottId) REFERENCES Alkalmazottak(Id) ON DELETE CASCADE,
                        FOREIGN KEY(FelvevoAlkalmazottId) REFERENCES Alkalmazottak(Id) ON DELETE CASCADE
                    )",
                    @"CREATE TABLE KeszletMozgasok(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FelszerelesId INTEGER,
                        AlkalmazottId INTEGER,
                        Datum TEXT DEFAULT (datetime('now')),
                        Tipus TEXT CHECK(Tipus IN('Beérkezés', 'Selejtezés', 'Áthelyezés')),
                        Mennyiseg INTEGER NOT NULL,
                        Megjegyzes TEXT,
                        FOREIGN KEY(FelszerelesId) REFERENCES Felszerelesek(Id) ON DELETE CASCADE,
                        FOREIGN KEY(AlkalmazottId) REFERENCES Alkalmazottak(Id) ON DELETE CASCADE
                    )",
                    @"CREATE TABLE EsemenyNaplo(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AlkalmazottId INTEGER,
                        DatumIdo TEXT DEFAULT(datetime('now')),
                        Tipus TEXT NOT NULL,
                        EntitasId INTEGER,
                        EntitasTipus TEXT,
                        Leiras TEXT NOT NULL,
                        FOREIGN KEY(AlkalmazottId) REFERENCES Alkalmazottak(Id) ON DELETE SET NULL
                    )",
                    };

                    foreach (var sql in tables)
                    {
                        using (var cmd = new SqliteCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

            }
        }
        /*
         * Ügyfél - I U
         * Felszerelés - I U
         * Bérlés - I U 
         * Alkalmazott - I U
         * Bejelentkezés - I U
         * Szabadság - I U
         * BérPeriódus - I U
         * Bérek - I U
         * Számla - I U
         * Kölcsön - I U
         * Készletmozgás - I U
         * Eseménynapló - I U
         */
        //#####################################################################
        //-------------------------------INSERTS-------------------------------
        //#####################################################################
        public long InsertUgyfel(Ugyfel ugyfel)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO Ugyfelek (Nev, Email, Telefon, Cim, Kedvezmeny) 
                      VALUES (@nev, @email, @telefon, @cim, @kedvezmeny);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@nev", ugyfel.Nev);
                    cmd.Parameters.AddWithValue("@email", ugyfel.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@telefon", ugyfel.Telefon ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@cim", ugyfel.Cim ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@kedvezmeny", ugyfel.Kedvezmeny);

                    try
                    {
                        return (long)cmd.ExecuteScalar();
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public long InsertFelszereles(Felszereles felszereles)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO Felszerelesek (Tipus, Meret, Allapot, BeszerzesiAr, NapiBerletiDij, RaktariHely, Selejt) 
                      VALUES (@tipus, @meret, @allapot, @beszerzesiAr, @napiBerletiDij, @raktariHely, @selejt);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@tipus", felszereles.Tipus);
                    cmd.Parameters.AddWithValue("@meret", felszereles.Meret);
                    cmd.Parameters.AddWithValue("@allapot", felszereles.Allapot);
                    cmd.Parameters.AddWithValue("@beszerzesiAr", felszereles.BeszerzesiAr);
                    cmd.Parameters.AddWithValue("@napiBerletiDij", felszereles.NapiBerletiDij);
                    cmd.Parameters.AddWithValue("@raktariHely", felszereles.RaktariHely);
                    cmd.Parameters.AddWithValue("@selejt", felszereles.Selejt);
                    return (long)cmd.ExecuteScalar();
                }
            }
        }

        public long InsertBerles(Berles berles)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO Berlesek (UgyfelId, FelszerelesId, KezdoDatum, VegDatum) 
                      VALUES (@ugyfelId, @felszerelesId, @kezdoDatum, @vegDatum);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@ugyfelId", berles.UgyfelId);
                    cmd.Parameters.AddWithValue("@felszerelesId", berles.FelszerelesId);
                    cmd.Parameters.AddWithValue("@kezdoDatum", berles.KezdoDatum);
                    cmd.Parameters.AddWithValue("@vegDatum", berles.VegDatum);
                    return (long)cmd.ExecuteScalar();
                }
            }
        }

        public long InsertAlkalmazott(Alkalmazott alkalmazott)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO Alkalmazottak (Nev, Beosztas, BelepesDatuma, Alapber, JutalekSzazalek, Aktiv) 
                      VALUES (@nev, @beosztas, @belepesDatuma, @alapber, @jutalekSzazalek, @aktiv);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@nev", alkalmazott.Nev);
                    cmd.Parameters.AddWithValue("@beosztas", alkalmazott.Beosztas);
                    cmd.Parameters.AddWithValue("@belepesDatuma", alkalmazott.BelepesDatuma);
                    cmd.Parameters.AddWithValue("@alapber", alkalmazott.Alapber);
                    cmd.Parameters.AddWithValue("@jutalekSzazalek", alkalmazott.JutalekSzazalek);
                    cmd.Parameters.AddWithValue("@aktiv", alkalmazott.Aktiv);
                    return (long)cmd.ExecuteScalar();
                }
            }
        }

        public long InsertBejelenkezes(Bejelentkezes bejelentkezes)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO Bejelenkezesek (AlkalmazottId, Erkezes, Tavozas) 
                      VALUES (@alkalmazottId, @erkezes, @tavozas);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@alkalmazottId", bejelentkezes.AlkalmazottId);
                    cmd.Parameters.AddWithValue("@erkezes", bejelentkezes.Erkezes);
                    cmd.Parameters.AddWithValue("@tavozas", bejelentkezes.Tavozas ?? (object)DBNull.Value);
                    try
                    {
                        return (long)cmd.ExecuteScalar();
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19) 
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public long InsertSzabadsag(Szabadsag szabadsag)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO Szabadsagok (AlkalmazottId, KezdoDatum, VegDatum, Tipus, Jovahagyva) 
                      VALUES (@alkalmazottId, @kezdoDatum, @vegDatum, @tipus, @jovahagyva);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@alkalmazottId", szabadsag.AlkalmazottId);
                    cmd.Parameters.AddWithValue("@kezdoDatum", szabadsag.KezdoDatum);
                    cmd.Parameters.AddWithValue("@vegDatum", szabadsag.VegDatum);
                    cmd.Parameters.AddWithValue("@tipus", szabadsag.Tipus);
                    cmd.Parameters.AddWithValue("@jovahagyva", szabadsag.Jovahagyva);
                    return (long)cmd.ExecuteScalar();
                }
            }
        }

        public long InsertBerperiodus(Berperiodus berPeriodus)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO BerPeriodusok (Nev, KezdoDatum, VegDatum, Zarolt) 
                      VALUES (@nev, @kezdoDatum, @vegDatum, @zarolt);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@nev", berPeriodus.Nev);
                    cmd.Parameters.AddWithValue("@kezdoDatum", berPeriodus.KezdoDatum);
                    cmd.Parameters.AddWithValue("@vegDatum", berPeriodus.VegDatum);
                    cmd.Parameters.AddWithValue("@zarolt", berPeriodus.Zarolt);
                    return (long)cmd.ExecuteScalar();
                }
            }
        }

        public long InsertBerek(Ber ber)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO Berek (AlkalmazottId, PeriodusId, Alapber, TuloraBer, HetvegiBonusz, Jutalek, EgyebPotlek, Eloleg, Szocho, Szja, NettoBer) 
                      VALUES (@alkalmazottId, @periodusId, @alapber, @tuloraBer, @hetvegiBonusz, @jutalek, @egyebPotlek, @eloleg, @szocho, @szja, @nettoBer);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@alkalmazottId", ber.AlkalmazottId);
                    cmd.Parameters.AddWithValue("@periodusId", ber.PeriodusId);
                    cmd.Parameters.AddWithValue("@alapber", ber.Alapber);
                    cmd.Parameters.AddWithValue("@tuloraBer", ber.TuloraBer);
                    cmd.Parameters.AddWithValue("@hetvegiBonusz", ber.HetvegiBonusz);
                    cmd.Parameters.AddWithValue("@jutalek", ber.Jutalek);
                    cmd.Parameters.AddWithValue("@egyebPotlek", ber.EgyebPotlek);
                    cmd.Parameters.AddWithValue("@eloleg", ber.Eloleg);
                    cmd.Parameters.AddWithValue("@szocho", ber.Szocho);
                    cmd.Parameters.AddWithValue("@szja", ber.Szja);
                    cmd.Parameters.AddWithValue("@nettoBer", ber.NettoBer);
                    return (long)cmd.ExecuteScalar();
                }
            }
        }

        public long InsertSzamla(Szamla szamla)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO Szamlak (BerlesId, KiallitasDatuma, FizetesiHatarido, Fizetve, Osszeg, Szamlaszam, Visszavonva) 
                      VALUES (@berlesId, @kiallitasDatuma, @fizetesiHatarido, @fizetve, @osszeg, @szamlaszam, @visszavonva);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@berlesId", szamla.BerlesId);
                    cmd.Parameters.AddWithValue("@kiallitasDatuma", szamla.KiallitasDatuma);
                    cmd.Parameters.AddWithValue("@fizetesiHatarido", szamla.FizetesiHatarido ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@fizetve", szamla.Fizetve);
                    cmd.Parameters.AddWithValue("@osszeg", szamla.Osszeg);
                    cmd.Parameters.AddWithValue("@szamlaszam", szamla.Szamlaszam);
                    cmd.Parameters.AddWithValue("@visszavonva", szamla.Visszavonva);
                    return (long)cmd.ExecuteScalar();
                }
            }
        }

        public long InsertKolcson(Kolcson kolcson)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO Kolcsonok (KiadoAlkalmazottId, FelvevoAlkalmazottId, Osszeg, KezdoDatum, VisszafizetesDatuma, Leiras) 
                      VALUES (@kiadoAlkalmazottId, @felvevoAlkalmazottId, @osszeg, @kezdoDatum, @visszafizetesDatuma, @leiras);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@kiadoAlkalmazottId", kolcson.KiadoAlkalmazottId);
                    cmd.Parameters.AddWithValue("@felvevoAlkalmazottId", kolcson.FelvevoAlkalmazottId);
                    cmd.Parameters.AddWithValue("@osszeg", kolcson.Osszeg);
                    cmd.Parameters.AddWithValue("@kezdoDatum", kolcson.KezdoDatum);
                    cmd.Parameters.AddWithValue("@visszafizetesDatuma", kolcson.VisszafizetesDatuma ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@leiras", kolcson.Leiras ?? (object)DBNull.Value);
                    try
                    {
                        return (long)cmd.ExecuteScalar();
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public long InsertKeszletMozgas(Keszletmozgas keszletmozgas)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO KeszletMozgasok (FelszerelesId, AlkalmazottId, Datum, Tipus, Mennyiseg, Megjegyzes) 
                      VALUES (@felszerelesId, @alkalmazottId, @datum, @tipus, @mennyiseg, @megjegyzes);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@felszerelesId", keszletmozgas.FelszerelesId);
                    cmd.Parameters.AddWithValue("@alkalmazottId", keszletmozgas.AlkalmazottId);
                    cmd.Parameters.AddWithValue("@datum", keszletmozgas.Datum);
                    cmd.Parameters.AddWithValue("@tipus", keszletmozgas.Tipus);
                    cmd.Parameters.AddWithValue("@mennyiseg", keszletmozgas.Mennyiseg);
                    cmd.Parameters.AddWithValue("@megjegyzes", keszletmozgas.Megjegyzes ?? (object)DBNull.Value);
                    return (long)cmd.ExecuteScalar();
                }
            }
        }

        public long InsertEsemeny(Esemenynaplo esemeny)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"INSERT INTO EsemenyNaplo (DatumIdo, Tipus, EntitasId, EntitasTipus, Leiras, AlkalmazottId) 
                      VALUES (@datumIdo, @tipus, @entitasId, @entitasTipus, @leiras, @alkalmazottId);
                      SELECT last_insert_rowid();",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@datumIdo", esemeny.DatumIdo);
                    cmd.Parameters.AddWithValue("@tipus", esemeny.Tipus);
                    cmd.Parameters.AddWithValue("@entitasId", esemeny.EntitasId);
                    cmd.Parameters.AddWithValue("@entitasTipus", esemeny.EntitasTipus);
                    cmd.Parameters.AddWithValue("@leiras", esemeny.Leiras);
                    cmd.Parameters.AddWithValue("@alkalmazottId", esemeny.AlkalmazottId);
                    return (long)cmd.ExecuteScalar();
                }
            }
        }
        //#####################################################################
        //-------------------------------UPDATES-------------------------------
        //#####################################################################
        public bool UpdateUgyfel(Ugyfel ugyfel)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE Ugyfelek 
                    SET Nev = @nev, Email = @email, Telefon = @telefon, Cim = @cim, Kedvezmeny = @kedvezmeny 
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", ugyfel.Id);
                    cmd.Parameters.AddWithValue("@nev", ugyfel.Nev);
                    cmd.Parameters.AddWithValue("@email", ugyfel.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@telefon", ugyfel.Telefon ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@cim", ugyfel.Cim ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@kedvezmeny", ugyfel.Kedvezmeny);

                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public bool UpdateFelszereles(Felszereles felszereles)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE Felszerelesek 
                    SET Tipus = @tipus, Meret = @meret, Allapot = @allapot, BeszerzesiAr = @beszerzesiAr, 
                        NapiBerletiDij = @napiBerletiDij, RaktariHely = @raktariHely, Selejt = @selejt 
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", felszereles.Id);
                    cmd.Parameters.AddWithValue("@tipus", felszereles.Tipus);
                    cmd.Parameters.AddWithValue("@meret", felszereles.Meret ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@allapot", felszereles.Allapot);
                    cmd.Parameters.AddWithValue("@beszerzesiAr", felszereles.BeszerzesiAr);
                    cmd.Parameters.AddWithValue("@napiBerletiDij", felszereles.NapiBerletiDij);
                    cmd.Parameters.AddWithValue("@raktariHely", felszereles.RaktariHely ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@selejt", felszereles.Selejt);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public bool UpdateBerles(Berles berles)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE Berlesek 
                    SET UgyfelId = @ugyfelId, FelszerelesId = @felszerelesId, KezdoDatum = @kezdoDatum, 
                        VegDatum = @vegDatum
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", berles.Id);
                    cmd.Parameters.AddWithValue("@ugyfelId", berles.UgyfelId);
                    cmd.Parameters.AddWithValue("@felszerelesId", berles.FelszerelesId);
                    cmd.Parameters.AddWithValue("@kezdoDatum", berles.KezdoDatum);
                    cmd.Parameters.AddWithValue("@vegDatum", berles.VegDatum);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public bool UpdateAlkalmazott(Alkalmazott alkalmazott)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE Alkalmazottak 
                    SET Nev = @nev, Beosztas = @beosztas, BelepesDatuma = @belepesDatuma, 
                        Alapber = @alapber, JutalekSzazalek = @jutalekSzazalek, Aktiv = @aktiv 
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", alkalmazott.Id);
                    cmd.Parameters.AddWithValue("@nev", alkalmazott.Nev);
                    cmd.Parameters.AddWithValue("@beosztas", alkalmazott.Beosztas);
                    cmd.Parameters.AddWithValue("@belepesDatuma", alkalmazott.BelepesDatuma);
                    cmd.Parameters.AddWithValue("@alapber", alkalmazott.Alapber);
                    cmd.Parameters.AddWithValue("@jutalekSzazalek", alkalmazott.JutalekSzazalek);
                    cmd.Parameters.AddWithValue("@aktiv", alkalmazott.Aktiv);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public bool UpdateBejelenkezes(Bejelentkezes bejelentkezes)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE Bejelenkezesek 
                    SET AlkalmazottId = @alkalmazottId, Erkezes = @erkezes, Tavozas = @tavozas 
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", bejelentkezes.Id);
                    cmd.Parameters.AddWithValue("@alkalmazottId", bejelentkezes.AlkalmazottId);
                    cmd.Parameters.AddWithValue("@erkezes", bejelentkezes.Erkezes);
                    cmd.Parameters.AddWithValue("@tavozas", bejelentkezes.Tavozas ?? (object)DBNull.Value);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public bool UpdateSzabadsag(Szabadsag szabadsag)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE Szabadsagok 
                    SET AlkalmazottId = @alkalmazottId, KezdoDatum = @kezdoDatum, VegDatum = @vegDatum, 
                        Tipus = @tipus, Jovahagyva = @jovahagyva 
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", szabadsag.Id);
                    cmd.Parameters.AddWithValue("@alkalmazottId", szabadsag.AlkalmazottId);
                    cmd.Parameters.AddWithValue("@kezdoDatum", szabadsag.KezdoDatum);
                    cmd.Parameters.AddWithValue("@vegDatum", szabadsag.VegDatum);
                    cmd.Parameters.AddWithValue("@tipus", szabadsag.Tipus);
                    cmd.Parameters.AddWithValue("@jovahagyva", szabadsag.Jovahagyva);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public bool UpdateBerperiodus(Berperiodus berPeriodus)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE BerPeriodusok 
                    SET Nev = @nev, KezdoDatum = @kezdoDatum, VegDatum = @vegDatum, Zarolt = @zarolt 
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", berPeriodus.Id);
                    cmd.Parameters.AddWithValue("@nev", berPeriodus.Nev);
                    cmd.Parameters.AddWithValue("@kezdoDatum", berPeriodus.KezdoDatum);
                    cmd.Parameters.AddWithValue("@vegDatum", berPeriodus.VegDatum);
                    cmd.Parameters.AddWithValue("@zarolt", berPeriodus.Zarolt);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public bool UpdateBerek(Ber ber)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE Berek 
                    SET AlkalmazottId = @alkalmazottId, PeriodusId = @periodusId, Alapber = @alapber, 
                        TuloraBer = @tuloraBer, HetvegiBonusz = @hetvegiBonusz, Jutalek = @jutalek, 
                        EgyebPotlek = @egyebPotlek, Eloleg = @eloleg, Szocho = @szocho, Szja = @szja, NettoBer = @nettoBer 
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", ber.Id);
                    cmd.Parameters.AddWithValue("@alkalmazottId", ber.AlkalmazottId);
                    cmd.Parameters.AddWithValue("@periodusId", ber.PeriodusId);
                    cmd.Parameters.AddWithValue("@alapber", ber.Alapber);
                    cmd.Parameters.AddWithValue("@tuloraBer", ber.TuloraBer);
                    cmd.Parameters.AddWithValue("@hetvegiBonusz", ber.HetvegiBonusz);
                    cmd.Parameters.AddWithValue("@jutalek", ber.Jutalek);
                    cmd.Parameters.AddWithValue("@egyebPotlek", ber.EgyebPotlek);
                    cmd.Parameters.AddWithValue("@eloleg", ber.Eloleg);
                    cmd.Parameters.AddWithValue("@szocho", ber.Szocho);
                    cmd.Parameters.AddWithValue("@szja", ber.Szja);
                    cmd.Parameters.AddWithValue("@nettoBer", ber.NettoBer);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public bool UpdateSzamla(Szamla szamla)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE Szamlak 
                    SET BerlesId = @berlesId, KiallitasDatuma = @kiallitasDatuma, 
                        FizetesiHatarido = @fizetesiHatarido, Fizetve = @fizetve, Osszeg = @osszeg, Szamlaszam = @szamlaszam, Visszavonva = @visszavonva 
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", szamla.Id);
                    cmd.Parameters.AddWithValue("@berlesId", szamla.BerlesId);
                    cmd.Parameters.AddWithValue("@kiallitasDatuma", szamla.KiallitasDatuma);
                    cmd.Parameters.AddWithValue("@fizetesiHatarido", szamla.FizetesiHatarido ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@fizetve", szamla.Fizetve);
                    cmd.Parameters.AddWithValue("@osszeg", szamla.Osszeg);
                    cmd.Parameters.AddWithValue("@szamlaszam", szamla.Szamlaszam);
                    cmd.Parameters.AddWithValue("@visszavonva", szamla.Visszavonva);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public bool UpdateKolcson(Kolcson kolcson)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE Kolcsonok 
                    SET KiadoAlkalmazottId = @kiadoAlkalmazottId, FelvevoAlkalmazottId = @felvevoAlkalmazottId, 
                        Osszeg = @osszeg, KezdoDatum = @kezdoDatum, VisszafizetesDatuma = @visszafizetesDatuma, Leiras = @leiras 
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", kolcson.Id);
                    cmd.Parameters.AddWithValue("@kiadoAlkalmazottId", kolcson.KiadoAlkalmazottId);
                    cmd.Parameters.AddWithValue("@felvevoAlkalmazottId", kolcson.FelvevoAlkalmazottId);
                    cmd.Parameters.AddWithValue("@osszeg", kolcson.Osszeg);
                    cmd.Parameters.AddWithValue("@kezdoDatum", kolcson.KezdoDatum);
                    cmd.Parameters.AddWithValue("@visszafizetesDatuma", kolcson.VisszafizetesDatuma ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@leiras", kolcson.Leiras ?? (object)DBNull.Value);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public bool UpdateKeszletMozgas(Keszletmozgas keszletmozgas)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE KeszletMozgasok 
                    SET FelszerelesId = @felszerelesId, AlkalmazottId = @alkalmazottId, Datum = @datum, 
                        Tipus = @tipus, Mennyiseg = @mennyiseg, Megjegyzes = @megjegyzes 
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", keszletmozgas.Id);
                    cmd.Parameters.AddWithValue("@felszerelesId", keszletmozgas.FelszerelesId);
                    cmd.Parameters.AddWithValue("@alkalmazottId", keszletmozgas.AlkalmazottId);
                    cmd.Parameters.AddWithValue("@datum", keszletmozgas.Datum);
                    cmd.Parameters.AddWithValue("@tipus", keszletmozgas.Tipus);
                    cmd.Parameters.AddWithValue("@mennyiseg", keszletmozgas.Mennyiseg);
                    cmd.Parameters.AddWithValue("@megjegyzes", keszletmozgas.Megjegyzes ?? (object)DBNull.Value);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }

        public bool UpdateEsemeny(Esemenynaplo esemeny)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    @"UPDATE EsemenyNaplo 
                    SET DatumIdo = @datumIdo, Tipus = @tipus, EntitasId = @entitasId, 
                        EntitasTipus = @entitasTipus, Leiras = @leiras, AlkalmazottId = @alkalmazottId 
                    WHERE Id = @id",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@id", esemeny.Id);
                    cmd.Parameters.AddWithValue("@datumIdo", esemeny.DatumIdo);
                    cmd.Parameters.AddWithValue("@tipus", esemeny.Tipus);
                    cmd.Parameters.AddWithValue("@entitasId", esemeny.EntitasId);
                    cmd.Parameters.AddWithValue("@entitasTipus", esemeny.EntitasTipus);
                    cmd.Parameters.AddWithValue("@leiras", esemeny.Leiras);
                    cmd.Parameters.AddWithValue("@alkalmazottId", esemeny.AlkalmazottId);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Adatbázis-korlátozás megsértése", ex);
                    }
                }
            }
        }
        //#####################################################################
        //-------------------------------DELETES-------------------------------
        //#####################################################################
        public bool DeleteUgyfel(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM Ugyfelek WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }
        public bool DeleteFelszereles(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM Felszerelesek WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }

        public bool DeleteBerles(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM Berlesek WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }

        public bool DeleteAlkalmazott(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM Alkalmazottak WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }

        public bool DeleteBejelenkezes(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM Bejelenkezesek WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }

        public bool DeleteSzabadsag(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM Szabadsagok WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }

        public bool DeleteBerperiodus(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM BerPeriodusok WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }

        public bool DeleteBerek(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM Berek WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }

        public bool DeleteSzamla(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM Szamlak WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }

        public bool DeleteKolcson(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM Kolcsonok WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }

        public bool DeleteKeszletMozgas(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM KeszletMozgasok WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }

        public bool DeleteEsemeny(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqliteCommand(
                    "DELETE FROM EsemenyNaplo WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException("Törlés sikertelen: Korlátozás megsértése.", ex);
                    }
                }
            }
        }

        //#####################################################################
        //-------------------------------GETS-------------------------------
        //#####################################################################
        //-------------------------------ByID----------------
        //###################################################
        public Ugyfel GetUgyfelById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Ugyfelek WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Customer with ID {id} not found.");

                            return new Ugyfel(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                Nev: reader.GetString(reader.GetOrdinal("Nev")),
                                Email: reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                                Telefon: reader.IsDBNull(reader.GetOrdinal("Telefon")) ? null : reader.GetString(reader.GetOrdinal("Telefon")),
                                Cim: reader.IsDBNull(reader.GetOrdinal("Cim")) ? null : reader.GetString(reader.GetOrdinal("Cim")),
                                RegisztracioDatuma: reader.GetString(reader.GetOrdinal("RegisztracioDatuma")),
                                Kedvezmeny: reader.GetInt32(reader.GetOrdinal("Kedvezmeny"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni az ügyfelet adatbázis hiba miatt.", ex);
            }
        }

        public Felszereles GetFelszerelesById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Felszerelesek WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Equipment with ID {id} not found.");

                            return new Felszereles(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                Tipus: reader.GetString(reader.GetOrdinal("Tipus")),
                                Meret: reader.IsDBNull(reader.GetOrdinal("Meret")) ? null : reader.GetString(reader.GetOrdinal("Meret")),
                                Allapot: reader.GetInt32(reader.GetOrdinal("Allapot")),
                                BeszerzesiAr: reader.GetDouble(reader.GetOrdinal("BeszerzesiAr")),
                                NapiBerletiDij: reader.GetDouble(reader.GetOrdinal("NapiBerletiDij")),
                                RaktariHely: reader.IsDBNull(reader.GetOrdinal("RaktariHely")) ? null : reader.GetString(reader.GetOrdinal("RaktariHely")),
                                Selejt: reader.GetBoolean(reader.GetOrdinal("Selejt"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a felszerelést adatbázis hiba miatt.", ex);
            }
        }

        public Berles GetBerlesById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Berlesek WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Rental with ID {id} not found.");

                            return new Berles(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                UgyfelId: reader.GetInt64(reader.GetOrdinal("UgyfelId")),
                                FelszerelesId: reader.GetInt64(reader.GetOrdinal("FelszerelesId")),
                                KezdoDatum: reader.GetString(reader.GetOrdinal("KezdoDatum")),
                                VegDatum: reader.GetString(reader.GetOrdinal("VegDatum"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a bérlést adatbázis hiba miatt.", ex);
            }
        }

        public Alkalmazott GetAlkalmazottById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Alkalmazottak WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Employee with ID {id} not found.");

                            return new Alkalmazott(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                Nev: reader.GetString(reader.GetOrdinal("Nev")),
                                Beosztas: reader.GetString(reader.GetOrdinal("Beosztas")),
                                BelepesDatuma: reader.GetString(reader.GetOrdinal("BelepesDatuma")),
                                Alapber: reader.GetDouble(reader.GetOrdinal("Alapber")),
                                JutalekSzazalek: reader.GetDouble(reader.GetOrdinal("JutalekSzazalek")),
                                Aktiv: reader.GetBoolean(reader.GetOrdinal("Aktiv"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni az alkalmazottat adatbázis hiba miatt.", ex);
            }
        }

        public Bejelentkezes GetBejelenkezesById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Bejelenkezesek WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Check-in record with ID {id} not found.");

                            return new Bejelentkezes(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId")),
                                Erkezes: reader.GetString(reader.GetOrdinal("Erkezes")),
                                Tavozas: reader.IsDBNull(reader.GetOrdinal("Tavozas")) ? null : reader.GetString(reader.GetOrdinal("Tavozas"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a bejelentkezést adatbázis hiba miatt.", ex);
            }
        }

        public Szabadsag GetSzabadsagById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Szabadsagok WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Leave record with ID {id} not found.");

                            return new Szabadsag(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId")),
                                KezdoDatum: reader.GetString(reader.GetOrdinal("KezdoDatum")),
                                VegDatum: reader.GetString(reader.GetOrdinal("VegDatum")),
                                Tipus: reader.GetString(reader.GetOrdinal("Tipus")),
                                Jovahagyva: reader.GetBoolean(reader.GetOrdinal("Jovahagyva"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a szabadságot adatbázis hiba miatt.", ex);
            }
        }

        public Berperiodus GetBerperiodusById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM BerPeriodusok WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Pay period with ID {id} not found.");

                            return new Berperiodus(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                Nev: reader.GetString(reader.GetOrdinal("Nev")),
                                KezdoDatum: reader.GetString(reader.GetOrdinal("KezdoDatum")),
                                VegDatum: reader.GetString(reader.GetOrdinal("VegDatum")),
                                Zarolt: reader.GetBoolean(reader.GetOrdinal("Zarolt"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a fizetési periódust adatbázis hiba miatt.", ex);
            }
        }

        public Ber GetBerekById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Berek WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Salary record with ID {id} not found.");

                            return new Ber(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId")),
                                PeriodusId: reader.GetInt64(reader.GetOrdinal("PeriodusId")),
                                Alapber: reader.GetDouble(reader.GetOrdinal("Alapber")),
                                TuloraBer: reader.GetDouble(reader.GetOrdinal("TuloraBer")),
                                HetvegiBonusz: reader.GetDouble(reader.GetOrdinal("HetvegiBonusz")),
                                Jutalek: reader.GetDouble(reader.GetOrdinal("Jutalek")),
                                EgyebPotlek: reader.GetDouble(reader.GetOrdinal("EgyebPotlek")),
                                Eloleg: reader.GetDouble(reader.GetOrdinal("Eloleg")),
                                Szocho: reader.GetDouble(reader.GetOrdinal("Szocho")),
                                Szja: reader.GetDouble(reader.GetOrdinal("Szja")),
                                NettoBer: reader.GetDouble(reader.GetOrdinal("NettoBer"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a fizetést adatbázis hiba miatt.", ex);
            }
        }

        public Szamla GetSzamlaById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Szamlak WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Invoice with ID {id} not found.");

                            return new Szamla(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                BerlesId: reader.GetInt64(reader.GetOrdinal("BerlesId")),
                                KiallitasDatuma: reader.GetString(reader.GetOrdinal("KiallitasDatuma")),
                                FizetesiHatarido: reader.IsDBNull(reader.GetOrdinal("FizetesiHatarido")) ? null : reader.GetString(reader.GetOrdinal("FizetesiHatarido")),
                                Fizetve: reader.GetBoolean(reader.GetOrdinal("Fizetve")),
                                Osszeg: reader.GetDouble(reader.GetOrdinal("Osszeg")),
                                Szamalszam: reader.GetString(reader.GetOrdinal("Szamlaszam")),
                                Visszavonva: reader.GetBoolean(reader.GetOrdinal("Visszavonva"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a számlát adatbázis hiba miatt.", ex);
            }
        }

        public Kolcson GetKolcsonById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Kolcsonok WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Loan with ID {id} not found.");

                            return new Kolcson(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                KiadoAlkalmazottId: reader.GetInt64(reader.GetOrdinal("KiadoAlkalmazottId")),
                                FelvevoAlkalmazottId: reader.GetInt64(reader.GetOrdinal("FelvevoAlkalmazottId")),
                                Osszeg: reader.GetDouble(reader.GetOrdinal("Osszeg")),
                                KezdoDatum: reader.GetString(reader.GetOrdinal("KezdoDatum")),
                                VisszafizetesDatuma: reader.IsDBNull(reader.GetOrdinal("VisszafizetesDatuma")) ? null : reader.GetString(reader.GetOrdinal("VisszafizetesDatuma")),
                                Leiras: reader.IsDBNull(reader.GetOrdinal("Leiras")) ? null : reader.GetString(reader.GetOrdinal("Leiras"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a kölcsönt adatbázis hiba miatt.", ex);
            }
        }

        public Keszletmozgas GetKeszletMozgasById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM KeszletMozgasok WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Inventory movement with ID {id} not found.");

                            return new Keszletmozgas(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                FelszerelesId: reader.GetInt64(reader.GetOrdinal("FelszerelesId")),
                                AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId")),
                                Datum: reader.GetString(reader.GetOrdinal("Datum")),
                                Tipus: reader.GetString(reader.GetOrdinal("Tipus")),
                                Mennyiseg: reader.GetInt32(reader.GetOrdinal("Mennyiseg")),
                                Megjegyzes: reader.IsDBNull(reader.GetOrdinal("Megjegyzes")) ? null : reader.GetString(reader.GetOrdinal("Megjegyzes"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a készletmozgást adatbázis hiba miatt.", ex);
            }
        }

        public Esemenynaplo GetEsemenyById(long id)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM EsemenyNaplo WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"Event log with ID {id} not found.");

                            return new Esemenynaplo(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                DatumIdo: reader.GetString(reader.GetOrdinal("DatumIdo")),
                                Tipus: reader.GetString(reader.GetOrdinal("Tipus")),
                                EntitasId: reader.GetInt32(reader.GetOrdinal("EntitasId")),
                                EntitasTipus: reader.GetString(reader.GetOrdinal("EntitasTipus")),
                                Leiras: reader.GetString(reader.GetOrdinal("Leiras")),
                                AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni az eseményt adatbázis hiba miatt.", ex);
            }
        }

        //###################################################
        //-------------------------------All----------------
        //###################################################
        public List<Ugyfel> GetAllUgyfelek()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var ugyfelek = new List<Ugyfel>();

            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Ugyfelek", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ugyfelek.Add(new Ugyfel(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                Nev: reader.GetString(reader.GetOrdinal("Nev")),
                                Email: reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                                Telefon: reader.IsDBNull(reader.GetOrdinal("Telefon")) ? null : reader.GetString(reader.GetOrdinal("Telefon")),
                                Cim: reader.IsDBNull(reader.GetOrdinal("Cim")) ? null : reader.GetString(reader.GetOrdinal("Cim")),
                                RegisztracioDatuma: reader.GetString(reader.GetOrdinal("RegisztracioDatuma")),
                                Kedvezmeny: reader.GetInt32(reader.GetOrdinal("Kedvezmeny"))
                            ));
                        }
                    }
                }
                return ugyfelek;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni az ügyfeleket adatbázis hiba miatt.", ex);
            }
        }

        public List<Felszereles> GetAllFelszerelesek()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var felszerelesek = new List<Felszereles>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Felszerelesek", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            felszerelesek.Add(new Felszereles(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                Tipus: reader.GetString(reader.GetOrdinal("Tipus")),
                                Meret: reader.IsDBNull(reader.GetOrdinal("Meret")) ? null : reader.GetString(reader.GetOrdinal("Meret")),
                                Allapot: reader.GetInt32(reader.GetOrdinal("Allapot")),
                                BeszerzesiAr: reader.GetDouble(reader.GetOrdinal("BeszerzesiAr")),
                                NapiBerletiDij: reader.GetDouble(reader.GetOrdinal("NapiBerletiDij")),
                                RaktariHely: reader.IsDBNull(reader.GetOrdinal("RaktariHely")) ? null : reader.GetString(reader.GetOrdinal("RaktariHely")),
                                Selejt: reader.GetBoolean(reader.GetOrdinal("Selejt"))
                            ));
                        }
                    }
                }
                return felszerelesek;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a felszereléseket adatbázis hiba miatt.", ex);
            }
        }

        public List<Berles> GetAllBerlesek()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var berlesek = new List<Berles>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Berlesek", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            berlesek.Add(new Berles(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                UgyfelId: reader.GetInt64(reader.GetOrdinal("UgyfelId")),
                                FelszerelesId: reader.GetInt64(reader.GetOrdinal("FelszerelesId")),
                                KezdoDatum: reader.GetString(reader.GetOrdinal("KezdoDatum")),
                                VegDatum: reader.GetString(reader.GetOrdinal("VegDatum"))
                            ));
                        }
                    }
                }
                return berlesek;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a bérléseket adatbázis hiba miatt.", ex);
            }
        }

        public List<Alkalmazott> GetAllAlkalmazottak()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var alkalmazottak = new List<Alkalmazott>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Alkalmazottak", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            alkalmazottak.Add(new Alkalmazott(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                Nev: reader.GetString(reader.GetOrdinal("Nev")),
                                Beosztas: reader.GetString(reader.GetOrdinal("Beosztas")),
                                BelepesDatuma: reader.GetString(reader.GetOrdinal("BelepesDatuma")),
                                Alapber: reader.GetDouble(reader.GetOrdinal("Alapber")),
                                JutalekSzazalek: reader.GetDouble(reader.GetOrdinal("JutalekSzazalek")),
                                Aktiv: reader.GetBoolean(reader.GetOrdinal("Aktiv"))
                            ));
                        }
                    }
                }
                return alkalmazottak;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni az alkalmazottakat adatbázis hiba miatt.", ex);
            }

        }

        public List<Bejelentkezes> GetAllBejelenkezesek()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var bejelenkezesek = new List<Bejelentkezes>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Bejelenkezesek", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bejelenkezesek.Add(new Bejelentkezes(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId")),
                                Erkezes: reader.GetString(reader.GetOrdinal("Erkezes")),
                                Tavozas: reader.IsDBNull(reader.GetOrdinal("Tavozas")) ? null : reader.GetString(reader.GetOrdinal("Tavozas"))
                            ));
                        }
                    }
                }
                return bejelenkezesek;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a bejelentkezéseket adatbázis hiba miatt.", ex);
            }
        }

        public List<Szabadsag> GetAllSzabadsagok()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var szabadsagok = new List<Szabadsag>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Szabadsagok", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            szabadsagok.Add(new Szabadsag(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId")),
                                KezdoDatum: reader.GetString(reader.GetOrdinal("KezdoDatum")),
                                VegDatum: reader.GetString(reader.GetOrdinal("VegDatum")),
                                Tipus: reader.GetString(reader.GetOrdinal("Tipus")),
                                Jovahagyva: reader.GetBoolean(reader.GetOrdinal("Jovahagyva"))
                            ));
                        }
                    }
                }
                return szabadsagok;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a szabadságokat adatbázis hiba miatt.", ex);
            }
        }

        public List<Berperiodus> GetAllBerperiodusok()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var berPeriodusok = new List<Berperiodus>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM BerPeriodusok", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            berPeriodusok.Add(new Berperiodus(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                Nev: reader.GetString(reader.GetOrdinal("Nev")),
                                KezdoDatum: reader.GetString(reader.GetOrdinal("KezdoDatum")),
                                VegDatum: reader.GetString(reader.GetOrdinal("VegDatum")),
                                Zarolt: reader.GetBoolean(reader.GetOrdinal("Zarolt"))
                            ));
                        }
                    }
                }
                return berPeriodusok;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a fizetési periódusokat adatbázis hiba miatt.", ex);
            }
        }

        public List<Ber> GetAllBerek()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var berek = new List<Ber>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Berek", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            berek.Add(new Ber(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId")),
                                PeriodusId: reader.GetInt64(reader.GetOrdinal("PeriodusId")),
                                Alapber: reader.GetDouble(reader.GetOrdinal("Alapber")),
                                TuloraBer: reader.GetDouble(reader.GetOrdinal("TuloraBer")),
                                HetvegiBonusz: reader.GetDouble(reader.GetOrdinal("HetvegiBonusz")),
                                Jutalek: reader.GetDouble(reader.GetOrdinal("Jutalek")),
                                EgyebPotlek: reader.GetDouble(reader.GetOrdinal("EgyebPotlek")),
                                Eloleg: reader.GetDouble(reader.GetOrdinal("Eloleg")),
                                Szocho: reader.GetDouble(reader.GetOrdinal("Szocho")),
                                Szja: reader.GetDouble(reader.GetOrdinal("Szja")),
                                NettoBer: reader.GetDouble(reader.GetOrdinal("NettoBer"))
                            ));
                        }
                    }
                }
                return berek;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a béreket adatbázis hiba miatt.", ex);
            }
        }

        public List<Szamla> GetAllSzamlak()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var szamlak = new List<Szamla>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Szamlak", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            szamlak.Add(new Szamla(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                BerlesId: reader.GetInt64(reader.GetOrdinal("BerlesId")),
                                KiallitasDatuma: reader.GetString(reader.GetOrdinal("KiallitasDatuma")),
                                FizetesiHatarido: reader.IsDBNull(reader.GetOrdinal("FizetesiHatarido")) ? null : reader.GetString(reader.GetOrdinal("FizetesiHatarido")),
                                Fizetve: reader.GetBoolean(reader.GetOrdinal("Fizetve")),
                                Osszeg: reader.GetDouble(reader.GetOrdinal("Osszeg")),
                                Szamalszam: reader.GetString(reader.GetOrdinal("Szamlaszam")),
                                Visszavonva: reader.GetBoolean(reader.GetOrdinal("Visszavonva"))
                            ));
                        }
                    }
                }
                return szamlak;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a számlákat adatbázis hiba miatt.", ex);
            }
        }

        public List<Kolcson> GetAllKolcsonok()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var kolcsonok = new List<Kolcson>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Kolcsonok", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            kolcsonok.Add(new Kolcson(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                KiadoAlkalmazottId: reader.GetInt64(reader.GetOrdinal("KiadoAlkalmazottId")),
                                FelvevoAlkalmazottId: reader.GetInt64(reader.GetOrdinal("FelvevoAlkalmazottId")),
                                Osszeg: reader.GetDouble(reader.GetOrdinal("Osszeg")),
                                KezdoDatum: reader.GetString(reader.GetOrdinal("KezdoDatum")),
                                VisszafizetesDatuma: reader.IsDBNull(reader.GetOrdinal("VisszafizetesDatuma")) ? null : reader.GetString(reader.GetOrdinal("VisszafizetesDatuma")),
                                Leiras: reader.IsDBNull(reader.GetOrdinal("Leiras")) ? null : reader.GetString(reader.GetOrdinal("Leiras"))
                            ));
                        }
                    }
                }
                return kolcsonok;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a kölcsönöket adatbázis hiba miatt.", ex);
            }
        }

        public List<Keszletmozgas> GetAllKeszletMozgasok()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var keszletMozgasok = new List<Keszletmozgas>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM KeszletMozgasok", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            keszletMozgasok.Add(new Keszletmozgas(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                FelszerelesId: reader.GetInt64(reader.GetOrdinal("FelszerelesId")),
                                AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId")),
                                Datum: reader.GetString(reader.GetOrdinal("Datum")),
                                Tipus: reader.GetString(reader.GetOrdinal("Tipus")),
                                Mennyiseg: reader.GetInt32(reader.GetOrdinal("Mennyiseg")),
                                Megjegyzes: reader.IsDBNull(reader.GetOrdinal("Megjegyzes")) ? null : reader.GetString(reader.GetOrdinal("Megjegyzes"))
                            ));
                        }
                    }
                }
                return keszletMozgasok;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a készletmozgásokat adatbázis hiba miatt.", ex);
            }
        }

        public List<Esemenynaplo> GetAllEsemenyek()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var esemenyek = new List<Esemenynaplo>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM EsemenyNaplo", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            esemenyek.Add(new Esemenynaplo(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                DatumIdo: reader.GetString(reader.GetOrdinal("DatumIdo")),
                                Tipus: reader.GetString(reader.GetOrdinal("Tipus")),
                                EntitasId: reader.GetInt32(reader.GetOrdinal("EntitasId")),
                                EntitasTipus: reader.GetString(reader.GetOrdinal("EntitasTipus")),
                                Leiras: reader.GetString(reader.GetOrdinal("Leiras")),
                                AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId"))
                            ));
                        }
                    }
                }
                return esemenyek;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni az eseménynaplót adatbázis hiba miatt.", ex);
            }
        }

        //#####################################################################
        //-------------------------------UNIQUE GETS---------------------------
        //#####################################################################
        public List<Felszereles> GetFelszerelesByType(string type)
        {
            List<Felszereles> felszerelesek = new List<Felszereles>();
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";

            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Felszerelesek WHERE Tipus = @type", conn))
                    {
                        cmd.Parameters.AddWithValue("@type", type);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                felszerelesek.Add(new Felszereles(
                                    Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                    Tipus: reader.GetString(reader.GetOrdinal("Tipus")),
                                    Meret: reader.IsDBNull(reader.GetOrdinal("Meret")) ? null : reader.GetString(reader.GetOrdinal("Meret")),
                                    Allapot: reader.GetInt32(reader.GetOrdinal("Allapot")),
                                    BeszerzesiAr: reader.GetDouble(reader.GetOrdinal("BeszerzesiAr")),
                                    NapiBerletiDij: reader.GetDouble(reader.GetOrdinal("NapiBerletiDij")),
                                    RaktariHely: reader.IsDBNull(reader.GetOrdinal("RaktariHely")) ? null : reader.GetString(reader.GetOrdinal("RaktariHely")),
                                    Selejt: reader.GetBoolean(reader.GetOrdinal("Selejt"))
                                ));
                            }
                        }
                    }
                }
                return felszerelesek;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a felszerelést adatbázis hiba miatt.", ex);
            }
        }

        public List<Felszereles> GetAvailableFelszerelesByType(string type, DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
                throw new ArgumentException("Start and end dates must be provided.");

            List<Felszereles> felszerelesek = new();

            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";

            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand(
                        @"SELECT f.* FROM Felszerelesek f
                          WHERE f.Tipus = @type
                          AND f.Id NOT IN (
                              SELECT b.FelszerelesId FROM Berlesek b
                              WHERE date(b.KezdoDatum) <= date(@endDate)
                              AND date(b.VegDatum) >= date(@startDate)
                          )", conn))
                    {
                        cmd.Parameters.AddWithValue("@type", type);
                        cmd.Parameters.AddWithValue("@startDate", startDate.Value.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@endDate", endDate.Value.ToString("yyyy-MM-dd"));

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                felszerelesek.Add(new Felszereles(
                                    Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                    Tipus: reader.GetString(reader.GetOrdinal("Tipus")),
                                    Meret: reader.IsDBNull(reader.GetOrdinal("Meret")) ? null : reader.GetString(reader.GetOrdinal("Meret")),
                                    Allapot: reader.GetInt32(reader.GetOrdinal("Allapot")),
                                    BeszerzesiAr: reader.GetDouble(reader.GetOrdinal("BeszerzesiAr")),
                                    NapiBerletiDij: reader.GetDouble(reader.GetOrdinal("NapiBerletiDij")),
                                    RaktariHely: reader.IsDBNull(reader.GetOrdinal("RaktariHely")) ? null : reader.GetString(reader.GetOrdinal("RaktariHely")),
                                    Selejt: reader.GetBoolean(reader.GetOrdinal("Selejt"))
                                ));
                            }
                        }
                    }
                }
                return felszerelesek;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a szabad felszerelést adatbázis hiba miatt.", ex);
            }
        }

        public string GenerateNextSzamlaszam()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                // Get current year
                int currentYear = DateTime.Now.Year;

                // Get the highest invoice number for this year
                using (var cmd = new SqliteCommand(
                    @"SELECT MAX(Szamlaszam) FROM Szamlak 
              WHERE Szamlaszam LIKE @yearPattern",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@yearPattern", $"{currentYear}-%");

                    var maxNumber = cmd.ExecuteScalar()?.ToString();

                    if (string.IsNullOrEmpty(maxNumber))
                    {
                        return $"{currentYear}-0001";
                    }

                    // Extract the numeric part and increment
                    int lastNumber = int.Parse(maxNumber.Split('-')[1]);
                    return $"{currentYear}-{(lastNumber + 1).ToString("D4")}";
                }
            }
        }

        public Bejelentkezes GetBejelentkezesByAlkalmazottAndDate(long alkalmazottId, DateTime date)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand(
                        @"SELECT * FROM Bejelenkezesek 
                          WHERE AlkalmazottId = @alkalmazottId 
                          AND date(Erkezes) = date(@date)", conn))
                    {
                        cmd.Parameters.AddWithValue("@alkalmazottId", alkalmazottId);
                        cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Bejelentkezes(
                                    Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                    AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId")),
                                    Erkezes: reader.GetString(reader.GetOrdinal("Erkezes")),
                                    Tavozas: reader.IsDBNull(reader.GetOrdinal("Tavozas")) ? null : reader.GetString(reader.GetOrdinal("Tavozas"))
                                );
                            }
                            return null;
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Hiba a bejelentkezés lekérdezésekor.", ex);
            }
        }

        public Berperiodus GetCurrentBerperiodus()
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand(
                        @"SELECT * FROM BerPeriodusok 
                          WHERE date(KezdoDatum) <= date('now') 
                          AND date(VegDatum) >= date('now') 
                          AND Zarolt = 0", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException("Nincs nyitott fizetési periódus.");
                            return new Berperiodus(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                Nev: reader.GetString(reader.GetOrdinal("Nev")),
                                KezdoDatum: reader.GetString(reader.GetOrdinal("KezdoDatum")),
                                VegDatum: reader.GetString(reader.GetOrdinal("VegDatum")),
                                Zarolt: reader.GetBoolean(reader.GetOrdinal("Zarolt"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a jelenlegi fizetési periódust adatbázis hiba miatt.", ex);
            }
        }

        public Ber GetWageForEmployeeInCurrentPeriod(long AlkalmazottId, long CurrentBerperiodusId)
        {
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand(
                        @"SELECT * FROM Berek 
                          WHERE AlkalmazottId = @alkalmazottId 
                          AND PeriodusId = @periodusId", conn))
                    {
                        cmd.Parameters.AddWithValue("@alkalmazottId", AlkalmazottId);
                        cmd.Parameters.AddWithValue("@periodusId", CurrentBerperiodusId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new KeyNotFoundException($"No wage record found for employee ID {AlkalmazottId} in period ID {CurrentBerperiodusId}.");
                            return new Ber(
                                Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId")),
                                PeriodusId: reader.GetInt64(reader.GetOrdinal("PeriodusId")),
                                Alapber: reader.GetDouble(reader.GetOrdinal("Alapber")),
                                TuloraBer: reader.GetDouble(reader.GetOrdinal("TuloraBer")),
                                HetvegiBonusz: reader.GetDouble(reader.GetOrdinal("HetvegiBonusz")),
                                Jutalek: reader.GetDouble(reader.GetOrdinal("Jutalek")),
                                EgyebPotlek: reader.GetDouble(reader.GetOrdinal("EgyebPotlek")),
                                Eloleg: reader.GetDouble(reader.GetOrdinal("Eloleg")),
                                Szocho: reader.GetDouble(reader.GetOrdinal("Szocho")),
                                Szja: reader.GetDouble(reader.GetOrdinal("Szja")),
                                NettoBer: reader.GetDouble(reader.GetOrdinal("NettoBer"))
                            );
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a fizetést az alkalmazott számára az aktuális periódusban adatbázis hiba miatt.", ex);
            }
        }

        public List<Ber> GetBerekByPeriodus(long periodusId)
        {            
            const string connectionString = "Data Source=DATABASE.db;Foreign Keys=True;";
            var berek = new List<Ber>();
            try
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("SELECT * FROM Berek WHERE PeriodusId = @periodusId", conn))
                    {
                        cmd.Parameters.AddWithValue("@periodusId", periodusId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                berek.Add(new Ber(
                                    Id: reader.GetInt64(reader.GetOrdinal("Id")),
                                    AlkalmazottId: reader.GetInt64(reader.GetOrdinal("AlkalmazottId")),
                                    PeriodusId: reader.GetInt64(reader.GetOrdinal("PeriodusId")),
                                    Alapber: reader.GetDouble(reader.GetOrdinal("Alapber")),
                                    TuloraBer: reader.GetDouble(reader.GetOrdinal("TuloraBer")),
                                    HetvegiBonusz: reader.GetDouble(reader.GetOrdinal("HetvegiBonusz")),
                                    Jutalek: reader.GetDouble(reader.GetOrdinal("Jutalek")),
                                    EgyebPotlek: reader.GetDouble(reader.GetOrdinal("EgyebPotlek")),
                                    Eloleg: reader.GetDouble(reader.GetOrdinal("Eloleg")),
                                    Szocho: reader.GetDouble(reader.GetOrdinal("Szocho")),
                                    Szja: reader.GetDouble(reader.GetOrdinal("Szja")),
                                    NettoBer: reader.GetDouble(reader.GetOrdinal("NettoBer"))
                                ));
                            }
                        }
                    }
                }
                return berek;
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("Nem sikerült lekérni a fizetést adatbázis hiba miatt.", ex);
            }
        }
    }
}
