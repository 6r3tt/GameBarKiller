using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;

namespace GameBarKiller
{
    public partial class GameBarKiller : ServiceBase
    {
        private int eventId = 1;
        private EventLog eventLog1;
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public GameBarKiller(string[] args)
        {
            InitializeComponent();
            string eventSourceName = "MySource";
            string logName = "MyNewLog";

            if (args.Length > 0)
            {
                eventSourceName = args[0];
            }

            if (args.Length > 1)
            {
                logName = args[1];
            }

            eventLog1 = new EventLog();
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("In OnStart. ");
            Timer timer = new Timer();
            timer.Interval = 60000; // 60 Seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            try
            {
                ProcessKiller();
            } catch
            {
                throw new NotImplementedException();
            }
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {   
            // TODO: Insert monitoring activities here.
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
            throw new NotImplementedException();
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop. ");
            /**
             * (Optional) If OnStop is a long-running method, repeat this procedure in the OnStop method.
             * Implement the SERVICE_STOP_PENDING status and return the SERVICE_STOPPED status before the OnStop method exits.
             * 
                // Update the service state to Stop Pending.
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                // Update the service state to Stopped.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
             */
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue.");
            try
            {
                ProcessKiller();
            } catch
            {
                throw new NotImplementedException();
            }
        }

        // Cycles through running processes an kills GameBarPresenceWriter.exe if it is present.
        private void ProcessKiller()
        {
            string procName = "GameBarPresenceWriter.exe";
            Process[] processes = Process.GetProcessesByName();

            foreach(Process process in processes)
            {
                try
                {
                    if (process == procName)
                    {
                        process.Kill();
                    }
                } catch
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}

// Service State Data
public enum ServiceState
{
    SERVICE_STOPPED = 0x00000001,
    SERVICE_START_PENDING = 0x00000002,
    SERVICE_STOP_PENDING = 0x00000003,
    SERVICE_RUNNING = 0x00000004,
    SERVICE_CONTINUE_PENDING = 0x00000005,
    SERVICE_PAUSE_PENDING = 0x00000006,
    SERVICE_PAUSED = 0x00000007,
}

// Struct Layout :)
[StructLayout(LayoutKind.Sequential)]
public struct ServiceStatus
{
    public int dwServiceType;
    public ServiceState dwCurrentState;
    public int dwControlsAccepted;
    public int dwWin32Exitcode;
    public int dwServiceSpecificExitCode;
    public int dwCheckPoint;
    public int dwWaitHint;
};