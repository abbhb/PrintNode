using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace PrintQueueApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 捕获 UI 线程未处理异常
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // 捕获非 UI 线程未处理异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            e.Handled = false; // 阻止应用崩溃（慎用！仅用于已知可恢复异常）
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException((Exception)e.ExceptionObject);
        }

        private void LogException(Exception ex)
        {
            // 记录到文件
            File.AppendAllText("crash.log", $"[{DateTime.Now}] CRASH: {ex}\n\n");

            // 可选：弹窗提示
            MessageBox.Show($"程序崩溃：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
