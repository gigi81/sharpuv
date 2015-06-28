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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Libuv;
using SharpUV.Callbacks;

namespace SharpUV
{
	public class TcpClientSocket : TcpSocket
	{
		public event EventHandler<UvArgs<IPEndPoint[]>> Resolved;
		public event EventHandler<UvArgs> Connected;

		private IntPtr _resolveReq = IntPtr.Zero;
		private IntPtr _address = IntPtr.Zero;

		public TcpClientSocket()
			: this(Loop.Default)
		{
		}

		public TcpClientSocket(Loop loop)
			: base(loop)
		{
			this.Connection = this.Alloc(uv_req_type.UV_CONNECT);
		}

		#region Delegates
		private uv_getaddrinfo_cb _resolveDelegate;
		private uv_connect_cb _connectDelegate;

		protected override void InitDelegates()
		{
			base.InitDelegates();
			_connectDelegate = new uv_connect_cb(this.OnConnect);
			_resolveDelegate = new uv_getaddrinfo_cb(this.OnResolve);
		}

		#endregion

		/// <summary>
		/// Connection handle
		/// </summary>
		/// <remarks>Handle type is <typeparamref name="Libuv.uv_connect_t"/></remarks>
		internal IntPtr Connection { get; set; }

		private UvEndPointsCallback _resolveCallback;

		public void Resolve(string node, string service, Action<UvArgs<IPEndPoint[]>> callback = null)
		{
			var hints = addrinfo.CreateHints();
			var hintsPtr = this.Alloc(Marshal.SizeOf(typeof(addrinfo)));
			Marshal.StructureToPtr(hints, hintsPtr, fDeleteOld: false);

			try
			{
				_resolveReq = this.Alloc(uv_req_type.UV_GETADDRINFO);
				CheckError(Uvi.uv_getaddrinfo(this.Loop.Handle, _resolveReq, _resolveDelegate, node, service, hintsPtr));
				this.Status = HandleStatus.Resolving;
				_resolveCallback = new UvEndPointsCallback(this, callback);
			}
			catch (Exception)
			{
				this.Free(_resolveReq);
				_connectCallback = null;
				throw;
			}
			finally
			{
				this.Free(hintsPtr);
			}
		}

		private void OnResolve(IntPtr resolver, int status, IntPtr addrinfo)
		{
			var callback = _resolveCallback;
			_resolveCallback = null;

			try
			{
				IPEndPoint[] value = null;
				if (status == 0)
				{
					var info = ((addrinfo)Marshal.PtrToStructure(addrinfo, typeof(addrinfo)));
					value = info.EndPoints.ToArray();
				}

				callback.Invoke(status, value, this.OnResolve, this.Resolved);
			}
			finally
			{
				Uvi.uv_freeaddrinfo(addrinfo);
				this.Free(_resolveReq);
			}
		}

		protected virtual void OnResolve(UvArgs<IPEndPoint[]> args)
		{
		}

		private UvCallback _connectCallback;

		public void Connect(string ip, int port, Action<UvArgs> callback = null)
		{
			this.Connect(TcpSocket.AllocSocketAddress(new IPEndPoint(IPAddress.Parse(ip), port), this.Loop), callback);
		}

		public void Connect(IPEndPoint endpoint, Action<UvArgs> callback = null)
		{
			this.Connect(TcpSocket.AllocSocketAddress(endpoint, this.Loop), callback);
		}

		public void Connect(IntPtr address, Action<UvArgs> callback = null)
		{
			try
			{
				_address = address;
				CheckError(Uvi.uv_tcp_connect(this.Connection, this.Handle, _address, _connectDelegate));
				this.Status = HandleStatus.Opening;
				_connectCallback = new UvCallback(this, callback);
			}
			catch (Exception)
			{
				_address = Free(_address);
				_connectCallback = null;
				throw;
			}
		}

		private void OnConnect(IntPtr connection, int status)
		{
			var callback = _connectCallback;
			_connectCallback = null;

			this.Status = status == 0 ? HandleStatus.Open : HandleStatus.Closed;
			callback.Invoke(status, this.OnConnect, this.Connected);
		}

		protected virtual void OnConnect(UvArgs args)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if(!this.IsDisposed)
			{
				if (this.Connection != IntPtr.Zero)
					this.Connection = this.Free(this.Connection);
			}

			base.Dispose(disposing);

			_address = Free(_address);
		}
	}

	public class TcpServerSocket : TcpSocket
	{
		public TcpServerSocket(TcpServer server)
			: base(server.Loop)
		{
			this.Server = server;
			this.Accept();
			this.Status = HandleStatus.Open;
		}

		private void Accept()
		{
			CheckError(Uvi.uv_accept(this.Server.Handle, this.Handle));
		}

		protected TcpServer Server { get; private set; }
	}

	public class TcpSocket : UvStream
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="loop"></param>
		/// <remarks>Handle type is <typeparamref name="Libuv.uv_tcp_t"/> that is a subclass of <typeparamref name="Libuv.uv_stream_t"/></remarks>
		protected TcpSocket(Loop loop)
			: base(loop, uv_handle_type.UV_TCP)
		{
			CheckError(Uvi.uv_tcp_init(this.Loop.Handle, this.Handle));
		}

		internal static IntPtr AllocSocketAddress(IPEndPoint endpoint, Loop loop)
		{
			IntPtr ret;

			switch (endpoint.AddressFamily)
			{
				case AddressFamily.InterNetwork:
					ret = loop.Allocs.Alloc(Uvi.sockaddr_in_size);
					loop.CheckError(Uvi.uv_ip4_addr(endpoint.Address.ToString(), endpoint.Port, ret));
					break;

				case AddressFamily.InterNetworkV6:
					ret = loop.Allocs.Alloc(Uvi.sockaddr_in6_size);
					loop.CheckError(Uvi.uv_ip6_addr(endpoint.Address.ToString(), endpoint.Port, ret));
					break;

				default:
					throw new ArgumentException(String.Format("AddressFamily {0} not supported", endpoint.AddressFamily));
			}

			return ret;
		}
	}
}
