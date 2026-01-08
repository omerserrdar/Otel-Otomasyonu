using System;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OtelV3.Services;

/// <summary>
/// Tema yönetimi servisi - Gece/Gündüz modu
/// </summary>
public partial class ThemeService : ObservableObject
{
    private static ThemeService? _instance;
    public static ThemeService Instance => _instance ??= new ThemeService();
    
    [ObservableProperty]
    private bool _isDarkMode = false;
    
    public string ThemeIcon => IsDarkMode ? "WeatherNight" : "WeatherSunny";
    public string ThemeText => IsDarkMode ? "Gece Modu" : "Gündüz Modu";
    
    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        ApplyTheme();
        OnPropertyChanged(nameof(ThemeIcon));
        OnPropertyChanged(nameof(ThemeText));
    }
    
    public void SetDarkMode(bool isDark)
    {
        IsDarkMode = isDark;
        ApplyTheme();
        OnPropertyChanged(nameof(ThemeIcon));
        OnPropertyChanged(nameof(ThemeText));
    }
    
    private void ApplyTheme()
    {
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = IsDarkMode 
                ? ThemeVariant.Dark 
                : ThemeVariant.Light;
        }
    }
}
