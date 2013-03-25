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

		static TcpEchoServer server;

		static void Main(string[] args)
		{
			server = new TcpEchoServer();
			server.StartListening(ServerEndPoint);

			var pool = new EchoClientsPool(10, 32 * 1024, 64);
			pool.Completed += pool_Completed;
			pool.Start();

			var stopWatch = new Stopwatch();

			stopWatch.Start();
			server.Loop.Run();
			stopWatch.Stop();

			pool.ToString();
			server.ToString();

			var total = pool.TotalBytes;

			System.GC.Collect();

			Console.WriteLine("Memory report: allocated {0}, deallocated {1}", Loop.Default.AllocatedBytes, Loop.Default.DeAllocatedBytes);
			Console.WriteLine("Handles allocated {0}", UvHandle.AllocatedHandles);
			Console.WriteLine("Transferred data {0} MB", total / (1024 * 1024));
			Console.WriteLine("Total time {0} seconds", stopWatch.Elapsed.TotalSeconds);
			Console.WriteLine("Performance: {0} MB/s", (int)(total / stopWatch.Elapsed.TotalSeconds / (1024 * 1024)));
			Console.ReadKey();
		}

		static void pool_Completed(object sender, EventArgs e)
		{
			server.Close();
		}
	}
}
