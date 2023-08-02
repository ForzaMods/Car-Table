using ForzaMods_CarTable.Resources;
using Memory;
using System;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Forms = System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Media;

namespace ForzaMods_CarTable
{
    public partial class MainWindow : Window
    {
        // variables
        public Mem M = new Mem();
        public static MainWindow mw;
        private bool Attached = false;
        private string Address = null;
        private string BaseAddress = null;
        private DispatcherTimer rainbowTimer = new DispatcherTimer();
        private Brush[] rainbowBrushes = { Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.Green, Brushes.Blue, Brushes.Indigo, Brushes.Violet };
        private int currentBrushIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
            mw = this;
            // setting the culture helped some people scan in the older versions, idk why ??
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            Task.Run(() => ForzaAttach());
            MessageBox.Show("This is opensource and free." +
                            "The only official download is the ForzaMods Github or UC. " +
                            "If you downloaded this off anywhere else than these sources you got scammed. " +
                            "Please follow the only proper tutorial thats the button in the app or in the UC post." +
                            "You should never listen to other tutorials that you see.");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rainbowTimer.Interval = TimeSpan.FromMilliseconds(500);
            rainbowTimer.Tick += RainbowTimer_Tick;
            rainbowTimer.Start();
        }

        private void RainbowTimer_Tick(object sender, EventArgs e)
        {
            mw.Youtube.Fill = rainbowBrushes[currentBrushIndex];
            mw.Video.Foreground = rainbowBrushes[currentBrushIndex];
            currentBrushIndex = (currentBrushIndex + 1) % rainbowBrushes.Length;
        }

        // main attach thread
        async void ForzaAttach()
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

                    Process Target = Process.GetProcessesByName("ForzaHorizon5")[0];
                    int scancount = 0;
                    int basescancount = 0;


                    // checks if player is ingame
                    // used as reading car id and checking if its not 0
                    while (true)
                    {
                        string testaddr = "0";

                        while (true)
                        {
                            if (testaddr == "0" || testaddr == "2A")
                            {
                                UpdateUI("Waiting for testaddr", true);
                                testaddr = ((await M.AoBScan((long)Target.MainModule.BaseAddress, (long)(Target.MainModule.BaseAddress + Target.MainModule.ModuleMemorySize), "00 00 50 4C 41 59 45 52 5F 43 41 52 00 00 00 00 00 00 0A", true, true, false)).FirstOrDefault() + 0x2A).ToString("X");
                                Thread.Sleep(50);
                                scancount++;
                            }
                            else if (!M.OpenProcess("ForzaHorizon5") || scancount == 50 || (testaddr != "0" || testaddr != "2A")) break;
                        }

                        if (M.ReadMemory<int>(testaddr) != 0 || !M.OpenProcess("ForzaHorizon5")) break;
                        else if (scancount == 50) UpdateUI("Testaddres brokey", true);
                        else UpdateUI("Not ingame, cant scan", true);

                        Thread.Sleep(500);
                    }


                    // base address for pointers
                    while ((BaseAddress == "0" || BaseAddress == null || BaseAddress == "2A" || BaseAddress == "5") && 100 > basescancount)
                    {
                        if (Target.MainModule.FileName.Contains("Microsoft.624F8B84B80"))
                            BaseAddress = (await M.AoBScan((long)Target.MainModule.BaseAddress, (long)(Target.MainModule.BaseAddress + Target.MainModule.ModuleMemorySize), "?0 ED ?? ?? ?? ?? 00 00 80 A0 ?? ?? ?? 7F 00 00 28 A3 ?? ?? ?? 7F 00 00 ?0", true, true, false)).FirstOrDefault().ToString("X");
                        else
                            BaseAddress = ((await M.AoBScan((long)Target.MainModule.BaseAddress, (long)(Target.MainModule.BaseAddress + Target.MainModule.ModuleMemorySize), "80 00 00 00 00 ?? ?? ?? ?? ?? ?? 00 00 20 EE ?? ?? ?? 7F 00 00 01 00 00 00 00 00 00 00 00", true, true)).FirstOrDefault() + 0x5).ToString("X");

                        UpdateUI("Waiting for baseaddress", true);
                        Thread.Sleep(25);
                        basescancount++;
                    }


                    try
                    {
                        if (basescancount != 100)
                        {
                            // pointers
                            if (Target.MainModule.FileName.Contains("Microsoft.624F8B84B80"))
                                Address = (BaseAddress + ",0x20,0x20,0x50,0x420,0x20,0x38,0x88,0x60,0x68,0x58,0x98,0x58,0x20,0x8F0,0x648");
                            else
                                Address = (BaseAddress + ",0x58,0xC8,0x78,0x30,0x0,0x458,0x58,0xB8,0x20,0x8F0,0x648");

                            UpdateUI("Scanned for Viper ID", true, true);
                        }
                        else { MessageBox.Show("Baseaddress is 0, restart the app until it fixes itself. might aswell wait for an update of this program"); }

                    } catch { MessageBox.Show("failed"); continue; }

                }
                else
                {
                    if (!Attached)
                        continue;

                    Attached = false;
                    UpdateUI("Doing Nothing", true, true);
                }
            }
        }

        // move the window
        private void Topbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }


        // closing
        private void Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (Attached)
                    M.WriteMemory<int>(Address, 3003);

                Environment.Exit(0);
            }
        }

        // open car id list
        private void List_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                Process.Start("explorer.exe", "https://github.com/ForzaMods/fh5idlist");
        }

        // updating ui, status and shit
        // this is like ultra shit code and I hate what Ive done but it works so I dont really care
        void UpdateUI(string text = "", bool status = false, bool updatebuttons = false)
        {
            if (updatebuttons)
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    GetIds.IsEnabled = Attached;
                    ID_Box.IsEnabled = Attached;
                });

            if (status)
                Dispatcher.BeginInvoke((Action)delegate () { Status.Content = "Status: " + text; });
        }

        private void GetIds_Click(object sender, RoutedEventArgs e)
        {
            var c = new CarIds();

            // opens id window
            if (!c.IsGetIdsOpen)
            {
                c.Show();
                c.IsGetIdsOpen = true;
                c.Times_Clicked = 0;
            }

            // easter egg
            if (c.Times_Clicked > 5)
                MessageBox.Show("Stop spamming this button retard");

            c.Times_Clicked++;
        }

        // mem writing
        private void ID_Box_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // checking if correct and if so then write to the address
            if (Regex.IsMatch(ID_Box.Text, @"^[0-9]+$") && (BaseAddress != "0" || BaseAddress != null || BaseAddress != "2a" || BaseAddress != "1"))
            {
                if (int.TryParse(ID_Box.Text, out int writeValue))
                    M.WriteMemory<int>(Address, writeValue);

                UpdateUI("Swapped the ID", true);
            }
            // checking for base address
            else if (BaseAddress == "0" || BaseAddress == null || BaseAddress == "2a" || BaseAddress == "5")
                MessageBox.Show("Base address for the pointer is incorrect. Please restart your game and tool", "Baseaddress error");
            // error if not correct
            else
            {
                MessageBox.Show("Input accepts only numbers");
                M.WriteMemory<int>(Address, 3003);
            }
        }

        private async void Youtube_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
               Process.Start("explorer.exe", "https://youtu.be/ApcQH8OhuPQ");
                rainbowTimer.Stop();
                mw.Youtube.Fill = Brushes.White;
                mw.Video.Foreground = Brushes.White;
            }
        }
    }
}
