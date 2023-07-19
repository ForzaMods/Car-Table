using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Memory;

namespace ForzaMods_CarTable
{
    public partial class MainWindow : Window
    {
        Mem M = new Mem();
        private bool   Attached = false;
        private string Address = "";

        public MainWindow()
        {
            InitializeComponent();

            Task.Run(ForzaAttach);
        }

        void ForzaAttach()
        {
            while (true)
            {
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
            if (IsNumericInput(ID_Box.Text))
            {
                M.WriteMemory(Address, "2bytes", ID_Box.Text);
                UpdateUI("Swapped the ID", "Status");
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

            await Task.Run(async () =>
            {
                Address = (await M.AoBScan("BB 0B 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00", true, true, false)).FirstOrDefault().ToString("X");
            });

            UpdateUI("Found the FD Viper Address", "Status");
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
                    ID_Box.IsEnabled = false;
                });

            else if (part != "Status" && Attached)
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    Scan.IsEnabled = true;
                    Swap.IsEnabled = true;
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
    }
}
