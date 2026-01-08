using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace OtelV3.Models;

public partial class MenuItem : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private MaterialIconKind _icon;
    
    [ObservableProperty]
    private bool _isSelected;
}
