using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtelV3.Models;
using OtelV3.Services;

namespace OtelV3.ViewModels;

public partial class NewReservationViewModel : ObservableObject
{
    // Guest Info
    [ObservableProperty] private string _guestName = string.Empty;
    [ObservableProperty] private string _guestIdNumber = string.Empty;
    [ObservableProperty] private string _guestPhone = string.Empty;
    [ObservableProperty] private string _guestEmail = string.Empty;
    [ObservableProperty] private string _guestNationality = "Türkiye";
    [ObservableProperty] private string _guestGender = "Belirtmek İstemiyor";
    
    // Room Info
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(DurationText))]
    [NotifyPropertyChangedFor(nameof(TotalAmount))]
    private DateTime? _checkInDate = DateTime.Now.Date;

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(DurationText))]
    [NotifyPropertyChangedFor(nameof(TotalAmount))]
    private DateTime? _checkOutDate = DateTime.Now.Date.AddDays(1);

    [ObservableProperty] private string _roomType = "Standart Oda";
    [ObservableProperty] private AvailableRoom? _selectedRoom;
    [ObservableProperty] private int _adultCount = 1;
    [ObservableProperty] private int _childCount = 0;
    
    // Payment Info
    [ObservableProperty] private string _paymentMethod = "Kredi Kartı";
    [ObservableProperty] private string _paymentStatus = "Ödeme Bekleniyor";
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(TotalAmount))]
    private decimal _nightlyRate = 1500;
    
    [ObservableProperty] private string _specialRequests = string.Empty;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;

    // Collections
    public ObservableCollection<string> Nationalities { get; } = new() 
        { "Türkiye", "Almanya", "İngiltere", "Rusya", "Diğer" };
        
    public ObservableCollection<string> Genders { get; } = new() 
        { "Erkek", "Kadın", "Belirtmek İstemiyor" };

    public ObservableCollection<string> RoomTypes { get; } = new() 
        { "Standart Oda", "Deluxe Oda", "Aile Odası", "King Suite" };
        
    public ObservableCollection<string> PaymentMethods { get; } = new() 
        { "Kredi Kartı", "Nakit", "Banka Havalesi", "Acenta (Booking/Expedia)" };
        
    public ObservableCollection<string> PaymentStatuses { get; } = new() 
        { "Ödeme Bekleniyor", "Kısmi Ödeme Alındı", "Tamamı Ödendi" };
        
    public ObservableCollection<AvailableRoom> AvailableRooms { get; } = new();
    
    public ObservableCollection<CustomerModel> Customers { get; } = new();
    
    [ObservableProperty] 
    private CustomerModel? _selectedCustomer;

    partial void OnSelectedCustomerChanged(CustomerModel? value)
    {
        if (value != null)
        {
            GuestName = value.Name;
            GuestPhone = value.Phone;
            GuestEmail = value.Email;
            GuestIdNumber = value.IdentityNumber;
        }
    }

    partial void OnSelectedRoomChanged(AvailableRoom? value)
    {
        if (value != null)
        {
            NightlyRate = value.Price;
        }
    }

    public NewReservationViewModel()
    {
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Load customers
            var customers = await DatabaseService.Instance.GetAllCustomersAsync();
            Customers.Clear();
            foreach (var c in customers)
            {
                Customers.Add(c);
            }
            
            // Load available rooms
            await LoadAvailableRoomsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string? _preSelectedRoomNumber;

    public void SetPreSelectedRoom(string roomNumber)
    {
        _preSelectedRoomNumber = roomNumber;
        // Dates are default today, which is fine.
        _ = LoadAvailableRoomsAsync();
    }

    private async Task LoadAvailableRoomsAsync()
    {
        if (!CheckInDate.HasValue || !CheckOutDate.HasValue) return;
        
        try
        {
            var rooms = await DatabaseService.Instance.GetAvailableRoomsAsync(
                CheckInDate.Value, 
                CheckOutDate.Value
            );
            
            AvailableRooms.Clear();
            foreach (var room in rooms)
            {
                var avRoom = new AvailableRoom
                {
                    DisplayName = room.RoomInfo,
                    RoomNumber = room.RoomNumber,
                    Price = room.Price
                };
                AvailableRooms.Add(avRoom);
                
                // Pre-select logic
                if (!string.IsNullOrEmpty(_preSelectedRoomNumber) && avRoom.RoomNumber == _preSelectedRoomNumber)
                {
                    SelectedRoom = avRoom;
                }
            }
            
            // Clear pre-selection after one use if found? 
            // Better to keep it until explicit reset, but for now this works.
            // If we found it, clearing it prevents re-selecting if user changes dates and room becomes unavailable.
            if (SelectedRoom != null) 
            {
                 _preSelectedRoomNumber = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading available rooms: {ex.Message}");
        }
    }

    // Computed Properties
    public string DurationText
    {
        get
        {
            if (CheckInDate.HasValue && CheckOutDate.HasValue)
            {
                var duration = (CheckOutDate.Value - CheckInDate.Value).Days;
                if (duration <= 0) return "Hatali Tarih";
                return $"{duration} Gece";
            }
            return "-";
        }
    }

    public decimal TotalAmount
    {
        get
        {
            if (CheckInDate.HasValue && CheckOutDate.HasValue)
            {
                var duration = (CheckOutDate.Value - CheckInDate.Value).Days;
                if (duration > 0)
                {
                    return duration * NightlyRate;
                }
            }
            return 0;
        }
    }

    // Commands
    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;
        
        // Validation
        if (SelectedCustomer == null && string.IsNullOrWhiteSpace(GuestName))
        {
            ErrorMessage = "Lütfen bir müşteri seçin veya misafir adı girin.";
            return;
        }
        
        if (SelectedRoom == null)
        {
            ErrorMessage = "Lütfen bir oda seçin.";
            return;
        }
        
        if (!CheckInDate.HasValue || !CheckOutDate.HasValue)
        {
            ErrorMessage = "Giriş ve çıkış tarihleri gereklidir.";
            return;
        }
        
        if (CheckOutDate.Value <= CheckInDate.Value)
        {
            ErrorMessage = "Çıkış tarihi giriş tarihinden sonra olmalıdır.";
            return;
        }

        try
        {
            IsSaving = true;
            
            // Get room number from selected room
            var roomNumber = SelectedRoom.RoomNumber;
            var customerId = SelectedCustomer?.Id ?? 0;
            var guestCount = AdultCount + ChildCount;
            
            // If no customer selected, create new one
            if (customerId == 0 && !string.IsNullOrWhiteSpace(GuestName))
            {
                await DatabaseService.Instance.AddCustomerAsync(
                    GuestName,
                    GuestPhone,
                    GuestEmail,
                    GuestIdNumber
                );
                // Reload customers and get the new one
                var customers = await DatabaseService.Instance.GetAllCustomersAsync();
                var newCustomer = customers.LastOrDefault();
                customerId = newCustomer?.Id ?? 0;
            }
            
            var success = await DatabaseService.Instance.AddReservationAsync(
                customerId,
                roomNumber,
                CheckInDate.Value,
                CheckOutDate.Value,
                TotalAmount,
                0, // Confirmed status
                guestCount
            );
            
            if (success)
            {
                // Update room status to occupied
                await DatabaseService.Instance.UpdateRoomStatusAsync(roomNumber, 0); // Occupied
                
                ReservationSaved?.Invoke(this, EventArgs.Empty);
                OnRequestNavigateBack();
            }
            else
            {
                ErrorMessage = "Rezervasyon kaydedilemedi.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Hata: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        OnRequestNavigateBack();
    }
    
    [RelayCommand]
    private void IncrementAdult() => AdultCount++;

    [RelayCommand]
    private void DecrementAdult()
    {
        if (AdultCount > 1) AdultCount--;
    }

    [RelayCommand]
    private void IncrementChild() => ChildCount++;

    [RelayCommand]
    private void DecrementChild()
    {
        if (ChildCount > 0) ChildCount--;
    }

    [RelayCommand]
    private async Task RefreshRoomsAsync()
    {
        await LoadAvailableRoomsAsync();
    }

    // Navigation Event
    public event EventHandler? RequestNavigateBack;
    public event EventHandler? ReservationSaved;

    private void OnRequestNavigateBack()
    {
        RequestNavigateBack?.Invoke(this, EventArgs.Empty);
    }
    
    partial void OnCheckInDateChanged(DateTime? value)
    {
        _ = LoadAvailableRoomsAsync();
    }
    
    partial void OnCheckOutDateChanged(DateTime? value)
    {
        _ = LoadAvailableRoomsAsync();
    }
}
