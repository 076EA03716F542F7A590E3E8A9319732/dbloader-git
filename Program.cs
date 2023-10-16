using Microsoft.Win32;

namespace DbLoader
{
    internal static class Program
    {
        public static void Main()
        {
            StartLogging();
            if (Logger is null || LogFile is null) { throw new InvalidOperationException("Unable to create log file"); }

            try 
            {
                EncryptDecrypt("_master.db");
            }
            catch (Exception ex) 
            {
                LogWrite(ex.Message);
            }
            EndLogging();
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }

        public static uint[] GetMasterLocalKeys()
        {
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
            
            return new[]
            {
                uint.Parse(key1.ToString()),
                uint.Parse(key2.ToString()),
                uint.Parse(key3.ToString())
            };
        }

        
        public static void EncryptDecrypt(string dbName)
        {
            uint[] masterLocalKeys = GetMasterLocalKeys();
            LogWrite("Getting master database");
            LogWrite();
            var cwd = Directory.GetCurrentDirectory();
            var currentDrive = Directory.GetCurrentDirectory().Split('\\')[0];
            const string expectedDbPath = "\\Program Files (x86)\\Steam\\steamapps\\common\\BLEACH Brave Souls\\home\\persistent\\files";
            var dbPath = Path.Combine(cwd, "_master.db");
            
            if (!File.Exists(dbPath))
            {
                LogWrite($"_master.db not found in: {dbPath}");
                dbPath = currentDrive + Path.Combine(currentDrive, expectedDbPath, "_master.db");
                LogWrite($"Will use install directory: {dbPath}");
                if (!File.Exists(dbPath))
                {
                    throw new InvalidOperationException(
                        "Database was not available in the install directory or current directory");
                }
                File.Copy(dbPath, Path.Combine(cwd, "_master.db"));
                dbPath = Path.Combine(cwd, "_master.db");
                LogWrite($"Copied input file: {dbPath}");
            }
            LogWrite();
            
            BuildLCGStream(masterLocalKeys, dbPath, dbName);
        }
        
        
        private static void BuildLCGStream(uint[] masterLocalKeys, string dbPath, string dbName)
        {
            var outPath = Path.Combine(Path.GetDirectoryName(dbPath), "out_master.db");
            using (var outfile = new FileStream(outPath, FileMode.OpenOrCreate))
            {
                LogWrite($"Created output file: {outPath}");
                using (var infile = new FileStream(dbPath, FileMode.Open))
                {
                    LogWrite($"Opened input file: {dbPath}");
                    // debug
                    EndLogging();
                    using (var lcgstream = new LCGStream(infile, masterLocalKeys[0], masterLocalKeys[1], masterLocalKeys[2]))
                    {
                        outfile.SetLength(0L);
                        CopyStream(lcgstream, outfile);
                    }
                }
                File.Delete(dbPath);
            }
            File.Move(outPath, dbPath);
        }
        
        
        private static void CopyStream(LCGStream inStream, Stream outStream)
        {
            byte[] array = new byte[4096];
            int num = 0;
            do
            {
                for (int i = 0; i < 256; i++)
                {
                    num = inStream.Read(array, 0, array.Length);
                    
                    if (num == 0)
                    {
                        break;
                    }
                    outStream.Write(array, 0, num);
                }
            }
            while (0 < num);
        }

        private static void StartLogging() 
        {
            LogFile = File.Create(Path.Combine(Directory.GetCurrentDirectory(), $"log-{DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss")}.txt"));
            Logger = new StreamWriter(LogFile);
        }

        private static void EndLogging() 
        {
            if (Logger is null || LogFile is null) { return; }
            Logger.Dispose();
            LogFile.Dispose();
        }

        private static void LogWrite(string msg = "") 
        {
            if (Logger is null) { throw new InvalidOperationException("No Logger initialized"); }
            Console.WriteLine(msg);
            Logger.WriteLine(msg);
        }

        private static FileStream? LogFile;

        private static StreamWriter? Logger;
        
    }
    
}



