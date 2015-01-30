using System.ComponentModel;
using System.Configuration.Install;

namespace CACCCheckInServiceLibraryHost
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
