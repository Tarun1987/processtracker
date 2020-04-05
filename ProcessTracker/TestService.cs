using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Linq;
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
            CheckProcessStatus();
        }

        private void CheckProcessStatus()
        {
            var dataLayer = new DataAccessLayer();

            // Get all processes from DB
            var dbProcessList = dataLayer.GetAllProcesses();
            if (dbProcessList?.Count <= 0)
            {
                Logger.LogInfo("Process list don't exists in database.");
                return;
            }

            // Get all processes which are running on command prompt.
            var runningProcessesList = Process.GetProcessesByName("cmd");
            foreach (var dbprocess in dbProcessList)
            {
                var runningProcess = runningProcessesList.FirstOrDefault(x => x.MainWindowTitle == dbprocess.ProcessName);

                // Process NOT RUNNING and ACTIVE in DB
                if (runningProcess == null && dbprocess.Activate)
                {
                    Process.Start(dbprocess.ActivatorFilePath);
                    Logger.LogInfo($"Process started. ProcessName:{dbprocess.ProcessName}");
                }
                // Process RUNNING and INACTIVE in  DB
                else if (runningProcess != null && !dbprocess.Activate)
                {
                    // Stop process
                    Process.Start(dbprocess.DeActivatorFilePath);
                    Logger.LogInfo($"Process stopped. ProcessName:{dbprocess.ProcessName}");
                }
                else
                {
                    Logger.LogInfo($"No action required. ProcessName: {dbprocess.ProcessName}");
                }

            }
        }

    }
}

