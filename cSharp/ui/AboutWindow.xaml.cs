using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace Relic_IC_Image_Parser
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Inithializing");

            InitializeComponent();

            Title += App.AppName;

            VersionBlock.Text = App.AppName + " - v" + App.VersionName;

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "About " + VersionBlock.Text);
        }

        private void LaunchLink(string uri)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Opening link: " + uri);
            Process.Start(new ProcessStartInfo(uri));
        }

        private void Hyperlink_Request(object sender, RequestNavigateEventArgs e)
        {
            LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
