using ShareWith.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Foundation.Collections;
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
        WFDDevice device;
        internal WFDPairInfo pairInfo;

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
            manager = new WFDManager(this, discoveredListener, discoveredListener);
            
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
        }
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
            ObservableCollection<WFDDevice> devList = new ObservableCollection<WFDDevice>(deviceList);
           // parent.comboDeviceList.ItemsSource = devList;

            if (deviceList.Count != 0)
            {
                foreach (WFDDevice dev in deviceList)
                {
                    Debug.WriteLine(dev.Name);
                }
                //parent.comboDeviceList.SelectedIndex = 0;
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


        //connceted
        public void onDeviceConnected(WFDPairInfo pair)
        {
            parent.pairInfo = pair;

            Debug.WriteLine("paring");
            parent.TxtMessage.Text = "Device's IP Address : " + pair.getRemoteAddress();

            pair.connectSocketAsync(this);
        }

        public void onDeviceConnectFailed(int reasonCode)
        {

        }

        public void onDeviceDisconnected()
        {

        }

        public void onSocketConnected(StreamSocket s)
        {
            parent.TxtMessage.Text = "Socket Connected.";

            DataWriter writer = new DataWriter(s.OutputStream);
            writer.WriteString("ping~ping~\n");

            writer.StoreAsync();
            writer.FlushAsync();

            parent.TxtMessage.Text = "Send Message.";
            //   writer.Dispose();
            //    s.Dispose();

            // parent.manager.unpair(parent.pairInfo);
        }
    }   

}
