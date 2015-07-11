using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Libuv;
using SharpUV.Callbacks;

namespace SharpUV
{
	public class TcpClientSocket : TcpSocket
	{
		public event EventHandler<UvArgs<IPEndPoint[]>> Resolved;
		public event EventHandler<UvArgs> Connected;

		private IntPtr _connectionReq = IntPtr.Zero;
		private IntPtr _resolveReq = IntPtr.Zero;
		private IntPtr _address = IntPtr.Zero;

		public TcpClientSocket()
			: this(Loop.Current)
		{
		}

		public TcpClientSocket(Loop loop)
			: base(loop)
		{
			_connectionReq = this.Loop.Requests.Create(uv_req_type.UV_CONNECT);
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
		internal IntPtr Connection { get { return _connectionReq; } }

		private UvEndPointsCallback _resolveCallback;

		public void Resolve(string node, string service, Action<UvArgs<IPEndPoint[]>> callback = null)
		{
			var hints = addrinfo.CreateHints();
			var hintsPtr = this.Loop.Allocs.Alloc(Marshal.SizeOf(typeof(addrinfo)));
			Marshal.StructureToPtr(hints, hintsPtr, fDeleteOld: false);

			try
			{
				_resolveReq = this.Loop.Requests.Create(uv_req_type.UV_GETADDRINFO);
				CheckError(Uvi.uv_getaddrinfo(this.Loop.Handle, _resolveReq, _resolveDelegate, node, service, hintsPtr));
				this.Status = HandleStatus.Resolving;
				_resolveCallback = new UvEndPointsCallback(this, callback);
			}
			catch (Exception)
			{
				this.Loop.Requests.Delete(_resolveReq);
				_connectCallback = null;
				throw;
			}
			finally
			{
				this.Loop.Allocs.Free(hintsPtr);
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
				this.Loop.Requests.Delete(_resolveReq);
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
				_address = this.Loop.Allocs.Free(_address);
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
			if (!this.IsDisposed)
			{
				_connectionReq = this.Loop.Requests.Delete(_connectionReq);
			}

			base.Dispose(disposing);
			_address = this.Loop.Allocs.Free(_address);
		}
	}
}
