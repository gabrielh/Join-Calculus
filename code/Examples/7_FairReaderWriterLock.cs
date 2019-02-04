using System.Threading;

using ZsK.Joins.Traces;

namespace ZsK.Joins.FairReaderWriterLock {

    public class FairReaderWriterLock {

        public void GetWriter() { getWriter.Invoked(); }
        public void ReleaseWriter() { releaseWriter.Invoked(); }
        public void GetReader() { getReader.Invoked(); }
        public void ReleaseReader() { releaseReader.Invoked(); }

        private @async Idle() { return idle.Invoked(); }
        private @async Readers(int n) { return readers.Invoked(n); }

        private @async FewerReaders(int n) { return fewerReaders.Invoked(n); }
        private void   ZeroReader() { zeroReader.Invoked(); }
        private @async IdleWriter() { return idleWriter.Invoked(); }
        private @async Writer() { return writer.Invoked(); }

        #region privates
        private Joiner joiner;

        private SyncMethod getWriter;
        private SyncMethod releaseWriter;
        private SyncMethod getReader;
        private SyncMethod releaseReader;
        
        private AsyncMethod idle;
        private AsyncMethod readers;

        private AsyncMethod idleWriter;
        private AsyncMethod writer;
        private AsyncMethod fewerReaders;
        private SyncMethod  zeroReader;
        #endregion

        public FairReaderWriterLock() {
            #region initialization
            joiner = new Joiner();

            getWriter = joiner.NewSyncMethod(GetWriter);
            releaseWriter = joiner.NewSyncMethod(ReleaseWriter);
            getReader = joiner.NewSyncMethod(GetReader);
            releaseReader = joiner.NewSyncMethod(ReleaseReader);
            idle = joiner.NewAsyncMethod(Idle);
            readers = joiner.NewAsyncMethod<int>(Readers);

            fewerReaders = joiner.NewAsyncMethod<int>(FewerReaders);
            zeroReader = joiner.NewSyncMethod(ZeroReader);
            idleWriter = joiner.NewAsyncMethod(IdleWriter);
            writer = joiner.NewAsyncMethod(Writer);
            #endregion


            (getWriter & idle).Do(() => {
                Writer();
            });

            (releaseWriter & writer).Do(() => {
                Idle();
            });

            (getReader & idle).Do(() => {
                Readers(1);
            });

            (getReader & readers).Do((int n) => {
                Readers(n + 1);
            });

            (releaseReader & readers).Do((int n) => {
                if (n == 1)
                    Idle();
                else
                    Readers(n - 1);
            });

            (getWriter & readers).Do((int n) => {
                FewerReaders(n);
                ZeroReader();
            });

            (zeroReader & idleWriter).Do(() => {
                Writer();
            });

            (releaseReader & fewerReaders).Do((int n) => {
                if (n == 1)
                    IdleWriter();
                else
                    FewerReaders(n - 1);
            });


            //start state
            Idle();
        }
    }

    public static class Example {

        public static void Demo() {

            var rwLock = new FairReaderWriterLock();

            var th1 = new Thread(() => {
                Thread.Sleep(500);

                Log.Trace("acquiring writer...");
                rwLock.GetWriter();
                Log.Trace("...done");

                Thread.Sleep(100);
                Log.Trace("releasing writer...");
                rwLock.ReleaseWriter();
                Log.Trace("...done");

                Thread.Sleep(1000);

                Log.MergeTrace();
            });

            var th2 = new Thread(() => {
                Log.Trace("acquiring reader...");
                rwLock.GetReader();
                Log.Trace("...done");

                Thread.Sleep(2000);

                Log.Trace("releasing reader...");
                rwLock.ReleaseReader();
                Log.Trace("...done");

                Log.MergeTrace();
            });

            var th3 = new Thread(() => {
                Thread.Sleep(1000);

                Log.Trace("acquiring reader...");
                rwLock.GetReader();
                Log.Trace("...done");

                Thread.Sleep(1000);

                Log.Trace("acquiring reader...");
                rwLock.GetReader();
                Log.Trace("...done");

                Log.Trace("releasing reader...");
                rwLock.ReleaseReader();
                Log.Trace("...done");

                Log.Trace("releasing reader...");
                rwLock.ReleaseReader();
                Log.Trace("...done");

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
