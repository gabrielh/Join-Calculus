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

    public sealed class AsyncMethod : Method {
        internal AsyncMethod(Joiner joiner, Delegate @delegate) {
            this.joiner = joiner;
            name = @delegate.Method.Name;
            this.@delegate = @delegate;
            //TODO: verify it returns async and no arguments is passed by reference
        }

        private object queueLock = new object();
        private List<AsyncMessage> messageQueue = new List<AsyncMessage>();

        internal void Enqueue(AsyncMessage asyncMessage) {
            lock (queueLock) {
                messageQueue.Add(asyncMessage);
            }
        }

        internal override void Dequeue(int count, SyncMessage primaryMessage) {
            Dequeue(count, primaryMessage.CollatedArguments);
        }

        internal void Dequeue(int count, List<object> arguments) {
            lock (queueLock) {
                if (count == 1) {
                    arguments.AddRange(messageQueue[0].Arguments);
                    messageQueue.RemoveAt(0);
                } else {
                    var arrayOfArguments = messageQueue.Take(count).Select(m => m.Arguments).ToArray();
                    if (DelegateArgumentTypes.Count() != 0) {
                        arguments.AddRange(TransposeAndType(arrayOfArguments));
                    }
                    messageQueue.RemoveRange(0, count);
                }
            }
        }

        internal override bool Match(int count) {
            return messageQueue.Count >= count;
        }

        public @async Invoked()
        {
            var message = new AsyncMessage(this, new object[]{});
            joiner.TryMatch(message);
            return null;
        }

        public @async Invoked(params object[] arguments) {
            var message = new AsyncMessage(this, arguments);
            joiner.TryMatch(message);
            return null;
        }
    }
}
