using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Mpv.NET.API;
using System;
using XKCB;

namespace XKCB
{
    public sealed partial class PlayerPage : Page
    {
        private readonly ApiClient _apiClient = new ApiClient();
        private Mpv.NET.API.Mpv? _mpv;

        public PlayerPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string streamId)
            {
                var streamData = await _apiClient.GetStreamAsync(streamId);

                if (streamData != null && !string.IsNullOrEmpty(streamData.HlsManifestUrl))
                {
                    string finalUrl = streamData.HlsManifestUrl;

                    // If it's a Hugging Face URL, route it through our WinUI 3 proxy
                    if (finalUrl.Contains("huggingface.co"))
                    {
                        LocalHlsProxy.Start();
                        finalUrl = finalUrl.Replace("https://huggingface.co", "http://127.0.0.1:54321");
                    }

                    InitializePlayer(finalUrl);
                }
            }
        }

        private void InitializePlayer(string manifestUrl)
        {
            try
            {
                AppLogger.Log("Initializing libmpv C-Engine in Detached Mode...");

                // 1. Force the absolute path to the .exe folder
                string dllPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "libmpv-2.dll");
                AppLogger.Log($"Looking for engine at: {dllPath}");

                // 2. Initialize with the absolute path
                _mpv = new Mpv.NET.API.Mpv(dllPath);

                // 3. Attach logs
                _mpv.LogMessage += Mpv_LogMessage;
                _mpv.RequestLogMessages(Mpv.NET.API.MpvLogLevel.Debug);

                AppLogger.Log("Configuring mpv properties...");

                // --- ENABLE NATIVE MPV UI & CONTROLS ---
                // Reactivate the internal Lua On-Screen Controller
                _mpv.SetPropertyString("osc", "yes");

                // Bind standard mpv hotkeys (Space to pause, arrows to seek, etc.)
                _mpv.SetPropertyString("input-default-bindings", "yes");

                // Ensure the detached window captures keyboard/mouse inputs directly
                _mpv.SetPropertyString("input-vo-keyboard", "yes");
                // ---------------------------------------

                _mpv.SetPropertyString("force-window", "yes");
                _mpv.SetPropertyString("ontop", "yes");
                _mpv.SetPropertyString("keep-open", "yes");
                _mpv.SetPropertyString("autofit", "1280x720");
                _mpv.SetPropertyString("hwdec", "auto");

                // Optional: Give the pop-out window a clean title
                _mpv.SetPropertyString("title", "XKCB Native Player");

                AppLogger.Log($"Loading Manifest: {manifestUrl}");
                _mpv.Command("loadfile", manifestUrl);
            }
            catch (System.Exception ex)
            {
                AppLogger.Log($"!!! ENGINE FATAL ERROR !!!");
                AppLogger.Log($"Message: {ex.Message}");
                AppLogger.Log($"StackTrace: {ex.StackTrace}");
            }
        }

        // 3. This event fires hundreds of times a second as the engine runs
        private void Mpv_LogMessage(object? sender, Mpv.NET.API.MpvLogMessageEventArgs e)
        {
            // We log exactly what mpv is doing at the hardware level
            AppLogger.Log($"[MPV {e.Message.LogLevel}] {e.Message.Prefix}: {e.Message.Text.TrimEnd()}");
        }

        private void CloseButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Destroy the C engine to free memory before leaving
            if (_mpv != null)
            {
                _mpv.Command("stop");
                _mpv.Dispose();
            }

            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}