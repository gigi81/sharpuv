using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpUV.Test
{
	public class TcpEchoServer : TcpServer
	{
		protected override TcpServerSocket CreateClientSocket()
		{
			return new TcpEchoServerSocket(this);
		}

		class TcpEchoServerSocket : TcpServerSocket
		{
			public TcpEchoServerSocket(TcpEchoServer server)
				: base(server)
			{
				this.ReadStart();
				Console.WriteLine("Client connected");
			}

			protected override void OnRead(byte[] data)
			{
				this.Write(data);
				//this.ProcessCommand(Encoding.UTF8.GetString(data));
			}

			private void ProcessCommand(string command)
			{
				if (String.IsNullOrWhiteSpace(command))
					return;

				if (command.Length == 4 && command.Substring(0, 4).Equals("quit", StringComparison.InvariantCultureIgnoreCase))
					this.Close();
				if (command.Length == 7 && command.Substring(0, 7).Equals("quitall", StringComparison.InvariantCultureIgnoreCase))
					this.Server.Close();
			}

			protected override void OnClose()
			{
				base.OnClose();
				Console.WriteLine("Client disconnected");
			}
		}
	}
}
