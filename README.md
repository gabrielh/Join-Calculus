**A C# Join-Calculus library**

Join-calculus offers a generic solution to synchronization problems. It is an elegant and simple yet powerful synchronization model. It is based on the pattern matching of messages and the execution of a block of code on matches; the messages can be either synchronous or asynchronous. Synchronous messages are blocking, can have arguments and return data. Asynchronous messages never block, can have arguments but cannot return any data. 

You can find more details on join-calculus in general and on this library in particular on my [blog](http://softwaretransactions.com/category/join-calculus/).

This join-calculus library allows C# programmers to try out join-calculus and investigate its potential in the real world.

Have a look at the wiki for more details.

This library supports the following features:
*    multiple synchronous channels can be defined in a chord
*    channels accept any number of arguments
*    chords definitions are easy to read
*    messages in chords can be defined multiple times with a factor

Note that the implementation is a bit out of date, incomplete and doesn't make use of the features of the latest versions of C# and .Net. The most notable missing features are the lack of support for async and IObservable.

There are no tests included. Do keep in mind there the library has been tested only superficially and should not be used in production code.

**Performance**
The goal of this library implementation of join-calculus is to give all C# users a chance to try out join-calculus and investigate its potential in the real world. The implementation is not optimized and it should be fairly easy to do some simple optimizations. However, since the implementation makes use of a main lock, it's performance is inherently limited.

**Examples**
The following snippet defines a simple asynchronous buffer :
{{
    (get & put).Do((T t) => {
        return t;
    });
}}
where the put method is asynchronous, while the get is synchronous and blocks until some value has been put in the buffer.

The code contains the classic examples of join-calculus with 
* asynchronous buffer
* synchronous buffer
* bounded synchronous buffer
* join
* wait
* reader/writer lock
* fair reader/writer lock
