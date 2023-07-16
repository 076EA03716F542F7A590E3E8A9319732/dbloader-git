using System;

namespace DbLoader
{
	public sealed class SQLiteStatement : IDisposable
	{
		public SQLiteStatement(IntPtr handle)
		{
			_handle = handle;
		}

		~SQLiteStatement()
		{
			Dispose();
		}
		
		public int ColumnCount()
		{
			ThrowIfDisposed();
			return KlbSqlite3Dll.sqlite3_column_count(_handle);
		}

		public string ColumnName(int i)
		{
			ThrowIfDisposed();
			return SQLiteUtil.ReadUTF8String(KlbSqlite3Dll.sqlite3_column_name(_handle, i), -1);
		}

		public int ColumnType(int i)
		{
			ThrowIfDisposed();
			return KlbSqlite3Dll.sqlite3_column_type(_handle, i);
		}

		public void BindInt(int i, int value)
		{
			ThrowIfDisposed();
			var num = KlbSqlite3Dll.sqlite3_bind_int(_handle, i, value);
			if (num != 0)
			{
				throw new InvalidOperationException(nameof(num));
			}
		}

		public void BindInt64(int i, long value)
		{
			ThrowIfDisposed();
			var num = KlbSqlite3Dll.sqlite3_bind_int64(_handle, i, value);
			if (num != 0)
			{
				throw new InvalidOperationException(nameof(num));
			}
		}

		public void BindDouble(int i, double value)
		{
			ThrowIfDisposed();
			var num = KlbSqlite3Dll.sqlite3_bind_double(_handle, i, value);
			if (num != 0)
			{
				throw new InvalidOperationException(nameof(num));
			}
		}

		public void BindText(int i, string value)
		{
			ThrowIfDisposed();
			var array = SQLiteUtil.ToUtf8(value, false);
			var num = KlbSqlite3Dll.sqlite3_bind_text(_handle, i, array, array.Length, KlbSqlite3Dll.SQLITE_TRANSIENT);
			if (num != 0)
			{
				throw new InvalidOperationException(nameof(num));
			}
		}

		public void BindBlob(int i, byte[] value)
		{
			ThrowIfDisposed();
			var num = KlbSqlite3Dll.sqlite3_bind_blob(_handle, i, value, value.Length, KlbSqlite3Dll.SQLITE_TRANSIENT);
			if (num != 0)
			{
				throw new InvalidOperationException(nameof(num));
			}
		}

		public bool Step()
		{
			ThrowIfDisposed();
			var num = KlbSqlite3Dll.sqlite3_step(_handle);
			if (num != 100 && num != 101)
			{
				throw new InvalidOperationException(nameof(num));
			}
			return num == 100;
		}

		public string GetText(int i)
		{
			ThrowIfDisposed();
			return SQLiteUtil.ReadUTF8String(KlbSqlite3Dll.sqlite3_column_text(_handle, i), KlbSqlite3Dll.sqlite3_column_bytes(_handle, i));
		}
        

		public void FinalizeHandle()
		{
			var intPtr = _handle;
			_handle = IntPtr.Zero;
			if (intPtr != IntPtr.Zero)
			{
				KlbSqlite3Dll.sqlite3_finalize(intPtr);
			}
		}

		public void Dispose()
		{
			FinalizeHandle();
			_disposed = true;
			GC.SuppressFinalize(this);
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}

		private IntPtr _handle;

		private bool _disposed;
	}
}
