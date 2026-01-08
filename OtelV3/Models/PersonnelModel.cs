using System;
using Avalonia.Media;

namespace OtelV3.Models;

public enum PersonnelStatus
{
    Active,
    OnLeave,
    Left
}

public class PersonnelModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string TcNo { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public decimal Salary { get; set; }
    public PersonnelStatus Status { get; set; }
    public string? AvatarUrl { get; set; }

    // Helper properties for UI binding
    public string StartDateDisplay => StartDate.ToString("dd MMM yyyy");
    public string SalaryDisplay => $"₺{Salary:N0}";
    
    public string StatusText => Status switch
    {
        PersonnelStatus.Active => "Aktif",
        PersonnelStatus.OnLeave => "İzinde",
        PersonnelStatus.Left => "Ayrıldı",
        _ => "Bilinmiyor"
    };

    public IBrush StatusBackground => Status switch
    {
        PersonnelStatus.Active => new SolidColorBrush(Color.Parse("#dcfce7")), // Emerald-100
        PersonnelStatus.OnLeave => new SolidColorBrush(Color.Parse("#fef3c7")), // Amber-100
        PersonnelStatus.Left => new SolidColorBrush(Color.Parse("#f3f4f6")), // Gray-100
        _ => Brushes.Transparent
    };

    public IBrush StatusForeground => Status switch
    {
        PersonnelStatus.Active => new SolidColorBrush(Color.Parse("#047857")), // Emerald-700
        PersonnelStatus.OnLeave => new SolidColorBrush(Color.Parse("#b45309")), // Amber-700
        PersonnelStatus.Left => new SolidColorBrush(Color.Parse("#374151")), // Gray-700
        _ => Brushes.Black
    };
    
    public IBrush StatusDotColor => Status switch
    {
        PersonnelStatus.Active => new SolidColorBrush(Color.Parse("#10b981")), // Emerald-500
        PersonnelStatus.OnLeave => new SolidColorBrush(Color.Parse("#f59e0b")), // Amber-500
        PersonnelStatus.Left => new SolidColorBrush(Color.Parse("#6b7280")), // Gray-500
        _ => Brushes.Gray
    };
}
