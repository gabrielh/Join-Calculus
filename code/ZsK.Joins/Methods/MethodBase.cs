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
using System.Collections.Generic;
using System.Linq;

namespace ZsK.Joins {

    public abstract class MethodBase {


        /// <summary>
        /// Joiner.NewChord = p1 & p2 & p3 & p4 > pppp;
        /// & returns rhs
        /// lhs:P1 Joins3.async 0
        /// rhs:P2 Joins3.async 0
        /// lhs:P2 Joins3.async 0
        /// rhs:P3 Joins3.async 0
        /// lhs:P3 Joins3.async 0
        /// rhs:P4 Joins3.async 0
        /// 
        /// 
        /// Joiner.NewChord = p1 & p2 & p3 & p4 > pppp;
        /// & returns lhs
        /// lhs:P1 Joins3.async 0
        /// rhs:P2 Joins3.async 0
        /// lhs:P1 Joins3.async 0
        /// rhs:P3 Joins3.async 0
        /// lhs:P1 Joins3.async 0
        /// rhs:P4 Joins3.async 0
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static MethodBase operator &(MethodBase lhs, MethodBase rhs) {
            lhs.CreateChord();
            lhs.chord.AddMethod(rhs, rhs.tempMultiplier);
            rhs.tempMultiplier = 1;
            return lhs;
        }
        //use once, reset to 1 after having read the value
        private int tempMultiplier = 1;
        public static MethodBase operator *(int multiplier, MethodBase method) {
            method.tempMultiplier = multiplier;
            //TODO: assert the multiplier is >= 1;
            return method;
        }

        protected Joiner joiner;
        internal Joiner Joiner { get { return joiner; } }

        private Chord chord;

        protected Delegate @delegate;

        internal IEnumerable<Type> DelegateArgumentTypes {
            get {
                return @delegate.Method.GetParameters().Select(pi => pi.ParameterType);
            }
        }

        internal Type ReturnType {
            get { return @delegate.Method.ReturnType; }
        }

        private void CreateChord() {
            if (chord == null) {
                if (this is SyncMethod) {
                    chord = new SyncChord(this as SyncMethod);
                } else {
                    chord = new AsyncChord();
                }
                chord.AddMethod(this, tempMultiplier);
                tempMultiplier = 1;
            }
            
        }

        protected string name;
        internal string Name {
            get {
                return name;
            }
        }

        private Chord NewChord(ThreadingType threadingType) {
            var chord = NewChord();
            chord.ThreadingType = threadingType;
            return chord;
        }

        private Chord NewChord() {
            //find out whether the chord is sync or async
            var chord = this.chord;
            this.chord = null;
            joiner.AddChord(chord);
            return chord;
        }

        public Chord Named(string chordName) {
            CreateChord();
            return NewChord().SetName(chordName); 
        }

        private void PrivatePool(Delegate d) {
            //TODO: only valid for async chords chord is AsyncChord...
            CreateChord();
            NewChord().PrivatePool(d);
        }

        private void PrivateSpawn(Delegate d) {
            //TODO: only valid for async chords chord is AsyncChord...
            CreateChord();
            NewChord(ThreadingType.Spawn).PrivateSpawn(d);
        }

        private void PrivateContinue(Delegate d) {
            //TODO: only valid for async chords chord is AsyncChord...
            CreateChord();
            NewChord(ThreadingType.Continue).PrivateContinue(d);
        }

        private void PrivateSyncContext(Delegate d) {
            //TODO: only valid for async chords chord is AsyncChord...
            CreateChord();
            NewChord(ThreadingType.SyncContext).SyncContext(d);
        }

        public void DoAny(Delegate d) {
            //TODO: only valid for sync chords chord is SyncChord...
            CreateChord();
            NewChord(ThreadingType.Sync).PrivateDo(d);
        }

        #region Pool overloads
        public void Pool(M m) { PrivatePool(m); }
        public void Pool<T>(M<T> m) { PrivatePool(m); }
        //public void Pool<T>(M1<T> m) { PrivatePool(m); }
        public void Pool<T, U>(M<T, U> m) { PrivatePool(m); }
        //public void Pool<T, U>(M1<T, U> m) { PrivatePool(m); }
        //public void Pool<T, U>(M2<T, U> m) { PrivatePool(m); }
        //public void Pool<T, U>(M3<T, U> m) { PrivatePool(m); }
        public void Pool<T, U, V>(M<T, U, V> m) { PrivatePool(m); }
        //public void Pool<T, U, V>(M1<T, U, V> m) { PrivatePool(m); }
        //public void Pool<T, U, V>(M2<T, U, V> m) { PrivatePool(m); }
        //public void Pool<T, U, V>(M3<T, U, V> m) { PrivatePool(m); }
        //public void Pool<T, U, V>(M4<T, U, V> m) { PrivatePool(m); }
        //public void Pool<T, U, V>(M5<T, U, V> m) { PrivatePool(m); }
        //public void Pool<T, U, V>(M6<T, U, V> m) { PrivatePool(m); }
        //public void Pool<T, U, V>(M7<T, U, V> m) { PrivatePool(m); }
        public void Pool<T, U, V, W>(M<T, U, V, W> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W>(M1<T, U, V, W> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W>(M2<T, U, V, W> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W>(M3<T, U, V, W> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W>(M4<T, U, V, W> m) { PrivatePool(m); }

        public void Pool<T, U, V, W, X, Y, Z, A>(M<T, U, V, W, X, Y, Z, A> m) { PrivatePool(m); }
        public void Pool<T, U, V, W, X, Y, Z, A, B>(M<T, U, V, W, X, Y, Z, A, B> m) { PrivatePool(m); }
        public void Pool<T, U, V, W, X, Y, Z, A, B, C>(M<T, U, V, W, X, Y, Z, A, B, C> m) { PrivatePool(m); }
        public void Pool<T, U, V, W, X, Y, Z, A, B, C, D>(M<T, U, V, W, X, Y, Z, A, B, C, D> m) { PrivatePool(m); }
        public void Pool<T, U, V, W, X, Y, Z, A, B, C, D, E>(M<T, U, V, W, X, Y, Z, A, B, C, D, E> m) { PrivatePool(m); }
        public void Pool<T, U, V, W, X, Y, Z, A, B, C, D, E, F>(M<T, U, V, W, X, Y, Z, A, B, C, D, E, F> m) { PrivatePool(m); }

        //public void Pool<T, U, V, W, X>(M<T, U, V, W, X> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X>(M1<T, U, V, W, X> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X>(M2<T, U, V, W, X> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X>(M3<T, U, V, W, X> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X>(M4<T, U, V, W, X> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X>(M5<T, U, V, W, X> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y>(M<T, U, V, W, X, Y> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y>(M1<T, U, V, W, X, Y> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y>(M2<T, U, V, W, X, Y> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y>(M3<T, U, V, W, X, Y> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y>(M4<T, U, V, W, X, Y> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y>(M5<T, U, V, W, X, Y> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y>(M6<T, U, V, W, X, Y> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z>(M<T, U, V, W, X, Y, Z> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z>(M1<T, U, V, W, X, Y, Z> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z>(M2<T, U, V, W, X, Y, Z> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z>(M3<T, U, V, W, X, Y, Z> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z>(M4<T, U, V, W, X, Y, Z> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z>(M5<T, U, V, W, X, Y, Z> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z>(M6<T, U, V, W, X, Y, Z> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z>(M7<T, U, V, W, X, Y, Z> m) { PrivatePool(m); }

        //public void Pool<T>(F<T> m) { PrivatePool(m); }
        //public void Pool<T, R>(F<T, R> m) { PrivatePool(m); }
        //public void Pool<T, R>(F1<T, R> m) { PrivatePool(m); }
        //public void Pool<T, U, R>(F<T, U, R> m) { PrivatePool(m); }
        //public void Pool<T, U, R>(F1<T, U, R> m) { PrivatePool(m); }
        //public void Pool<T, U, R>(F2<T, U, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, R>(F3<T, U, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, R>(F<T, U, V, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, R>(F1<T, U, V, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, R>(F2<T, U, V, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, R>(F3<T, U, V, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, R>(F4<T, U, V, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, R>(F5<T, U, V, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, R>(F6<T, U, V, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, R>(F7<T, U, V, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, R>(F<T, U, V, W, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, R>(F1<T, U, V, W, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, R>(F2<T, U, V, W, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, R>(F3<T, U, V, W, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, R>(F4<T, U, V, W, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, R>(F<T, U, V, W, X, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, R>(F1<T, U, V, W, X, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, R>(F2<T, U, V, W, X, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, R>(F3<T, U, V, W, X, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, R>(F4<T, U, V, W, X, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, R>(F5<T, U, V, W, X, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, R>(F<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, R>(F1<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, R>(F2<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, R>(F3<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, R>(F4<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, R>(F5<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, R>(F6<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, Z, R>(F<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, Z, R>(F1<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, Z, R>(F2<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, Z, R>(F3<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, Z, R>(F4<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, Z, R>(F5<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, Z, R>(F6<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        ////public void Pool<T, U, V, W, X, Y, Z, R>(F7<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }

        #endregion

        #region Spawn overloads
        public void Spawn(M m) { PrivateSpawn(m); }
        public void Spawn<T>(M<T> m) { PrivateSpawn(m); }
        public void Spawn<T, U>(M<T, U> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V>(M<T, U, V> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W>(M<T, U, V, W> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, X>(M<T, U, V, W, X> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, X, Y>(M<T, U, V, W, X, Y> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, X, Y, Z>(M<T, U, V, W, X, Y, Z> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, X, Y, Z, A>(M<T, U, V, W, X, Y, Z, A> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, X, Y, Z, A, B>(M<T, U, V, W, X, Y, Z, A, B> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, X, Y, Z, A, B, C>(M<T, U, V, W, X, Y, Z, A, B, C> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, X, Y, Z, A, B, C, D>(M<T, U, V, W, X, Y, Z, A, B, C, D> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, X, Y, Z, A, B, C, D, E>(M<T, U, V, W, X, Y, Z, A, B, C, D, E> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, X, Y, Z, A, B, C, D, E, F>(M<T, U, V, W, X, Y, Z, A, B, C, D, E, F> m) { PrivateSpawn(m); }
        /*
        public void Spawn<T>(F<T> m) { PrivateSpawn(m); }
        public void Spawn<T, R>(F<T, R> m) { PrivateSpawn(m); }
        public void Spawn<T, R>(F1<T, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, R>(F<T, U, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, R>(F1<T, U, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, R>(F2<T, U, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, R>(F3<T, U, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, R>(F<T, U, V, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, R>(F1<T, U, V, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, R>(F2<T, U, V, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, R>(F3<T, U, V, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, R>(F4<T, U, V, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, R>(F5<T, U, V, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, R>(F6<T, U, V, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, R>(F7<T, U, V, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, R>(F<T, U, V, W, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, R>(F1<T, U, V, W, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, R>(F2<T, U, V, W, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, R>(F3<T, U, V, W, R> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W, R>(F4<T, U, V, W, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, R>(F<T, U, V, W, X, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, R>(F1<T, U, V, W, X, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, R>(F2<T, U, V, W, X, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, R>(F3<T, U, V, W, X, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, R>(F4<T, U, V, W, X, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, R>(F5<T, U, V, W, X, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, R>(F<T, U, V, W, X, Y, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, R>(F1<T, U, V, W, X, Y, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, R>(F2<T, U, V, W, X, Y, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, R>(F3<T, U, V, W, X, Y, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, R>(F4<T, U, V, W, X, Y, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, R>(F5<T, U, V, W, X, Y, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, R>(F6<T, U, V, W, X, Y, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z, R>(F<T, U, V, W, X, Y, Z, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z, R>(F1<T, U, V, W, X, Y, Z, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z, R>(F2<T, U, V, W, X, Y, Z, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z, R>(F3<T, U, V, W, X, Y, Z, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z, R>(F4<T, U, V, W, X, Y, Z, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z, R>(F5<T, U, V, W, X, Y, Z, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z, R>(F6<T, U, V, W, X, Y, Z, R> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z, R>(F7<T, U, V, W, X, Y, Z, R> m) { PrivateSpawn(m); }
        */
        #endregion

        #region Continue overloads
        public void Continue(M m) { PrivateContinue(m); }
        public void Continue<T>(M<T> m) { PrivateContinue(m); }
        //public void Continue<T>(M1<T> m) { PrivateContinue(m); }
        public void Continue<T, U>(M<T, U> m) { PrivateContinue(m); }
        //public void Continue<T, U>(M1<T, U> m) { PrivateContinue(m); }
        //public void Continue<T, U>(M2<T, U> m) { PrivateContinue(m); }
        //public void Continue<T, U>(M3<T, U> m) { PrivateContinue(m); }
        public void Continue<T, U, V>(M<T, U, V> m) { PrivateContinue(m); }
        //public void Continue<T, U, V>(M1<T, U, V> m) { PrivateContinue(m); }
        //public void Continue<T, U, V>(M2<T, U, V> m) { PrivateContinue(m); }
        //public void Continue<T, U, V>(M3<T, U, V> m) { PrivateContinue(m); }
        //public void Continue<T, U, V>(M4<T, U, V> m) { PrivateContinue(m); }
        //public void Continue<T, U, V>(M5<T, U, V> m) { PrivateContinue(m); }
        //public void Continue<T, U, V>(M6<T, U, V> m) { PrivateContinue(m); }
        //public void Continue<T, U, V>(M7<T, U, V> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W>(M<T, U, V, W> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W>(M1<T, U, V, W> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W>(M2<T, U, V, W> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W>(M3<T, U, V, W> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W>(M4<T, U, V, W> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, X>(M<T, U, V, W, X> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X>(M1<T, U, V, W, X> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X>(M2<T, U, V, W, X> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X>(M3<T, U, V, W, X> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X>(M4<T, U, V, W, X> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X>(M5<T, U, V, W, X> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, X, Y>(M<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M1<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M2<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M3<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M4<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M5<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M6<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, X, Y, Z>(M<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M1<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M2<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M3<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M4<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M5<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M6<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M7<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, X, Y, Z, A>(M<T, U, V, W, X, Y, Z, A> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, X, Y, Z, A, B>(M<T, U, V, W, X, Y, Z, A, B> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, X, Y, Z, A, B, C>(M<T, U, V, W, X, Y, Z, A, B, C> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, X, Y, Z, A, B, C, D>(M<T, U, V, W, X, Y, Z, A, B, C, D> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, X, Y, Z, A, B, C, D, E>(M<T, U, V, W, X, Y, Z, A, B, C, D, E> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, X, Y, Z, A, B, C, D, E, F>(M<T, U, V, W, X, Y, Z, A, B, C, D, E, F> m) { PrivateContinue(m); }

        /*
        public void Continue<T>(F<T> m) { PrivateContinue(m); }
        public void Continue<T, R>(F<T, R> m) { PrivateContinue(m); }
        public void Continue<T, R>(F1<T, R> m) { PrivateContinue(m); }
        public void Continue<T, U, R>(F<T, U, R> m) { PrivateContinue(m); }
        public void Continue<T, U, R>(F1<T, U, R> m) { PrivateContinue(m); }
        public void Continue<T, U, R>(F2<T, U, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, R>(F3<T, U, R> m) { PrivateContinue(m); }
        public void Continue<T, U, V, R>(F<T, U, V, R> m) { PrivateContinue(m); }
        public void Continue<T, U, V, R>(F1<T, U, V, R> m) { PrivateContinue(m); }
        public void Continue<T, U, V, R>(F2<T, U, V, R> m) { PrivateContinue(m); }
        public void Continue<T, U, V, R>(F3<T, U, V, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, R>(F4<T, U, V, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, R>(F5<T, U, V, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, R>(F6<T, U, V, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, R>(F7<T, U, V, R> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, R>(F<T, U, V, W, R> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, R>(F1<T, U, V, W, R> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, R>(F2<T, U, V, W, R> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, R>(F3<T, U, V, W, R> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W, R>(F4<T, U, V, W, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, R>(F<T, U, V, W, X, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, R>(F1<T, U, V, W, X, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, R>(F2<T, U, V, W, X, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, R>(F3<T, U, V, W, X, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, R>(F4<T, U, V, W, X, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, R>(F5<T, U, V, W, X, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, R>(F<T, U, V, W, X, Y, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, R>(F1<T, U, V, W, X, Y, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, R>(F2<T, U, V, W, X, Y, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, R>(F3<T, U, V, W, X, Y, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, R>(F4<T, U, V, W, X, Y, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, R>(F5<T, U, V, W, X, Y, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, R>(F6<T, U, V, W, X, Y, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z, R>(F<T, U, V, W, X, Y, Z, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z, R>(F1<T, U, V, W, X, Y, Z, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z, R>(F2<T, U, V, W, X, Y, Z, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z, R>(F3<T, U, V, W, X, Y, Z, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z, R>(F4<T, U, V, W, X, Y, Z, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z, R>(F5<T, U, V, W, X, Y, Z, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z, R>(F6<T, U, V, W, X, Y, Z, R> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z, R>(F7<T, U, V, W, X, Y, Z, R> m) { PrivateContinue(m); }
        */
        #endregion

        #region Do overloads
        public void Do(M m) { DoAny(m); }
        public void Do<T>(M<T> m) { DoAny(m); }
        public void Do<T>(M1<T> m) { DoAny(m); }
        public void Do<T, U>(M<T, U> m) { DoAny(m); }
        public void Do<T, U>(M1<T, U> m) { DoAny(m); }
        public void Do<T, U>(M2<T, U> m) { DoAny(m); }
        public void Do<T, U>(M3<T, U> m) { DoAny(m); }
        public void Do<T, U, V>(M<T, U, V> m) { DoAny(m); }
        public void Do<T, U, V>(M1<T, U, V> m) { DoAny(m); }
        public void Do<T, U, V>(M2<T, U, V> m) { DoAny(m); }
        public void Do<T, U, V>(M3<T, U, V> m) { DoAny(m); }
        public void Do<T, U, V>(M4<T, U, V> m) { DoAny(m); }
        public void Do<T, U, V>(M5<T, U, V> m) { DoAny(m); }
        public void Do<T, U, V>(M6<T, U, V> m) { DoAny(m); }
        public void Do<T, U, V>(M7<T, U, V> m) { DoAny(m); }

        public void Do<T, U, V, W>(M<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M1<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M2<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M3<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M4<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M5<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M6<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M7<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M8<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M9<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M10<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M11<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M12<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M13<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M14<T, U, V, W> m) { DoAny(m); }
        public void Do<T, U, V, W>(M15<T, U, V, W> m) { DoAny(m); }

        public void Do<T, U, V, W, X>(M<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M1<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M2<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M3<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M4<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M5<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M6<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M7<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M8<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M9<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M10<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M11<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M12<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M13<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M14<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M15<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M16<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M17<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M18<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M19<T, U, V, W, X> m) { DoAny(m); }
        public void Do<T, U, V, W, X>(M20<T, U, V, W, X> m) { DoAny(m); }

        public void Do<T, U, V, W, X, Y>(M<T, U, V, W, X, Y> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y>(M1<T, U, V, W, X, Y> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y>(M2<T, U, V, W, X, Y> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y>(M3<T, U, V, W, X, Y> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y>(M4<T, U, V, W, X, Y> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y>(M5<T, U, V, W, X, Y> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y>(M6<T, U, V, W, X, Y> m) { DoAny(m); }

        public void Do<T, U, V, W, X, Y, Z>(M<T, U, V, W, X, Y, Z> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z>(M1<T, U, V, W, X, Y, Z> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z>(M2<T, U, V, W, X, Y, Z> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z>(M3<T, U, V, W, X, Y, Z> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z>(M4<T, U, V, W, X, Y, Z> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z>(M5<T, U, V, W, X, Y, Z> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z>(M6<T, U, V, W, X, Y, Z> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z>(M7<T, U, V, W, X, Y, Z> m) { DoAny(m); }

        public void Do<T, U, V, W, X, Y, Z, A>(M<T, U, V, W, X, Y, Z, A> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z, A, B>(M<T, U, V, W, X, Y, Z, A, B> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z, A, B, C>(M<T, U, V, W, X, Y, Z, A, B, C> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z, A, B, C, D>(M<T, U, V, W, X, Y, Z, A, B, C, D> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z, A, B, C, D, E>(M<T, U, V, W, X, Y, Z, A, B, C, D, E> m) { DoAny(m); }
        public void Do<T, U, V, W, X, Y, Z, A, B, C, D, E, F>(M<T, U, V, W, X, Y, Z, A, B, C, D, E, F> m) { DoAny(m); }


        public void Do<T>(F<T> m) { DoAny(m); }
        public void Do<T, R>(F<T, R> m) { DoAny(m); }
        public void Do<T, R>(F1<T, R> m) { DoAny(m); }
        public void Do<T, U, R>(F<T, U, R> m) { DoAny(m); }
        public void Do<T, U, R>(F1<T, U, R> m) { DoAny(m); }
        public void Do<T, U, R>(F2<T, U, R> m) { DoAny(m); }
        //public void Do<T, U, R>(F3<T, U, R> m) { PrivateDo(m); }
        public void Do<T, U, V, R>(F<T, U, V, R> m) { DoAny(m); }
        public void Do<T, U, V, R>(F1<T, U, V, R> m) { DoAny(m); }
        public void Do<T, U, V, R>(F2<T, U, V, R> m) { DoAny(m); }
        public void Do<T, U, V, R>(F3<T, U, V, R> m) { DoAny(m); }
        //public void Do<T, U, V, R>(F4<T, U, V, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, R>(F5<T, U, V, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, R>(F6<T, U, V, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, R>(F7<T, U, V, R> m) { PrivateDo(m); }
        public void Do<T, U, V, W, R>(F<T, U, V, W, R> m) { DoAny(m); }
        public void Do<T, U, V, W, R>(F1<T, U, V, W, R> m) { DoAny(m); }
        public void Do<T, U, V, W, R>(F2<T, U, V, W, R> m) { DoAny(m); }
        public void Do<T, U, V, W, R>(F3<T, U, V, W, R> m) { DoAny(m); }
        public void Do<T, U, V, W, R>(F4<T, U, V, W, R> m) { DoAny(m); }
        public void Do<T, U, V, W, X, R>(F<T, U, V, W, X, R> m) { DoAny(m); }
        //public void Do<T, U, V, W, X, R>(F1<T, U, V, W, X, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, R>(F2<T, U, V, W, X, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, R>(F3<T, U, V, W, X, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, R>(F4<T, U, V, W, X, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, R>(F5<T, U, V, W, X, R> m) { PrivateDo(m); }
        public void Do<T, U, V, W, X, Y, R>(F<T, U, V, W, X, Y, R> m) { DoAny(m); }
        //public void Do<T, U, V, W, X, Y, R>(F1<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F2<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F3<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F4<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F5<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F6<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        public void Do<T, U, V, W, X, Y, Z, R>(F<T, U, V, W, X, Y, Z, R> m) { DoAny(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F1<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F2<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F3<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F4<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F5<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F6<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F7<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }

        public void Do<T, U, V, W, X, Y, Z, A, R>(F<T, U, V, W, X, Y, Z, A, R> m) { DoAny(m); }
        #endregion

        #region SyncContext overloads
        public void SyncContext(M m) { PrivateSyncContext(m); }
        public void SyncContext<T>(M<T> m) { PrivateSyncContext(m); }
        public void SyncContext<T>(M1<T> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U>(M<T, U> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U>(M1<T, U> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U>(M2<T, U> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U>(M3<T, U> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V>(M<T, U, V> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V>(M1<T, U, V> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V>(M2<T, U, V> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V>(M3<T, U, V> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V>(M4<T, U, V> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V>(M5<T, U, V> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V>(M6<T, U, V> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V>(M7<T, U, V> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, W>(M<T, U, V, W> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, W>(M1<T, U, V, W> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, W>(M2<T, U, V, W> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, W>(M3<T, U, V, W> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, W>(M4<T, U, V, W> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M<T, U, V, W, X> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M1<T, U, V, W, X> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M2<T, U, V, W, X> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M3<T, U, V, W, X> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M4<T, U, V, W, X> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M5<T, U, V, W, X> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M<T, U, V, W, X, Y> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M1<T, U, V, W, X, Y> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M2<T, U, V, W, X, Y> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M3<T, U, V, W, X, Y> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M4<T, U, V, W, X, Y> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M5<T, U, V, W, X, Y> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M6<T, U, V, W, X, Y> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M<T, U, V, W, X, Y, Z> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M1<T, U, V, W, X, Y, Z> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M2<T, U, V, W, X, Y, Z> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M3<T, U, V, W, X, Y, Z> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M4<T, U, V, W, X, Y, Z> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M5<T, U, V, W, X, Y, Z> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M6<T, U, V, W, X, Y, Z> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M7<T, U, V, W, X, Y, Z> m) { PrivateSyncContext(m); }

        public void SyncContext<T>(F<T> m) { PrivateSyncContext(m); }
        public void SyncContext<T, R>(F<T, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, R>(F1<T, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, R>(F<T, U, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, R>(F1<T, U, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, R>(F2<T, U, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, R>(F3<T, U, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, R>(F<T, U, V, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, R>(F1<T, U, V, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, R>(F2<T, U, V, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, R>(F3<T, U, V, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, R>(F4<T, U, V, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, R>(F5<T, U, V, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, R>(F6<T, U, V, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, R>(F7<T, U, V, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, W, R>(F<T, U, V, W, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, W, R>(F1<T, U, V, W, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, W, R>(F2<T, U, V, W, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, W, R>(F3<T, U, V, W, R> m) { PrivateSyncContext(m); }
        public void SyncContext<T, U, V, W, R>(F4<T, U, V, W, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F<T, U, V, W, X, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F1<T, U, V, W, X, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F2<T, U, V, W, X, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F3<T, U, V, W, X, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F4<T, U, V, W, X, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F5<T, U, V, W, X, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F<T, U, V, W, X, Y, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F1<T, U, V, W, X, Y, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F2<T, U, V, W, X, Y, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F3<T, U, V, W, X, Y, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F4<T, U, V, W, X, Y, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F5<T, U, V, W, X, Y, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F6<T, U, V, W, X, Y, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F<T, U, V, W, X, Y, Z, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F1<T, U, V, W, X, Y, Z, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F2<T, U, V, W, X, Y, Z, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F3<T, U, V, W, X, Y, Z, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F4<T, U, V, W, X, Y, Z, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F5<T, U, V, W, X, Y, Z, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F6<T, U, V, W, X, Y, Z, R> m) { PrivateSyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F7<T, U, V, W, X, Y, Z, R> m) { PrivateSyncContext(m); }
        #endregion


        internal abstract bool Match(int count);

        internal abstract void Dequeue(int count, SyncMessage primaryMessage);

    }

}
