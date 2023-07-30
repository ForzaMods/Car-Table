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

namespace ForzaMods_CarTable
{
    public partial class MainWindow : Window
{
        // variables
        public Mem M = new Mem();
        public static MainWindow mw;
        private bool Attached = false;
        private string BaseAddress = null;
        private string Address = null;
        public bool IsGetIdsOpen = false;
        public int Times_Clicked = 0;

        public MainWindow()
        {
            InitializeComponent();
            mw = this;
            // setting the culture helped some people scan in the older versions, idk why ??
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            Task.Run(ForzaAttach);
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
                    
                    // more variables such as where to start scan, aob string ect
                    Process Process = Process.GetProcessesByName("ForzaHorizon5")[0];
                    long start = (long)Process.MainModule.BaseAddress;
                    long end = (long)(Process.MainModule.BaseAddress + Process.MainModule.ModuleMemorySize);
                    string aob = null;
                    string testaddr = "0";
                    int scancount = 0;
                    int basescancount = 0;
                    nuint add = 0x0;

                    // defines what to add and the aob
                    if (Process.MainModule.FileName.Contains("Microsoft.624F8B84B80"))
                        aob = "30 ED ?? ?? ?? ?? 00 00 80 A0 ?? ?? ?? 7F 00 00 28 A3 ?? ?? ?? 7F 00 00 10";
                    else
                    {
                        aob = "80 00 00 00 00 ?? ?? ?? ?? ?? ?? 00 00 20 EE ?? ?? ?? 7F 00 00 01 00 00 00 00 00 00 00 00";
                        add = 0x5;
                    }
                        
                        
                    // test address is the car id address
                    while (true)
                    {
                        if (testaddr == "0" || testaddr == "2A")
                        {
                            UpdateUI("Waiting for testaddr", true);
                            testaddr = ((await M.AoBScan(start, end, "00 00 50 4C 41 59 45 52 5F 43 41 52 00 00 00 00 00 00 0A", true, true, false)).FirstOrDefault() + 0x2A).ToString("X");
                            Thread.Sleep(50);
                            scancount++;
                        }
                        else if (!M.OpenProcess("ForzaHorizon5") || scancount == 50)
                            break;
                        else
                            break;

                        Thread.Sleep(50);
                    }
                        

                    // checks if player is ingame
                    // used as reading car id and checking if its not 0
                    while (true)
                    {
                        if (M.ReadMemory<int>(testaddr) != 0 || !M.OpenProcess("ForzaHorizon5"))
                            break;
                        else if (scancount == 50)
                            UpdateUI("Testaddres brokey", true);
                        else
                            UpdateUI("Not ingame, cant scan", true);

                        Thread.Sleep(500);
                    }


                    // base address for pointers
                    while ((BaseAddress == "0" || BaseAddress == null || BaseAddress == "2A" || BaseAddress == "1") && 100 > basescancount)
                    {
                        BaseAddress = ((await M.AoBScan(start, end, aob, true, true)).FirstOrDefault() + add).ToString("X");
                        UpdateUI("Waiting for baseaddress", true);
                        basescancount++;
                    }
                       

                    try
                    {
                        if (basescancount != 100)
                        {
                            // pointers
                            if (Process.MainModule.FileName.Contains("Microsoft.624F8B84B80"))
                                Address = (BaseAddress + ",0x20,0x20,0x50,0x420,0x20,0x38,0x88,0x60,0x68,0x58,0x98,0x58,0x20,0x8F0,0x648");
                            else
                                Address = (BaseAddress + ",0x58,0xC8,0x78,0x30,0x0,0x458,0x58,0xB8,0x20,0x8F0,0x648");

                            UpdateUI(attached: true);
                            UpdateUI("Scanned for Viper ID", true);
                        }
                        else { MessageBox.Show("Baseaddress is 0, restart the app until it fixes itself. might aswell wait for an update of this program"); }

                    } catch { MessageBox.Show("failed"); }

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
                Environment.Exit(0);

                if (Attached)
                    M.WriteMemory<int>(Address, 3003);
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
        void UpdateUI(string text = "", bool status = false, bool attached = false)
        {
            if (!status)
                Dispatcher.BeginInvoke((Action)delegate ()
                {
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
            // opens id window
            if (!IsGetIdsOpen)
            {
                var getids = new CarIds();
                getids.Show();
                IsGetIdsOpen = true;
                Times_Clicked = 0;
            }

            // easter egg
            if (Times_Clicked > 5)
                MessageBox.Show("Stop spamming this button retard");

            Times_Clicked++;
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
            else if (BaseAddress == "0" || BaseAddress == null || BaseAddress == "2a" || BaseAddress == "1")
                MessageBox.Show("Base address for the pointer is incorrect. Please restart your game and tool", "Baseaddress error");
            // error if not correct
            else
            {
                MessageBox.Show("Input accepts only numbers");
                M.WriteMemory<int>(Address, 3003);
            }
        }
    }
}
