using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using PrintQueueApp.Models; // 引用模型命名空间

namespace PrintQueueApp.Converters
{
    public class LogLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not LogLevel level) return Brushes.Gray;

            return level switch
            {
                LogLevel.Info => (SolidColorBrush)Application.Current.FindResource("InfoColor"),
                LogLevel.Warning => (SolidColorBrush)Application.Current.FindResource("WarningColor"),
                LogLevel.Error => (SolidColorBrush)Application.Current.FindResource("ErrorColor"),
                LogLevel.Debug => (SolidColorBrush)Application.Current.FindResource("DebugColor"),
                _ => Brushes.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}