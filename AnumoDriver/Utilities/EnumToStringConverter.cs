using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace AnumoDriver.Utilities;

/// <summary>
/// Converts between enum values and their string representations
/// </summary>
public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        var text = value.ToString();

        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                newText.Append(' ');
            newText.Append(text[i]);
        }

        return newText.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        var text = value.ToString();

        if (string.IsNullOrEmpty(text))
            return null;

        text = text.Replace(" ", string.Empty);

        try
        {
            return Enum.Parse(targetType, text);
        }
        catch
        {
            return null;
        }
    }
}

public class EnumToTitleConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        var text = value.ToString();

        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                newText.Append(' ');
            newText.Append(text[i]);
        }

        return newText.ToString().ToUpper();
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture) => null;
}
