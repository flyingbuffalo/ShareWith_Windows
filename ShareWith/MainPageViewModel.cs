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
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic01.jpg", UriKind.Absolute)), Title = "01" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic03.jpg", UriKind.Absolute)), Title = "02" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic05.jpg", UriKind.Absolute)), Title = "03" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic04.jpg", UriKind.Absolute)), Title = "04" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic02.jpg", UriKind.Absolute)), Title = "05" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic06.jpg", UriKind.Absolute)), Title = "06" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic01.jpg", UriKind.Absolute)), Title = "07" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic02.jpg", UriKind.Absolute)), Title = "08" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic03.jpg", UriKind.Absolute)), Title = "09" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic04.jpg", UriKind.Absolute)), Title = "10" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic05.jpg", UriKind.Absolute)), Title = "11" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic06.jpg", UriKind.Absolute)), Title = "12" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic01.jpg", UriKind.Absolute)), Title = "13" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic02.jpg", UriKind.Absolute)), Title = "14" });
            this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic03.jpg", UriKind.Absolute)), Title = "15" });
            //this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic04.jpg", UriKind.Absolute)), Title = "16" });
            //this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic05.jpg", UriKind.Absolute)), Title = "17" });
            //this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic06.jpg", UriKind.Absolute)), Title = "18" });
            //this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic01.jpg", UriKind.Absolute)), Title = "19" });
            //this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic02.jpg", UriKind.Absolute)), Title = "20" });
            //this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic03.jpg", UriKind.Absolute)), Title = "21" });
            //this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic04.jpg", UriKind.Absolute)), Title = "22" });
            //this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic05.jpg", UriKind.Absolute)), Title = "23" });
            //this.Datas.Add(new Data { BitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/pic06.jpg", UriKind.Absolute)), Title = "24" });
        
        }
    }
    public class Data
    {
        public BitmapImage BitmapImage { get; set; }
        public String Title { get; set; }
    }
}
