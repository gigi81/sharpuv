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
	public enum FileStatus
	{
        Closed = 0,
		Opening,
		Open,
		Closing,
	}

	public class File : IDisposable
	{
        public event EventHandler<UvArgs> Opened;
		public event EventHandler<UvArgs> Closed;
        public event EventHandler<UvDataArgs> DataRead;
        public event EventHandler<UvDataArgs> DataWrite;

		private FileStatus _status = FileStatus.Closed;
		private int _file = 0;

		public File()
			: this(Loop.Current)
		{
		}

		public File(Loop loop)
		{
			this.Loop = loop;
			this.Status = FileStatus.Closed;
			this.InitDelegates();
		}

		~File()
		{
            this.Dispose();
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
		public FileStatus Status
		{
			get { return _status; }
			private set
			{
				_status = value;

				switch(value)
				{
					case FileStatus.Open:
						GC.ReRegisterForFinalize(this);
						break;

					case FileStatus.Opening:
					case FileStatus.Closing:
						GC.ReRegisterForFinalize(this);
						break;

					case FileStatus.Closed:
						GC.SuppressFinalize(this);
						break;
				}
			}
		}

		private UvCallback _openCallback;

		public void OpenRead(string path, Action<UvArgs> callback = null)
		{
			this.Open(path, FileAccessMode.ReadOnly, FileOpenMode.OnlyIfExists | FileOpenMode.BinaryMode, 0, callback);
		}

        public void OpenWrite(string path, Action<UvArgs> callback = null)
        {
			this.Open(path, FileAccessMode.WriteOnly, FileOpenMode.Create | FileOpenMode.Truncate | FileOpenMode.BinaryMode, 0, callback);
        }

		public void OpenAppend(string path, Action<UvArgs> callback = null)
		{
			this.Open(path, FileAccessMode.WriteOnly, FileOpenMode.Create | FileOpenMode.Append | FileOpenMode.BinaryMode, 0, callback);
		}

		public void Open(string path, FileAccessMode access, FileOpenMode mode, FilePermissions permissions, Action<UvArgs> callback = null)
		{
            if (this.IsDisposed)
                throw new InvalidOperationException("Cannot open a stream after it has been disposed");

            if (this.Status != FileStatus.Closed)
                throw new InvalidOperationException(String.Format("Cannot open a file handle when it's status is {0}", this.Status));

			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest();
				CheckError(uv_fs_open(this.Loop, req, path, access, mode, permissions, _openDelegate));
				this.Status = FileStatus.Opening;
                _openCallback = new UvCallback(this, callback);
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private static int uv_fs_open(Loop loop, IntPtr req, string path, FileAccessMode rw, FileOpenMode open, FilePermissions permissions, uv_fs_cb cb)
		{
			return Uvi.uv_fs_open(loop.Handle, req, path, (int)rw | (int)open, (int)permissions, cb);
		}

		private void OnOpen(IntPtr req)
		{
            var callback = _openCallback;
            _openCallback = null;

			_file = this.FreeRequest(req);
			this.Status = _file != -1 ? FileStatus.Open : FileStatus.Closed;
            callback.Invoke(_file, this.OnOpen, this.Opened);
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
            if (this.Status != FileStatus.Open)
                throw new InvalidOperationException(String.Format("Cannot close the file handle while the status is {0}", this.Status));

			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest();
				CheckError(Uvi.uv_fs_close(this.Loop.Handle, req, _file, _closeDelegate));
				this.Status = FileStatus.Closing;
                _closeCallback = new UvCallback(this, callback);
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnClose(IntPtr req)
		{
            var callback = _closeCallback;
            _closeCallback = null;

			_file = this.FreeRequest(req);
			if(_file != -1)
				this.Status = FileStatus.Closed;

			this.Dispose(false);
            callback.Invoke(_file, this.OnClose, this.Closed);
		}

		protected virtual void OnClose(UvArgs args)
		{
		}

		private UvDataCallback _readCallback;

		public void Read(Action<UvDataArgs> callback = null)
		{
			this.Read (1024, callback);
		}

		/// <summary>
		/// Read from the file
		/// </summary>
		public void Read(uint length, Action<UvDataArgs> callback = null)
		{
			if (this.Status != FileStatus.Open)
				throw new InvalidOperationException("File handle must be open in order to read data");

			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest(length);
				CheckError(Uvi.uv_fs_read(this.Loop.Handle, req, _file, new[] { this.Loop.Requests[req] }, 1, -1, _readDelegate));
                _readCallback = new UvDataCallback(this, callback);
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnRead(IntPtr req)
		{
            var callback = _readCallback;
            _readCallback = null;

			callback.Invoke(this.FreeReadRequest(req), this.OnRead, this.DataRead);
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
			if (this.Status != FileStatus.Open)
				throw new InvalidOperationException("File handle must be open in order to write data");

			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest(data, offset, length);
				CheckError(Uvi.uv_fs_write(this.Loop.Handle, req, _file, new[] { this.Loop.Requests[req] }, 1, -1, _writeDelegate));
                _writeCallback = new UvDataCallback(this, callback, data);
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnWrite(IntPtr req)
		{
            var callback = _writeCallback;
            _writeCallback = null;

            callback.Invoke(this.FreeRequest(req), this.OnWrite, this.DataWrite);
		}

		protected virtual void OnWrite(UvDataArgs args)
		{
		}

		private void CheckError(int code)
		{
			this.Loop.CheckError(code);
		}

		private IntPtr CreateRequest()
		{
			return this.Loop.Requests.Create(uv_req_type.UV_FS);
		}

		private IntPtr CreateRequest(uint length)
        {
            return this.Loop.Requests.Create(uv_req_type.UV_FS, length);
        }

		private IntPtr CreateRequest(byte[] data, int offset, int length)
		{
			return this.Loop.Requests.Create(uv_req_type.UV_FS, data, offset, length);
		}

		private uv_file FreeRequest(IntPtr req)
		{
			var ret = Uvi.uv_fs_req_result(req);
			Uvi.uv_fs_req_cleanup(req);
			this.Loop.Requests.Delete(req);
			return ret;
		}

		private UvDataArgs FreeReadRequest(IntPtr req)
        {
            var ret = Uvi.uv_fs_req_result(req);
            Uvi.uv_fs_req_cleanup(req);
            var data = this.Loop.Requests.CopyAndDelete(req, (int)ret);
            return new UvDataArgs(ret, data);
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
            if (this.Status != FileStatus.Closed)
                this.Close(); //it will call dispose on the close callback
            else
                this.Dispose(true);
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
