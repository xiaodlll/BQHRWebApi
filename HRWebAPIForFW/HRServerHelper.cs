using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dcms.Common.Services;
using Dcms.HR.Services;
using Dcms.HR.Business;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Dcms.HR.DataEntities;

namespace HRWebApi {
    public class HRServerHelper {

        public static object GetHRService(Type type) {
            string server = System.Configuration.ConfigurationManager.AppSettings["ServerIP"];
            string port = System.Configuration.ConfigurationManager.AppSettings["ServerPort"];

            string url = string.Format("tcp://{0}:{1}/{2}", server, port, "ServiceProvider.rem");
            IServiceProvider serviceProvider = (IServiceProvider)Activator.GetObject(typeof(IServiceProvider), url);

            object service = serviceProvider.GetService(type);
            return service;
        }

        public static object GetResultBySQL(string pSql) {
            DataTable dt = GetData(pSql);
            if (dt.Rows.Count > 0) {
                return dt.Rows[0][0].ToString();
            }
            return null;
        }
        public static DataTable GetData(string cmdSQL) {
            APIRequest request = new APIRequest();
            request.ServiceType = "Dcms.HR.Services.IIntegrationCopyService,Dcms.HR.Business.Integration";
            request.Method = "ExecuteSelectSql";
            List<APIRequestParameter> list = new List<APIRequestParameter>();
            APIRequestParameter para = new APIRequestParameter();
            para.Name = "sql";
            para.Type = "System.String";
            para.Value = cmdSQL;
            list.Add(para);
            request.Parameters = list.ToArray();
            //LoggerHelper.Info(string.Format("GetData Request:{0}", Newtonsoft.Json.JsonConvert.SerializeObject(request).ToString()));
            string resultJson = HRServerHelper.GetHRService<IExtendItemService>().InvokeHRService(JsonConvert.SerializeObject(request));
            APIResponse response = JsonConvert.DeserializeObject<APIResponse>(resultJson);
            //LoggerHelper.Info(string.Format("GetData Response:{0}", Newtonsoft.Json.JsonConvert.SerializeObject(response).ToString()));
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(response.ResultValue);
            return dt;
        }

        public static T GetHRService<T>() {
            T t = (T)(GetHRService(typeof(T)));
            return t;
        }

        /// <summary>
        /// 调用HR服务
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<APIResponse> CallService(APIRequest request)
        {
            string resultJson = HRServerHelper.GetHRService<IExtendItemService>().InvokeHRServiceEx(JsonConvert.SerializeObject(request));

            return await Task.FromResult<APIResponse>(JsonConvert.DeserializeObject<APIResponse>(resultJson));
        }
        /// <summary>
        /// 调用HR服务
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<APIExResponse> CallServiceEx(APIRequest request) {
            string resultJson = HRServerHelper.GetHRService<IExtendItemService>().InvokeHRServiceEx(JsonConvert.SerializeObject(request));

            return await Task.FromResult<APIExResponse>(JsonConvert.DeserializeObject<APIExResponse>(resultJson));
        }

        public static APIResponse CallServiceNotAsync(APIRequest request) {
            string resultJson = HRServerHelper.GetHRService<IExtendItemService>().InvokeHRService(JsonConvert.SerializeObject(request));

            return (APIResponse)(JsonConvert.DeserializeObject<APIResponse>(resultJson));
        }
    }
}
