using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpUV.Test
{
	public class TcpEchoServer : TcpServer
	{
		protected override HttpServerSocket CreateClientSocket()
		{
			return new TcpEchoServerSocket(this);
		}

		class TcpEchoServerSocket : HttpServerSocket
		{
			public TcpEchoServerSocket(TcpEchoServer server)
				: base(server)
			{
				this.Write(Encoding.UTF8.GetBytes("Echo Server (type quit to close)>\r\n"));
				this.ReadStart();
			}

			protected override void OnRead(byte[] data)
			{
				var value = Encoding.UTF8.GetString(data);

				if (value.Replace("\r", "").Replace("\n", "").Equals("quit", StringComparison.InvariantCultureIgnoreCase))
					this.Close();
				else
					this.Write(data);
			}

			protected override void OnClose()
			{
				base.OnClose();

				//we close the server connection
				this.Server.Close();
			}
		}
	}
}
