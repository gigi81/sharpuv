using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Libuv;

namespace SharpUV
{
	internal class RequestCollection
	{
		private readonly Loop _loop;
		private readonly BufferCollection _buffer;
		private readonly Dictionary<IntPtr, uv_buf_t> _writes = new Dictionary<IntPtr, uv_buf_t>();

		internal RequestCollection(Loop loop, BufferCollection buffer)
		{
			_loop = loop;
			_buffer = buffer;
		}

		/// <summary>
		/// Creates a request that do not need a buffer
		/// </summary>
		/// <param name="reqType"></param>
		/// <returns></returns>
		internal IntPtr Create(uv_req_type reqType)
		{
			return this.Create(reqType, BufferCollection.EmptyBuffer);
		}

		/// <summary>
		/// Creates a request that needs to allocate a buffer of the specified size
		/// </summary>
		/// <param name="reqType"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		internal IntPtr Create(uv_req_type reqType, uint length)
		{
			return Create(reqType, _buffer.CreateBuffer(length));
		}

		/// <summary>
		/// Creates a request that needs to allocate a buffer and copy data from the specified buffer
		/// </summary>
		/// <param name="reqType"></param>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		internal IntPtr Create(uv_req_type reqType, byte[] data, int offset, int length)
		{
			return Create(reqType, _buffer.CreateBuffer(data, offset, length));
		}

		private IntPtr Create(uv_req_type reqType, uv_buf_t buffer)
		{
			var requestHandle = _loop.Allocs.AllocRequest(reqType);

			uv_req_t request = new uv_req_t()
			{
				type = reqType,
				data = buffer.data
			};

			Marshal.StructureToPtr(request, requestHandle, false);

			_writes.Add(requestHandle, buffer);
			return requestHandle;
		}

		internal IntPtr Delete(IntPtr requestHandle)
		{
			if (requestHandle == IntPtr.Zero)
				return requestHandle;

			_buffer.DeleteBuffer(_writes[requestHandle]);
			_writes.Remove(requestHandle);
			return _loop.Allocs.FreeRequest(requestHandle);
		}

		internal byte[] CopyAndDelete(IntPtr requestHandle, int size)
		{
			if (requestHandle == IntPtr.Zero)
				return new byte[0];

			var data = _buffer.CopyAndDeleteBuffer(_writes[requestHandle], size);
			_loop.Allocs.FreeRequest(requestHandle);
			_writes.Remove(requestHandle);
			return data;
		}

		internal uv_buf_t this[IntPtr ptr]
		{
			get { return _writes[ptr]; }
		}
	}
}
