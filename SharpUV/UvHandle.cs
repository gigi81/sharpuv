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
        Opening,
		Open,
		Closing,
		Closed,
        Resolving
	}

	public abstract class UvHandle : IDisposable
	{
	    private static int _allocatedHandles = 0;

		public event EventHandler<UvArgs> Closed;

        private readonly Loop _loop;
        private IntPtr _handle;
        private UvCallback _closeCallback;

		/// <summary>
		/// The number of current allocated handles
		/// </summary>
        public static int CurrentlyAllocatedHandles { get { return _allocatedHandles; } }

		protected UvHandle(Loop loop, IntPtr handle)
			: this(loop)
		{
			_handle = handle;
		}

		protected UvHandle(Loop loop, int handleSize)
			: this(loop)
		{
            _handle = this.Alloc(handleSize);
		}

		internal UvHandle(Loop loop, uv_handle_type handleType)
			: this(loop)
		{
            _handle = this.Alloc(handleType);
		}

		private UvHandle(Loop loop)
		{
			_loop = loop;

			this.Status = HandleStatus.Closed;
			this.InitDelegates();

            _allocatedHandles++;
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
		public Loop Loop { get { return _loop; } }

		/// <summary>
		/// Pointer to the underlying libuv uv_handle_t
		/// </summary>
        internal IntPtr Handle { get { return _handle; } }

		/// <summary>
		/// Handle status
		/// </summary>
		public HandleStatus Status { get; protected set; }

		/// <summary>
		/// Closes the stream
		/// </summary>
		public void Close(bool dispose = false, Action<UvArgs> callback = null)
		{
			if (this.Status != HandleStatus.Open)
				return;

			this.DisposeAfterClose = dispose;
			Uvi.uv_close(this.Handle, _closeDelegate);
			this.Status = HandleStatus.Closing;
            _closeCallback = new UvCallback(this, callback);
		}

		private void OnClose(IntPtr handle)
		{
            var callback = _closeCallback;
            _closeCallback = null;

			this.Status = HandleStatus.Closed;
            callback.Invoke((int)handle, this.OnClose, this.Closed);
			if (this.DisposeAfterClose)
				this.Dispose(true);
		}

		protected virtual void OnClose(UvArgs args)
		{
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
			if (this.Status != HandleStatus.Closed)
				this.Close(true); //will later call the dispose
			else
				this.Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.IsDisposed)
				return;

            if (_handle != IntPtr.Zero)
		    {
                _handle = this.Free(_handle);
                _allocatedHandles--;
		    }

            this.IsDisposed = true;
		    GC.SuppressFinalize(this);
		}
		#endregion

		public bool DisposeAfterClose { get; set; }
	}
}
