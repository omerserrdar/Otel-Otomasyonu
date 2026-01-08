using System;
using Avalonia.Media;
using Material.Icons;

namespace OtelV3.Models;



public class ReservationModel
{
    public string Id { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestContact { get; set; } = string.Empty;
    public string? GuestAvatarUrl { get; set; }
    public string GuestInitials { get; set; } = string.Empty;
    
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string GuestCount { get; set; } = string.Empty;
    
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    
    public decimal Amount { get; set; }
    public ReservationStatus Status { get; set; }
    
    // Helpers for UI binding
    public string FormattedCheckIn => CheckInDate.ToString("dd MMMM yyyy");
    public string FormattedCheckOut => CheckOutDate.ToString("dd MMMM yyyy");
    public string FormattedAmount => $"₺{Amount:N0}";
    
    public string StatusText => Status switch
    {
        ReservationStatus.Confirmed => "Onaylandı",
        ReservationStatus.Pending => "Beklemede",
        ReservationStatus.CheckedIn => "Giriş Yaptı",
        ReservationStatus.CheckedOut => "Çıkış Yaptı",
        _ => string.Empty
    };
    
    public IBrush StatusBackground => Status switch
    {
        ReservationStatus.Confirmed => new SolidColorBrush(Color.Parse("#1A11b4d4")), // primary/10
        ReservationStatus.Pending => new SolidColorBrush(Color.Parse("#fef3c7")), // amber-100
        ReservationStatus.CheckedIn => new SolidColorBrush(Color.Parse("#d1fae5")), // emerald-100
        ReservationStatus.CheckedOut => new SolidColorBrush(Color.Parse("#f3f4f6")), // gray-100
        _ => Brushes.Transparent
    };
    
    public IBrush StatusForeground => Status switch
    {
        ReservationStatus.Confirmed => new SolidColorBrush(Color.Parse("#11b4d4")), // primary
        ReservationStatus.Pending => new SolidColorBrush(Color.Parse("#b45309")), // amber-700
        ReservationStatus.CheckedIn => new SolidColorBrush(Color.Parse("#047857")), // emerald-700
        ReservationStatus.CheckedOut => new SolidColorBrush(Color.Parse("#4b5563")), // gray-600
        _ => Brushes.Black
    };
}
