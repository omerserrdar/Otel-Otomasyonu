using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtelV3.Models;
using OtelV3.Services;

namespace OtelV3.ViewModels
{
    public partial class CustomersViewModel : ObservableObject
    {
        private List<CustomerModel> _allCustomers = new();
        
        [ObservableProperty]
        private ObservableCollection<CustomerModel> _customers = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedFilter = "Tümü"; // Tümü, VIP Müşteriler, Aktif Konaklayanlar, Kara Liste
        
        [ObservableProperty]
        private bool _isLoading;

        public event EventHandler? RequestAddCustomer;

        public CustomersViewModel()
        {
            _ = LoadCustomersAsync();
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                IsLoading = true;
                _allCustomers = (await DatabaseService.Instance.GetAllCustomersAsync()).ToList();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilters()
        {
            var query = _allCustomers.AsEnumerable();
            
            // Search filter (name, phone, identity)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(c => 
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.Phone?.Contains(SearchText) ?? false) ||
                    (c.IdentityNumber?.Contains(SearchText) ?? false));
            }
            
            // Category filter
            if (!string.IsNullOrEmpty(SelectedFilter) && SelectedFilter != "Tümü")
            {
                query = SelectedFilter switch
                {
                    "VIP Müşteriler" => query.Where(c => c.IsVip),
                    "Kara Liste" => query.Where(c => c.IsBlacklisted),
                    // "Aktif Konaklayanlar" would require a join with reservations - simplified for now
                    _ => query
                };
            }
            
            Customers.Clear();
            foreach (var customer in query)
            {
                Customers.Add(customer);
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
            await LoadCustomersAsync();
        }

        [RelayCommand]
        private void AddCustomer()
        {
            RequestAddCustomer?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private async Task DeleteCustomerAsync(CustomerModel customer)
        {
            if (customer == null) return;
            
            try
            {
                var success = await DatabaseService.Instance.DeleteCustomerAsync(customer.Id);
                if (success)
                {
                    _allCustomers.Remove(customer);
                    Customers.Remove(customer);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting customer: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ToggleVipAsync(CustomerModel customer)
        {
            if (customer == null) return;
            
            var newVipStatus = !customer.IsVip;
            var success = await DatabaseService.Instance.UpdateCustomerStatusAsync(customer.Id, newVipStatus, customer.IsBlacklisted);
            
            if (success)
            {
                // Refresh list or update item
                // Better to simple reload or update properties if Model was Observable. 
                // Since it's not, let's create a new instance with updated value to trigger UI update if we replaced it, 
                // BUT finding and replacing in ObservableCollection is okay.
                
                // Hack: Force UI update by replacing the item
                var index = Customers.IndexOf(customer);
                if (index != -1)
                {
                    customer.IsVip = newVipStatus;
                    // Trigger collection changed by creating a new reference or just notify? 
                    // ObservableCollection doesn't listen to property changes of items unless items implement INotifyPropertyChanged.
                    // So we must replace the item.
                    
                    var newCustomer = new CustomerModel
                    {
                        Id = customer.Id,
                        Name = customer.Name,
                        Phone = customer.Phone,
                        Email = customer.Email,
                        IdentityNumber = customer.IdentityNumber,
                        LastVisitDate = customer.LastVisitDate,
                        LastVisitStatus = customer.LastVisitStatus,
                        IsVip = newVipStatus,
                        IsBlacklisted = customer.IsBlacklisted, // Keep existing
                        VisitCount = customer.VisitCount,
                        AvatarUrl = customer.AvatarUrl
                    };
                    
                    Customers[index] = newCustomer;
                }
            }
        }

        [RelayCommand]
        private async Task ToggleBlacklistAsync(CustomerModel customer)
        {
            if (customer == null) return;
            
            var newBlacklistStatus = !customer.IsBlacklisted;
            var success = await DatabaseService.Instance.UpdateCustomerStatusAsync(customer.Id, customer.IsVip, newBlacklistStatus);
            
            if (success)
            {
                var index = Customers.IndexOf(customer);
                if (index != -1)
                {
                    customer.IsBlacklisted = newBlacklistStatus; // Update source model to persist state across filters

                    var newCustomer = new CustomerModel
                    {
                        Id = customer.Id,
                        Name = customer.Name,
                        Phone = customer.Phone,
                        Email = customer.Email,
                        IdentityNumber = customer.IdentityNumber,
                        LastVisitDate = customer.LastVisitDate,
                        LastVisitStatus = customer.LastVisitStatus,
                        IsVip = customer.IsVip, 
                        IsBlacklisted = newBlacklistStatus,
                        VisitCount = customer.VisitCount,
                        AvatarUrl = customer.AvatarUrl
                    };
                    
                    Customers[index] = newCustomer;
                }
            }
        }
    }
}
