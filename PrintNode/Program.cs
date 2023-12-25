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


// �޸� IP �Ͷ˿�
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
        builder.WithOrigins("*") // ����Ŀ�����Դ
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



// ��������
bool connected1 = false;
bool connected2 = false;
bool connected3 = false;
while (!connected1)
{
    try
    {
        // ���Խ������ӵĴ���
        // תpdf����,�����ֻ�ü���req����Ϣ
        RocketMQConsumer rocketMQConsumer = new RocketMQConsumer(Config.toPrintTopic, Config.getToPrintTopicGroup(), Config.toPrintIp, ToPrintCallBack.successCallback, ToPrintCallBack.errorCallback, tags: Config.tag);
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

        //������
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

//����������ע��
while (!connected3)
{
    try
    {
        ConsulServer.start();
        connected3 = true; // ���ӳɹ��󽫱�־λ��Ϊ true���˳�ѭ��
    }
    catch (Exception e)
    {
        // �����쳣ʱ�Ĵ���
        Console.WriteLine("��������������ʧ�ܣ�" + e.Message);

        Thread.Sleep(60 * 1000); // �ȴ�һ���ӣ��Ժ���Ϊ��λ
    }
}

app.Run();
