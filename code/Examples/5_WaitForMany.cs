using System.Threading;

using ZsK.Joins.Traces;

namespace ZsK.Joins.WaitForMany {

    public class WaitForMany {

        public void Wait() { wait.Invoked(); }
        public @async Signal() { return signal.Invoked(); }

        private Joiner joiner = new Joiner();

        private SyncMethod wait;
        private AsyncMethod signal;

        public WaitForMany(int n) {
            wait = joiner.NewSyncMethod(Wait);
            signal = joiner.NewAsyncMethod(Signal);

            (wait & n*signal).Do(() => { });
        }
    }

    public static class Example {

        public static void Demo() {

            var waiter = new WaitForMany(2);

            var th1 = new Thread(() => {
                Thread.Sleep(100);
                Log.Trace("Signal");
                waiter.Signal();
                Log.MergeTrace();
            });

            var th2 = new Thread(() => {
                Thread.Sleep(200);
                Log.Trace("Signal");
                waiter.Signal();

                Log.MergeTrace();
            });

            th1.Start();
            th2.Start();

            Log.Trace("Waiting");
            waiter.Wait();
            Log.Trace("Done");
            Log.MergeTrace();

            th1.Join();
            th2.Join();
        }
    }
}
