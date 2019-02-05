**Join many**

The simples example, it just waits for a predefined number of blocking calls to the Join() method to release them all simultaneously. The example illustrate the library's support for chords that support large number of identical methods.

Here is the code for _join many_:

{{
public class JoinMany {

    public void Join() { join.Invoked(); }

    private Joiner joiner= new Joiner();

    private SyncMethod join;

    public JoinMany(int n) {
        join = joiner.NewSyncMethod(Join);

        (n*join).Do(() => { });
    }
}
}}
