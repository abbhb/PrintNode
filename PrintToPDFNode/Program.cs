using PrintToPDFNode;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System;

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

            Config.toPdfIp = configs.Ip;
            Config.toPdfTopic = configs.Topic;
            Config.toPdfTopicR = configs.Rtopic;
            Config.Accesskey = configs.Accesskey;
            Config.Secretkey = configs.Secretkey;
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

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

int ports = CommonUtils.GetRandomPort();
// 修改 IP 和端口
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(System.Net.IPAddress.Parse("127.0.0.1"), ports);
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// 不断重试
bool connected1 = false;
bool connected2 = false;
while (!connected1)
{
    try
    {
        // 尝试建立连接的代码
        // 转pdf监听,处理端只用监听req的消息
        RocketMQConsumer rocketMQConsumer = new RocketMQConsumer(Config.toPdfTopic, Config.getToPdfTopicGroup(), Config.toPdfIp, ToPdfCallBack.successCallback, ToPdfCallBack.errorCallback, tags: "req");
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
        // 尝试建立连接的代码
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



app.Run();
