using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpUV
{
	internal sealed class FileCopy
	{
		private readonly ReadFile _read;
		private readonly WriteFile _write;
		private readonly Action<UvArgs> _completed;

		public FileCopy(string source, string destination, Action<UvArgs> completed)
		{
			_read = new ReadFile(source);
			_write = new WriteFile(destination);
			_completed = completed;

			_read.Opened += FileOpened;
			_write.Opened += FileOpened;
			_read.Closed += FileClosed;
			_write.Closed += FileClosed;

			_read.Open(_write);
			_write.Open(_read);
		}

		private void FileOpened(object sender, UvArgs e)
		{
			this.FileCompleted();
		}

		private void FileClosed(object sender, UvArgs e)
		{
			this.FileCompleted();
		}

		private void FileCompleted()
		{
			if (_read.Status == FileStatus.Closed && _write.Status == FileStatus.Closed)
				if (_completed != null)
					_completed(this.GetArgs());
		}

		private UvArgs GetArgs()
		{
			return _read._error ??
				   _write._error ??
				   UvArgs.UvEmpty;
		}

		private class ReadFile : File
		{
			private readonly string _path;
			private WriteFile _write;
			internal UvArgs _error;

			public ReadFile(string path)
			{
				_path = path;
			}

			public void Open(WriteFile write)
			{
				_write = write;
				this.OpenRead(_path);
			}

			protected override void OnOpen(UvArgs args)
			{
				if (args.Successful)
				{
					if (_write.Status == FileStatus.Open)
						this.Read();
					else if (_write.Status == FileStatus.Closed)
						this.Close();
				}
				else
				{
					_error = args;
					if (_write.Status == FileStatus.Open)
						_write.Close();
				}
			}

			protected override void OnRead(UvDataArgs args)
			{
				if (args.Code > 0)
				{
					_write.Write(args.Data);
				}
				else
				{
					if(!args.Successful)
						_error = args;

					this.Close();
					_write.Close();
				}
			}

			protected override void OnClose(UvArgs args)
			{
				if (!args.Successful)
					_error = args;
			}
		}

		private class WriteFile : File
		{
			private readonly string _path;
			private ReadFile _read;
			internal UvArgs _error;

			public WriteFile(string path)
			{
				_path = path;
			}

			public void Open(ReadFile read)
			{
				_read = read;
				this.OpenWrite(_path);
			}

			protected override void OnOpen(UvArgs args)
			{
				if (args.Successful)
				{
					if (_read.Status == FileStatus.Open)
						_read.Read();
					else if (_read.Status == FileStatus.Closed)
						this.Close();
				}
				else
				{
					_error = args;
					if (_read.Status == FileStatus.Open)
						_read.Close();
				}
			}

			protected override void OnWrite(UvDataArgs args)
			{
				if (args.Successful)
				{
					_read.Read();
				}
				else
				{
					_error = args;
					_read.Close();
					this.Close();
				}
			}

			protected override void OnClose(UvArgs args)
			{
				if(!args.Successful)
					_error = args;
			}
		}
	}
}
