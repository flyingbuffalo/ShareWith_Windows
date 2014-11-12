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

        internal WFDManager manager;
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
            manager = new WFDManager(this, discoveredListener, discoveredListener, discoveredListener);
                       
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

        private void deviceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ImageButton)
            {
                ImageButton deviceBtn = sender as ImageButton;
                selectedDevice = devList[(int)deviceBtn.Tag];

                txtMessage.Text = "Connect to " + selectedDevice.Name;
                manager.pairAsync(selectedDevice);
            }       
        }
    }


    public class DiscoveredListener : WFDDeviceDiscoveredListener, WFDDeviceConnectedListener, WFDPairInfo.PairSocketConnectedListener
    {
        MainPage parent;
        int BLOCK_SIZE = 1024;
        StorageFile file ;

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

        public async void onSocketReceived(StreamSocket s)
        {
            parent.startProgress();
            Debug.WriteLine("RECEIVEDD ");
            StorageFolder folder = await parent.FileSave();
            Debug.WriteLine("FileSave");

            await parent.RecieveFileFromPeerAsync(s, folder);
        }

        public async void onSocketConnected(StreamSocket s)
        {
            parent.TxtMessage.Text = "Connected.";

            //File Choose
            try
            {
                file = await parent.FileChooser();
                Debug.WriteLine("FileChooser");
            }
            catch (FileNotFoundException e)
            {
                parent.TxtMessage.Text = "File not choosed.";
                s.Dispose();
                parent.manager.unpair(parent.pairInfo);
                parent.backToMainPage();
                return;
            }

            BasicProperties fileProperty = await file.GetBasicPropertiesAsync();
            if (fileProperty.Size != 0)
            {
                double fileSize = Convert.ToDouble(fileProperty.Size);
                Debug.WriteLine("onSocketConnected fileSize :" + fileSize);

                await parent.SendFileToPeerAsync(s, file);
            }   
            else 
            {
                throw new FileNotFoundException("[Exception] : File is null.");
            }
        }
    }   
}
