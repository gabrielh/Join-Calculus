/*
Copyright (C) by Gabriel Zs. K. Horvath

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/


namespace ZsK.Joins {

    public partial class SyncMethod : Method {
        public void Invoked() {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { });
            joiner.TryMatchAndWait(message, out outArguments);
        }

        public void Invoked<T>(T t) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t });
            joiner.TryMatchAndWait(message, out outArguments);
        }

        public void Invoked<T>(out T t) {
            object[] outArguments = null;
            t = default(T);
            var message = new SyncMessage(this, new object[] { t });
            joiner.TryMatchAndWait(message, out outArguments);
            t = (T)outArguments[message.ArgumentIndex];
        }

        public void Invoked<T, U>(T t, U u) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u });
            joiner.TryMatchAndWait(message, out outArguments);
        }

        public void Invoked<T, U>(T t, out U u) {
            object[] outArguments = null;
            u = default(U);
            var message = new SyncMessage(this, new object[] { t, u });
            joiner.TryMatchAndWait(message, out outArguments);
            if (message.ArgumentSubIndex != -1) {
                u = ((U[])outArguments[message.ArgumentIndex + 1]) [message.ArgumentSubIndex];
            } else {
                u = (U)outArguments[message.ArgumentIndex + 1];
            }
        }

        public void Invoked<T, U>(out T t, out U u) {
            object[] outArguments = null;
            u = default(U);
            t = default(T);
            var message = new SyncMessage(this, new object[] { t, u });
            joiner.TryMatchAndWait(message, out outArguments);
            if (message.ArgumentSubIndex != -1) {
                t = ((T[])outArguments[message.ArgumentIndex + 0])[message.ArgumentSubIndex];
                u = ((U[])outArguments[message.ArgumentIndex + 1])[message.ArgumentSubIndex];
            } else {
                t = (T)outArguments[message.ArgumentIndex + 0];
                u = (U)outArguments[message.ArgumentIndex + 1];
            }
        }

        public void Invoked<T, U, V>(T t, U u, V v) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u, v });
            joiner.TryMatchAndWait(message, out outArguments);
        }

        public void Invoked<T, U, V>(T t, U u, out V v) {
            object[] outArguments = null;
            v = default(V);
            var message = new SyncMessage(this, new object[] { t, u, v });
            joiner.TryMatchAndWait(message, out outArguments);
            if (message.ArgumentSubIndex != -1) {
                v = ((V[])outArguments[message.ArgumentIndex + 2])[message.ArgumentSubIndex];
            } else {
                v = (V)outArguments[message.ArgumentIndex + 2];
            }
        }

        public void Invoked<T, U, V>(T t, out U u, out V v) {
            u = default(U);
            v = default(V);
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u, v });
            joiner.TryMatchAndWait(message, out outArguments);
            if (message.ArgumentSubIndex != -1) {
                u = ((U[])outArguments[message.ArgumentIndex + 1])[message.ArgumentSubIndex];
                v = ((V[])outArguments[message.ArgumentIndex + 2])[message.ArgumentSubIndex];
            } else {
                u = (U)outArguments[message.ArgumentIndex + 1];
                v = (V)outArguments[message.ArgumentIndex + 2];
            }
        }

        public void Invoked<T, U, V>(out T t, out U u, out V v) {
            object[] outArguments = null;
            t = default(T);
            u = default(U);
            v = default(V);
            var message = new SyncMessage(this, new object[] { t, u, v });
            joiner.TryMatchAndWait(message, out outArguments);
            if (message.ArgumentSubIndex != -1) {
                t = ((T[])outArguments[message.ArgumentIndex + 0])[message.ArgumentSubIndex];
                u = ((U[])outArguments[message.ArgumentIndex + 1])[message.ArgumentSubIndex];
                v = ((V[])outArguments[message.ArgumentIndex + 2])[message.ArgumentSubIndex];
            } else {
                t = (T)outArguments[message.ArgumentIndex + 0];
                u = (U)outArguments[message.ArgumentIndex + 1];
                v = (V)outArguments[message.ArgumentIndex + 2];
            }
        }

        public void Invoked<T, U, V, W>(T t, U u, V v, W w) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u, v, w });
            joiner.TryMatchAndWait(message, out outArguments);
        }

        public void Invoked<T, U, V, W>(T t, U u, V v, out W w) {
            object[] outArguments = null;
            w = default(W);
            var message = new SyncMessage(this, new object[] { t, u, v, w });
            joiner.TryMatchAndWait(message, out outArguments);
            if (message.ArgumentSubIndex != -1) {
                w = ((W[])outArguments[message.ArgumentIndex + 3])[message.ArgumentSubIndex];
            } else {
                w = (W)outArguments[message.ArgumentIndex + 3];
            }
        }

        public void Invoked<T, U, V, W>(T t, U u, out V v, out W w) {
            object[] outArguments = null;
            v = default(V);
            w = default(W);
            var message = new SyncMessage(this, new object[] { t, u, v, w });
            joiner.TryMatchAndWait(message, out outArguments);
            if (message.ArgumentSubIndex != -1) {
                v = ((V[])outArguments[message.ArgumentIndex + 2])[message.ArgumentSubIndex];
                w = ((W[])outArguments[message.ArgumentIndex + 3])[message.ArgumentSubIndex];
            } else {
                v = (V)outArguments[message.ArgumentIndex + 2];
                w = (W)outArguments[message.ArgumentIndex + 3];
            }
        }

        public void Invoked<T, U, V, W>(T t, out U u, out V v, out W w) {
            object[] outArguments = null;
            u = default(U);
            v = default(V);
            w = default(W);
            var message = new SyncMessage(this, new object[] { t, u, v, w });
            joiner.TryMatchAndWait(message, out outArguments);
            if (message.ArgumentSubIndex != -1) {
                u = ((U[])outArguments[message.ArgumentIndex + 1])[message.ArgumentSubIndex];
                v = ((V[])outArguments[message.ArgumentIndex + 2])[message.ArgumentSubIndex];
                w = ((W[])outArguments[message.ArgumentIndex + 3])[message.ArgumentSubIndex];
            } else {
                u = (U)outArguments[message.ArgumentIndex + 1];
                v = (V)outArguments[message.ArgumentIndex + 2];
                w = (W)outArguments[message.ArgumentIndex + 3];
            }
        }

        public void Invoked<T, U, V, W>(out T t, out U u, out V v, out W w) {
            object[] outArguments = null;
            t = default(T);
            u = default(U);
            v = default(V);
            w = default(W);
            var message = new SyncMessage(this, new object[] { t, u, v, w });
            joiner.TryMatchAndWait(message, out outArguments);
            if (message.ArgumentSubIndex != -1) {
                t = ((T[])outArguments[message.ArgumentIndex + 0])[message.ArgumentSubIndex];
                u = ((U[])outArguments[message.ArgumentIndex + 1])[message.ArgumentSubIndex];
                v = ((V[])outArguments[message.ArgumentIndex + 2])[message.ArgumentSubIndex];
                w = ((W[])outArguments[message.ArgumentIndex + 3])[message.ArgumentSubIndex];
            } else {
                t = (T)outArguments[message.ArgumentIndex + 0];
                u = (U)outArguments[message.ArgumentIndex + 1];
                v = (V)outArguments[message.ArgumentIndex + 2];
                w = (W)outArguments[message.ArgumentIndex + 3];
            }
        }

        public void Invoked<T, U, V, W, X>(T t, U u, V v, W w, X x) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u, v, w, x });
            joiner.TryMatchAndWait(message, out outArguments);
        }

        public void Invoked<T, U, V, W, X, Y>(T t, U u, V v, W w, X x, Y y) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u, v, w, x, y });
            joiner.TryMatchAndWait(message, out outArguments);
        }

        public void Invoked<T, U, V, W, X, Y, Z>(T t, U u, V v, W w, X x, Y y, Z z) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u, v, w, x, y, z });
            joiner.TryMatchAndWait(message, out outArguments);
        }

        public R Invoked<R>() {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { });
            R result = (R)joiner.TryMatchAndWait(message, out outArguments);
            return result;
        }

        public R Invoked<T, R>(T t) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] {t});
            R result = (R)joiner.TryMatchAndWait(message, out outArguments);
            return result;
        }

        public R Invoked<T, U, R>(T t, U u) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u });
            R result = (R)joiner.TryMatchAndWait(message, out outArguments);
            return result;
        }

        public R Invoked<T, U, V, R>(T t, U u, V v) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u, v });
            R result = (R)joiner.TryMatchAndWait(message, out outArguments);
            return result;
        }

        public R Invoked<T, U, V, W, R>(T t, U u, V v, W w) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u, v, w });
            R result = (R)joiner.TryMatchAndWait(message, out outArguments);
            return result;
        }

        public R Invoked<T, U, V, W, X, R>(T t, U u, V v, W w, X x) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u, v, w, x });
            R result = (R)joiner.TryMatchAndWait(message, out outArguments);
            return result;
        }

        public R Invoked<T, U, V, W, X, Y, R>(T t, U u, V v, W w, X x, Y y) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u, v, w, x, y });
            R result = (R)joiner.TryMatchAndWait(message, out outArguments);
            return result;
        }

        public R Invoked<T, U, V, W, X, Y, Z, R>(T t, U u, V v, W w, X x, Y y, Z z) {
            object[] outArguments = null;
            var message = new SyncMessage(this, new object[] { t, u, v, w, x, y, z});
            R result = (R)joiner.TryMatchAndWait(message, out outArguments);
            return result;
        }
    }
}
