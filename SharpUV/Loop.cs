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
using System.Runtime.InteropServices;
using System.Text;
using Libuv;

namespace SharpUV
{
	public class Loop : IDisposable
	{
        private static readonly Loop DefaultLoop = new Loop(Uvi.uv_default_loop());

		internal readonly BufferManager BufferManager = new BufferManager();
        internal readonly LoopAllocs Allocs = new LoopAllocs();
        internal readonly List<LoopWork> Works = new List<LoopWork>();

		public Loop()
			: this(Uvi.uv_loop_new())
		{
		}

		protected Loop(IntPtr handle)
		{
			this.Handle = handle;
		}

		~Loop()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// This function starts the event loop. It blocks until the reference count
		/// of the loop drops to zero.
		/// </summary>
		public void Run()
		{
			CheckError(Uvi.uv_run(this.Handle));
		}

		/// <summary>
		/// This function polls for new events without blocking
		/// </summary>
		public void RunOnce()
		{
			CheckError(Uvi.uv_run_once(this.Handle));
		}

		public void CheckError(int code)
		{
			if (code != 0)
				throw new UvException(this.GetLastError());
		}

		public Error GetLastError()
		{
			return new Error(Uvi.uv_last_error(this.Handle));
		}

		public IntPtr Handle { get; private set; }

		public static Loop Default
		{
			get { return DefaultLoop; }
		}

        public void QueueWork(Action run, Action after)
        {
            this.Works.Add(new LoopWork(this, run, after, this.WorkCompleted));
        }

        private void WorkCompleted(LoopWork work)
        {
            this.Works.Remove(work);
        }

		#region Dispose Management
		/// <summary>
		/// Indicates if the object has been disposed
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Dispose the object freeing unmanaged resources allocated
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.IsDisposed)
				return;

			//we don't delete the default's loop handle
			if (this.Handle != DefaultLoop.Handle)
			{
				if (this.Handle != IntPtr.Zero)
				{
					Uvi.uv_loop_delete(this.Handle);
					this.Handle = IntPtr.Zero;
				}
			}

			GC.SuppressFinalize(this);
			this.IsDisposed = true;
		}
		#endregion

        public ulong AllocatedBytes { get { return this.Allocs.AllocatedBytes; } }

        public ulong DeAllocatedBytes { get { return this.Allocs.DeAllocatedBytes; } }

        public int PendingWorks { get { return this.Works.Count; } }

        public void DumpAllocs()
        {
            this.Allocs.DumpAllocs();
        }
    }

    internal class LoopAllocs
    {
        private readonly Dictionary<IntPtr, ulong> _allocs = new Dictionary<IntPtr, ulong>();
        private ulong _allocated = 0;
        private ulong _deallocated = 0;

        internal IntPtr Alloc(int size)
        {
            return this.Alloc((ulong) size);
        }

        internal IntPtr Alloc(ulong size)
        {
            var ret = Marshal.AllocHGlobal((int)size);
            _allocs.Add(ret, size);
            _allocated += size;
            return ret;
        }

        internal IntPtr Free(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return ptr;

            _deallocated += _allocs[ptr];
            _allocs.Remove(ptr);

            Marshal.FreeHGlobal(ptr);

            return IntPtr.Zero;
        }

        public void DumpAllocs()
        {
            foreach (var alloc in _allocs)
            {
                Console.WriteLine("allocated {0}", alloc.Value);
            }
        }

        public ulong AllocatedBytes { get { return _allocated; } }

        public ulong DeAllocatedBytes { get { return _deallocated; } }
    }

    internal class LoopWork : IDisposable
    {
        private readonly uv_work_cb _run;
        private readonly uv_after_work_cb _after;
        private readonly Action _runAction;
        private readonly Action _afterAction;
        private readonly Action<LoopWork> _completed;
        private readonly Loop _loop;
        private readonly IntPtr _work;

        public LoopWork(Loop loop, Action run, Action after, Action<LoopWork> completed)
        {
            _run = new uv_work_cb(this.Run);
            _after = new uv_after_work_cb(this.After);
            _runAction = run;
            _afterAction = after;
            _completed = completed;
            _loop = loop;
            _work = _loop.Allocs.Alloc(Uvi.uv_req_size(uv_req_type.UV_WORK));

            try
            {
                _loop.CheckError(Uvi.uv_queue_work(_loop.Handle, _work, _run, _after));
            }
            catch (Exception)
            {
                _work = _loop.Allocs.Free(_work);
                throw;
            }
        }

        private void Run(IntPtr work)
        {
            if(_runAction != null)
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
            _loop.Allocs.Free(_work);
        }

        #endregion
    }
}
