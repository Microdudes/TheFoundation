using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Kitt
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static ObservableCollection<KeyValuePair<DateTime, string>> messages;

        public MainPage()
        {
            InitializeComponent();

            messages = new ObservableCollection<KeyValuePair<DateTime, string>>();

        }

        // Define the connection string to connect to IoT Hub
        private const string DeviceConnectionString = "HostName=TheFoundation.azure-devices.net;DeviceId=Kitt;SharedAccessKey=lqps5V/+PpiCd8qKZDmsKh0yYylEF1JX10NemqPyM8w=";

        // Create a message and send it to IoT Hub.
        static async Task SendEvent(DeviceClient deviceClient)
        {
            string dataBuffer;
            dataBuffer = Guid.NewGuid().ToString();
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
            await deviceClient.SendEventAsync(eventMessage);
        }
        // Receive messages from IoT Hub
        static async Task ReceiveCommands(DeviceClient deviceClient)
        {
            Message receivedMessage;
            string messageData;

            while (true)
            {

                receivedMessage = await deviceClient.ReceiveAsync();

                if (receivedMessage != null)
                {
                    messageData = Encoding.UTF8.GetString(receivedMessage.GetBytes());
                    await deviceClient.CompleteAsync(receivedMessage);

                    messages.Add(new KeyValuePair<DateTime, string>(receivedMessage.EnqueuedTimeUtc.DateTime, messageData));
                }
            }

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Create the IoT Hub Device Client instance
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString);

            // Send an event
            //SendEvent(deviceClient).Wait();

            // Receive commands in the queue
            await ReceiveCommands(deviceClient);
        }
    }
}
