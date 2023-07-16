using System;

namespace DbLoader;

public class SqliteDatabase : IDisposable
{

	public SqliteDatabase(uint[] keys, string dbPath)
	{
		_keys = keys;
		_dbPath = dbPath;

	}
	public bool IsOpened { get; private set; }

	public void Open(bool enableDbFile)
	{
		if (IsOpened)
		{
			throw new InvalidOperationException("There is already an open connection");
		}

		if (!enableDbFile) return;
        
		var filename = SQLiteUtil.ToUtf8(
			string.Format("{1}.{2}.{3} {0}", _dbPath, _keys[0], _keys[1], _keys[2]), 
			true);
		var zVfs = SQLiteUtil.ToUtf8("klb_vfs", true);
        
		KlbSqlite3Dll.klbvfs_register();
		if (KlbSqlite3Dll.sqlite3_open_v2(filename, out this.Connection, 6, zVfs) != 0)
		{
			throw new InvalidOperationException("Could not open database file, incorrect keys?");
		}
		IsOpened = true;
		ConfigureConnection();
	}
    

	private void ConfigureConnection()
	{
		ExecuteNonQuery("PRAGMA temp_store=MEMORY; PRAGMA busy_timeout=100");
	}

	public void Close()
	{
		Dispose();
	}

	private void ExecuteNonQuery(string query)
	{
		ExecuteNonQuery(SQLiteUtil.ToUtf8(query, true));
	}

	private void ExecuteNonQuery(byte[] query)
	{
		if (!IsOpened)
		{
			throw new InvalidOperationException("ERROR: Can't execute the query. Db is closed.");
		}
		var zero = IntPtr.Zero;
		var num = KlbSqlite3Dll.sqlite3_exec(Connection, query, IntPtr.Zero, IntPtr.Zero, ref zero);
		if (num != 0)
		{
			throw new InvalidOperationException($"Sqlite exception code: {num}");
		}
	}

	public void Dispose()
	{
		if (IsOpened)
		{
			KlbSqlite3Dll.sqlite3_close(Connection);
			IsOpened = false;
		}
	}
    
	private readonly uint[] _keys;
		
	private readonly string _dbPath;

	public IntPtr Connection;
    
}