using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SharpUV.NUnit
{
	[TestFixture]
	public class WorkTests
	{
		[Test]
		public void ExecuteWork()
		{
			bool run = false;

			Loop.Default.QueueWork(() => { run = true; });
			Assert.AreEqual(1, Loop.Default.PendingWorks);

			Loop.Default.Run();

			Assert.AreEqual(true, run);
			Assert.AreEqual(0, Loop.Default.PendingWorks);
		}

		[Test]
		public void ExecuteWorkAndAfter()
		{
			bool run = false, after = false;

			Loop.Default.QueueWork(() => { run = true; }, () => { after = true; });
			Assert.AreEqual(1, Loop.Default.PendingWorks);

			Loop.Default.Run();

			Assert.AreEqual(true, run);
			Assert.AreEqual(true, after);
			Assert.AreEqual(0, Loop.Default.PendingWorks);
		}
	}
}
