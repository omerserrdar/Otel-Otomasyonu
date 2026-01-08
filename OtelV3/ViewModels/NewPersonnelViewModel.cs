using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtelV3.Models;
using OtelV3.Services;

namespace OtelV3.ViewModels;

public partial class NewPersonnelViewModel : ObservableObject
{
    // Section 1: Kişisel Bilgiler
    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _tcNo = string.Empty;

    [ObservableProperty]
    private DateTime? _birthDate;

    [ObservableProperty]
    private string _gender = "Erkek";

    // Section 2: İletişim Bilgileri
    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    // Section 3: Kurumsal & Finansal
    [ObservableProperty]
    private string? _selectedDepartment;

    [ObservableProperty]
    private string? _selectedPosition;

    [ObservableProperty]
    private DateTime? _startDate = DateTime.Now;

    [ObservableProperty]
    private decimal? _salary;

    [ObservableProperty]
    private string _iban = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ObservableCollection<string> DepartmentOptions { get; } = new()
    {
        "Kat Hizmetleri (Housekeeping)",
        "Ön Büro (Front Office)",
        "Yiyecek & İçecek",
        "İnsan Kaynakları",
        "Satış & Pazarlama",
        "Teknik Servis"
    };

    public ObservableCollection<string> PositionOptions { get; } = new()
    {
        "Resepsiyonist",
        "Kat Görevlisi",
        "Garson",
        "Barmen",
        "Müdür",
        "Stajyer",
        "Aşçı",
        "Tekniker"
    };

    // Section 4: Acil Durum
    [ObservableProperty]
    private string _emergencyName = string.Empty;

    [ObservableProperty]
    private string _emergencyRelation = string.Empty;

    [ObservableProperty]
    private string _emergencyPhone = string.Empty;

    public event EventHandler? RequestNavigateBack;
    public event EventHandler? PersonnelSaved;

    [RelayCommand]
    private void SetGender(string gender)
    {
        Gender = gender;
    }

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
        
        if (string.IsNullOrWhiteSpace(TcNo))
        {
            ErrorMessage = "TC Kimlik numarası gereklidir.";
            return;
        }
        
        if (string.IsNullOrWhiteSpace(SelectedDepartment))
        {
            ErrorMessage = "Departman seçiniz.";
            return;
        }
        
        if (string.IsNullOrWhiteSpace(SelectedPosition))
        {
            ErrorMessage = "Pozisyon seçiniz.";
            return;
        }
        
        if (!Salary.HasValue || Salary.Value <= 0)
        {
            ErrorMessage = "Geçerli bir maaş giriniz.";
            return;
        }

        try
        {
            IsSaving = true;
            
            var fullName = $"{FirstName} {LastName}";
            var startDateTime = StartDate ?? DateTime.Now;
            
            var success = await DatabaseService.Instance.AddPersonnelAsync(
                fullName,
                TcNo,
                SelectedPosition,
                SelectedDepartment,
                Email,
                Phone,
                startDateTime,
                Salary.Value,
                0 // Active status
            );
            
            if (success)
            {
                PersonnelSaved?.Invoke(this, EventArgs.Empty);
                RequestNavigateBack?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = "Personel kaydedilemedi.";
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
        RequestNavigateBack?.Invoke(this, EventArgs.Empty);
    }
}
