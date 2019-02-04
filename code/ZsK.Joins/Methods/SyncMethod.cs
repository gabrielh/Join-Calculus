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
using System.Threading;

namespace ZsK.Joins {

    public sealed partial class SyncMethod : Method {
        internal SyncMethod() { }

        internal SyncMethod(Joiner joiner, Delegate @delegate) {
            this.joiner = joiner;
            name = @delegate.Method.Name;
            this.@delegate = @delegate;
            returnsVoid = @delegate.Method.ReturnType == typeof(void);
        }


        private object queueLock = new object();
        private List<SyncMessage> messageQueue = new List<SyncMessage>();

        internal override void Dequeue(int count, SyncMessage primaryMessage) {
            //TODO: deal with the arguments and the returns... and count
            lock (queueLock) {
                if (count == 1) {
                    messageQueue[0].ArgumentIndex = primaryMessage.CollatedArguments.Count;
                    primaryMessage.CollatedArguments.AddRange(messageQueue[0].AllArguments);
                    primaryMessage.ReturnToSyncMessages.Add(messageQueue[0]);
                    messageQueue[0].PrimaryMessage = primaryMessage;
                    messageQueue.RemoveAt(0);
                } else {
                    var dequeuedMessages = messageQueue.Take(count);
                    foreach (var m in dequeuedMessages) {
                        m.PrimaryMessage = primaryMessage;
                    }
                    if (DelegateArgumentTypes.Count() != 0) {
                        var arrayOfArguments = dequeuedMessages.Select((m,i) => {
                            m.ArgumentIndex = primaryMessage.CollatedArguments.Count;
                            m.ArgumentSubIndex = i;
                            return m.AllArguments;
                        }).ToArray();
                        primaryMessage.CollatedArguments.AddRange(TransposeAndType(arrayOfArguments));
                    }
                    primaryMessage.ReturnToSyncMessages.AddRange(dequeuedMessages);
                    messageQueue.RemoveRange(0, count);

                }
            }
        }

        internal void Enqueue(SyncMessage syncMessage) {
            lock (queueLock) {
                messageQueue.Add(syncMessage);
            }
        }

        internal SyncMessage Top {
            get {
                lock (queueLock) {
                    return messageQueue.First();
                }
            }
        }

        internal override bool Match(int count) {
            lock (queueLock) {
                return messageQueue.Count >= count;
            }
        }


        //TODO: implement this.
        public void BeginInvoke<T>(F<T> continuation) {

        }

        internal int Timeout = -1;

        private readonly bool returnsVoid;
        private Dictionary<Thread, Queue<SyncMessage>> threadToSyncMessage = new Dictionary<Thread, Queue<SyncMessage>>();
        private object threadToSyncMessageLock = new object();

        /// <summary>
        /// Associate the execution of the primary thread with the methods of the other non-primary messages.
        /// So when a ReturnTo call comes on the method (i.e. here) we can find in our dictionary (which maps threads to 
        /// messages) the correct message and pass to it the returned value. 
        /// 
        /// Note that we can have multiple messages for a given method.
        /// </summary>
        /// <param name="syncMessage"></param>
        internal void Register(SyncMessage syncMessage) {
            lock (threadToSyncMessageLock) {
                Queue<SyncMessage> queue;
                if (!threadToSyncMessage.TryGetValue(Thread.CurrentThread, out queue)) {
                    queue = new Queue<SyncMessage>();
                    threadToSyncMessage.Add(Thread.CurrentThread, queue);
                }
                queue.Enqueue(syncMessage);
            }
        }

        internal void Unregister(SyncMessage syncMessage) {
            //TODO: error "handling"???
            lock (threadToSyncMessageLock) {
                if (threadToSyncMessage.ContainsKey(Thread.CurrentThread)) {
                    threadToSyncMessage.Remove(Thread.CurrentThread);
                }
            }
        }

        internal void CleanUpReturnCalls(object[] arguments) {
            //TODO: error "handling"
            //unblock thread if returns void
            //throw if returns something else than void
            lock (threadToSyncMessageLock) {
                if (threadToSyncMessage.ContainsKey(Thread.CurrentThread)) {
                    if (returnsVoid) {
                        foreach (var syncMessage in threadToSyncMessage[Thread.CurrentThread]) {
                            syncMessage.WakeUp();
                        }
                    } else {
                        foreach (var syncMessage in threadToSyncMessage[Thread.CurrentThread]) {
                            syncMessage.WakeUpAndThrow(new Exception(
                                String.Format("Return on synchronous method {0} not called.", this.@delegate.Method.Name)));
                        }
                    }
                    threadToSyncMessage.Remove(Thread.CurrentThread);
                }
            }
        }

        ////TODO: verify overload rules for these methods

        public void Return(object returnedValue) {
            //TODO: error "handling"
           var message = threadToSyncMessage[Thread.CurrentThread].First(m => !m.ReturnValueSet);
           message.ReturnValue = returnedValue;
           message.ReturnValueSet = true;
            if (returnsVoid) {
                //TODO: error "handling"
                throw new Exception();
            }
        }
    }
}
