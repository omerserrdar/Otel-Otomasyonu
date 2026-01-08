using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using OtelV3.Views;
using OtelV3.ViewModels;

namespace OtelV3;

public partial class App : Application
{
    /// <summary>
    /// Mevcut tema (Dark/Light)
    /// </summary>
    public static ThemeVariant CurrentTheme { get; private set; } = ThemeVariant.Light;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new DashboardWindow
            {
                DataContext = new DashboardViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    /// <summary>
    /// Temayı değiştirir (Dark/Light)
    /// </summary>
    /// <param name="theme">ThemeVariant.Dark veya ThemeVariant.Light</param>
    public static void SetTheme(ThemeVariant theme)
    {
        if (Current != null)
        {
            Current.RequestedThemeVariant = theme;
            CurrentTheme = theme;
        }
    }
    
    /// <summary>
    /// Tema değiştirir (Toggle)
    /// </summary>
    public static void ToggleTheme()
    {
        var newTheme = CurrentTheme == ThemeVariant.Light 
            ? ThemeVariant.Dark 
            : ThemeVariant.Light;
        SetTheme(newTheme);
    }
    
    /// <summary>
    /// Mevcut tema dark mı?
    /// </summary>
    public static bool IsDarkMode => CurrentTheme == ThemeVariant.Dark;
}
