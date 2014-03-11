using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Libuv;

namespace SharpUV
{
    internal class BufferCollection
    {
        internal static readonly uv_buf_t EmptyBuffer = Uvi.uv_buf_init(IntPtr.Zero, 0);

        private readonly Loop _loop;

        internal BufferCollection(Loop loop)
        {
            _loop = loop;
        }

        internal uv_buf_t CreateBuffer(byte[] data, int offset, int length)
        {
            var ret = this.CreateBuffer((uint)length);
            Marshal.Copy(data, offset, ret.data, length);
            return ret;
        }

        internal void AllocBuffer(IntPtr buf, uint size)
        {
            var data = this.CreateBuffer(size);
            Marshal.StructureToPtr(data, buf, fDeleteOld: false);
        }

        internal uv_buf_t CreateBuffer(uint size)
        {
            return this.CreateBuffer(_loop.BufferManager.Alloc((int)size), size);
        }

        internal uv_buf_t CreateBuffer(IntPtr data, uint size)
        {
            return Uvi.uv_buf_init(data, size);
        }

        internal void DeleteBuffer(uv_buf_t buffer)
        {
            if (buffer.data != IntPtr.Zero)
                buffer.data = _loop.BufferManager.Free(buffer.data);
        }

        internal byte[] CopyAndDeleteBuffer(IntPtr buf, int size)
        {
            var data = (uv_buf_t) Marshal.PtrToStructure(buf, typeof (uv_buf_t));
            return CopyAndDeleteBuffer(data, size);
        }

        internal byte[] CopyAndDeleteBuffer(uv_buf_t buf, int size)
        {
            byte[] data = new byte[size > 0 ? size : 0];

            if (size > 0)
                Marshal.Copy(buf.data, data, 0, size);

            this.DeleteBuffer(buf);
            return data;
        }
    }

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

        internal IntPtr Create(uv_req_type reqType, byte[] data, int offset, int length)
        {
            var requestHandle = _loop.Allocs.Alloc(Uvi.uv_req_size(reqType));
            var buffer = _buffer.CreateBuffer(data, offset, length);

            uv_req_t request = new uv_req_t()
            {
                type = reqType,
                data = buffer.data
            };

            Marshal.StructureToPtr(request, requestHandle, false);

            _writes.Add(requestHandle, buffer);
            return requestHandle;
        }

        internal void Delete(IntPtr requestHandle)
        {
            _buffer.DeleteBuffer(_writes[requestHandle]);
            _loop.Allocs.Free(requestHandle);
            _writes.Remove(requestHandle);
        }

        internal uv_buf_t this[IntPtr ptr]
        {
            get { return _writes[ptr]; }
        }
    }
}
