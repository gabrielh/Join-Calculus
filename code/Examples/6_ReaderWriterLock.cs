using System;
using System.Threading;

using ZsK.Joins.Traces;

namespace ZsK.Joins.ReaderWriterLock {

public class ReaderWriterLock {

    public void GetWriter() { getWriter.Invoked(); }
    public void ReleaseWriter() { releaseWriter.Invoked(); }
    public void GetReader() { getReader.Invoked(); }
    public void ReleaseReader() { releaseReader.Invoked(); }

    private @async Idle() { return idle.Invoked(); }
    private @async Readers(int n) { return readers.Invoked(n); }
    private @async Writer() { return writer.Invoked(); }

    #region privates
    private Joiner joiner;

    private SyncMethod getWriter;
    private SyncMethod releaseWriter;
    private SyncMethod getReader;
    private SyncMethod releaseReader;
        
    private AsyncMethod idle;
    private AsyncMethod readers;
    private AsyncMethod writer;
    #endregion

    public ReaderWriterLock() {
        #region initialization
        joiner = new Joiner();

        getWriter = joiner.NewSyncMethod(GetWriter);
        releaseWriter = joiner.NewSyncMethod(ReleaseWriter);
        getReader = joiner.NewSyncMethod(GetReader);
        releaseReader = joiner.NewSyncMethod(ReleaseReader);
        idle = joiner.NewAsyncMethod(Idle);
        readers = joiner.NewAsyncMethod<int>(Readers);
        writer = joiner.NewAsyncMethod(Writer);
        #endregion


        (getWriter & idle).Do(() => {
            Writer();
        });

        (releaseWriter & readers).Do((int n) => {
            Readers(n);
            throw new Exception("Cannot release writer lock if not taken");
        });

        // getReader & idle => Readers(1);
        (releaseReader & idle).Do(() => {
            Idle();
            throw new Exception("Cannot release reader lock if not taken");
        });

        //need a async Writer state, otherwise a call to releaseWriter will generate an idle...

        // releaseWriter & writer => Idle()
        (releaseWriter & writer).Do(() => {
            Log.Trace("Match 'releaseWriter & writer': idle");
            Idle();
        });

        // getReader & idle => Readers(1);
        (getReader & idle).Do(() => {
            Log.Trace("Match 'getReader & idle': " + 1 + " readers");
            Readers(1);
        });

        // getReader & readers(n) => Readers(n+1);
        (getReader & readers).Do((int n) => {
            Log.Trace("Match 'getReader & readers': " + (n+1) + " readers");
            Readers(n + 1);
        });

        // releaseReader & readers(n) => Readers(n-1);
        (releaseReader & readers).Do((int n) => {
            if (n == 1){
                Log.Trace("Match 'releaseReader & readers': " + 0 + " readers");
                Idle();
            } else{
                Log.Trace("Match 'releaseReader & readers': " + (n-1) + " readers");
                Readers(n - 1);
            }
        });

        //start state
        Idle();
    }
}

    public static class Example {

        public static void Demo() {

            var rwLock = new ReaderWriterLock();

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

                Thread.Sleep(100);

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


        public static void DemoError() {

            var rwLock = new ReaderWriterLock();

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

                Log.Trace("RELEASING WRITER!!!");
                //this should throw an exception, can't release the writer if not taken
                try {
                    rwLock.ReleaseWriter();
                } catch { }

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

                Thread.Sleep(100);

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
