using ProcessTracker.Helpers;
using ProcessTracker.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ProcessTracker.Data
{
    public class DataAccessLayer
    {
        public IList<ProcessModel> GetAllProcesses()
        {
            IList<ProcessModel> processList = new List<ProcessModel>();
            try
            {
                var conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
                Logger.LogInfo($"Connection string: {conStr}");

                var query = "SELECT [ProcessName], [ActivatorFilePath], [DeActivatorFilePath], [Activate] from [dbo].[Processes]";
                using (SqlConnection con = new SqlConnection(conStr))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (con.State == ConnectionState.Closed)
                        con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        processList.Add(new ProcessModel
                        {
                            ProcessName = reader["ProcessName"].ToString(),
                            ActivatorFilePath = reader["ActivatorFilePath"].ToString(),
                            DeActivatorFilePath = reader["DeActivatorFilePath"].ToString(),
                            Activate = (bool)reader["Activate"]
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Something went wrong, Exception: {e.Message}");
            }

            return processList;
        }
    }
}
