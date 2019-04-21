using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrtTree
{
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
        public static bool CreatePath(string PathToCreate, Native.Win32ApiErrorCallback OnWin32Error, Action OnCreateDirectory, Action<string> OnExists)
        {
            if (IsDirectory(PathToCreate))
            {
                OnExists(PathToCreate);
                return true;
            }
            if (Native.CreateDirectoryW(PathToCreate, IntPtr.Zero))
            {
                OnCreateDirectory?.Invoke();
                return true;
            }

            bool ok = false;
            if (Marshal.GetLastWin32Error() == (int)Native.Win32Error.ERROR_PATH_NOT_FOUND)
            {
                // not found. try to create the parent dir.
                int LastPos = PathToCreate.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                if (LastPos != -1)
                {
                    string parentDir = PathToCreate.Substring(0, LastPos);
                    if (ok = CreatePath(parentDir, OnWin32Error, OnCreateDirectory, OnExists))
                    {
                        // parent dir exist/was created
                        ok = Native.CreateDirectoryW(PathToCreate, IntPtr.Zero);
                        
                        if (ok)
                        {
                            OnCreateDirectory?.Invoke();
                        }
                        else
                        {
                            if (Marshal.GetLastWin32Error() == 183) // ERROR_ALREADY_EXISTS
                            {
                                OnExists?.Invoke(parentDir);
                                ok = true;
                            }
                            else
                            {
                                OnWin32Error?.Invoke(Marshal.GetLastWin32Error(), "CreateDirectoryW", PathToCreate);
                            }
                        }
                    }
                }
            }
            else if (Marshal.GetLastWin32Error() == 183) // ERROR_ALREADY_EXISTS
            {
                OnExists?.Invoke(PathToCreate);
                ok = true;
            }
            else
            {
                OnWin32Error?.Invoke(Marshal.GetLastWin32Error(), "CreateDirectoryW", PathToCreate);
            }
            return ok;
        }
    }
}
