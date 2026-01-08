using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtelV3.Models;
using OtelV3.Services;

namespace OtelV3.ViewModels;

public partial class PersonnelViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<PersonnelModel> _personnelList = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public PersonnelViewModel()
    {
        _ = LoadPersonnelAsync();
    }

    private async Task LoadPersonnelAsync()
    {
        try
        {
            IsLoading = true;
            PersonnelList.Clear();
            
            var personnel = await DatabaseService.Instance.GetAllPersonnelAsync();
            foreach (var p in personnel)
            {
                PersonnelList.Add(p);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading personnel: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPersonnelAsync();
    }

    public event EventHandler? RequestAddPersonnel;

    [RelayCommand]
    private void AddNewPersonnel()
    {
        RequestAddPersonnel?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void EditPersonnel(PersonnelModel personnel)
    {
        // TODO: Implement edit navigation
    }

    [RelayCommand]
    private async Task DeletePersonnelAsync(PersonnelModel personnel)
    {
        if (personnel == null) return;
        
        try
        {
            var success = await DatabaseService.Instance.DeletePersonnelAsync(personnel.Id);
            if (success)
            {
                PersonnelList.Remove(personnel);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting personnel: {ex.Message}");
        }
    }
}
