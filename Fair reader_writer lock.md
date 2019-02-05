**Fair reader/writer lock**

A far more complex example. This implements the fair read/writer lock, which will block new reader from obtaining a lock when a writer is waiting for the lock. A detailed explanation can be found [here](http://softwaretransactions.com/2011/04/24/join-calculus-examples/).

{{
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

    public FairReaderWriterLock() {
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
}}
