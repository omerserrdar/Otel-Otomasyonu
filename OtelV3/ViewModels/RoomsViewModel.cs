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

public partial class RoomsViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedFilter = "Tümü"; // Tümü, Tek Kişilik, Çift Kişilik, Süit
    [ObservableProperty] private bool _isLoading;
    
    // Stats
    [ObservableProperty] private string _totalRoomsCount = "0";
    [ObservableProperty] private string _availableRoomsCount = "0";
    [ObservableProperty] private string _occupiedRoomsCount = "0";
    [ObservableProperty] private string _cleaningRoomsCount = "0";

    private List<RoomViewModel> _allRooms = new();
    public ObservableCollection<RoomViewModel> AllRooms { get; } = new();

    public RoomsViewModel()
    {
        _ = LoadRoomsAsync();
    }

    private async Task LoadRoomsAsync()
    {
        try
        {
            IsLoading = true;
            _allRooms = (await DatabaseService.Instance.GetAllRoomsAsync()).ToList();
            ApplyFilters();
            
            // Load stats
            var stats = await DatabaseService.Instance.GetRoomStatsAsync();
            TotalRoomsCount = stats.Total.ToString();
            AvailableRoomsCount = stats.Available.ToString();
            OccupiedRoomsCount = stats.Occupied.ToString();
            CleaningRoomsCount = stats.Cleaning.ToString();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading rooms: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilters()
    {
        var query = _allRooms.AsEnumerable();
        
        // Search filter (room number or guest name)
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(r => 
                r.RoomNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (r.GuestName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        
        // Room type filter
        if (!string.IsNullOrEmpty(SelectedFilter) && SelectedFilter != "Tümü")
        {
            query = query.Where(r => r.RoomType.Contains(SelectedFilter, StringComparison.OrdinalIgnoreCase));
        }
        
        AllRooms.Clear();
        foreach (var room in query)
        {
            AllRooms.Add(room);
        }
    }
    
    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedFilterChanged(string value) => ApplyFilters();

    [RelayCommand]
    private void SetFilter(string filter)
    {
        SelectedFilter = filter;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadRoomsAsync();
    }

    [RelayCommand]
    private void AddRoom()
    {
        RequestAddRoom?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task DeleteRoomAsync(RoomViewModel room)
    {
        if (room == null) return;
        
        try
        {
            var success = await DatabaseService.Instance.DeleteRoomAsync(room.RoomNumber);
            if (success)
            {
                _allRooms.Remove(room);
                AllRooms.Remove(room);
                await LoadRoomsAsync(); // Refresh stats
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting room: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private void EditRoom(RoomViewModel room)
    {
        if (room == null) return;
        RequestEditRoom?.Invoke(this, room);
    }

    [RelayCommand]
    private async Task RoomAction(RoomViewModel room)
    {
        if (room == null) return;
        
        if (room.Status == RoomStatus.Available)
        {
            // Trigger navigation to Reservation with this room pre-selected
            RequestReservation?.Invoke(this, room);
        }
        else if (room.Status == RoomStatus.Cleaning)
        {
            // Change status from Cleaning to Available
            try
            {
                var success = await DatabaseService.Instance.UpdateRoomStatusAsync(room.RoomNumber, (int)RoomStatus.Available);
                if (success)
                {
                    // Refresh the room list to show updated status
                    await LoadRoomsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating room status: {ex.Message}");
            }
        }
    }
    
    public event EventHandler? RequestAddRoom;
    public event EventHandler<RoomViewModel>? RequestEditRoom;
    public event EventHandler<RoomViewModel>? RequestReservation;
}
