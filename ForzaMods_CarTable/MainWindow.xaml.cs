using ForzaMods_CarTable.Resources;
using Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ForzaMods_CarTable
{
    public partial class MainWindow : Window
    {
        public Mem M = new Mem();
        public static MainWindow mw;
        private bool Attached = false;
        private IEnumerable<nuint> Addresses = null;
        public bool IsGetIdsOpen = false;
        public int Times_Clicked = 0;

        public MainWindow()
        {
            InitializeComponent();
            mw = this;
            Thread AttachThread = new Thread(ForzaAttach);
            AttachThread.Start();
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
        }

        void ForzaAttach()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (M.OpenProcess("ForzaHorizon5"))
                {
                    if (Attached)
                        continue;

                    Attached = true;
                    UpdateUI("Opened Forza Process", true);
                    UpdateUI(attached: true);
                }
                else
                {
                    if (!Attached)
                        continue;

                    Attached = false;
                    UpdateUI("Doing Nothing", true);
                    UpdateUI();
                }
            }
        }

        private void Topbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                Environment.Exit(0);
        }

        private void Swap_Click(object sender, RoutedEventArgs e)
        {
            if (Regex.IsMatch(ID_Box.Text, @"^[0-9]+$") && Addresses != null)
            {
                if (int.TryParse(ID_Box.Text, out int writeValue) && Addresses != null)
                {
                    foreach (nuint addr in Addresses)
                        M.WriteMemory<int>(addr.ToString("X"), writeValue);
                }

                UpdateUI("Swapped the ID", true);

                if ((bool)MessageboxSwitch.IsChecked)
                    MessageBox.Show("Please verify if the price changed, if not buy at your own risk. We currently do not guarantee that it works 100%", "Warning");
            }
            else if (Addresses == null)
                MessageBox.Show("Scan first silly");
            else
                MessageBox.Show("Input accepts only numbers");
        }

        private async void Scan_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI("Scanning for FD Viper Address", true);
            Addresses = null;

            Thread ScanThread = new Thread(async () =>
            {
                try
                {
                    Addresses = await M.AoBScan("BB 0B 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00", true, true, false);
                }
                catch { }

                if (Addresses != null)
                    UpdateUI("Found the FD Viper Address", true);
                else
                    UpdateUI("Failed at finding the address", true);
            });
            ScanThread.Start();
        }
        private void List_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                Process.Start("explorer.exe", "https://github.com/ForzaMods/fh5idlist");
        }

        void UpdateUI(string text = "", bool status = false, bool attached = false)
        {
            if (!status)
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    Swap.IsEnabled = attached;
                    Scan.IsEnabled = attached;
                    GetIds.IsEnabled = attached;
                    ID_Box.IsEnabled = attached;
                });

            else
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    Status.Content = "Status: " + text;
                });
        }

        private void GetIds_Click(object sender, RoutedEventArgs e)
        {
            if (!IsGetIdsOpen)
            {
                var getids = new CarIds();
                getids.Show();
                IsGetIdsOpen = true;
                Times_Clicked = 0;
            }

            if (Times_Clicked > 5)
                MessageBox.Show("Stop spamming this button retard");

            Times_Clicked++;
        }
    }
}
