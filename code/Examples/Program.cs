using System;
using System.Collections.Generic;

using ZsK.Joins.Traces;

namespace ZsK.Joins.Examples  {

    class Program {

        static void Main(string[] args) {
            if (Console.WindowHeight < 40) Console.WindowHeight = 40;
            if (Console.WindowWidth < 100) Console.WindowWidth = 100;

            ZsK.Joins.AsyncBufferDetailed.Example.Demo();

            var demos = new List<Action>() {
                AsyncBuffer.Example.Demo, 
                SyncBuffer.Example.Demo,
                BoundedSyncBuffer.Example.Demo,
                JoinMany.Example.Demo,
                WaitForMany.Example.Demo,
                ReaderWriterLock.Example.Demo,
                ReaderWriterLock.Example.DemoError,
                FairReaderWriterLock.Example.Demo,
            };
            
            foreach (var demo in demos) {
                Console.WriteLine(" ");
                Console.WriteLine(" ");
                var title = demo.Method.DeclaringType.Namespace.Split('.')[2];
                Console.WriteLine("".PadLeft(title.Length + 2, '='));
                Console.WriteLine(" " + title);
                Console.WriteLine("".PadLeft(title.Length + 2, '='));

                Log.ClearLogs();

                demo();

                Log.DumpLogToConsole(15);

                Console.WriteLine(" ");
                Console.WriteLine("press any key to continue...");
                Console.ReadKey();
            }
            Console.WriteLine(" ");
            Console.WriteLine("All done, press any key to terminate...");
            Console.ReadKey();
        }
    }
}
