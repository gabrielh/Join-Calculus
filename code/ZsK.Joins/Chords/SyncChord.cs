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
using System.Linq;
using System.Collections.Generic;

namespace ZsK.Joins {


    internal sealed class  SyncChord : Chord {
        internal SyncChord(SyncMethod primarySyncMethod){
            primaryMethod = primarySyncMethod;
        }

        private SyncMethod primaryMethod;

        internal SyncMethod PrimaryMethod {
            set { primaryMethod = value;}
        }

        internal override bool Match() {
            return methodToMultiplier.All(pair => pair.Key.Match(pair.Value));
        }

        internal override void Dequeue(ref SyncMessage primaryMessage, Message newMessage) {
            primaryMessage = primaryMethod.Top;

            primaryMessage.CollatedArguments = new List<object>();
            primaryMessage.ReturnToSyncMessages = new List<SyncMessage>();

            foreach (var pair in methods) {
                pair.Key.Dequeue(pair.Value, primaryMessage);//, primaryMessage.CollatedArguments, primaryMessage.mReturnToSyncMessages);
            }
            //remove the primary message from the list of returns
            primaryMessage.ReturnToSyncMessages.Remove(primaryMessage);
        }


        internal object ExecuteBody(object[] arguments) {
            switch (threadingType) {
                case ThreadingType.Continue:
                case ThreadingType.Spawn:
                case ThreadingType.Pool:
                    throw new Exception("Unexcepted ThreadingType");
                    break;
                case ThreadingType.Sync:
                    try {
                        return @delegate.DynamicInvoke(arguments);
                    } catch (Exception e) {
                        if (e.InnerException != null) {
                            throw e.InnerException;
                        } else {
                            throw;
                        }
                    }
                    break;
                case ThreadingType.SyncContext:
                    if (primaryMethod.Joiner.SynchronizationContext == null) {
                        throw new SynchronizationContextNotSet("You must set the SynchronizationContext in the Joiner class");
                    }
                    try {
                        //TODO: must test the exception handling here
                        //TODO: test return value
                        object result = null;
                        primaryMethod.Joiner.SynchronizationContext.Send(state => result = @delegate.DynamicInvoke(arguments), null);
                        return result;
                    } catch (Exception e) {
                        if (e.InnerException != null) {
                            throw e.InnerException;
                        } else {
                            throw;
                        }
                    }
                    break;
            }
            throw new Exception("Shouldn't have arrived this far...");
        }
    }
}
