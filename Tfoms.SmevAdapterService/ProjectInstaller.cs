using System.ComponentModel;
using System.Configuration.Install;

namespace Tfoms.SmevAdapterService
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
