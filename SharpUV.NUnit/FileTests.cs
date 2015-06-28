using System;
using System.Text;
using NUnit.Framework;
using SharpUV;

namespace SharpUV.NUnit
{
	[TestFixture]
	public class FileTests
	{
		private static string TestFilePath
		{
			get{ return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.txt"); }
		}

		[Test]
		public void WriteAndReadFile()
		{
			const string data = "test string";

			var handle = new WriteFileHandle(data);
			handle.Open(TestFilePath, FileAccessMode.WriteOnly, FileOpenMode.Create | FileOpenMode.Truncate, FilePermissions.S_IRUSR | FilePermissions.S_IWUSR);

			Loop.Default.Run();

            var handle2 = new ReadFileHandle();
            handle2.Open(TestFilePath, FileAccessMode.ReadOnly, FileOpenMode.OnlyIfExists, FilePermissions.S_IRUSR);

            Loop.Default.Run();

			Assert.AreEqual(data, handle2.Content);
			Assert.AreEqual(Loop.Default.AllocatedBytes, Loop.Default.DeAllocatedBytes);
		}

		internal class WriteFileHandle : FileHandle
		{
			private string _data;

			public WriteFileHandle(string data)
			{
				_data = data;
			}

			protected override void OnOpen(UvArgs args)
			{
				args.Throw ();
				this.Write(Encoding.UTF8.GetBytes(_data));
			}

			protected override void OnWrite(UvDataArgs args)
			{
				args.Throw ();
				this.Close();
			}

			protected override void OnClose (UvArgs args)
			{
				args.Throw ();
			}
		}

        internal class ReadFileHandle : FileHandle
        {
			byte[] _data;

            public string Content
            {
                get { return Encoding.UTF8.GetString(_data); }
            }

            protected override void OnOpen(UvArgs args)
            {
                args.Throw();
                this.Read();
            }

            protected override void OnRead(UvDataArgs args)
            {
                args.Throw();
                _data = args.Data;
                this.Close();
            }

            protected override void OnClose(UvArgs args)
            {
                args.Throw();
            }
        }
	}
}

