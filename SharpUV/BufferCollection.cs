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
}
