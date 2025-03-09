using Consul;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrintNode;
using PrintQueueApp.models;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using PrintStatus = PrintQueueApp.models.PrintStatus;

namespace PrintQueueApp.utils
{
    public class ServiceHost : IDisposable
    {
        private readonly Action<string, PrintQueueApp.Models.LogLevel> _logAction;

        private readonly Action<ServiceStatus> _updateStatusAction;
        private readonly Action<PrintStatus> _updatePrintStatusAction;

        private Thread _serviceThread;
        private Thread _statusserviceThread;
        private volatile bool _isRunning;
        private string[] args;

        public ServiceHost(Action<string, PrintQueueApp.Models.LogLevel> logHandler, Action<ServiceStatus> updateStatus, Action<PrintStatus> updatePrintStatusAction)
        {
            _logAction = logHandler;
            _updateStatusAction = updateStatus;
            _updatePrintStatusAction = updatePrintStatusAction;

        }

        private void UpdateServiceStatus(string serviceName, bool isHealthy)
        {
            var status = new ServiceStatus
            {
                ServiceName = serviceName,
                StatusColor = isHealthy ? System.Windows.Media.Brushes.LimeGreen : Brushes.Red,
                LastUpdate = DateTime.Now.ToString("HH:mm:ss")
            };
            _updateStatusAction?.Invoke(status);
        }

        public void Start()
        {
            _isRunning = true;
            _serviceThread = new Thread(ServiceMainLoop)
            {
                IsBackground = true,
                Name = "BackgroundServices"
            };
            _serviceThread.Start();

            _statusserviceThread = new Thread(ServiceStatuwsUpdateLoop)
            {
                IsBackground = true,
                Name = "ServiceStatuwsUpdateServices"
            };
            _statusserviceThread.Start();
        }

        private void ServiceMainLoop()
        {
            Log("开始拉起服务-_-", PrintQueueApp.Models.LogLevel.Debug);
            // 原服务
            UpdateServiceStatus(ServiceNames.Background, false); // 初始状态为异常

            var builder = WebApplication.CreateBuilder(args);


            // 修改 IP 和端口
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(System.Net.IPAddress.Parse(Config.myip), Config.myport);
            });

            // Add services to the container.

            builder.Services.AddControllers().AddApplicationPart(typeof(PrintNode.Controllers.PrintDeviceController).Assembly) // 示例类型
    .AddApplicationPart(typeof(PrintNode.Controllers.HealthController).Assembly)
    .AddApplicationPart(typeof(PrintNode.Controllers.WeatherForecastController).Assembly);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    builder.WithOrigins("*") // 允许的跨域来源
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.UseCors("AllowSpecificOrigin");
            app.MapGet("/", () => "Hello World!");
            app.MapControllers();



            // 不断重试
            bool connected1 = false;
            bool connected2 = false;
            bool connected3 = false;
            while (!connected1)
            {
                try
                {
                    // 尝试建立连接的代码
                    // 转pdf监听,处理端只用监听req的消息
                    RocketMQConsumer rocketMQConsumer = new RocketMQConsumer(Config.toPrintTopic, Config.getToPrintTopicGroup(), Config.toPrintIp, ToPrintCallBack.successCallback, ToPrintCallBack.errorCallback, tags: Config.tag);
                    rocketMQConsumer.start();

                    connected1 = true; // 连接成功后将标志位置为 true，退出循环
                }
                catch (Exception e)
                {
                    // 发生异常时的处理
                    Log("消费者连接失败：" + e.Message, PrintQueueApp.Models.LogLevel.Error);
                    Thread.Sleep(60 * 1000); // 等待一分钟（以毫秒为单位
                }
            } 
            //生产者
            while (!connected2)
            {
                try
                {

                    //生产者
                    RocketMQSendCenter.toPDFRespSend.Start();

                    connected2 = true; // 连接成功后将标志位置为 true，退出循环
                }
                catch (Exception e)
                {
                    // 发生异常时的处理
                    Log("生产者连接失败：" + e.Message, PrintQueueApp.Models.LogLevel.Error);

                    Thread.Sleep(60 * 1000); // 等待一分钟（以毫秒为单位
                }
            }

            //服务发现中心注册
            while (!connected3)
            {
                try
                {
                    ConsulServer.start();
                    connected3 = true; // 连接成功后将标志位置为 true，退出循环
                }
                catch (Exception e)
                {
                    // 发生异常时的处理
                    Log("服务发现中心连接失败：" + e.Message, PrintQueueApp.Models.LogLevel.Error);

                    Thread.Sleep(60 * 1000); // 等待一分钟（以毫秒为单位
                }
            }
            UpdateServiceStatus(ServiceNames.Background, true); // 拉起成功
            Log("后台服务拉起成功", PrintQueueApp.Models.LogLevel.Debug);
            app.Run();


        }


        private void ServiceStatuwsUpdateLoop()
        {
            UpdateServiceStatus(ServiceNames.StatusService, true); // 拉起成功
            Log("状态更新服务拉起成功", PrintQueueApp.Models.LogLevel.Debug);

            while (true)
            {
                try
                {
                    PrintNode.PrintStatus printStatu = GetPrintDeviceStatus.getStatus();
                    PrintStatus printStatus = new PrintStatus();
                    if (printStatu != null) {
                        printStatus.ListNums = printStatu.listNums;
                        printStatus.StatusType = printStatu.statusType;
                        printStatus.StatusTypeMessage = printStatu.statusTypeMessage;
                        printStatus.PrintName = printStatu.printName;
                        printStatus.PrintJobs = printStatu.printJobs;
                        printStatus.PrintDescription = printStatu.printDescription;

                    }
                    _updatePrintStatusAction?.Invoke(printStatus);
                    // 每秒更新一次状态
                    Thread.Sleep(2000);

                }catch(Exception ex)
                {
                    Log(ex.Message, PrintQueueApp.Models.LogLevel.Error);
                }

            }
            
           


        }


        private void Log(string message, PrintQueueApp.Models.LogLevel level) =>
            _logAction?.Invoke($"{DateTime.Now:HH:mm:ss} {message}", level);

        public void Dispose()
        {
            _isRunning = false;
            _serviceThread?.Join(3000);
            _statusserviceThread?.Join(3000);
        }
    }

    public class ServiceNames
    {
        public static string Background { get; set; } = "后台服务";
        public static string StatusService { get; set; } = "状态刷新服务";
    }
}
