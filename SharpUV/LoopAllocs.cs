using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Libuv;

namespace SharpUV
{
	internal class LoopAllocs
	{
		private readonly Dictionary<IntPtr, int> _allocs = new Dictionary<IntPtr, int>();
		private ulong _allocated = 0;
		private ulong _deallocated = 0;
		private uint _allocatedHandles = 0;

		internal IntPtr AllocHandle(uv_handle_type handleType)
		{
			var ret = Alloc(Uvi.uv_handle_size(handleType));
			_allocatedHandles++;
			return ret;
		}

		internal IntPtr AllocRequest(uv_req_type requestType)
		{
			return Alloc(Uvi.uv_req_size(requestType));
		}

		internal IntPtr Alloc(int size)
		{
			if (size <= 0)
				return IntPtr.Zero;

			var ret = Marshal.AllocHGlobal(size);
			_allocs.Add(ret, size);
			_allocated += (ulong)size;
			return ret;
		}

		internal IntPtr FreeHandle(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return ptr;

			var ret = this.Free(ptr);
			_allocatedHandles--;
			return ret;
		}

		internal IntPtr FreeRequest(IntPtr ptr)
		{
			return this.Free(ptr);
		}

		internal IntPtr Free(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return ptr;

			_deallocated += (ulong)_allocs[ptr];
			_allocs.Remove(ptr);

			Marshal.FreeHGlobal(ptr);

			return IntPtr.Zero;
		}

		public void DumpAllocs()
		{
			foreach (var alloc in _allocs)
			{
				Console.WriteLine("allocated {0}", alloc.Value);
			}
		}

		public ulong AllocatedBytes { get { return _allocated; } }

		public ulong DeAllocatedBytes { get { return _deallocated; } }

		/// <summary>
		/// The number of current allocated handles
		/// </summary>
		public uint CurrentlyAllocatedHandles { get { return _allocatedHandles; } }
	}
}
