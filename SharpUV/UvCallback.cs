using System;
using System.Net;

namespace SharpUV
{
	internal class UvCallback<TArgs> where TArgs : UvArgs
	{
        protected readonly object _sender;
		protected Action<TArgs> _callback;

		protected UvCallback(object sender, Action<TArgs> callback)
		{
			_sender = sender;
			_callback = callback;
		}

	    internal Action<TArgs> Callback
	    {
            get { return _callback; }
	    }

		protected void Invoke(TArgs args, Action<TArgs> callback, EventHandler<TArgs> handler)
		{
			if (callback != null)
				callback.Invoke(args);

			if (_callback != null)
				_callback.Invoke(args);

			if (handler != null)
				handler(_sender, args);
		}
	}

	internal sealed class UvCallback : UvCallback<UvArgs>
	{
		internal UvCallback(object sender, Action<UvArgs> callback)
			: base(sender, callback)
		{
		}

		public void Invoke(int code, Action<UvArgs> callback, EventHandler<UvArgs> handler)
		{
			this.Invoke(new UvArgs(code), callback, handler);
		}
	}

	internal sealed class UvDataCallback : UvCallback<UvDataArgs>
	{
		private byte[] _data;

		internal UvDataCallback(object sender, Action<UvDataArgs> callback, byte[] data = null)
			: base(sender, callback)
		{
			_data = null;
		}

		public void Invoke(int code, Action<UvDataArgs> callback, EventHandler<UvDataArgs> handler)
		{
			base.Invoke(new UvDataArgs(code, _data), callback, handler);
		}

        public new void Invoke(UvDataArgs args, Action<UvDataArgs> callback, EventHandler<UvDataArgs> handler)
        {
			base.Invoke(args, callback, handler);
        }
	}

	internal sealed class UvStatCallback : UvCallback<UvStatArgs>
	{
		internal UvStatCallback(object sender, Action<UvStatArgs> callback)
			: base(sender, callback)
		{
		}

		public new void Invoke(UvStatArgs args, Action<UvStatArgs> callback, EventHandler<UvStatArgs> handler)
		{
			base.Invoke(args, callback, handler);
		}
	}
}
