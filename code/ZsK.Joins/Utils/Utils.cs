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

namespace ZsK.Joins.Utilities {

    public static class Utils {
        public static string ToString2<T>(this IEnumerable<T> source) {
            return source.Select(o => o.ToString()).Aggregate((s, v) => s + v);
        }

        public static void Spawn(Action action) {
            new Thread(() => action()).Start();
        }

        public static void Join(Action action1, Action action2) {
            JoinMany(new Action[]{action1, action2});
        }

        public static void Join(Action action1, Action action2, Action action3) {
            JoinMany(new Action[]{action1, action2, action3});
        }

        public static void JoinMany(Action[] actions) {
            var threads = actions.Select(a => new Thread(() => a())).ToList();
            foreach (var t in threads){
                t.Start();
            }
            foreach (var t in threads){
                t.Join();
            }
        }

        public static void Times(this int count, Action<int> body) {
            for (int i = 0; i < count; i++) {
                body(i);
            }
        }
        public static void Times(this int count, Action body) {
            for (int i = 0; i < count; i++) {
                body();
            }
        }
    }

}
