using System.Net.NetworkInformation;

namespace PrintToPDFNode
{
    public class CommonUtils
    {
        public static int GetRandomPort()
        {
            var random = new Random();
            var randomPort = random.Next(6000, 65535);

            while (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(p => p.Port == randomPort))
            {
                randomPort = random.Next(6000, 65535);
            }

            return randomPort;
        }
    }
}
