using ShareWith.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using System.ComponentModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls;
using Buffalo.WiFiDirect;

using Windows.UI.Xaml.Markup;

using Windows.Devices.WiFiDirect;
using Windows.Devices.Enumeration;
using Windows.Networking.Proximity;


// 기본 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234237에 나와 있습니다.

namespace ShareWith
{
    /// <summary>
    /// 대부분의 응용 프로그램에 공통되는 특성을 제공하는 기본 페이지입니다.
    /// </summary>
    public sealed partial class DeviceListTestPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// 이는 강력한 형식의 뷰 모델로 변경될 수 있습니다.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper는 각 페이지에서 탐색 및 프로세스 수명 관리를 
        /// 지원하는 데 사용됩니다.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public DeviceListTestPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

        }

        /// <summary>
        /// 탐색 중 전달된 콘텐츠로 페이지를 채웁니다. 이전 세션의 페이지를
        /// 다시 만들 때 저장된 상태도 제공됩니다.
        /// </summary>
        /// <param name="sender">
        /// 대개 <see cref="NavigationHelper"/>인 이벤트 소스
        /// </param>
        /// <param name="e">다음에 전달된 탐색 매개 변수를 제공하는 이벤트 데이터입니다.
        /// <see cref="Frame.Navigate(Type, Object)"/>에 전달된 매개 변수와
        /// 이전 세션 동안 이 페이지에 유지된
        /// 유지됩니다. 페이지를 처음 방문할 때는 이 상태가 null입니다.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// 응용 프로그램이 일시 중지되거나 탐색 캐시에서 페이지가 삭제된 경우
        /// 이 페이지와 관련된 상태를 유지합니다.  값은
        /// <see cref="SuspensionManager.SessionState"/>의 serialization 요구 사항을 만족해야 합니다.
        /// </summary>
        /// <param name="sender"> 대개 <see cref="NavigationHelper"/>인 이벤트 소스</param>
        /// <param name="e">serializable 상태로 채워질
        /// 빈 사전입니다.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            
        }

        #region NavigationHelper 등록

        /// 이 섹션에서 제공되는 메서드는 NavigationHelper에서
        /// 페이지의 탐색 메서드에 응답하기 위해 사용됩니다.
        /// 
        /// 페이지별 논리는 다음에 대한 이벤트 처리기에 있어야 합니다.  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// 및 <see cref="GridCS.Common.NavigationHelper.SaveState"/>입니다.
        /// 탐색 매개 변수는 LoadState 메서드 
        /// 및 이전 세션 동안 유지된 페이지 상태에서 사용할 수 있습니다.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        public void DrawDeviceList(List<String> deviceList)
        {
            imgDeviceCircle.Visibility = Visibility.Visible;
            tbDeviceName.Visibility = Visibility.Visible;

            int deviceCount = deviceList.Count;
            if (deviceCount > 0)
            {
                for (int i = 0; i < deviceCount; i++)
                {
                    double angle = ((double)i / (double)deviceCount)*360.0 - 90;
                    double xPos = _circleRadius * Math.Cos(angle * (Math.PI / 180.0)) + _centerX;
                    double yPos = _circleRadius * Math.Sin(angle * (Math.PI / 180.0)) + _centerY;

                    ImageButton deviceBtn = new ImageButton();
                    //if (deviceList[i].IsDevice)
                        deviceBtn.Style = canvasGrid.Resources["styleBtnAndroid"] as Style;
                    //else
                    //    deviceBtn.Style = canvasGrid.Resources["styleBtnComputer"] as Style;
                    deviceBtn.Tag = i;

                    deviceBtn.PointerEntered += deviceButton_PointerEntered;
                    deviceBtn.PointerExited += deviceButton_PointerExited;
                    deviceBtn.Click += deviceButton_Click;

                    Canvas.SetLeft(deviceBtn, _centerX);
                    Canvas.SetTop(deviceBtn, _centerY);
                    canvasGrid.Children.Add(deviceBtn);

                    DoubleAnimation doubleAnimationX = new DoubleAnimation();
                    doubleAnimationX.Duration = _animationDuration;
                    Storyboard.SetTargetProperty(doubleAnimationX, "(Canvas.Left)");
                    Storyboard.SetTarget(doubleAnimationX, deviceBtn);
                    doubleAnimationX.From = _centerX;
                    doubleAnimationX.To = xPos;

                    DoubleAnimation doubleAnimationY = new DoubleAnimation();
                    doubleAnimationY.Duration = _animationDuration;
                    Storyboard.SetTargetProperty(doubleAnimationY, "(Canvas.Top)");
                    Storyboard.SetTarget(doubleAnimationY, deviceBtn);
                    doubleAnimationY.From = _centerY;
                    doubleAnimationY.To = yPos;

                    Storyboard sb = new Storyboard();
                    sb.Duration = _animationDuration;
                    sb.Children.Add(doubleAnimationX);
                    sb.Children.Add(doubleAnimationY);

                    sb.Begin();
                }
            }
        }

        public void DrawDeviceListTest()
        {
            Duration duration = new Duration(TimeSpan.FromSeconds(1));

            ImageButton andBtn = new ImageButton();
            andBtn.Style = canvasGrid.Resources["styleBtnAndroid"] as Style;
            Canvas.SetLeft(andBtn, _centerX);
            Canvas.SetTop(andBtn, _centerY);

            canvasGrid.Children.Add(andBtn);

            DoubleAnimation doubleAnimationX = new DoubleAnimation();
            doubleAnimationX.Duration = duration;
            Storyboard.SetTargetProperty(doubleAnimationX, "(Canvas.Left)");
            Storyboard.SetTarget(doubleAnimationX, andBtn);
            doubleAnimationX.From = _centerX;
            doubleAnimationX.To = _centerX + 200;

            DoubleAnimation doubleAnimationY = new DoubleAnimation();
            doubleAnimationY.Duration = duration;
            Storyboard.SetTargetProperty(doubleAnimationY, "(Canvas.Top)");
            Storyboard.SetTarget(doubleAnimationY, andBtn);
            doubleAnimationY.From = _centerY;
            doubleAnimationY.To = _centerY + 200;

            Storyboard sb = new Storyboard();
            sb.Duration = duration;
            sb.Children.Add(doubleAnimationX);
            sb.Children.Add(doubleAnimationY);

            sb.Begin();
        }


        List<String> devList = new List<String>();

        private double _canvasWidth, _canvasHeight;
        private double _centerX, _centerY;
        private readonly double _circleRadius = 250.0;
        private readonly Duration _animationDuration = new Duration(TimeSpan.FromSeconds(1));
        private void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _canvasWidth = canvasGrid.ActualWidth;
            _canvasHeight = canvasGrid.ActualHeight;
            Debug.WriteLine("width=" + _canvasWidth + ", height=" + _canvasHeight);
            Debug.WriteLine("window width=" + this.ActualWidth + ", height=" + this.ActualHeight);

            _centerX = _canvasWidth/2 - 100;
            _centerY = _canvasHeight/2 - 100;

            tbDeviceName.Visibility = Visibility.Collapsed;
            Canvas.SetLeft(tbDeviceName, _canvasWidth / 2 - tbDeviceName.Width/2);
            Canvas.SetTop(tbDeviceName, _canvasHeight / 2 - tbDeviceName.Height/2);
            tbDeviceName.Text = "";

            imgDeviceCircle.Visibility = Visibility.Collapsed;
            Canvas.SetLeft(imgDeviceCircle, _canvasWidth / 2 - 270);
            Canvas.SetTop(imgDeviceCircle, _canvasHeight / 2 - 270);

            progressBackground.Visibility = Visibility.Collapsed;
            progressForegroundPath.Visibility = Visibility.Collapsed;
            Canvas.SetLeft(progressBackground, _canvasWidth / 2 - 190);
            Canvas.SetTop(progressBackground, _canvasHeight / 2 - 190);
            Canvas.SetLeft(progressForegroundPath, _canvasWidth / 2);
            Canvas.SetTop(progressForegroundPath, _canvasHeight / 2);

            tbProgressPersent.Visibility = Visibility.Collapsed;
            Canvas.SetLeft(tbProgressPersent, _canvasWidth / 2 - tbProgressPersent.Width / 2);
            Canvas.SetTop(tbProgressPersent, _canvasHeight / 2 - tbProgressPersent.Height / 2);
            imgProgressDone.Visibility = Visibility.Collapsed;
            Canvas.SetLeft(imgProgressDone, _canvasWidth / 2 - 85);
            Canvas.SetTop(imgProgressDone, _canvasHeight / 2 - 85);

            devList.Add("영민");
            devList.Add("아연");
            devList.Add("연주");
            devList.Add("한터");
            devList.Add("혜원");

            DrawDeviceList(devList);
        }

        private void deviceButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is ImageButton)
            {
                ImageButton deviceBtn = sender as ImageButton;
                string name = devList[(int)deviceBtn.Tag];
                tbDeviceName.Text = name;
            }
        }

        private void deviceButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            tbDeviceName.Text = "";
        }

        private void deviceButton_Click(object sender, RoutedEventArgs e)
        {
            startProgress();

            //progressbar thread test
            IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(async (workItem) =>
            {
                
            });

            System.Threading.Tasks.Task.Run( async () =>
            {
                for (int i = 0; i <= 500; i++)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        setProgressValue(i/5.0);
                    });

                    await System.Threading.Tasks.Task.Delay(5);
                }
            });
        }


        public void startProgress()
        {
            tbDeviceName.Text = "";
            tbDeviceName.Visibility = Visibility.Collapsed;
            imgDeviceCircle.Visibility = Visibility.Collapsed;

            tbProgressPersent.Text = "0%";
            tbProgressPersent.Visibility = Visibility.Visible;

            //remove image button
            for (int i = canvasGrid.Children.Count - 1; i >= 0; i--)
            {
                if (canvasGrid.Children[i] is ImageButton)
                {
                    canvasGrid.Children.RemoveAt(i);
                }
            }

            progressBackground.Visibility = Visibility.Visible;

            setProgressValue(0);
        }

        private readonly double _progressRadius = 155.0;
        public void setProgressValue(double persentageValue)
        {
            if (persentageValue > 100.0) persentageValue = 100.0;
            else if (persentageValue < 0.0) persentageValue = 0.0;

            if (persentageValue <= -0.001)
            {
                progressForegroundPath.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                progressForegroundPath.Visibility = Visibility.Visible;
            }

            double angle = 360.0 * persentageValue / 100.0;
            if (angle >= 360.0) angle = 359.99999;
            Debug.WriteLine("" + persentageValue + "% -> " + angle);

            //A size rotationAngle isLargeArcFlag sweepDirectionFlag endPoint
            //var stringPath = "M0,-155 A155,155 0 0 1 77.5,134.234";
            PathGeometry pathGeo = new PathGeometry();
            PathFigure figure = new PathFigure();
            ArcSegment arcSeg = new ArcSegment();
           
            arcSeg.Size = new Size(_progressRadius, _progressRadius);
            arcSeg.RotationAngle = 0;
            arcSeg.IsLargeArc = (angle>180.0 ? true : false);
            arcSeg.SweepDirection = SweepDirection.Clockwise;
            arcSeg.Point = new Point(_progressRadius * Math.Cos((angle-90) * (Math.PI / 180.0)),
                                     _progressRadius * Math.Sin((angle-90) * (Math.PI / 180.0)));

            figure.StartPoint = new Point(0, -_progressRadius);
            figure.Segments.Add(arcSeg);
            pathGeo.Figures.Add(figure);

            progressForegroundPath.Data = pathGeo;

            tbProgressPersent.Text = "" + (int)persentageValue + "%";


            if (persentageValue >= 100.0)
            {
                this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    onProgressDone();
                });
            }
        }

        private void onProgressDone()
        {
            tbProgressPersent.Text = "";
            tbProgressPersent.Visibility = Visibility.Collapsed;

            imgProgressDone.Visibility = Visibility.Visible;
        }
    }
}
