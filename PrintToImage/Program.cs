


//��ע��config
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using PrintToImage;

string configFilePath = "Config.yaml"; // �滻Ϊʵ�ʵ������ļ�·��
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
            Console.WriteLine($"toImageIp:{configs.Ip},toImageTopic{configs.Topic}");

            Config.toImageIp = configs.Ip;
            Config.toImageTopic = configs.Topic;
            Config.toImageTopicR = configs.Rtopic;
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


// תpdf����,�����ֻ�ü���req����Ϣ
RocketMQConsumer rocketMQConsumer = new RocketMQConsumer(Config.toImageTopic, Config.getToImageTopicGroup(), Config.toImageIp, ToImageCallBack.successCallback, ToImageCallBack.errorCallback, tags: "req");
rocketMQConsumer.start();

//������
RocketMQSendCenter.toImageRespSend.Start();


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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

app.MapControllers();

app.Run();
