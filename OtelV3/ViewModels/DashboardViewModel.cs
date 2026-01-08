using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using OtelV3.Models;
using OtelV3.Services;
using OtelV3.Views;
using MenuItem = OtelV3.Models.MenuItem;

namespace OtelV3.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _userName = "Ömer Serdar";
    
    [ObservableProperty]
    private string _userRole = "Resepsiyon Şefi";
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private string _selectedFilter = "Tümü";
    
    [ObservableProperty]
    private MenuItem? _selectedMenuItem;
    
    [ObservableProperty]
    private object? _currentPage;

    [ObservableProperty]
    private bool _isLoading;
    
    private HomeView? _homeView;
    private ReservationsView? _reservationsView;
    private NewReservationView? _newReservationView;
    private RoomsView? _roomsView;
    private NewRoomView? _newRoomView;
    
    private CustomersView? _customersView;
    private NewCustomerView? _newCustomerView;
    private CustomersViewModel? _customersVM;

    private PersonnelView? _personnelView;
    private NewPersonnelView? _newPersonnelView;
    private PersonnelViewModel? _personnelVM;
    
    // private NewPersonnelViewModel _newPersonnelVM; // Handled locally in navigation now
    
    private FinanceView? _financeView;

    private FinanceViewModel? _financeVM;
    
    private CheckoutView? _checkoutView;
    private CheckoutViewModel? _checkoutVM;
    
    public ObservableCollection<MenuItem> MenuItems { get; } = new()
    {
        new MenuItem { Title = "Genel Bakış", Icon = MaterialIconKind.ViewDashboard, IsSelected = true },
        new MenuItem { Title = "Rezervasyonlar", Icon = MaterialIconKind.CalendarMonth, IsSelected = false },
        new MenuItem { Title = "Odalar", Icon = MaterialIconKind.Bed, IsSelected = false },
        new MenuItem { Title = "Misafirler", Icon = MaterialIconKind.AccountGroup, IsSelected = false },
        new MenuItem { Title = "Personel Yönetimi", Icon = MaterialIconKind.AccountTie, IsSelected = false },
        new MenuItem { Title = "Check-Out", Icon = MaterialIconKind.Logout, IsSelected = false },
        new MenuItem { Title = "Finans & Analiz", Icon = MaterialIconKind.Finance, IsSelected = false }
    };
    
    public ObservableCollection<StatCard> StatCards { get; } = new();
    
    public ObservableCollection<Room> Rooms { get; } = new();
    
    public ObservableCollection<Reservation> Reservations { get; } = new();
    
    public string[] FilterOptions { get; } = new[]
    {
        "Tümü",
        "Boş",
        "Dolu",
        "Temizlik"
    };
    
    [ObservableProperty]
    private int _availableRoomCount;
    
    [ObservableProperty]
    private int _occupiedRoomCount;
    
    [ObservableProperty]
    private int _cleaningRoomCount;

    public DashboardViewModel()
    {
        // Initialize Home View
        _homeView = new HomeView();
        _homeView.DataContext = this;
        CurrentPage = _homeView;

        
        // Load dashboard data
        _ = LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            IsLoading = true;

            // Load stats
            var stats = await DatabaseService.Instance.GetDashboardStatsAsync();
            
            StatCards.Clear();
            StatCards.Add(new StatCard
            {
                Title = "Doluluk Oranı",
                Value = $"%{stats.OccupancyRate:F0}",
                ChangeText = "+12%",
                IsNeutralChange = false,
                IsPositiveChange = true,
                Icon = MaterialIconKind.ChartPie,
                IconBackground = new SolidColorBrush(Color.Parse("#202563eb")), // 20% opacity of foreground
                IconForeground = new SolidColorBrush(Color.Parse("#2563eb"))
            });
            StatCards.Add(new StatCard
            {
                Title = "Bugün Giriş",
                Value = stats.TodayCheckIn.ToString(),
                ChangeText = "+5%",
                IsNeutralChange = false,
                IsPositiveChange = true,
                Icon = MaterialIconKind.Login,
                IconBackground = new SolidColorBrush(Color.Parse("#2016a34a")), // 20% opacity
                IconForeground = new SolidColorBrush(Color.Parse("#16a34a"))
            });
            StatCards.Add(new StatCard
            {
                Title = "Bugün Çıkış",
                Value = stats.TodayCheckOut.ToString(),
                ChangeText = "-2%",
                IsNeutralChange = false,
                IsPositiveChange = false,
                Icon = MaterialIconKind.Logout,
                IconBackground = new SolidColorBrush(Color.Parse("#204f46e5")), // 20% opacity
                IconForeground = new SolidColorBrush(Color.Parse("#4f46e5"))
            });
            StatCards.Add(new StatCard
            {
                Title = "Temizlik Bekleyen",
                Value = stats.CleaningRooms.ToString(),
                ChangeText = "+3",
                IsNeutralChange = false,
                IsPositiveChange = false, // Increase in cleaning is usually negative/busy
                Icon = MaterialIconKind.Broom,
                IconBackground = new SolidColorBrush(Color.Parse("#20ea580c")), // 20% opacity
                IconForeground = new SolidColorBrush(Color.Parse("#ea580c"))
            });

            // Load rooms for dashboard
            Rooms.Clear();
            var rooms = await DatabaseService.Instance.GetRoomsForDashboardAsync();
            foreach (var room in rooms)
            {
                Rooms.Add(room);
            }

            // Update room counts
            var roomStats = await DatabaseService.Instance.GetRoomStatsAsync();
            AvailableRoomCount = roomStats.Available;
            OccupiedRoomCount = roomStats.Occupied;
            CleaningRoomCount = roomStats.Cleaning;

            // Load recent reservations
            Reservations.Clear();
            var reservations = await DatabaseService.Instance.GetRecentReservationsAsync(5);
            foreach (var res in reservations)
            {
                Reservations.Add(res);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshDashboardAsync()
    {
        await LoadDashboardDataAsync();
    }

    
    [RelayCommand]
    private void SelectMenuItem(MenuItem item)
    {
        if (SelectedMenuItem != null) SelectedMenuItem.IsSelected = false;
        
        SelectedMenuItem = item;
        item.IsSelected = true;
        
        foreach (var menuItem in MenuItems)
        {
            menuItem.IsSelected = (menuItem == item);
        }
        
        switch (item.Title)
        {
            case "Genel Bakış":
                 NavigateToHome();
                 break;
            case "Rezervasyonlar":
                 NavigateToReservations();
                 break;
            case "Odalar":
                 NavigateToRooms();
                 break;
            case "Misafirler":
                 NavigateToCustomers();
                 break;
            case "Personel Yönetimi":
                 NavigateToPersonnel();
                 break;
            case "Check-Out":
                 NavigateToCheckout();
                 break;
            case "Finans & Analiz":
                 NavigateToFinance();
                 break;
            default:
                 CurrentPage = null;
                 break;
        }
    }
    
    [RelayCommand]
    private void SetFilter(string filter)
    {
        SelectedFilter = filter;
    }
    
    [RelayCommand]
    private void ViewAllReservations()
    {
        var resItem = System.Linq.Enumerable.FirstOrDefault(MenuItems, m => m.Title == "Rezervasyonlar");
        if (resItem != null) SelectMenuItem(resItem);
    }
    
    [RelayCommand]
    private void Logout()
    {
        // Handle logout
    }
    
    // Theme Properties (bound from ThemeService)
    public string ThemeIcon => ThemeService.Instance.ThemeIcon;
    public string ThemeText => ThemeService.Instance.ThemeText;
    
    [RelayCommand]
    private void ToggleTheme()
    {
        ThemeService.Instance.ToggleTheme();
        OnPropertyChanged(nameof(ThemeIcon));
        OnPropertyChanged(nameof(ThemeText));
    }
    
    [RelayCommand]
    private void Search()
    {
        // Perform search
    }

    private void NavigateToHome()
    {
        if (_homeView == null)
        {
            _homeView = new HomeView();
            _homeView.DataContext = this;
        }
        CurrentPage = _homeView;
        _ = LoadDashboardDataAsync(); // Refresh data
    }

    private ReservationsViewModel? _reservationsVM;

    private void NavigateToReservations()
    {
        if (_reservationsView == null)
        {
            _reservationsView = new ReservationsView();
        }
        
        if (_reservationsVM == null)
        {
            _reservationsVM = new ReservationsViewModel();
            _reservationsVM.RequestNewReservation += (s, e) => NavigateToNewReservation();
        }
        else
        {
            _reservationsVM.RefreshCommand.Execute(null);
        }
        
        _reservationsView.DataContext = _reservationsVM;
        CurrentPage = _reservationsView;
    }

    private void NavigateToNewReservation(RoomViewModel? preSelectedRoom = null)
    {
        if (_newReservationView == null)
        {
            _newReservationView = new NewReservationView();
        }
        
        // Always reset ViewModel
        var vm = new NewReservationViewModel();
        
        if (preSelectedRoom != null)
        {
            vm.SetPreSelectedRoom(preSelectedRoom.RoomNumber);
        }
        
        vm.RequestNavigateBack += (s, e) => NavigateToReservations();
        vm.ReservationSaved += async (s, e) => await LoadDashboardDataAsync();
        _newReservationView.DataContext = vm;

        CurrentPage = _newReservationView;
    }

    private RoomsViewModel? _roomsVM;

    private void NavigateToRooms()
    {
        if (_roomsView == null)
        {
             _roomsView = new RoomsView();
        }
        
        // Always get fresh ViewModel and refresh data
        if (_roomsVM == null)
        {
            _roomsVM = new RoomsViewModel();
            _roomsVM.RequestAddRoom += (s, e) => NavigateToNewRoom();
            _roomsVM.RequestEditRoom += (s, room) => NavigateToNewRoom(room);
            _roomsVM.RequestReservation += (s, room) => NavigateToNewReservation(room);
        }
        else
        {
            // Refresh data when navigating back
            _roomsVM.RefreshCommand.Execute(null);
        }
        
        _roomsView.DataContext = _roomsVM;
        CurrentPage = _roomsView;
    }

    private void NavigateToNewRoom(RoomViewModel? roomToEdit = null)
    {
        if (_newRoomView == null)
        {
             _newRoomView = new NewRoomView();
        }
        
        // Ensure ViewModel is fresh or reset
        var vm = new NewRoomViewModel();
        
        if (roomToEdit != null)
        {
            vm.LoadForEdit(roomToEdit);
        }
        else
        {
            vm.Reset();
        }
        
        vm.RequestNavigateBack += (s, e) => NavigateToRooms();
        vm.RoomSaved += async (s, e) => 
        {
            // Only refresh dashboard, rooms list will be refreshed by NavigateToRooms
            await LoadDashboardDataAsync();
        };
        _newRoomView.DataContext = vm;
        
        CurrentPage = _newRoomView;
    }

    private void NavigateToCustomers()
    {
        if (_customersView == null)
        {
            _customersView = new CustomersView();
        }
        
        if (_customersVM == null)
        {
            _customersVM = new CustomersViewModel();
            _customersVM.RequestAddCustomer += (s, e) => NavigateToNewCustomer();
        }
        else
        {
            _customersVM.RefreshCommand.Execute(null);
        }
        
        _customersView.DataContext = _customersVM;
        CurrentPage = _customersView;
    }

    private void NavigateToNewCustomer()
    {
        if (_newCustomerView == null)
        {
            _newCustomerView = new NewCustomerView();
        }
        
        var vm = new NewCustomerViewModel();
        vm.RequestNavigateBack += (s, e) => NavigateToCustomers();
        vm.CustomerSaved += async (s, e) => await LoadDashboardDataAsync();
        _newCustomerView.DataContext = vm;
        
        CurrentPage = _newCustomerView;
    }

    private void NavigateToPersonnel()
    {
        if (_personnelView == null)
        {
            _personnelView = new PersonnelView();
        }
        
        if (_personnelVM == null)
        {
            _personnelVM = new PersonnelViewModel();
            _personnelVM.RequestAddPersonnel += (s, e) => NavigateToNewPersonnel();
        }
        else
        {
            _personnelVM.RefreshCommand.Execute(null);
        }
        
        _personnelView.DataContext = _personnelVM;
        CurrentPage = _personnelView;
    }

    private void NavigateToNewPersonnel()
    {
        if (_newPersonnelView == null)
        {
            _newPersonnelView = new NewPersonnelView();
        }
        
        // Always create fresh VM for new entry
        var vm = new NewPersonnelViewModel();
        vm.RequestNavigateBack += (s, e) => NavigateToPersonnel();
        vm.PersonnelSaved += async (s, e) => 
        {
            await LoadDashboardDataAsync(); // Refresh dashboard stats
            // The navigate back is handled by RequestNavigateBack
        };
        
        _newPersonnelView.DataContext = vm;
        CurrentPage = _newPersonnelView;
    }
    private void NavigateToFinance()
    {
        if (_financeView == null)
        {
            _financeView = new FinanceView();
        }
        
        if (_financeVM == null)
        {
            _financeVM = new FinanceViewModel();
        }
        
        _financeView.DataContext = _financeVM;
        CurrentPage = _financeView;
    }

    private void NavigateToCheckout()
    {
        if (_checkoutView == null)
        {
            _checkoutView = new CheckoutView();
        }
        
        if (_checkoutVM == null)
        {
            _checkoutVM = new CheckoutViewModel();
        }
        
        _checkoutView.DataContext = _checkoutVM;
        CurrentPage = _checkoutView;
    }
}
