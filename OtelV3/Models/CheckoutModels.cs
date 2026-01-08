using Avalonia.Media;
using System;

namespace OtelV3.Models;

public enum CheckoutStatus
{
    Pending,        // Bekliyor
    InProcess,      // İşlemde
    Completed,      // Tamamlandı
    PartialPayment  // Kısmi Ödeme
}

public class GuestCheckoutModel
{
    public int Id { get; set; }
    public string RoomNo { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestInitials { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal Balance { get; set; }
    public CheckoutStatus Status { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;

    // UI Helpers
    public IBrush AvatarBackground { get; set; } = Brushes.LightGray;
    public IBrush AvatarForeground { get; set; } = Brushes.White;
    
    public string StatusText => Status switch
    {
        CheckoutStatus.Pending => "Bekliyor",
        CheckoutStatus.InProcess => "İşlemde",
        CheckoutStatus.Completed => "Tamamlandı",
        CheckoutStatus.PartialPayment => "Kısmi Ödeme",
        _ => "Bilinmiyor"
    };

    public IBrush StatusBackground => Status switch
    {
        CheckoutStatus.Pending => new SolidColorBrush(Color.Parse("#fffbeb")), // amber-100
        CheckoutStatus.InProcess => new SolidColorBrush(Color.Parse("#dbeafe")), // blue-100
        CheckoutStatus.Completed => new SolidColorBrush(Color.Parse("#d1fae5")), // emerald-100
        CheckoutStatus.PartialPayment => new SolidColorBrush(Color.Parse("#fffbeb")), // amber-100
        _ => Brushes.LightGray
    };

    public IBrush StatusForeground => Status switch
    {
        CheckoutStatus.Pending => new SolidColorBrush(Color.Parse("#92400e")), // amber-800
        CheckoutStatus.InProcess => new SolidColorBrush(Color.Parse("#1e40af")), // blue-800
        CheckoutStatus.Completed => new SolidColorBrush(Color.Parse("#065f46")), // emerald-800
        CheckoutStatus.PartialPayment => new SolidColorBrush(Color.Parse("#92400e")), // amber-800
        _ => Brushes.DarkGray
    };
    
    public bool IsCheckOutToday => CheckOutDate.Date == DateTime.Today;
    public string CheckInDisplay => CheckInDate.ToString("dd.MM.yyyy");
    public string CheckOutDisplay => CheckOutDate.ToString("dd.MM.yyyy");
    public int StayDuration => (CheckOutDate - CheckInDate).Days;
}

public class InvoiceItemModel
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = "Other"; // Restoran, Konaklama vs.
}
