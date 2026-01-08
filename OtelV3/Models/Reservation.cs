using Avalonia.Media;

namespace OtelV3.Models;

public enum ReservationStatus
{
    Confirmed,    // Onaylandı
    CheckedIn,    // Giriş Yaptı
    Pending,      // Bekliyor
    CheckedOut    // Çıkış Yaptı
}

public class Reservation
{
    public string ReservationId { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestInitials { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string CheckInDate { get; set; } = string.Empty;
    public string CheckOutDate { get; set; } = string.Empty;
    public ReservationStatus Status { get; set; }
    public string Amount { get; set; } = string.Empty;
    
    public string StatusText => Status switch
    {
        ReservationStatus.Confirmed => "Onaylandı",
        ReservationStatus.CheckedIn => "Giriş Yaptı",
        ReservationStatus.Pending => "Bekliyor",
        ReservationStatus.CheckedOut => "İşlem Tamamlandı",
        _ => string.Empty
    };
    
    public IBrush AvatarBackground => GuestInitials switch
    {
        "AY" => new SolidColorBrush(Color.Parse("#dbeafe")),
        "SJ" => new SolidColorBrush(Color.Parse("#f3e8ff")),
        "MK" => new SolidColorBrush(Color.Parse("#fef3c7")),
        "ES" => new SolidColorBrush(Color.Parse("#fce7f3")),
        _ => new SolidColorBrush(Color.Parse("#e0e7ff"))
    };
    
    public IBrush AvatarForeground => GuestInitials switch
    {
        "AY" => new SolidColorBrush(Color.Parse("#2563eb")),
        "SJ" => new SolidColorBrush(Color.Parse("#9333ea")),
        "MK" => new SolidColorBrush(Color.Parse("#d97706")),
        "ES" => new SolidColorBrush(Color.Parse("#db2777")),
        _ => new SolidColorBrush(Color.Parse("#4f46e5"))
    };
    
    public IBrush StatusBackground => Status switch
    {
        ReservationStatus.Confirmed => new SolidColorBrush(Color.Parse("#dcfce7")),
        ReservationStatus.CheckedIn => new SolidColorBrush(Color.Parse("#dbeafe")),
        ReservationStatus.Pending => new SolidColorBrush(Color.Parse("#fef3c7")),
        ReservationStatus.CheckedOut => new SolidColorBrush(Color.Parse("#f3f4f6")),
        _ => Brushes.LightGray
    };
    
    public IBrush StatusForeground => Status switch
    {
        ReservationStatus.Confirmed => new SolidColorBrush(Color.Parse("#15803d")),
        ReservationStatus.CheckedIn => new SolidColorBrush(Color.Parse("#1d4ed8")),
        ReservationStatus.Pending => new SolidColorBrush(Color.Parse("#b45309")),
        ReservationStatus.CheckedOut => new SolidColorBrush(Color.Parse("#374151")),
        _ => Brushes.Black
    };
    
    public IBrush StatusBorderBrush => Status switch
    {
        ReservationStatus.Confirmed => new SolidColorBrush(Color.Parse("#bbf7d0")),
        ReservationStatus.CheckedIn => new SolidColorBrush(Color.Parse("#bfdbfe")),
        ReservationStatus.Pending => new SolidColorBrush(Color.Parse("#fde68a")),
        ReservationStatus.CheckedOut => new SolidColorBrush(Color.Parse("#e5e7eb")),
        _ => Brushes.Gray
    };
    
    public IBrush StatusDotColor => Status switch
    {
        ReservationStatus.Confirmed => new SolidColorBrush(Color.Parse("#22c55e")),
        ReservationStatus.CheckedIn => new SolidColorBrush(Color.Parse("#3b82f6")),
        ReservationStatus.Pending => new SolidColorBrush(Color.Parse("#f59e0b")),
        ReservationStatus.CheckedOut => new SolidColorBrush(Color.Parse("#6b7280")),
        _ => Brushes.Gray
    };
}
