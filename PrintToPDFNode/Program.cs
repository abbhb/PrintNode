using PrintToPDFNode;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// תpdf����,�����ֻ�ü���req����Ϣ
RocketMQConsumer rocketMQConsumer = new RocketMQConsumer(Config.toPdfTopic, Config.getToPdfTopicGroup(), Config.toPdfIp, ToPdfCallBack.successCallback, ToPdfCallBack.errorCallback,tags:"req");
rocketMQConsumer.start();

//������
RocketMQSendCenter.toPDFRespSend.Start();
app.Run();
