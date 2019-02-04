using System.Threading;

using ZsK.Joins.Traces;

namespace ZsK.Joins.SyncBuffer {

    public class SyncBuffer<T>  {

        public void Put(T t) { put.Invoked(t); }
        public T Get() { return get.Invoked<T>(); }

        private Joiner joiner = new Joiner();

        private SyncMethod get;
        private SyncMethod put;

        public SyncBuffer() {
            get = joiner.NewSyncMethod<T>(Get);
            put = joiner.NewSyncMethod<T>(Put);
            
            (get & put).Do((T t) => {
                Log.Trace("Match 'get & put': passing " + t + " from Put to Get");
                return t;
            });
        }
    }

    public static class Example {

        public static void Demo() {

            var buffer = new SyncBuffer<int>();

            var th1 = new Thread(() => {
                Log.Trace("Calling Put...");
                buffer.Put(1);
                Log.Trace("Put 1 done");

                Log.Trace("Calling Put...");
                buffer.Put(2);
                Log.Trace("Put 2 done");
                
                Thread.Sleep(200);

                Log.Trace("Slept for 200 ms");

                Log.Trace("Calling Put...");
                buffer.Put(3);
                Log.Trace("Put 3 done");
                Log.MergeTrace();
            });

            var th2 = new Thread(() => {
                Log.Trace("Calling Put...");
                buffer.Put(11);
                Log.Trace("Put 11 done");

                Log.Trace("Calling Put...");
                buffer.Put(12);
                Log.Trace("Put 12 done");

                Thread.Sleep(200);
                Log.Trace("Slept for 200 ms");

                Log.Trace("Calling Put...");
                buffer.Put(13);
                Log.Trace("Put 13 done");
                Log.MergeTrace();
            });

            var th3 = new Thread(() => {
                Thread.Sleep(100);
                Log.Trace("Slept for 100 ms");
                Log.Trace("Get called..."); Log.Trace("Get returned {0}", buffer.Get());
                Log.Trace("Get called..."); Log.Trace("Get returned {0}", buffer.Get());
                Log.Trace("Get called..."); Log.Trace("Get returned {0}", buffer.Get());
                Log.Trace("Get called..."); Log.Trace("Get returned {0}", buffer.Get());
                Log.Trace("Get called..."); Log.Trace("Get returned {0}", buffer.Get());
                Log.Trace("Get called..."); Log.Trace("Get returned {0}", buffer.Get());
                Log.MergeTrace();
            });
            th1.Start();
            th2.Start();
            th3.Start();

            th1.Join();
            th2.Join();
            th3.Join();
        }
    }
}
