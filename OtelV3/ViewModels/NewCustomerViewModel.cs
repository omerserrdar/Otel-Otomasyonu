using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtelV3.Services;

namespace OtelV3.ViewModels;

public partial class NewCustomerViewModel : ObservableObject
{
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _gender = string.Empty;
    [ObservableProperty] private string _idType = "TCKN";
    [ObservableProperty] private string _idNumber = string.Empty;
    [ObservableProperty] private string _nationality = "TR";
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _zipCode = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public ObservableCollection<string> GenderOptions { get; } = new() { "Erkek", "Kadın", "Diğer" };
    public ObservableCollection<string> IdTypeOptions { get; } = new() { "T.C. Kimlik No", "Pasaport", "Ehliyet" };
    public ObservableCollection<string> NationalityOptions { get; } = new() { "Türkiye", "ABD", "Almanya", "İngiltere", "Diğer" };

    public event EventHandler? RequestNavigateBack;
    public event EventHandler? CustomerSaved;

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;
        
        // Validation
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "Ad ve soyad gereklidir.";
            return;
        }
        
        if (string.IsNullOrWhiteSpace(IdNumber))
        {
            ErrorMessage = "Kimlik numarası gereklidir.";
            return;
        }
        
        if (string.IsNullOrWhiteSpace(Phone))
        {
            ErrorMessage = "Telefon numarası gereklidir.";
            return;
        }

        try
        {
            IsSaving = true;
            
            var fullName = $"{FirstName} {LastName}";
            
            var success = await DatabaseService.Instance.AddCustomerAsync(
                fullName,
                Phone,
                Email,
                IdNumber
            );
            
            if (success)
            {
                CustomerSaved?.Invoke(this, EventArgs.Empty);
                OnRequestNavigateBack();
            }
            else
            {
                ErrorMessage = "Müşteri kaydedilemedi.";
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
