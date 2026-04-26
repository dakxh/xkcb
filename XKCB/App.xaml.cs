using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace XKCB
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        // 1. We make the Window public and static so the Player can find it
        public static Window MainWindow { get; private set; } = null!;

        public App()
        {
            this.InitializeComponent();

            // Clear the old log file every time the app boots up
            var logPath = System.IO.Path.Combine(AppContext.BaseDirectory, "xkcb_crash.log");
            if (System.IO.File.Exists(logPath)) System.IO.File.Delete(logPath);

            AppLogger.Log("=== XKCB ENGINE STARTED ===");

            // 1. Trap UI Thread Crashes
            this.UnhandledException += (s, e) =>
            {
                AppLogger.Log($"FATAL APP CRASH: {e.Exception.Message}\n{e.Exception.StackTrace}");
            };

            // 2. Trap Background/Network Thread Crashes
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                AppLogger.Log($"FATAL BACKGROUND CRASH: {e.Exception.Message}\n{e.Exception.StackTrace}");
            };
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // 2. We assign our MainWindow here instead of the default m_window
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
