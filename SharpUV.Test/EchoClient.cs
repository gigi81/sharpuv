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

		public EchoClient(int packetSize, int total)
		{
			this.PacketSize = packetSize;
			this.PacketsTotal = total;
		}

		public int PacketSize { get; private set; }

		public int PacketSents { get; private set; }

		public int PacketsTotal { get; private set; }

		public bool Completed
		{
			get { return this.PacketSents >= this.PacketsTotal; }
		}

		public void Run()
		{
			this.Checker = new DataChecker();
			this.Connect(ServerEndPoint);
		}

		public void SendPacket()
		{
			try
			{
				//Console.WriteLine("Client SendPacket {0}", ++_packets);
				var data = this.CreateRandom(PacketSize);
				this.Write(data);
				this.Checker.Sent(data);
				this.PacketSents++;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error sending packet {0}", ex.Message);
			}
		}

		protected override void OnConnect()
		{
			base.OnConnect();
			if (this.Status == TcpClientSocketStatus.Connected)
			{
				this.SendPacket();
			}
			else
			{
				this.Close();
			}
		}

		protected override void OnRead(byte[] data)
		{
			//Console.WriteLine("Client Received echo ({0} bytes)", data.Length);

			base.OnRead(data);
			this.Checker.Received(data);

			if (!this.Checker.Check())
			{
				//error!!!
			}
			else
			{
				this.Checker.Flush();

				if (this.Checker.IsEmpty && this.Completed)
				{
					Console.WriteLine("bytes limit reached");
					this.Close();
				}
				else if (this.Checker.IsEmpty)
				{
					this.SendPacket();
				}
			}
		}

		protected override void OnWrite()
		{
			base.OnWrite();

			if(!this.IsReading)
				this.ReadStart();
		}

		protected override void OnClose()
		{
			base.OnClose();
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
