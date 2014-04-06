#region License
/**
 * Copyright (c) 2012 Luigi Grilli
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute,
 * sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Libuv;
using uv_file = System.Int32;

namespace SharpUV
{
	public enum FileHandleStatus
	{
		Opening,
		Open,
		Closing,
		Closed
	}

	public enum FileAccessMode
	{
		ReadOnly  = 0x0000,  /* open for reading only */
		WriteOnly = 0x0001,  /* open for writing only */
		ReadWrite = 0x0002 /* open for reading and writing */
	}

	[Flags]
	public enum FileOpenMode
	{
		Default 	 = 0x0000,
		Append 		 = 0x0008,  /* writes done at eof */
		Create 		 = 0x0100,  /* create and open file */
		Truncate 	 = 0x0200,  /* open and truncate */
		OnlyIfExists = 0x0400,  /* open only if file doesn't already exist */
		TextMode 	 = 0x4000,  /* file mode is text (translated) */
		BinaryMode 	 = 0x8000  /* file mode is binary (untranslated) */
	}

	[Flags]
	public enum FilePermissions
	{
		S_IRWXU  = 0x0700,		// user (file owner) has read, write and execute permission 
		S_IRUSR  = 0x0400,		// user has read permission 
		S_IREAD  = 0x0400,
		S_IWUSR  = 0x0200,		// user has write permission 
		S_IWRITE = 0x0200,
		S_IXUSR  = 0x0100,		// user has execute permission
		S_IEXEC  = 0x0100,

		S_IRWXG  = 0x0070,		// group has read, write and execute permission     
		S_IRGRP  = 0x0040,		// group has read permission 
		S_IWGRP  = 0x0020,		// group has write permission
		S_IXGRP  = 0x0010,		// group has execute permission 

		S_IRWXO  = 0x0007,		// others have read, write and execute permission 
		S_IROTH  = 0x0004,		// others have read permission 
		S_IWOTH  = 0x0002,		// others have write permisson 
		S_IXOTH  = 0x0001		// others have execute permission 
	}

	public class FileHandle : IDisposable
	{
		public event EventHandler Closed;

		private FileHandleStatus _status;
		private int _file = 0;

		public FileHandle()
			: this(Loop.Default)
		{
		}

		public FileHandle(Loop loop)
		{
			this.Loop = loop;
			this.Status = FileHandleStatus.Closed;
			this.InitDelegates();
		}

		~FileHandle()
		{
			this.Close();
		}

		#region Delegates
		private uv_fs_cb _openDelegate;
		private uv_fs_cb _closeDelegate;
		private uv_fs_cb _readDelegate;
		private uv_fs_cb _writeDelegate;

		private void InitDelegates ()
		{
			_openDelegate = new uv_fs_cb(this.OnOpen);
			_closeDelegate = new uv_fs_cb(this.OnClose);
			_readDelegate = new uv_fs_cb(this.OnRead);
			_writeDelegate = new uv_fs_cb(this.OnWrite);
		}
		#endregion

		/// <summary>
		/// The Loop wherein this object is running
		/// </summary>
		public Loop Loop { get; private set; }

		/// <summary>
		/// Handle status
		/// </summary>
		public FileHandleStatus Status
		{
			get { return _status; }
			private set
			{
				_status = value;

				switch(value)
				{
					case FileHandleStatus.Open:
						GC.ReRegisterForFinalize(this);
						break;

					case FileHandleStatus.Opening:
					case FileHandleStatus.Closing:
						GC.ReRegisterForFinalize(this);
						break;

					case FileHandleStatus.Closed:
						GC.SuppressFinalize(this);
						break;
				}
			}
		}

		public void Open(string path)
		{
			this.Open(path, FileAccessMode.ReadOnly, FileOpenMode.OnlyIfExists | FileOpenMode.BinaryMode, 0);
		}

		public void Open(string path, FileAccessMode access, FileOpenMode mode, FilePermissions permissions)
		{
			if (this.Status != FileHandleStatus.Closed)
				return;

			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest();
				CheckError(uv_fs_open(this.Loop, req, path, access, mode, permissions, _openDelegate));
				this.Status = FileHandleStatus.Opening;
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private static int uv_fs_open(Loop loop, IntPtr req, string path, FileAccessMode rw, FileOpenMode open, FilePermissions permissions, uv_fs_cb cb)
		{
			int p = (int)permissions;
			return Uvi.uv_fs_open(loop.Handle, req, path, (int)rw | (int)open, 0x0777, cb);
		}

		/// <summary>
		/// Closes the stream. After this call the stream will not be valid
		/// </summary>
		public void Close()
		{
			if (this.Status != FileHandleStatus.Open)
				return;

			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest();
				CheckError(Uvi.uv_fs_close(this.Loop.Handle, req, _file, _closeDelegate));
				this.Status = FileHandleStatus.Closing;
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnOpen(IntPtr req)
		{
			_file = this.FreeRequest(req);
			this.Status = _file != -1 ? FileHandleStatus.Open : FileHandleStatus.Closed;
			this.OnOpen(new UvArgs(_file));
		}

		protected virtual void OnOpen(UvArgs args)
		{
		}

		private void OnClose(IntPtr req)
		{
			_file = this.FreeRequest(req);
			if(_file != -1)
				this.Status = FileHandleStatus.Closed;
			this.OnClose();
		}

		protected virtual void OnClose()
		{
			this.Dispose(false);
			if (this.Closed != null)
				this.Closed(this, EventArgs.Empty);
		}

		public void Read(byte[] data)
		{
			this.Read (data, 0, data.Length);
		}

		/// <summary>
		/// Read from the file
		/// </summary>
		public void Read(byte[] data, int offset, int length)
		{
			if (this.Status != FileHandleStatus.Open)
				throw new InvalidOperationException("File handle must be open in order to read data");

			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest(data, offset, length);
				CheckError(Uvi.uv_fs_read(this.Loop.Handle, req, _file, new[] { this.Loop.Requests[req] }, 1, -1, _readDelegate));
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnRead(IntPtr req)
		{
			var ret = this.FreeRequest(req);
			this.OnRead(new UvArgs(ret));
		}

		protected virtual void OnRead(UvArgs args)
		{
		}

		public void Write(byte[] data)
		{
			this.Write (data, 0, data.Length);
		}

		public void Write(byte[] data, int offset, int length)
		{
			if (this.Status != FileHandleStatus.Open)
				throw new InvalidOperationException("File handle must be open in order to write data");

			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest(data, offset, length);
				CheckError(Uvi.uv_fs_write(this.Loop.Handle, req, _file, new[] { this.Loop.Requests[req] }, 1, -1, _writeDelegate));
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnWrite(IntPtr req)
		{
			var ret = this.FreeRequest(req);
			this.OnWrite(new UvArgs(ret));
		}

		protected virtual void OnWrite(UvArgs args)
		{
		}

		internal void CheckError(int code)
		{
			this.Loop.CheckError(code);
		}

		internal IntPtr CreateRequest()
		{
			return this.Loop.Requests.Create(uv_req_type.UV_FS);
		}

		internal IntPtr CreateRequest(byte[] data, int offset, int length)
		{
			return this.Loop.Requests.Create(uv_req_type.UV_FS, data, offset, length);
		}

		internal uv_file FreeRequest(IntPtr req)
		{
			if (req == IntPtr.Zero)
				return 0;

			var ret = Uvi.uv_fs_req_result(req);
			Uvi.uv_fs_req_cleanup(req);
			this.Loop.Requests.Delete(req);
			if (ret < 0)
				throw new UvException(ret);

			return ret;
		}

		#region Dispose Management
		/// <summary>
		/// Indicates if the object has been disposed
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Dispose the object freeing unmanaged resources allocated
		/// </summary>
		public void Dispose()
		{
			this.Close();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.IsDisposed)
				return;

			GC.SuppressFinalize(this);
			this.IsDisposed = true;
		}
		#endregion
	}
}
