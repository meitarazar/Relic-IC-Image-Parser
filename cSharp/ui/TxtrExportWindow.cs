using System;
using System.Windows;
using System.Windows.Media;

namespace Relic_IC_Image_Parser.cSharp.ui
{
    /// <summary>
    /// Interaction logic for TxtrExportWindow.xaml
    /// </summary>
    public partial class TxtrExportWindow : Window
    {
        public string txtrFilePath;
        
        public TxtrExportWindow(string fileName)
        {
            InitializeComponent();

            TextBoxFileName.Text = fileName;
        }

        /// <summary>
        /// We need to set the default path and prevent any changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioDefault_Checked(object sender, RoutedEventArgs e)
        {
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
            // here we check if the path ends with a slash char
            string path = TextBoxPath.Text;
            if (!path.Substring(path.Length - 1).Equals("/"))
            {
                // if we don't, mark the path text box with red border for error
                TextBoxPath.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }

            // everything is fine, make sure the dialog returns true and set the TxtrFilePath
            DialogResult = true;
            txtrFilePath = TextBoxPath.Text + TextBoxFileName.Text;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // canceling, make sure the dialog returns false and blank the TxtrFilePath
            DialogResult = false;
            txtrFilePath = null;
            Close();
        }
    }
}
