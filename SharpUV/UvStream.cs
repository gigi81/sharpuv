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
using System.Runtime.InteropServices;
using System.IO;
using Libuv;

namespace SharpUV
{
	public class UvStream : UvHandle
	{
        public event EventHandler<UvDataArgs> OnReadData;
        public event EventHandler<UvDataArgs> OnWriteData;
        public event EventHandler<UvArgs> ShuttedDown;

        private bool _isReading = false;

        private UvDataCallback _readCallback;
        private UvDataCallback _writeCallback;
        private UvCallback _shutdownCallback;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loop">Loop</param>
		/// <param name="handle">Pointer to a <typeparamref name="Libuv.uv_stream_t"/> structure</param>
		protected UvStream(Loop loop, IntPtr handle)
			: base(loop, handle)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loop">Loop</param>
		/// <param name="handleSize">Size of the <typeparamref name="Libuv.uv_stream_t"/> structure to allocate</param>
		protected UvStream(Loop loop, int handleSize)
			: base(loop, handleSize)
		{
		}

		internal UvStream(Loop loop, uv_handle_type handleType)
			: base(loop, handleType)
		{
		}

		#region Delegates
		private uv_alloc_cb _allocDelegate;
		private uv_read_cb _readDelegate;
		private uv_write_cb _writeDelegate;
		private uv_shutdown_cb _shutdownDelegate;

		protected override void InitDelegates()
		{
			base.InitDelegates();

			_allocDelegate    = new uv_alloc_cb(this.OnAlloc);
			_readDelegate     = new uv_read_cb(this.OnRead);
			_writeDelegate    = new uv_write_cb(this.OnWrite);
			_shutdownDelegate = new uv_shutdown_cb(this.OnShutdown);
		}
		#endregion

		public void ReadStart(Action<UvDataArgs> callback = null)
		{
			CheckError(Uvi.uv_read_start(this.Handle, _allocDelegate, _readDelegate));
            _isReading = true;
            _readCallback = new UvDataCallback(this, callback, null);
		}

		public void ReadStop()
		{
			CheckError(Uvi.uv_read_stop(this.Handle));
            _isReading = false;
		}

        public bool IsReading { get { return _isReading; } }

		private void OnAlloc(IntPtr tcp, SizeT size, IntPtr buf)
		{
			try
			{
                this.Loop.Buffers.AllocBuffer(buf, (uint)size.Value);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Buffer allocation failed with error: {0}", ex.Message);
			}
		}

        private void OnRead(IntPtr stream, int nread, IntPtr buf)
		{
            var data = this.Loop.Buffers.CopyAndDeleteBuffer(buf, (int)nread);
            _readCallback.Invoke((int)nread, data, this.OnRead, this.OnReadData);

            if (nread < 0)
				this.Close();
		}

	    public bool CanRead
	    {
            get { return Uvi.uv_is_readable(this.Handle) != 0; }
	    }

        public bool CanWrite
        {
            get { return Uvi.uv_is_writable(this.Handle) != 0; }
        }

        public void Write(byte[] data, Action<UvDataArgs> callback = null)
		{
			this.Write(data, 0, data.Length, callback);
		}

        public void Write(byte[] data, int offset, int length, Action<UvDataArgs> callback = null)
		{
			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.Loop.Requests.Create(uv_req_type.UV_WRITE, data, offset, length);
				CheckError((Uvi.uv_write(req, this.Handle, new[] { this.Loop.Requests[req] }, 1, _writeDelegate)));
                _writeCallback = new UvDataCallback(this, callback, data);
			}
			catch (Exception)
			{
				this.Loop.Requests.Delete(req);
				throw;
			}
		}

		private void OnWrite(IntPtr requestHandle, int status)
		{
			this.Loop.Requests.Delete(requestHandle);
            _writeCallback.Invoke(status, this.OnWrite, this.OnWriteData);
		}

		public void Shutdown(Action<UvArgs> callback = null)
		{
			IntPtr req = this.Alloc(uv_req_type.UV_SHUTDOWN);

			try
			{
				if (_isReading)
					this.ReadStop();

				CheckError(Uvi.uv_shutdown(req, this.Handle, _shutdownDelegate));
                _shutdownCallback = new UvCallback(this, callback);
			}
			catch (Exception)
			{
				this.Free(req);
				throw;
			}
		}

		private void OnShutdown(IntPtr req, int status)
		{
            this.Free(req);
            _shutdownCallback.Invoke(status, this.OnShutdown, this.ShuttedDown);
			this.Close();
		}

		protected virtual void OnRead(UvDataArgs args)
		{
		}

        protected virtual void OnWrite(UvDataArgs args)
		{
		}

		protected virtual void OnShutdown(UvArgs args)
		{
		}
	}
}
