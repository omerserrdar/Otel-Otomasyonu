using Avalonia.Media;
using Material.Icons;

namespace OtelV3.Models;

public class StatCard
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ChangeText { get; set; } = string.Empty;
    public bool IsPositiveChange { get; set; }
    public bool IsNeutralChange { get; set; }
    public MaterialIconKind Icon { get; set; }
    public IBrush IconBackground { get; set; } = Brushes.Transparent;
    public IBrush IconForeground { get; set; } = Brushes.Black;
}
