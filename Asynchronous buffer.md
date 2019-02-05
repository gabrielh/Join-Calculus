**Asynchronous buffer**

One of the simplest example of join-calculus it uses one asynchronous method to put data in the buffer and one synchronous channels to retrieve a value from the buffer. The get will block until a value is available in the buffer, while the put method will return immediately without waiting for the value to he retrieved as it is the case in the synchronous buffer

Here is the code for an asynchrounous buffer:

{{
public class AsyncBuffer<T> {

    public @async Put(T t) { return put.Invoked(t); }
    public T Get() { return get.Invoked<T>(); }

    private Joiner joiner;

    private SyncMethod get;
    private AsyncMethod put;

    public AsyncBuffer() {
        joiner = new Joiner();

        get = joiner.NewSyncMethod<T>(Get);
        put = joiner.NewAsyncMethod<T>(Put);

        (get & put).Do((T t) => {
            return t;
        });
    }
}
}}

