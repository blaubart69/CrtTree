﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CrtTree
{

    public class Native
    {
        public delegate void Win32ApiErrorCallback(int LastError, string Apiname, string Text);

        [Flags]
        public enum FileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        public enum Win32Error : int
        {
            ERROR_PATH_NOT_FOUND = 3,
            ERROR_ACCESS_DENIED = 5
        }

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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetFileAttributesW(string lpFileName);
    }
}
