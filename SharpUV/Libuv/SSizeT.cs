using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Libuv
{
    public sealed class SSizeT
    {
        public long Value { get; set; }

        public SSizeT(long value)
        {
            this.Value = value;
        }

        [SuppressMessage("Microsoft.Usage", "CA2225")]
        public static implicit operator SSizeT(int value)
        {
            return new SSizeT(value);
        }

        [SuppressMessage("Microsoft.Usage", "CA2225")]
        public static implicit operator int(SSizeT value)
        {
            return (int)value.Value;
        }

        [SuppressMessage("Microsoft.Usage", "CA2225")]
        public static implicit operator SSizeT(long value)
        {
            return new SSizeT(value);
        }

        [SuppressMessage("Microsoft.Usage", "CA2225")]
        public static implicit operator long(SSizeT value)
        {
            return value.Value;
        }
    }

    public sealed class SSizeTMarshaler : ICustomMarshaler
    {
        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "data parameter is a hidden requirement of the API")]
        public static ICustomMarshaler GetInstance(string data)
        {
            return new SSizeTMarshaler();
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
            SSizeT value = (SSizeT)ManagedObj;

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
                return new SSizeT(pNativeData.ToInt32());

            if (IntPtr.Size == 8)
                return new SSizeT(pNativeData.ToInt64());

            throw new ArgumentException("Invalid Pointer Size");
        }
        #endregion
    }
}
