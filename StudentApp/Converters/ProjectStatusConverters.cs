using System.Globalization;

namespace StudentApp.Converters;

public class BoolToStatusBgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEvaluated && isEvaluated)
            return Color.FromArgb("#dcfce7"); // Green bg
        
        return Color.FromArgb("#fefce8"); // Yellow/Orange bg for Pending
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BoolToStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEvaluated && isEvaluated)
            return "Evaluado";
            
        return "En revisión";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BoolToStatusTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEvaluated && isEvaluated)
            return Color.FromArgb("#15803d"); // Green Text
            
        return Color.FromArgb("#854d0e"); // Yellow/Orange text
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BoolToStatusDotConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEvaluated && isEvaluated)
            return Color.FromArgb("#22c55e"); // Green Dot
            
        return Color.FromArgb("#f59e0b"); // Orange Dot
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
