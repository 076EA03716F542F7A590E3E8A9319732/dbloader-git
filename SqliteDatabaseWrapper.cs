
using System;

namespace DbLoader
{
	public class SqliteDatabaseWrapper : IDisposable
	{
		public SqliteDatabaseWrapper(IntPtr handle)
		{
			_handle = handle;
		}

		private void Close()
		{
			var intPtr = _handle;
			_handle = IntPtr.Zero;
			if (intPtr != IntPtr.Zero)
			{
				KlbSqlite3Dll.sqlite3_close(intPtr);
			}
		}

		private SQLiteStatement Prepare(string query)
		{
			var array = SQLiteUtil.ToUtf8(query, false);
			var num = KlbSqlite3Dll.sqlite3_prepare_v2(_handle, array, array.Length, out var zero, out _);
			if (num != 0)
			{
				throw new InvalidOperationException(nameof(num));
			}
			return new SQLiteStatement(zero);
		}

		public SQLiteStatement Query(string query, params object[] args)
		{
			SQLiteStatement sqliteStatement = null!;
			SQLiteStatement result;
			try
			{
				sqliteStatement = Prepare(query);
				ApplyBind(sqliteStatement, args);
				result = sqliteStatement;
			}
			catch
			{
				sqliteStatement.FinalizeHandle();
				throw;
			}
			return result;
		}

		private void ApplyBind(SQLiteStatement stmt, object[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case int:
						stmt.BindInt(i + 1, (int)args[i]);
						break;
					case long:
						stmt.BindInt64(i + 1, (long)args[i]);
						break;
					case double:
						stmt.BindDouble(i + 1, (double)args[i]);
						break;
					case float:
						stmt.BindDouble(i + 1, Convert.ToDouble((float)args[i]));
						break;
					case string:
						stmt.BindText(i + 1, (string)args[i]);
						break;
					default:
					{
						if (!(args[i] is byte[]))
						{
							throw new InvalidOperationException("Unsupported Type:" + args[i].GetType());
						}
						stmt.BindBlob(i + 1, (byte[])args[i]);
						break;
					}
				}
			}
		}
        
		~SqliteDatabaseWrapper()
		{
			if (!_isDisposed)
			{
				Dispose();
			}
		}

		public void Dispose()
		{
			_isDisposed = true;
			Close();
			GC.SuppressFinalize(this);
		}

		private IntPtr _handle;

		private bool _isDisposed;
	}
}
