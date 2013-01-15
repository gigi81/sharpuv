using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using SharpUV;

namespace SharpUV.Test
{
	class Program
	{
		static IPEndPoint ServerEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 10000);

		static void Main(string[] args)
		{
			var server = new TcpEchoServer();
			server.StartListening(ServerEndPoint);
			server.Loop.Run();

			server = null;
			System.GC.Collect();

			Console.WriteLine("Memory report: allocated {0}, deallocated {1}", Loop.Default.AllocatedBytes, Loop.Default.DeAllocatedBytes);
			Console.ReadKey();
		}
	}
}
