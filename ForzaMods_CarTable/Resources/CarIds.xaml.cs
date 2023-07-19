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

            Thread ReadThread = new Thread(() =>
            {
                while (true)
                {
                    HoveredID.Content = MainWindow.mw.M.Read2Byte(addr);
                }
            });

            Thread ScanThread = new Thread(async () =>
            {
                addr = (await MainWindow.mw.M.AoBScan("", true, true, false)).FirstOrDefault().ToString("X");
                ReadThread.Start();
            });
            ScanThread.Start();
            Status.Content = "Status: Found the ID Addr";
        }

        private void Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.IsGetIdsOpen = false;
            Hide();
        }
    }
}
