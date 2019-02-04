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

    internal sealed class AsyncChord : Chord {

        internal AsyncChord(){
        }
        
        internal override bool Match() {
            return methodToMultiplier.All(pair => pair.Key.Match(pair.Value));
        }

        internal override void Dequeue(ref SyncMessage primaryMessage, Message newMessage) {
            newMessage.CollatedArguments = new List<object>();
            foreach (var pair in methods) {
                (pair.Key as AsyncMethod).Dequeue(pair.Value, newMessage.CollatedArguments);
            }
        }

        internal void ExecuteBody(object[] arguments) {
            switch (threadingType) {
                case ThreadingType.Sync:
                    throw new Exception("Unexcepted ThreadingType");
                    break;

                case ThreadingType.Continue:
                    try {
                        @delegate.DynamicInvoke(arguments);
                    } catch (Exception e) {
                        if (e.InnerException != null) {
                            throw e.InnerException;
                        } else {
                            throw;
                        }
                    }
                    break;

                case ThreadingType.Pool:
                    try {
                        //TODO: must test the exception handling here
                        ThreadPool.QueueUserWorkItem(arg => @delegate.DynamicInvoke(arguments), null);
                    } catch (Exception e) {
                        if (e.InnerException != null) {
                            throw e.InnerException;
                        } else {
                            throw;
                        }
                    }
                    break;

                case ThreadingType.SyncContext:
                    if (methods.First().Key.Joiner.SynchronizationContext == null) {
                        throw new SynchronizationContextNotSet("You must set the SynchronizationContext in the Joiner class");
                    }
                    try {
                        //TODO: must deal with exceptions here
                        methods.First().Key.Joiner.SynchronizationContext.Post(state => @delegate.DynamicInvoke(arguments), null);
                    } catch (Exception e) {
                        if (e.InnerException != null) {
                            throw e.InnerException;
                        } else {
                            throw;
                        }
                    }
                    break;

                case ThreadingType.Spawn:
                    new Thread(() => {
                        try {
                            @delegate.DynamicInvoke(arguments);
                        } catch (Exception e) {
                            //TODO: must deal with this case... who will get the exception??????
                        }
                    }).Start();
                    break;
            }
        }

    }

}
