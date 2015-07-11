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
		Closed = 0,
        Opening,
		Open,
		Closing,
        Resolving
	}

	public abstract class UvHandle : IDisposable
	{
		public event EventHandler<UvArgs> Closed;

        private readonly Loop _loop;
        private IntPtr _handle;
        private UvCallback _closeCallback;
		private bool _disposeAfterClose = false;

		internal UvHandle(Loop loop, uv_handle_type handleType)
			: this(loop)
		{
            _handle = loop.Allocs.AllocHandle(handleType);
		}

		private UvHandle(Loop loop)
		{
			_loop = loop;

			this.Status = HandleStatus.Closed;
			this.InitDelegates();
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

			_disposeAfterClose = dispose;
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
			if (_disposeAfterClose)
				this.Dispose(true);
		}

		protected virtual void OnClose(UvArgs args)
		{
		}

		internal void CheckError(int code)
		{
			this.Loop.CheckError(code);
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

            _handle = _loop.Allocs.FreeHandle(_handle);
            this.IsDisposed = true;
		    GC.SuppressFinalize(this);
		}
		#endregion
	}
}
