using System;
using System.Configuration;

namespace REST_API.Processings.OptionsChoosing
{
    public class OptionChoosing
    {
        public string ChooseRestorePath(int options)
        {
            switch (options)
            {
                case 1:
                    return ConfigurationManager.AppSettings["restoreMainPath"];
                case 2:
                    return ConfigurationManager.AppSettings["restoreTablesPath"] + "\\OnlineJournal";
                case 3:
                    return ConfigurationManager.AppSettings["restoreLogPath"];
                default:
                    throw new ArgumentException("Invalid option");
            }
        }

        public (string DBName, string path) ChooseDB(int options)
        {
            switch (options)
            {
                case 1:
                    return ("OnlineJournal", ConfigurationManager.AppSettings["restoreMainPath"]);
                case 2:
                    return ("OnlineJournalLogs", ConfigurationManager.AppSettings["restoreLogPath"]);
                default:
                    throw new ArgumentException("Invalid option");
            }
        }
    }
}