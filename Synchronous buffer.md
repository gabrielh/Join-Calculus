**Synchronous buffer**

One of the simplest example of join-calculus it uses two synchronous channels to synchronize the putter and getter threads. When both get and put methods have been invoked, the value passed in the put is returned by the get method. Both get and put method will block until both methods have been invoked: get blocks until a put has been invoked, put blocks until a get has been invoked.

Here is the code for a synchrounous buffer:

{{
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
            return t;
        });
    }
}
}}

