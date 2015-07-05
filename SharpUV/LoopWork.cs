using Libuv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpUV
{
	internal sealed class LoopWork : IDisposable
	{
		private readonly uv_work_cb _run;
		private readonly uv_after_work_cb _after;
		private readonly Action _runAction;
		private readonly Action _afterAction;
		private readonly Action<LoopWork> _completed;
		private readonly Loop _loop;
		private readonly IntPtr _work;

		private bool _disposed = false;

		public LoopWork(Loop loop, Action run, Action after, Action<LoopWork> completed)
		{
			_run = new uv_work_cb(this.Run);
			_after = new uv_after_work_cb(this.After);
			_runAction = run;
			_afterAction = after;
			_completed = completed;
			_loop = loop;
			_work = _loop.Requests.Create(uv_req_type.UV_WORK);

			try
			{
				_loop.CheckError(Uvi.uv_queue_work(_loop.Handle, _work, _run, _after));
			}
			catch (Exception)
			{
				_work = _loop.Requests.Delete(_work);
				throw;
			}
		}

		private void Run(IntPtr work)
		{
			if (_runAction != null)
				_runAction();
		}

		private void After(IntPtr work, int status)
		{
			try
			{
				if (_afterAction != null)
					_afterAction();
			}
			finally
			{
				_completed(this);
				this.Dispose();
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_disposed)
				return;

			_loop.Requests.Delete(_work);
			_disposed = true;
		}

		#endregion
	}
}
