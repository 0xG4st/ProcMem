using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessMemoryDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter process ID: ");
            int processId = Convert.ToInt32(Console.ReadLine());

            Process process = Process.GetProcessById(processId);
            IntPtr processHandle = OpenProcess(PROCESS_VM_READ, false, processId);
            if (processHandle == IntPtr.Zero)
            {
                Console.WriteLine("Could not open process.");
                return;
            }

            Console.WriteLine("Dumping memory offsets and register values of process '" + process.ProcessName + "' (ID: " + processId + ")");
            Console.WriteLine("Base address: " + process.MainModule.BaseAddress.ToString("X"));
            Console.WriteLine("------------------------------------------------------------------");

            int bytesRead = 0;
            byte[] buffer = new byte[8];
            for (long offset = 0; offset < process.MainModule.ModuleMemorySize; offset += 8)
            {
                ReadProcessMemory(processHandle, IntPtr.Add(process.MainModule.BaseAddress, (int)offset), buffer, 8, out bytesRead);
                Console.WriteLine(offset.ToString("X") + ": " + BitConverter.ToInt64(buffer, 0).ToString("X") + " RAX: " + BitConverter.ToInt64(buffer, 0).ToString("X") + " RSI: " + BitConverter.ToInt64(buffer, 0).ToString("X"));
            }

            CloseHandle(processHandle);
            Console.WriteLine("Done.");
        }

        private const int PROCESS_VM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}
