using System.Threading;

using ZsK.Joins.Traces;

namespace ZsK.Joins.AsyncBuffer {

public class AsyncBuffer<T> {

    public @async Put(T t) { return put.Invoked(t); }
    public T Get() { return get.Invoked<T>(); }

    #region privates
    private Joiner joiner;

    private SyncMethod get;
    private AsyncMethod put;
    #endregion

    public AsyncBuffer() {
        #region initialization
        joiner = new Joiner();

        get = joiner.NewSyncMethod<T>(Get);
        put = joiner.NewAsyncMethod<T>(Put);
        #endregion

        (get & put).Do((T t) => {
            Log.Trace("Match 'get & put': passing " + t + " from Put to Get");
            return t;
        });
    }
}


    public static class Example {

        public static void Demo() {

            var buffer = new AsyncBuffer<int>();

            var th1 = new Thread(() => {
                buffer.Put(1);
                Log.Trace("Put 1 done");
                buffer.Put(2);
                Log.Trace("Put 2 done");
                Thread.Sleep(200);
                Log.Trace("Slept for 200 ms");
                buffer.Put(3);
                Log.Trace("Put 3 done");
                Log.MergeTrace();
            });

            var th2 = new Thread(() => {
                buffer.Put(11);
                Log.Trace("Put 11 done");
                buffer.Put(12);
                Log.Trace("Put 12 done");
                Thread.Sleep(200);
                Log.Trace("Slept for 200 ms");
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
