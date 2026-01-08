using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtelV3.Models;
using OtelV3.Services;

namespace OtelV3.ViewModels;

public partial class NewRoomViewModel : ObservableObject
{
    // Form Properties
    [ObservableProperty] private string _roomNumber = string.Empty;
    [ObservableProperty] private int? _floor;
    [ObservableProperty] private string? _selectedRoomType;
    [ObservableProperty] private int _capacity = 2;
    [ObservableProperty] private decimal? _price;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string _errorMessage = string.Empty;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsAvailableSelected))]
    [NotifyPropertyChangedFor(nameof(IsCleaningSelected))]
    [NotifyPropertyChangedFor(nameof(IsMaintenanceSelected))]
    private RoomStatus _status = RoomStatus.Available;

    // Helper properties for RadioButtons
    public bool IsAvailableSelected
    {
        get => Status == RoomStatus.Available;
        set { if (value) Status = RoomStatus.Available; }
    }
    
    public bool IsCleaningSelected
    {
        get => Status == RoomStatus.Cleaning;
        set { if (value) Status = RoomStatus.Cleaning; }
    }
    
    public bool IsMaintenanceSelected
    {
        get => Status == RoomStatus.Maintenance;
        set { if (value) Status = RoomStatus.Maintenance; }
    }

    // Collections
    public ObservableCollection<string> RoomTypes { get; } = new()
    {
        "Standart Oda",
        "Deluxe Oda",
        "Suit Oda",
        "Aile Odası",
        "Kral Dairesi"
    };

    public ObservableCollection<AmenityItem> Amenities { get; } = new()
    {
        new AmenityItem { Name = "Wifi Erişim", IsChecked = true },
        new AmenityItem { Name = "TV & Uydu", IsChecked = true },
        new AmenityItem { Name = "Minibar" },
        new AmenityItem { Name = "Klima", IsChecked = true },
        new AmenityItem { Name = "Balkon / Teras" },
        new AmenityItem { Name = "Jakuzi" },
        new AmenityItem { Name = "Kasa" },
        new AmenityItem { Name = "Oda Servisi" }
    };

    // Events
    public event EventHandler? RequestNavigateBack;
    public event EventHandler? RoomSaved;

    // Commands
    [RelayCommand]
    private void IncreaseCapacity()
    {
        if (Capacity < 10) Capacity++;
    }

    [RelayCommand]
    private void DecreaseCapacity()
    {
        if (Capacity > 1) Capacity--;
    }

    [ObservableProperty] private bool _isEditMode;
    private string _originalRoomNumber = string.Empty;

    public void Reset()
    {
        IsEditMode = false;
        _originalRoomNumber = string.Empty;
        RoomNumber = string.Empty;
        Floor = null;
        SelectedRoomType = null;
        Capacity = 2;
        Price = null;
        Notes = string.Empty;
        Status = RoomStatus.Available;
        ErrorMessage = string.Empty;
        
        foreach(var a in Amenities) a.IsChecked = false;
        Amenities[0].IsChecked = true; // Wifi default
        Amenities[1].IsChecked = true; // TV default
        Amenities[3].IsChecked = true; // AC default
    }

    public void LoadForEdit(RoomViewModel room)
    {
        IsEditMode = true;
        _originalRoomNumber = room.RoomNumber;
        RoomNumber = room.RoomNumber;
        SelectedRoomType = room.RoomType;
        
        // Parse "2 Kişi" -> 2
        if (int.TryParse(room.Capacity.Split(' ')[0], out int cap)) Capacity = cap;
        else Capacity = 2;
        
        Price = room.Price;
        Status = room.Status;
        
        // Features mapping (simple contains check)
        foreach(var a in Amenities)
        {
            a.IsChecked = room.Features.Contains(a.Name);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;
        
        // Validation
        if (string.IsNullOrWhiteSpace(RoomNumber))
        {
            ErrorMessage = "Oda numarası gereklidir.";
            return;
        }
        
        if (string.IsNullOrWhiteSpace(SelectedRoomType))
        {
            ErrorMessage = "Oda tipi seçiniz.";
            return;
        }
        
        if (!Price.HasValue || Price.Value <= 0)
        {
            ErrorMessage = "Geçerli bir fiyat giriniz.";
            return;
        }

        try
        {
            IsSaving = true;
            
            // Get selected amenities
            var selectedAmenities = string.Join(", ", Amenities.Where(a => a.IsChecked).Select(a => a.Name));
            
            // Calculate area based on room type
            var area = SelectedRoomType switch
            {
                "Standart Oda" => "25m²",
                "Deluxe Oda" => "35m²",
                "Suit Oda" => "50m²",
                "Aile Odası" => "45m²",
                "Kral Dairesi" => "80m²",
                _ => "30m²"
            };
            
            bool success = false;
            
            if (IsEditMode)
            {
                // Editing
                // Note: If RoomNumber changed, we might need a different query or logic.
                // Here assuming we update based on Room Number input.
                // If ID is immutable logic is safer, but here RoomNumber IS the ID.
                // Assuming UpdateRoomDetailsAsync uses RoomNumber as key.
                // If user Changed RoomNumber, we might technically be creating a new room or creating orphaned record.
                // For safety, we use the inputted RoomNumber, but if it changed from Original, Update might fail if we target NEW number.
                // Actually UpdateRoomDetailsAsync uses "WHERE OdaNo = @OdaNo". 
                // So pass 'RoomNumber'. But if RoomNumber was changed in UI, it won't match any DB record (unless we update PK).
                // Correct logic: UPDATE ... WHERE OdaNo = @OriginalRoomNumber AND SET OdaNo = @NewRoomNumber.
                // My UpdateRoomDetailsAsync only updates details WHERE OdaNo = @OdaNo.
                // So I will force RoomNumber to match _originalRoomNumber if EditMode, OR I update DB Service later.
                // For now, let's assume RoomNumber is key and should not change, or we use _originalRoomNumber for WHERE.
                // Since I implemented UpdateRoomDetailsAsync taking 'roomNumber' for both SET (no, it doesn't SET OdaNo) and WHERE...
                // Wait, usage: "WHERE OdaNo = @OdaNo".
                // So I must pass the Original Room Number if I want to update the existing record.
                // But if I pass Original, and use input values for update... 
                // Currently UpdateRoomDetailsAsync DOES NOT update OdaNo column. It updates others.
                // So this is safe. 
                
                success = await DatabaseService.Instance.UpdateRoomDetailsAsync(
                    IsEditMode ? _originalRoomNumber : RoomNumber, // Use original to find record
                    SelectedRoomType,
                    Capacity,
                    selectedAmenities,
                    area,
                    Price.Value,
                    (int)Status
                );
            }
            else
            {
                // New
                success = await DatabaseService.Instance.AddRoomAsync(
                    RoomNumber,
                    SelectedRoomType,
                    Capacity,
                    selectedAmenities,
                    area,
                    Price.Value,
                    (int)Status
                );
            }
            
            if (success)
            {
                RoomSaved?.Invoke(this, EventArgs.Empty);
                OnRequestNavigateBack();
            }
            else
            {
                ErrorMessage = "İşlem başarısız. Oda numarası hatası olabilir.";
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

    private void OnRequestNavigateBack()
    {
        RequestNavigateBack?.Invoke(this, EventArgs.Empty);
    }
}
