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

namespace ZsK.Joins {
    public abstract class Method : MethodBase {
        private Queue<Message> queue = new Queue<Message>();

        internal int QueueLength {
            get {
                lock (queue) {
                    return queue.Count();
                }
            }
        }

        private Type ByValue(Type byRef) {
            if (byRef.IsByRef) {
                return byRef.Assembly.GetType(byRef.FullName.Replace("&", ""));
            } else {
                return byRef;
            }
        }

        protected object[] TransposeAndType(object[][] input) {
            int lx = input.Length;
            int ly = lx==0 ? 0 :input[0].Length;
            var result = new object[ly];
            for (int y = 0; y < ly; y++) {
                var row = Array.CreateInstance(ByValue(DelegateArgumentTypes.ElementAt(y)), lx);
                for (int x = 0; x < lx; x++) {
                    row.SetValue(input[x][y], x);
                }
                result[y] = row;
            }
            return result;
        }
    }
}
