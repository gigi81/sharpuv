using Libuv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpUV
{
	public class Filesystem
	{
		public event EventHandler<UvArgs> DirectoryCreated;
		public event EventHandler<UvArgs> DirectoryRemoved;
		public event EventHandler<UvStatArgs> DataStat;
		public event EventHandler<UvArgs> Deleted;

		public Filesystem()
			: this(Loop.Current)
		{
		}

		public Filesystem(Loop loop)
		{
			this.Loop = loop;
			this.InitDelegates();
		}

		#region Delegates
		private uv_fs_cb _mkdirDelegate;
		private uv_fs_cb _rmdirDelegate;
		private uv_fs_cb _statDelegate;
		private uv_fs_cb _deleteDelegate;

		private void InitDelegates()
		{
			_mkdirDelegate = new uv_fs_cb(this.OnCreateDirectory);
			_rmdirDelegate = new uv_fs_cb(this.OnRemoveDirectory);
			_statDelegate = new uv_fs_cb(this.OnStat);
			_deleteDelegate = new uv_fs_cb(this.OnDelete);
		}
		#endregion

		/// <summary>
		/// The Loop wherein this object is running
		/// </summary>
		public Loop Loop { get; private set; }

		private UvCallback _mkdirCallback;

		public void CreateDirectory(string path, Action<UvArgs> callback = null)
		{
			this.CreateDirectory(path, FilePermissions.S_IRWXU, callback);
		}

		public void CreateDirectory(string path, FilePermissions permissions, Action<UvArgs> callback = null)
		{
			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest();
				CheckError(Uvi.uv_fs_mkdir(this.Loop.Handle, req, path, (int)permissions, _mkdirDelegate));
				_mkdirCallback = new UvCallback(this, callback);
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnCreateDirectory(IntPtr req)
		{
			var callback = _mkdirCallback;
			_mkdirCallback = null;

			callback.Invoke(this.FreeRequest(req), this.OnCreateDirectory, this.DirectoryCreated);
		}

		protected virtual void OnCreateDirectory(UvArgs args)
		{
		}

		private UvCallback _rmdirCallback;

		public void RemoveDirectory(string path, Action<UvArgs> callback = null)
		{
			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest();
				CheckError(Uvi.uv_fs_rmdir(this.Loop.Handle, req, path, _rmdirDelegate));
				_rmdirCallback = new UvCallback(this, callback);
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnRemoveDirectory(IntPtr req)
		{
			var callback = _rmdirCallback;
			_rmdirCallback = null;

			callback.Invoke(this.FreeRequest(req), this.OnRemoveDirectory, this.DirectoryRemoved);
		}

		protected virtual void OnRemoveDirectory(UvArgs args)
		{
		}

		private UvStatCallback _statCallback;

		public void Stat(string path, Action<UvStatArgs> callback = null)
		{
			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest();
				CheckError(Uvi.uv_fs_stat(this.Loop.Handle, req, path, _statDelegate));
				_statCallback = new UvStatCallback(this, callback);
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnStat(IntPtr req)
		{
			var callback = _statCallback;
			_statCallback = null;

			callback.Invoke(this.FreeStatRequest(req), this.OnStat, this.DataStat);
		}

		protected virtual void OnStat(UvStatArgs args)
		{
		}

		private UvCallback _deleteCallback;

		public void Delete(string path, Action<UvArgs> callback = null)
		{
			IntPtr req = IntPtr.Zero;

			try
			{
				req = this.CreateRequest();
				CheckError(Uvi.uv_fs_unlink(this.Loop.Handle, req, path, _deleteDelegate));
				_deleteCallback = new UvCallback(this, callback);
			}
			catch (Exception)
			{
				this.FreeRequest(req);
				throw;
			}
		}

		private void OnDelete(IntPtr req)
		{
			var callback = _deleteCallback;
			_deleteCallback = null;

			callback.Invoke(this.FreeRequest(req), this.OnDelete, this.Deleted);
		}

		protected virtual void OnDelete(UvArgs args)
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

		private int FreeRequest(IntPtr req)
		{
			var ret = Uvi.uv_fs_req_result(req);
			Uvi.uv_fs_req_cleanup(req);
			this.Loop.Requests.Delete(req);
			return ret;
		}

		private UvStatArgs FreeStatRequest(IntPtr req)
		{
			var ret = Uvi.uv_fs_req_result(req);
			var stat = UvStat.Create(ret == 0 ? Uvi.uv_fs_req_stat(req) : IntPtr.Zero);
			Uvi.uv_fs_req_cleanup(req);
			this.Loop.Requests.Delete(req);
			return new UvStatArgs(ret, stat);
		}
	}
}
