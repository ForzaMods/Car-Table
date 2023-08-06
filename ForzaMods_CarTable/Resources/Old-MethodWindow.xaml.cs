using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using F = System.Windows.Forms;

namespace ForzaMods_CarTable.Resources
{
    /// <summary>
    /// Interaction logic for Old_MethodWindow.xaml
    /// </summary>
    public partial class Old_MethodWindow : Window
    {
        public static Old_MethodWindow OLD;
        public IEnumerable<nuint> addreses; 

        public Old_MethodWindow()
        {
            InitializeComponent();
            OLD = this;
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
                F.DialogResult dialog = F.MessageBox.Show("Do you want to go back into the main window?", "", F.MessageBoxButtons.OKCancel);   
                if (dialog == F.DialogResult.OK)
                {
                    this.Hide();
                    var mw = new MainWindow();
                    mw.isloaded = true;
                    mw.Show();
                }
                else
                {
                    if (MainWindow.mw.Attached)
                        MainWindow.mw.M.WriteMemory<int>(MainWindow.mw.Address, 3003);

                    Environment.Exit(0);
                }
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
            // useless in this case and I cba to remove it

            if (updatebuttons)
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    ID_Box.IsEnabled = MainWindow.mw.Attached;
                });

            if (status)
                Dispatcher.BeginInvoke((Action)delegate () { Status.Content = "Status: " + text; });
        }

        private async void Scan_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI("Scanning for FD Viper Address", true);

            await Task.Run(async () =>
            {
                addreses = (await MainWindow.mw.M.AoBScan("BB 0B 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00", true, true, false));
            });

            UpdateUI("Found the FD Viper Address", true);
        }

        private void ID_Box_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // checking if correct and if so then write to the address
            if (Regex.IsMatch(ID_Box.Text, @"^[0-9]+$") && (MainWindow.mw.BaseAddress != "0" || MainWindow.mw.BaseAddress != null || MainWindow.mw.BaseAddress != "2a" || MainWindow.mw.BaseAddress != "1"))
            {
                if (int.TryParse(ID_Box.Text, out int writeValue))
                    foreach (nuint addr in addreses)
                        MainWindow.mw.M.WriteMemory<int>(addr.ToString("X"), writeValue);

                UpdateUI("Swapped the ID", true);
            }
            // checking for base address
            //else if (MainWindow.mw.BaseAddress == "0" || MainWindow.mw.BaseAddress == null || MainWindow.mw.BaseAddress == "2a" || MainWindow.mw.BaseAddress == "5")
            //    MessageBox.Show("Base address for the pointer is incorrect. Please restart your game and tool", "Baseaddress error");
            // error if not correct
            else
            {
                MessageBox.Show("Input accepts only numbers");
                MainWindow.mw.M.WriteMemory<int>(MainWindow.mw.Address, 3003);
            }
        }
    }
}
