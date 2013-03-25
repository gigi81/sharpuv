using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Libuv;

namespace SharpUV
{
	internal class BufferManager : IDisposable
	{
		private List<BufferManagerItem> _usedBuffers = new List<BufferManagerItem>();
		private List<BufferManagerItem> _freeBuffers = new List<BufferManagerItem>();

		internal IntPtr Alloc(int size)
		{
			IntPtr ret = this.FindFree(size);
			if (ret != IntPtr.Zero)
				return ret;

			ret = Marshal.AllocHGlobal(size);
			_usedBuffers.Add(new BufferManagerItem(ret, size));
			return ret;
		}

		internal IntPtr Free(IntPtr buffer)
		{
			var ret = _usedBuffers.Find(i => i.Data == buffer);
			_usedBuffers.Remove(ret);
			_freeBuffers.Add(ret);

			return IntPtr.Zero;
		}

		private IntPtr FindFree(int size)
		{
			for (int i = 0; i < _freeBuffers.Count; i++)
			{
				if (_freeBuffers[i].Size < size)
					continue;

				var ret = _freeBuffers[i];
				_usedBuffers.Add(ret);
				_freeBuffers.Remove(ret);
				return ret.Data;
			}

			return IntPtr.Zero;
		}
	
		#region IDisposable Members

		public void Dispose()
		{
			foreach (var buffer in _freeBuffers)
				Marshal.FreeHGlobal(buffer.Data);

			foreach (var buffer in _usedBuffers)
				Marshal.FreeHGlobal(buffer.Data);

			_freeBuffers = new List<BufferManagerItem>();
			_usedBuffers = new List<BufferManagerItem>();
		}

		#endregion
	}

	internal struct BufferManagerItem
	{
		private IntPtr _data;
		private int _size;

		public BufferManagerItem(IntPtr data, int size)
		{
			_data = data;
			_size = size;
		}

		internal IntPtr Data { get { return _data; } }

		internal int Size { get { return _size; } }

		public override int GetHashCode()
		{
			return this.Data.GetHashCode();
		}
	}
}
