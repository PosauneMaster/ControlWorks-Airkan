using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;


namespace ControlWorks.Services
{

    // Set 'RunInstaller' attribute to true.
    [RunInstaller(true)]
    public class MyInstallerClass : Installer
    {
        public MyInstallerClass() : base()
        {
            // Attach the 'Committed' event.
            this.Committed += new InstallEventHandler(MyInstaller_Committed);
            // Attach the 'Committing' event.
            this.Committing += new InstallEventHandler(MyInstaller_Committing);
        }
        // Event handler for 'Committing' event.
        private void MyInstaller_Committing(object sender, InstallEventArgs e)
        {
            Console.WriteLine("");
            Console.WriteLine("Committing Event occurred.");
            Console.WriteLine("");
        }
        // Event handler for 'Committed' event.
        private void MyInstaller_Committed(object sender, InstallEventArgs e)
        {
            Console.WriteLine("");
            Console.WriteLine("Committed Event occurred.");
            Console.WriteLine("");
        }
        // Override the 'Install' method.
        public override void Install(IDictionary savedState)
        {
            RunProcess("Uninstall.bat");
            base.Install(savedState);

            var path = Context.Parameters["assemblyPath"];
            var assemblyName = AssemblyName.GetAssemblyName(path);
            var version = assemblyName.Version.ToString();

            var options = $"\"ControlWorks wrapper service for REST API version {version}\"";

            RunProcess("Install.bat", options);
            RunProcess("Start.bat");

        }

        private void RunProcess(string processName, string options = "")
        {
            var path = Context.Parameters["assemblyPath"];
            var fi = new FileInfo(path);
            var filepath = Path.Combine(fi.DirectoryName, processName);
            var installLogPath = Path.Combine(fi.DirectoryName, "installLog.txt");

            var batchOptions = fi.DirectoryName;
            if (!string.IsNullOrEmpty(options))
            {
                batchOptions = $"{fi.DirectoryName} {options}";
            }

            var processInfo = new ProcessStartInfo(filepath, batchOptions);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;

            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            var sb = new StringBuilder();
            sb.AppendLine(process.StandardOutput.ReadToEnd());
            sb.AppendLine(process.StandardError.ReadToEnd());

            var exitCode = process.ExitCode;

            File.AppendAllText(installLogPath, sb.ToString());
            process.Close();
        }



        // Override the 'Commit' method.
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }
        // Override the 'Rollback' method.
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }
    }
}
