using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memory;

namespace ForzaMods_CarTable;

public partial class MainWindow : Window
{
    private string Address;
    private bool Attached;

    // variables
    private readonly Mem M = new();

    public MainWindow()
    {
        InitializeComponent();
        Task.Run(() => ForzaAttach());
        MessageBox.Show("This is opensource and free." +
                        "The only official download is the ForzaMods Github or UC. " +
                        "If you downloaded this off anywhere else than these sources you got scammed. " +
                        "Please follow the only proper tutorial thats the button in the app or in the UC post." +
                        "You should never listen to other tutorials that you see.");
    }

    // main attach thread
    private void ForzaAttach()
    {
        while (true)
        {
            Thread.Sleep(1000);

            if (M.OpenProcess("ForzaHorizon5"))
            {
                if (Attached)
                    continue;

                Attached = true;
                Dispatcher.BeginInvoke(() => { Status.Content = "Opened Forza Process"; });
            }
            else
            {
                if (!Attached)
                    continue;

                Attached = false;
                Dispatcher.BeginInvoke(() => { Status.Content = "Doing Nothing"; });
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
        if (e.ChangedButton != MouseButton.Left)
            return;

        try
        {
            if (Attached && M.ReadMemory<int>(Address) != 0)
                M.WriteMemory(Address, 3003);
        }
        catch
        {
        }

        Environment.Exit(0);
    }

    // open car id list
    private void List_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            Process.Start("explorer.exe", "https://github.com/ForzaMods/fh5idlist");
    }

    // mem writing
    private void ID_Box_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!Attached)
            return;

        // checking if correct and if so then write to the address
        if (Regex.IsMatch(ID_Box.Text, @"^[0-9]+$"))
        {
            if (int.TryParse(ID_Box.Text, out var writeValue))
                M.WriteMemory(Address, writeValue);

            Status.Content = "Swapped the id";
            return;
        }

        // error if not correct
        MessageBox.Show("Input accepts only numbers");
        M.WriteMemory(Address, 3003);
    }

    private void Youtube_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            Process.Start("explorer.exe", "https://youtu.be/ApcQH8OhuPQ");
    }

    // find addr func
    private void ScanForID(object sender, RoutedEventArgs e)
    {
        Task.Run(async () =>
        {
            Address = (await M.AoBScan(0x10000000000, 0x30000000000,
                    "BB 0B 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00", true)).FirstOrDefault()
                .ToString("X");
            Dispatcher.Invoke(() => { Status.Content = "Scanned for ID,\nAddr: " + Address; });
        });
    }
}