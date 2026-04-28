using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;

namespace XKCB
{
    public sealed partial class CatalogPage : Page
    {
        private readonly ApiClient _apiClient = new ApiClient();
        private readonly DispatcherTimer _debounceTimer;

        public ObservableCollection<MediaItem> Movies { get; } = new ObservableCollection<MediaItem>();

        public CatalogPage()
        {
            this.InitializeComponent();

            // CRITICAL: Keep this page (and its D1 data + scroll position) fully loaded in RAM
            this.NavigationCacheMode = NavigationCacheMode.Required;

            MovieGrid.ItemsSource = Movies;

            _debounceTimer = new DispatcherTimer();
            _debounceTimer.Interval = TimeSpan.FromMilliseconds(300);
            _debounceTimer.Tick += DebounceTimer_Tick;

            // Prevent re-fetching the initial payload if we are navigating back to an already cached page
            if (Movies.Count == 0)
            {
                LoadInitialData();
            }
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                _debounceTimer.Stop();
                _debounceTimer.Start();
            }
        }

        private async void DebounceTimer_Tick(object sender, object e)
        {
            _debounceTimer.Stop();
            var query = SearchBox.Text;

            if (string.IsNullOrWhiteSpace(query))
            {
                SearchBox.ItemsSource = null;
                return;
            }

            var results = await _apiClient.SearchAsync(query);
            SearchBox.ItemsSource = results;
        }

        private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is MediaItem selectedItem)
            {
                // Push the DetailsPage onto the Frame stack
                this.Frame.Navigate(typeof(DetailsPage), selectedItem.Id);
            }
        }

        private async void LoadInitialData()
        {
            var response = await _apiClient.GetCatalogAsync(0);

            if (response?.Data != null)
            {
                foreach (var movie in response.Data)
                {
                    Movies.Add(movie);
                }
            }
        }

        private void MovieGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is MediaItem clickedMovie)
            {
                // Push the DetailsPage onto the Frame stack
                this.Frame.Navigate(typeof(DetailsPage), clickedMovie.Id);
            }
        }
    }
}