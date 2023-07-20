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
            Status.Content = "Status: Scanning for the ID Addr";

            await Task.Run(async () =>
            {
                addr = ((await MainWindow.mw.M.AoBScan("?0 ?? ?? ?? ?? 01 00 00 ?0 ?? ?? ?? ?? 01 00 00 00 0? 00 00 00 00 00 00 ?0 ?? ?? ?? ?? ?? 00 00 00 00 0? 0? 00 00 00 00 ?? ?? ?? ?? ?? 01 00 00 ?? ?? ?? ?? ?? 0? 00 00 ?? ?? ?F ?? ?? 0? 00 00 ?0 ?? ?? ?? ?? 01 00 00 ?0 ?? ?? ?? ?? 01 00 00 ?0 ?? ?? ?? ?? 0? 00 00 ?? ?? ?? ?? ?? ?? 00 00 ?0 ?? ?? ?? ?? 0? 00 00 ?? ?? ?? ?? ?? 0? 00 00 ?? ?? ?? ?? ?? 0? 00 00 ?? ?? ?? ?? ?? 0? 00 00 ?0 ?? ?? ?? ?? 02 00 00 ?? ?? ?? ?? ?? 02 00 00", true, true, false)).FirstOrDefault() + 0xC0).ToString("X");
            });
            if (addr != "0" || addr != "80" || addr != "128" || addr != "C0")
                Status.Content = "Status: Found the ID Addr";
            else
                Status.Content = "Status: Failed at finding the ID Addr";

            MessageBox.Show(addr);

            _ = Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(500);
                    Dispatcher.BeginInvoke((Action)delegate { });
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
