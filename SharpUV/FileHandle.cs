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

	public abstract class FileHandle : IDisposable
	{
		private const int DefaultReadBufferSize = 1024*32;

		public event EventHandler Closed;

		private FileHandleStatus _status;

		protected FileHandle(Loop loop)
		{
			this.Loop = loop;
			this.Status = FileHandleStatus.Closed;
		}

		~FileHandle()
		{
			this.Close();
		}

		/// <summary>
		/// The Loop wherein this object is running
		/// </summary>
		public Loop Loop { get; private set; }

		/// <summary>
		/// Handle status
		/// </summary>
		internal FileHandleStatus Status
		{
			get { return _status; }
			private set
			{
				_status = value;

				switch(value)
				{
					case FileHandleStatus.Open:
						this.ReadBuffer = this.Alloc(DefaultReadBufferSize);
						GC.ReRegisterForFinalize(this);
						break;

					case FileHandleStatus.Opening:
					case FileHandleStatus.Closing:
						GC.ReRegisterForFinalize(this);
						break;

					case FileHandleStatus.Closed:
						this.ReadBuffer = this.Free(this.ReadBuffer);
						GC.SuppressFinalize(this);
						break;
				}
			}
		}

		/// <summary>
		/// File handle
		/// </summary>
		internal uv_file File { get; private set; }

		/// <summary>
		/// Read buffer
		/// </summary>
		internal IntPtr ReadBuffer { get; private set; }

		public void Open(string path, FileAccessMode rw, FileOpenMode open, FilePermissions permissions)
		{
			if (this.Status != FileHandleStatus.Closed)
				return;

			var req = this.CreateRequest();

			try
			{
				CheckError(Uvi.uv_fs_open(this.Loop.Handle, req, path, rw, open, permissions, this.OnOpen));
				this.Status = FileHandleStatus.Opening;
			}
			catch (Exception)
			{
				this.FreeRequest(req, false);
				throw;
			}
		}

		/// <summary>
		/// Closes the stream. After this call the stream will not be valid
		/// </summary>
		public void Close()
		{
			if (this.Status != FileHandleStatus.Open)
				return;

			var req = this.CreateRequest();

			try
			{
				CheckError(Uvi.uv_fs_close(this.Loop.Handle, req, this.File, this.OnClose));
				this.Status = FileHandleStatus.Closing;
			}
			catch (Exception)
			{
				this.FreeRequest(req, false);
				throw;
			}
		}

		private void OnOpen(IntPtr req)
		{
			this.File = this.FreeRequest(req);
			this.Status = this.File > 0 ? FileHandleStatus.Open : FileHandleStatus.Closed;
			this.OnOpen();
		}

		protected virtual void OnOpen()
		{
		}

		private void OnClose(IntPtr req)
		{
			this.File = this.FreeRequest(req);
			this.Status = FileHandleStatus.Closed;
			this.OnClose();
		}

		protected virtual void OnClose()
		{
			this.Dispose(false);
			if (this.Closed != null)
				this.Closed(this, EventArgs.Empty);
		}

		/// <summary>
		/// Closes the stream. After this call the stream will not be valid
		/// </summary>
		public void Read()
		{
			if (this.Status != FileHandleStatus.Open)
				throw new InvalidOperationException("File handle must be open in order to read data");

			var req = this.CreateRequest();

			try
			{
				CheckError(Uvi.uv_fs_read(this.Loop.Handle, req, this.File, this.ReadBuffer, DefaultReadBufferSize, -1, this.OnRead));
				this.Status = FileHandleStatus.Closing;
			}
			catch (Exception)
			{
				this.FreeRequest(req, false);
				throw;
			}
		}

		private void OnRead(IntPtr req)
		{
			var ret = this.FreeRequest(req);
			this.OnRead();
		}

		protected virtual void OnRead()
		{
		}

		internal void CheckError(int code)
		{
			this.Loop.CheckError(code);
		}

		internal IntPtr Alloc(int size)
		{
			return this.Loop.Alloc(size);
		}

		internal IntPtr Free(IntPtr ptr)
		{
			return this.Loop.Free(ptr);
		}

		internal IntPtr CreateRequest()
		{
			return this.Alloc(Uvi.uv_req_size(uv_req_type.UV_FS));
		}

		internal uv_file FreeRequest(IntPtr req, bool cleanup = true)
		{
			var ret = Uvi.uv_fs_req_result(req);
			if(cleanup)
				Uvi.uv_fs_req_cleanup(req);

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
