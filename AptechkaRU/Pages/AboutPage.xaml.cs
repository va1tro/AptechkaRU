using iText.Barcodes.Qrcode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using ZXing;
using ZXing.Common;

namespace AptechkaRU.Pages
{
    /// <summary>
    /// Логика взаимодействия для AboutPage.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
        }
        private void Btn_qrcode_Click(object sender, RoutedEventArgs e)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = 300,
                    Height = 300
                }
            };
            var result = writer.Write(@"https://music.yandex.ru/album/32210445/track/97021523?utm_source=desktop&utm_medium=copy_link");
            var bitmap = new BitmapImage();
            using (var memoryStream = new MemoryStream())
            {
                result.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;
                bitmap.BeginInit();
                bitmap.StreamSource = memoryStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
            }
            imgQr.Source = bitmap;
        }

        private void BtnBackToShop_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
