using NesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace NesWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        INes nes = NesFactory.New();

        WriteableBitmap wb = new WriteableBitmap(256, 240, 72, 72, PixelFormats.Pbgra32, null);

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await nes.InsertCartidgeAsync(@"C:\Users\Spike\Desktop\nestest.nes");
            new Thread(() =>
            {
                nes.PowerUp();
            }).Start();

            img.Source = wb;
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            wb.Lock();
            for (int y = 0; y < 240; ++y)
            {
                for (int x = 0; x < 256; ++x)
                {
                    int rgba = nes.GetBackgroundColor(x, y);
                    byte r = (byte)(rgba >> 24);
                    byte g = (byte)(rgba >> 16);
                    g &= 0xFF;
                    byte b = (byte)(rgba >> 8);
                    b &= 0xFF;
                    wb.SetPixel(x, y, Color.FromArgb(0xff,r, g, b));
                }
            }

            wb.Unlock();
        }
    }
}
