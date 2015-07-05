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
			handle.OpenWrite(TestFilePath);

			Loop.Current.Run();

            var handle2 = new ReadFileHandle();
            handle2.OpenRead(TestFilePath);

            Loop.Current.Run();

			Assert.AreEqual(data, handle2.Content);
			this.CheckCurrentLoop();
		}

		[Test]
		public void StatFile()
		{
			const string data = "test string";

			var handle = new WriteFileHandle(data);
			handle.OpenWrite(TestFilePath);

			Loop.Current.Run();

			var handle2 = new Filesystem();
			handle2.Stat(TestFilePath, (args) => {
				Assert.AreEqual(data.Length, args.Stat.st_size);
			});

			Loop.Current.Run();
		}

		[Test]
		public void DeleteFile()
		{
			const string data = "test string";

			var handle = new WriteFileHandle(data);
			handle.OpenWrite(TestFilePath);

			Loop.Current.Run();

			var handle2 = new Filesystem();
			handle2.Delete(TestFilePath, (args) =>
			{
				Assert.IsTrue(args.Successful);
			});

			Loop.Current.Run();
		}

		[Test]
		public void CopyFile()
		{
			const string data = "test string";

			var handle = new WriteFileHandle(data);
			handle.OpenWrite(TestFilePath);

			Loop.Current.Run();

			var handle2 = new Filesystem();
			handle2.Copy(TestFilePath, TestFilePath + "copy", (args) =>
			{
				Assert.IsTrue(args.Successful);
			});

			Loop.Current.Run();
		}

		private void CheckCurrentLoop()
		{
			Assert.AreEqual(0, Loop.Current.AllocatedBytes);
			Assert.AreEqual(0, Loop.Current.AllocatedHandles);
		}

		internal class WriteFileHandle : File
		{
			private string _data;

			public WriteFileHandle(string data)
			{
				_data = data;
			}

			protected override void OnOpen(UvArgs args)
			{
				args.Throw();
				this.Write(Encoding.UTF8.GetBytes(_data));
			}

			protected override void OnWrite(UvDataArgs args)
			{
				args.Throw();
				this.Close();
			}

			protected override void OnClose (UvArgs args)
			{
				args.Throw();
			}
		}

        internal class ReadFileHandle : File
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

