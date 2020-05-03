using ProcessTracker.Helpers;
using ProcessTracker.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ProcessTracker.Data
{
    public class DataAccessLayer
    {
        private readonly string _conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        public IList<ProcessModel> GetAllApplications()
        {
            IList<ProcessModel> processList = new List<ProcessModel>();
            try
            {
                Logger.LogInfo($"Connection string: {_conStr}");

                var query = "SELECT [Id], [ProcessId], [ProcessName], [ActivatorFilePath], [DeActivatorFilePath], [Activate] from [dbo].[Processes]";
                using (SqlConnection con = new SqlConnection(_conStr))
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (con.State == ConnectionState.Closed)
                        con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var process = new ProcessModel
                        {
                            ProcessName = reader["ProcessName"].ToString(),
                            ActivatorFilePath = reader["ActivatorFilePath"].ToString(),
                            DeActivatorFilePath = reader["DeActivatorFilePath"].ToString(),
                            Activate = (bool)reader["Activate"],
                            Id = (int)reader["Id"],
                        };

                        int processId = 0;
                        if (reader["ProcessId"] != null && int.TryParse(reader["ProcessId"].ToString(), out processId))
                            process.ProcessId = processId;

                        processList.Add(process);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Something went wrong, Exception: {e.Message}");
            }

            return processList;
        }

        public bool UpdateApplicationsStatus(IList<ProcessModel> applicationsList)
        {
            try
            {
                StringBuilder query = new StringBuilder("");
                foreach (var item in applicationsList)
                {
                    query.Append($"UPDATE [dbo].[Processes] SET [ProcessId] = {item.ProcessId}, [CpuUsage] = {item.CpuUsage}, [MemoryUsage]= {item.MemoryUsage} WHERE [Id] = {item.Id};");
                }
                using (SqlConnection con = new SqlConnection(_conStr))
                using (SqlCommand cmd = new SqlCommand(query.ToString(), con))
                {
                    if (con.State == ConnectionState.Closed)
                        con.Open();
                    var count = cmd.ExecuteNonQuery();
                    return count > 0;
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Something went wrong, Exception: {e.Message}");
                return false;
            }
        }
    }
}
