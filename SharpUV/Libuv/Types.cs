#region License
/**
 * Copyright (c) 2011 Kerry Snyder
 * Copyright (c) 2012 Luigi Grilli
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute,
 * sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
#endregion

using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Libuv
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_req_t
	{
		internal IntPtr data;
		internal uv_req_type type;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_process_options_t
	{
		internal uv_exit_cb exit_cb;
		internal string file;
		internal IntPtr[] args;
		internal IntPtr[] env;
		internal string cwd;
		internal int windows_verbatim_arguments;

		internal IntPtr stdin_stream;
		internal IntPtr stdout_stream;
		internal IntPtr stderr_stream;
	}

	/// <summary>
	/// Due to platform differences the user cannot rely on the ordering of the
	/// base and len members of the uv_buf_t struct. The user is responsible for 
	/// freeing base after the uv_buf_t is done. Return struct passed by value.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
#if __MonoCS__
	internal struct uv_buf_t
	{
		internal IntPtr data;
		internal IntPtr len;
	}
#else
	internal struct uv_buf_t
	{
		internal IntPtr len;
		internal IntPtr data;
	}
#endif


	// From: http://www.elitepvpers.com/forum/co2-programming/159327-advanced-winsock-c.html
	[StructLayout(LayoutKind.Sequential, Size = 16)]
	internal struct sockaddr_in
	{
		internal short sin_family;
		internal ushort sin_port;
	}

    // From: http://www.pinvoke.net/default.aspx/Structures/sockaddr_in6.html
    [StructLayout(LayoutKind.Sequential, Size = 28)]
    internal struct sockaddr_in6
    {
        internal short sin6_family;
        internal ushort sin6_port;
    }

    internal enum uv_err_code
	{
		UV_UNKNOWN = -1,
		UV_OK = 0,
		UV_EOF,
		UV_EACCESS,
		UV_EAGAIN,
		UV_EADDRINUSE,
		UV_EADDRNOTAVAIL,
		UV_EAFNOSUPPORT,
		UV_EALREADY,
		UV_EBADF,
		UV_EBUSY,
		UV_ECONNABORTED,
		UV_ECONNREFUSED,
		UV_ECONNRESET,
		UV_EDESTADDRREQ,
		UV_EFAULT,
		UV_EHOSTUNREACH,
		UV_EINTR,
		UV_EINVAL,
		UV_EISCONN,
		UV_EMFILE,
		UV_ENETDOWN,
		UV_ENETUNREACH,
		UV_ENFILE,
		UV_ENOBUFS,
		UV_ENOMEM,
		UV_ENONET,
		UV_ENOPROTOOPT,
		UV_ENOTCONN,
		UV_ENOTSOCK,
		UV_ENOTSUP,
		UV_EPROTO,
		UV_EPROTONOSUPPORT,
		UV_EPROTOTYPE,
		UV_ETIMEDOUT,
		UV_ECHARSET,
		UV_EAIFAMNOSUPPORT,
		UV_EAINONAME,
		UV_EAISERVICE,
		UV_EAISOCKTYPE,
		UV_ESHUTDOWN
	}

	internal enum uv_handle_type
	{
		UV_UNKNOWN_HANDLE = 0,
		UV_ASYNC,
		UV_CHECK,
		UV_FS_EVENT,
		UV_FS_POLL,
		UV_HANDLE,
		UV_IDLE,
		UV_NAMED_PIPE,
		UV_POLL,
		UV_PREPARE,
		UV_PROCESS,
		UV_STREAM,
		UV_TCP,
		UV_TIMER,
		UV_TTY,
		UV_UDP,
		UV_SIGNAL
	}

	internal enum uv_req_type
	{
		UV_UNKNOWN_REQ = 0,
		UV_REQ,
		UV_CONNECT,
		UV_WRITE,
		UV_SHUTDOWN,
		UV_UDP_SEND,
		UV_FS,
		UV_WORK,
		UV_GETADDRINFO
	}
		
    [Flags]
    public enum uv_tcp_flags : uint
    {
        /// <summary>
        /// Used with uv_tcp_bind, when an IPv6 address is used
        /// </summary>
        UV_TCP_IPV6ONLY = 1
    }

	#region Callbacks
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void uv_shutdown_cb(IntPtr req, int status);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void uv_alloc_cb(
				IntPtr stream,
				[MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Libuv.SizeTMarshaler")]
				SizeT suggested_size,
                IntPtr buf
	);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void uv_read_cb(
                IntPtr req,
                int nread,
                IntPtr buf
    ); //buf = uv_buf_t

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void uv_write_cb(IntPtr req, int status);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void uv_connect_cb(IntPtr conn, int status); //uv_connect_t*

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void uv_close_cb(IntPtr conn);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void uv_connection_cb(IntPtr server, int status); //uv_stream_t* server

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void uv_watcher_cb(IntPtr watcher, int status);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void uv_exit_cb(IntPtr handle, int exit_status, int term_signal); // uv_process_t*

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void uv_fs_cb(IntPtr req); // uv_fs_t*

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void uv_thread_run(IntPtr arg);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void uv_work_cb(IntPtr req); //uv_work_t* req

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void uv_after_work_cb(IntPtr req, int status); //uv_work_t* req

	#endregion
}
