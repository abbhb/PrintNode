using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PrintQueueApp.Models
{
    public class PrintJob : INotifyPropertyChanged
    {
        public int JobId { get; set; } = new Random().Next(100);
        private string _jobName;

        public string JobName
        {
            get => _jobName;
            set
            {
                _jobName = value;
                OnPropertyChanged();
            }
        }
        private string _status = "未知状态";

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                // 状态变化时触发命令重新验证
                CommandManager.InvalidateRequerySuggested();
            }

        }



       
        public ObservableCollection<KeyValuePair<string, string>> Details { get; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
