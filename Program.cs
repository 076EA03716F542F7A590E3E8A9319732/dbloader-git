using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace DbLoader
{
    internal static class Program
    {

        public static void Main()
        {
            var cwd = Directory.GetCurrentDirectory();
            var sqlite3PreReq = Path.Combine(cwd, "sqlite3.exe");
            if (!File.Exists(sqlite3PreReq))
            {
                Console.WriteLine("sqlite3.exe was not found");
                Console.WriteLine("Download from: https://www.sqlite.org/download.html");
                Console.WriteLine();
                Console.WriteLine("Precompiled Binaries for Windows");
                Console.WriteLine("sqlite-tools-win32-x86-3420000.zip (1.93 MiB)");
                Console.WriteLine("A bundle of command-line tools for managing SQLite database files, including the command-line shell program, the sqldiff.exe program, and the sqlite3_analyzer.exe program.");
                
                Console.WriteLine();
                Console.WriteLine("Press any key to quit...");
                Console.ReadKey();
                return;
            }
            
            // ------------------------------------------------
            // get master keys from registry
            // ------------------------------------------------
            var regPath = "HKEY_CURRENT_USER\\SOFTWARE\\KLab\\BleachBraveSouls";
#pragma warning disable CA1416
            var key1 = Registry.GetValue(regPath, "MasterKey1_h3417775103", null);
            var key2 = Registry.GetValue(regPath, "MasterKey2_h3417775100", null);
            var key3 = Registry.GetValue(regPath, "MasterKey3_h3417775101", null);
#pragma warning restore CA1416
            
            if (key1 is null || key2 is null || key3 is null)
            {
                throw new InvalidOperationException("Unable to get one or more keys, is the game installed?");
            }
            
            // ------------------------------------------------
            // get master db
            // ------------------------------------------------
            Console.WriteLine("Getting master database");
            Console.WriteLine();
            var currentDrive = Directory.GetCurrentDirectory().Split('\\')[0];
            const string expectedDbPath = "\\Program Files (x86)\\Steam\\steamapps\\common\\BLEACH Brave Souls\\home\\persistent\\files";
            var dbPath = Path.Combine(cwd, "_master.db");
            if (!File.Exists(dbPath))
            {
                Console.WriteLine($"_master.db not found in {dbPath}");
                dbPath = currentDrive + Path.Combine(currentDrive, expectedDbPath, "_master.db");
                Console.WriteLine($"Will try to copy from {dbPath}");
                if (!File.Exists(dbPath))
                {
                    throw new InvalidOperationException(
                        "Database was not available in the install directory or current directory");
                }
                File.Copy(dbPath, Path.Combine(cwd, "_master.db"));
                dbPath = Path.Combine(cwd, "_master.db");
            }
            Console.WriteLine();
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine();

            
            // ------------------------------------------------
            // get klbsqlite3.dll
            // ------------------------------------------------
            Console.WriteLine("Getting klbsqlite3.dll");
            Console.WriteLine();
            var dllPath = Path.Combine(cwd, "klbsqlite3.dll");
            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"klbsqlite3.dll not found in {dllPath}");
                var dllInstallPath ="\\Program Files (x86)\\Steam\\steamapps\\common\\BLEACH Brave Souls\\BleachBraveSouls_Data\\Plugins\\x86_64";
                dllPath = currentDrive + Path.Combine(dllInstallPath, "klbsqlite3.dll");
                Console.WriteLine($"Will try to copy from {dllPath}");
                
                if (!File.Exists(dllPath))
                {
                    throw new InvalidOperationException(
                        "Klbsqlite3.dll was not found, is the game installed?");
                }
                File.Copy(dllPath, Path.Combine(cwd, "klbsqlite3.dll"));
                dllPath = Path.Combine(cwd, "klbsqlite3.dll");
            }
            Console.WriteLine();
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine();


            // ------------------------------------------------
            // open db connection
            // ------------------------------------------------
            var keys = new[]
            {
                uint.Parse(key1.ToString()),
                uint.Parse(key2.ToString()),
                uint.Parse(key3.ToString())
            };

            Console.WriteLine("Opening Db");
            using var db = new SqliteDatabase(keys, dbPath);
            db.Open(true);
            Console.WriteLine($"Db Open Status: {db.IsOpened}");
            Console.WriteLine();
            
            using (var dbWrapper = new SqliteDatabaseWrapper(db.Connection))
            {
                var fp = Path.Combine(cwd, "sqlOutput.sql");
                if (File.Exists(fp))
                {
                    Console.WriteLine($"Overwriting {fp}");
                    File.Delete(fp);
                }
                Console.WriteLine();
                
                
                using (var sw = File.AppendText(fp))
                {
                    
                    // ------------------------------------------------
                    // get table create queries
                    // ------------------------------------------------
                    var sqliteStatement =
                        dbWrapper.Query("SELECT name, sql FROM sqlite_master ORDER BY type DESC;",
                            Array.Empty<object>());
                    var createNames = new List<string>();
                    
                    Console.WriteLine("Getting create queries");
                    Console.WriteLine();
                    sw.WriteLine("BEGIN TRANSACTION;");
                    while (sqliteStatement.Step())
                    {
                        Console.Write("+");
                        var createName = sqliteStatement.GetText(0);
                        var createQuery = sqliteStatement.GetText(1);
                        sw.WriteLine(createQuery + ";");

                        if (createQuery.StartsWith("CREATE TABLE"))
                        {
                            createNames.Add(createName);
                        }
                    }
                    sw.WriteLine("END TRANSACTION;");
                    Console.WriteLine();
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine();

                    
                    // ------------------------------------------------
                    // get table data
                    // ------------------------------------------------
                    Console.WriteLine("Getting table data");
                    sw.WriteLine("BEGIN TRANSACTION;");
                    Console.WriteLine();
                    foreach (var name in createNames)
                    {
                        var tableStatement =
                            dbWrapper.Query($"SELECT * FROM {name};");
                        var colCount = tableStatement.ColumnCount();
                        var colNames = string.Empty;

                        var n = 0;
                        do
                        {
                            colNames += tableStatement.ColumnName(n);
                            n++;
                            if (n != colCount)
                            {
                                colNames += ", ";
                            }
                        } while (n < colCount);

                        n = 0;

                        Console.Write("+");
                        while (tableStatement.Step())
                        {
                            var innerDataStr = "";
                            do
                            {
                                var dataVal = string.Empty;
                                switch (tableStatement.ColumnType(n))
                                {
                                    // int and float
                                    case 1:
                                    case 2:
                                        dataVal = tableStatement.GetText(n);
                                        break;
                                    case 3:
                                        var text = tableStatement.GetText(n);
                                        if (text.Contains('\''))
                                        {
                                            text = text.Replace("'", "''");
                                        }

                                        dataVal = $"'{text}'";
                                        break;
                                    case 4:
                                        throw new InvalidOperationException(
                                            $"Blob found {tableStatement.GetText(n)}");
                                    case 5:
                                        dataVal = "NULL";
                                        break;
                                }


                                innerDataStr += dataVal;
                                n++;
                                if (n != colCount)
                                {
                                    innerDataStr += ", ";
                                }
                            } while (n < colCount);
                            
                            n = 0;
                            var dataStr = $"INSERT INTO {name} ({colNames}) VALUES ({innerDataStr});";
                            sw.Write(dataStr);
                        }
                    }
                    sw.WriteLine("END TRANSACTION;");
                    Console.WriteLine();
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine();
                }
            }
            db.Close();

            
            Console.WriteLine("Creating plaintext db");
            Console.WriteLine();
            var ptDbPath = Path.Combine(cwd, "_plaintext.db");
            if (File.Exists(ptDbPath))
            {
                Console.WriteLine($"Overwriting existing db {ptDbPath}");
                File.Delete(ptDbPath);
            }
            Console.WriteLine();
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine();
            
            
            // ------------------------------------------------
            // build batch file
            // ------------------------------------------------
            Console.WriteLine("Building batch file");
            Console.WriteLine();
            var sqlBat = Path.Combine(cwd, "sqlCommandLineLoad.bat");
            if (!File.Exists(sqlBat))
            {
                using (StreamWriter sw = File.AppendText(sqlBat))
                {
                    sw.WriteLine(Path.Combine(cwd, "sqlite3.exe") + " -echo -interactive -init " + Path.Combine(cwd, ".sqliterc") + " %*");
                }
            }
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine();

            // ------------------------------------------------
            // build sqlite3 cli file
            // ------------------------------------------------
            Console.WriteLine("Building sqlite3 cli file");
            Console.WriteLine();
            var sqlRc = Path.Combine(cwd, ".sqliterc");
            if (!File.Exists(sqlRc))
            {
                using (StreamWriter sw = File.AppendText(sqlRc))
                {
                    sw.WriteLine(".open _plaintext.db");
                    sw.WriteLine(".read sqlOutput.sql");
                    sw.WriteLine(".exit");
                }
            }
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine();

            // ------------------------------------------------
            // build database file
            // ------------------------------------------------
            Process.Start("cmd", $"/k \"sqlCommandLineLoad.bat\"");
            
            Console.WriteLine("Executing batch file");
            Console.WriteLine();
            Console.WriteLine("When the sqlite3 cli (sqlite>) appears on the second terminal window, the process is complete and you can close all windows");
            Console.WriteLine("You can also delete sqliteOutput.sql to recover drive space");
            
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }
    }
}