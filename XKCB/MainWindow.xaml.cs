using Microsoft.UI.Xaml;

namespace XKCB
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "Movie Engine V2 (Native)";

            // On boot, inject the CatalogPage into the frame
            RootFrame.Navigate(typeof(CatalogPage));
        }
    }
}