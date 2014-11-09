using ShareWith.Common;
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Foundation.Collections;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Core;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using System.Diagnostics;
using Buffalo.WiFiDirect;
using WinRTXamlToolkit.Controls;


// 기본 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234237에 나와 있습니다.

namespace ShareWith
{
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private WFDManager manager;
        internal WFDDevice selectedDevice;
        internal WFDPairInfo pairInfo;
        internal List<WFDDevice> devList = new List<WFDDevice>();

        DiscoveredListener discoveredListener = null;

        public TextBlock TxtMessage
        {
            get { return txtMessage; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            discoveredListener = new DiscoveredListener(this);
            new WFDManager(this, discoveredListener, discoveredListener, discoveredListener);
                       
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private void btnDiscovery_Click(object sender, RoutedEventArgs e)
        {
            manager.getDevicesAsync();
            txtMessage.Text = "Finding Devices...";

            //this.Frame.Navigate(typeof(DeviceListTestPage));
        }

        private async void deviceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ImageButton)
            {
                ImageButton deviceBtn = sender as ImageButton;
                selectedDevice = devList[(int)deviceBtn.Tag];

                txtMessage.Text = "Connect to " + selectedDevice.Name;
                manager.pairAsync(selectedDevice);
            }       
        }

        /* private void deviceButton_Click(object sender, RoutedEventArgs e)
        {   // progress bar
            //progress circle test code
            startProgress();

            //make thread <3
            System.Threading.Tasks.Task.Run(async () =>
            {
                for (int i = 0; i <=  (올림)filesize/buffersize; i++)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        setProgressValue( i / 올림(filesize/buffersize) * 100);
                    });

                   // await System.Threading.Tasks.Task.Delay(5);안쓰면될걸
                }
            });
        }*/
    }


    public class DiscoveredListener : WFDDeviceDiscoveredListener, WFDDeviceConnectedListener, WFDPairInfo.PairSocketConnectedListener
    {
        MainPage parent;
        public DiscoveredListener(MainPage parent)
        {
            this.parent = parent;
        }

        public async void onDevicesDiscovered(List<WFDDevice> deviceList)
        {
            parent.devList = deviceList;

            if (deviceList.Count != 0)
            {
                foreach (WFDDevice dev in deviceList)
                {
                    Debug.WriteLine(dev.Name);
                }
                parent.DrawDeviceList(deviceList);
                parent.TxtMessage.Text = "Found " + deviceList.Count;
            }
            else
            {
                parent.TxtMessage.Text = "Found Not";
            }
        }

        public void onDevicesDiscoverFailed(int reasonCode)
        {
            
        }


        //connceted(device-paring)
        public void onDeviceConnected(WFDPairInfo pair)
        {
            parent.pairInfo = pair;

            Debug.WriteLine("MainPage : paring");
            parent.TxtMessage.Text = "Device's IP Address : " + pair.getRemoteAddress();

            pair.connectSocketAsync(this);
        }

        public void onDeviceConnectFailed(int reasonCode)
        {
            Debug.WriteLine("connection failed by reasoncode=" + reasonCode);
            parent.TxtMessage.Text = ("connection failed by reasoncode=" + reasonCode);
        }

        public void onDeviceDisconnected()
        {

        }

        public async void onSocketConnected(StreamSocket s)
        {
            parent.TxtMessage.Text = "Connected.";
            /*
            DataWriter writer = new DataWriter(s.OutputStream);
            writer.WriteString("ping~ping~\n");

            writer.StoreAsync();
            writer.FlushAsync();

            parent.TxtMessage.Text = "Send Message.";
            //   writer.Dispose();
            //    s.Dispose();
            
            // parent.manager.unpair(parent.pairInfo);
            */

            //File Send
            int BLOCK_SIZE = 1024;

            StorageFile file = await parent.FileChooser();
            Debug.WriteLine("FileChooser");
            BasicProperties fileProperty = await file.GetBasicPropertiesAsync();
            double fileSize = Convert.ToDouble(fileProperty.Size);
        //transferPercent = Convert.ToInt32(Math.Ceiling(fileSize) / BLOCK_SIZE * 100);
            await parent.SendFileToPeerAsync(s, file);

            if (fileProperty.Size != 0)
            {
                parent.startProgress();

                await System.Threading.Tasks.Task.Run(async () =>
                {
                    for (int i = 0; i <= Math.Ceiling(fileSize) / BLOCK_SIZE; i++)
                    {
                        await parent.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            parent.setProgressValue(Convert.ToInt32(Math.Ceiling(fileSize) / BLOCK_SIZE * 100));
                        });
                        // await System.Threading.Tasks.Task.Delay(5);안쓰면될걸
                    }
                });
            }
            else
            {
                throw new FileNotFoundException("[Exception] : File is null.");
            }   
        }

        void onSocketReceived(StreamSocket s)
        {
            
        }
    }   




}
