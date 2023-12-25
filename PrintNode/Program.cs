using PrintNode;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var builder = WebApplication.CreateBuilder(args);


//先注入config
string configFilePath = "Config.yaml"; // 替换为实际的配置文件路径
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
            Console.WriteLine($"toPdfIp:{configs.Ip},toPdfTopic{configs.Topic}");

            Config.toPrintIp = configs.Ip;
            Config.toPrintTopic = configs.Topic;
            Config.toPrintTopicR = configs.Rtopic;
            Config.tag = configs.Tag;
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
        Console.WriteLine("Error loading or parsing the YAML file: " + ex.Message);
    }
}
else
{
    Console.WriteLine("The YAML file does not exist.");
}

TempFileUtil.isHavePath();


// 修改 IP 和端口
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(System.Net.IPAddress.Parse(Config.myip), Config.myport);
});

// Add services to the container.

builder.Services.AddControllers();
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
        Console.WriteLine("消费者连接失败：" + e.Message);
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
        Console.WriteLine("生产者连接失败：" + e.Message);

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
        Console.WriteLine("服务发现中心连接失败：" + e.Message);

        Thread.Sleep(60 * 1000); // 等待一分钟（以毫秒为单位
    }
}

app.Run();
