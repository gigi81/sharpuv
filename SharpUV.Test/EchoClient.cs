using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpUV.Test
{
	class EchoClient : TcpClientSocket
	{
		private static Random Random = new Random();
		private static IPEndPoint ServerEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 10000);

		public void Run()
		{
			this.Checker = new DataChecker();
			this.Connect(ServerEndPoint);
		}

		protected override void OnConnect()
		{
			base.OnConnect();
			if (this.Status == TcpClientSocketStatus.Connected)
				this.SendPacket();
			else
				this.Close();
		}

		protected override void OnRead(byte[] data)
		{
			base.OnRead(data);
			this.Checker.Received(data);
		}

		protected override void OnWrite()
		{
			base.OnWrite();
		}

		protected override void OnClose()
		{
			base.OnClose();
		}

		private void SendPacket()
		{
			var data = this.CreateRandom(32);
			this.Write(data);
			this.Checker.Sent(data);
		}

		private byte[] CreateRandom(int size)
		{
			var data = new byte[size];
			Random.NextBytes(data);
			return data;
		}

		private DataChecker Checker { get; set; }
	}
}
