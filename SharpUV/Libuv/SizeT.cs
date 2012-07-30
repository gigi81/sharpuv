using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Libuv
{
	public sealed class SizeT
	{
		public ulong Value { get; set; }

		public SizeT(ulong value)
		{
			this.Value = value;
		}

		[SuppressMessage("Microsoft.Usage", "CA2225")]
		public static implicit operator SizeT(uint value)
		{
			return new SizeT(value);
		}

		[SuppressMessage("Microsoft.Usage", "CA2225")]
		public static implicit operator uint(SizeT value)
		{
			return (uint)value.Value;
		}

		[SuppressMessage("Microsoft.Usage", "CA2225")]
		public static implicit operator SizeT(ulong value)
		{
			return new SizeT(value);
		}

		[SuppressMessage("Microsoft.Usage", "CA2225")]
		public static implicit operator ulong(SizeT value)
		{
			return value.Value;
		}
	}

	public sealed class SizeTMarshaler : ICustomMarshaler
	{
		[SuppressMessage("Microsoft.Usage", "CA1801", Justification = "data parameter is a hidden requirement of the API")]
		public static ICustomMarshaler GetInstance(string data)
		{
			return new SizeTMarshaler();
		}

		#region ICustomMarshaler Members

		public void CleanUpManagedData(object ManagedObj)
		{
			// Nothing to do
		}

		public void CleanUpNativeData(IntPtr pNativeData)
		{
			Marshal.FreeCoTaskMem(pNativeData);
		}

		public int GetNativeDataSize()
		{
			return IntPtr.Size;
		}

		public IntPtr MarshalManagedToNative(object ManagedObj)
		{
			SizeT value = (SizeT)ManagedObj;

			checked //enables overflows exceptions
			{
				if (IntPtr.Size == 4)
					return (IntPtr) value.Value;
				
				if (IntPtr.Size == 8)
					return (IntPtr)value.Value;

				throw new ArgumentException("Invalid Pointer Size");
			}
		}

		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			if (IntPtr.Size == 4)
				return new SizeT((uint)pNativeData.ToInt32());
			
			if (IntPtr.Size == 8)
				return new SizeT((ulong)pNativeData.ToInt64());

			throw new ArgumentException("Invalid Pointer Size");
		}
		#endregion
	}
}
