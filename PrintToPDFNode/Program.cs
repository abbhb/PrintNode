using PrintToPDFNode;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System;

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

int ports = CommonUtils.GetRandomPort();
// �޸� IP �Ͷ˿�
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

// ��������
bool connected1 = false;
bool connected2 = false;
while (!connected1)
{
    try
    {
        // ���Խ������ӵĴ���
        // תpdf����,�����ֻ�ü���req����Ϣ
        RocketMQConsumer rocketMQConsumer = new RocketMQConsumer(Config.toPdfTopic, Config.getToPdfTopicGroup(), Config.toPdfIp, ToPdfCallBack.successCallback, ToPdfCallBack.errorCallback, tags: "req");
        rocketMQConsumer.start();

        connected1 = true; // ���ӳɹ��󽫱�־λ��Ϊ true���˳�ѭ��
    }
    catch (Exception e)
    {
        // �����쳣ʱ�Ĵ���
        Console.WriteLine("����������ʧ�ܣ�" + e.Message);
        Thread.Sleep(60 * 1000); // �ȴ�һ���ӣ��Ժ���Ϊ��λ
    }
}


//������
while (!connected2)
{
    try
    {
        // ���Խ������ӵĴ���
        RocketMQSendCenter.toPDFRespSend.Start();

        connected2 = true; // ���ӳɹ��󽫱�־λ��Ϊ true���˳�ѭ��
    }
    catch (Exception e)
    {
        // �����쳣ʱ�Ĵ���
        Console.WriteLine("����������ʧ�ܣ�" + e.Message);

        Thread.Sleep(60 * 1000); // �ȴ�һ���ӣ��Ժ���Ϊ��λ
    }
}



app.Run();
