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
		private Dictionary<IntPtr, uv_buf_t> _writes = new Dictionary<IntPtr, uv_buf_t>();

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

		private uv_buf_t OnAlloc(IntPtr tcp, SizeT size)
		{
			try
			{
				return CreateBuffer(size);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Buffer allocation failed with error: {0}", ex.Message);
				return new uv_buf_t { data = IntPtr.Zero, len = IntPtr.Zero };
			}
		}

		private void OnRead(IntPtr stream, IntPtr nread, uv_buf_t buf)
		{
			int size = (int) nread;

			if (size > 0)
			{
				var data = new byte[size];
				Marshal.Copy(buf.data, data, 0, size);
				this.OnRead(data);
			}

			this.DeleteBuffer(buf);

			if (size < 0)
				this.Close();
		}

		public void Write(byte[] data)
		{
			this.Write(data, 0, data.Length);
		}

		public void Write(byte[] data, int offset, int length)
		{
			IntPtr requestHandle = IntPtr.Zero;

			try
			{
				requestHandle = this.InitWrite(data, offset, length);
				CheckError((Uvi.uv_write(requestHandle, this.Handle, new[] { _writes[requestHandle] }, 1, _writeDelegate)));
			}
			catch (Exception)
			{
				if(requestHandle != IntPtr.Zero)
					this.FreeWrite(requestHandle);

				throw;
			}
		}

		private void OnWrite(IntPtr requestHandle, int status)
		{
			if (status != 0)
			{
				var error = this.Loop.GetLastError();
			}

			this.FreeWrite(requestHandle);
			this.OnWrite();
		}

		private IntPtr InitWrite(byte[] data, int offset, int length)
		{
			var requestHandle = this.Alloc(uv_req_type.UV_WRITE);
			var buffer = this.CreateBuffer(data, offset, length);

			uv_req_t request = new uv_req_t()
			{
				type = uv_req_type.UV_WRITE,
				data = buffer.data
			};

			Marshal.StructureToPtr(request, requestHandle, false);

			_writes.Add(requestHandle, buffer);
			return requestHandle;
		}

		private void FreeWrite(IntPtr requestHandle)
		{
			this.DeleteBuffer(_writes[requestHandle]);
			this.Free(requestHandle);
			_writes.Remove(requestHandle);
		}

		public void Shutdown()
		{
			var req = this.Alloc(uv_req_type.UV_SHUTDOWN);

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
			if (status != 0)
			{
				var error = this.Loop.GetLastError();
			}

			this.Free(req);
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

		#region Buffer management methods
		internal uv_buf_t CreateBuffer(byte[] data, int offset, int length)
		{
			var ret = this.CreateBuffer((uint)length);
			Marshal.Copy(data, offset, ret.data, length);
			return ret;
		}

		internal uv_buf_t CreateBuffer(uint size)
		{
			return this.CreateBuffer(this.Loop.BufferManager.Alloc((int)size), size);
		}

		internal uv_buf_t CreateBuffer(IntPtr data, uint size)
		{
			//return Uvi.uv_buf_init(data, size);
			return new uv_buf_t()
			{
				data = data,
#if __MonoCS__
				len = (IntPtr)size
#else
				len = (IntPtr)size
#endif
			};
		}

		internal void DeleteBuffer(uv_buf_t buffer)
		{
			if (buffer.data != IntPtr.Zero)
				buffer.data = this.Loop.BufferManager.Free(buffer.data);
		}
		#endregion
	}
}
