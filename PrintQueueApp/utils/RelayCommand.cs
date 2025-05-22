using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PrintQueueApp.utils
{
    public class RelayCommand<T> : ICommand
    {
        /// <summary>
        /// 命令能否执行
        /// </summary>
        readonly Func<T, bool> _canExecute;
        /// <summary>
        /// 命令执行的方法
        /// </summary>
        readonly Action<T> _execute;

        /// <summary>
        /// 命令的构造函数
        /// </summary>
        /// <param name="action">命令需执行的方法</param>
        /// <param name="canExecute">命令是否可以执行的方法</param>
        public RelayCommand(Action<T> action, Func<T, bool> canExecute)
        {
            _execute = action;
            _canExecute = canExecute;
        }

        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(Object parameter)
        {
            if (_canExecute == null)
                return true;
            return _canExecute((T)parameter);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(Object parameter)
        {
            _execute((T)parameter);
        }
        /// <summary>
        /// 事件追加、移除
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }


    }
}
