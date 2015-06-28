using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Libuv
{
	public sealed class OffT
	{
		public long Value { get; set; }

		public OffT(long value)
		{
			this.Value = value;
		}

		[SuppressMessage("Microsoft.Usage", "CA2225")]
		public static implicit operator OffT(int value)
		{
			return new OffT(value);
		}

		[SuppressMessage("Microsoft.Usage", "CA2225")]
		public static implicit operator int(OffT value)
		{
			return (int)value.Value;
		}

		[SuppressMessage("Microsoft.Usage", "CA2225")]
		public static implicit operator OffT(long value)
		{
			return new OffT(value);
		}

		[SuppressMessage("Microsoft.Usage", "CA2225")]
		public static implicit operator long(OffT value)
		{
			return value.Value;
		}
	}

	public sealed class OffTMarshaler : ICustomMarshaler
	{
		[SuppressMessage("Microsoft.Usage", "CA1801", Justification = "data parameter is a hidden requirement of the API")]
		public static ICustomMarshaler GetInstance(string data)
		{
			return new OffTMarshaler();
		}

		#region ICustomMarshaler Members

		public void CleanUpManagedData(object ManagedObj)
		{
			// Nothing to do
		}

		public void CleanUpNativeData(IntPtr pNativeData)
		{
            // Nothing to do
		}

		public int GetNativeDataSize()
		{
			return IntPtr.Size;
		}

		public IntPtr MarshalManagedToNative(object ManagedObj)
		{
			OffT value = (OffT)ManagedObj;

			checked //enables overflows exceptions
			{
				if (IntPtr.Size == 4)
					return (IntPtr)value.Value;

				if (IntPtr.Size == 8)
					return (IntPtr)value.Value;

				throw new ArgumentException("Invalid Pointer Size");
			}
		}

		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			if (IntPtr.Size == 4)
				return new OffT(pNativeData.ToInt32());

			if (IntPtr.Size == 8)
				return new OffT(pNativeData.ToInt64());

			throw new ArgumentException("Invalid Pointer Size");
		}
		#endregion
	}
}
