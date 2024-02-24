using System.Runtime.InteropServices;

namespace ClassAnalyser.Services
{
    public partial class MemoryService
    {
        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        [LibraryImport("kernel32.dll")]
        private static partial nuint OpenProcess(int in_dwDesiredACcess, [MarshalAs(UnmanagedType.Bool)] bool in_bInheritHandle, int in_dwProcessId);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ReadProcessMemory(nuint in_hProcess, nuint in_lpBaseAddress, byte[] in_lpBuffer, nuint in_dwSize, out nuint out_lpNumberOfBytesRead);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CloseHandle(nuint in_hObject);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool IsWow64Process(nuint in_processHandle, [MarshalAs(UnmanagedType.Bool)] out bool out_isWow64Process);

        public static bool IsProcess64Bit(int in_processID)
        {
            var processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, in_processID);

            if (processHandle == 0)
                return false;

            if (Environment.Is64BitOperatingSystem &&
                IsWow64Process(processHandle, out bool out_isWow64Process))
            {
                return !out_isWow64Process;
            }

            return true;
        }

        public static bool IsAccessible(int in_processID, nuint in_address)
        {
            var processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, in_processID);

            if (processHandle == 0)
                return false;

            var buffer = new byte[1];
            var result = ReadProcessMemory(processHandle, in_address, buffer, 1, out _);

            CloseHandle(processHandle);

            return result;
        }

        public static byte[] ReadBytes(int in_processID, nuint in_address, uint in_length)
        {
            var result = new byte[in_length];

            if (in_address == 0)
                return [];

            var processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, in_processID);

            if (processHandle == 0)
                return [];

            if (!ReadProcessMemory(processHandle, in_address, result, in_length, out _))
                throw new Exception($"Failed to read process memory ({Marshal.GetLastPInvokeError()}).");

            CloseHandle(processHandle);

            return result;
        }

        public static byte[] ReadBytes(int in_processID, ulong in_address, uint in_length)
        {
            return ReadBytes(in_processID, (nuint)in_address, in_length);
        }
    }
}
