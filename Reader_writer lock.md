**Reader/writer lock**

A far more complex example. This implements the read/writer lock. A detailed explanation can be found [here](http://softwaretransactions.com/2011/04/24/join-calculus-examples/).

{{
public class ReaderWriterLock {
    public void GetWriter() { getWriter.Invoked(); }
    public void ReleaseWriter() { releaseWriter.Invoked(); }
    public void GetReader() { getReader.Invoked(); }
    public void ReleaseReader() { releaseReader.Invoked(); }

    private @async Idle() { return idle.Invoked(); }
    private @async Readers(int n) { return readers.Invoked(n); }
    private @async Writer() { return writer.Invoked(); }

    private Joiner joiner;

    private SyncMethod getWriter;
    private SyncMethod releaseWriter;
    private SyncMethod getReader;
    private SyncMethod releaseReader;
        
    private AsyncMethod idle;
    private AsyncMethod readers;
    private AsyncMethod writer;

    public ReaderWriterLock() {
        joiner = new Joiner();

        getWriter = joiner.NewSyncMethod(GetWriter);
        releaseWriter = joiner.NewSyncMethod(ReleaseWriter);
        getReader = joiner.NewSyncMethod(GetReader);
        releaseReader = joiner.NewSyncMethod(ReleaseReader);
        idle = joiner.NewAsyncMethod(Idle);
        readers = joiner.NewAsyncMethod<int>(Readers);
        writer = joiner.NewAsyncMethod(Writer);

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
            Idle();
        });

        // getReader & idle => Readers(1);
        (getReader & idle).Do(() => {
            Readers(1);
        });

        // getReader & readers(n) => Readers(n+1);
        (getReader & readers).Do((int n) => {
            Readers(n + 1);
        });

        // releaseReader & readers(n) => Readers(n-1);
        (releaseReader & readers).Do((int n) => {
            if (n == 1)
                Idle();
            else
                Readers(n - 1);
        });

        //start state
        Idle();
    }
}
}}
