using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrtTree
{
    public delegate void Win32ErrorCallback(int LastErrorCode, string Api, string Message);

    class Misc
    {
        public static string GetFullPathLong(string relativeFilename)
        {
            StringBuilder sbFull = null;       // Full resolved path will go here
            StringBuilder sbFile = null;       // Filename will go here

            uint sizeNeededWithoutZero = Native.GetFullPathName(relativeFilename, 0, sbFull, sbFile);

            if (sizeNeededWithoutZero == 0)
            {
                int LastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                Console.Error.WriteLine($"E: GetFullPathName: lastErr = {LastError}");
                throw new System.ComponentModel.Win32Exception();
            }

            if (sizeNeededWithoutZero > 0)
            {
                sbFull = new StringBuilder((int)sizeNeededWithoutZero);
                sbFile = new StringBuilder((int)sizeNeededWithoutZero);

                sizeNeededWithoutZero = Native.GetFullPathName(relativeFilename, sizeNeededWithoutZero, sbFull, sbFile);
            }

            return sbFull.ToString();
        }
        public static int CountChar(string line, char charToCount)
        {
            int count = 0;

            for (int i = 0; i < line.Length; ++i)
            {
                if (line[i] == charToCount)
                {
                    ++count;
                }
            }

            return count;
        }
        public static string GetLongFilenameNotation(string Filename)
        {
            if (Filename.StartsWith(@"\\?\"))
            {
                return Filename;
            }

            if (Filename.Length >= 2 && Filename[1] == ':')
            {
                return @"\\?\" + Filename;
            }
            else if (Filename.StartsWith(@"\\") && !Filename.StartsWith(@"\\?\"))
            {
                return @"\\?\UNC\" + Filename.Remove(0, 2);
            }
            return Filename;
        }
        public static bool IsDirectory(string dir)
        {
            uint rc = Native.GetFileAttributesW(dir);

            if (rc == uint.MaxValue)
            {
                //int LastError = Spi.Win32.GetLastWin32Error();
                return false;   // doesn't exist
            }
            /*
            FILE_ATTRIBUTE_DIRECTORY
            16 (0x10)
            The handle that identifies a directory.
            */
            //return (rc & 0x10) != 0;
            return (rc & (uint)Native.FileAttributes.Directory) != 0;
        }
    }
}
