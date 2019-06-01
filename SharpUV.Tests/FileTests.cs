﻿using System;
using System.Text;
using Xunit;

namespace SharpUV.Tests
{
	public class FileTests
	{
		private static string TestFilePath
		{
			get{ return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.txt"); }
		}

		[Fact]
		public void WriteAndReadFile()
		{
			const string data = "test string";

			var handle = new WriteFileHandle(data);
			handle.OpenWrite(TestFilePath);

			Loop.Current.Run();

            var handle2 = new ReadFileHandle();
            handle2.OpenRead(TestFilePath);

            Loop.Current.Run();

			Assert.Equal(data, handle2.Content);
			this.CheckCurrentLoop();
		}

		[Fact]
		public void StatFile()
		{
			const string data = "test string";

			var handle = new WriteFileHandle(data);
			handle.OpenWrite(TestFilePath);

			Loop.Current.Run();

			var handle2 = new Filesystem();
			handle2.Stat(TestFilePath, (args) => {
				Assert.Equal((ulong)data.Length, args.Stat.st_size);
			});

			Loop.Current.Run();
		}

		[Fact]
		public void DeleteFile()
		{
			const string data = "test string";

			var handle = new WriteFileHandle(data);
			handle.OpenWrite(TestFilePath);

			Loop.Current.Run();

			var handle2 = new Filesystem();
			handle2.Delete(TestFilePath, (args) =>
			{
				Assert.True(args.Successful);
			});

			Loop.Current.Run();
		}

		[Fact]
		public void CopyFile()
		{
			const string data = "test string";

			var handle = new WriteFileHandle(data);
			handle.OpenWrite(TestFilePath);

			Loop.Current.Run();

			var handle2 = new Filesystem();
			handle2.Copy(TestFilePath, TestFilePath + "copy", (args) =>
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

