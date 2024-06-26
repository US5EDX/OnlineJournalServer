using REST_API.Processings.OptionsChoosing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace REST_API.Processings.DataBase
{
    public class DataBaseActionExecuting
    {
        public Exception ExecuteArchive(int option)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["conwithoutinithial"].ConnectionString);

            try
            {
                var result = new OptionChoosing().ChooseDB(option);

                string filePath = result.path + $"\\{result.DBName}_" +
                DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".bak";

                con.Open();
                SqlCommand command = con.CreateCommand();
                command.Parameters.Add(new SqlParameter("@databaseName", result.DBName));
                command.Parameters.Add(new SqlParameter("@filePath", filePath));
                command.CommandText = ConfigurationManager.AppSettings["archiveCommand"];
                command.ExecuteNonQuery();
                con.Close();

                return null;
            }
            catch (Exception ex)
            {
                con.Close();
                return ex;
            }
        }

        public Exception ExecuteRestore(int option, string fileName)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["conwithoutinithial"].ConnectionString);

            try
            {
                var result = new OptionChoosing().ChooseDB(option);

                string filePath = result.path + '\\' + fileName;

                con.Open();
                SqlCommand command = con.CreateCommand();

                command.Parameters.Add(new SqlParameter("@databaseName", result.DBName));
                command.Parameters.Add(new SqlParameter("@filePath", filePath));
                command.CommandText = ConfigurationManager.AppSettings["restoreCommand"];
                command.ExecuteNonQuery();

                con.Close();

                return null;
            }
            catch (Exception ex)
            {
                con.Close();
                return ex;
            }
        }
    }
}