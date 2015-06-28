using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;

namespace SharpUV.NUnit
{
	[TestFixture]
	public class TimerTests
	{
		[Test]
		public void Delay()
		{
			//delay to test (3 seconds)
			var delay = new TimeSpan(0, 0, 0, 3 /* seconds */);
			//tollerance (actual time should be between 2.9 and 3.1 seconds)
			var tollerance = new TimeSpan(0, 0, 0, 0, 100 /* milliseconds */);

			Stopwatch stopwatch = new Stopwatch();
			Timer timer = new Timer();

			Loop.Default.QueueWork(() => {
				stopwatch.Start();
				timer.Start(delay, TimeSpan.Zero, () =>
				{
					stopwatch.Stop();
					timer.Stop();
					timer.Close();
				});
			});

			Loop.Default.Run();

			Assert.GreaterOrEqual(stopwatch.Elapsed, delay.Subtract(tollerance));
			Assert.Less(stopwatch.Elapsed, delay.Add(tollerance));
		}
	}
}
