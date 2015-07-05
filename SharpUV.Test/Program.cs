using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using SharpUV;

namespace SharpUV.Test
{
	class Program
	{
		static readonly IPEndPoint ServerEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 10000);

		static TcpEchoServer server;

		static void Main(string[] args)
		{
			TestServer ();
            Console.WriteLine("Done. Press any key to exit...");
			Console.ReadKey();
		}

		static void TestServer()
		{
			//create the server
			server = new TcpEchoServer();
			server.StartListening(ServerEndPoint);

			//create a pool of clients
			var pool = new EchoClientsPool(10, 128 * 1024, 1024);
			//set to true to verify the data transferred (if enabled will slow down transfer rate)
			pool.SkipCheck = true;
			pool.Completed += pool_Completed;
			pool.Start();

			//start the server and 
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			server.Loop.Run();
			stopWatch.Stop();

			pool.ToString();
			server.ToString();

			var total = pool.TotalBytes;

			server.Dispose();
			pool.Dispose();

			Loop.Current.Run();

			GC.Collect();

			//seems like the garbage collector is 
			Thread.Sleep(2000);

			Console.WriteLine("Allocated memory: {0}", Loop.Current.AllocatedBytes);
			Console.WriteLine("Handles not deallocated: {0}", Loop.Current.AllocatedHandles);
			Console.WriteLine("Pending loop works: {0}", Loop.Current.PendingWorks);
			Console.WriteLine("Transferred data {0} MB", total / (1024 * 1024));
			Console.WriteLine("Total time {0} seconds", stopWatch.Elapsed.TotalSeconds);
			Console.WriteLine("Performance: {0} MB/s", (int)(total / stopWatch.Elapsed.TotalSeconds / (1024 * 1024)));
		}

		static void pool_Completed(object sender, EventArgs e)
		{
			server.Close();
		}
	}
}
