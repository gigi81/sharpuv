#region License
/**
 * Copyright (c) 2012 Luigi Grilli
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute,
 * sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using Libuv;

namespace SharpUV
{
	public class TcpServer : UvHandle
	{
		public const int DefaultBackLog = 128;

		private readonly List<TcpServerSocket> _clients = new List<TcpServerSocket>();
	    private IntPtr _address = IntPtr.Zero;

		public TcpServer()
			: this(Loop.Default)
		{
		}

		public TcpServer(Loop loop)
			: base(loop, uv_handle_type.UV_TCP)
		{
			this.BackLog = DefaultBackLog;
			CheckError(Uvi.uv_tcp_init(this.Loop.Handle, this.Handle));
		}

		#region Delegates
		private uv_connection_cb _connectionDelegate;

		protected override void InitDelegates()
		{
			base.InitDelegates();
			_connectionDelegate = new uv_connection_cb(this.OnClientConnected);
		}

		#endregion

		public int BackLog { get; set; }

		public int ConnectedClients { get { return _clients.Count; } }

		public void StartListening(IPEndPoint endpoint)
		{
		    try
		    {
		        _address = TcpSocket.AllocSocketAddress(endpoint, this.Loop);
                CheckError(Uvi.uv_tcp_bind(this.Handle, _address, 0));
                CheckError(Uvi.uv_listen(this.Handle, this.BackLog, _connectionDelegate));
		    }
		    catch (Exception)
		    {
		        _address = Free(_address);
		    }
		}

		private void OnClientConnected(IntPtr server, int status)
		{
			var client = this.CreateClientSocket();
			client.Closed += this.OnClientClosed;
			_clients.Add(client);
		}

		private void OnClientClosed(object sender, EventArgs e)
		{
			var client = (TcpServerSocket) sender;
			_clients.Remove(client);
		}

		protected virtual TcpServerSocket CreateClientSocket()
		{
			return new TcpServerSocket(this);
		}

		protected override void OnClose()
		{
			foreach (var client in _clients)
			{
				try
				{
					client.Shutdown();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Client shutdown returned error: {0}", ex.Message);
				}
			}

			base.OnClose();
            _address = Free(_address);
		}
	}
}
