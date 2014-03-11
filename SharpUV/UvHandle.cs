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
using System.Text;
using Libuv;

namespace SharpUV
{
	/// <summary>
	/// Status of an handle
	/// </summary>
	public enum HandleStatus
	{
		Open,
		Closing,
		Closed
	}

	public abstract class UvHandle : IDisposable
	{
		public event EventHandler Closed;

		/// <summary>
		/// The number of current allocated handles
		/// </summary>
		public static int CurrentlyAllocatedHandles { get; private set; }

		protected UvHandle(Loop loop, IntPtr handle)
			: this(loop)
		{
			this.Handle = handle;
		}

		protected UvHandle(Loop loop, int handleSize)
			: this(loop)
		{
			this.Handle = this.Alloc(handleSize);
		}

		internal UvHandle(Loop loop, uv_handle_type handleType)
			: this(loop)
		{
			this.Handle = this.Alloc(handleType);
		}

		private UvHandle(Loop loop)
		{
			this.Loop = loop;
			this.HandleStatus = HandleStatus.Open;
			this.InitDelegates();

			UvHandle.CurrentlyAllocatedHandles++;
		}

		~UvHandle()
		{
			this.Dispose();
		}

		#region Delegates
		private uv_close_cb _closeDelegate;

		protected virtual void InitDelegates()
		{
			_closeDelegate = new uv_close_cb(this.OnClose);
		}
		#endregion


		/// <summary>
		/// The Loop wherein this object is running
		/// </summary>
		public Loop Loop { get; private set; }

		/// <summary>
		/// Pointer to the underlying libuv uv_handle_t
		/// </summary>
		internal IntPtr Handle { get; private set; }

		/// <summary>
		/// Handle status
		/// </summary>
		internal HandleStatus HandleStatus { get; private set; }

		/// <summary>
		/// Closes the stream
		/// </summary>
		public void Close(bool dispose = false)
		{
			if (this.HandleStatus != HandleStatus.Open)
				return;

			this.DisposeAfterClose = dispose;
			Uvi.uv_close(this.Handle, _closeDelegate);
			this.HandleStatus = HandleStatus.Closing;
		}

		private void OnClose(IntPtr handle)
		{
			this.HandleStatus = HandleStatus.Closed;
			this.OnClose();
			if (this.DisposeAfterClose)
				this.Dispose(true);
		}

		protected virtual void OnClose()
		{
			if (this.Closed != null)
				this.Closed(this, EventArgs.Empty);
		}

		internal void CheckError(int code)
		{
			this.Loop.CheckError(code);
		}

		internal IntPtr Alloc(int size)
		{
			return this.Loop.Allocs.Alloc(size);
		}

		internal IntPtr Alloc(uv_handle_type handleType)
		{
			return Alloc(Uvi.uv_handle_size(handleType));
		}

		internal IntPtr Alloc(uv_req_type requestType)
		{
			return Alloc(Uvi.uv_req_size(requestType));
		}

		internal IntPtr Free(IntPtr ptr)
		{
			return this.Loop.Allocs.Free(ptr);
		}

		#region Disposal Management
		/// <summary>
		/// Indicates if the object has been disposed
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Dispose the object freeing unmanaged resources allocated
		/// </summary>
		public void Dispose()
		{
			if (this.HandleStatus != HandleStatus.Closed)
				this.Close(true); //will later call the dispose
			else
				this.Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.IsDisposed)
				return;

		    if (this.Handle != IntPtr.Zero)
		    {
		        this.Handle = this.Free(this.Handle);
		        UvHandle.CurrentlyAllocatedHandles--;
		    }

            this.IsDisposed = true;
		    GC.SuppressFinalize(this);
		}
		#endregion

		public bool DisposeAfterClose { get; set; }
	}
}
