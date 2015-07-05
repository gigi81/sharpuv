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

			Loop.Current.QueueWork(() => { run = true; });
			Assert.AreEqual(1, Loop.Current.PendingWorks);

			Loop.Current.Run();

			Assert.AreEqual(true, run);
			Assert.AreEqual(0, Loop.Current.PendingWorks);
		}

		[Test]
		public void ExecuteWorkAndAfter()
		{
			bool run = false, after = false;

			Loop.Current.QueueWork(() => { run = true; }, () => { after = true; });
			Assert.AreEqual(1, Loop.Current.PendingWorks);

			Loop.Current.Run();

			Assert.AreEqual(true, run);
			Assert.AreEqual(true, after);
			Assert.AreEqual(0, Loop.Current.PendingWorks);
		}
	}
}
