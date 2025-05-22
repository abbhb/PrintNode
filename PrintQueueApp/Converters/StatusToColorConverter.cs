using System;
using System.Windows.Data;
using System.Windows.Media;
using PrintQueueApp.Models; // 根据实际命名空间调整

namespace PrintQueueApp.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not string status) return System.Windows.Media.Brushes.Gray;

            return status.ToLower() switch
            {
                "等待中" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243)),   // 蓝色
                "进行中" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 193, 7)),   // 黄色
                "已完成" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)),    // 绿色
                "已取消" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(158, 158, 158)), // 灰色
                "错误" => new SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54)),    // 红色
                _ => System.Windows.Media.Brushes.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}