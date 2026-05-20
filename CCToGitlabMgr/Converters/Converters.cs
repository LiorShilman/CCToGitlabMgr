using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CCToGitlabMgr.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            if (parameter?.ToString() == "Invert") flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }

    public class StepStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status)
                {
                    case "Completed": return new SolidColorBrush(Color.FromRgb(0x3E, 0xCF, 0x8E));
                    case "InProgress": return new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x35));
                    case "Error": return new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                    default: return new SolidColorBrush(Color.FromRgb(0x5A, 0x64, 0x89));
                }
            }
            return new SolidColorBrush(Color.FromRgb(0x5A, 0x64, 0x89));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Returns Visibility.Visible when value equals parameter, Collapsed otherwise.
    /// Also works as bool converter for RadioButton IsChecked bindings.
    /// </summary>
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEqual = value?.ToString() == parameter?.ToString();

            if (targetType == typeof(Visibility))
                return isEqual ? Visibility.Visible : Visibility.Collapsed;

            return isEqual;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
            {
                // Try to convert parameter to the target type
                if (targetType == typeof(int) && int.TryParse(parameter?.ToString(), out int intVal))
                    return intVal;
                return parameter?.ToString();
            }
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Returns Visible when string is not null/empty, Collapsed otherwise.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Returns Visible when int value > 0, Collapsed otherwise.
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int n && n > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && !b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && !b;
        }
    }
}
