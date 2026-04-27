using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace XKCB
{
    public sealed partial class DetailsPage : Page
    {
        private readonly ApiClient _apiClient = new ApiClient();

        public DetailsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string mediaId)
            {
                var details = await _apiClient.GetDetailsAsync(mediaId);

                if (details != null)
                {
                    TitleText.Text = details.Title;
                    YearText.Text = details.Year;
                    RatingText.Text = $"★ {(details.Rating.ToString() ?? "N/A")}";

                    if (!string.IsNullOrEmpty(details.BackdropUrl))
                    {
                        BackdropImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(details.BackdropUrl));
                    }

                    if (!string.IsNullOrEmpty(details.PosterUrl))
                    {
                        PosterImageBrush.ImageSource = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(details.PosterUrl));
                    }

                    if (details.Type == "movie")
                    {
                        SourcesSection.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        SeasonsSection.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

                        if (details.Sources != null)
                        {
                            SourcesList.ItemsSource = details.Sources;
                        }
                    }
                    else if (details.Type == "series")
                    {
                        SourcesSection.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        SeasonsSection.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

                        // if (details.Seasons != null) { SeasonsList.ItemsSource = details.Seasons; }
                    }
                }
            }
        }

        private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
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