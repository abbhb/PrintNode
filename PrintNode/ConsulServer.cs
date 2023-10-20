using Consul;

namespace PrintNode
{
    public class ConsulServer
    {
        public static void start()
        {
            
            var consulClient = new ConsulClient(p => { p.Address = new Uri($"http://{Config.consulip}:{Config.consulport}"); });//请求注册的 Consul 地址


            var httpCheck = new AgentServiceCheck()
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                Interval = TimeSpan.FromSeconds(10),//间隔固定的时间访问一次，https://localhost:44308/api/Health
                HTTP = $"http://{Config.myip}:{Config.myport}/api/Health",//健康检查地址 44308是visualstudio启动的端口
                Timeout = TimeSpan.FromSeconds(5)
            };

            var registration = new AgentServiceRegistration()
            {
                Checks = new[] { httpCheck },
                ID = Config.tag,
                Name = Config.name,
                Address = Config.myip,
                Port = Config.myport,
                Tags = new[] { "printer" }

            };

            consulClient.Agent.ServiceRegister(registration);//注册服务 
            Console.WriteLine("服务已注册。");


            // 注册程序退出事件处理程序
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                Console.WriteLine("程序退出，正在取消注册服务...");
                consulClient.Agent.ServiceDeregister(registration.ID);
            };
        }
    }
}
