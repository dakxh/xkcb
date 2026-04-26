using System;
using System.IO;

namespace XKCB
{
    public static class AppLogger
    {
        // This puts the log file directly in the folder where your .exe lives
        private static readonly string LogFile = Path.Combine(AppContext.BaseDirectory, "xkcb_crash.log");

        public static void Log(string message)
        {
            try
            {
                var logLine = $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n";

                // Write to Visual Studio's Output window
                System.Diagnostics.Debug.WriteLine(logLine);

                // Write to the physical text file
                File.AppendAllText(LogFile, logLine);
            }
            catch
            {
                // Fail silently so the logger itself doesn't cause a crash
            }
        }
    }
}