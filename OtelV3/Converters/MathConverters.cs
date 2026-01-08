using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace OtelV3.Converters
{
    public static class MathConverters
    {
        public static readonly IMultiValueConverter Multiply = new FuncMultiValueConverter<object, double>(parts =>
        {
            if (parts == null) return 0;
            return parts.OfType<double>().Aggregate(1.0, (acc, val) => acc * val); // simplified
        });
    }
}
