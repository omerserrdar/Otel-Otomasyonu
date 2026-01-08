using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OtelV3.ViewModels;

namespace OtelV3.Views;

public partial class NewRoomView : UserControl
{
    public NewRoomView()
    {
        InitializeComponent();
        DataContext = new NewRoomViewModel();
    }
}
