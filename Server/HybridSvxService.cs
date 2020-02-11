using ServerApp;
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

namespace Server
{
    partial class HybridSvxService : ServiceBase
    {
        //private static Timer aTimer;

        public HybridSvxService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //aTimer = new Timer(10000); // 10 Seconds
            //aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            //aTimer.Enabled = true;

            System.Threading.Thread workerThread = new System.Threading.Thread(longprocess);
            workerThread.Start();
            
        }
       
        private void longprocess()
        {
            Program Server = new Program();
            Server.StartServer();
        }

        protected override void OnStop()
        {
            //aTimer.Stop();
        }

    }
}
