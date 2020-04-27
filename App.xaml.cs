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
        private static Assembly ExeAssembly = Assembly.GetExecutingAssembly();
        public static readonly string AppName = ExeAssembly.GetName().Name;
        private static readonly System.Version Version = ExeAssembly.GetName().Version;
        public static readonly string VersionName = Version.Major + "." + Version.Minor + "." + Version.Build;

        private List<string> notExistFiles = new List<string>();
        private List<string> existFiles = new List<string>();

        public static int editorsOpen = 0;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoadArguments(e.Args);

            CheckNoTExistFiles();

            LoadExistFilesOrOpenLauncher();
        }

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

        private void CheckNoTExistFiles()
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

        private void LoadExistFilesOrOpenLauncher()
        {
            if (existFiles.Count > 0)
            {
                // Open editor window for each file
                foreach (string file in existFiles)
                {
                    EditorWindow editor = new EditorWindow(file);
                    editor.Show();
                }
            }
            else
            {
                LaunchWindow launcher = new LaunchWindow();
                launcher.Show();
            }
        }
    }
}
