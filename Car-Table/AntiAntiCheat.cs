using static Memory.Imps;
using static Car_Table.MainWindow;

namespace Car_Table;

public class AntiAntiCheat
{
    private readonly nuint _rtlUserThreadStart, _ntCreateThreadEx;
    private readonly byte[]? _origRtlBytes, _origCreateThreadBytes;

    public AntiAntiCheat(nint ntDllHandle)
    {
        _rtlUserThreadStart = GetProcAddress(ntDllHandle, "RtlUserThreadStart");
        _ntCreateThreadEx = GetProcAddress(ntDllHandle, "NtCreateThreadEx");
        _origRtlBytes = Window.Mem.ReadArrayMemory<byte>(_rtlUserThreadStart, 7);
        _origCreateThreadBytes = Window.Mem.ReadArrayMemory<byte>(_ntCreateThreadEx, 8);
    }
    
    public void Install()
    {
        Window.Mem.WriteArrayMemory(_rtlUserThreadStart, new byte[] { 0x48, 0x83, 0xEC, 0x78, 0x4C, 0x8B, 0xC2 });
        Window.Mem.WriteArrayMemory(_ntCreateThreadEx, new byte[] { 0x4C, 0x8B, 0xD1, 0xB8, 0xC7, 0x00, 0x00, 0x00 });
    }

    public void UnInstall()
    {
        Window.Mem.WriteArrayMemory(_rtlUserThreadStart, _origRtlBytes);
        Window.Mem.WriteArrayMemory(_ntCreateThreadEx, _origCreateThreadBytes);
    }
}