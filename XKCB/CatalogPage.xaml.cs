using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media; // NEW: Required for VisualTreeHelper

namespace XKCB
{
    public sealed partial class CatalogPage : Page
    {
        private readonly ApiClient _apiClient = new ApiClient();
        private readonly DispatcherTimer _debounceTimer;

        // NEW: Pagination State
        private int? _nextCursor = 0;
        private bool _isLoading = false;

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

            // NEW: Wait for the page to load to grab the ScrollViewer
            this.Loaded += CatalogPage_Loaded;

            // Prevent re-fetching the initial payload if we are navigating back to an already cached page
            if (Movies.Count == 0)
            {
                LoadDataAsync();
            }
        }

        private void CatalogPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Extract the hidden ScrollViewer from inside the GridView
            var scrollViewer = GetScrollViewer(MovieGrid);
            if (scrollViewer != null)
            {
                // Unsubscribe first to prevent duplicate event fires when navigating back and forth
                scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            }
        }

        // NEW: Recursively search the visual tree for the ScrollViewer
        private ScrollViewer? GetScrollViewer(DependencyObject element)
        {
            if (element is ScrollViewer viewer) return viewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        // NEW: Fire the next fetch if we get near the bottom of the grid
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sender is ScrollViewer sv)
            {
                // If we are within 200 pixels of the bottom, load the next page
                if (sv.ScrollableHeight > 0 && sv.VerticalOffset >= sv.ScrollableHeight - 200)
                {
                    LoadDataAsync();
                }
            }
        }

        private async void LoadDataAsync()
        {
            // Abort if a fetch is already running, or if the API returned null for NextCursor (end of catalog)
            if (_isLoading || _nextCursor == null) return;

            _isLoading = true;
            LoadingRing.IsActive = true; // Show spinner

            var response = await _apiClient.GetCatalogAsync(_nextCursor.Value);

            if (response != null)
            {
                // Store the cursor for the NEXT page
                _nextCursor = response.NextCursor;

                if (response.Data != null)
                {
                    foreach (var movie in response.Data)
                    {
                        Movies.Add(movie);
                    }
                }
            }

            _isLoading = false;
            LoadingRing.IsActive = false; // Hide spinner
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
                this.Frame.Navigate(typeof(DetailsPage), selectedItem.Id);
            }
        }

        private void MovieGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is MediaItem clickedMovie)
            {
                this.Frame.Navigate(typeof(DetailsPage), clickedMovie.Id);
            }
        }
    }
}