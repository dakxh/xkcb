using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace XKCB
{
    public sealed partial class DetailsPage : Page
    {
        private readonly ApiClient _apiClient = new ApiClient();

        public DetailsPage()
        {
            this.InitializeComponent();
        }

        // This runs automatically when we navigate to this page
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // We expect the Media ID to be passed as a string parameter
            if (e.Parameter is string mediaId)
            {
                var details = await _apiClient.GetDetailsAsync(mediaId);

                if (details != null)
                {
                    // Populate the UI
                    TitleText.Text = details.Title;
                    YearText.Text = details.Year;

                    if (!string.IsNullOrEmpty(details.BackdropUrl))
                    {
                        BackdropImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new System.Uri(details.BackdropUrl));
                    }

                    // Bind the sources (If it's a movie)
                    if (details.Type == "movie" && details.Sources != null)
                    {
                        SourcesList.ItemsSource = details.Sources;
                    }
                }
            }
        }

        private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Navigate back if possible
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void PlayButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string streamId)
            {
                Frame.Navigate(typeof(PlayerPage), streamId);
            }
        }
    }
}