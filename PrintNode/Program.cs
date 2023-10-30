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

// 转pdf监听,处理端只用监听req的消息
RocketMQConsumer rocketMQConsumer = new RocketMQConsumer(Config.toPrintTopic, Config.getToPrintTopicGroup(), Config.toPrintIp, ToPrintCallBack.successCallback, ToPrintCallBack.errorCallback, tags: Config.tag);
rocketMQConsumer.start();

//生产者
RocketMQSendCenter.toPDFRespSend.Start();
ConsulServer.start();
app.Run();
