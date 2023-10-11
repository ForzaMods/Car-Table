using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
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
    [DllImport("kernel32.dll")]
    public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

    // variables
    private readonly Mem M = new();
    private string? Address;
    private bool Attached;

    public MainWindow()
    {
        InitializeComponent();
        Task.Run(() => ForzaAttach());
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
        if (!Attached)
            return;

        Status.Content = "Starting scan";

        Task.Run(async () =>
        {
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-GB");
                GetSystemInfo(out var si);
                ulong _address = 0xFFFFFFFFFF;
                var Proc = M.MProc.Process;
                VirtualQueryEx(Proc.Handle, (IntPtr)_address, out var m, (uint)si.PageSize);
                _address = (ulong)m.BaseAddress + (ulong)m.RegionSize;
                long ScanStartAddr = (long)m.BaseAddress;
                long ScanEndAddr;
                int count = 0;
                string AOBString = "BB 0B 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00";
                Address = "0";

                while (_address < 0x3FFFFFFFFFF)
                {
                    VirtualQueryEx(Proc.Handle, (IntPtr)_address, out m, (uint)si.PageSize);
                    if (_address == (ulong)m.BaseAddress + (ulong)m.RegionSize)
                        break;
                    if (m.RegionSize > 70000000000000 && count > 0)
                        break;

                    ScanEndAddr = (long)m.BaseAddress + (long)m.RegionSize;

                    if (count % 100 == 0)
                    {
                        if (ScanEndAddr - ScanStartAddr > 500000000)
                        {
                            for (int i = 1; i < 2; i++)
                            {
                                Address = (await M.AoBScan(ScanStartAddr, ScanStartAddr + (((ScanEndAddr - ScanStartAddr) / 2) * i), AOBString, true, true)).FirstOrDefault().ToString("X");
                            }
                        }
                        else
                        {
                            Address = (await M.AoBScan(ScanStartAddr, ScanEndAddr, AOBString, true, true)).FirstOrDefault().ToString("X");
                        }
                        if (Address != "0")
                        {
                            Dispatcher.Invoke(() => Status.Content = $"Finished. Address:\n{Address}");
                            break;
                        }

                        ScanStartAddr = ScanEndAddr;
                    }

                    _address = (ulong)m.BaseAddress + (ulong)m.RegionSize;
                    count++;
                }

                if (Address == "0")
                {
                    Dispatcher.Invoke(() => Status.Content = "Finished. Failed.");
                }
            }
            catch (Exception a)
            {
                Dispatcher.Invoke(() => Status.Content = $"Failed. Exception:\n{a.Message}");
            }
        });
    }
}