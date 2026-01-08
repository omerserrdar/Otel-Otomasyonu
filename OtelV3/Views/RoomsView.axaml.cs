using Avalonia.Controls;
using OtelV3.ViewModels;

namespace OtelV3.Views;

public partial class RoomsView : UserControl
{
    public RoomsView()
    {
        InitializeComponent();
        DataContext = new RoomsViewModel();
    }
}
