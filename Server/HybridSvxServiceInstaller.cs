using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ServerService
{
    [RunInstaller(true)]
    public class HybridSvxServiceInstaller : Installer
    {
        public HybridSvxServiceInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            // Setup the Service Account type per your requirement
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            serviceInstaller.ServiceName = "ServerForChatApplication";
            serviceInstaller.DisplayName = "ServerForChatApplication Service";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.Description = "My server service for chat application";

            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }

    }
}
