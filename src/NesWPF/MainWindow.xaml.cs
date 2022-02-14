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
        WriteableBitmap wb1 = new WriteableBitmap(256, 240, 72, 72, PixelFormats.Pbgra32, null);

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //await nes.InsertCartidgeAsync(@"C:\Users\Spike\Desktop\nestest.nes");
            await nes.InsertCartidgeAsync(@"C:\Users\Spike\Desktop\smb.nes");

            new Thread(() =>
            {
                nes.PowerUp();
            }).Start();

            img.Source = wb;
            img1.Source = wb1;
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            //wb.Lock();
            //for (int y = 0; y < 240; ++y)
            //{
            //    for (int x = 0; x < 256; ++x)
            //    {
            //        int rgba = nes.GetBackgroundColor(x, y);
            //        byte r = (byte)(rgba >> 24);
            //        byte g = (byte)(rgba >> 16);
            //        g &= 0xFF;
            //        byte b = (byte)(rgba >> 8);
            //        b &= 0xFF;
            //        wb.SetPixel(x, y, Color.FromArgb(0xff,r, g, b));
            //    }
            //}

            //wb.Unlock();
            wb.Lock();
            for (int y = 0; y < 30; ++y)
            {
                for (int x = 0; x < 32; ++x)
                {
                    int[][] rgba = nes.GetBackgroundTileColor(x, y);
                    byte[] data = new byte[8 * 8 * 4];
                    int count = 0;

                    for (int i = 0; i < rgba.Length; ++i)
                    {
                        for (int j = 0; j < rgba[i].Length; ++j)
                        {
                            byte r = (byte)(rgba[i][j] >> 24);
                            byte g = (byte)(rgba[i][j] >> 16);
                            g &= 0xFF;
                            byte b = (byte)(rgba[i][j] >> 8);
                            b &= 0xFF;
                            byte a = (byte)(rgba[i][j] & 0xFF);
                            data[count++] = b;
                            data[count++] = g;
                            data[count++] = r;
                            data[count++] = a;
                        }
                    }


                    wb.WritePixels(new Int32Rect(x * 8, y * 8, 8, 8), data, 8 * 4, 0);
                }
            }

            wb.Unlock();
        }

        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            int[] s = nes.GetPalette();
            for (int i = 0; i < s.Length; ++i)
            {
                Grid grid = FindName($"grid{i}") as Grid;

                int rgba = s[i];
                byte r = (byte)(rgba >> 24);
                byte g = (byte)(rgba >> 16);
                g &= 0xFF;
                byte b = (byte)(rgba >> 8);
                b &= 0xFF;
                byte a = (byte)(rgba & 0xFF);

                grid.Background = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Name == "down")
            {
                nes.Down(true);
            }
            else if (btn.Name == "start")
            {
                nes.Start(true);
            }
            else if (btn.Name == "select")
            {
                nes.Select(true);
            }
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Name == "down")
            {
                nes.Down(false);
            }
            else if (btn.Name == "start")
            {
                nes.Start(false);
            }
            else if (btn.Name == "select")
            {
                nes.Select(false);
            }
        }

        private void Button_Click4(object sender, RoutedEventArgs e)
        {
            wb1.Lock();
            int[][] rgba = nes.GetSpriteTileColor();
            byte[] data = new byte[8 * 8 * 4];
            int count = 0;

            for (int i = 0; i < rgba.Length; ++i)
            {
                for (int j = 0; j < rgba[i].Length; ++j)
                {
                    byte r = (byte)(rgba[i][j] >> 24);
                    byte g = (byte)(rgba[i][j] >> 16);
                    g &= 0xFF;
                    byte b = (byte)(rgba[i][j] >> 8);
                    b &= 0xFF;
                    byte a = (byte)(rgba[i][j] & 0xFF);
                    data[count++] = b;
                    data[count++] = g;
                    data[count++] = r;
                    data[count++] = a;
                }
            }


            wb1.WritePixels(new Int32Rect(0 * 8, 0 * 8, 8, 8), data, 8 * 4, 0);

            wb1.Unlock();

            
        }
    }
}
