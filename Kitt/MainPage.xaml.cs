using Kitt.Contracts;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace Kitt
{
    public sealed partial class MainPage : Page
    {
        ObservableCollection<KeyValuePair<DateTime, StressStatus>> messages;

        double rrInterval = 1;

        int[] ledPins = { 12, 18, 13, 6, 5, 27, 4 };
        GpioPin[] pins = new GpioPin[7];
        GpioPinValue[] pinValues = { GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low };
        Rectangle[] ledRects = new Rectangle[7];

        public MainPage()
        {
            InitializeComponent();

            messages = new ObservableCollection<KeyValuePair<DateTime, StressStatus>>();
            DataContext = messages;
        }

        private void InitGpio()
        {
            //var gpio = GpioController.GetDefault();

            //if (gpio == null)
            //{
            //    return;
            //}

            ledRects[0] = Led1;
            ledRects[1] = Led2;
            ledRects[2] = Led3;
            ledRects[3] = Led4;
            ledRects[4] = Led5;
            ledRects[5] = Led6;
            ledRects[6] = Led7;

            //for (int i = 0; i < 7; i++)
            //{
            //    pins[i] = gpio.OpenPin(ledPins[i]);

            //    if (pins[i] == null)
            //    {
            //        return;
            //    }

            //    pins[i].Write(pinValues[i]);
            //    pins[i].SetDriveMode(GpioPinDriveMode.Output);
            //}
        }

        private const string DeviceConnectionString = "HostName=TheFoundation.azure-devices.net;DeviceId=Kitt;SharedAccessSignature=SharedAccessSignature sr=TheFoundation.azure-devices.net%2fdevices%2fKitt&sig=wz%2fcgqMs6NzmUFA0SvghagjMdl85W4y6%2bSPtBezBEsg%3d&se=1491856737";

        static async Task SendEvent(DeviceClient deviceClient)
        {
            string dataBuffer = JsonConvert.SerializeObject(new DistressCall { IGetYouOutOfThere = true });
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
            await deviceClient.SendEventAsync(eventMessage);
        }

        void ToggleLedAsync(int led)
        {
            if (led < 0 || led >= pins.Length)
            {
                return;
            }

            if (pinValues[led] == GpioPinValue.Low)
            {
                pinValues[led] = GpioPinValue.High;
                pins[led].Write(pinValues[led]);
                ledRects[led].Fill = new SolidColorBrush(Colors.Red);
            }
            else
            {
                pinValues[led] = GpioPinValue.Low;
                pins[led].Write(pinValues[led]);
                ledRects[led].Fill = new SolidColorBrush(Colors.Gray);
            }
        }

        async Task ReceiveCommands(DeviceClient deviceClient)
        {
            Message receivedMessage;
            string messageData;

            while (true)
            {
                receivedMessage = await deviceClient.ReceiveAsync();

                if (receivedMessage != null)
                {

                    try
                    {
                        messageData = Encoding.UTF8.GetString(receivedMessage.GetBytes());
                        StressStatus status = JsonConvert.DeserializeObject<StressStatus>(messageData);
                        rrInterval = status.RrInterval;

                        if (status.PulsFrequency > 160)
                        {
                            await SendEvent(deviceClient);
                        }

                        messages.Add(new KeyValuePair<DateTime, StressStatus>(receivedMessage.EnqueuedTimeUtc.DateTime, status));

                    }
                    catch (Exception ex)
                    {
                        messages.Add(new KeyValuePair<DateTime, StressStatus>(receivedMessage.EnqueuedTimeUtc.DateTime, new StressStatus { PulsFrequency = -1, RrInterval = 0 }));
                    }

                    await deviceClient.RejectAsync(receivedMessage);
                }
            }

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                InitGpio();

                DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString);

                ReceiveCommands(deviceClient);
                CycleLeds();
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        int leader = -1;
        int follower = -4;
        bool forward = true;
        bool active = true;
        int cycles = 11;
        double waitTime = 0.1;

        async Task CycleLeds()
        {
            while (active)
            {
                for (int i = 0; i < cycles; i++)
                {
                    waitTime = rrInterval / cycles;
                    await Task.Delay((int)(waitTime * 1000));

                    ToggleLedAsync(leader);
                    ToggleLedAsync(follower);

                    if (forward)
                    {
                        leader++;
                        follower++;
                    }
                    else
                    {
                        leader--;
                        follower--;
                    }
                }

                forward = !forward;
            }
        }

        private void Led1_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ToggleLedAsync(0);
        }

        private void Led2_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ToggleLedAsync(1);
        }

        private void Led3_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ToggleLedAsync(2);

        }

        private void Led4_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ToggleLedAsync(3);
        }

        private void Led5_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ToggleLedAsync(4);

        }

        private void Led6_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ToggleLedAsync(5);

        }

        private void Led7_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ToggleLedAsync(6);

        }
    }
}
