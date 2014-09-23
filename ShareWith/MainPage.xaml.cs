using System;
using ShareWith.Common;
using ShareWith.Controls;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;


// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace ShareWith
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : ShareWith.Common.LayoutAwarePage
    {
        public ObservableCollection<Data> Datas { get; set; }      

        public MainPage()
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.Datas = new ObservableCollection<Data>();
            Datas.Add(new Data { Image = new BitmapImage(new Uri("ms-appx:///Assets/btn_android.png", UriKind.Absolute))});

        }
      
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class Data
    {
        public BitmapImage Image { get; set; }

    }
}
