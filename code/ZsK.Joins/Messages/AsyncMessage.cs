/*
Copyright (C)  by Gabriel Horvath

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
using System.Diagnostics;
using System.Linq;

namespace ZsK.Joins {

    internal sealed class AsyncMessage : Message {

        internal AsyncMessage(AsyncMethod asyncMethod, object[] arguments) {
            AsyncMethod = asyncMethod;
            Arguments = arguments;
            VerifyArguments();
        }

        /// <summary>
        /// The sync method associated to this message.
        /// </summary>
        internal readonly AsyncMethod AsyncMethod;
        
        internal readonly object[] Arguments;

        //TODO: test this method
        [Conditional("DEBUG")]
        private void VerifyArguments() {
            if (!AsyncMethod.DelegateArgumentTypes.SequenceEqual(Arguments.Select(o => o.GetType()))) {
                //TODO: improve the exception message
                throw new Exception("Arguments don't match the signature of the method");
            }
        }
    }
}
