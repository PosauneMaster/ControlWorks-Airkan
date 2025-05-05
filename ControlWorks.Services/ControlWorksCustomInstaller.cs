using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;

namespace ControlWorks.Services
{
    [RunInstaller(true)]
    public partial class ControlWorksCustomInstaller : System.Configuration.Install.Installer
    {
        public ControlWorksCustomInstaller()
        {
            File.AppendAllText(@"C:\ControlWorks\InstallLog.txt", "Install Constructor.");

            this.AfterInstall += ControlWorksInstaller_AfterInstall;
        }

        private void ControlWorksInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            File.AppendAllText(@"C:\ControlWorks\InstallLog.txt", "After Install Event occurred.");
        }

    }
}
