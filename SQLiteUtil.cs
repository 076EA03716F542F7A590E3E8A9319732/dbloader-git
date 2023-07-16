using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbLoader
{
	public static class SQLiteUtil
	{
		public static string ReadUTF8String(IntPtr ptr, int len)
		{
			if (ptr == IntPtr.Zero || len == 0)
			{
				return string.Empty;
			}
			if (len < 0)
			{
				len = 0;
				while (Marshal.ReadByte(ptr, len) != 0)
				{
					len++;
				}
				if (len == 0)
				{
					return string.Empty;
				}
			}
			var array = new byte[len];
			Marshal.Copy(ptr, array, 0, len);
			return Encoding.UTF8.GetString(array, 0, len);
		}

		public static byte[] ToUtf8(string str, bool nullTerminate)
		{
			if (str == null)
			{
				return null;
			}
			return !nullTerminate 
				? Encoding.UTF8.GetBytes(str) 
				: Encoding.UTF8.GetBytes(str + "\0");
		}
        
	}
}
