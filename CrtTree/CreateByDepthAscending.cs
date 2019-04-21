using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrtTree
{
    class CreateByDepthAscending
    {
        public static void Run(string baseDir, IEnumerable<string> dirsToCreate, Native.Win32ApiErrorCallback win32error, Action<string> OnExists, Action OnCreated)
        {
            IEnumerable<string> directoriesByLevelAscending =
                dirsToCreate
                .OrderBy(dirname => Misc.CountChar(dirname, '\\'));

            foreach (string dirname in directoriesByLevelAscending)
            {
                string FullDirname = Path.Combine(baseDir, dirname);
                if (CreateDirectory(FullDirname, OnExists, win32error))
                {
                    //
                    OnCreated?.Invoke();
                }
            }

        }
        static bool CreateDirectory(string Fullname, Action<string> OnExists, Native.Win32ApiErrorCallback OnError)
        {
            bool ok = Native.CreateDirectoryW(Fullname, IntPtr.Zero);
            if (!ok)
            {
                int LastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();

                if (LastError == 183) // "Cannot create a file when that file already exists."
                {
                    OnExists(Fullname);
                }
                else
                {
                    OnError(LastError, "CreateDirectoryW", Fullname);
                }
            }

            return ok;
        }
    }
}
