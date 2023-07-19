using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ForzaMods_CarTable.Resources;
using Memory;

namespace ForzaMods_CarTable
{
    public partial class MainWindow : Window
    {
        public Mem M = new Mem();
        public static MainWindow mw;
        private bool Attached = false;
        private bool ShowMessageBox = true;
        private string Address = "";
        public bool IsGetIdsOpen = false;
        public bool GlobalBeta = false; // Set to false for release on github, true is for discord tests
        public bool FeatureBeta = true; 
        private string FeatureString = "The Car ID Getting is only a gui part, the app may crash when you click scan"; // Set it when a feature is in beta

        public MainWindow()
        {
            InitializeComponent();
            mw = this;
            Thread AttachThread = new Thread(ForzaAttach);
            AttachThread.Start();
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");

            if (GlobalBeta)
                MessageBox.Show("This is a beta release, not everything has to work");

            if (FeatureBeta)
                MessageBox.Show(FeatureString);
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
                    UpdateUI("Opened Forza Process", "Status");
                    UpdateUI("", "");
                }
                else
                {
                    if (!Attached)
                        continue;

                    Attached = false;
                    UpdateUI("", "");
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
            if (IsNumericInput(ID_Box.Text) && Address != "0")
            {
                M.WriteMemory(Address, "2bytes", ID_Box.Text);
                UpdateUI("Swapped the ID", "Status");

                if (ShowMessageBox)
                    MessageBox.Show("Please verify if the price changed, if not buy at your own risk. We currently do not guarantee that it works 100%", "Warning");
            }
            else
                MessageBox.Show("Input accepts only numbers");
        }

        private bool IsNumericInput(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]+$");
        }

        private async void Scan_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI("Scanning for FD Viper Address", "Status");

            
            Thread ScanThread = new Thread(async () =>
            {
                try
                {
                    Address = (await M.AoBScan("BB 0B 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00", true, true, false)).FirstOrDefault().ToString("X");
                }
                catch { }

                if (Address != "0")
                    UpdateUI("Found the FD Viper Address", "Status");
                else
                    UpdateUI("Failed at finding the address", "Status");
            });
            ScanThread.Start();  
        }
        private void List_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/ForzaMods/fh5idlist");
        }

        void UpdateUI(string text, string part)
        {
            if (part != "Status" && !Attached)
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    Scan.IsEnabled = false;
                    Swap.IsEnabled = false;
                    GetIds.IsEnabled = false;
                    ID_Box.IsEnabled = false;
                });

            else if (part != "Status" && Attached)
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    Scan.IsEnabled = true;
                    Swap.IsEnabled = true;
                    GetIds.IsEnabled = true;
                    ID_Box.IsEnabled = true;
                });

            else if (part == "Status")
            {
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    Status.Content = "Status: " + text;
                });
            }
        }

        private void GetIds_Click(object sender, RoutedEventArgs e)
        {
            var getids = new CarIds();

            if (!IsGetIdsOpen)
            {
                getids.Show();
                IsGetIdsOpen = true;
            }
        }

        private void MessageBoxSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if ((bool)MessageboxSwitch.IsChecked)
                ShowMessageBox = true;
            else 
                ShowMessageBox = false;
        }
    }
}
