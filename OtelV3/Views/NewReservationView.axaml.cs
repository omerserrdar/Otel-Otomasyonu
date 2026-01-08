using Avalonia.Controls;
using OtelV3.ViewModels;

namespace OtelV3.Views;

public partial class NewReservationView : UserControl
{
    public NewReservationView()
    {
        InitializeComponent();
        // Typically DataContext is set by the parent (DashboardViewModel) or DI.
        // But for standalone testing or direct usage:
        // DataContext = new NewReservationViewModel(); 
        // We will leave it empty here to allow binding from parent.
    }
}
