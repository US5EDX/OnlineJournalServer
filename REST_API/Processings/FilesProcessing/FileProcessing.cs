using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace REST_API.Processings.FilesProcessing
{
    public class FileProcessing
    {
        public List<string> GetNames(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                return Directory.GetFiles(directoryPath)
                .Select(path => Path.GetFileName(path)).OrderByDescending(pa => pa).ToList();
            }

            return null;
        }

        public bool ArchiveTables(List<string> toSave, string databaseName)
        {
            string scriptPath = ConfigurationManager.AppSettings["restoreTablesPath"] +
                ConfigurationManager.AppSettings["archiveExec"];

            string dateTime = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");

            bool res = true;

            foreach (var table in toSave)
            {
                string exportPath = ConfigurationManager.AppSettings["restoreTablesPath"] +
                    '\\' + databaseName + '\\' + dateTime + '_' + table + "_backup.dat";

                res = ExecuteFile(scriptPath, databaseName, table, exportPath);

                if (!res)
                    return res;
            }

            return res;
        }

        public bool RestoreTable(string toRestore, string databaseName)
        {
            string scriptPath = ConfigurationManager.AppSettings["restoreTablesPath"] +
                ConfigurationManager.AppSettings["restoreExec"];

            string importPath = ConfigurationManager.AppSettings["restoreTablesPath"] +
                '\\' + databaseName + '\\' + toRestore;

            var array = toRestore.Split('_');

            string table = string.Join("_", array.Skip(6).Take(array.Length - 7));

            return ExecuteFile(scriptPath, databaseName, table, importPath);
        }

        private static bool ExecuteFile(string scriptPath, string databaseName, string table, string filePath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = scriptPath,
                Arguments = $"{ConfigurationManager.AppSettings["serverName"]} {databaseName} {$"[{table}]"} {filePath}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    while (!process.StandardOutput.EndOfStream)
                    {
                        string line = process.StandardOutput.ReadLine();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}