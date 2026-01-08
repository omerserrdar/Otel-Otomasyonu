using Avalonia.Media;
using Material.Icons;

namespace OtelV3.Models;

public enum RoomStatus
{
    Occupied,      // Dolu
    Available,     // Müsait
    Cleaning,      // Temizlik
    Maintenance    // Bakımda
}

public class Room
{
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public RoomStatus Status { get; set; }
    public string? GuestName { get; set; }
    
    public MaterialIconKind StatusIcon => Status switch
    {
        RoomStatus.Occupied => MaterialIconKind.Account,
        RoomStatus.Available => MaterialIconKind.CheckCircle,
        RoomStatus.Cleaning => MaterialIconKind.Broom,
        RoomStatus.Maintenance => MaterialIconKind.Wrench,
        _ => MaterialIconKind.Help
    };
    
    public string StatusText => Status switch
    {
        RoomStatus.Occupied => string.IsNullOrEmpty(GuestName) ? "Dolu" : GuestName,
        RoomStatus.Available => "Müsait",
        RoomStatus.Cleaning => "Temizlik",
        RoomStatus.Maintenance => "Bakımda",
        _ => string.Empty
    };
    
    public IBrush BorderColor => Status switch
    {
        RoomStatus.Occupied => new SolidColorBrush(Color.Parse("#3b82f6")),
        RoomStatus.Available => new SolidColorBrush(Color.Parse("#10b981")),
        RoomStatus.Cleaning => new SolidColorBrush(Color.Parse("#f59e0b")),
        RoomStatus.Maintenance => new SolidColorBrush(Color.Parse("#64748b")),
        _ => Brushes.Gray
    };
    
    public IBrush IconColor => Status switch
    {
        RoomStatus.Occupied => new SolidColorBrush(Color.Parse("#3b82f6")),
        RoomStatus.Available => new SolidColorBrush(Color.Parse("#10b981")),
        RoomStatus.Cleaning => new SolidColorBrush(Color.Parse("#f59e0b")),
        RoomStatus.Maintenance => new SolidColorBrush(Color.Parse("#64748b")),
        _ => Brushes.Gray
    };
    
    public IBrush StatusTextColor => Status switch
    {
        RoomStatus.Occupied => new SolidColorBrush(Color.Parse("#3b82f6")),
        RoomStatus.Available => new SolidColorBrush(Color.Parse("#10b981")),
        RoomStatus.Cleaning => new SolidColorBrush(Color.Parse("#d97706")),
        RoomStatus.Maintenance => new SolidColorBrush(Color.Parse("#64748b")),
        _ => Brushes.Gray
    };
    
    public IBrush CardBackground => Status switch
    {
        RoomStatus.Maintenance => new SolidColorBrush(Color.Parse("#f8fafc")),
        _ => Brushes.White
    };
    
    public double CardOpacity => Status == RoomStatus.Maintenance ? 0.8 : 1.0;
}
