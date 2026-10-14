using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.Utilities.Services
{
    public class LogServices
    {
        public string SettingPath
        {
            get
            {
                var folder_path = $"{AppDomain.CurrentDomain.BaseDirectory}AppFiles";
                if (!System.IO.Directory.Exists(folder_path)) System.IO.Directory.CreateDirectory(folder_path);
                return System.IO.Path.Combine(folder_path, "Settings.json");
            }
        }
        public string RemoveDoubleSpace(string str)
        {
            string ret = str;
            if (string.IsNullOrEmpty(ret)) return string.Empty;
            try
            {
                while (ret.Contains("  "))
                {
                    ret = ret.Replace("  ", " ").Trim();
                }
            }
            catch (Exception ex)
            {
                ErrorLogToFile(ex.Message, ex.StackTrace, nameof(RemoveDoubleSpace));
            }
            return ret;
        }
        public void ErrorLogToFile(string ErrorMsg, string StackTraceMsg, string errWhere)
        {
            try
            {
                string pth = System.AppDomain.CurrentDomain.BaseDirectory + "\\AppLog\\" + DateTime.Now.ToString("yyyyMMdd") + "\\";
                if (!Directory.Exists(pth))
                {
                    Directory.CreateDirectory(pth);
                }

                if (!File.Exists(pth + "Log.info"))
                {
                    using (StreamWriter sWriter = File.CreateText(pth + "Log.info"))
                    {
                        sWriter.WriteLine("Time             : " + DateTime.Now.ToString("HH:mm:ss"));
                        sWriter.WriteLine("Error            : " + ErrorMsg.Trim('\0', ' ', '\n', '\r'));
                        sWriter.WriteLine("StackTrace       : " + StackTraceMsg.Trim('\0', ' ', '\n', '\r'));
                        sWriter.WriteLine("Error Occurred In : " + errWhere);
                        sWriter.WriteLine();
                        sWriter.WriteLine();
                    }
                }
                else
                {
                    using (StreamWriter sWriter = File.AppendText(pth + "Log.info"))
                    {
                        sWriter.WriteLine("Time             : " + DateTime.Now.ToString("HH:mm:ss"));
                        sWriter.WriteLine("Error            : " + ErrorMsg.Trim('\0', ' ', '\n', '\r'));
                        sWriter.WriteLine("StackTrace       : " + StackTraceMsg.Trim('\0', ' ', '\n', '\r'));
                        sWriter.WriteLine("Error Occurred In : " + errWhere);
                        sWriter.WriteLine();
                        sWriter.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it to another location if needed.
            }
        }
        public void Serialize<T>(string path, T any)
        {
            if (any != null)
            {
                var json = JsonConvert.SerializeObject(any);
                System.IO.File.WriteAllText(path, json);
            }
        }
        public T Deserialize<T>(string path)
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json);
            }
            return default;
        }
    }
}
