using System;
using System.Net;

namespace SharpUV
{
	public class UvArgs : EventArgs
	{
		public static readonly UvArgs UvEmpty = new UvArgs(0);

		private int _error;
		private UvException _exception;

		public UvArgs (int error)
		{
			_error = error;
			if (_error < 0)
				_exception = new UvException (error);
		}

		public bool Successful
		{
			get { return _error >= 0; }
		}

		public int Code
		{
			get { return _error; }
		}

		public UvException Exception
		{
			get { return _exception; }
		}

		public void Throw()
		{
			if (_exception != null)
				throw _exception;
		}
	}

    public class UvArgs<T> : UvArgs
    {
        private T _data;

        public UvArgs(int error, T data)
            : base(error)
        {
            _data = data;
        }

        public T Data
        {
            get { return _data; }
        }
    }

	public class UvDataArgs : UvArgs
	{
		private byte[] _data;

		public UvDataArgs (int error, byte[] data)
			: base(error)
		{
			_data = data;
		}

		public byte[] Data
		{
			get { return _data; }
		}

        public int Length
        {
            get { return this.Code; }
        }
	}

    public class UvStatArgs : UvArgs
    {
        private UvStat _stat;

		public UvStatArgs(int error, IntPtr stat)
			: this(error, UvStat.Create(stat))
		{
		}

		public UvStatArgs(int error, UvStat stat)
            : base(error)
        {
            _stat = stat;
        }

        public UvStat Stat
        {
            get { return _stat; }
        }
    }
}
