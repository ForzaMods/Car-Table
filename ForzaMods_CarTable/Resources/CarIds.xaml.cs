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
            string addr = "";
            Status.Content = "Scanning for the ID Addr";

            await Task.Run(async () =>
            {
                addr = ((await MainWindow.mw.M.AoBScan("?0 ?? ?? ?? ?? 01 00 00 ?0 ?? ?? ?? ?? 01 00 00 00 01 00 00 00 00 00 00 40 1D ?? ?? F7 7F 00 00 00 00 00 00 00 00 00 00 ?0 ?? ?? ?? ?? 01 00 00 01 00 00 01 01 00 00 00 FF FF FF FF 00 00 00 00 ?0 ?? ?? ?? ?? 01 00 00 ?0 ?? ?? ?? ?? 01 00 00 ?0 2? ?? ?? ?? 01 00 00 12 00 00 00 00 00 00 00 ?0 ?? ?? ?? ?? 01 00 00 29 00 00 00 00 00 00 00 ?0 ?? ?? ?? ?? 01 00 00 ?0 ?? ?? ?? ?? 01 00 00", true, true, false)).FirstOrDefault() + 128).ToString("X");
            });
            Status.Content = "Status: Found the ID Addr";

            _ = Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(500);
                    HoveredID.Content = MainWindow.mw.M.Read2Byte(addr);
                }
            });
        }

        private void Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.IsGetIdsOpen = false;
            Hide();
        }
    }
}
