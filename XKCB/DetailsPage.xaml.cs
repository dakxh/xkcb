using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace XKCB
{
    public sealed partial class DetailsPage : Page
    {
        private readonly ApiClient _apiClient = new ApiClient();

        // Internal State Management for the Drill-Down Layout
        private MediaDetails? _mediaDetails;
        private int _activeSeasonNumber = -1;

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
                        BackdropImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(details.BackdropUrl));

                    if (!string.IsNullOrEmpty(details.PosterUrl))
                        PosterImageBrush.ImageSource = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(details.PosterUrl));

                    if (details.Type == "movie")
                    {
                        SourcesSection.Visibility = Visibility.Visible;
                        SeasonsSection.Visibility = Visibility.Collapsed;

                        if (details.Sources != null) SourcesList.ItemsSource = details.Sources;
                    }
                    else if (details.Type == "series")
                    {
                        SourcesSection.Visibility = Visibility.Collapsed;
                        SeasonsSection.Visibility = Visibility.Visible;

                        if (details.Seasons != null && details.Seasons.Count > 0)
                        {
                            _mediaDetails = details;
                            SeasonsList.ItemsSource = details.Seasons;

                            // Automatically drill-down into the first available season
                            SelectSeason(details.Seasons[0].SeasonNumber);
                        }
                    }
                }
            }
        }

        // --- MASTER-DETAIL INTERACTION LOGIC ---

        private void SelectSeason(int seasonNumber)
        {
            if (_mediaDetails?.Seasons == null) return;

            _activeSeasonNumber = seasonNumber;
            var selectedSeason = _mediaDetails.Seasons.FirstOrDefault(s => s.SeasonNumber == seasonNumber);

            if (selectedSeason != null)
            {
                // Push the episodes to the lower container
                EpisodesList.ItemsSource = selectedSeason.Episodes;
            }

            // Force a visual refresh of all rendered season buttons
            UpdateAllSeasonButtonsState();
        }

        private void SeasonButton_Loaded(object sender, RoutedEventArgs e)
        {
            // Triggers as soon as the XAML engine paints the button, ensuring the 
            // first season instantly highlights without waiting for a layout pass.
            if (sender is Button btn && btn.Tag is int sNum)
            {
                UpdateButtonVisuals(btn, sNum == _activeSeasonNumber);
            }
        }

        private void SeasonButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int sNum)
            {
                SelectSeason(sNum);
            }
        }

        // --- VISUAL FEEDBACK HELPERS ---

        private void UpdateAllSeasonButtonsState()
        {
            for (int i = 0; i < SeasonsList.Items.Count; i++)
            {
                var container = SeasonsList.ContainerFromIndex(i) as ContentPresenter;
                if (container != null)
                {
                    var button = FindVisualChild<Button>(container);
                    if (button != null && button.Tag is int sNum)
                    {
                        UpdateButtonVisuals(button, sNum == _activeSeasonNumber);
                    }
                }
            }
        }

        private void UpdateButtonVisuals(Button btn, bool isActive)
        {
            if (isActive)
            {
                btn.Background = new SolidColorBrush(Microsoft.UI.Colors.White);
                btn.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black);
                btn.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.White);
            }
            else
            {
                btn.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                btn.Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 229, 229, 229)); // #E5E5E5
                btn.BorderBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 42, 42, 42)); // #2A2A2A
            }
        }

        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild) return typedChild;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }

        // --- NAVIGATION ---

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string streamId)
            {
                Frame.Navigate(typeof(PlayerPage), streamId);
            }
        }
    }
}