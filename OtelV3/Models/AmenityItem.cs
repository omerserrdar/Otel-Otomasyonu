using CommunityToolkit.Mvvm.ComponentModel;

namespace OtelV3.Models;

public partial class AmenityItem : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _isChecked;
}
