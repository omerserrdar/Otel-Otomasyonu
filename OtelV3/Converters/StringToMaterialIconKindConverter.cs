using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Material.Icons;

namespace OtelV3.Converters
{
    public class StringToMaterialIconKindConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string iconName && Enum.TryParse<MaterialIconKind>(iconName, true, out var kind))
            {
                return kind;
            }
            return MaterialIconKind.QuestionMark; // Default fallback
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
