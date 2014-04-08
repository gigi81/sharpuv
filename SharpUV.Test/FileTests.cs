using System;
using System.Text;

using SharpUV;

namespace SharpUV.Test
{
	public class FileTests
	{
		private static string TestFilePath
		{
			get{ return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.txt"); }
		}

		public void Open()
		{
			var handle = new WriteFileHandle();
			handle.Open(TestFilePath, FileAccessMode.WriteOnly, FileOpenMode.Create | FileOpenMode.Truncate, FilePermissions.S_IRUSR | FilePermissions.S_IWUSR);

			Loop.Default.Run();

			//Assert.AreEqual(System.IO.File.ReadAllText(path), "test");
		}

		internal class WriteFileHandle : FileHandle
		{
			protected override void OnOpen(UvArgs args)
			{
				args.Throw ();
				this.Write(Encoding.UTF8.GetBytes("test"));
			}

			protected override void OnWrite(UvDataArgs args)
			{
				args.Throw ();
				this.Close();
			}

			protected override void OnClose(UvArgs args)
			{
				args.Throw ();
			}
		}
	}
}

