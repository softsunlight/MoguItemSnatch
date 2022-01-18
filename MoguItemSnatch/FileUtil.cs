using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MoguItemSnatch
{
    /// <summary>
    /// 文件操作工具类
    /// </summary>
    public class FileUtil
    {
        private static readonly Dictionary<string, object> writeLockDic = new Dictionary<string, object>();

        public static bool Write(string filePath, string content)
        {
            try
            {
                if (!writeLockDic.ContainsKey(filePath))
                {
                    writeLockDic[filePath] = new object();
                }
                lock (writeLockDic[filePath])
                {
                    File.WriteAllText(filePath, content);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write("FileUtil.Write Error", ex);
            }
            return false;
        }

        public static bool Read(string filePath, out string content)
        {
            content = string.Empty;
            try
            {
                content = File.ReadAllText(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write("FileUtil.Read Error", ex);
            }
            return false;
        }

    }
}
