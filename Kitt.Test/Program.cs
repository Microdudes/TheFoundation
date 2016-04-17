using Kitt.Contracts;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Kitt.Test
{
    class Program
    {
        static ServiceClient serviceClient;
        static string connectionString = "HostName=TheFoundation.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=EaP6bFGKAndYwwZYixzZ9wnUZu2C1gJ5Wk2QWkk+ErI=";

        static void Main(string[] args)
        {
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

            while (true)
            {
                Console.ReadKey();
                SendCloudToDeviceMessageAsync();
            }
        }

        private async static Task SendCloudToDeviceMessageAsync()
        {
            var rnd = new Random();
            var messageString = JsonConvert.SerializeObject(new StressStatus
            {
                PulsFrequency = rnd.Next(80, 200),
                RrInterval = rnd.NextDouble() + 0.5
            });
            await serviceClient.SendAsync("Kitt", new Message(Encoding.UTF8.GetBytes(messageString)));
        }
    }
}
