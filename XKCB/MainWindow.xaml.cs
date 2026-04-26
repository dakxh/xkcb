using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System;

namespace XKCB
{
    public sealed partial class MainWindow : Window
    {
        private readonly ApiClient _apiClient = new ApiClient();
        private readonly DispatcherTimer _debounceTimer;

        // ObservableCollection automatically updates the UI when items are added
        public ObservableCollection<MediaItem> Movies { get; } = new ObservableCollection<MediaItem>();

        public MainWindow()
        {
            this.InitializeComponent();

            // Bind the XAML GridView to our Movies list
            MovieGrid.ItemsSource = Movies;

            // Change the Window Title
            this.Title = "Movie Engine V2 (Native)";

            _debounceTimer = new DispatcherTimer();
            _debounceTimer.Interval = TimeSpan.FromMilliseconds(300); // Wait 300ms
            _debounceTimer.Tick += DebounceTimer_Tick;

            LoadInitialData();
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only trigger if the user actually typed something (not code changing the text)
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                _debounceTimer.Stop();  // Reset the countdown
                _debounceTimer.Start(); // Start it again
            }
        }

        private async void DebounceTimer_Tick(object? sender, object e)
        {
            _debounceTimer.Stop(); // Stop the timer

            var query = SearchBox.Text;

            if (!string.IsNullOrWhiteSpace(query))
            {
                // Hit the Cloudflare Edge
                var results = await _apiClient.SearchAsync(query);

                // Populate the native dropdown
                SearchBox.ItemsSource = results;
            }
            else
            {
                SearchBox.ItemsSource = null; // Clear if empty
            }
        }

        private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is MediaItem selectedItem)
            {
                // Navigate to the Details Page using the same logic as clicking a poster
                var detailsFrame = new Frame();
                detailsFrame.Navigate(typeof(DetailsPage), selectedItem.Id);
                this.Content = detailsFrame;
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

        private void MovieGrid_ItemClick(object sender, Microsoft.UI.Xaml.Controls.ItemClickEventArgs e)
        {
            // Grab the specific movie the user clicked
            if (e.ClickedItem is MediaItem clickedMovie)
            {
                // In WinUI, the Window's Root is typically a Frame if you want navigation. 
                // Since our Grid is directly on the MainWindow, we can swap out the Grid's content, 
                // but the cleaner way is to use a Frame. 
                // For now, let's just create a new window to show the details quickly!

                var detailsFrame = new Frame();
                detailsFrame.Navigate(typeof(DetailsPage), clickedMovie.Id);
                this.Content = detailsFrame;
            }
        }
    }
}