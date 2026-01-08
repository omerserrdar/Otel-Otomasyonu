using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtelV3.Models;
using OtelV3.Services;

namespace OtelV3.ViewModels;

public partial class ReservationsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private DateTime? _filterCheckInDate;
    
    [ObservableProperty]
    private DateTime? _filterCheckOutDate;
    
    [ObservableProperty]
    private bool _filterStatusConfirmed = true;
    
    [ObservableProperty]
    private bool _filterStatusPending;
    
    [ObservableProperty]
    private bool _filterStatusCheckedIn;
    
    [ObservableProperty]
    private bool _filterStatusCheckedOut;
    
    [ObservableProperty]
    private string _selectedRoomTypeFilter = "Tüm Odalar";
    
    [ObservableProperty]
    private int _itemsPerPage = 10;

    [ObservableProperty]
    private bool _isLoading;
    
    private List<ReservationModel> _allReservations = new();
    
    public ObservableCollection<string> RoomTypes { get; } = new()
    {
        "Tüm Odalar",
        "Standart Tek Kişilik",
        "Deluxe Çift Kişilik",
        "King Suite",
        "Aile Odası"
    };
    
    public ObservableCollection<int> PageSizeOptions { get; } = new() { 10, 20, 50 };
    
    public ObservableCollection<ReservationModel> Reservations { get; } = new();

    public ReservationsViewModel()
    {
        _ = LoadReservationsAsync();
    }
    
    private async Task LoadReservationsAsync()
    {
        try
        {
            IsLoading = true;
            _allReservations = (await DatabaseService.Instance.GetAllReservationsAsync()).ToList();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading reservations: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private void ApplyFilters()
    {
        var query = _allReservations.AsEnumerable();
        
        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(r => 
                r.GuestName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                r.Id.ToString().Contains(SearchText) ||
                (r.RoomNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        
        // Status filters (OR logic - show if any checked status matches)
        var anyStatusSelected = FilterStatusConfirmed || FilterStatusPending || FilterStatusCheckedIn || FilterStatusCheckedOut;
        if (anyStatusSelected)
        {
            query = query.Where(r =>
                (FilterStatusConfirmed && r.Status == ReservationStatus.Confirmed) ||
                (FilterStatusPending && r.Status == ReservationStatus.Pending) ||
                (FilterStatusCheckedIn && r.Status == ReservationStatus.CheckedIn) ||
                (FilterStatusCheckedOut && r.Status == ReservationStatus.CheckedOut));
        }
        
        // Date filters
        if (FilterCheckInDate.HasValue)
        {
            query = query.Where(r => r.CheckInDate.Date >= FilterCheckInDate.Value.Date);
        }
        if (FilterCheckOutDate.HasValue)
        {
            query = query.Where(r => r.CheckOutDate.Date <= FilterCheckOutDate.Value.Date);
        }
        
        // Room type filter
        if (!string.IsNullOrEmpty(SelectedRoomTypeFilter) && SelectedRoomTypeFilter != "Tüm Odalar")
        {
            query = query.Where(r => r.RoomType == SelectedRoomTypeFilter);
        }
        
        // Apply to observable collection
        Reservations.Clear();
        foreach (var res in query.Take(ItemsPerPage))
        {
            Reservations.Add(res);
        }
    }
    
    // Property change handlers trigger filtering
    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnFilterCheckInDateChanged(DateTime? value) => ApplyFilters();
    partial void OnFilterCheckOutDateChanged(DateTime? value) => ApplyFilters();
    partial void OnFilterStatusConfirmedChanged(bool value) => ApplyFilters();
    partial void OnFilterStatusPendingChanged(bool value) => ApplyFilters();
    partial void OnFilterStatusCheckedInChanged(bool value) => ApplyFilters();
    partial void OnFilterStatusCheckedOutChanged(bool value) => ApplyFilters();
    partial void OnSelectedRoomTypeFilterChanged(string value) => ApplyFilters();
    partial void OnItemsPerPageChanged(int value) => ApplyFilters();
    
    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        FilterCheckInDate = null;
        FilterCheckOutDate = null;
        FilterStatusConfirmed = false;
        FilterStatusPending = false;
        FilterStatusCheckedIn = false;
        FilterStatusCheckedOut = false;
        SelectedRoomTypeFilter = "Tüm Odalar";
    }
    
    public event EventHandler? RequestNewReservation;

    [RelayCommand]
    private void NewReservation()
    {
        RequestNewReservation?.Invoke(this, EventArgs.Empty);
    }
    
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadReservationsAsync();
    }
}
