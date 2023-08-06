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
        public int Times_Clicked = 0;
        public bool IsGetIdsOpen = false;

        public CarIds()
        {
            InitializeComponent();
        }

        // move the window
        private void Topbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        // main scan thread
        private async void ScanForIDADdr_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Before starting the scan you need to be in horizon promo (NOT car collection) and hovering over the fd viper, this wont continue until you click ok");
            Status.Content = "Status: Scanning for the ID Addr";
            string address = null;

            // starts scan for id then waits till found
            await Task.Run(async () =>
            {
                address = (await MainWindow.mw.M.AoBScan("B8 0B 00 00 AC 00 00 00 00 00 ?? ?? 00 00 00 00", true, true, false)).FirstOrDefault().ToString("X");
            });


            // checks if address is null or 0, if not then makes a read thread
            if (address != null || address != "0")
            {
                Status.Content = "Status: Found the ID Addresses";
                Thread readThread = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(50);
                        Dispatcher.BeginInvoke((Action)delegate { HoveredID.Content = MainWindow.mw.M.ReadMemory<int>(address).ToString(); });
                    }
                });
                readThread.Start();
            }
            else
                Status.Content = "Status: Failed at finding the ID Addresses";
        }

        private void Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsGetIdsOpen = false;
            Times_Clicked = 0;
            Hide();
        }
    }
}
