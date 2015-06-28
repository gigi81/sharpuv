using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpUV
{
	internal class LoopAllocs
	{
		private readonly Dictionary<IntPtr, ulong> _allocs = new Dictionary<IntPtr, ulong>();
		private ulong _allocated = 0;
		private ulong _deallocated = 0;

		internal IntPtr Alloc(int size)
		{
			return this.Alloc((ulong)size);
		}

		internal IntPtr Alloc(ulong size)
		{
			var ret = Marshal.AllocHGlobal((int)size);
			_allocs.Add(ret, size);
			_allocated += size;
			return ret;
		}

		internal IntPtr Free(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return ptr;

			_deallocated += _allocs[ptr];
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
	}
}
