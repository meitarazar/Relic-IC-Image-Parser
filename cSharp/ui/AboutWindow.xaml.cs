using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            InitializeComponent();

            Title += App.AppName;

            TextBoxVersion.Text = App.AppName + " - v" + App.VersionName;
        }

        private void LaunchLink(string uri)
        {
            Process.Start(new ProcessStartInfo(uri));
        }

        private void Hyperlink_Request(object sender, RequestNavigateEventArgs e)
        {
            LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
