sharpuv
============

What
----

A C# binding to [libuv](https://github.com/joyent/libuv/) for .NET.
In the binding I tried also to wrap the C calls to a more object oriented API.
For now it's available only an echo tcp/ip server test program tested only on Windows 7.

This work is based on the initial effort by [Kerry Snyder](https://github.com/kersny/libuv-csharp).

Requirements
----

You need python and svn on the path to be able to build the libuv library.
Libuv is downloaded as a submodule repository so you just need to clone this repository and it will be downloaded automatically.
We are using a branch with a modified build script in order to be able to build the library as a DLL (shared build).
Unfortunately the libuv-test is broken when building libuv as a DLL. So you will need to run the solution build twice.

