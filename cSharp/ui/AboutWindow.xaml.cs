using Relic_IC_Image_Parser.cSharp.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Relic_IC_Image_Parser.cSharp.ui
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
