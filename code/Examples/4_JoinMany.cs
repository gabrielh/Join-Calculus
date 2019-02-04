using System.Threading;

using ZsK.Joins.Traces;

namespace ZsK.Joins.JoinMany {

    public class JoinMany {

        public void Join() { join.Invoked(); }

        private Joiner joiner= new Joiner();

        private SyncMethod join;

        public JoinMany(int n) {
            join = joiner.NewSyncMethod(Join);

            (n*join).Do(() => { });
        }
    }


    public static class Example {

        public static void Demo() {

            var buffer = new JoinMany(3);

            var th1 = new Thread(() => {
                Thread.Sleep(400);
                Log.Trace("Joiner...");
                buffer.Join();
                Log.Trace("...joined");
                Log.MergeTrace();
            });

            var th2 = new Thread(() => {
                Thread.Sleep(200);
                Log.Trace("Joiner...");
                buffer.Join();
                Log.Trace("...joined");
                Log.MergeTrace();
            });

            var th3 = new Thread(() => {
                Thread.Sleep(700);
                Log.Trace("Joiner...");
                buffer.Join();
                Log.Trace("...joined");
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
