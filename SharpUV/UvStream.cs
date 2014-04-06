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

		public void ReadStart()
		{
			CheckError(Uvi.uv_read_start(this.Handle, _allocDelegate, _readDelegate));
			this.IsReading = true;
		}

		public void ReadStop()
		{
			CheckError(Uvi.uv_read_stop(this.Handle));
			this.IsReading = false;
		}

		public bool IsReading { get; private set; }

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

        private void OnRead(IntPtr stream, SSizeT nread, IntPtr buf)
		{
            if (nread.Value > 0)
				this.OnRead(this.Loop.Buffers.CopyAndDeleteBuffer(buf, (int)nread.Value));

            if (nread.Value < 0)
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

		public void Write(byte[] data)
		{
			this.Write(data, 0, data.Length);
		}

		public void Write(byte[] data, int offset, int length)
		{
			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.Loop.Requests.Create(uv_req_type.UV_WRITE, data, offset, length);
				CheckError((Uvi.uv_write(req, this.Handle, new[] { this.Loop.Requests[req] }, 1, _writeDelegate)));
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

			if (status != 0) {
				var ex = new UvException (status);
			}

			this.OnWrite();
		}

		public void Shutdown()
		{
			IntPtr req = this.Alloc(uv_req_type.UV_SHUTDOWN);

			try
			{
				if (this.IsReading)
					this.ReadStop();

				CheckError(Uvi.uv_shutdown(req, this.Handle, _shutdownDelegate));
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

			if (status != 0)
			{
				var ex = new UvException(status);
			}

			this.OnShutdown();
			this.Close();
		}

		protected virtual void OnRead(byte[] data)
		{
		}

		protected virtual void OnWrite()
		{
		}

		protected virtual void OnShutdown()
		{
		}
	}
}
