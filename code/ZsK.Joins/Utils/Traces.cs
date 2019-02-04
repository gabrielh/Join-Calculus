/*
Copyright (C) by Gabriel Zs. K. Horvath

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZsK.Joins.Traces {

    /// <summary>
    /// Abstract base of the class whose instances will be linked to actual methods and used to 
    /// specify chords/patterns.
    /// </summary>
    public static class Log {

        public class Entry {
            public long Time;
            public string String;
            public string ThreadName;
            public int ThreadId = 0;
            public int ThreadHashCode;
            public object[] Arguments;
            public override string ToString() {
                string s = "@@@";
                try {
                    s = string.Format(String, Arguments);
                } catch {}
                string TimeS = DeltaToString(start, Time);
                string res = string.Format("{0} {1} {2}", TimeS, ThreadName, s);
                return res;
            }
        }

        [DllImport("kernel32.dll")]
        extern static short QueryPerformanceCounter(ref long x);

        [DllImport("kernel32.dll")]
        extern static short QueryPerformanceFrequency(ref long x);

        static private long frequency = 1;
        static private long start = 0;


        static Log() {
            //e.g 3579545 Hz -> 279.36 ns -> e.g. 500 clock cycles
            QueryPerformanceFrequency(ref frequency);
            QueryPerformanceCounter(ref start);
        }

        public static long Now() {
            long result = 0;
            QueryPerformanceCounter(ref result);
            return result;

        }
        public static string NowS() {
            long now = 0;
            QueryPerformanceCounter(ref now);
            return DeltaToString(start, now);
        }



        private static string FormatSeconds(double ticks) {
            ticks = Math.Abs(ticks);
            if (ticks == 0)
                return "  0.000 s ";
            long factor = 1;
            string units = "????";
            //if (ticks > 1) {
            //    factor = 1;
            //    units = " s ";
            //} else if (ticks > 0.001) {
            //    factor = 1000;
            //    units = " ms";
            //} else if (ticks > 0.000001) {
            //    factor = 1000000;
            //    units = " us";
            //} else if (ticks > 0.000000001) {
            //    factor = 1000000000;
            //    units = " ns";
            //}
            //override 
            factor = 1;
            units = " s ";
            
            return Math.Round(factor * ticks, 3+3).ToString(".0000").PadLeft(6) + units;
        }

        public static string DeltaToString(long start, long now) {
            return FormatSeconds((double)(now - start) / frequency);
        }

        
        internal static void Add(string line) {
            //this is obviously a bit slow... we should just save the needed values in a struct and ToString them 
            var entry = new Entry();
            QueryPerformanceCounter(ref entry.Time);
            entry.ThreadName = Thread.CurrentThread.Name;
            entry.String = line;
            FullLog.Add(entry);
            //mData.Add(DeltaToString(start, now) + ":" + Thread.CurrentThread.Name + ":" + line);
            //System.Threading.Thread.MemoryBarrier();
        }

        public static void Dump() {
            while (true) {
                Entry entry = FullLog.Last();
                FullLog.Remove(FullLog.Last());
                if (entry == null) {
                    break;
                }
                System.Diagnostics.Trace.WriteLine(entry.ToString());
            };
        }


        private static ConsoleColor[] goodColours = { ConsoleColor.Gray, ConsoleColor.DarkCyan, ConsoleColor.DarkYellow, ConsoleColor.DarkMagenta, ConsoleColor.DarkBlue, ConsoleColor.DarkRed, ConsoleColor.DarkGray, ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.White, ConsoleColor.Green};

        public static void DumpLogToConsole(int offset) {
            var startingColour = Console.ForegroundColor;
            int colourIndex = 0;
            Dictionary<int, int> threadToColourIndex = new Dictionary<int, int>();
            foreach (Entry entry in Log.FullLog.OrderBy(e => e.Time)) {
                if (!threadToColourIndex.ContainsKey(entry.ThreadId)) {
                    threadToColourIndex[entry.ThreadId] = colourIndex++;
                }

                Console.ForegroundColor = goodColours[threadToColourIndex[entry.ThreadId]];
                Console.WriteLine("".PadLeft(threadToColourIndex[entry.ThreadId] * offset, ' ') + entry.ToString());
            }
            Console.ForegroundColor = startingColour;
        }
        
        public static void DumpLast(int max) {
            List<Entry> entries = new List<Entry>();
            while (true) {
                Entry entry = FullLog.Last();
                FullLog.Remove(FullLog.Last());
                if (entry == null) {
                    break;
                }
                entries.Add(entry);
            };
            int count = entries.Count;
            foreach (Entry entry in entries) {
                count--;
                if (count < max) {
                    System.Diagnostics.Trace.WriteLine(entry.ToString());
                }
            }
        }

        [ThreadStatic]
        public static List<Entry> ThreadLog = new List<Entry>();

        public static void Trace(string s, params object[] arguments) {
            if (ThreadLog == null) {
                ThreadLog = new List<Entry>();
            }
            Entry entry = new Entry() { 
                Arguments = arguments, 
                ThreadName = Thread.CurrentThread.Name, 
                ThreadHashCode = Thread.CurrentThread.GetHashCode(), 
                ThreadId = Thread.CurrentThread.ManagedThreadId, 
                String = s, 
                Time = Now() 
            };
            ThreadLog.Add(entry);
            //System.Diagnostics.Debug.WriteLine(entry.ToString());
        }

        public static List<Entry> FullLog = new List<Entry>();


        private static object mergeLock = new object();

        public static void MergeTrace() {
            lock (mergeLock) {
                if (ThreadLog != null) {
                    FullLog.AddRange(ThreadLog);
                }
            }
        }

        public static void ClearLogs() {
            FullLog.Clear();
            ThreadLog = null;
            QueryPerformanceCounter(ref start);
        }
    }
}
