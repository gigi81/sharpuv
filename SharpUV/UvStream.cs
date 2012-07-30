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

		public void ReadStart()
		{
			CheckError(Uvi.uv_read_start(this.Handle, OnAlloc, this.OnRead));
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
			return this.CreateBuffer(size);
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

			if(size < 0)
				this.Close();
		}

		public void Write(byte[] data)
		{
			this.Write(data, 0, data.Length);
		}

		public void Write(byte[] data, int offset, int length)
		{
			var requestHandle = this.Alloc(uv_req_type.UV_WRITE);
			uv_buf_t buffer = CreateBuffer(data, offset, length);
			uv_req_t request = new uv_req_t()
			{
				type = uv_req_type.UV_WRITE,
				data = buffer.data
			};

			Marshal.StructureToPtr(request, requestHandle, false);

			try
			{
				CheckError((Uvi.uv_write(requestHandle, this.Handle, new[] { buffer }, 1, this.OnWrite)));
				_writes.Add(requestHandle, buffer);
			}
			catch (Exception)
			{
				this.Free(requestHandle);
				this.DeleteBuffer(buffer);
				throw;
			}
		}

		private void OnWrite(IntPtr requestHandle, int status)
		{
			if (status != 0)
			{
				var error = this.Loop.GetLastError();
			}

			this.DeleteBuffer(_writes[requestHandle]);
			this.Free(requestHandle);
			_writes.Remove(requestHandle);
			this.OnWrite();
		}

		public void Shutdown()
		{
			var req = this.Alloc(uv_req_type.UV_SHUTDOWN);

			try
			{
				CheckError(Uvi.uv_shutdown(req, this.Handle, this.OnShutdown));
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
		internal uv_buf_t CreateBuffer(uint size)
		{
			return CreateBuffer(this.Alloc((int)size), size);
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

		internal uv_buf_t CreateBuffer(byte[] data, int offset, int length)
		{
			var ret = CreateBuffer((uint)length);
			Marshal.Copy(data, offset, ret.data, length);
			return ret;
		}

		internal void DeleteBuffer(uv_buf_t buffer)
		{
			if (buffer.data != IntPtr.Zero)
				buffer.data = this.Free(buffer.data);
		}
		#endregion
	}
}
