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
                    // Pass the entire streamData object so we have access to the subs array
                    InitializePlayer(streamData);
                }
            }
        }

        private void InitializePlayer(WatchResponse streamData)
        {
            try
            {
                AppLogger.Log("Initializing libmpv C-Engine in Detached Mode...");

                string dllPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "libmpv-2.dll");
                _mpv = new Mpv.NET.API.Mpv(dllPath);

                _mpv.LogMessage += Mpv_LogMessage;
                _mpv.RequestLogMessages(Mpv.NET.API.MpvLogLevel.Debug);

                _mpv.SetPropertyString("osc", "yes");
                _mpv.SetPropertyString("input-default-bindings", "yes");
                _mpv.SetPropertyString("input-vo-keyboard", "yes");
                _mpv.SetPropertyString("force-window", "yes");
                _mpv.SetPropertyString("ontop", "yes");
                _mpv.SetPropertyString("keep-open", "yes");
                _mpv.SetPropertyString("autofit", "1280x720");
                _mpv.SetPropertyString("hwdec", "auto");
                _mpv.SetPropertyString("title", "XKCB Native Player");

                // --- NEW: Hook into the FileLoaded event ---
                _mpv.FileLoaded += (sender, args) =>
                {
                    if (streamData.AvailableSubs != null && streamData.AvailableSubs.Count > 0)
                    {
                        AppLogger.Log($"Video timeline established. Injecting {streamData.AvailableSubs.Count} subtitle tracks...");
                        foreach (var sub in streamData.AvailableSubs)
                        {
                            if (!string.IsNullOrEmpty(sub.Url))
                            {
                                string title = !string.IsNullOrEmpty(sub.Language) ? sub.Language : "Unknown";

                                try
                                {
                                    // 'auto' loads it into the track list without forcing it on.
                                    // Passing the title twice populates both the 'Title' and 'Language' metadata in the UI.
                                    _mpv.Command("sub-add", sub.Url, "auto", title, title);
                                    AppLogger.Log($"Added subtitle: {title} ({sub.Url})");
                                }
                                catch (Exception subEx)
                                {
                                    AppLogger.Log($"Failed to add subtitle {title}: {subEx.Message}");
                                }
                            }
                        }
                    }
                };
                // -------------------------------------------

                AppLogger.Log($"Loading Manifest directly: {streamData.HlsManifestUrl}");
                _mpv.Command("loadfile", streamData.HlsManifestUrl);
            }
            catch (System.Exception ex)
            {
                AppLogger.Log($"!!! ENGINE FATAL ERROR !!!");
                AppLogger.Log($"Message: {ex.Message}");
                AppLogger.Log($"StackTrace: {ex.StackTrace}");
            }
        }

        private void Mpv_LogMessage(object? sender, Mpv.NET.API.MpvLogMessageEventArgs e)
        {
            AppLogger.Log($"[MPV {e.Message.LogLevel}] {e.Message.Prefix}: {e.Message.Text.TrimEnd()}");
        }

        private void CloseButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
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