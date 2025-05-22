using PrintNode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using PrintStatus = PrintQueueApp.models.PrintStatus;

namespace PrintQueueApp.Converters
{
   public class PrinterStatusToMessageConverter : IValueConverter, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 每次转换时触发通知
            int type = (int)value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusType"));


            // 根据目标类型返回不同结果
            return targetType.Name switch
            {
                //nameof(String) => GetStatusText(statusO),   // 当绑定到Text属性时
                nameof(Brush) => GetStatusColor(type),   // 当绑定到Foreground属性时
                _ => throw new NotSupportedException()
            };
        }

        private static string GetStatusText(PrintStatus status)
        {
            return status.StatusType switch
            {
                0 => "❌ 异常："+status.StatusTypeMessage,
                1 => "🖨️ 空闲",
                2 => "✅ 繁忙",
                3 => "🕒 忙碌",
                4 => "🕒 爆满",
                _ => "❓ 未知状态"
            };
        }

        private static Brush GetStatusColor(int status)
        {
            return status switch
            {
                0 => Brushes.Red,
                1 => Brushes.LimeGreen,
                2 => Brushes.Orange,
                3 => Brushes.DodgerBlue,
                _ => Brushes.Gray
              
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
