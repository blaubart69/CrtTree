using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spi;

namespace CrtTree
{
    class Opts
    {
        public string baseDir;
        public string filename;
        public bool createPath = false;
    }
    class GetOpts
    {
        public static Opts Run(string[] args)
        {
            Opts opts = new Opts();

            bool showHelp = false;
            var CommandLineOpts = new BeeOptsBuilder()
                .Add('b', "base", OPTTYPE.VALUE, "basedir",         (v) => opts.baseDir = v)
                .Add('p', "path", OPTTYPE.BOOL,  "create path",     (v) => opts.createPath = true)
                .Add('h', "help", OPTTYPE.BOOL,  "show this help",  (v) => showHelp = true)
                .GetOpts();
            
            IList<string> pargs = BeeOpts.Parse(args, CommandLineOpts, (optname) => Console.Error.WriteLine($"unknow option: [{optname}]"));

            if (showHelp)
            {
                opts = null;
                PrintUsage(CommandLineOpts);
            }
            else if (pargs.Count == 1)
            {
                opts.filename = pargs[0];
            }
            else
            {
                opts = null;
                PrintUsage(CommandLineOpts);
            }

            return opts;
        }
        private static void PrintUsage(IEnumerable<BeeOpts> CommandOpts)
        {
            Console.Error.WriteLine("Usage: CrtTree [OPTIONS] {filename}"
            + "\ncreates directories given in {filename} line by line.\n"
            + "\nDirectories (= lines in the textfile) will be sorted ascending by depth. (count of \"\\\")"
            + "\nThen CreateDirectoryW() is called for each directory."
            );
            Console.Error.WriteLine("\nOptions:");
            BeeOpts.PrintOptions(CommandOpts);
        }
    }
}
