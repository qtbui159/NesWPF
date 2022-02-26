using NesLib;
using NesLib.Common;
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
using System.Windows.Threading;

namespace NesWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private INes nes = NesFactory.New();
        private WriteableBitmap wb = new WriteableBitmap(256, 240, 72, 72, PixelFormats.Pbgra32, null);

        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            tips.Text = "请在Release编译下运行";
#endif
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            testBtn.IsEnabled = false;
            marioBtn.IsEnabled = false;

            await nes.InsertCartidgeAsync(@".\mario.nes");

            new Thread(() =>
            {
                nes.PowerUp(Paint);
            })
            {
                IsBackground = true,
            }.Start();

            img.Source = wb;
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            testBtn.IsEnabled = false;
            marioBtn.IsEnabled = false;

            await nes.InsertCartidgeAsync(@".\nestest.nes");

            new Thread(() =>
            {
                nes.PowerUp(Paint);
            }).Start();

            img.Source = wb;
        }

        private void Paint(int[][] rgba)
        {
            byte[] data = new byte[256 * 240 * 4];

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

            Application.Current.Dispatcher.Invoke(() =>
            {
                wb.Lock();
                wb.WritePixels(new Int32Rect(0 * 8, 0 * 8, 256, 240), data, 256 * 4, 0);
                wb.Unlock();
            });
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
            string s = btn.Name;
            s = s.Substring(0, 1).ToUpper() + s.Substring(1);
            JoystickButton jb = (JoystickButton)Enum.Parse(typeof(JoystickButton), s);
            nes.P1JoystickKey(jb, true);
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Button btn = sender as Button;
            string s = btn.Name;
            s = s.Substring(0, 1).ToUpper() + s.Substring(1);
            JoystickButton jb = (JoystickButton)Enum.Parse(typeof(JoystickButton), s);
            nes.P1JoystickKey(jb, false);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                nes.P1JoystickKey(JoystickButton.Up, true);
            }
            else if (e.Key == Key.S)
            {
                nes.P1JoystickKey(JoystickButton.Down, true);
            }
            else if (e.Key == Key.A)
            {
                nes.P1JoystickKey(JoystickButton.Left, true);
            }
            else if (e.Key == Key.D)
            {
                nes.P1JoystickKey(JoystickButton.Right, true);
            }
            else if (e.Key == Key.J)
            {
                nes.P1JoystickKey(JoystickButton.A, true);
            }
            else if (e.Key == Key.K)
            {
                nes.P1JoystickKey(JoystickButton.B, true);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                nes.P1JoystickKey(JoystickButton.Up, false);
            }
            else if (e.Key == Key.S)
            {
                nes.P1JoystickKey(JoystickButton.Down, false);
            }
            else if (e.Key == Key.A)
            {
                nes.P1JoystickKey(JoystickButton.Left, false);
            }
            else if (e.Key == Key.D)
            {
                nes.P1JoystickKey(JoystickButton.Right, false);
            }
            else if (e.Key == Key.J)
            {
                nes.P1JoystickKey(JoystickButton.A, false);
            }
            else if (e.Key == Key.K)
            {
                nes.P1JoystickKey(JoystickButton.B, false);
            }
        }
    }
}
