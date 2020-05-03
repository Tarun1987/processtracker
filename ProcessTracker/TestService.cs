using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;
using ProcessTracker.Helpers;
using ProcessTracker.Data;
using System.Configuration;

namespace ProcessTracker
{
    public partial class TestService : ServiceBase
    {
        private Timer _timer = new Timer();
        private readonly int _interval = int.Parse(ConfigurationManager.AppSettings["ReplayTime"]);
        public TestService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.LogInfo("Service started. Time:" + DateTime.Now);
            _timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            _timer.Interval = _interval;
            _timer.Enabled = true;
        }

        protected override void OnStop()
        {
            Logger.LogInfo("Service stopped. Time:" + DateTime.Now);
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            Logger.LogInfo($"Replay time. {_interval}");
            CheckApplicationsStatus();
        }

        private void CheckApplicationsStatus()
        {
            var dataLayer = new DataAccessLayer();

            // Get all processes from DB
            var dbApplications = dataLayer.GetAllApplications();
            if (dbApplications?.Count <= 0)
            {
                Logger.LogInfo("Process list don't exists in database.");
                return;
            }

            foreach (var dbprocess in dbApplications)
            {
                // Process NOT RUNNING and ACTIVE in DB
                if (dbprocess.ProcessId <= 0 && dbprocess.Activate)
                {
                    try
                    {
                        var startingProcess = Process.Start(dbprocess.ActivatorFilePath);
                        if (startingProcess.Id > 0)
                        {
                            dbprocess.ProcessId = startingProcess.Id;
                            dbprocess.MemoryUsage = startingProcess.WorkingSet64 / 1024 / 1024;
                            dbprocess.CpuUsage = 5;
                            // Set CPU usage and memory usage.
                            Logger.LogInfo($"Process started. ProcessName:{dbprocess.ProcessName}");
                        }
                        else
                            Logger.LogInfo($"Process not started. ProcessName:{dbprocess.ProcessName}");
                    }
                    catch (Exception)
                    {
                        Logger.LogInfo($"Process not started. ProcessName:{dbprocess.ProcessName}");
                    }
                }
                // Process RUNNING and INACTIVE in  DB
                else if (dbprocess.ProcessId > 0 && !dbprocess.Activate)
                {
                    // Stop process
                    var stoppingProcess = Process.Start(dbprocess.DeActivatorFilePath);
                    if (stoppingProcess.Id > 0)
                    {
                        dbprocess.ProcessId = 0;
                        dbprocess.CpuUsage = 0;
                        dbprocess.MemoryUsage = 0;
                        try
                        {
                            Process proc = Process.GetProcessById(dbprocess.ProcessId);
                            proc.Kill();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        Logger.LogInfo($"Process stopped. ProcessName:{dbprocess.ProcessName}");
                    }
                    else
                        Logger.LogInfo($"Process could not be stopped. ProcessName:{dbprocess.ProcessName}");
                }
                else
                {
                    Logger.LogInfo($"No action required. ProcessName: {dbprocess.ProcessName} | ProcessId: {dbprocess.ProcessId}");
                }

            }

            // Now stave status of all process in database
            var isDbUpdated = dataLayer.UpdateApplicationsStatus(dbApplications);
            if (isDbUpdated)
                Logger.LogInfo("Database updated.");
            else
                Logger.LogError("Database not updated");

        }
    }
}

