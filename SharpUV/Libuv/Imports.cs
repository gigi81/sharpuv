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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using uv_file = System.Int32;

namespace Libuv
{
	internal static class Uvi
	{
	    private const string ModuleName = "libuv";

        internal const int sockaddr_in_size = 16;

        internal const int sockaddr_in6_size = 28;

		internal enum uv_run_mode : int
		{
			UV_RUN_DEFAULT = 0,
			UV_RUN_ONCE,
			UV_RUN_NOWAIT
		};

		#region Loop functions

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr uv_loop_new();
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void uv_loop_delete(IntPtr loop); //uv_loop_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr uv_default_loop(); // uv_loop_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_run(IntPtr loop, uv_run_mode uv_run_mode); // uv_loop_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr uv_strerror(int err);

		#endregion

		#region Common functions

		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_handle_size(uv_handle_type handleType);

		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_req_size(uv_req_type reqType);

		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_check_init(IntPtr loop, IntPtr check); // uv_loop_t*, uv_check_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_check_start(IntPtr check, uv_watcher_cb cb); // uv_check_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_check_stop(IntPtr check); // uv_check_t*

		#endregion

		#region Stream Functions

		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_read_start(IntPtr stream, uv_alloc_cb alloc_cb, uv_read_cb read); // uv_stream_t*
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_read_stop(IntPtr stream); // uv_stream_t*
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_write(IntPtr req, IntPtr handle, uv_buf_t[] bufs, int bufcnt, uv_write_cb cb); // uv_write_t*, uv_stream_t*
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_shutdown(IntPtr req, IntPtr handle, uv_shutdown_cb cb); // uv_shutdown_t*, uv_stream_t*

		#endregion

		#region Timer functions

		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_timer_init(IntPtr loop, IntPtr timer); // uv_loop_t*, uv_timer_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_timer_start(IntPtr timer, uv_watcher_cb cb, double after, double repeat); // uv_timer_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_timer_stop(IntPtr timer); // uv_timer_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_timer_again(IntPtr timer); // uv_timer_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_timer_set_repeat(IntPtr timer, double time); // uv_timer_t*

		#endregion

		#region Tcp functions

		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr prepare); // uv_loop_t*, uv_tcp_t*
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_accept(IntPtr server, IntPtr client); // uv_stream_t*, uv_stream_t*
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void uv_close(IntPtr handle, uv_close_cb cb); // uv_handle_t*
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_tcp_connect(IntPtr connect, IntPtr tcp_handle, IntPtr address, uv_connect_cb cb); // uv_connect_t*, uv_tcp_t*, sockaddr_in* or sockaddr_in6*
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_ip4_addr(string ip, int port, IntPtr address); //sockaddr_in
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_ip6_addr(string ip, int port, IntPtr address); //sockaddr_in6

		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_tcp_bind(IntPtr handle, IntPtr address, uv_tcp_flags flags); // uv_tcp_t*, sockaddr_in*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_listen(IntPtr stream, int backlog, uv_connection_cb cb); // uv_stream_t*

		#endregion

		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_prepare_init(IntPtr loop, IntPtr prepare); // uv_loop_t*, uv_prepare_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_prepare_start(IntPtr prepare, uv_watcher_cb cb); // uv_prepare_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_prepare_stop(IntPtr prepare); // uv_prepare_t*

		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_pipe_init(IntPtr loop, IntPtr prepare); // uv_loop_t*, uv_pipe_t*
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_pipe_connect(IntPtr connect, IntPtr tcp_handle, string path, uv_connect_cb cb); // uv_connect_req*, uv_pipe_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_pipe_bind(IntPtr prepare, string name); // uv_pipe_t*

		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_idle_init(IntPtr loop, IntPtr idle); // uv_loop_t*, uv_idle_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_idle_start(IntPtr idle, uv_watcher_cb cb); // uv_idle_t*
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_idle_stop(IntPtr idle); // uv_idle_t*

		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_spawn(IntPtr loop, IntPtr process, uv_process_options_t options); // uv_loop_t*, uv_process_t*
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_process_kill(IntPtr process, int signum); // uv_process_t*

		#region Filesystem functions
		/// <summary>
		/// 
		/// </summary>
		/// <param name="req">(uv_fs_t*)</param>
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void uv_fs_req_cleanup(IntPtr req);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="req">(uv_fs_t*)</param>
		/// <returns></returns>
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uv_file uv_fs_req_result(IntPtr req);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loop">(uv_loop_t*)</param>
		/// <param name="req">(uv_fs_t*)</param>
		/// <param name="path"></param>
		/// <param name="flags"></param>
		/// <param name="mode"></param>
		/// <param name="cb"></param>
		/// <returns></returns>
		[DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_fs_open(IntPtr loop, IntPtr req, byte[] path, int flags, int mode, uv_fs_cb cb);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loop">(uv_loop_t*)</param>
		/// <param name="req">(uv_fs_t*)</param>
		/// <param name="file"></param>
		/// <param name="cb"></param>
		/// <returns></returns>
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_fs_close(IntPtr loop, IntPtr req, uv_file file, uv_fs_cb cb);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loop">(uv_loop_t*)</param>
		/// <param name="req">(uv_fs_t*)</param>
		/// <param name="file"></param>
		/// <param name="buf">(void*)</param>
		/// <param name="length"></param>
		/// <param name="offset"></param>
		/// <param name="cb"></param>
		/// <returns></returns>
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_fs_read(
			IntPtr loop,
			IntPtr req,
			uv_file file,
			uv_buf_t[] bufs,
			int nbufs,
			[MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Libuv.OffTMarshaler")]
			OffT offset,
			uv_fs_cb cb
		);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loop">(uv_loop_t*)</param>
		/// <param name="req">(uv_fs_t*)</param>
		/// <param name="file"></param>
		/// <param name="buf">(void*)</param>
		/// <param name="length"></param>
		/// <param name="offset"></param>
		/// <param name="cb"></param>
		/// <returns></returns>
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_fs_write(
			IntPtr loop,
			IntPtr req,
			uv_file file,
			uv_buf_t[] bufs,
			int nbufs,
			long offset,
			uv_fs_cb cb
		);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loop">(uv_loop_t*)</param>
		/// <param name="req">(uv_fs_t*)</param>
		/// <param name="path"></param>
		/// <param name="mode"></param>
		/// <param name="cb"></param>
		/// <returns></returns>
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_fs_mkdir(IntPtr loop, IntPtr req, string path, int mode, uv_fs_cb cb);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loop">(uv_loop_t*)</param>
		/// <param name="req">(uv_fs_t*)</param>
		/// <param name="path"></param>
		/// <param name="cb"></param>
		/// <returns></returns>
		[DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_fs_rmdir(IntPtr loop, IntPtr req, string path, uv_fs_cb cb);

		#endregion

        #region Threading
        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_thread_create(IntPtr tid, uv_thread_run entry, IntPtr arg);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_thread_join(IntPtr tid);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint uv_thread_self();

        /// <summary>
        /// Queues a work request to execute asynchronously on the thread pool
        /// </summary>
        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_queue_work(IntPtr loop, IntPtr req, uv_work_cb work_cb, uv_after_work_cb after_work_cb); //uv_work_t* req, 

        #endregion

        #region Utility functions
        [DllImport (ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uv_buf_t uv_buf_init(IntPtr data, uint size);

        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_is_readable(IntPtr handle); //uv_stream_t*
        [DllImport(ModuleName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_is_writable(IntPtr handle); //uv_stream_t*
        #endregion
    }
}
