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

        public HybridSvxService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            System.Threading.Thread workerThread = new System.Threading.Thread(Longprocess);
            workerThread.Start();
            
        }
       
        private void Longprocess()
        {
            Program Server = new Program();
            Server.StartServer();
        }

        protected override void OnStop()
        {

        }

    }
}
