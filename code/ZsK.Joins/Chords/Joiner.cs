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


using System.Collections.Generic;
using System.Threading;

namespace ZsK.Joins {

    public class Joiner {

        public Joiner() {
            synchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Lock to serialize queuing, dequeuing and lookup operations
        /// Also used when adding a new chord
        /// </summary>
        private object lookupLock = new object();

        /// <summary>
        /// All the chords in this Joiner. Access is serialized with lookupLock
        /// </summary>
        private List<AsyncChord> allChords = new List<AsyncChord>();

        private Dictionary<AsyncMethod, List<Chord>> asyncMethodToChords = new Dictionary<AsyncMethod, List<Chord>>();
        private Dictionary<SyncMethod,  List<SyncChord>> syncMethodToChords  = new Dictionary<SyncMethod,  List<SyncChord>>();

        internal void AddChord(Chord chord) {
            // must lock as we allow chords to be added (removed?) dynamically
            lock (lookupLock) {
                foreach (Method method in chord.Methods) {
                    if (method is SyncMethod) {
                        List<SyncChord> chords;
                        if (!syncMethodToChords.TryGetValue(method as SyncMethod, out chords)) {
                            chords = new List<SyncChord>();
                            syncMethodToChords.Add(method as SyncMethod, chords);
                        }
                        chords.Add(chord as SyncChord);
                    } else {
                        List<Chord> chords;
                        if (!asyncMethodToChords.TryGetValue(method as AsyncMethod, out chords)) {
                            chords = new List<Chord>();
                            asyncMethodToChords.Add(method as AsyncMethod, chords);
                        }
                        chords.Add(chord);
                    }
                }
            }
        }

        private Chord AsyncLookup(AsyncMessage newMessage, ref SyncMessage primaryMessage) {
            //we are inside a lookupLock
            foreach (Chord chord in asyncMethodToChords[newMessage.AsyncMethod]) {
                if (chord.Match()) {
                    chord.Dequeue(ref primaryMessage, newMessage);
                    if (chord is SyncChord) {
                        //put some contextual info in the primary message
                        primaryMessage.IsPrimary = true;
                        primaryMessage.MatchingChord = chord as SyncChord;
                        return chord;
                    } else {
                        (chord as AsyncChord).ExecuteBody(newMessage.CollatedArguments.ToArray());
                    }
                }
            }
            return null;
        }


        internal void TryMatch(AsyncMessage newMessage) {
            Chord matchedChord;
            SyncMessage primaryMessage = null;
            lock (lookupLock) {
                newMessage.AsyncMethod.Enqueue(newMessage);
                matchedChord = AsyncLookup(newMessage, ref primaryMessage);
            }
            if (matchedChord == null) {
                return;
            }
            if (matchedChord is SyncChord) {
                primaryMessage.WakeUp();
            } else {
                (matchedChord as AsyncChord).ExecuteBody(newMessage.Arguments);
            }


        }


        /// <summary>
        /// Following the arrival of a sync message, find out whether we have a match. 
        /// Returns true and the matching primary method if match has been found.
        /// </summary>
        private bool SyncLookup(out SyncMessage primaryMessage, SyncMessage newMessage) {
            //we are inside a lookupLock
            primaryMessage = null;
            foreach (SyncChord syncChord in syncMethodToChords[newMessage.SyncMethod]) {
                if (syncChord.Match()) {
                    syncChord.Dequeue(ref primaryMessage, newMessage);
                    //put some contextual info in the primary message
                    primaryMessage.IsPrimary = true;
                    primaryMessage.MatchingChord = syncChord;
                    return true;
                }
            }
            return false;
        }



        /// <summary>
        /// Having receveived a sync message we will try to find a match.
        /// 
        /// If a match has been found and the message isn't the primary one we wait until we get a ReturnTo call
        /// 
        /// If no match has been found we wait until we are wakened up. It will be either because we are the 
        /// primary message or because we will have received a ReturnTo call.
        /// 
        /// This is only relevant for sync messages as async ones cannot wait.
        /// 
        /// </summary>
        internal object TryMatchAndWait(SyncMessage newMessage , out object[] arguments) {
            arguments = null;
            bool matchFound;
            SyncMessage primaryMessage;
            lock (lookupLock) {
                //TODO: the original version says not to queue this message... can't remember why...
                //if the arrival of this message triggers a match it will be dequeued anyway
                newMessage.SyncMethod.Enqueue(newMessage);
                
                matchFound = SyncLookup(out primaryMessage, newMessage);

                //TODO: would it be easier to have two lists of sync chords, one for single sync chords and another one for multiple ones?
            }
            //we can now leave our lock as the match and the dequeuing have been done

            if (matchFound) {
                // this message completes a chord
                // is newMessage the primary message?
                if (!newMessage.IsPrimary) {
                    // newMessage is an returned to non-primary message
                    // lets wake up the primary thread to tell it to do its job :)
                    primaryMessage.WakeUp();

                    //TODO: non primary methods which return void don't have to return via a Return call
                    //TODO: if no Return is called when required we must throw an exception.
                    // and wait until we get a return call
                    //TODO: do we want to timeout here???
                    newMessage.Wait();
                    //TODO: should we return now or when the main body has returned????
                    arguments = primaryMessage.OutArguments;
                    return newMessage.ReturnValue;
                }
                //here match found and we are primary
            } else {
                // if we have no match we must wait...
                // until someone signals a match (if we are primary) or until we time out 
                bool timedOut = newMessage.WaitWithTimeout();
                if (timedOut) {
                    //there can be a gap between the timeout and a successful match 
                    //we will catch this here
                    lock (lookupLock) {
                        if (!newMessage.IsAwake) {
                            //no match happened in between
                            //remove this message from the queue
                            newMessage.RemoveFromQueue();
                            //execute the code expected to be executed
                            //TODO: deal with exceptions
                            //TODO; could throw a timeout exception here... well, the action could do that...
                            return newMessage.OnTimeOut();
                        }
                    }
                }
                // here we have been wakened up either because newMessage is primary 
                // or because newMessage is non-primary and got a return call
                //TODO: deal with error of non primary not being wakened up by a return call

            }

            //got a match, find out whether we are the primary message or not.
            if (newMessage.IsPrimary) {
                // newMessage is an awakened or not primary message
                // tell all the other messages of this chord that this is the primary thread:
                newMessage.RegisterMessages(newMessage);
                var args = newMessage.CollatedArguments.ToArray();
                object returnValue = newMessage.MatchingChord.ExecuteBody(args);
                //check that all the return to have been called...
                newMessage.OutArguments = args;
                newMessage.CleanUpReturnCalls(args);

                newMessage.UnregisterMessages();
                arguments = args;
                return returnValue;
            } else {
                // newMessage is returned to non-primary message
                arguments = newMessage.PrimaryMessage.OutArguments;
                return newMessage.ReturnValue;
            }
        }

        //TODO: add loads of overloads

        public SyncMethod NewSyncMethod(M @delegate) { return new SyncMethod(this, @delegate);}

        public SyncMethod NewSyncMethod<T>(M<T> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T>(M1<T> @delegate) { return new SyncMethod(this, @delegate); }

        public SyncMethod NewSyncMethod<T, U>(M<T, U> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U>(M1<T, U> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U>(M2<T, U> @delegate) { return new SyncMethod(this, @delegate); }

        public SyncMethod NewSyncMethod<T, U, V>(M<T, U, V> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U, V>(M1<T, U, V> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U, V>(M5<T, U, V> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U, V>(M7<T, U, V> @delegate) { return new SyncMethod(this, @delegate); }

        public SyncMethod NewSyncMethod<T, U, V, W>(M<T, U, V, W> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U, V, W>(M1<T, U, V, W> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U, V, W>(M2<T, U, V, W> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U, V, W>(M3<T, U, V, W> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U, V, W>(M4<T, U, V, W> @delegate) { return new SyncMethod(this, @delegate); }

        //public SyncMethod NewSyncMethod<T, U, V, W, X>(M<T, U, V, W, X> @delegate) { return new SyncMethod(this, @delegate); }
        //public SyncMethod NewSyncMethod<T, U, V, W, X, Y>(M<T, U, V, W, X, Y> @delegate) { return new SyncMethod(this, @delegate); }
        //public SyncMethod NewSyncMethod<T, U, V, W, X, Y, Z>(M<T, U, V, W, X, Y, Z> @delegate) { return new SyncMethod(this, @delegate); }

        public SyncMethod NewSyncMethod<R>(F<R> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, R>(F<T, R> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U, R>(F<T, U, R> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U, V, R>(F<T, U, V, R> @delegate) { return new SyncMethod(this, @delegate); }
        public SyncMethod NewSyncMethod<T, U, V, W, R>(F<T, U, V, W, R> @delegate) { return new SyncMethod(this, @delegate); }
        //public SyncMethod NewSyncMethod<T, U, V, W, X, R>(F<T, U, V, W, X, R> @delegate) { return new SyncMethod(this, @delegate); }
        //public SyncMethod NewSyncMethod<T, U, V, W, X, Y, R>(F<T, U, V, W, X, Y, R> @delegate) { return new SyncMethod(this, @delegate); }
        //public SyncMethod NewSyncMethod<T, U, V, W, X, Y, Z, R>(F<T, U, V, W, X, Y, Z, R> @delegate) { return new SyncMethod(this, @delegate); }

        public AsyncMethod NewAsyncMethod(F<@async> @delegate) { return new AsyncMethod(this, @delegate); }
        public AsyncMethod NewAsyncMethod<T>(F<T, @async> @delegate) { return new AsyncMethod(this, @delegate); }
        public AsyncMethod NewAsyncMethod<T, U>(F<T, U, @async> @delegate) { return new AsyncMethod(this, @delegate); }
        public AsyncMethod NewAsyncMethod<T, U, V>(F<T, U, V, @async> @delegate) { return new AsyncMethod(this, @delegate); }
        public AsyncMethod NewAsyncMethod<T, U, V, W>(F<T, U, V, W, @async> @delegate) { return new AsyncMethod(this, @delegate); }
        //public AsyncMethod NewAsyncMethod<T, U, V, W, X>(F<T, U, V, W, X, async> @delegate) { return new AsyncMethod(this, @delegate); }
        //public AsyncMethod NewAsyncMethod<T, U, V, W, X, Y>(F<T, U, V, W, X, Y, async> @delegate) { return new AsyncMethod(this, @delegate); }
        //public AsyncMethod NewAsyncMethod<T, U, V, W, X, Y, Z>(F<T, U, V, W, X, Y, Z, async> @delegate) { return new AsyncMethod(this, @delegate); }


        private SynchronizationContext synchronizationContext;
        public SynchronizationContext SynchronizationContext {
            get { return synchronizationContext; }
            set { synchronizationContext = value; }
        }

    }
}
