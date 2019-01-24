using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace clawUI0Detect
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
            using (var sc = new ServiceController(serviceInstaller1.ServiceName))
            {
                sc.Start();
            }
        }
    }
}