To try it out , install the nuget package:  

{{ 
Install-Package Join-Calculus 
}}
Create a new console project and replace the content of Program.cs with the code below. This will illustrate how the asynchronous buffer works. The buffer doesn't block when the client puts data, but blocks on the get until some data becomes available. 

{{
using System;
using System.Threading;
using ZsK.Joins;
using ZsK.Joins.Traces;

namespace AsyncBuffer {

    public class AsyncBuffer<T> {
        // Put is asynchronous, a message is queued up 
        // but control is immedialtetly returned to the caller
        public @async Put(T t) { return put.Invoked(t); }

        // Get is synchronous, it will return whenever the message 
        // has been consumed in a match
        public T Get() { return get.Invoked<T>(); }

        // Joiner does all the work, queues up the messages and identifies matches
        private Joiner joiner;

        // these two fields connect the invocation of the instance methods 
        // above to the Jointer.
        // They are also used to define the patterns/chords.
        private SyncMethod get;
        private AsyncMethod put;

        public AsyncBuffer() {
            joiner = new Joiner();

            // initialize and register the Method instances 
            // with the Jointer
            get = joiner.NewSyncMethod<T>(Get);
            put = joiner.NewAsyncMethod<T>(Put);

            // combine the Methods to define patterns/chords the code in 
            // the lambda will be invoked whenever there is a match.
            // In this case there will be a match whenever 
            // both Get and Put have been invoked
            (get & put).Do((T t) => {
                Log.Trace("Match 'get & put': passing " + t + " from Put to Get");
                return t;
            });
        }
    }

    public static class Program {
        public static void Main() {
            if (Console.WindowHeight < 40) Console.WindowHeight = 40;
            if (Console.WindowWidth < 100) Console.WindowWidth = 100;
            Log.ClearLogs();

            var buffer = new AsyncBuffer<int>();
            var th1 = new Thread(() => {
                Log.Trace("thread 1");
                Thread.Sleep(10);
                Log.Trace("Putting 1..., Put queue:[]()");
                buffer.Put(1);
                Log.Trace("...done, Put queue:[1](1)");
                Thread.Sleep(200);
                Log.Trace("Slept for 200 ms");
                Log.Trace("Putting 4..., Put queue:[]()");
                buffer.Put(4);
                Log.Trace("...done, Put queue:[4](4)");
                Log.MergeTrace();
            });
            var th2 = new Thread(() => {
                Log.Trace("thread 2");
                Thread.Sleep(20);
                Log.Trace("Putting 2..., Put queue:[1](1)");
                buffer.Put(2);
                Log.Trace("...done, Put queue:[2,1](2,1)");
                Thread.Sleep(150);
                Log.Trace("Slept for 200 ms");
                Log.Trace("Putting 3..., Put queue:[]()");
                buffer.Put(3);
                Log.Trace("...done, Put queue:[3](3)");
                Log.Trace("Calling Get...blocked...");
                Log.Trace("...Get returned {0}", buffer.Get());
                Log.MergeTrace();
            });
            var th3 = new Thread(() => {
                Log.Trace("thread 3");
                Thread.Sleep(10);
                Thread.Sleep(100);
                Log.Trace("Slept for 200 ms");
                Log.Trace("Calling Get...");
                Log.Trace("...returned {0}", buffer.Get());
                Log.Trace("Calling Get...");
                Log.Trace("...Get returned {0}", buffer.Get());
                Log.Trace("Calling Get...blocked...");
                Log.Trace("...Get returned {0}", buffer.Get());
                Log.MergeTrace();
            });
            th1.Start();th2.Start();th3.Start();
            th1.Join();th2.Join();th3.Join();
            Log.DumpLogToConsole(25);
            Console.ReadKey();
        }
    }
}

}}