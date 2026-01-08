using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OtelV3.Converters;

public class FilterColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // value is SelectedFilter
        // parameter is the FilterOption for this button
        
        if (value is string selected && parameter is string option)
        {
             bool isSelected = selected == option;
             
             // If target is Brush (Background/Foreground)
             if (targetType == typeof(IBrush))
             {
                 if (isSelected)
                 {
                     // Selected State
                     // For Background: TextMainBrush (#111718)
                     // For Foreground: White
                     // We need to know if we are returning Background or Foreground.
                     // A simple way is to pass "Background" or "Foreground" as part of parameter? 
                     // Or just make two converters? 
                     // Let's assume we use this for Background, and another one or logic for foreground.
                     
                     // To keep it simple, let's just hardcode colors here or use Resources if possible (but resources access in converter is tricky).
                     return SolidColorBrush.Parse("#111718"); 
                 }
                 else
                 {
                     // Unselected State Background = White
                     return Brushes.White;
                 }
             }
        }
        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class FilterForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string selected && parameter is string option && selected == option)
        {
            return Brushes.White; 
        }
        
        // Unselected Foreground = TextSecondary (#618389)
        return SolidColorBrush.Parse("#618389");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
