using GameProject02.Controls;
using GameProject02.Views;
using Microsoft.Maui.Controls;

namespace GameProject02.Views;

public class BasePage : ContentPage
{
    protected Grid MainLayout { get; private set; }
    protected TopHeaderView Header { get; private set; }
    protected FooterView Footer { get; private set; }
    protected ContentView ContentArea { get; private set; }

    public BasePage()
    {
        // Create the main layout with 3 rows: Header, Content, Footer
        MainLayout = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },    // Header
                new RowDefinition { Height = GridLength.Star },    // Content (flexible)
                new RowDefinition { Height = GridLength.Auto }     // Footer
            }
        };

        // Create Header (persistent, updates in real-time)
        Header = new TopHeaderView();
        MainLayout.Add(Header, row: 0);

        // Create Content Area (changes per page)
        ContentArea = new ContentView();
        MainLayout.Add(ContentArea, row: 1);

        // Create Footer (persistent, buttons change per page)
        Footer = new FooterView();
        MainLayout.Add(Footer, row: 2);

        // Set the main layout as the page content
        Content = MainLayout;
    }

    /// <summary>
    /// Set the page-specific content in the middle area
    /// </summary>
    protected void SetPageContent(View content)
    {
        ContentArea.Content = content;
    }

    /// <summary>
    /// Set the footer content (buttons)
    /// </summary>
    protected void SetFooterContent(View content)
    {
        Footer.SetContent(content);
    }

    /// <summary>
    /// Refresh header data (called automatically by timer in TopHeaderView)
    /// </summary>
    protected void RefreshHeader()
    {
        // Header updates automatically via its internal timer
        // This method is here if you need to force a refresh
    }
}