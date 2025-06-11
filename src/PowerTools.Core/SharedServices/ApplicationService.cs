using System.Diagnostics;
using System.IO;
using System.Windows;

namespace PowerTools.Core.SharedServices
{
    public class ApplicationService
    {
        private static ApplicationService _instance;

        public static ApplicationService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ApplicationService();

                return _instance;
            }
        }

        private ApplicationService()
        {

        }

        public void Restart()
        {
            var executionLocation = Path.GetDirectoryName(Application.ResourceAssembly.Location);
            var applicationFullPath = Path.Combine(executionLocation, "PowerTools.exe");

            Process.Start(applicationFullPath);
            Application.Current.Shutdown();
        }
    }
}
