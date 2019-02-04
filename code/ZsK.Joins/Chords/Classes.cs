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


using System;

namespace ZsK.Joins {

    public class JoinException : Exception {
        public JoinException(string message) : base(message) { }
    }

    public class SignatureMismatchException : JoinException {
        public SignatureMismatchException(string message) : base(message) { }
    }

    public class IncompatibleSchedulingException : JoinException {
        public IncompatibleSchedulingException(string message) : base(message) { }
    }

    public class CannotAddSynchMethodToAsyncChord : JoinException {
        public CannotAddSynchMethodToAsyncChord(string message) : base(message) { }
    }

    public class SynchronizationContextNotSet : JoinException {
        public SynchronizationContextNotSet(string message) : base(message) { }
    }

    
    


    internal enum ThreadingType {

        Undefined,

        //TODO: verify the "be executed on the thread of the primary method." bit
        /// <summary>
        /// Default, for synchronous chords only, the body will be executed on the thread of the primary method.
        /// </summary>
        Sync,

        /// <summary>
        /// The async chord body will be sent to the specified thread pool
        /// </summary>
        Pool,

        /// <summary>
        /// A new thread will be created to execute the body of the chord
        /// </summary>
        Spawn,

        /// <summary>
        /// The body of the chord will be executed on the thread of the completing message.
        /// </summary>
        Continue,

        /// <summary>
        /// The body of the chord will be executed on the associated SynchronizationContext via Send or Post 
        /// depending on whether the chord is synchronous or asynchronous.
        /// </summary>
        SyncContext,
    }

    public delegate R F<R>();

    public delegate R F<T, R>(T t);
    public delegate R F1<T, R>(out T t);

    public delegate R F <T, U, R>(T t, U u);
    public delegate R F1<T, U, R>(T t, out U u);
    public delegate R F2<T, U, R>(out T t, out U u);

    public delegate R F<T, U, V, R>(T t, U u, V v);
    public delegate R F1<T, U, V, R>(T t, U u, out V v);
    public delegate R F2<T, U, V, R>(T t, out U u, out V v);
    public delegate R F3<T, U, V, R>(out T t, out U u, out V v);

    public delegate R F<T, U, V, W, R>(T t, U u, V v, W w);
    public delegate R F1<T, U, V, W, R>(T t, U u, V v, out W w);
    public delegate R F2<T, U, V, W, R>(T t, U u, out V v, out W w);
    public delegate R F3<T, U, V, W, R>(T t, out U u, out V v, out W w);
    public delegate R F4<T, U, V, W, R>(out T t, out U u, out V v, out W w);

    public delegate R F<T, U, V, W, X, R>(T t, U u, V v, W w, X x);
    public delegate R F1<T, U, V, W, X, R>(T t, U u, V v, W w, out X x);
    public delegate R F2<T, U, V, W, X, R>(T t, U u, V v, out W w, out X x);
    public delegate R F3<T, U, V, W, X, R>(T t, U u, out V v, out W w, out X x);
    public delegate R F4<T, U, V, W, X, R>(T t, out U u, out V v, out W w, out X x);
    public delegate R F5<T, U, V, W, X, R>(out T t, out U u, out V v, out W w, out X x);

    
    public delegate R F<T, U, V, W, X, Y, R>(T t, U u, V v, W w, X x, Y y);
    public delegate R F1<T, U, V, W, X, Y, R>(T t, U u, V v, W w, X x, out Y y);
    public delegate R F2<T, U, V, W, X, Y, R>(T t, U u, V v, W w, out X x, out Y y);
    public delegate R F3<T, U, V, W, X, Y, R>(T t, U u, V v, out W w, out X x, out Y y);
    public delegate R F4<T, U, V, W, X, Y, R>(T t, U u, out V v, out W w, out X x, out Y y);
    public delegate R F5<T, U, V, W, X, Y, R>(T t, out U u, out V v, out W w, out X x, out Y y);
    public delegate R F6<T, U, V, W, X, Y, R>(out T t, out U u, out V v, out W w, out X x, out Y y);

    public delegate R F<T, U, V, W, X, Y, Z, R>(T t, U u, V v, W w, X x, Y y, Z z);
    public delegate R F1<T, U, V, W, X, Y, Z, R>(T t, U u, V v, W w, X x, Y y, out Z z);
    public delegate R F2<T, U, V, W, X, Y, Z, R>(T t, U u, V v, W w, X x, out Y y, out Z z);
    public delegate R F3<T, U, V, W, X, Y, Z, R>(T t, U u, V v, W w, out X x, out Y y, out Z z);
    public delegate R F4<T, U, V, W, X, Y, Z, R>(T t, U u, V v, out W w, out X x, out Y y, out Z z);
    public delegate R F5<T, U, V, W, X, Y, Z, R>(T t, U u, out V v, out W w, out X x, out Y y, out Z z);
    public delegate R F6<T, U, V, W, X, Y, Z, R>(T t, out U u, out V v, out W w, out X x, out Y y, out Z z);
    public delegate R F7<T, U, V, W, X, Y, Z, R>(out T t, out U u, out V v, out W w, out X x, out Y y, out Z z);

    public delegate R F<T, U, V, W, X, Y, Z, A, R>(T t, U u, V v, W w, X x, Y y, Z z, A a);

    public delegate void M();

    public delegate void M<T>(T t);
    public delegate void M1<T>(out T t);

    public delegate void M<T, U>(T t, U u);
    public delegate void M1<T, U>(T t, out U u);
    public delegate void M2<T, U>(out T t, out U u);
    public delegate void M3<T, U>(out T t, U u);

    public delegate void M<T, U, V>(T t, U u, V v);
    public delegate void M1<T, U, V>(T t, U u, out V v);
    public delegate void M2<T, U, V>(T t, out U u, V v);
    public delegate void M3<T, U, V>(out T t, U u, V v);
    public delegate void M4<T, U, V>(out T t, out U u, V v);
    public delegate void M5<T, U, V>(T t, out U u, out V v);
    public delegate void M6<T, U, V>(out T t, U u, out V v);
    public delegate void M7<T, U, V>(out T t, out U u, out V v);
    
    public delegate void M<T, U, V, W>(T t, U u, V v, W w);
    public delegate void M1<T, U, V, W>(T t, U u, V v, out W w);
    public delegate void M2<T, U, V, W>(T t, U u, out V v, out W w);
    public delegate void M3<T, U, V, W>(T t, out U u, out V v, out W w);
    public delegate void M4<T, U, V, W>(out T t, out U u, out V v, out W w);
    public delegate void M5<T, U, V, W>(T t, U u, out V v, W w);
    public delegate void M6<T, U, V, W>(T t, out U u, V v, W w);
    public delegate void M7<T, U, V, W>(out T t, U u, V v, W w);
    public delegate void M8<T, U, V, W>(out T t, out U u, V v, W w);
    public delegate void M9<T, U, V, W>(out T t, U u, out V v, W w);
    public delegate void M10<T, U, V, W>(out T t, U u, V v, out W w);
    public delegate void M11<T, U, V, W>(T t, out U u, out V v, W w);
    public delegate void M12<T, U, V, W>(T t, out U u, V v, out W w);
    public delegate void M13<T, U, V, W>(out T t, U u, out V v, out W w);
    public delegate void M14<T, U, V, W>(out T t, out U u, V v, out W w);
    public delegate void M15<T, U, V, W>(out T t, out U u, out V v, W w);

    
    public delegate void M<T, U, V, W, X>(T t, U u, V v, W w, X x);
    public delegate void M1<T, U, V, W, X>(out T t, U u, V v, W w, X x);
    public delegate void M2<T, U, V, W, X>(T t, out U u, V v, W w, X x);
    public delegate void M3<T, U, V, W, X>(T t, U u, out V v, W w, X x);
    public delegate void M4<T, U, V, W, X>(T t, U u, V v, out W w, X x);
    public delegate void M5<T, U, V, W, X>(T t, U u, V v, W w, out X x);

    public delegate void M6<T, U, V, W, X>(out T t, out U u, V v, W w, X x);
    public delegate void M7<T, U, V, W, X>(out T t, U u, out V v, W w, X x);
    public delegate void M8<T, U, V, W, X>(out T t, U u, V v, out W w, X x);
    public delegate void M9<T, U, V, W, X>(out T t, U u, V v, W w, out X x);
    public delegate void M10<T, U, V, W, X>(T t, out U u, out V v, W w, X x);
    public delegate void M11<T, U, V, W, X>(T t, out U u, V v, out W w, X x);
    public delegate void M12<T, U, V, W, X>(T t, out U u, V v, W w, out X x);
    public delegate void M13<T, U, V, W, X>(T t, U u, out V v, out W w, X x);
    public delegate void M14<T, U, V, W, X>(T t, U u, out V v, W w, out X x);
    public delegate void M15<T, U, V, W, X>(T t, U u, V v, out W w, out X x);

    public delegate void M16<T, U, V, W, X>(out T t, out U u, out V v, W w, X x);
    public delegate void M17<T, U, V, W, X>(out T t, out U u, V v, out W w, X x);
    public delegate void M18<T, U, V, W, X>(out T t, out U u, V v, W w, out X x);
    public delegate void M19<T, U, V, W, X>(out T t, U u, out V v, out W w, X x);
    public delegate void M20<T, U, V, W, X>(out T t, U u, out V v, W w, out X x);

    public delegate void M<T, U, V, W, X, Y>(T t, U u, V v, W w, X x, Y y);
    public delegate void M1<T, U, V, W, X, Y>(T t, U u, V v, W w, X x, out Y y);
    public delegate void M2<T, U, V, W, X, Y>(T t, U u, V v, W w, out X x, out Y y);
    public delegate void M3<T, U, V, W, X, Y>(T t, U u, V v, out W w, out X x, out Y y);
    public delegate void M4<T, U, V, W, X, Y>(T t, U u, out V v, out W w, out X x, out Y y);
    public delegate void M5<T, U, V, W, X, Y>(T t, out U u, out V v, out W w, out X x, out Y y);
    public delegate void M6<T, U, V, W, X, Y>(out T t, out U u, out V v, out W w, out X x, out Y y);

    public delegate void M<T, U, V, W, X, Y, Z>(T t, U u, V v, W w, X x, Y y, Z z);
    public delegate void M1<T, U, V, W, X, Y, Z>(T t, U u, V v, W w, X x, Y y, out Z z);
    public delegate void M2<T, U, V, W, X, Y, Z>(T t, U u, V v, W w, X x, out Y y, out Z z);
    public delegate void M3<T, U, V, W, X, Y, Z>(T t, U u, V v, W w, out X x, out Y y, out Z z);
    public delegate void M4<T, U, V, W, X, Y, Z>(T t, U u, V v, out W w, out X x, out Y y, out Z z);
    public delegate void M5<T, U, V, W, X, Y, Z>(T t, U u, out V v, out W w, out X x, out Y y, out Z z);
    public delegate void M6<T, U, V, W, X, Y, Z>(T t, out U u, out V v, out W w, out X x, out Y y, out Z z);
    public delegate void M7<T, U, V, W, X, Y, Z>(out T t, out U u, out V v, out W w, out X x, out Y y, out Z z);


    public delegate void M<T, U, V, W, X, Y, Z, A>(T t, U u, V v, W w, X x, Y y, Z z, A a);
    public delegate void M<T, U, V, W, X, Y, Z, A, B>(T t, U u, V v, W w, X x, Y y, Z z, A a, B b);
    public delegate void M<T, U, V, W, X, Y, Z, A, B, C>(T t, U u, V v, W w, X x, Y y, Z z, A a, B b, C c);
    public delegate void M<T, U, V, W, X, Y, Z, A, B, C, D>(T t, U u, V v, W w, X x, Y y, Z z, A a, B b, C c, D d);
    public delegate void M<T, U, V, W, X, Y, Z, A, B, C, D, E>(T t, U u, V v, W w, X x, Y y, Z z, A a, B b, C c, D d, E e);
    public delegate void M<T, U, V, W, X, Y, Z, A, B, C, D, E, F>(T t, U u, V v, W w, X x, Y y, Z z, A a, B b, C c, D d, E e, F f);


    public class @async {  }
}
