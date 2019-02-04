using System.Threading;

using ZsK.Joins.Traces;
using ZsK.Joins.Utilities;

namespace ZsK.Joins.BoundedSyncBuffer {

    public class BoundedSyncBuffer<T> {

        public void Put(T t) { put.Invoked<T>(t); }
        public T Get() { return get.Invoked<T>(); }

        private @async EmptySlot() { return emptySlot.Invoked(); }
        private @async FullSlot(T t) { return fullSlot.Invoked(t); }

        #region privates
        private Joiner joiner;

        private SyncMethod get;
        private SyncMethod put;
        private AsyncMethod emptySlot;
        private AsyncMethod fullSlot;
        #endregion

        public BoundedSyncBuffer(int size) {
            #region initialization
            joiner = new Joiner();

            put = joiner.NewSyncMethod<T>(Put);
            get = joiner.NewSyncMethod<T>(Get);
            emptySlot = joiner.NewAsyncMethod(EmptySlot);
            fullSlot = joiner.NewAsyncMethod<T>(FullSlot);
            #endregion

            (put & emptySlot).Do((T t) => {
                Log.Trace("Match 'put & emptySlot': passing " + t + " from Put to EmptySlot");
                FullSlot(t);
            });
            (get & fullSlot).Do((T t) => {
                Log.Trace("Match 'get & fullSlot': passing " + t + " from FullSlot to Get");
                EmptySlot();
                return t;
            });

            size.Times(() => EmptySlot());
        }
    }

    public static class Example {

        public static void Demo() {

            var buffer = new BoundedSyncBuffer<int>(2);

            var th1 = new Thread(() => {
                Log.Trace("start");
                buffer.Put(1);
                Log.Trace("Put 1 done");
                buffer.Put(2);
                Log.Trace("Put 2 done");


                buffer.Put(3);
                Log.Trace("Put 3 done");
                buffer.Put(4);
                Log.Trace("Put 4 done");

                buffer.Put(5);
                Log.Trace("Put 5 done");

                Log.MergeTrace();
            });

            var th2 = new Thread(() => {
                Thread.Sleep(100);
                Log.Trace("Slept for 100 ms");
                Log.Trace("Get returned {0}", buffer.Get());
                Log.Trace("Get returned {0}", buffer.Get());

                Thread.Sleep(100);
                Log.Trace("Slept for 100 ms");

                Log.Trace("Get returned {0}", buffer.Get());
                Log.Trace("Get returned {0}", buffer.Get());
                Log.Trace("Get returned {0}", buffer.Get());

                Log.MergeTrace();
            });

            th1.Start();
            th2.Start();

            th1.Join();
            th2.Join();
        }
    }
}
