using System;
using System.Runtime.InteropServices;

namespace DbLoader
{
	public static class KlbSqlite3Dll
	{
		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_open_v2(byte[] filename, out IntPtr db, int flags, byte[] zVfs);

		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_close(IntPtr db);

		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_exec(IntPtr db, byte[] sql, IntPtr cb1, IntPtr cb2, ref IntPtr errMsg);

		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_prepare_v2(IntPtr db, byte[] zSql, int nByte, out IntPtr ppStmt, out IntPtr pzTail);

		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_step(IntPtr pStmt);

		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_finalize(IntPtr pStmt);
        
		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_column_count(IntPtr pStmt);

		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr sqlite3_column_name(IntPtr pStmt, int iCol);

		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_column_type(IntPtr pStmt, int iCol);
		
		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr sqlite3_column_text(IntPtr pStmt, int iCol);
		
		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_column_bytes(IntPtr pStmt, int iCol);
		
		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_bind_int(IntPtr pStmt, int n, int value);

		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_bind_int64(IntPtr pStmt, int n, long value);

		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_bind_text(IntPtr pStmt, int n, byte[] value, int length, IntPtr freetype);

		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_bind_double(IntPtr pStmt, int n, double value);

		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int sqlite3_bind_blob(IntPtr pStmt, int n, byte[] value, int length, IntPtr freetype);
        
		[DllImport("klbsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int klbvfs_register();
		
		public static readonly IntPtr SQLITE_TRANSIENT = (IntPtr)(-1);
        
	}
}
