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
        static void Main(string[] args)
        {
            string eventHubConnectionString = "{IoT Hub Service Bus ConnectionString}";
            string eventHubName = "TheFoundation";
            string storageAccountName = "adfrtest";
            string iotHubCon = "{IoT Hub SAS}";
            string storageAccountKey = "{Storage Account Key}";
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
