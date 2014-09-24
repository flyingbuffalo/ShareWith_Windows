using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace ItemsTemplatePanel
{
    public class MainPageViewModel
    {
        public ObservableCollection<Data> Datas { get; set; }

        public MainPageViewModel()
        {
            this.Datas = new ObservableCollection<Data>();
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/btn_android.png", UriKind.Absolute)), Title = "01" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/btn_com.png", UriKind.Absolute)), Title = "02" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/btn_android.png", UriKind.Absolute)), Title = "03" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/btn_com.png", UriKind.Absolute)), Title = "04" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/btn_android.png", UriKind.Absolute)), Title = "05" });
        }
    }

    public class Data
    {
        public BitmapImage BitmapImage { get; set; }
        public String Title { get; set; }
    }
}
