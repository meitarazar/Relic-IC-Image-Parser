using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace Relic_IC_Image_Parser
{
    /// <summary>
    /// Interaction logic for TxtrExportWindow.xaml
    /// </summary>
    public partial class TxtrExportWindow : Window
    {
        public string txtrFilePath;
        
        public TxtrExportWindow(string fileName)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Inithializing");

            InitializeComponent();

            TextBoxFileName.Text = fileName;
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File name: " + fileName);
        }

        /// <summary>
        /// We need to set the default path and prevent any changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioDefault_Checked(object sender, RoutedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Texture path: Art/Textures/");

            if (TextBoxPath != null)
            {
                TextBoxPath.Text = "Art/Textures/";
                TextBoxPath.IsEnabled = false;
            }
        }

        /// <summary>
        /// Here we enable the option to edit that path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioCustom_Checked(object sender, RoutedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Texture path: [custom]");

            if (TextBoxPath != null) {
                TextBoxPath.IsEnabled = true;
            }
        }

        /// <summary>
        /// If the text have bee changed, return to the default border, just in case we have came back from an error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxPath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBoxPath.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
        }

        /// <summary>
        /// Logic for pressing the OK button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Testing path...");

            // here we check if the path ends with a slash char
            string path = TextBoxPath.Text;
            if (!path.Substring(path.Length - 1).Equals("/"))
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Path not ending with '/'");

                // if we don't, mark the path text box with red border for error
                TextBoxPath.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }

            // everything is fine, make sure the dialog returns true and set the TxtrFilePath
            DialogResult = true;
            txtrFilePath = "Data:" + TextBoxPath.Text + TextBoxFileName.Text + ".txr";
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Texture full path: " + txtrFilePath);
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Cancel.");

            // canceling, make sure the dialog returns false and blank the TxtrFilePath
            DialogResult = false;
            txtrFilePath = null;
            Close();
        }
    }
}
