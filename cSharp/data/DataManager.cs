using Relic_IC_Image_Parser.cSharp.util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using static Relic_IC_Image_Parser.cSharp.imaging.ImageManager;

namespace Relic_IC_Image_Parser.cSharp.data
{
    /// <summary>
    /// Class that wraps all files and settings related operations
    /// </summary>
    class DataManager
    {
        // The divider used in the method 'PopulateRecentFilesList' down below so the list in the launcher will look more readable.
        private const string ITEM_INNER_DIVIDER = "\n  ";

        // The keys used in the App.config file to define the app settings
        private const string SETTINGS_RECENT_FILES_COUNT = "recentFilesCount";
        private const string SETTINGS_RECENT_FILES = "recentFiles";
        private const string SETTINGS_RELIC_IMAGE_DPI = "relicImageDpi";
        private const string SETTINGS_RETURN_TO_LAUNCHER = "alwaysReturnToLauncher";
        private const string SETTINGS_LOG_TAKE = "logTake";
        private const string SETTINGS_LOG_PATH = "logPath";
        private const string SETTINGS_LOG_FILE = "logFile";

        // Static links to the app's configuration file
        private static readonly Configuration configuration = ConfigurationManager.OpenExeConfiguration(AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName);
        private static readonly AppSettingsSection appSettings = configuration.AppSettings;

        // Local variables taken from the app's configuration file
        private static readonly int recentFilesCount = int.Parse(appSettings.Settings[SETTINGS_RECENT_FILES_COUNT].Value);
        private static readonly List<string> recentFiles = new List<string>(appSettings.Settings[SETTINGS_RECENT_FILES].Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
        public static readonly double relicImageDpi = double.Parse(appSettings.Settings[SETTINGS_RELIC_IMAGE_DPI].Value);
        public static readonly bool alwaysReturnToLauncher = bool.Parse(appSettings.Settings[SETTINGS_RETURN_TO_LAUNCHER].Value);
        public static readonly bool logTake = bool.Parse(appSettings.Settings[SETTINGS_LOG_TAKE].Value);
        public static readonly string logPath = appSettings.Settings[SETTINGS_LOG_PATH].Value;
        public static readonly string logFile = appSettings.Settings[SETTINGS_LOG_FILE].Value;

        // Custom init OpenFileDialog
        private static readonly OpenFileDialog fileChooser;

        /// <summary>
        /// Static method for init complex variables
        /// </summary>
        static DataManager()
        {
            //Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Inithializing");

            fileChooser = new OpenFileDialog
            {
                Title = "Choose an Image File",

                Filter = "All supported formats (*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.spt;*.txr;*.tiff)|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.spt;*.txr;*.tiff|" +
                "Bitmap (*.bmp)|*.bmp|" +
                "Graphics Interchange Format (*.gif)|*.gif|" +
                "Joint Photographic Experts Group (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphics (*.png)|*.png|" +
                "Relic Split Image format (*.spt)|*.spt|" +
                "Relic Texture format (*.txr)|*.txr|" +
                "Tagged Image File Format (*.tiff)|*.tiff|" +
                "All files (*.*)|*.*",

                CheckFileExists = true,

                CheckPathExists = true,

                Multiselect = true
            };
            //Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "[OpenFileDialog]: " + fileChooser.ToString());
        }

        /// <summary>
        /// Receives the UI list from the launcher window (<see cref="ListBox"/>) and fills it with the recent files opend by the app.
        /// </summary>
        /// <param name="listBox">The <see cref="ListBox"/> to populate.</param>
        public static void PopulateRecentFilesList(System.Windows.Controls.ListBox listBox)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Populating recent files list...");

            List<string> items = new List<string>(recentFiles.Count);
            foreach (string item in recentFiles)
            {
                FileInfo info = new FileInfo(item);

                // takes the file info and breakes the Name and Path using our divider
                items.Add(info.Name + ITEM_INNER_DIVIDER + info.DirectoryName);

            }
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Recent files: " + string.Join(", ", items));

            // Set the list's new items
            listBox.ItemsSource = items;
        }

        /// <summary>
        /// Called when a list item at the launcher window is pressed.
        /// <para>The method takes the text value of the item and reconstruct the full file path from it.</para>
        /// <para>If the file exists, opening a new <see cref="EditorWindow"/> with the selected file.</para>
        /// <para>If the file does not exists, it prompts a message box to ask you if you want to remove it from the list.
        /// If you do, it removes the file from the settings <see cref="recentFiles"/> reference, save the new settings to the config and update the list.</para>
        /// </summary>
        /// <param name="listBox">The list object that was interacted.</param>
        /// <param name="itemContent">The item' Text field string.</param>
        public static void SelectFromRecentFilesList(System.Windows.Controls.ListBox listBox, string itemContent)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Recent file was selected");

            // reconstructs the full file path
            string[] item = itemContent.Split(new string[] { ITEM_INNER_DIVIDER }, StringSplitOptions.None);
            string fullName = item[1] + "\\" + item[0];
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File path: " + fullName);

            if (File.Exists(fullName))
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File exists.");
                PopFileToRecentTop(fullName);
                OpenEditorWindow(Window.GetWindow(listBox), fullName);
            }
            else
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File does not exists!");

                // prompt a message box to ask the user
                // do you want to remove the file from recent files list?
                string msg = "The file:\n" +
                    fullName + "\n\n" +
                    "Does not exist, would you like to remove it from the list?";
                MessageBoxResult result = System.Windows.MessageBox.Show(msg, App.AppName, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Removing not existing file");

                    recentFiles.Remove(fullName);
                    appSettings.Settings[SETTINGS_RECENT_FILES].Value = string.Join(";", recentFiles);
                    configuration.Save();
                    PopulateRecentFilesList(listBox);
                }
            }
        }

        /// <summary>
        /// Used to open a new Editor window with the provided file path.
        /// <para>If the calling window is the Launcher window, close it.</para>
        /// </summary>
        /// <param name="window">The calling window.</param>
        /// <param name="fullFileName">The file path to open in the editor.</param>
        private static void OpenEditorWindow(Window window, string fullFileName)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Openning EditorWindow...");
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File path: " + fullFileName);

            // append another editor window to the count
            App.editorsOpen++;

            // actual open
            EditorWindow editor = new EditorWindow(fullFileName);
            editor.Show();

            // close if launcher
            if (window is LaunchWindow)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Closing LaunchWindow");
                window.Close();
            }
        }

        /// <summary>
        /// Take the provided file path and put it in the top of recent files.
        /// </summary>
        /// <param name="fileName">The file path to put on top of the list.</param>
        private static void PopFileToRecentTop(string fileName)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Puting file at recent files top...");
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File path: " + fileName);

            // if we already contains it, remove it to reposition on top
            if (recentFiles.Contains(fileName))
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "The file exists in the list, removing...");
                recentFiles.Remove(fileName);
            }

            // if we have exceeded the count limit, remove the last one
            else if (recentFiles.Count == recentFilesCount)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Max capacity has been reached, removing last file...");
                recentFiles.RemoveAt(recentFiles.Count - 1);
            }

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Puting file at the top");
            // put the file on top
            recentFiles.Insert(0, fileName);

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Updating settings");
            // update settings and save
            appSettings.Settings[SETTINGS_RECENT_FILES].Value = string.Join(";", recentFiles);
            configuration.Save();
        }

        /// <summary>
        /// Open a File Chooser Dialog.
        /// </summary>
        /// <param name="window">The calling window.</param>
        public static void OpenNewFile(Window window)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Opening file chooser dialog...");

            DialogResult result = fileChooser.ShowDialog();
            if (result == DialogResult.OK)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Dialog return OK");

                // if dialog returns OK open file in editor
                string fileName = fileChooser.FileName;
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File name: " + fileName);

                PopFileToRecentTop(fileName);
                OpenEditorWindow(window, fileName);
                return;
            }

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Dialog return: " + result.ToString());
        }

        /// <summary>
        /// Open a File From Provided Argument.
        /// </summary>
        /// <param name="fullFileName">The file path to open.</param>
        public static void OpenArgFile(string fullFileName)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Opening argument file...");
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File name: " + fullFileName);

            PopFileToRecentTop(fullFileName);
            OpenEditorWindow(null, fullFileName);
        }

        /// <summary>
        /// Open a new Save File Dialog based on the export type provided.
        /// </summary>
        /// <param name="exportType">The export type of the file.</param>
        /// <param name="fileName">The original file name.</param>
        /// <returns>If the dialog returns OK return file path, otherwise return null.</returns>
        public static string SaveFile(ExportType exportType, string fileName)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Preparing for file save...");

            // remove the extention from file name
            string[] fileNameSplit = fileName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            string filenameNoExt = string.Join(".", fileNameSplit, 0, fileNameSplit.Length - 1);
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File name without extension: " + filenameNoExt);

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Export Image File",
                FileName = filenameNoExt,
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
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Filter: " + saveFileDialog.Filter);

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Opening save file dialog...");
            DialogResult result = saveFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Dialog return OK");
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File path: " + saveFileDialog.FileName);
                return saveFileDialog.FileName;
            }

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Dialog return: " + result.ToString());
            return null;
        }
    }
}
