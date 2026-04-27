using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace XKCB
{
    public static class LocalHlsProxy
    {
        private static HttpListener? _listener;
        private static readonly HttpClient _httpClient = new HttpClient();
        private static bool _isRunning = false;

        public static void Start()
        {
            if (_isRunning) return;
            _isRunning = true;

            _listener = new HttpListener();
            // 127.0.0.1 bypasses strict Windows localhost ACL requirements
            _listener.Prefixes.Add("http://127.0.0.1:54321/");
            _listener.Start();

            Task.Run(ListenAsync);
        }

        private static async Task ListenAsync()
        {
            while (_isRunning && _listener != null)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = ProcessRequestAsync(context);
                }
                catch { /* Ignore shutdown exceptions */ }
            }
        }

        private static async Task ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                string path = context.Request.RawUrl ?? "";

                // Intercept the proxy path
                if (path.Contains("/xkca/") && !path.Contains("/resolve/"))
                {
                    path = path.Replace("/xkca/", "/xkca/resolve/");
                }

                string targetUrl = "https://huggingface.co" + path;

                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, targetUrl);
                using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                context.Response.StatusCode = (int)response.StatusCode;

                // Identify if the file is a manifest
                bool isM3u8 = targetUrl.EndsWith(".m3u8") ||
                              (response.Content.Headers.ContentType?.MediaType?.Contains("mpegurl") == true);

                if (isM3u8)
                {
                    context.Response.ContentType = "application/vnd.apple.mpegurl";
                    string manifestText = await response.Content.ReadAsStringAsync();

                    // --- THE PROXY ESCAPE FIX ---
                    // 1. Force all sub-manifests and video chunks to route back to THIS local proxy
                    // This prevents the engine from escaping out to the real internet domain
                    manifestText = manifestText.Replace("https://huggingface.co", "http://127.0.0.1:54321");

                    // 2. Inject the missing /resolve/ directory for Hugging Face
                    manifestText = manifestText.Replace("/xkca/resolve/", "/xkca/"); // clean up to prevent doubling
                    manifestText = manifestText.Replace("/xkca/", "/xkca/resolve/");
                    // ----------------------------

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(manifestText);
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    // For raw video chunks (.ts), stream them directly
                    if (response.Content.Headers.ContentType != null)
                    {
                        context.Response.ContentType = response.Content.Headers.ContentType.ToString();
                    }
                    if (response.Content.Headers.ContentLength.HasValue)
                    {
                        context.Response.ContentLength64 = response.Content.Headers.ContentLength.Value;
                    }

                    using var stream = await response.Content.ReadAsStreamAsync();
                    await stream.CopyToAsync(context.Response.OutputStream);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Log($"Proxy Error: {ex.Message}");
                context.Response.StatusCode = 500;
            }
            finally
            {
                context.Response.Close();
            }
        }

        public static void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
            _listener?.Close();
        }
    }
}