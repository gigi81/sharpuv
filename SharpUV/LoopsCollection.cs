using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpUV
{
	public static class LoopsCollection
	{
		private static readonly Dictionary<int, Loop> _loops = new Dictionary<int, Loop>();
		private static readonly ReaderWriterLock _lock = new ReaderWriterLock();

		internal static Loop CurrentLoop
		{
			get
			{
				var threadId = Thread.CurrentThread.ManagedThreadId;

				_lock.AcquireReaderLock(1000);

				try
				{
					Loop loop;
					if (_loops.TryGetValue(threadId, out loop))
						return loop;
				}
				finally
				{
					_lock.ReleaseReaderLock();
				}

				_lock.AcquireWriterLock(1000);

				try
				{
					Loop loop;
					if (_loops.TryGetValue(threadId, out loop))
						return loop;

					loop = new Loop();
					_loops.Add(threadId, loop);
					return loop;
				}
				finally
				{
					_lock.ReleaseWriterLock();
				}
			}
		}
	}
}
