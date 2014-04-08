using System;

namespace SharpUV
{
	public class UvArgs
	{
		private int _error;
		private UvException _exception;

		public UvArgs (int error)
		{
			_error = error;
			if (_error < 0)
				_exception = new UvException (error);
		}

		public bool IsSuccesful
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
	}
}

