using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CrtTree
{
    class Stats
    {
        public long created;
        public long exists;
        public long error;
    }

    class Program
    {
        static int Main(string[] args)
        {
            var opts = GetOpts.Run(args);
            if (opts == null)
            {
                return 1;
            }

            opts.baseDir = Misc.GetFullPathLong(opts.baseDir);
            opts.baseDir = Misc.GetLongFilenameNotation(opts.baseDir);
            Console.Error.WriteLine($"baseDir: {opts.baseDir}");

            if (!File.Exists(opts.filename))
            {
                Console.Error.WriteLine($"input file does not exists. [{opts.filename}]");
                return 2;
            }

            if (Misc.IsDirectory(opts.baseDir))
            {
                Console.Error.WriteLine($"base directory is not a directory. [{opts.baseDir}]");
                return 4;
            }

            IEnumerable<string> directoriesByLevelAscending = 
                File
                .ReadLines(opts.filename)
                .OrderBy(dirname => Misc.CountChar(dirname, '\\'));

            Stats stats = new Stats();

            Task CreateTree = Task.Run( () =>
            {
                using (var errorWriter = TextWriter.Synchronized(new StreamWriter(@".\crtTreeError.txt", append: false, encoding: Encoding.UTF8)))
                {

                    Win32ErrorCallback OnWin32err =
                        (LastErrorCode, Api, Message) =>
                        {
                            errorWriter.WriteLine($"{LastErrorCode}\t{Api}\t{Message}");
                            Interlocked.Increment(ref stats.error);
                        };

                    Action<string> OnExists = (name) => Interlocked.Increment(ref stats.exists);

                    foreach (string dirname in directoriesByLevelAscending)
                    {
                        string FullDirname = Path.Combine(opts.baseDir, dirname);
                        if ( CreateDirectory(FullDirname, OnExists, OnWin32err) )
                        {
                            Interlocked.Increment(ref stats.created);
                        }
                    }
                }
            });

            new Spi.StatusLineWriter(Console.Error).WriteUntilFinished(
                TaskToWaitFor: CreateTree
                , milliseconds: 1000
                , GenerateText: () => $"created: {stats.created:N0}\texists: {stats.exists:N0}\terror: {stats.error:N0}");

            return stats.error > 0 ? 8 : 0;
        }
        static bool CreateDirectory(string Fullname, Action<string> OnExists, Win32ErrorCallback OnError)
        {
            bool ok = Native.CreateDirectoryW(Fullname, IntPtr.Zero);
            if ( !ok )
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
