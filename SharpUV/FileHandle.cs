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
	[CLSCompliant (false)]
	public enum FilePermissions
	{
		S_ISUID     = 0x0800, // Set user ID on execution
		S_ISGID     = 0x0400, // Set group ID on execution
		S_ISVTX     = 0x0200, // Save swapped text after use (sticky).
		S_IRUSR     = 0x0100, // Read by owner
		S_IWUSR     = 0x0080, // Write by owner
		S_IXUSR     = 0x0040, // Execute by owner
		S_IRGRP     = 0x0020, // Read by group
		S_IWGRP     = 0x0010, // Write by group
		S_IXGRP     = 0x0008, // Execute by group
		S_IROTH     = 0x0004, // Read by other
		S_IWOTH     = 0x0002, // Write by other
		S_IXOTH     = 0x0001, // Execute by other

		S_IRWXG     = (S_IRGRP | S_IWGRP | S_IXGRP),
		S_IRWXU     = (S_IRUSR | S_IWUSR | S_IXUSR),
		S_IRWXO     = (S_IROTH | S_IWOTH | S_IXOTH),
		ACCESSPERMS = (S_IRWXU | S_IRWXG | S_IRWXO), // 0777
		ALLPERMS    = (S_ISUID | S_ISGID | S_ISVTX | S_IRWXU | S_IRWXG | S_IRWXO), // 07777
		DEFFILEMODE = (S_IRUSR | S_IWUSR | S_IRGRP | S_IWGRP | S_IROTH | S_IWOTH), // 0666
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

		private UvCallback _openCallback;

		public void Open(string path, Action<UvArgs> callback = null)
		{
			this.Open(path, FileAccessMode.ReadOnly, FileOpenMode.OnlyIfExists | FileOpenMode.BinaryMode, 0, callback);
		}

		public void Open(string path, FileAccessMode access, FileOpenMode mode, FilePermissions permissions, Action<UvArgs> callback = null)
		{
			if (this.Status != FileHandleStatus.Closed)
				return;

			IntPtr req = IntPtr.Zero;

			try
			{
				_openCallback = new UvCallback(callback);
				req = this.CreateRequest();
				CheckError(uv_fs_open(this.Loop, req, path, access, mode, permissions, _openDelegate));
				this.Status = FileHandleStatus.Opening;
			}
			catch (Exception)
			{
				_openCallback = null;
				this.FreeRequest(req);
				throw;
			}
		}

		private static int uv_fs_open(Loop loop, IntPtr req, string path, FileAccessMode rw, FileOpenMode open, FilePermissions permissions, uv_fs_cb cb)
		{
			return Uvi.uv_fs_open(loop.Handle, req, Encoding.ASCII.GetBytes(path), (int)rw | (int)open, (int)permissions, cb);
		}

		private void OnOpen(IntPtr req)
		{
			_file = this.FreeRequest(req);
			this.Status = _file != -1 ? FileHandleStatus.Open : FileHandleStatus.Closed;
			_openCallback.Invoke(_file, this.OnOpen);
			_openCallback = null;
		}

		protected virtual void OnOpen(UvArgs args)
		{
		}

		private UvCallback _closeCallback;

		/// <summary>
		/// Closes the stream. After this call the stream will not be valid
		/// </summary>
		public void Close(Action<UvArgs> callback = null)
		{
			if (this.Status != FileHandleStatus.Open)
				return;

			IntPtr req = IntPtr.Zero;

			try
			{
				_closeCallback = new UvCallback(callback);
				req = this.CreateRequest();
				CheckError(Uvi.uv_fs_close(this.Loop.Handle, req, _file, _closeDelegate));
				this.Status = FileHandleStatus.Closing;
			}
			catch (Exception)
			{
				_closeCallback = null;
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnClose(IntPtr req)
		{
			_file = this.FreeRequest(req);
			if(_file != -1)
				this.Status = FileHandleStatus.Closed;
			_closeCallback.Invoke(_file, this.OnClose);
			_closeCallback = null;

			this.Dispose(false);
			if (this.Closed != null)
				this.Closed(this, EventArgs.Empty);
		}

		protected virtual void OnClose(UvArgs args)
		{
		}

		private UvDataCallback _readCallback;

		public void Read(byte[] data, Action<UvDataArgs> callback = null)
		{
			this.Read (data, 0, data.Length, callback);
		}

		/// <summary>
		/// Read from the file
		/// </summary>
		public void Read(byte[] data, int offset, int length, Action<UvDataArgs> callback = null)
		{
			if (this.Status != FileHandleStatus.Open)
				throw new InvalidOperationException("File handle must be open in order to read data");

			IntPtr req = IntPtr.Zero;

			try
			{
				_readCallback = new UvDataCallback(callback, data);
				req = this.CreateRequest(data, offset, length);
				CheckError(Uvi.uv_fs_read(this.Loop.Handle, req, _file, new[] { this.Loop.Requests[req] }, 1, -1, _readDelegate));
			}
			catch (Exception)
			{
				_readCallback = null;
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnRead(IntPtr req)
		{
			_readCallback.Invoke(this.FreeRequest(req), this.OnRead);
			_readCallback = null;
		}

		protected virtual void OnRead(UvDataArgs args)
		{
		}

		private UvDataCallback _writeCallback;

		public void Write(byte[] data, Action<UvDataArgs> callback = null)
		{
			this.Write (data, 0, data.Length, callback);
		}

		public void Write(byte[] data, int offset, int length, Action<UvDataArgs> callback = null)
		{
			if (this.Status != FileHandleStatus.Open)
				throw new InvalidOperationException("File handle must be open in order to write data");

			IntPtr req = IntPtr.Zero;

			try
			{
				_writeCallback = new UvDataCallback(callback, data);
				req = this.CreateRequest(data, offset, length);
				CheckError(Uvi.uv_fs_write(this.Loop.Handle, req, _file, new[] { this.Loop.Requests[req] }, 1, -1, _writeDelegate));
			}
			catch (Exception)
			{
				_writeCallback = null;
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnWrite(IntPtr req)
		{
			_writeCallback.Invoke(this.FreeRequest(req), this.OnWrite);
			_writeCallback = null;
		}

		protected virtual void OnWrite(UvDataArgs args)
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
