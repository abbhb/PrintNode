using PrintNode;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var builder = WebApplication.CreateBuilder(args);


//��ע��config
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
            Console.WriteLine($"toPdfIp:{configs.Ip},toPdfTopic{configs.Topic}");

            Config.toPrintIp = configs.Ip;
            Config.toPrintTopic = configs.Topic;
            Config.toPrintTopicR = configs.Rtopic;
            Config.tag = configs.Tag;
            Config.name = configs.Name;
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

// תpdf����,������ֻ�ü���req����Ϣ
RocketMQConsumer rocketMQConsumer = new RocketMQConsumer(Config.toPrintTopic, Config.getToPrintTopicGroup(), Config.toPrintIp, ToPrintCallBack.successCallback, ToPrintCallBack.errorCallback, tags: Config.tag);
rocketMQConsumer.start();

//������
RocketMQSendCenter.toPDFRespSend.Start();

app.Run();