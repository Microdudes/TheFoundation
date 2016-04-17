using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitt.Contracts;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Message = Microsoft.Azure.Devices.Client.Message;

namespace Semi
{
    class Program
    {
        //static ServiceClient serviceClient;
        //static string connectionString = "HostName=TheFoundation.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=EaP6bFGKAndYwwZYixzZ9wnUZu2C1gJ5Wk2QWkk+ErI=";
        //HostName=TheFoundation.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=EaP6bFGKAndYwwZYixzZ9wnUZu2C1gJ5Wk2QWkk+ErI=
        static void Main(string[] args)
        {
            string eventHubConnectionString = "Endpoint=sb://iothub-ns-thefoundat-28839-9c0bf3af4c.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=EaP6bFGKAndYwwZYixzZ9wnUZu2C1gJ5Wk2QWkk+ErI=";
            string eventHubName = "TheFoundation";
            string storageAccountName = "adfrtest";
            string iotHubCon = "HostName=TheFoundation.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=EaP6bFGKAndYwwZYixzZ9wnUZu2C1gJ5Wk2QWkk+ErI=";
            string storageAccountKey = "5k4QBUJt1xHQWU6jgkYOj5C3R1eloJE9IJvttdcPjyh69DDUHy/qD8h+ldQhqHMsKnXYI9AtxiZ9E7AyOMRyUA==";
            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageAccountName, storageAccountKey);

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, "messages/events", EventHubConsumerGroup.DefaultGroupName, iotHubCon, storageConnectionString, "messages-events");
            Console.WriteLine("Registering EventProcessor...");
            var options = new EventProcessorOptions();
            options.ExceptionReceived += (sender, e) => { Console.WriteLine(e.Exception); };

            eventProcessorHost.RegisterEventProcessorAsync<SemiEventProcessor>(options).Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
