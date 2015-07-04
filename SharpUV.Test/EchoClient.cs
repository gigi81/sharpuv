using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpUV.Test
{
	class EchoClient : TcpClientSocket
	{
		private static readonly Random Random = new Random();
		private static readonly IPEndPoint ServerEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 10000);

		public EchoClient(int packetSize, int total)
		{
			this.PacketSize = packetSize;
			this.PacketsTotal = total;
		}

		public int PacketSize { get; private set; }

		public int PacketSents { get; private set; }

		public int PacketsTotal { get; private set; }

        public bool SkipCheck { get; set; }

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

		protected override void OnConnect(UvArgs args)
		{
			if (this.Status == HandleStatus.Open)
				this.SendPacket();
			else
				this.Close();
		}

		protected override void OnRead(UvDataArgs args)
		{
            if(args.Successful)
            {
                this.Checker.Received(args.Data);
                if (this.SkipCheck)
                {
                    this.RunReceive();
                    this.AfterReceive();
                }
                else
                {
                    this.Loop.QueueWork(this.RunReceive, this.AfterReceive);
                }
            }
            else
            {
                this.Close();
            }
		}

	    private bool _lastCheck = false;

        private void RunReceive()
        {
            _lastCheck = this.SkipCheck || this.Checker.Check();
            this.Checker.Flush();
        }

        private void AfterReceive()
        {
            if (!_lastCheck)
            {
                Console.WriteLine("Data check failed");
            }
            else
            {
                if (this.Checker.IsEmpty && this.Completed)
                {
                    Console.WriteLine("transfer completed");
                    this.Close();
                }
                else if (this.Checker.IsEmpty)
                {
                    this.SendPacket();
                }
            }
        }

		protected override void OnWrite(UvDataArgs args)
		{
			if(!this.IsReading)
				this.ReadStart();
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
