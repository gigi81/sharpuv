#region License
/**
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
using System.Runtime.InteropServices;
using System.Text;
using Libuv;

namespace SharpUV
{
	public class Loop : IDisposable
	{
		private static readonly Loop DefaultLoop = new Loop(Uvi.uv_default_loop());
		private Dictionary<IntPtr, int> _allocs = new Dictionary<IntPtr, int>();

		public Loop()
			: this(Uvi.uv_loop_new())
		{
		}

		protected Loop(IntPtr handle)
		{
			this.Handle = handle;
		}

		~Loop()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// This function starts the event loop. It blocks until the reference count
		/// of the loop drops to zero.
		/// </summary>
		public void Run()
		{
			CheckError(Uvi.uv_run(this.Handle));
		}

		/// <summary>
		/// This function polls for new events without blocking
		/// </summary>
		public void RunOnce()
		{
			CheckError(Uvi.uv_run_once(this.Handle));
		}

		public void CheckError(int code)
		{
			if (code != 0)
				throw new UvException(this.GetLastError());
		}

		public Error GetLastError()
		{
			return new Error(Uvi.uv_last_error(this.Handle));
		}

		public IntPtr Handle { get; private set; }

		public static Loop Default
		{
			get { return DefaultLoop; }
		}

		internal IntPtr Alloc(int size)
		{
			var ret = Marshal.AllocHGlobal(size);
			_allocs.Add(ret, size);
			this.AllocatedBytes += (ulong)size;
			return ret;
		}

		internal IntPtr Free(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return ptr;

			Marshal.FreeHGlobal(ptr);

			this.DeAllocatedBytes += (ulong)_allocs[ptr];
			_allocs.Remove(ptr);

			return IntPtr.Zero;
		}

		public ulong AllocatedBytes { get; private set; }

		public ulong DeAllocatedBytes { get; private set; }

		#region Dispose Management
		/// <summary>
		/// Indicates if the object has been disposed
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Dispose the object freeing unmanaged resources allocated
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.IsDisposed)
				return;

			//we don't delete the default's loop handle
			if (this.Handle != DefaultLoop.Handle)
			{
				if (this.Handle != IntPtr.Zero)
				{
					Uvi.uv_loop_delete(this.Handle);
					this.Handle = IntPtr.Zero;
				}
			}

			GC.SuppressFinalize(this);
			this.IsDisposed = true;
		}
		#endregion
	}
}
