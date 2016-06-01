using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices;
using Microsoft.Band;
using Microsoft.Band.Notifications;
using Microsoft.Band.Sensors;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KITT.Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string DeviceConnectionString = "{YourDeviceConnectionString}";
        private const string DeviceConnectionStringMichael = "{YourDeviceConnectionString}";

        private DeviceClient deviceClient;
     
        private static IBandInfo bandInfo;
        public static IBandClient BandClient;

        public MainPage()
        {
            this.InitializeComponent();
            deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bool initResult = await InitBand();
            if (initResult)
            {
                IEnumerable<BandTile> tileList = await BandClient.TileManager.GetTilesAsync();
                foreach (var tile in tileList)
                {
                    if (tile.Name.Equals("K.I.T.T."))
                    {
                        InstallBtn.Content = "Reinstall";
                        await SendStatus();
                        await StartWatch();
                        await ReceiveCommands(deviceClient);
                        return;
                    }
                    else
                    {
                        InstallBtn.Content = "Install";
                    }
                }                
            }
        }

        private async void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            await RunInstaller();
            await SendStatus();
            await StartWatch();
            await ReceiveCommands(deviceClient);
        }

        private async Task GetandSendLocation(int heartRate)
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            Geolocator geolocator = new Geolocator();
            Geoposition pos = await geolocator.GetGeopositionAsync();

            StressStatus stressStatus = new StressStatus();
            stressStatus.PulsFrequency = heartRate;
            stressStatus.Latitude = pos.Coordinate.Latitude;
            stressStatus.Longitude = pos.Coordinate.Longitude;

            await SendStressStatus(stressStatus);
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
                    }
                    catch (Exception ex)
                    {
                      
                    }

                    await deviceClient.CompleteAsync(receivedMessage);
                }
            }

        }

        public static bool IsConnected()
        {
            return BandClient != null;
        }

        public static async Task FindFirstDevice()
        {
            var bandList = await BandClientManager.Instance.GetBandsAsync();
            if (bandList != null && bandList.Length > 0)
            {
                bandInfo = bandList[0];
            }
        }
        public static async Task<bool> InitBand()
        {
            if (!IsConnected())
            {
                await FindFirstDevice();
                if (bandInfo != null)
                {
                    BandClient = await BandClientManager.Instance.ConnectAsync(bandInfo);
                    return BandClient != null;
                }
            }
            return false;
        }

        public static Guid tileID = new Guid("D781F673-6D05-4D69-BCFF-EA7E705C3418");

        public async Task StartWatch()
        {
            if (BandClient.SensorManager.HeartRate.GetCurrentUserConsent() != UserConsent.Granted && BandClient.SensorManager.RRInterval.GetCurrentUserConsent() != UserConsent.Granted)
            {
                await BandClient.SensorManager.RRInterval.RequestUserConsentAsync();
                await BandClient.SensorManager.HeartRate.RequestUserConsentAsync();
            }
            else if (BandClient.SensorManager.HeartRate.GetCurrentUserConsent() == UserConsent.Granted && BandClient.SensorManager.RRInterval.GetCurrentUserConsent() == UserConsent.Granted)
            {
                if (IsConnected())
                {
                    await BandClient.SensorManager.RRInterval.StartReadingsAsync();
                    await BandClient.SensorManager.HeartRate.StartReadingsAsync();
                    BandClient.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;
                    BandClient.SensorManager.RRInterval.ReadingChanged += RRInterval_ReadingChanged;
                }
            }
        }

        private void RRInterval_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandRRIntervalReading> e)
        {
            throw new NotImplementedException();
        }

        public static async Task RunInstaller()
        {
            var designed = new Band_Layout();
            await BandClient.TileManager.RemoveTileAsync(tileID);
            BandTile myTile = new BandTile(tileID)
            {
                Name = "K.I.T.T.",
                TileIcon = await LoadIcon("ms-appx:///Assets/TileIconLarge.png"),
                SmallIcon = await LoadIcon("ms-appx:///Assets/TileIconSmall.png")
            };
            myTile.PageLayouts.Add(designed.Layout);
            await BandClient.TileManager.AddTileAsync(myTile);
            await BandClient.TileManager.SetPagesAsync(tileID, new PageData(Guid.NewGuid(), 0, designed.Data.All));
        }

        private static async Task<BandIcon> LoadIcon(string uri)
        {
            StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                WriteableBitmap bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return bitmap.ToBandIcon();
            }
        }

        private async Task SendStatus()
        {
            string dataBuffer = JsonConvert.SerializeObject(new DistressCall { IAmConnected = true });
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
            await deviceClient.SendEventAsync(eventMessage);
        }

        private async Task SendStressStatus(StressStatus status)
        {
            string dataBuffer = JsonConvert.SerializeObject(status);
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
            await deviceClient.SendEventAsync(eventMessage);
        }

        private async Task SendCloudToDeviceMessageAsync()
        {
            var rnd = new Random();
            var messageString = JsonConvert.SerializeObject(new StressStatus
            {
                PulsFrequency = rnd.Next(80, 200),
                RrInterval = rnd.NextDouble() + 0.5
            });
            // await serviceClient.SendAsync("Kitt", new Message(Encoding.UTF8.GetBytes(messageString)));
        }
        public static async Task SendMessage(string message)
        {
            await BandClient.NotificationManager.SendMessageAsync(tileID, "Hallo Michael!", message, DateTimeOffset.Now, MessageFlags.ShowDialog);
        }
        private async void HeartRate_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> e)
        {
            Debug.WriteLine(e.SensorReading.HeartRate);
            if (e.SensorReading.HeartRate >= 160)
            {
                await BandClient.SensorManager.HeartRate.StopReadingsAsync();
                await GetandSendLocation(e.SensorReading.HeartRate);
                await BandClient.SensorManager.HeartRate.StartReadingsAsync();
            }
        }
    }
}
