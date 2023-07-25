using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ForzaMods_CarTable.Resources
{
    public partial class CarIds : Window
    {
        public CarIds()
        {
            InitializeComponent();
        }

        private void Topbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private async void ScanForIDADdr_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Before starting the scan you need to be in horizon promo and hovering over the fd viper, this wont continue until you click ok");
            Status.Content = "Status: Scanning for the ID Addr";
            string address = null;

            await Task.Run(async () =>
            {
                address = (await MainWindow.mw.M.AoBScan("BB 0B 00 00 AC 00 00 00 00 00 00 00 00 00 00 00 01", true, true, false)).FirstOrDefault().ToString("X");
            });

            if (address != null)
            {
                Status.Content = "Status: Found the ID Addresses";
                Thread readThread = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(50);
                        Dispatcher.BeginInvoke((Action)delegate
                        {
                            HoveredID.Content = MainWindow.mw.M.ReadMemory<int>(address).ToString();
                        });
                    }
                });
                readThread.Start();
            }
            else
                Status.Content = "Status: Failed at finding the ID Addresses";
        }

        private void Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.IsGetIdsOpen = false;
            MainWindow.mw.Times_Clicked = 0;
            Hide();
        }
    }
}
