using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memory;
using static Memory.Imps;

namespace ForzaMods_CarTable;

public partial class MainWindow : Window
{
    // variables
    private readonly Mem M = new();
    private string? Address;
    private bool Attached;

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

    // dll imports
    [DllImport("kernel32.dll")]
    private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer,
        uint dwLength);

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
        if (!Attached)
            return;

        Task.Run(async () =>
        {
            try
            {
                var time = new Stopwatch();
                time.Start();
                CultureInfo.CurrentCulture = new CultureInfo("en-GB");

                Dispatcher.Invoke(() =>
                {
                    ScanForIDBtn.IsEnabled = false;
                    Status.Content = "Scanning for addr";
                });
                var count = 0;

                var si = new SYSTEM_INFO();
                GetSystemInfo(out si);

                long ScanStartAddr;
                long ScanEndAddr;

                ulong _address = 0xFFFFFFFFFF;
                var process = Process.GetProcessesByName("ForzaHorizon5")[0];
                var m = new MEMORY_BASIC_INFORMATION();
                VirtualQueryEx(process.Handle, (IntPtr)_address, out m, si.PageSize);
                
                _address = m.BaseAddress + (ulong)m.RegionSize;
                ScanStartAddr = (long)m.BaseAddress;
                
                while (_address < 0x3FFFFFFFFFF)
                {
                    VirtualQueryEx(process.Handle, (IntPtr)_address, out m, si.PageSize);

                    if (_address == m.BaseAddress + (ulong)m.RegionSize)
                        break;

                    if (m.RegionSize > 70000000000000 && count > 0)
                        break;

                    ScanEndAddr = (long)m.BaseAddress + m.RegionSize;

                    if (ScanEndAddr - ScanStartAddr > 500000000)
                        for (var i = 1; i < 2; i++)
                            Address = (await M.AoBScan(ScanStartAddr,
                                    ScanStartAddr + (ScanEndAddr - ScanStartAddr) / 2 * i,
                                    "BB 0B 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00", true)).FirstOrDefault().ToString("X");
                    else
                        Address = (await M.AoBScan(ScanStartAddr, ScanEndAddr,
                                "BB 0B 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00", true)).FirstOrDefault().ToString("X");


                    ScanStartAddr = ScanEndAddr;


                    if (Address != "0")
                    {
                        Dispatcher.Invoke(() =>
                        {
                            time.Stop();
                            ScanForIDBtn.IsEnabled = true;
                            Status.Content = "Scan finished.\nAddr:" + Address + "\nScan took:" +
                                             time.Elapsed.TotalSeconds;
                        });
                        break;
                    }

                    _address = m.BaseAddress + (ulong)m.RegionSize;
                    count++;
                }


                if (Address == "0")
                    Dispatcher.Invoke(() =>
                    {
                        time.Stop();
                        ScanForIDBtn.IsEnabled = true;
                        Status.Content = "Scan finished. Failed.";
                    });
            }
            catch (Exception error)
            {
                Dispatcher.Invoke(() =>
                {
                    ScanForIDBtn.IsEnabled = true;
                    Status.Content = "Scan wasnt finished. Failed.\nError: " + error.Message;
                });
            }
        });
    }
}