using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Libuv;

namespace SharpUV
{
	public class TcpServerSocket : TcpSocket
	{
		private readonly TcpServer _server;

		public TcpServerSocket(TcpServer server)
			: base(server.Loop)
		{
			if (server == null)
				throw new ArgumentNullException("server");

			_server = server;

			this.Accept();
			this.Status = HandleStatus.Open;
		}

		private void Accept()
		{
			CheckError(Uvi.uv_accept(this.Server.Handle, this.Handle));
		}

		protected TcpServer Server { get { return _server; } }

		protected override void OnClose(UvArgs args)
		{
			this.Server.RemoveClient(this);
			base.OnClose(args);
		}
	}
}
