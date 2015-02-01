using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using uint64_t = System.Int64;

namespace SharpUV
{
    [StructLayout(LayoutKind.Sequential)]
    public struct uv_timespec_t
    {
        long tv_sec;
        long tv_nsec;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct UvStat
    {
        uint64_t st_dev;
        uint64_t st_mode;
        uint64_t st_nlink;
        uint64_t st_uid;
        uint64_t st_gid;
        uint64_t st_rdev;
        uint64_t st_ino;
        uint64_t st_size;
        uint64_t st_blksize;
        uint64_t st_blocks;
        uint64_t st_flags;
        uint64_t st_gen;
        uv_timespec_t st_atim;
        uv_timespec_t st_mtim;
        uv_timespec_t st_ctim;
        uv_timespec_t st_birthtim;

        public static UvStat Create(IntPtr ptr)
        {
            var ret = new UvStat();
            Marshal.PtrToStructure(ptr, ret);
            return ret;
        }
    };
}
