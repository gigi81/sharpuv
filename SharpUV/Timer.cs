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
using System.Text;
using Libuv;

namespace SharpUV
{
	public class Timer : UvHandle
	{
		private TimeSpan _repeat;

		public Timer()
			: this(Loop.Default)
		{
		}

		public Timer(Loop loop)
			: base(loop, uv_handle_type.UV_TIMER)
		{
			CheckError(Uvi.uv_timer_init(this.Loop.Handle, this.Handle));
		}

		public TimeSpan Delay { get; set; }

		public TimeSpan Repeat
		{
			get { return _repeat; }
			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentException("value");

				CheckError(Uvi.uv_timer_set_repeat(this.Handle, value.TotalMilliseconds));
				_repeat = value;
			}
		}

		public void Start()
		{
			CheckError(Uvi.uv_timer_start(this.Handle, this.OnTick, this.Delay.TotalMilliseconds, this.Repeat.TotalMilliseconds));
		}

		public void Stop()
		{
			CheckError(Uvi.uv_timer_stop(this.Handle));
		}

		public void Again()
		{
			CheckError(Uvi.uv_timer_again(this.Handle));
		}

		private void OnTick(IntPtr watcher, int status)
		{
			this.OnTick();
		}

		protected virtual void OnTick()
		{
		}
	}
}
