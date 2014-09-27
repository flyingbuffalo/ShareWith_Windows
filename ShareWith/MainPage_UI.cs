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
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls;
using Buffalo.WiFiDirect;
using Windows.Devices.WiFiDirect;
using Windows.Devices.Enumeration;
using Windows.Networking.Proximity;

namespace ShareWith
{
    public sealed partial class MainPage : Page
    {
        public void DrawDeviceList(List<WFDDevice> deviceList)
        {
            btnDiscovery.Visibility = Visibility.Collapsed;

            imgDeviceCircle.Visibility = Visibility.Visible;
            tbDeviceName.Visibility = Visibility.Visible;

            int deviceCount = deviceList.Count;
            if (deviceCount > 0)
            {
                for (int i = 0; i < deviceCount; i++)
                {
                    double angle = ((double)i / (double)deviceCount) * 360.0 - 90;
                    double xPos = _circleRadius * Math.Cos(angle * (Math.PI / 180.0)) + _centerX;
                    double yPos = _circleRadius * Math.Sin(angle * (Math.PI / 180.0)) + _centerY;

                    ImageButton deviceBtn = new ImageButton();
                    if (deviceList[i].IsDevice)
                        deviceBtn.Style = canvasGrid.Resources["styleBtnAndroid"] as Style;
                    else
                        deviceBtn.Style = canvasGrid.Resources["styleBtnComputer"] as Style;
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

            _centerX = _canvasWidth / 2 - 100;
            _centerY = _canvasHeight / 2 - 100;

            tbDeviceName.Visibility = Visibility.Collapsed;
            Canvas.SetLeft(tbDeviceName, _canvasWidth / 2 - tbDeviceName.Width / 2);
            Canvas.SetTop(tbDeviceName, _canvasHeight / 2 - tbDeviceName.Height / 2);
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
            
        }

        private void deviceButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is ImageButton)
            {
                ImageButton deviceBtn = sender as ImageButton;
                WFDDevice device = devList[(int)deviceBtn.Tag];
                tbDeviceName.Text = device.Name;
            }
        }

        private void deviceButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            tbDeviceName.Text = "";
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
            arcSeg.IsLargeArc = (angle > 180.0 ? true : false);
            arcSeg.SweepDirection = SweepDirection.Clockwise;
            arcSeg.Point = new Point(_progressRadius * Math.Cos((angle - 90) * (Math.PI / 180.0)),
                                     _progressRadius * Math.Sin((angle - 90) * (Math.PI / 180.0)));

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
            Debug.WriteLine("onProgressDone");

            tbProgressPersent.Visibility = Visibility.Collapsed;

            imgProgressDone.Visibility = Visibility.Visible;


            //파일 전송 또는 수신 완료 후에 처리할 코드
            //ex) 소켓끊기, 페어링끊기, 페이지리로드or재배치 등등등
 
        }
    }
}
