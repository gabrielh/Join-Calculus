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
using System.Diagnostics;
using System.Linq;
using System.Threading;

using ZsK.Joins.Utilities;

namespace ZsK.Joins {

    internal sealed class SyncMessage : Message {

        internal SyncMessage(SyncMethod syncMethod, object[] allArguments) {
            SyncMethod = syncMethod;
            AllArguments = allArguments;

            //todo check that the arguments match the delegate in the method

            //VerifyArguments();
            //could check that the name of the caller parent matches the name of syncmethod's argument
                //i.e. match the S2 in "s2 = joiner.NewSyncMethod<int>(S2);" 
                //  and the S2 in "private void S2(int i) { s2.Invoked(i); }"
        }

        /// <summary>
        /// The sync method associated to this message.
        /// </summary>
        internal readonly SyncMethod SyncMethod;

        [Conditional("DEBUG")]
        private void VerifyArguments() {
            Debug.WriteLine(SyncMethod.DelegateArgumentTypes.ToString2());
            Debug.WriteLine(AllArguments.ToString2());
            if (!SyncMethod.DelegateArgumentTypes.SequenceEqual(AllArguments.Select(o => o.GetType()))) {
                //TODO: improve the exception message
                throw new Exception("Arguments don't match the signature of the method");
            }
        }



        /// <summary>
        /// The value returned to the caller be a ReturnTo.
        /// This is set in the 
        /// </summary>
        internal object ReturnValue;

        /// <summary>
        /// The value returned to the caller be a ReturnTo.
        /// This is set in the 
        /// </summary>
        internal bool ReturnValueSet = false;

        /// <summary>
        /// This is set when a match is found
        /// </summary>
        internal bool IsPrimary;

        /// <summary>
        /// Set after a match has been found
        /// </summary>
        internal SyncChord MatchingChord;

        /// <summary>
        /// 
        /// </summary>
        internal object[] AllArguments;

        /// <summary>
        /// Position of the arguments of this message in the array of objects passed to the body of the chord.
        /// </summary>
        internal int ArgumentIndex;

        /// <summary>
        /// Array index from which to obtain the out value in the array of objects passed to the body of the chord.
        /// </summary>
        internal int ArgumentSubIndex = -1;

        /// <summary>
        /// Set after a match has been found
        /// </summary>
        internal object[] OutArguments;


        /// <summary>
        /// This message has timed out, remove from queue
        /// </summary>
        internal void RemoveFromQueue() {
            throw new NotImplementedException();
        }

        private ManualResetEvent @event = new ManualResetEvent(false);

        /// <summary>
        /// Execute the delegate on time out.
        /// </summary>
        /// <returns></returns>
        internal object OnTimeOut() {
            throw new NotImplementedException();
        }

        internal void WakeUp() {
            exceptionToThrowOnWake = null;
            @event.Set();
        }

        private Exception exceptionToThrowOnWake;
        internal void WakeUpAndThrow(Exception exception) {
            exceptionToThrowOnWake = exception;
            @event.Set();
        }

        /// <summary>
        /// Wait until we get the return call
        /// </summary>
        /// <returns>True if not timed out</returns>
        internal bool Wait() {
            //TODO: find out about exitContext argument
            return @event.WaitOne(-1, false);
        }

        /// <summary>
        /// Wait until someone signals a match which includes this message or timeout
        /// </summary>
        /// <returns>True if not timed out</returns>
        internal bool WaitWithTimeout() {
            //TODO: find out about exitContext argument
            //TODO: add here some information somewhere (!!!) so that when the chord blocks, the person debugging it can understand better what is missing to complete the chord.
            return @event.WaitOne(SyncMethod.Timeout, false);
        }

        /// <summary>
        /// 
        /// </summary>
        internal bool IsAwake {
            get { 
                // this return true if the event has been signaled
                return @event.WaitOne(0, false); 
            }
        }


        /// <summary>
        /// If this message is a primary message, we keep a list of all the messages 
        /// which will have to be returned to..., i.e. all the syncMessages in a matched chord.
        /// </summary>
        internal List<SyncMessage> ReturnToSyncMessages = new List<SyncMessage>();




        internal void RegisterMessages(SyncMessage primaryMessage) {
            foreach (SyncMessage m in ReturnToSyncMessages) {
                m.SyncMethod.Register(m);
            }
        }

        internal void UnregisterMessages() {
            foreach (SyncMessage m in ReturnToSyncMessages) {
                m.SyncMethod.Unregister(m);
            }
        }

        internal void CleanUpReturnCalls(object[] arguments) {
            foreach (SyncMessage m in ReturnToSyncMessages) {
                m.SyncMethod.CleanUpReturnCalls(arguments);
            }
        }
    }
}
