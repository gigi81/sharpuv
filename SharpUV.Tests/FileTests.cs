using System;
using System.Text;
using Xunit;

namespace SharpUV.Tests
{
	public class FileTests
	{
		[Fact]
		public void WriteAndReadFile()
		{
			const string data = "test string";

            var path = System.IO.Path.GetTempFileName();

			var handle = new WriteFileHandle(data);
			handle.OpenWrite(path);

            Loop.Current.Run();

            var handle2 = new ReadFileHandle();
            handle2.OpenRead(path);

            Loop.Current.Run();

			Assert.Equal(data, handle2.Content);
			this.CheckCurrentLoop();
		}

		[Fact]
		public void StatFile()
		{
			const string data = "test string";

            var path = System.IO.Path.GetTempFileName();

            var handle = new WriteFileHandle(data);
			handle.OpenWrite(path);

			Loop.Current.Run();

			var handle2 = new Filesystem();
			handle2.Stat(path, (args) => {
				Assert.Equal((ulong)data.Length, args.Stat.st_size);
			});

			Loop.Current.Run();
		}

		[Fact]
		public void DeleteFile()
		{
			const string data = "test string";

            var path = System.IO.Path.GetTempFileName();

            var handle = new WriteFileHandle(data);
			handle.OpenWrite(path);

			Loop.Current.Run();

			var handle2 = new Filesystem();
			handle2.Delete(path, (args) =>
			{
				Assert.True(args.Successful);
			});

			Loop.Current.Run();
		}

		[Fact]
		public void CopyFile()
		{
			const string data = "test string";

            var path = System.IO.Path.GetTempFileName();

            var handle = new WriteFileHandle(data);
			handle.OpenWrite(path);

			Loop.Current.Run();

			var handle2 = new Filesystem();
			handle2.Copy(path, path + "copy", (args) =>
			{
				Assert.True(args.Successful);
			});

			Loop.Current.Run();
		}

		private void CheckCurrentLoop()
		{
			Assert.Equal(0u, Loop.Current.AllocatedBytes);
			Assert.Equal(0u, Loop.Current.AllocatedHandles);
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

