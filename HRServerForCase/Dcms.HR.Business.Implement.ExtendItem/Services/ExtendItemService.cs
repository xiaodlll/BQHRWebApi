﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行库版本:2.0.50727.42
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------
 

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml;
using System.IO;

using Dcms.Common.Torridity.Query;
using Dcms.Common.Torridity;
using Dcms.Common.Torridity.Metadata;
using Dcms.Common;
using Dcms.Common.Services;
using Dcms.Common.Extensions;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading;
using System.Linq;

namespace Dcms.HR.Services
{


    [ServiceClass(typeof(IExtendItemService), ServiceCreateType.Callback)]
    public class ExtendItemService : HRBusinessServiceEx<ExtendItem>, IExtendItemService
    {

        #region webapi接口

        private IDataEntityTypeList _dataEntityTypes;
        /// <summary>
        /// 所有注册的实体类型的列表
        /// </summary>
        private IDataEntityTypeList DataEntityTypes
        {
            get
            {
                if (_dataEntityTypes == null)
                {
                    _dataEntityTypes = Factory.GetService<IDataEntityTypeService>().DataEntityTypes;
                }
                return _dataEntityTypes;
            }
        }

        public void LoginHR()
        {
            IOnlineUserService onlineUserService = Factory.GetService<IOnlineUserService>();
            ILoginService loginService = Factory.GetService<ILoginService>();
            ILoginUser loginUser = null;
            try
            {
                loginUser = loginService.CurrentUser;
            }
            catch
            {
            }
            if (loginUser == null || loginService.CurrentUser == null || loginService.CurrentUser.Name != "SystemAdmin")
            {
                loginUser = loginService.Login("SystemAdmin", "dcms");

                #region 设置语言环境
                Thread uiThread = Thread.CurrentThread;
                string language = HRHelper.DefaultLanguage;
                if (!language.CheckNullOrEmpty())
                {
                    switch (language)
                    {
                        case "0":
                            uiThread.CurrentUICulture = new CultureInfo("en");
                            break;
                        case "1"://简体
                            uiThread.CurrentUICulture = new CultureInfo("zh-CN");
                            break;
                        case "2"://繁体
                            uiThread.CurrentUICulture = new CultureInfo("zh-CHT");
                            break;
                        default:
                            break;
                    }
                }
                CultureInfo myInfo = uiThread.CurrentCulture.Clone() as CultureInfo;
                DateTimeFormatInfo myDTFI = myInfo.DateTimeFormat;
                myDTFI.ShortDatePattern = "yyyy-MM-dd";
                myDTFI.LongDatePattern = "yyyy-MM-dd";
                myDTFI.ShortTimePattern = "HH:mm";
                myDTFI.LongTimePattern = "HH:mm:ss";
                uiThread.CurrentCulture = myInfo;
                #endregion
            }
        }

        public virtual string InvokeHRService(string pInput)
        {
            //LoginHR();
            string returnJson = string.Empty;
            APIResponse response = new APIResponse();
            response.State = "0";
            //TraceStopwatch ts = new TraceStopwatch();
            try
            {
                APIRequest request = (APIRequest)JsonConvert.DeserializeObject(pInput, typeof(APIRequest));
                if (request != null)
                {
                    string serviceType = request.ServiceType;
                    string method = request.Method;
                    List<Type> argTypes = new List<Type>();
                    List<Object> args = new List<object>();
                    if (request.Parameters != null)
                    {
                        foreach (APIRequestParameter parameter in request.Parameters)
                        {
                            Type parType = Type.GetType(parameter.Type);
                            argTypes.Add(parType);

                            object value = parameter.Value;
                            if (parameter.Value != null)
                            {
                                if (parType != typeof(string) && value.ToString().IndexOf("{") == 0)
                                {
                                    value = JsonConvert.DeserializeObject(pInput, parType);
                                }
                                else if (parType.Equals(typeof(Object)) || parType.Equals(typeof(string)) || (!parType.IsInterface && !parType.IsClass))
                                {
                                    if (parType.Equals(typeof(Guid)))
                                    {
                                        Guid temp = Guid.Empty;
                                        Guid.TryParse(parameter.Value.ToString(), out temp);
                                        value = temp;
                                    }
                                    else
                                    {
                                        value = Convert.ChangeType(parameter.Value, parType);
                                    }
                                }
                                else if (parType.IsClass)
                                {
                                    string a = "Digiwin.HR.XDataEntity.";//Dcms.HR.Business.DynamicAssembly
                                    IXmlSerializer serializer = OrmEntry.CreateXmlSerializer();
                                    IDataEntityType dt = GetDataEntityType(parType);
                                    value = serializer.Deserialize(dt, parameter.Value.ToString());
                                }
                            }
                            args.Add(value);
                        }
                    }
                    object result = null;
                    Type returnType = null;
                    Type callType = Type.GetType(serviceType);
                    object service = Factory.GetService(callType);
                    if (method.Equals("Save") || method.Equals("Delete"))
                        callType = typeof(IDocumentService);
                    if (method.Equals("Audit"))
                        callType = typeof(IAuditService);
                    MethodInfo methodInfo = service.GetType().GetMethod(method, argTypes.ToArray());//支持个案写法
                    //ts.RecordWithoutWatch("Service:" + service.ToString());
                    if (methodInfo == null)
                    {
                        throw new Exception("找不到方法:" + method);
                    }
                    if (method.Equals("Save") || method.Equals("Delete"))
                    {
                        methodInfo = callType.GetMethod(method, argTypes.ToArray());
                    }
                    try
                    {
                        result = methodInfo.Invoke(service, args.ToArray());
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                        {
                            throw new Exception(ex.InnerException.Message);
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    //if (result != null) {
                    //    ts.RecordWithoutWatch("Service:" + result.ToString());
                    //}
                    returnType = methodInfo.ReturnType;

                    response.ResultType = returnType.FullName;
                    if (response.ResultType.IndexOf("System.") < 0)
                    { //加上程序集
                        response.ResultType = string.Format(returnType.FullName + "," + returnType.Assembly.FullName);
                    }
                    if (returnType.Name != "Void")
                    {
                        if (result == null)
                        {
                            response.ResultValue = null;
                        }
                        else
                        {
                            //if (returnType.Equals(typeof(DataTable))) {
                            //    DataTable dtResult = result as DataTable;
                            //    foreach (DataColumn dc in dtResult.Columns) {
                            //        if (dc.DataType == typeof(DateTime)) {
                            //            dc.DateTimeMode = DataSetDateTime.Unspecified;
                            //        }
                            //    }
                            //    response.ResultValue = JsonConvert.SerializeObject(dtResult);
                            //} else {
                            //    response.ResultValue = JsonConvert.SerializeObject(result);
                            //}
                            response.ResultValue = JsonConvert.SerializeObject(result);
                        }
                        //else if (returnType.Equals(typeof(Object)) || returnType.Equals(typeof(string)) || (!returnType.IsInterface && !returnType.IsClass)) {
                        //    response.ResultValue = result.ToString();
                        //} else if (returnType.IsInterface || returnType.IsClass) {
                        //    response.ResultValue = JsonConvert.SerializeObject(result);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                response.State = "-1";
                response.Msg = ex.Message;
            }
            finally
            {
                returnJson = JsonConvert.SerializeObject(response);
            }
            //ts.RecordWithoutWatch("returnJson:" + returnJson);
            return returnJson;
        }

        public virtual string InvokeHRServiceEx(string pInput)
        {
            LoginHR();
            string returnJson = string.Empty;
            APIExResponse response = new APIExResponse();
            response.State = "0";
            //TraceStopwatch ts = new TraceStopwatch();
            try
            {
                APIRequest request = (APIRequest)JsonConvert.DeserializeObject(pInput, typeof(APIRequest));
                if (request != null)
                {
                    string serviceType = request.ServiceType;
                    string method = request.Method;
                    List<Type> argTypes = new List<Type>();
                    List<Object> args = new List<object>();
                    if (request.Parameters != null)
                    {
                        foreach (APIRequestParameter parameter in request.Parameters)
                        {
                            bool isArray = false;
                            string parameterType = parameter.Type;
                            if (parameter.Type.IndexOf("[]") > 0)
                            {
                                isArray = true;
                                parameterType = parameterType.Replace("[]", "");
                            }
                            Type parType = Type.GetType(parameterType);
                            bool isHREntity = false;
                            if (parameterType.IndexOf(".DataEntities.") > 0)
                            {
                                string entName = parameterType.Substring(parameter.Type.LastIndexOf('.') + 1);
                                isHREntity = true;
                                if (DataEntityTypes.Contains(entName))
                                {
                                    parType = DataEntityTypes[entName].CreateInstance().GetType();
                                }
                                else if (parameterType.IndexOf("ForAPIX") > 0)
                                {
                                    parType = Type.GetType(parameterType + ",DigiWin.HR.CaseBusiness");
                                }
                                else if (parameterType.IndexOf("ForAPI") > 0)
                                {
                                    parType = Type.GetType(parameterType + "," + serviceType.Substring(serviceType.LastIndexOf(',') + 1));
                                }
                                if (parType == null && parameterType.IndexOf(".IAuditObject") > 0)
                                {
                                    parType = Type.GetType("Dcms.HR.DataEntities.EmployeeTranslation,Dcms.HR.Business.EmployeeTranslation");
                                }
                            }
                            Type arrayType = parType;
                            if (isArray)
                            {
                                parType = parType.MakeArrayType();
                            }
                            argTypes.Add(parType);

                            object value = parameter.Value;
                            if (parameter.Value != null)
                            {
                                if (isHREntity)
                                {
                                    if (isArray)
                                    {
                                        JArray array = (JArray)(JsonConvert.DeserializeObject(parameter.Value.ToString()));
                                        if (array.Count > 0)
                                        {
                                            Array arrInstance = Array.CreateInstance(arrayType, array.Count);
                                            int index = 0;
                                            foreach (var item in array)
                                            {
                                                arrInstance.SetValue(JsonConvert.DeserializeObject(item.ToString(), arrayType), index++);
                                            }
                                            value = arrInstance;
                                        }
                                    }
                                    else
                                    {
                                        value = JsonConvert.DeserializeObject(parameter.Value.ToString(), parType);
                                    }
                                }
                                else
                                {
                                    if (isArray)
                                    {
                                        JArray array = (JArray)(JsonConvert.DeserializeObject(parameter.Value.ToString()));
                                        if (array.Count > 0)
                                        {
                                            Array arrInstance = Array.CreateInstance(arrayType, array.Count);
                                            int index = 0;
                                            foreach (var item in array)
                                            {
                                                arrInstance.SetValue(Convert.ChangeType(item.ToString(), arrayType), index++);
                                            }
                                            value = arrInstance;
                                        }
                                    }
                                    else
                                    {
                                        value = Convert.ChangeType(parameter.Value, parType);
                                    }
                                }
                            }
                            args.Add(value);
                        }
                    }
                    object result = null;
                    Type returnType = null;
                    Type callType = Type.GetType(serviceType);
                    object service = Factory.GetService(callType);
                    if (method.Equals("Save") || method.Equals("Delete"))
                        callType = typeof(IDocumentService);
                    if (method.Equals("Audit"))
                        callType = typeof(IAuditService);
                    MethodInfo methodInfo = service.GetType().GetMethod(method, argTypes.ToArray());//支持个案写法
                    //ts.RecordWithoutWatch("Service:" + service.ToString());
                    if (methodInfo == null)
                    {
                        string typeString = string.Empty;
                        foreach (var item in argTypes)
                        {
                            typeString += item.ToString() + ";";
                        }
                        throw new Exception(string.Format("找不到方法:{0} {1} Service:{2} Input:{3}", method, typeString, service.GetType(), pInput));
                    }
                    if (method.Equals("Save") || method.Equals("Delete"))
                    {
                        methodInfo = callType.GetMethod(method, argTypes.ToArray());
                    }
                    try
                    {
                        result = methodInfo.Invoke(service, args.ToArray());
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                        {
                            throw new Exception(ex.InnerException.ToString());
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    if (result != null)
                    {
                        // ts.RecordWithoutWatch("Service:" + result.ToString());
                    }
                    returnType = methodInfo.ReturnType;

                    response.ResultType = returnType.FullName;
                    if (response.ResultType.IndexOf("System.") < 0)
                    { //加上程序集
                        response.ResultType = string.Format(returnType.FullName + "," + returnType.Assembly.FullName);
                    }
                    if (returnType.Name != "Void")
                    {
                        if (result == null)
                        {
                            response.ResultValue = null;
                        }
                        else
                        {
                            response.ResultValue = result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.State = "-1";
                response.Msg = ex.ToString();
            }
            finally
            {
                returnJson = JsonConvert.SerializeObject(response).ToString();
            }
            //ts.RecordWithoutWatch("returnJson:" + returnJson);
            return returnJson;
        }

        private IDataEntityType GetDataEntityType(Type parType)
        {
            IDataEntityType type = null;
            string pathCase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin/DigiWin.HR.CaseVersion.dll");
            if (File.Exists(pathCase))
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin/Dcms.HR.Business.DynamicAssembly.dll");
                if (File.Exists(path))
                {
                    Assembly assembly = Assembly.LoadFrom(path);
                    if (parType != null)
                    {
                        string typeNameEx = string.Format("{0}.{1}", "Digiwin.HR.XDataEntity", parType.Name);
                        Type typeCase = assembly.GetType(typeNameEx);
                        if (typeCase != null)
                        {
                            type = OrmEntry.GetDataEntityType(typeCase);
                        }
                    }
                }
            }

            if (type == null)
            {
                type = OrmEntry.GetDataEntityType(parType);
            }
            return type;
        }

        #endregion
    }
}
