using Relic_IC_Image_Parser.cSharp.data;
using Relic_IC_Image_Parser.cSharp.util;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Relic_IC_Image_Parser
{
    /// <summary>
    /// Interaction logic for LaunchWindow.xaml
    /// </summary>
    public partial class LaunchWindow : Window
    {
        public LaunchWindow()
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Inithializing");

            InitializeComponent();

            InitSelf();

            DataManager.PopulateRecentFilesList(RecentItems);
        }

        /// <summary>
        /// Just make some cosmetic preparations before showing itself
        /// </summary>
        private void InitSelf()
        {
            Title = App.AppName;

            TextBox.Text = "Welcome!" + "\n\n" +
                "This is a simple tool built to work with Relic's SPT and TXR files." + "\n" +
                "   (based on Impossible Creature's files)" + "\n\n" +
                "You can:" + "\n" +
                "   - View the inner images and the full image that they construct" + "\n" +
                "   - Convert the full image to a known image format (like PNG duhhh)" + "\n" +
                "   - Create a new SPT or TXR file from a known image format (YEAH!!!)";
            InnerTitle.Text = App.AppName + " (v" + App.VersionName + ")";
        }

        /// <summary>
        /// Start to react to drag move after a mouse press.
        /// </summary>
        /// <param name="sender">The interacted object.</param>
        /// <param name="e">The event data.</param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        /// <summary>
        /// If the user wants to open a file, who are we to say no?
        /// </summary>
        /// <param name="sender">The interacted object.</param>
        /// <param name="e">The event data.</param>
        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Opening new file...");
            DataManager.OpenFile(this);
        }

        /// <summary>
        /// The user pressed on an item from the recent files list, we sould do something about it...
        /// </summary>
        /// <param name="sender">The interacted object.</param>
        /// <param name="e">The event data.</param>
        private void RecentItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Opening file from recent...");
            if (e.AddedItems.Count > 0)
            {
                DataManager.SelectFromRecentFilesList(RecentItems, (string)e.AddedItems[0]);
            }
        }

        /// <summary>
        /// Close! Close the damn thing!
        /// </summary>
        /// <param name="sender">The interacted object.</param>
        /// <param name="e">The event data.</param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Close.");
            Close();
        }
    }
}
