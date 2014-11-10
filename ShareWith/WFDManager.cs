using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using Windows.Devices.WiFiDirect;
using Windows.Devices.Enumeration;


namespace Buffalo.WiFiDirect
{
    public class WFDManager
    {
        private WFDDeviceDiscoveredListener wfdDeviceDiscoveredListener;
        private WFDDeviceConnectedListener wfdDeviceConnectedListener;
        private WFDPairInfo.PairSocketConnectedListener wfdPairSocketConnectedListener;

        private readonly DependencyObject parent;

        public void setWFDDeviceDiscoveredListener(WFDDeviceDiscoveredListener wfdDeviceDiscoveredListener)
        {
            this.wfdDeviceDiscoveredListener = wfdDeviceDiscoveredListener;
        }

        public void setWFDDeviceConnectedListener(WFDDeviceConnectedListener wfdDeviceConnectedListener)
        {
            this.wfdDeviceConnectedListener = wfdDeviceConnectedListener;
        }



        public WFDManager(DependencyObject parent,
                          WFDDeviceDiscoveredListener wfdDeviceDiscoveredListener,
                          WFDDeviceConnectedListener wfdDeviceConnectedListener,
                          WFDPairInfo.PairSocketConnectedListener wfdPairSocketConnectedListener
            )
        {
            this.parent = parent;
            setWFDDeviceConnectedListener(wfdDeviceConnectedListener);
            setWFDDeviceDiscoveredListener(wfdDeviceDiscoveredListener);

            //PeerFinder.Start();가 getDevicesAsync로 가야하는건가..
            /*peer Application을 찾는 프로세스를 시작하고 Application을 원격 피어에서 검색할 수 있게 만듦*/
            //    PeerFinder.Start();

            /* 상대 peer에서 connection요청이 왔을 경우 처리할 함수*/
            /// public event EventHandler<ConnectionRequestedEventArgs> ConnectionRequested;


        }

        //private delegate void WorkItemHandler(IAsyncAction operation);

        public void getDevicesAsync()
        {
            List<WFDDevice> wfdList = new List<WFDDevice>();

            /* checkPeerFinder : 
             * SupportedDiscoveryTypes 검색 옵션을 PeerFinder와 사용할 수 있는지 확인
             * PeerDiscoveryTypes.Browse는 FindAllPeersAsync, connectAsync를 사용하는데 WiFi Direct를 사용할 수 있는 지 확인
             * 
             * allowWifiDirect :
             * WiFi Direct를 이용하여 StreamSocket을 사용할 수 있는 지 확인
             */


            IAsyncAction asyncAction = ThreadPool.RunAsync(async (workItem) =>
            {
                /*to Android*/
                string wfdSelector = WiFiDirectDevice.GetDeviceSelector();
                DeviceInformationCollection devInfoCollection = await DeviceInformation.FindAllAsync(wfdSelector);

                /* to Windows
                 * PeerFinder에서 WiFi Direct를 사용할 수 있는 지 확인하여 가능 할 경우에만 FindAllPeerAsync함수를 호출한다.
                 */

                foreach (DeviceInformation devInfo in devInfoCollection)
                { /* to Android */
                    wfdList.Add(new WFDDevice(devInfo));
                }

                /*비동기 작업이 취소되면 wfdList를 clear한다*/
                if (workItem.Status == AsyncStatus.Canceled)
                {
                    wfdList.Clear();
                }


                await parent.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        //call callback
                        //WFDDeviceDiscoverdListner.onDevicesDiscovered를 통해 wfdList를 리턴한다
                        wfdDeviceDiscoveredListener.onDevicesDiscovered(wfdList);
                    });


                /*CoreWindow.GetForCurrentThread().Dispatcher.RunAsync
                    (CoreDispatcherPriority.Normal, () =>
                    {
                        l.onDevicesDiscovered(wfdList);
                    });*/
            });

            //onDevicesDiscoverFailed() 추가해야함
        }

        //private WFDDeviceConnectedListener connectedListener = null;
        /*
         * @param device : 연결하고자 하는 WFDDevice
         */
        public void pairAsync(WFDDevice device)
        {
            parent.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (device.IsDevice)
                { /*to Android*/
                    DeviceInformation devInfo = (DeviceInformation)device.WFDDeviceInfo;


                    WiFiDirectDevice wfdDevice = null;
                    try
                    {
                        wfdDevice = await WiFiDirectDevice.FromIdAsync(devInfo.Id);
                    }
                    catch (Exception e)
                    {
                        parent.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            wfdDeviceConnectedListener.onDeviceConnectFailed(10);   // <- make reason code!!!
                        });
                        return;
                    }

                    wfdDevice.ConnectionStatusChanged += new TypedEventHandler<WiFiDirectDevice, object>(async (WiFiDirectDevice sender, object arg)
                        =>
                        {
                            await parent.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                wfdDeviceConnectedListener.onDeviceDisconnected();
                            });
                        });

                    var endpointPairCollection = wfdDevice.GetConnectionEndpointPairs();
                    EndpointPair endpointPair = endpointPairCollection[0];


                    wfdDeviceConnectedListener.onDeviceConnected(new WFDPairInfo(device, endpointPair, parent));
                    //onDeviceConnectFailed(int reasonCode)추가해야함
                }
            });
        }

        public void unpair(WFDPairInfo pair)
        {
            if (pair.getWFDDevice().IsDevice)
            {
                (pair.getWFDDevice().WFDDeviceInfo as WiFiDirectDevice).Dispose();

            }
        }
    }   
}
