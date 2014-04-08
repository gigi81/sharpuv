using System;

namespace SharpUV
{
	internal abstract class UvCallback<TArgs> where TArgs : UvArgs
	{
		protected Action<TArgs> _callback;

		protected UvCallback(Action<TArgs> callback)
		{
			_callback = callback;
		}

		protected abstract TArgs CreateArgs(int code);

		public void Invoke(int code, Action<TArgs> callback)
		{
			var args = this.CreateArgs(code);

			if (callback != null)
				callback.Invoke (args);

			if(_callback != null)
				_callback.Invoke (args);
		}
	}

	internal class UvCallback : UvCallback<UvArgs>
	{
		internal UvCallback(Action<UvArgs> callback)
			: base(callback)
		{
		}

		protected override UvArgs CreateArgs (int code)
		{
			return new UvArgs(code);
		}
	}

	internal class UvDataCallback : UvCallback<UvDataArgs>
	{
		private byte[] _data;

		internal UvDataCallback(Action<UvDataArgs> callback, byte[] data)
			: base(callback)
		{
			_data = data;
		}

		protected override UvDataArgs CreateArgs (int code)
		{
			return new UvDataArgs (code, _data);
		}
	}
}

