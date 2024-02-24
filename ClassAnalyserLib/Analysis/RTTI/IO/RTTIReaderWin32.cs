using ClassAnalyser.Services;
using ClassAnalyser.Helpers;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace ClassAnalyser.Analysis.RTTI.IO
{
    public class RTTIReaderWin32(int in_processID) : RTTIReaderOS(in_processID)
    {
        public override nuint GetBaseAddress()
        {
            if (!MemoryService.IsProcess64Bit(ProcessID))
                return 0;

            var process = Process.GetProcessById(ProcessID);

            foreach (ProcessModule module in process.Modules)
                return (nuint)module.BaseAddress;

            return 0;
        }

        public override nuint GetPointerSize()
        {
            return (nuint)(MemoryService.IsProcess64Bit(ProcessID) ? 8 : 4);
        }

        public override bool IsMemoryAccessible(nuint in_address)
        {
            return MemoryService.IsAccessible(ProcessID, in_address);
        }

        public override T Read<T>(nuint in_address)
        {
            var size = (uint)Marshal.SizeOf(typeof(T));

            if (typeof(T).Equals(typeof(nuint)))
                size = (uint)GetPointerSize();

            var data = MemoryService.ReadBytes(ProcessID, in_address, size);

            if (data.Length <= 0)
                return default;

            return MemoryHelper.ByteArrayToUnmanagedType<T>(data);
        }

        public override nuint ReadPointer(nuint in_address)
        {
            return Read<nuint>(in_address);
        }

        public override string ReadStringNullTerminated(nuint in_address, Encoding in_encoding = null)
        {
            var data = new List<byte>();
            var encoding = in_encoding ?? Encoding.UTF8;

            var addr = in_address;

            if (encoding == Encoding.Unicode ||
                encoding == Encoding.BigEndianUnicode)
            {
                ushort us;

                while ((us = Read<ushort>(addr)) != 0)
                {
                    data.Add((byte)(us & 0xFF));
                    data.Add((byte)((us >> 8) & 0xFF));
                    addr += 2;
                }
            }
            else
            {
                byte b;

                while ((b = Read<byte>(addr)) != 0)
                {
                    data.Add(b);
                    addr++;
                }
            }

            return encoding.GetString(data.ToArray());
        }
    }
}
