using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OtelV3.Converters
{
    public static class BooleanConverters
    {
        // Colors
        private static readonly IBrush PrimaryBrush = Brush.Parse("#11b4d4");
        private static readonly IBrush MutedBrush = Brush.Parse("#618389");
        private static readonly IBrush GreenBrush = Brush.Parse("#10b981");
        private static readonly IBrush BlackBrush = Brushes.Black; // or TextMain

        public static readonly IValueConverter IfTrueThenPrimaryElseMuted = new FuncValueConverter<bool, IBrush>(b => b ? PrimaryBrush : MutedBrush);
        
        public static readonly IValueConverter IfTrueThenGreenElseBlack = new FuncValueConverter<bool, IBrush>(b => b ? GreenBrush : BlackBrush);

        public static readonly IValueConverter IfTrueThenBoldElseNormal = new FuncValueConverter<bool, FontWeight>(b => b ? FontWeight.Bold : FontWeight.Normal);

        public static readonly IValueConverter IfTrueThen08Else1 = new FuncValueConverter<bool, double>(b => b ? 0.8 : 1.0);

        public static readonly IValueConverter IfTrueThenThicknessElse0 = new FuncValueConverter<bool, Thickness>(b => b ? new Thickness(2) : new Thickness(0));

        public static readonly IValueConverter IsStringEqual = new FuncValueConverter<object?, object?, bool>((val, param) => 
        {
            return val?.ToString() == param?.ToString();
        });
    }
}
