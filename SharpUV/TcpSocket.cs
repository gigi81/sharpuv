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
