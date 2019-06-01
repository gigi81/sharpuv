using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace SharpUV.Tests
{
	public class WorkTests
	{
		[Fact]
		public void ExecuteWork()
		{
			bool run = false;

			Loop.Current.QueueWork(() => { run = true; });
			Assert.Equal(1, Loop.Current.PendingWorks);

			Loop.Current.Run();

            Assert.True(run);
			Assert.Equal(0, Loop.Current.PendingWorks);
		}

		[Fact]
		public void ExecuteWorkAndAfter()
		{
			bool run = false, after = false;

			Loop.Current.QueueWork(() => { run = true; }, () => { after = true; });
			Assert.Equal(1, Loop.Current.PendingWorks);

			Loop.Current.Run();

            Assert.True(run);
            Assert.True(after);
			Assert.Equal(0, Loop.Current.PendingWorks);
		}
	}
}
