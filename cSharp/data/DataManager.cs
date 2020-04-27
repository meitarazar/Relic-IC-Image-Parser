using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using static Relic_IC_Image_Parser.cSharp.imaging.ImageManager;

namespace Relic_IC_Image_Parser.cSharp.data
{
    class DataManager
    {
        private const string ITEM_INNER_DIVIDER = "\n  ";

        private const string SETTINGS_RECENT_FILES_COUNT = "recentFilesCount";
        private const string SETTINGS_RECENT_FILES = "recentFiles";
        private const string SETTINGS_RELIC_IMAGE_DPI = "relicImageDpi";
        private const string SETTINGS_LOG_TAKE = "logTake";
        private const string SETTINGS_LOG_PATH = "logPath";
        private const string SETTINGS_LOG_FILE = "logFile";

        private static readonly Configuration configuration = ConfigurationManager.OpenExeConfiguration(AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName);

        private static readonly AppSettingsSection appSettings = configuration.AppSettings;

        private static readonly int recentFilesCount = int.Parse(appSettings.Settings[SETTINGS_RECENT_FILES_COUNT].Value);
        private static readonly List<string> recentFiles = new List<string>(appSettings.Settings[SETTINGS_RECENT_FILES].Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
        public static readonly double relicImageDpi = double.Parse(appSettings.Settings[SETTINGS_RELIC_IMAGE_DPI].Value);
        private static readonly bool logTake = bool.Parse(appSettings.Settings[SETTINGS_LOG_TAKE].Value);
        private static readonly string logPath = appSettings.Settings[SETTINGS_LOG_PATH].Value;
        private static readonly string logFile = appSettings.Settings[SETTINGS_LOG_FILE].Value;

        private static readonly OpenFileDialog fileChooser;

        static DataManager()
        {
            fileChooser = new OpenFileDialog
            {
                Title = "Choose an Image File",
                //Filter = "All supported formats (*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.spt;*.txr;*.tiff;*.tga)|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.spt;*.txr;*.tiff;*.tga|" +
                Filter = "All supported formats (*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.spt;*.txr;*.tiff)|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.spt;*.txr;*.tiff|" +
                "Bitmap (*.bmp)|*.bmp|" +
                "Graphics Interchange Format (*.gif)|*.gif|" +
                "Joint Photographic Experts Group (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphics (*.png)|*.png|" +
                "Relic Split Image format (*.spt)|*.spt|" +
                "Relic Texture format (*.txr)|*.txr|" +
                "Tagged Image File Format (*.tiff)|*.tiff|" +
                //"Truevision Graphics Adapter (*.tga)|*.tga|" +
                "All files (*.*)|*.*",

                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = true
            };
        }

        public static void PopulateRecentFilesList(System.Windows.Controls.ListBox listBox)
        {
            List<string> items = new List<string>(recentFiles.Count);
            foreach (string item in recentFiles)
            {
                FileInfo info = new FileInfo(item);
                items.Add(info.Name + ITEM_INNER_DIVIDER + info.DirectoryName);

            }
            listBox.ItemsSource = items;
        }

        public static void SelectFromRecentFilesList(System.Windows.Controls.ListBox listBox, string itemContent)
        {
            string[] item = itemContent.Split(new string[] { ITEM_INNER_DIVIDER }, StringSplitOptions.None);
            string fullName = item[1] + "\\" + item[0];
            if (File.Exists(fullName))
            {
                PopFileToRecentTop(fullName);
                OpenEditorWindow(Window.GetWindow(listBox), fullName);
            }
            else
            {
                string msg = "The file:\n" +
                    fullName + "\n\n" +
                    "Does not exist, would you like to remove it from the list?";
                MessageBoxResult result = System.Windows.MessageBox.Show(msg, App.AppName, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    recentFiles.Remove(fullName);
                    appSettings.Settings[SETTINGS_RECENT_FILES].Value = string.Join(";", recentFiles);
                    configuration.Save();
                    PopulateRecentFilesList(listBox);
                }
            }
        }

        private static void OpenEditorWindow(Window window, string fullFileName)
        {
            App.editorsOpen++;
            EditorWindow editor = new EditorWindow(fullFileName);
            editor.Show();

            if (window is LaunchWindow)
            {
                window.Close();
            }
        }

        private static void PopFileToRecentTop(string fileName)
        {
            if (recentFiles.Contains(fileName))
            {
                recentFiles.Remove(fileName);
            }
            else if (recentFiles.Count == recentFilesCount)
            {
                recentFiles.RemoveAt(recentFiles.Count - 1);
            }
            recentFiles.Insert(0, fileName);
            appSettings.Settings[SETTINGS_RECENT_FILES].Value = string.Join(";", recentFiles);
            configuration.Save();
        }

        public static void OpenFile(Window window)
        {
            if (fileChooser.ShowDialog() == DialogResult.OK)
            {
                string fileName = fileChooser.FileName;
                PopFileToRecentTop(fileName);
                OpenEditorWindow(window, fileName);
            }
        }

        public static string SaveFile(ExportType exportType)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Export Image File",
                CheckFileExists = false,
                CheckPathExists = true,
                AddExtension = true
            };

            switch (exportType)
            {
                case ExportType.BMP:
                    saveFileDialog.Filter = "Bitmap (*.bmp)|*.bmp";
                    break;
                case ExportType.GIF:
                    saveFileDialog.Filter = "Graphics Interchange Format (*.gif)|*.gif";
                    break;
                case ExportType.JPG:
                    saveFileDialog.Filter = "Joint Photographic Experts Group (*.jpg;*.jpeg)|*.jpg;*.jpeg";
                    break;
                case ExportType.PNG:
                    saveFileDialog.Filter = "Portable Network Graphics (*.png)|*.png";
                    break;
                case ExportType.TIFF:
                    saveFileDialog.Filter = "Tagged Image File Format (*.tiff)|*.tiff";
                    break;
                case ExportType.SPT:
                    saveFileDialog.Filter = "Relic Split Image format (*.spt)|*.spt";
                    break;
                case ExportType.TXR:
                    saveFileDialog.Filter = "Relic Texture format (*.txr)|*.txr";
                    break;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }

            return null;
        }
    }
}
