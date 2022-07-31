using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SpectrumEngine.Tools.Output;

namespace SpectrumEngine.Client.Avalonia.Controls.DevTools;

public class OutputDecorationsConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is OutputSection section)
        {
            var decorations = "";
            if (section.Underline)
            {
                decorations = "Underline";
            }

            if (section.Strikethrough)
            {
                if (decorations.Length > 0)
                {
                    decorations += ",";
                }
                decorations += "Strikethrough";
            }
            return TextDecorationCollection.Parse(decorations);
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}