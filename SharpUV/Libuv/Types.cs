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
	internal struct uv_handle_t
	{
		internal uv_handle_type type;
		internal IntPtr close_cb;
		internal IntPtr data;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_req_t
	{
		internal uv_req_type type;
		internal IntPtr data;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_connect_t
	{
		internal uv_req_type type;
		internal IntPtr data;
		#if !__MonoCS__
		internal NativeOverlapped overlapped;
		internal IntPtr queued_bytes;
		internal uv_err_t error;
		internal IntPtr next_req;
		#endif
		internal IntPtr cb;
		internal IntPtr stream; //uv_stream_t*
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_shutdown_t
	{
		internal uv_req_type type;
		internal IntPtr data;
		#if !__MonoCS__
		internal NativeOverlapped overlapped;
		internal IntPtr queued_bytes;
		internal uv_err_t error;
		internal IntPtr next_req;
		#endif
		internal IntPtr handle;
		internal IntPtr cb;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_err_t
	{
		internal uv_err_code code;
		internal int sys_errno_;
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
		internal const int Size = 16;

		internal short sin_family;
		internal ushort sin_port;
		internal struct in_addr
		{
			internal uint S_addr;
			internal struct _S_un_b
			{
				internal byte s_b1, s_b2, s_b3, s_b4;
			}
			internal _S_un_b S_un_b;
			internal struct _S_un_w
			{
				internal ushort s_w1, s_w2;
			}
			internal _S_un_w S_un_w;
		}
		internal in_addr sin_addr;
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
		UV_IDLE,
		UV_NAMED_PIPE,
		UV_POLL,
		UV_PREPARE,
		UV_PROCESS,
		UV_TCP,
		UV_TIMER,
		UV_TTY,
		UV_UDP
	}

	internal enum uv_req_type
	{
		UV_UNKNOWN_REQ = 0,
		UV_CONNECT,
		UV_WRITE,
		UV_SHUTDOWN,
		UV_UDP_SEND,
		UV_FS,
		UV_WORK,
		UV_GETADDRINFO
	}

	public enum FileAccessMode
	{
		ReadOnly  = 0x0000,  /* open for reading only */
		WriteOnly = 0x0001,  /* open for writing only */
		ReadWrite = 0x0002 /* open for reading and writing */
	}

	[Flags]
	public enum FileOpenMode
	{
		Default 	 = 0x0000,
		Append 		 = 0x0008,  /* writes done at eof */
		Create 		 = 0x0100,  /* create and open file */
		Truncate 	 = 0x0200,  /* open and truncate */
		OnlyIfExists = 0x0400,  /* open only if file doesn't already exist */
		TextMode 	 = 0x4000,  /* file mode is text (translated) */
		BinaryMode 	 = 0x8000  /* file mode is binary (untranslated) */
	}

	[Flags]
	public enum FilePermissions
	{
		S_IRWXU  = 0x0700,		// user (file owner) has read, write and execute permission 
		S_IRUSR  = 0x0400 ,		// user has read permission 
		S_IREAD  = 0x0400,
		S_IWUSR  = 0x0200,		// user has write permission 
		S_IWRITE = 0x0200,
		S_IXUSR  = 0x0100,		// user has execute permission
		S_IEXEC  = 0x0100,

		S_IRWXG  = 0x0070,		// group has read, write and execute permission     
		S_IRGRP  = 0x0040,		// group has read permission 
		S_IWGRP  = 0x0020,		// group has write permission
		S_IXGRP  = 0x0010,		// group has execute permission 

		S_IRWXO  = 0x0007,		// others have read, write and execute permission 
		S_IROTH  = 0x0004,		// others have read permission 
		S_IWOTH  = 0x0002,		// others have write permisson 
		S_IXOTH  = 0x0001		// others have execute permission 
	}

	#region Callbacks
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void uv_shutdown_cb(IntPtr req, int status);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate uv_buf_t uv_alloc_cb(
				IntPtr stream,
				[MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Libuv.SizeTMarshaler")]
				SizeT suggested_size
	);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void uv_read_cb(IntPtr req, IntPtr nread, uv_buf_t buf);

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

	#endregion
}
