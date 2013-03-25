using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpUV.Test
{
	class EchoClientsPool
	{
		public event EventHandler Completed;

		private EchoClient[] _clients;
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

		public void Close()
		{
			for (int i = 0; i < _clients.Length; i++)
				_clients[i].Close();
		}

		protected virtual void OnCompleted()
		{
			if (this.Completed != null)
				this.Completed(this, EventArgs.Empty);
		}
	}
}
