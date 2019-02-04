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

    /// <summary>
    /// Chrods are made of a list of JoinMethod, one instance of a JoinMethod per Method instance.
    /// 
    /// For example in 
    ///     A & B & A
    /// and 
    ///     A & D 
    /// all A reference the same Method, but in the chord we will have different instances of Chordmethod each containing the 
    /// number of instances of each method.
    /// 
    /// This could be replaced by a dictionary of methods versus multiples.
    /// 
    /// We want to be able to distinguish 
    /// 
    ///    A & 2*A 
    ///    
    /// from 
    ///
    ///    3*A 
    ///    
    /// this is necessary as the list of argument will be different:
    ///   
    ///    A & 2*A => (A a, A[] as)
    ///    3*A => (A[] as)
    /// 
    /// We will need two lists, one for matching (total multiples) and one for building the argument list...
    /// 
    /// </summary>
    public abstract class Chord {

        protected Chord() { }

        protected Chord(Chord chord) {
            name = chord.name;
            methodToMultiplier = chord.methodToMultiplier;
        }
        protected Type returnType;

        internal void AddMethod(MethodBase method, int multiplier) {
            //TODO: check this delegate isn't being added twice...
            //TODO: check no sync method is added if first method is async

            if ((methods.Count > 0) && (methods.First().Key is AsyncMethod) && (method is SyncMethod)) {
                throw new CannotAddSynchMethodToAsyncChord("Sync chords must start with a sync method.");
            }

            // the first one added determines the return type of the chord body
            if (methodToMultiplier.Count == 0) {
                returnType = method.ReturnType;
            }
            if (methodToMultiplier.ContainsKey(method)) {
                //update the multipler
                methodToMultiplier[method] = methodToMultiplier[method] + multiplier;
            } else {
                methodToMultiplier.Add(method, multiplier);
            }
            methods.Add(new KeyValuePair<MethodBase, int>(method, multiplier));
        }

        /// <summary>
        /// Total multipliers, these are used to determine whether we have a match
        /// </summary>
        protected Dictionary<MethodBase, int> methodToMultiplier = new Dictionary<MethodBase, int>();

        /// <summary>
        /// Partial multipliers, this is an ordered list. All the arguments of the chord body must be in the same order
        /// </summary>]
        protected List<KeyValuePair<MethodBase, int>> methods = new List<KeyValuePair<MethodBase, int>>();

        internal IEnumerable<MethodBase> Methods {
            get {
                return methodToMultiplier.Keys;
            }
        }

        private string name;

        //TODO: default name to the chord definition, trivial to do

        internal Chord SetName(string name) {
            this.name = name;
            return this;
        }


        //TODO: enforce some of the constraint between ThreadingType and the type of the chord
        internal ThreadingType threadingType = ThreadingType.Undefined;

        internal ThreadingType ThreadingType {
            set {
                threadingType = value;
            }
        }

        protected Delegate @delegate;

        private string ChordSignature() {
            string arguments = string.Join(",", ChordTypes().Select(t => t.Name).ToArray());
            string returnType;
            if (this.returnType == null || this.returnType == typeof(@async) || this.returnType == typeof(void))
            {
                returnType = "void ";
            } else {
                returnType = this.returnType.Name;
            }
            return returnType + "(" + arguments + ")";
        }

        private List<Type> ChordTypes() {
            var chordTypes = new List<Type>();
            foreach (var pair in methods) {
                if (pair.Value == 1) {
                    chordTypes.AddRange(pair.Key.DelegateArgumentTypes);
                } else {
                    foreach (var t in pair.Key.DelegateArgumentTypes) {
                        if (t.IsByRef) {
                            chordTypes.Add(t.Assembly.GetType(t.FullName.Replace("&", "")).MakeArrayType().MakeByRefType());
                        } else {
                            chordTypes.Add(t.MakeArrayType());
                        }
                    }
                }
            }
            return chordTypes;
    }
        private void VerifyDelegate() {
            //verify that list of arguments of each method matches the passed delegate arguments
            //TODO: most of this logic should go elsewhere...
            var bodyArgTypes = @delegate.Method.GetParameters().Select(pi => pi.ParameterType);
            if (ChordTypes().Count() != bodyArgTypes.Count()) {
                throw new SignatureMismatchException(String.Format(
                    "Arguments count mismatch, body arguments and method arguments don't match in chord {0}, expected signature is {1}.", 
                    name,
                    ChordSignature()));
            }
            if (!ChordTypes().SequenceEqual(bodyArgTypes)) {
                throw new SignatureMismatchException(String.Format(
                    "Body arguments and method arguments don't match in chord {0}, expected signature is {1}.",
                    name,
                    ChordSignature()));
            }
            if (this is SyncChord) {
                if (returnType != @delegate.Method.ReturnType) {
                    throw new SignatureMismatchException(String.Format(
                        "Return type of body doesn't match return type of primary method in chord {0}, expected signature is {1}.",
                        name,
                        ChordSignature()));
                }
            } else {
                if (@delegate.Method.ReturnType != typeof(void)) {
                    throw new SignatureMismatchException(String.Format(
                        "Return type of body of asynchronous chord must be void, chord {0}.",
                        name));
                }
            }
        }

        internal void PrivatePool(Delegate d) {
            //TODO: only valid for async chords mChord is AsyncChord...
            threadingType = ThreadingType.Pool;
            @delegate = d;
            VerifyDelegate();
            if (!(this is AsyncChord)) {
                throw new IncompatibleSchedulingException("Only asynchronous chords can be executed in the thread pool.");
            }
        }

        #region Pool overloads
        public void Pool(M m) { PrivatePool(m); }
        public void Pool<T>(M<T> m) { PrivatePool(m); }
        public void Pool<T>(M1<T> m) { PrivatePool(m); }
        public void Pool<T, U>(M<T, U> m) { PrivatePool(m); }
        public void Pool<T, U>(M1<T, U> m) { PrivatePool(m); }
        public void Pool<T, U>(M2<T, U> m) { PrivatePool(m); }
        //public void Pool<T, U>(M3<T, U> m) { PrivatePool(m); }
        public void Pool<T, U, V>(M<T, U, V> m) { PrivatePool(m); }
        public void Pool<T, U, V>(M1<T, U, V> m) { PrivatePool(m); }
        public void Pool<T, U, V>(M2<T, U, V> m) { PrivatePool(m); }
        public void Pool<T, U, V>(M3<T, U, V> m) { PrivatePool(m); }
        //public void Pool<T, U, V>(M4<T, U, V> m) { PrivatePool(m); }
        //public void Pool<T, U, V>(M5<T, U, V> m) { PrivatePool(m); }
        //public void Pool<T, U, V>(M6<T, U, V> m) { PrivatePool(m); }
        //public void Pool<T, U, V>(M7<T, U, V> m) { PrivatePool(m); }
        public void Pool<T, U, V, W>(M<T, U, V, W> m) { PrivatePool(m); }
        public void Pool<T, U, V, W>(M1<T, U, V, W> m) { PrivatePool(m); }
        public void Pool<T, U, V, W>(M2<T, U, V, W> m) { PrivatePool(m); }
        public void Pool<T, U, V, W>(M3<T, U, V, W> m) { PrivatePool(m); }
        public void Pool<T, U, V, W>(M4<T, U, V, W> m) { PrivatePool(m); }
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

        public void Pool<T>(F<T> m) { PrivatePool(m); }
        public void Pool<T, R>(F<T, R> m) { PrivatePool(m); }
        public void Pool<T, R>(F1<T, R> m) { PrivatePool(m); }
        public void Pool<T, U, R>(F<T, U, R> m) { PrivatePool(m); }
        public void Pool<T, U, R>(F1<T, U, R> m) { PrivatePool(m); }
        public void Pool<T, U, R>(F2<T, U, R> m) { PrivatePool(m); }
        //public void Pool<T, U, R>(F3<T, U, R> m) { PrivatePool(m); }
        public void Pool<T, U, V, R>(F<T, U, V, R> m) { PrivatePool(m); }
        public void Pool<T, U, V, R>(F1<T, U, V, R> m) { PrivatePool(m); }
        public void Pool<T, U, V, R>(F2<T, U, V, R> m) { PrivatePool(m); }
        public void Pool<T, U, V, R>(F3<T, U, V, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, R>(F4<T, U, V, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, R>(F5<T, U, V, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, R>(F6<T, U, V, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, R>(F7<T, U, V, R> m) { PrivatePool(m); }
        public void Pool<T, U, V, W, R>(F<T, U, V, W, R> m) { PrivatePool(m); }
        public void Pool<T, U, V, W, R>(F1<T, U, V, W, R> m) { PrivatePool(m); }
        public void Pool<T, U, V, W, R>(F2<T, U, V, W, R> m) { PrivatePool(m); }
        public void Pool<T, U, V, W, R>(F3<T, U, V, W, R> m) { PrivatePool(m); }
        public void Pool<T, U, V, W, R>(F4<T, U, V, W, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, R>(F<T, U, V, W, X, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, R>(F1<T, U, V, W, X, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, R>(F2<T, U, V, W, X, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, R>(F3<T, U, V, W, X, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, R>(F4<T, U, V, W, X, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, R>(F5<T, U, V, W, X, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, R>(F<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, R>(F1<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, R>(F2<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, R>(F3<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, R>(F4<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, R>(F5<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, R>(F6<T, U, V, W, X, Y, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z, R>(F<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z, R>(F1<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z, R>(F2<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z, R>(F3<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z, R>(F4<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z, R>(F5<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z, R>(F6<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        //public void Pool<T, U, V, W, X, Y, Z, R>(F7<T, U, V, W, X, Y, Z, R> m) { PrivatePool(m); }
        #endregion

        internal void PrivateSpawn(Delegate d) {
            //TODO: only valid for async chords mChord is AsyncChord...
            threadingType = ThreadingType.Spawn;
            @delegate = d;
            VerifyDelegate();
            if (!(this is AsyncChord)) {
                throw new IncompatibleSchedulingException("Only asynchronous chords can be spawned.");
            }
        }

        #region Spawn overloads
        public void PrivateSpawn(M m) { PrivateSpawn(m); }
        public void Spawn<T>(M<T> m) { PrivateSpawn(m); }
        public void Spawn<T>(M1<T> m) { PrivateSpawn(m); }
        public void Spawn<T, U>(M<T, U> m) { PrivateSpawn(m); }
        public void Spawn<T, U>(M1<T, U> m) { PrivateSpawn(m); }
        public void Spawn<T, U>(M2<T, U> m) { PrivateSpawn(m); }
        //public void Spawn<T, U>(M3<T, U> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V>(M<T, U, V> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V>(M1<T, U, V> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V>(M2<T, U, V> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V>(M3<T, U, V> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V>(M4<T, U, V> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V>(M5<T, U, V> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V>(M6<T, U, V> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V>(M7<T, U, V> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W>(M<T, U, V, W> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W>(M1<T, U, V, W> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W>(M2<T, U, V, W> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W>(M3<T, U, V, W> m) { PrivateSpawn(m); }
        public void Spawn<T, U, V, W>(M4<T, U, V, W> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X>(M<T, U, V, W, X> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X>(M1<T, U, V, W, X> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X>(M2<T, U, V, W, X> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X>(M3<T, U, V, W, X> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X>(M4<T, U, V, W, X> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X>(M5<T, U, V, W, X> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y>(M<T, U, V, W, X, Y> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y>(M1<T, U, V, W, X, Y> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y>(M2<T, U, V, W, X, Y> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y>(M3<T, U, V, W, X, Y> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y>(M4<T, U, V, W, X, Y> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y>(M5<T, U, V, W, X, Y> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y>(M6<T, U, V, W, X, Y> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z>(M<T, U, V, W, X, Y, Z> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z>(M1<T, U, V, W, X, Y, Z> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z>(M2<T, U, V, W, X, Y, Z> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z>(M3<T, U, V, W, X, Y, Z> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z>(M4<T, U, V, W, X, Y, Z> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z>(M5<T, U, V, W, X, Y, Z> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z>(M6<T, U, V, W, X, Y, Z> m) { PrivateSpawn(m); }
        //public void Spawn<T, U, V, W, X, Y, Z>(M7<T, U, V, W, X, Y, Z> m) { PrivateSpawn(m); }

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

        #endregion


        internal void PrivateContinue(Delegate d) {
            //TODO: only valid for async chords mChord is AsyncChord...
            threadingType = ThreadingType.Continue;
            @delegate = d;
            VerifyDelegate();
            if (!(this is AsyncChord)) {
                throw new IncompatibleSchedulingException("Use \"Do\" to run an sync chord on the thread of the primary method.");
            }
        }

        #region Continue overloads
        public void Continue(M m) { PrivateContinue(m); }
        public void Continue<T>(M<T> m) { PrivateContinue(m); }
        public void Continue<T>(M1<T> m) { PrivateContinue(m); }
        public void Continue<T, U>(M<T, U> m) { PrivateContinue(m); }
        public void Continue<T, U>(M1<T, U> m) { PrivateContinue(m); }
        public void Continue<T, U>(M2<T, U> m) { PrivateContinue(m); }
        //public void Continue<T, U>(M3<T, U> m) { PrivateContinue(m); }
        public void Continue<T, U, V>(M<T, U, V> m) { PrivateContinue(m); }
        public void Continue<T, U, V>(M1<T, U, V> m) { PrivateContinue(m); }
        public void Continue<T, U, V>(M2<T, U, V> m) { PrivateContinue(m); }
        public void Continue<T, U, V>(M3<T, U, V> m) { PrivateContinue(m); }
        //public void Continue<T, U, V>(M4<T, U, V> m) { PrivateContinue(m); }
        //public void Continue<T, U, V>(M5<T, U, V> m) { PrivateContinue(m); }
        //public void Continue<T, U, V>(M6<T, U, V> m) { PrivateContinue(m); }
        //public void Continue<T, U, V>(M7<T, U, V> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W>(M<T, U, V, W> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W>(M1<T, U, V, W> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W>(M2<T, U, V, W> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W>(M3<T, U, V, W> m) { PrivateContinue(m); }
        public void Continue<T, U, V, W>(M4<T, U, V, W> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X>(M<T, U, V, W, X> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X>(M1<T, U, V, W, X> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X>(M2<T, U, V, W, X> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X>(M3<T, U, V, W, X> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X>(M4<T, U, V, W, X> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X>(M5<T, U, V, W, X> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M1<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M2<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M3<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M4<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M5<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y>(M6<T, U, V, W, X, Y> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M1<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M2<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M3<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M4<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M5<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M6<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }
        //public void Continue<T, U, V, W, X, Y, Z>(M7<T, U, V, W, X, Y, Z> m) { PrivateContinue(m); }

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

        #endregion


        internal void PrivateDo(Delegate d) {
            threadingType = ThreadingType.Sync;
            @delegate = d;
            VerifyDelegate();
            if (!(this is SyncChord)) {
                throw new IncompatibleSchedulingException("Use \"Continue\" to run an async chord on the incoming thread.");
            }
        }

        #region Do overloads
        public void Do(M m) { PrivateDo(m); }
        public void Do<T>(M<T> m) { PrivateDo(m); }
        public void Do<T>(M1<T> m) { PrivateDo(m); }
        public void Do<T, U>(M<T, U> m) { PrivateDo(m); }
        public void Do<T, U>(M1<T, U> m) { PrivateDo(m); }
        public void Do<T, U>(M2<T, U> m) { PrivateDo(m); }
        //public void Do<T, U>(M3<T, U> m) { PrivateDo(m); }
        public void Do<T, U, V>(M<T, U, V> m) { PrivateDo(m); }
        public void Do<T, U, V>(M1<T, U, V> m) { PrivateDo(m); }
        public void Do<T, U, V>(M2<T, U, V> m) { PrivateDo(m); }
        public void Do<T, U, V>(M3<T, U, V> m) { PrivateDo(m); }
        //public void Do<T, U, V>(M4<T, U, V> m) { PrivateDo(m); }
        //public void Do<T, U, V>(M5<T, U, V> m) { PrivateDo(m); }
        //public void Do<T, U, V>(M6<T, U, V> m) { PrivateDo(m); }
        //public void Do<T, U, V>(M7<T, U, V> m) { PrivateDo(m); }
        public void Do<T, U, V, W>(M<T, U, V, W> m) { PrivateDo(m); }
        public void Do<T, U, V, W>(M1<T, U, V, W> m) { PrivateDo(m); }
        public void Do<T, U, V, W>(M2<T, U, V, W> m) { PrivateDo(m); }
        public void Do<T, U, V, W>(M3<T, U, V, W> m) { PrivateDo(m); }
        public void Do<T, U, V, W>(M4<T, U, V, W> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X>(M<T, U, V, W, X> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X>(M1<T, U, V, W, X> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X>(M2<T, U, V, W, X> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X>(M3<T, U, V, W, X> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X>(M4<T, U, V, W, X> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X>(M5<T, U, V, W, X> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y>(M<T, U, V, W, X, Y> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y>(M1<T, U, V, W, X, Y> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y>(M2<T, U, V, W, X, Y> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y>(M3<T, U, V, W, X, Y> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y>(M4<T, U, V, W, X, Y> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y>(M5<T, U, V, W, X, Y> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y>(M6<T, U, V, W, X, Y> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z>(M<T, U, V, W, X, Y, Z> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z>(M1<T, U, V, W, X, Y, Z> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z>(M2<T, U, V, W, X, Y, Z> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z>(M3<T, U, V, W, X, Y, Z> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z>(M4<T, U, V, W, X, Y, Z> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z>(M5<T, U, V, W, X, Y, Z> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z>(M6<T, U, V, W, X, Y, Z> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z>(M7<T, U, V, W, X, Y, Z> m) { PrivateDo(m); }

        public void Do<T>(F<T> m) { PrivateDo(m); }
        public void Do<T, R>(F<T, R> m) { PrivateDo(m); }
        public void Do<T, R>(F1<T, R> m) { PrivateDo(m); }
        public void Do<T, U, R>(F<T, U, R> m) { PrivateDo(m); }
        public void Do<T, U, R>(F1<T, U, R> m) { PrivateDo(m); }
        public void Do<T, U, R>(F2<T, U, R> m) { PrivateDo(m); }
        //public void Do<T, U, R>(F3<T, U, R> m) { PrivateDo(m); }
        public void Do<T, U, V, R>(F<T, U, V, R> m) { PrivateDo(m); }
        public void Do<T, U, V, R>(F1<T, U, V, R> m) { PrivateDo(m); }
        public void Do<T, U, V, R>(F2<T, U, V, R> m) { PrivateDo(m); }
        public void Do<T, U, V, R>(F3<T, U, V, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, R>(F4<T, U, V, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, R>(F5<T, U, V, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, R>(F6<T, U, V, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, R>(F7<T, U, V, R> m) { PrivateDo(m); }
        public void Do<T, U, V, W, R>(F<T, U, V, W, R> m) { PrivateDo(m); }
        public void Do<T, U, V, W, R>(F1<T, U, V, W, R> m) { PrivateDo(m); }
        public void Do<T, U, V, W, R>(F2<T, U, V, W, R> m) { PrivateDo(m); }
        public void Do<T, U, V, W, R>(F3<T, U, V, W, R> m) { PrivateDo(m); }
        public void Do<T, U, V, W, R>(F4<T, U, V, W, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, R>(F<T, U, V, W, X, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, R>(F1<T, U, V, W, X, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, R>(F2<T, U, V, W, X, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, R>(F3<T, U, V, W, X, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, R>(F4<T, U, V, W, X, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, R>(F5<T, U, V, W, X, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F1<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F2<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F3<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F4<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F5<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, R>(F6<T, U, V, W, X, Y, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F1<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F2<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F3<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F4<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F5<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F6<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        //public void Do<T, U, V, W, X, Y, Z, R>(F7<T, U, V, W, X, Y, Z, R> m) { PrivateDo(m); }
        #endregion



        internal void SyncContext(Delegate d) {
            threadingType = ThreadingType.SyncContext;
            @delegate = d;
            VerifyDelegate();
            //TODO: more tests here... see above
        }

        #region SyncContext overloads
        public void SyncContext(M m) { SyncContext(m); }
        public void SyncContext<T>(M<T> m) { SyncContext(m); }
        public void SyncContext<T>(M1<T> m) { SyncContext(m); }
        public void SyncContext<T, U>(M<T, U> m) { SyncContext(m); }
        public void SyncContext<T, U>(M1<T, U> m) { SyncContext(m); }
        public void SyncContext<T, U>(M2<T, U> m) { SyncContext(m); }
        //public void SyncContext<T, U>(M3<T, U> m) { SyncContext(m); }
        public void SyncContext<T, U, V>(M<T, U, V> m) { SyncContext(m); }
        public void SyncContext<T, U, V>(M1<T, U, V> m) { SyncContext(m); }
        public void SyncContext<T, U, V>(M2<T, U, V> m) { SyncContext(m); }
        public void SyncContext<T, U, V>(M3<T, U, V> m) { SyncContext(m); }
        //public void SyncContext<T, U, V>(M4<T, U, V> m) { SyncContext(m); }
        //public void SyncContext<T, U, V>(M5<T, U, V> m) { SyncContext(m); }
        //public void SyncContext<T, U, V>(M6<T, U, V> m) { SyncContext(m); }
        //public void SyncContext<T, U, V>(M7<T, U, V> m) { SyncContext(m); }
        public void SyncContext<T, U, V, W>(M<T, U, V, W> m) { SyncContext(m); }
        public void SyncContext<T, U, V, W>(M1<T, U, V, W> m) { SyncContext(m); }
        public void SyncContext<T, U, V, W>(M2<T, U, V, W> m) { SyncContext(m); }
        public void SyncContext<T, U, V, W>(M3<T, U, V, W> m) { SyncContext(m); }
        public void SyncContext<T, U, V, W>(M4<T, U, V, W> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M<T, U, V, W, X> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M1<T, U, V, W, X> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M2<T, U, V, W, X> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M3<T, U, V, W, X> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M4<T, U, V, W, X> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X>(M5<T, U, V, W, X> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M<T, U, V, W, X, Y> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M1<T, U, V, W, X, Y> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M2<T, U, V, W, X, Y> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M3<T, U, V, W, X, Y> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M4<T, U, V, W, X, Y> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M5<T, U, V, W, X, Y> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y>(M6<T, U, V, W, X, Y> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M<T, U, V, W, X, Y, Z> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M1<T, U, V, W, X, Y, Z> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M2<T, U, V, W, X, Y, Z> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M3<T, U, V, W, X, Y, Z> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M4<T, U, V, W, X, Y, Z> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M5<T, U, V, W, X, Y, Z> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M6<T, U, V, W, X, Y, Z> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z>(M7<T, U, V, W, X, Y, Z> m) { SyncContext(m); }

        public void SyncContext<T>(F<T> m) { SyncContext(m); }
        public void SyncContext<T, R>(F<T, R> m) { SyncContext(m); }
        public void SyncContext<T, R>(F1<T, R> m) { SyncContext(m); }
        public void SyncContext<T, U, R>(F<T, U, R> m) { SyncContext(m); }
        public void SyncContext<T, U, R>(F1<T, U, R> m) { SyncContext(m); }
        public void SyncContext<T, U, R>(F2<T, U, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, R>(F3<T, U, R> m) { SyncContext(m); }
        public void SyncContext<T, U, V, R>(F<T, U, V, R> m) { SyncContext(m); }
        public void SyncContext<T, U, V, R>(F1<T, U, V, R> m) { SyncContext(m); }
        public void SyncContext<T, U, V, R>(F2<T, U, V, R> m) { SyncContext(m); }
        public void SyncContext<T, U, V, R>(F3<T, U, V, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, R>(F4<T, U, V, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, R>(F5<T, U, V, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, R>(F6<T, U, V, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, R>(F7<T, U, V, R> m) { SyncContext(m); }
        public void SyncContext<T, U, V, W, R>(F<T, U, V, W, R> m) { SyncContext(m); }
        public void SyncContext<T, U, V, W, R>(F1<T, U, V, W, R> m) { SyncContext(m); }
        public void SyncContext<T, U, V, W, R>(F2<T, U, V, W, R> m) { SyncContext(m); }
        public void SyncContext<T, U, V, W, R>(F3<T, U, V, W, R> m) { SyncContext(m); }
        public void SyncContext<T, U, V, W, R>(F4<T, U, V, W, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F<T, U, V, W, X, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F1<T, U, V, W, X, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F2<T, U, V, W, X, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F3<T, U, V, W, X, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F4<T, U, V, W, X, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, R>(F5<T, U, V, W, X, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F<T, U, V, W, X, Y, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F1<T, U, V, W, X, Y, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F2<T, U, V, W, X, Y, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F3<T, U, V, W, X, Y, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F4<T, U, V, W, X, Y, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F5<T, U, V, W, X, Y, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, R>(F6<T, U, V, W, X, Y, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F<T, U, V, W, X, Y, Z, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F1<T, U, V, W, X, Y, Z, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F2<T, U, V, W, X, Y, Z, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F3<T, U, V, W, X, Y, Z, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F4<T, U, V, W, X, Y, Z, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F5<T, U, V, W, X, Y, Z, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F6<T, U, V, W, X, Y, Z, R> m) { SyncContext(m); }
        //public void SyncContext<T, U, V, W, X, Y, Z, R>(F7<T, U, V, W, X, Y, Z, R> m) { SyncContext(m); }
        #endregion


        public override string ToString() {
            string result = name + "("+ threadingType +") : ";
            string and = "";
            foreach (var pair in methods) {
                if (pair.Value == 1) {
                    result += and + pair.Key.Name;
                } else {
                    result += and + pair.Value + "*" + pair.Key.Name;
                }
                and = " & ";
            }
            return result;
        }

        internal abstract bool Match();
        internal abstract void Dequeue(ref SyncMessage primaryMessage, Message newMessage);
    }
}
