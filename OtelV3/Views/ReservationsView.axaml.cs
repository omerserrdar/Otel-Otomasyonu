using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OtelV3.ViewModels;

namespace OtelV3.Views;

public partial class ReservationsView : UserControl
{
    public ReservationsView()
    {
        InitializeComponent();
        DataContext = new ReservationsViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
