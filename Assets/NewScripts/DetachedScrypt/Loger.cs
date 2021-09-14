using System;
using System.IO;
using System.Text;
using UnityEngine;


namespace Clicker.DetachedScrypts
{
    public class Loger : MonoBehaviour
    {
        private const string LogFormat = "{0:HH:mm:ss} [{1}]: {2}\r";
        string persistantPath;
        string error_file_path;
        string dif_file_path;

        private void Start()
        {
            DateTime.Now.ToString("dd.mm.yyyy");
            dif_file_path = $"/{DateTime.Now.ToString("dd.mm.yyyy")}_different_logs.log";
            error_file_path = $"/{DateTime.Now.ToString("dd.mm.yyyy")}_error_logs.log";
            persistantPath = $"{Application.persistentDataPath}/Logs";
            if (!Directory.Exists(persistantPath))
                Directory.CreateDirectory(persistantPath);
            Application.logMessageReceived += logLogger;
        }

        private void logLogger(string condition, string stackTrace, LogType type)
        {
            string log = string.Format(LogFormat, DateTime.UtcNow, type, condition);
            if (type == LogType.Error)
                using (FileStream fs = File.Open(persistantPath + error_file_path, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    var bytes = Encoding.UTF8.GetBytes(log);
                    fs.Write(bytes, 0, bytes.Length);
                }
            else
                using (FileStream fs = File.Open(persistantPath + error_file_path, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    var bytes = Encoding.UTF8.GetBytes(log);
                    fs.Write(bytes, 0, bytes.Length);
                }
        }
    }
}