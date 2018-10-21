using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CrtTree
{
    public class Native
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public static extern uint GetFullPathName(
            string lpFileName, 
            uint nBufferLength, 
            [Out] StringBuilder lpBuffer, 
            [Out] StringBuilder lpFilePart);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [System.Security.SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CreateDirectoryW(string lpPathName, IntPtr lpSecurityAttributes);
    }
}
