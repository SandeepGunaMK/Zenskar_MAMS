using System.Configuration;
using System.Data;
using System.Windows;
using System.Net; // Add this using directive

namespace Zenskar_MAMS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
    }
}
