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
        /// Used to keep watch on how many Editors are open
        /// </summary>
        public static int editorsOpen = 0;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoadArguments(e.Args);

            CheckNotExistFiles();

            LoadExistFilesOrOpenLauncher();
        }

        /// <summary>
        /// Loading the files to their appropriate list.
        /// </summary>
        /// <param name="args">The arguments array received.</param>
        private void LoadArguments(string[] args)
        {
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
                // open editor window for each file
                foreach (string file in existFiles)
                {
                    EditorWindow editor = new EditorWindow(file);
                    editor.Show();
                }
            }

            // if no existing files were provided, open Launcher
            else
            {
                LaunchWindow launcher = new LaunchWindow();
                launcher.Show();
            }
        }
    }
}
