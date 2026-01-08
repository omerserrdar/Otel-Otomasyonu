using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OtelV3.Models;
using OtelV3.Services;

namespace OtelV3.ViewModels;

public partial class CheckoutViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<GuestCheckoutModel> _checkoutList = new();
    
    [ObservableProperty]
    private GuestCheckoutModel? _selectedGuest;
    
    [ObservableProperty]
    private ObservableCollection<InvoiceItemModel> _invoiceItems = new();
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private string _selectedFilter = "Tümü"; // Tümü, Ödeme Bekleyen, Tamamlanan
    
    [ObservableProperty]
    private decimal _collectionAmount;
    
    [ObservableProperty]
    private bool _isReceiptRequested;

    [ObservableProperty]
    private decimal _totalInvoiceAmount; // Borç (Room + Extras)
    
    [ObservableProperty]
    private decimal _totalPaidAmount; // Ödenen
    
    [ObservableProperty]
    private decimal _remainingBalance; // Kalan
    
    private readonly DatabaseService _dbService;

    public CheckoutViewModel()
    {
        _dbService = DatabaseService.Instance;
        // Initialize async void
        _ = LoadDataAsync();
    }
    
    private async Task LoadDataAsync()
    {
        var guests = await _dbService.GetCheckoutListAsync();
        
        // Filter in memory
        var query = guests.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(g => 
                g.GuestName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                g.RoomNo.Contains(SearchText));
        }
        
        if (SelectedFilter == "Ödeme Bekleyen")
        {
            query = query.Where(g => g.Balance > 0);
        }
        else if (SelectedFilter == "Tamamlanan")
        {
             query = query.Where(g => g.Balance <= 0);
        }

        CheckoutList = new ObservableCollection<GuestCheckoutModel>(query);
    }

    partial void OnSearchTextChanged(string value) => _ = LoadDataAsync();
    partial void OnSelectedFilterChanged(string value) => _ = LoadDataAsync();

    partial void OnSelectedGuestChanged(GuestCheckoutModel? value)
    {
        if (value != null)
        {
            _ = LoadInvoiceDetails(value);
            CollectionAmount = value.Balance; // Teklif edilen ödeme = kalan bakiye
        }
        else
        {
            InvoiceItems.Clear();
            CollectionAmount = 0;
            TotalInvoiceAmount = 0;
            TotalPaidAmount = 0;
            RemainingBalance = 0;
        }
    }

    private async Task LoadInvoiceDetails(GuestCheckoutModel guest)
    {
        var items = await _dbService.GetReservationDetailsAsync(guest.Id);
        InvoiceItems = new ObservableCollection<InvoiceItemModel>(items);
        
        TotalInvoiceAmount = items.Sum(x => x.Amount);
        // Balance comes from the main list query which is accurate
        RemainingBalance = guest.Balance;
        // Derived Paid Amount (Total Debt - Remaining)
        TotalPaidAmount = TotalInvoiceAmount - RemainingBalance;
    }

    [RelayCommand]
    private void SetFilter(string filter)
    {
        SelectedFilter = filter;
    }

    [RelayCommand]
    private async Task AddQuickExtra(string type)
    {
        if (SelectedGuest == null) return;
        
        decimal amount = type switch
        {
            "Su" => 20,
            "Kahve" => 60,
            "Snack" => 45,
            _ => 0
        };
        
        if (amount > 0)
        {
           var currentId = SelectedGuest.Id;
           
           await _dbService.AddReservationTransactionAsync(currentId, type, "Ekstra", amount);
           // Refresh details
           await LoadDataAsync(); // Refresh needed to update main list balance display
           
           // We need to re-select the guest because LoadDataAsync recreates the list
           var newGuest = CheckoutList.FirstOrDefault(x => x.Id == currentId);
           if (newGuest != null)
           {
               SelectedGuest = newGuest;
               // LoadInvoiceDetails will receive the new guest object
           }
        }
    }
    
    [RelayCommand]
    private async Task ProcessPayment()
    {
        if (SelectedGuest == null || CollectionAmount <= 0) return;

        // 1. Ödemeyi Kaydet
        await _dbService.AddReservationTransactionAsync(SelectedGuest.Id, "Nakit/KK Ödeme", "Tahsilat", CollectionAmount);
        
        // Notify Finance to refresh
        WeakReferenceMessenger.Default.Send(new FinanceDataChangedMessage());
        
        // 2. Kalan bakiye kontrolü (Local calculation before refresh)
        var estimatedBalance = RemainingBalance - CollectionAmount;
        
        if (estimatedBalance <= 0)
        {
            // Checkout işlemi (Veritabanında Durum=2 yapılır)
             await _dbService.CheckoutReservationAsync(SelectedGuest.Id);
             
             // Listeyi yenile (Checkout yapılanlar listeden gidecek çünkü GetCheckoutListAsync sadece Durum=1 getiriyor)
             await LoadDataAsync();
             SelectedGuest = null; 
        }
        else
        {
            var currentId = SelectedGuest.Id;
            // Sadece bakiye güncelle, checkout yapma
            await LoadDataAsync();

            var newGuest = CheckoutList.FirstOrDefault(x => x.Id == currentId);
            if (newGuest != null) SelectedGuest = newGuest;
        }
    }
}
