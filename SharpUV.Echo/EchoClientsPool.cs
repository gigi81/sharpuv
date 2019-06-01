using System;

namespace SharpUV.Echo
{
	class EchoClientsPool : IDisposable
	{
		public event EventHandler Completed;

		private readonly EchoClient[] _clients;
		private int _closed = 0;

		public EchoClientsPool(int size, int packetSize, int total)
		{
			_clients = new EchoClient[size];

			for (int i = 0; i < _clients.Length; i++)
			{
				_clients[i] = new EchoClient(packetSize, total);
				_clients[i].Closed += ClientClosed;
			}

			this.TotalBytes = size * packetSize * total;
		}

		public int TotalBytes { get; private set; }

	    public bool SkipCheck
	    {
	        get { return _clients[0].SkipCheck; }
            set
            {
                for (int i = 0; i < _clients.Length; i++)
                    _clients[i].SkipCheck = value;
            }
	    }

		private void ClientClosed(object sender, EventArgs e)
		{
			if (++_closed >= _clients.Length)
				this.OnCompleted();
		}

		public void Start()
		{
			for (int i = 0; i < _clients.Length; i++)
				_clients[i].Run();
		}

		public void Close(bool dispose = false)
		{
			for (int i = 0; i < _clients.Length; i++)
                _clients[i].Close(dispose);
		}

		protected virtual void OnCompleted()
		{
			if (this.Completed != null)
				this.Completed(this, EventArgs.Empty);
		}

        #region IDisposable Members

        public void Dispose()
        {
            for (int i = 0; i < _clients.Length; i++)
            {
                if (_clients[i] == null)
                    continue;

                _clients[i].Dispose();
                _clients[i] = null;
            }
        }

        #endregion
    }
}
