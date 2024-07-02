using Dcms.HR.DataEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HRWebApi
{
    public class ApiConfig
    {
        static Dictionary<string, APIRequest> _dic;
        static ApiConfig()
        {
            _dic = new Dictionary<string, APIRequest>();
        }

        public static string RequestJsonPath = string.Empty;

        public static void Init(string path)
        {
            RequestJsonPath = path;
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] fis = dir.GetFiles("*.json");
            StringBuilder sbError = new StringBuilder();
            foreach (FileInfo fi in fis)
            {
                try
                {
                    string content = string.Empty;
                    using (StreamReader sr = new StreamReader(fi.FullName, Encoding.UTF8))
                    {
                        content = sr.ReadToEnd();
                        sr.Close();
                    }
                    if (!string.IsNullOrEmpty(content))
                    {
                        APIRequest request = (APIRequest)JsonConvert.DeserializeObject(content, typeof(APIRequest));
                        _dic.Add(fi.Name.Remove(fi.Name.Length - 5), request);
                    }
                }
                catch (Exception ex)
                {
                    sbError.AppendLine("Json文件读取异常:" + fi.Name + "Error:" + ex.Message);
                }
            }
            if (sbError.Length > 0)
            {
                throw new Exception(sbError.ToString());
            }
        }


        public static APIRequest GetApiByCode(string code)
        {
            APIRequest api;
            if (_dic.TryGetValue(code, out api))
            {
                return (APIRequest)api.Clone();
            }

            return api;
        }
    }
}