using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using static System.BitConverter;
using static System.Buffer;
using static Memory.Imps;

namespace Car_Table;

public class Addresses
{
    [DllImport("kernel32", SetLastError = true)]
    private static extern int WaitForSingleObject(nint handle, int milliseconds);

    private readonly MainWindow _mainWindow;
    
    private nuint _ptr = nuint.Zero;
    private const int VirtualFunctionIndex = 9;

    public Addresses(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }
    
    public void Attach()
    {
        var attached = false;

        while (true)
        {
            Task.Delay(500).Wait();
            
            if (_mainWindow.Mem.OpenProcess("ForzaHorizon5"))
            {
                if (attached)
                {
                    continue;
                }

                _mainWindow.Dispatcher.Invoke(() =>
                {
                    _mainWindow.AllCars.IsEnabled = true;
                    _mainWindow.FreeCars.IsEnabled = true;
                    _mainWindow.AddAll.IsEnabled = true;
                    _mainWindow.AddRare.IsEnabled = true;
                    _mainWindow.Visual.IsEnabled = true;
                    _mainWindow.Perf.IsEnabled = true;
                });
                attached = true;
            }
            else
            {
                if (!attached)
                {
                    continue;
                }

                _mainWindow.Dispatcher.Invoke(() =>
                {
                    _mainWindow.AllCars.IsEnabled = false;
                    _mainWindow.FreeCars.IsEnabled = false;
                    _mainWindow.AddAll.IsEnabled = false;
                    _mainWindow.AddRare.IsEnabled = false;
                    _mainWindow.Visual.IsEnabled = false;
                    _mainWindow.Perf.IsEnabled = false;
                });
                attached = false;
            }    
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private bool AobScan()
    {
        if (_ptr != nuint.Zero)
        {
            return true;
        }

        var sigResult = _mainWindow.Mem.ScanForSig("0F 84 ? ? ? ? 48 8B 35 ? ? ? ? 48 85 F6 74").FirstOrDefault();

        if (sigResult == 0)
        {
            return false;
        }
        
        var parmAddress = sigResult + 0x6 + 0x3;
        var parm = _mainWindow.Mem.ReadMemory<int>(parmAddress);
        var pCDataBaseAddress = sigResult + (nuint)parm + 0x6 + 0x7;
        _ptr = _mainWindow.Mem.ReadMemory<nuint>(pCDataBaseAddress);
        return true;
    }
    
    private nuint GetVirtualFunctionPtr(nuint ptr, int index)
    {
        var pVtableBytes = new byte[8];
        var procHandle = _mainWindow.Mem.MProc.Handle;
        ReadProcessMemory(procHandle, ptr, pVtableBytes, (nuint)pVtableBytes.Length, nint.Zero);

        var pVtable = (nuint)ToInt64(pVtableBytes, 0);
        var vTableBytes = new byte[8];
        var lpBaseAddress = pVtable + (nuint)nuint.Size * (nuint)index;
        ReadProcessMemory(procHandle, lpBaseAddress, vTableBytes, (nuint)vTableBytes.Length, nint.Zero);

        return (nuint)ToInt64(vTableBytes, 0);
    }
    
    public void Query(string command)
    {
        if (!AobScan())
        {
            MessageBox.Show("Failed.");
            return;
        }

        _mainWindow.AntiAntiCheat = new AntiAntiCheat(GetModuleHandle("ntdll.dll"));
        _mainWindow.AntiAntiCheat?.UnInstall();
        var procHandle = _mainWindow.Mem.MProc.Handle;
        var allocShellCodeAddress = VirtualAllocEx(procHandle, nuint.Zero, 0x1000, 0x3000, 0x40);

        var rcx = _ptr;
        var rdx = VirtualAllocEx(procHandle, nuint.Zero, 0x1000, 0x3000, 0x40);
        var r8 = VirtualAllocEx(procHandle, nuint.Zero, 0x1000, 0x3000, 0x40);

        byte[] shellCode = {
            0x48,0xBA, 0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,                     
            0x49,0xB8, 0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,                    
            0xFF,0x25, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00 
        };

        BlockCopy(GetBytes(rdx.ToUInt64()), 0, shellCode, 0x02, 8);
        BlockCopy(GetBytes(r8.ToUInt64()), 0, shellCode, 0x0C, 8);

        var callFunction = GetVirtualFunctionPtr(_ptr, VirtualFunctionIndex);

        BlockCopy(GetBytes(callFunction.ToUInt64()), 0, shellCode, shellCode.Length - 8, 8);

        _mainWindow.Mem.WriteStringMemory(r8, command + "\0");

        WriteProcessMemory(procHandle, allocShellCodeAddress, shellCode, (nuint)shellCode.Length, nint.Zero);

        var handle = CreateRemoteThread(procHandle, (nint)null, 0, allocShellCodeAddress, rcx, 0, out _);

#pragma warning disable CA1806
        WaitForSingleObject(handle, int.MaxValue);
#pragma warning restore CA1806
        VirtualFreeEx(procHandle, allocShellCodeAddress, 0x1000, 0x4000);
        VirtualFreeEx(procHandle, r8, 0x1000, 0x4000);

        var resultBytes = new byte[8];
        ReadProcessMemory(procHandle, rdx, resultBytes, (nuint)resultBytes.Length, nint.Zero);
    }
}