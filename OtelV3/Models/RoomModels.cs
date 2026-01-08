using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;

namespace OtelV3.Models;


public partial class RoomViewModel : ObservableObject
{
    [ObservableProperty] private string _roomNumber = string.Empty;
    [ObservableProperty] private string _roomType = string.Empty;
    [ObservableProperty] private string _capacity = string.Empty;
    [ObservableProperty] private string _features = string.Empty;
    [ObservableProperty] private string _area = string.Empty;
    [ObservableProperty] private decimal _price;
    [ObservableProperty] private string? _guestName;
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(StatusColor))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusBackground))]
    [NotifyPropertyChangedFor(nameof(StatusForeground))]
    [NotifyPropertyChangedFor(nameof(StatusBorderBrush))]
    [NotifyPropertyChangedFor(nameof(ActionButtonText))]
    [NotifyPropertyChangedFor(nameof(ActionButtonBackground))]
    [NotifyPropertyChangedFor(nameof(ActionButtonForeground))]
    [NotifyPropertyChangedFor(nameof(ActionButtonIcon))]
    private RoomStatus _status;

    // Visual Properties based on Status
    public IBrush StatusColor
    {
        get
        {
            return Status switch
            {
                RoomStatus.Available => Brushes.Green, // Green-500
                RoomStatus.Occupied => Brushes.Red, // Red-500
                RoomStatus.Cleaning => Brushes.Orange, // Orange-500
                RoomStatus.Maintenance => Brushes.Gray, // Gray-400
                _ => Brushes.Gray
            };
        }
    }

    public string StatusText
    {
        get
        {
            return Status switch
            {
                RoomStatus.Available => "Müsait",
                RoomStatus.Occupied => "Dolu",
                RoomStatus.Cleaning => "Temizlik",
                RoomStatus.Maintenance => "Bakımda",
                _ => "-"
            };
        }
    }
    
    public IBrush StatusBackground
    {
        get
        {
            // Tailwdin colors converted roughly
            return Status switch
            {
                RoomStatus.Available => new SolidColorBrush(Color.Parse("#f0fdf4")), // green-50
                RoomStatus.Occupied => new SolidColorBrush(Color.Parse("#fef2f2")), // red-50
                RoomStatus.Cleaning => new SolidColorBrush(Color.Parse("#fff7ed")), // orange-50
                RoomStatus.Maintenance => new SolidColorBrush(Color.Parse("#f3f4f6")), // gray-100
                _ => Brushes.Transparent
            };
        }
    }
    
    public IBrush StatusForeground
    {
        get
        {
            return Status switch
            {
                RoomStatus.Available => new SolidColorBrush(Color.Parse("#16a34a")), // green-600
                RoomStatus.Occupied => new SolidColorBrush(Color.Parse("#dc2626")), // red-600
                RoomStatus.Cleaning => new SolidColorBrush(Color.Parse("#ea580c")), // orange-600
                RoomStatus.Maintenance => new SolidColorBrush(Color.Parse("#4b5563")), // gray-600
                _ => Brushes.Black
            };
        }
    }
    
    public IBrush StatusBorderBrush
    {
        get
        {
             return Status switch
            {
                RoomStatus.Available => new SolidColorBrush(Color.Parse("#dcfce7")), // green-100
                RoomStatus.Occupied => new SolidColorBrush(Color.Parse("#fee2e2")), // red-100
                RoomStatus.Cleaning => new SolidColorBrush(Color.Parse("#ffedd5")), // orange-100
                RoomStatus.Maintenance => new SolidColorBrush(Color.Parse("#e5e7eb")), // gray-200
                _ => Brushes.Transparent
            };
        }
    }
    
    // Action Button Logic
    public string ActionButtonText
    {
        get
        {
            return Status switch
            {
                RoomStatus.Available => "Rezervasyon",
                RoomStatus.Occupied => "Detaylar",
                RoomStatus.Cleaning => "Hazırla",
                RoomStatus.Maintenance => "Durum",
                _ => "İşlem"
            };
        }
    }
    
    public MaterialIconKind? ActionButtonIcon
    {
         get
        {
            return Status switch
            {
                RoomStatus.Cleaning => MaterialIconKind.Broom,
                RoomStatus.Maintenance => MaterialIconKind.Build,
                _ => null
            };
        }
    }

    public IBrush ActionButtonBackground
    {
        get
        {
             return Status switch
            {
                RoomStatus.Available => new SolidColorBrush(Color.Parse("#11b4d4")), // Primary
                RoomStatus.Occupied => new SolidColorBrush(Color.Parse("#f6f8f8")), // background-light
                RoomStatus.Cleaning => new SolidColorBrush(Color.Parse("#fff7ed")), // orange-50
                RoomStatus.Maintenance => new SolidColorBrush(Color.Parse("#f3f4f6")), // gray-100
                _ => Brushes.Gray
            };
        }
    }
    
    public IBrush ActionButtonForeground
    {
        get
        {
             return Status switch
            {
                RoomStatus.Available => Brushes.White,
                RoomStatus.Occupied => new SolidColorBrush(Color.Parse("#0d191b")), // text-main
                RoomStatus.Cleaning => new SolidColorBrush(Color.Parse("#ea580c")), // orange-600
                RoomStatus.Maintenance => new SolidColorBrush(Color.Parse("#4b5563")), // gray-600
                _ => Brushes.White
            };
        }
    }

    [RelayCommand]
    private void Action()
    {
        // Handle action based on status
        System.Diagnostics.Debug.WriteLine($"Action clicked for {RoomNumber}");
    }
}
