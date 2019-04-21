﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace Spi
{
    public delegate void OnOption(string value);
    public delegate void OnUnknownOption(string value);

    public enum OPTTYPE
    {
        VALUE,
        BOOL
    }

    public class BeeOpts
    {
        public readonly char        opt;
        public readonly string      optLong;
        public readonly OPTTYPE     type;
        public readonly string      desc;

        public readonly OnOption    OnOptionCallback;

        private bool _wasFound;

        public BeeOpts(char opt, string optLong, OPTTYPE type, string desc, OnOption OnOptionCallback)
        {
            this.opt = opt;
            this.optLong = optLong;
            this.desc = desc;
            this.type = type;
            this.OnOptionCallback = OnOptionCallback;

            this._wasFound = false;
        }
        public static void PrintOptions(IEnumerable<BeeOpts> opts)
        {
            /*
            * Options:
             -b, --basedir=VALUE        appends this basedir to all filenames in the file
             -n, --dryrun               show what would be deleted
             -h, --help                 show this message and exit
            */
            foreach ( BeeOpts o in opts )
            {
                string valueOpt = o.type == OPTTYPE.VALUE ? "=VALUE" : String.Empty;
                string left = $"  -{o.opt}, --{o.optLong}{valueOpt}";
                Console.Error.WriteLine($"{left,-30}{o.desc}");
            }
        }
        public static List<string> Parse(string[] args, IList<BeeOpts> opts, OnUnknownOption OnUnknown)
        {
            List<string> parsedArgs = new List<string>();

            int i = 0;
            while (i < args.Length)
            {
                string curr = args[i];

                if ( curr.StartsWith("--") )
                {
                    if (curr.Length > 2)
                    {
                        ParseLong(args, opts, ref i, OnUnknown);
                    }
                    else
                    {
                        parsedArgs.AddRange(args.Skip(i + 1));
                        break;
                    }
                }
                else if ( curr.StartsWith("-"))
                {
                    if (curr.Length > 1)
                    {
                        ParseShort(args, opts, ref i, OnUnknown);
                    }
                    else
                    {
                        throw new Exception($"bad option [{curr}]");
                    }
                }
                else
                {
                    parsedArgs.Add(args[i]);
                }
                ++i;
            }

            return parsedArgs;
        }
        private static void ParseShort(string[] args, IList<BeeOpts> opts, ref int i, OnUnknownOption OnUnknown)
        {
            string currArg = args[i];
            int j = 1;

            while (j < currArg.Length)
            {
                char curr = currArg[j];

                BeeOpts foundOpt = opts.FirstOrDefault(o => o.opt == curr);
                if ( foundOpt == null )
                {
                    OnUnknown(curr.ToString());
                    break;
                }
                else
                {
                    foundOpt._wasFound = true;

                    if ( foundOpt.type == OPTTYPE.BOOL )
                    {
                        foundOpt.OnOptionCallback(null);
                        ++j;
                    }
                    else if (foundOpt.type == OPTTYPE.VALUE)
                    {
                        if (j < currArg.Length - 1)     
                        {
                            foundOpt.OnOptionCallback(currArg.Substring(j + 1));    // rest is the value
                        }
                        else
                        {
                            string value = ReadNextAsArg(args, ref i);
                            foundOpt.OnOptionCallback(value);
                        }
                        break;
                    }
                }
            }
        }
        private static void ParseLong(string[] args, IList<BeeOpts> opts, ref int i, OnUnknownOption OnUnknown)
        {
            string longOpt = args[i].Substring(2);

            string[] optWithValue = longOpt.Split('=');

            string optname;
            if ( optWithValue.Length == 1 || optWithValue.Length == 2)
            {
                optname = optWithValue[0];
            }
            else
            {
                throw new Exception($"bad option [{longOpt}]");
            }

            BeeOpts foundOpt = opts.FirstOrDefault(o => o.optLong.Equals(optname));

            if ( foundOpt == null )
            {
                OnUnknown?.Invoke(optname);
                ++i;
            }
            else
            {
                foundOpt._wasFound = true;

                if (foundOpt.type == OPTTYPE.BOOL)
                {
                    foundOpt.OnOptionCallback(null);
                }
                else if (foundOpt.type == OPTTYPE.VALUE)
                {
                    if (optWithValue.Length == 2)
                    {
                        foundOpt.OnOptionCallback(optWithValue[1]);
                    }
                    else
                    {
                        string value = ReadNextAsArg(args, ref i);
                        foundOpt.OnOptionCallback(value);
                    }
                }
            }
        }
        private static string ReadNextAsArg(string[] args, ref int i)
        {
            string value = (i+1) < args.Length ? args[i+1] : null;

            if (value != null)
            {
                if (value.StartsWith("-") || value.StartsWith("--"))
                {
                    return null;
                }
            }

            if ( value != null )
            {
                ++i;
            }

            return value;
        }
    }
    public class BeeOptsBuilder
    {
        private IList<BeeOpts> _data = new List<BeeOpts>();

        public BeeOptsBuilder Add(char opt, string optLong, OPTTYPE type, string desc, OnOption OptionCallback)
        {
            _data.Add(new BeeOpts(opt, optLong, type, desc, OptionCallback));
            return this;
        }
        public IList<BeeOpts> GetOpts()
        {
            return _data;
        }
    }
}