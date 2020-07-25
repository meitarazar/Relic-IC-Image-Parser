using Relic_IC_Image_Parser.cSharp.data;
using Relic_IC_Image_Parser.cSharp.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Relic_IC_Image_Parser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // global app name and version
        private static Assembly ExeAssembly = Assembly.GetExecutingAssembly();
        public static readonly string AppName = ExeAssembly.GetName().Name;
        private static readonly System.Version Version = ExeAssembly.GetName().Version;
        public static readonly string VersionName = Version.Major + "." + Version.Minor + "." + Version.Build;

        // variables for handeling files as arguments
        private List<string> notExistFiles = new List<string>();
        private List<string> existFiles = new List<string>();

        /// <summary>
        /// Used to keep watch on how the app was opened
        /// </summary>
        public static bool openedFromArgs = false;

        /// <summary>
        /// Used to keep watch on how many Editors are open
        /// </summary>
        public static int editorsOpen = 0;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Logger.InitLog();

            LoadArguments(e.Args);

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Not existing files: " + string.Join(", ", notExistFiles));
            CheckNotExistFiles();

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Existing files: " + string.Join(", ", existFiles));
            LoadExistFilesOrOpenLauncher();
        }

        /// <summary>
        /// Loading the files to their appropriate list.
        /// </summary>
        /// <param name="args">The arguments array received.</param>
        private void LoadArguments(string[] args)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Loading args...");
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Args: " + string.Join(", ", args));
            foreach (string file in args)
            {
                if (File.Exists(file))
                {
                    existFiles.Add(file);
                }
                else
                {
                    notExistFiles.Add(file);
                }
            }
        }

        /// <summary>
        /// Prompting to the user about the files that do not exists and thus won't be opened
        /// </summary>
        private void CheckNotExistFiles()
        {
            if (notExistFiles.Count > 0)
            {
                string title = AppName;
                string msg = "These files were not found or they are folders!" + "\n" +
                    "(check syntax, maybe you wrote something wrong...)" + "\n\n" +
                    "- " + string.Join("\n- ", notExistFiles);

                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Load all existing files to Editors.
        /// <para>Or open Launcher if there are none.</para>
        /// </summary>
        private void LoadExistFilesOrOpenLauncher()
        {
            if (existFiles.Count > 0)
            {
                // mark that we have been opned from args
                openedFromArgs = true;
                
                // open editor window for each file
                foreach (string file in existFiles)
                {
                    DataManager.OpenArgFile(file);
                }
            }

            // if no existing files were provided, open Launcher
            else
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Opening LaunchWindow...");

                LaunchWindow launcher = new LaunchWindow();
                launcher.Show();
            }
        }
    }
}
