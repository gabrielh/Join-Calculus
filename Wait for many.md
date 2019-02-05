**Wait for many**

Another simple example where the caller to the synchronous _Wait_ method is blocked until the asynchronous _Signal_ method has been called a configurable number of times. 

Here is the code for _Wait for many_:

{{
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
}}
