using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpUV
{
	[StructLayout(LayoutKind.Sequential)]
	public struct uv_timespec_t
	{
		private static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0);

		public long tv_sec;
		public long tv_nsec;

		public DateTime DateTime
		{
			get
			{
				return EpochTime +
					   TimeSpan.FromSeconds(tv_sec) +
					   TimeSpan.FromTicks(tv_nsec / 100);
			}
		}
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct UvStat
	{
		/// <summary>
		/// Device ID of device containing file
		/// </summary>
		public ulong st_dev;
		/// <summary>
		/// Mode of file (see FilePermissions)
		/// </summary>
		public ulong st_mode;
		/// <summary>
		/// Number of hard links to the file
		/// </summary>
		public ulong st_nlink;
		/// <summary>
		/// User ID of file
		/// </summary>
		public ulong st_uid;
		/// <summary>
		/// Group ID of file
		/// </summary>
		public ulong st_gid;
		/// <summary>
		/// Device ID (if file is character or block special)
		/// </summary>
		public ulong st_rdev;
		public ulong st_ino;
		public ulong st_size;
		/// <summary>
		/// A file system-specific preferred I/O block size 
		/// for this object. In some file system types, this 
		/// may vary from file to file. 
		/// </summary>
		public ulong st_blksize;
		/// <summary>
		/// Number of blocks allocated for this object
		/// </summary>
		public ulong st_blocks;
		public ulong st_flags;
		public ulong st_gen;
		private uv_timespec_t st_atim;
		private uv_timespec_t st_mtim;
		private uv_timespec_t st_ctim;
		private uv_timespec_t st_birthtim;

		public ulong Size
		{
			get { return st_size; }
		}

		public ulong SizeOnDisk
		{
			get { return st_blksize * st_blocks; }
		}

		public DateTime Accessed
		{
			get { return st_atim.DateTime; }
		}

		public DateTime Modified
		{
			get { return st_mtim.DateTime; }
		}

		public DateTime LastFileStatusChange
		{
			get { return st_ctim.DateTime; }
		}

		public DateTime Created
		{
			get { return st_birthtim.DateTime; }
		}

		internal static UvStat Create(IntPtr ptr)
		{
			if(ptr != IntPtr.Zero)
				return (UvStat) Marshal.PtrToStructure(ptr, typeof(UvStat));

			return new UvStat();
		}
	};
}
