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

		private BufferCollection _buffers;
		private RequestCollection _requests;

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
			CheckError(Uvi.uv_run(this.Handle, Uvi.uv_run_mode.UV_RUN_DEFAULT));
		}

		/// <summary>
		/// This function polls for new events without blocking
		/// </summary>
		public void RunOnce()
		{
			CheckError(Uvi.uv_run(this.Handle, Uvi.uv_run_mode.UV_RUN_ONCE));
		}

		public void CheckError(int code)
		{
			if (code < 0)
				throw new UvException(code);
		}

		internal IntPtr Handle { get; private set; }

		/// <summary>
		/// The Loop for the current thread
		/// </summary>
		public static Loop Current
		{
			get { return LoopsCollection.CurrentLoop; }
		}

        public void QueueWork(Action run, Action after = null)
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

        public ulong AllocatedBytes { get { return this.Allocs.AllocatedMemory; } }

		public uint AllocatedHandles { get { return this.Allocs.AllocatedHandles; } }

        public int PendingWorks { get { return this.Works.Count; } }

        public void DumpAllocs()
        {
            this.Allocs.DumpAllocs();
        }

		internal BufferCollection Buffers
		{
			get { return _buffers ?? (_buffers = new BufferCollection(this)); }
		}

		internal RequestCollection Requests
		{
			get { return _requests ?? (_requests = new RequestCollection(this, this.Buffers)); }
		}
    }
}
