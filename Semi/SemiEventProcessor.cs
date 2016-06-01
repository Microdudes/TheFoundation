using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitt.Contracts;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Semi
{
    public class SemiEventProcessor : IEventProcessor
    {
        static ServiceClient serviceClient;
        static string connectionString = "{IoT Hub SAS}";

        public Task OpenAsync(PartitionContext context)
        {
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

            Console.WriteLine("SimpleEventProcessor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);
            return Task.FromResult<object>(null);
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                var data = eventData.GetBytes();
                var message = Encoding.UTF8.GetString(data);

                Console.WriteLine(string.Format("{4} Partition: '{0}', Offset: {1}, Key: '{2}', Sequence: {3}",
                    context.Lease.PartitionId, eventData.Offset, eventData.PartitionKey, eventData.SequenceNumber, eventData.EnqueuedTimeUtc));

                var device = eventData.SystemProperties["iothub-connection-device-id"].ToString();

                if (device == "Michael")
                {
                    var status = JsonConvert.DeserializeObject<StressStatus>(message);
                    await serviceClient.SendAsync("Kitt", new Message(data));
                    Console.WriteLine("--> Routing to Kitt");
                }

                if (device == "Kitt")
                {
                    var distress = JsonConvert.DeserializeObject<DistressCall>(message);
                    await serviceClient.SendAsync("Michael", new Message(data));
                    Console.WriteLine("--> Routing to Michael");
                }
            }

            await context.CheckpointAsync();
        }
    }
}
