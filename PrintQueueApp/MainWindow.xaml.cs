using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel;
using PrintQueueApp.Models;
using System.Collections.ObjectModel;
using PrintQueueApp.utils;
using System.Windows.Input;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Windows.Threading;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PrintNode;
using NewLife.RocketMQ.Protocol;
using PrintQueueApp.models;
using PrintStatus = PrintQueueApp.models.PrintStatus;
using System.Runtime.CompilerServices;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using System.Management;
using System.Text;

namespace PrintQueueApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<PrintJob> PrintJobs { get; } = new();
        public ObservableCollection<LogEntry> LogEntries { get; } = new();

        public PrintStatus PrintStatusObject {
            get;
            set;
        }= new();

        private ConsoleInterceptor _interceptor;



        // 服务状态集合
        public ObservableCollection<ServiceStatus> ServiceStatusCollection { get; }
            = new ObservableCollection<ServiceStatus>();

        // 服务后台
        private ServiceHost _serviceHost;


        // 添加加载状态属性
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));

                // 状态变化时触发命令重新验证
                CommandManager.InvalidateRequerySuggested();
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            InitializeServiceStatus();
            InitializeService();
            SetupConsoleRedirection();
            DataContext = this;
            //LoadDemoData();
        }


        // 同步打印任务方法
        public void SyncPrintJobs(List<PrintRW> sourceJobs)
        {
            var newJobsDict = sourceJobs
                .Where(rw => int.TryParse(rw.id, out _)) // 过滤无效ID
                .ToDictionary(
                    rw => int.Parse(rw.id),
                    rw => new PrintJob
                    {
                        JobId = int.Parse(rw.id),
                        JobName = rw.documentName,
                        Status = rw.jobStatus,
                        Details = {
                            new KeyValuePair<string, string>("任务ID", $"{rw.id}"),
                            new KeyValuePair<string, string>("任务名", $"{rw.documentName}"),
                            new KeyValuePair<string, string>("页数", $"{rw.pagesPrinted}/{rw.pageCount}"),
                            new KeyValuePair<string, string>("状态", rw.jobStatus),
                            new KeyValuePair<string, string>("开始时间", rw.startTime.ToString("yyyy-MM-dd HH:mm:ss"))
                        }
                        // 其他属性映射...
                    });
            // 移除不存在的老任务
            var toRemove = PrintJobs
                .Where(pj => !newJobsDict.ContainsKey(pj.JobId))
                .ToList();

            foreach (var job in toRemove)
            {
                PrintJobs.Remove(job);
            }

            // 添加或更新任务
            foreach (var kvp in newJobsDict)
            {
                var existing = PrintJobs.FirstOrDefault(pj => pj.JobId == kvp.Key);
                if (existing == null)
                {
                    PrintJobs.Add(kvp.Value);
                }
                else
                {
                    // 更新现有任务属性
                    existing.JobName = kvp.Value.JobName;
                    existing.Status = kvp.Value.Status;
                    existing.Details.Clear();
                    foreach (var detail in kvp.Value.Details)
                    {
                        existing.Details.Add(detail);
                    }
                    // 其他属性更新...
                }
            }
        }


        private void InitializeServiceStatus()
        {
            // 预置需要监控的服务
            ServiceStatusCollection.Add(new ServiceStatus
            {
                ServiceName = ServiceNames.Background
            });
            // 状态更新服务
            ServiceStatusCollection.Add(new ServiceStatus
            {
                ServiceName = ServiceNames.StatusService
            });
        }

        // 状态更新回调方法
        private void UpdateServiceStatusUI(ServiceStatus status)
        {
            Dispatcher.Invoke(() =>
            {
                var existing = ServiceStatusCollection
                    .FirstOrDefault(s => s.ServiceName == status.ServiceName);

                if (existing != null)
                {
                    existing.StatusColor = status.StatusColor;
                    existing.LastUpdate = status.LastUpdate;
                }
                else
                {
                    ServiceStatusCollection.Add(status);
                }
            });
        }

        private void UpdatePrintStatus(PrintStatus status)
        {
            Dispatcher.Invoke(() =>
            {
                //PrintStatusObject = status;
                // mock
                PrintStatusObject.StatusType = status.StatusType;
                PrintStatusObject.StatusTypeMessage = status.StatusTypeMessage;
                PrintStatusObject.ListNums = status.ListNums;
                PrintStatusObject.PrintName = status.PrintName;
                PrintStatusObject.PrintDescription = status.PrintDescription;
                PrintStatusObject.PrintJobs = status.PrintJobs;
                SyncPrintJobs(PrintStatusObject.PrintJobs);
            });
        }


        private void InitializeService()
        {
            _serviceHost = new ServiceHost(LogToUI,UpdateServiceStatusUI, UpdatePrintStatus);
            _serviceHost.Start();
        }

        private void LogToUI(string message, LogLevel logLevel)
        {
            Dispatcher.Invoke(() =>
            {
                LogEntries.Add(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Message = message,
                    Level = logLevel,
                });

                // 自动滚动到底部
                if (logScrollViewer.VerticalOffset == logScrollViewer.ScrollableHeight)
                {
                    logScrollViewer.ScrollToEnd();
                }
            });
        }

        private void LoadDemoData()
        {
            // 模拟打印任务
            var job1 = new PrintJob
            {
                JobName = "年度报告.pdf",
                Status = "等待中",
                Details = {
                  new KeyValuePair<string, string>("任务ID", "1"),

                    new KeyValuePair<string, string>("页数", "23"),
                    new KeyValuePair<string, string>("纸张类型", "A4"),
                    new KeyValuePair<string, string>("状态", "等待中")
                }
            };
            var job2 = new PrintJob
            {
                JobName = "年度报告2.pdf",
                Status = "已完成",
                Details = {
                  new KeyValuePair<string, string>("任务ID", "2"),

                    new KeyValuePair<string, string>("页数", "23"),
                    new KeyValuePair<string, string>("纸张类型", "A4"),
                    new KeyValuePair<string, string>("状态", "已完成")
                }
            };
            var job3 = new PrintJob
            {
                JobName = "年度报告3.pdf",
                Status = "进行中",
                Details = {
                 new KeyValuePair<string, string>("任务ID", "3"),

                    new KeyValuePair<string, string>("页数", "3"),
                    new KeyValuePair<string, string>("纸张类型", "A4"),
                    new KeyValuePair<string, string>("状态", "进行中")
                }
            };
            var job5 = new PrintJob
            {
                JobName = "年度报告5.pdf",
                Status = "已取消",
                Details = {
                 new KeyValuePair<string, string>("任务ID", "5"),

                    new KeyValuePair<string, string>("页数", "5"),
                    new KeyValuePair<string, string>("纸张类型", "A4"),
                    new KeyValuePair<string, string>("状态", "已取消")
                }
            };
            var job6 = new PrintJob
            {
                JobName = "年度报告6.pdf",
                Status = "错误",
                Details = {
                   new KeyValuePair<string, string>("任务ID", "6"),

                    new KeyValuePair<string, string>("页数", "5"),
                    new KeyValuePair<string, string>("纸张类型", "A4"),
                    new KeyValuePair<string, string>("状态", "错误")
                }
            };

            AddPrintJob(job3);
            AddPrintJob(job5);
            AddPrintJob(job6);
            AddPrintJob(job1);

            AddPrintJob(job2);

            // 模拟日志
            LogEntries.Add(new LogEntry
            {
                Message = "系统初始化完成",
                Level = LogLevel.Info
            });
        }

        // 最小化按钮事件
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void SetupConsoleRedirection()
        {
            // 创建自定义拦截器
            _interceptor = new ConsoleInterceptor();

            // 绑定到UI更新
            _interceptor.ConsoleOutput += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    LogToUI(e.Message, LogLevel.Info);
                });
            };

            // 重定向标准输出和错误输出
            Console.SetOut(_interceptor);
            Console.SetError(_interceptor);
         
        }

        // 自定义 TextWriter
        private class ConsoleInterceptor : TextWriter
        {
            public event EventHandler<ConsoleMessageEventArgs> ConsoleOutput;

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(string value)
            {
                RaiseEvent(value);
            }

            public override void WriteLine(string value)
            {
                RaiseEvent(value + Environment.NewLine);
            }

            private void RaiseEvent(string message)
            {
                ConsoleOutput?.Invoke(this, new ConsoleMessageEventArgs(message));
            }
        }

        // 自定义事件参数
        public class ConsoleMessageEventArgs : EventArgs
        {
            public string Message { get; }

            public ConsoleMessageEventArgs(string message)
            {
                Message = message;
            }
        }

        private void LoadConfig()
        {
            const string configFilePath = "Config.yaml";
            if (File.Exists(configFilePath))
            {
                try
                {
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    using (var reader = new StreamReader(configFilePath))
                    {
                        config1 configs = deserializer.Deserialize<config1>(reader);
                        LogEntries.Add(new LogEntry
                        {
                            Message = $"toPdfIp:{configs.Ip},toPdfTopic{configs.Topic}",
                            Level = LogLevel.Info
                        });
                        Config.toPrintIp = configs.Ip;
                        Config.toPrintTopic = configs.Topic;
                        Config.toPrintTopicR = configs.Rtopic;
                        Config.tag = configs.Tag;
                        Config.printSecret = configs.PrintSecret;
                        Config.name = configs.Name;
                        Config.Accesskey = configs.Accesskey;
                        Config.Secretkey = configs.Secretkey;
                        Config.myport = configs.Myport;
                        Config.myip = configs.Myip;
                        Config.consulip = configs.Consulip;
                        Config.consulport = configs.Consulport;
                        Config.printDeviceSettingExeName = configs.ExeName;
                        Config.driveName = configs.DriveName;
                    }
                }
                catch (Exception ex)
                {
                    LogEntries.Add(new LogEntry
                    {
                        Message = "Error loading or parsing the YAML file: " + ex.Message,
                        Level = LogLevel.Info
                    });
                }
            }
            else
            {
                LogEntries.Add(new LogEntry
                {
                    Message = "The YAML file does not exist.",
                    Level = LogLevel.Info
                });
            }

            TempFileUtil.isHavePath();

        }



        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 取消按钮事件

        /// <summary>
        /// 命令要执行的方法
        /// </summary>
        void CancelPrintJobExecute(object sender)
        {
           
            // 获取命令参数
            if (sender is not PrintJob job) return;
          
            LogEntries.Add(new LogEntry
            {
                Message = string.Format("取消了任务ID: {0}, 任务名称: {1}", job.JobId,job.JobName),
                Level = LogLevel.Info
            });


            RemovePrintJob(job);
        }

        /// <summary>
        /// 命令是否可以执行
        /// </summary>
        /// <returns></returns>
        bool CanCancelPrintJobExecute(object sender)
        { // 获取命令参数
            if (sender is not PrintJob job) return false;
            if (IsLoading) return false;
            if (job is null) return false;
            if (job.Status.Contains("等待")|| job.Status.Contains("正在后台打印") || job.Status.Contains("正在打印"))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 创建新命令
        /// </summary>
        public ICommand ClickCancelPrintJob
        {
            get
            {
                return new RelayCommand<object>(CancelPrintJobExecute, CanCancelPrintJobExecute);
            }
        }

        // 添加任务方法
        public void AddPrintJob(PrintJob job)
        {
            StartUpdate();
            job.PropertyChanged += OnPrintJobStatusChanged; // 订阅状态变更
            PrintJobs.Add(job);
            SortPrintJobs(); // 添加后立即排序
        }

        // 删除任务方法
        public void RemovePrintJob(PrintJob job)
        {
            StartUpdate();
            // 取消实际逻辑
            int jobIdToCancel = Convert.ToInt32(job.JobId); // 将此替换为要取消的作业的实际 Job ID

            // 使用 WMI 查询来获取打印队列信息
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PrintJob");

            // 获取所有打印任务
            ManagementObjectCollection printJobs = searcher.Get();

            foreach (ManagementObject printJob in printJobs)
            {
                int jobId = Convert.ToInt32(printJob["JobId"]);

                if (jobId == jobIdToCancel)
                {
                    try
                    {
                        // 取消打印任务
                        // printJob.InvokeMethod("Delete", null);
                        printJob.Delete();
                        LogToUI("已取消打印任务，Job ID: " + jobId,LogLevel.Info);
                    }
                    catch (ManagementException e)
                    {
                        LogToUI($"无法取消打印任务，Job ID:{jobId}错误消息：{e.Message} " ,LogLevel.Error);

                    }
                }
            }
            job.PropertyChanged -= OnPrintJobStatusChanged; // 取消订阅
            PrintJobs.Remove(job);
            SortPrintJobs();
        }

        // 排序方法
        private void SortPrintJobs()
        {
            LogEntries.Add(new LogEntry
            {
                Message = "任务列表发生排序",
                Level = LogLevel.Debug
            });
            // 使用稳定排序算法保持相对顺序
            var sorted = PrintJobs
                .OrderBy(j => GetStatusPriority(j.Status))
                .ThenByDescending(j => j.JobId)
                .ToList();

            // 直接操作原集合保持绑定不变
            for (int i = 0; i < sorted.Count; i++)
            {
                int currentIndex = PrintJobs.IndexOf(sorted[i]);
                if (currentIndex != i)
                {
                    PrintJobs.Move(currentIndex, i);
                }
            }
        }

        // 状态优先级映射
        private int GetStatusPriority(string status) => status switch
        {
            "进行中" => 0,
            "等待中" => 1,
            "异常" => 2,
            "已完成" => 3,
            _ => 4
        };

        // 状态变更监听
        private void OnPrintJobStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PrintJob.Status))
            {
                SortPrintJobs();
            }
        }

        // 状态更新
        // 加载控制计时器
        private DispatcherTimer _loadingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };

        // 开始更新流程
        private void StartUpdate()
        {
            IsLoading = true;

            _loadingTimer.Stop();
            _loadingTimer.Tick -= OnLoadingComplete;
            _loadingTimer.Tick += OnLoadingComplete;
            _loadingTimer.Start();
        }

        // 完成加载
        private void OnLoadingComplete(object sender, EventArgs e)
        {
            _loadingTimer.Stop();
            IsLoading = false;
        }
    }
}