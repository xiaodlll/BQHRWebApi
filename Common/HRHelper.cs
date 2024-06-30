//---------------------------------------------------------------- 
//Copyright (C) 2005-2006 Digital China Management System Co.,Ltd
//Http://www.Dcms.com.cn 
// All rights reserved.
//<author>wuyxb</author>
//<createDate>2006-7-12</createDate>
//<description>Global</description>
//---------------------------------------------------------------- 

using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.Common.Torridity.Metadata;
using Dcms.Common.Torridity;
using Dcms.HR.DataEntities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Dcms.HR.Services
{
    /// <summary>
    /// HR通用辅助类
    /// </summary>
    public partial class HRHelper
    {


        private static string _dbConnectionString;

        public static string DBConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_dbConnectionString))
                {
                    var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .Build();

                    _dbConnectionString = configuration["ConnectionStrings:HRMDB"];
                }
                return _dbConnectionString;
            }
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="pSql">SQL</param>
        /// <remarks>无权限检查，默认ConnectionString</remarks>
        public static void ExecuteNonQuery(string pSql)
        {
            #region 参数检查
            if (string.IsNullOrEmpty(pSql))
            {
                return;
            }
            #endregion

            SqlHelper.ExecuteNonQuery(DBConnectionString, CommandType.Text, pSql);
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="pSql">SQL</param>
        /// <remarks>无权限检查，默认ConnectionString</remarks>
        public static void ExecuteNonQueryWithTrans(List<string> pSqlList)
        {
            #region 参数检查
            if (pSqlList.Count == 0)
            {
                return;
            }
            #endregion
            using (SqlConnection connection = new SqlConnection(DBConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    foreach (string insertSql in pSqlList)
                    {
                        using (SqlCommand command = new SqlCommand(insertSql, connection))
                        {
                            command.Transaction = transaction;
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Handle exception
                    throw;
                }
            }
            // SqlHelper.ExecuteNonQuery(DBConnectionString, CommandType.Text, pSql);
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="pSql">SQL</param>
        /// <remarks>无权限检查，默认ConnectionString</remarks>
        public static void ExecuteNonQuery(string pSql, SqlParameter[] pParas)
        {
            #region 参数检查
            if (string.IsNullOrEmpty(pSql))
            {
                return;
            }
            #endregion

            SqlHelper.ExecuteNonQuery(DBConnectionString, CommandType.Text, pSql, pParas);
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="pSql">SQL</param>
        /// <remarks>无权限检查，默认ConnectionString</remarks>
        public static void ExecuteNonQueryWithTrans(string pSql, SqlParameter[] pParas)
        {
            #region 参数检查
            if (string.IsNullOrEmpty(pSql))
            {
                return;
            }
            #endregion

            SqlHelper.ExecuteNonQuery(DBConnectionString, CommandType.Text, pSql, pParas);
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="pSql">SQL</param>
        /// <returns>数据表</returns>
        public static DataTable ExecuteDataTable(string pSql)
        {
            #region 参数检查
            if (string.IsNullOrEmpty(pSql))
            {
                return null;
            }
            #endregion

            return SqlHelper.ExecuteDataset(DBConnectionString, CommandType.Text, pSql).Tables[0];
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="pSql">SQL</param>
        /// <returns>数据表</returns>
        public static DataTable ExecuteDataTable(string pSql, SqlParameter[] pParas)
        {
            #region 参数检查
            if (string.IsNullOrEmpty(pSql))
            {
                return null;
            }
            #endregion

            return SqlHelper.ExecuteDataset(DBConnectionString, CommandType.Text, pSql, pParas).Tables[0];
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="pSql">SQL</param>
        /// <returns>数据</returns>
        public static object ExecuteScalar(string pSql)
        {
            #region 参数检查
            if (string.IsNullOrEmpty(pSql))
            {
                return null;
            }
            #endregion

            return SqlHelper.ExecuteScalar(DBConnectionString, CommandType.Text, pSql);
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="pSql">SQL</param>
        /// <returns>数据</returns>
        public static object ExecuteScalar(string pSql, SqlParameter[] pParas)
        {
            #region 参数检查
            if (string.IsNullOrEmpty(pSql))
            {
                return null;
            }
            #endregion

            return SqlHelper.ExecuteScalar(DBConnectionString, CommandType.Text, pSql, pParas);
        }

        /// <summary>
        /// 大量插入資料
        /// </summary>
        /// <param name="pTable"></param>
        public static void BulkCopyInsert(DataTable pTable)
        {
            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(DBConnectionString))
            {
                sqlBulkCopy.BulkCopyTimeout = 3600;
                sqlBulkCopy.DestinationTableName = string.Format(@"[{0}]", pTable.TableName);
                sqlBulkCopy.WriteToServer(pTable);

            }
        }




        /// <summary>
        /// 查看Table是否存在
        /// </summary>
        /// <param name="pTableName"></param>
        /// <returns></returns>
        public static bool IsExistTable(string pTableName, string pColumName = "")
        {
            string sql = string.Format(@"SELECT * FROM syscolumns WHERE id = object_id('{0}') ", pTableName);
            if (pColumName != null)
            {
                sql += string.Format(@" AND [name] = '{0}'", pColumName);
            }
            return ExecuteDataTable(sql).Rows.Count > 0;
        }

        public static DataTable GetCodeInfo(string kindname)
        {
            string sql = string.Format("select CodeInfoId, InfoCode, ScName from CodeInfo where KindName = '{0}' and Enabled=1 ", kindname);
            return ExecuteDataTable(sql);
        }

        public static bool isExistFormNumber(string table,string formType,string formNumber) {
            string sql = string.Format("select * from {0} where ESSType = '{1}' and ESSNo='{2}' ", table,formType,formNumber);
            DataTable dt = ExecuteDataTable(sql);
            if(dt!=null&&dt.Rows.Count>0){
                return true;
            }
            return false;
        }

        /// <summary>
        /// 將DataTable轉List
        /// </summary>
        /// <typeparam name="T">list中的類型</typeparam>
        /// <param name="dt">要轉換的DataTable</param>
        /// <returns></returns>
        public static List<T> DataTableToList<T>(DataTable dt) where T : class, new()
        {
            List<T> list = new List<T>();
            T t = new T();
            PropertyInfo[] prop = t.GetType().GetProperties();
            //遍歷所有DataTable的行
            foreach (DataRow dr in dt.Rows)
            {
                t = new T();
                //通過反射獲取T類型的所有成員
                foreach (PropertyInfo pi in prop)
                {
                    //DataTable列名=屬性名(不分大小寫)
                    if (dt.Columns.IndexOf(pi.Name) >= 0)
                    {
                        //屬性值不為空
                        if (dr[pi.Name] != DBNull.Value)
                        {
                            object value = Convert.ChangeType(dr[pi.Name], pi.PropertyType);
                            //給T類型字段賦值
                            pi.SetValue(t, value, null);
                        }
                    }
                }
                //將T類型添加到集合list
                list.Add(t);
            }
            return list;
        }

        public static List<ExpandoObject> ConvertToExpandoObjects(DataTable dt)
        {
            var list = new List<ExpandoObject>();
            foreach (DataRow row in dt.Rows)
            {
                dynamic expando = new ExpandoObject();
                var dict = (IDictionary<string, object>)expando;

                foreach (DataColumn column in dt.Columns)
                {
                    dict[column.ColumnName] = row[column];
                }

                list.Add(expando);
            }
            return list;
        }



        public static Dictionary<int, List<ExpandoObject>> ConvertToExpandoObjects(Dictionary<int, DataTable> dic)
        {
            var res = new Dictionary<int, List<ExpandoObject>>();
            foreach (int i in dic.Keys)
            {
                var list = new List<ExpandoObject>();
                DataTable dt = dic[i];
                foreach (DataRow row in dt.Rows)
                {
                    dynamic expando = new ExpandoObject();
                    var dict = (IDictionary<string, object>)expando;

                    foreach (DataColumn column in dt.Columns)
                    {
                        dict[column.ColumnName] = row[column];
                    }

                    list.Add(expando);
                }
                res.Add(i, list);
            }
            return res;
        }

        public static string GetArrayToStrBySQL(IEnumerable pArray)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var str in pArray)
            {
                sb.AppendFormat(",'{0}'", str.ToString());
            }
            if (sb.Length > 0)
                sb.Remove(0, 1);
            return sb.ToString();
        }

        public static string GenerateSqlInsert<T>(T obj, string tableName)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columns = new StringBuilder();
            var values = new StringBuilder();

            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(obj);

                if (propertyValue != null)
                {
                    columns.AppendFormat("[{0}],", property.Name);
                    if (propertyValue is string || propertyValue is char)
                    {
                        values.AppendFormat("'{0}',", propertyValue.ToString().Replace("'", "''"));
                    }
                    else if (propertyValue is DateTime)
                    {
                        DateTime dt1 = (DateTime)propertyValue;
                        values.AppendFormat("'{0}',", dt1.ToString("yyyy-MM-dd"));
                    }
                    else if (propertyValue is Boolean)
                    {
                        bool tf = false;
                        Boolean.TryParse(propertyValue.ToString(), out tf);
                        values.AppendFormat("'{0}',", tf == true ? 1 : 0);
                    }
                    else
                    {
                        values.AppendFormat("{0},", propertyValue);
                    }
                }
            }

            if (columns.Length > 0)
            {
                columns.Length--; // 移除最后一个逗号
            }
            if (values.Length > 0)
            {
                values.Length--; // 移除最后一个逗号
            }

            return string.Format("INSERT INTO [{0}] ({1},Flag,CreateBy,CreateDate,LastModifiedBy,LastModifiedDate,OwnerId) VALUES ({2},1,'98385A19-5BA6-43E5-BD0A-6A727F2E9C35',GETDATE(),'98385A19-5BA6-43E5-BD0A-6A727F2E9C35',GETDATE(),'Bad');", tableName, columns, values);
        }


        public static string GenerateSqlInsertMulti<T>(List<T> entities, string tableName)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columns = new StringBuilder();
            var values = new StringBuilder();
            StringBuilder sbSqls = new StringBuilder();
            foreach (var obj in entities)
            {
                foreach (var property in properties)
                {
                    var propertyValue = property.GetValue(obj);

                    if (propertyValue != null)
                    {
                        columns.AppendFormat("[{0}],", property.Name);
                        if (propertyValue is string || propertyValue is char)
                        {
                            values.AppendFormat("'{0}',", propertyValue.ToString().Replace("'", "''"));
                        }
                        else if (propertyValue is DateTime)
                        {
                            DateTime dt1 = (DateTime)propertyValue;
                            values.AppendFormat("'{0}',", dt1.ToString("yyyy-MM-dd"));
                        }
                        else if (propertyValue is Boolean)
                        {
                            bool tf = false;
                            Boolean.TryParse(propertyValue.ToString(), out tf);
                            values.AppendFormat("'{0}',", tf == true ? 1 : 0);
                        }
                        else
                        {
                            values.AppendFormat("{0},", propertyValue);
                        }
                    }
                }

                if (columns.Length > 0)
                {
                    columns.Length--; // 移除最后一个逗号
                }
                if (values.Length > 0)
                {
                    values.Length--; // 移除最后一个逗号
                }
                sbSqls.Append(string.Format("INSERT INTO [{0}] ({1},Flag,CreateBy,CreateDate,LastModifiedBy,LastModifiedDate,OwnerId) VALUES ({2},1,'98385A19-5BA6-43E5-BD0A-6A727F2E9C35',GETDATE(),'98385A19-5BA6-43E5-BD0A-6A727F2E9C35',GETDATE(),'Bad');", tableName, columns, values));
            }
            return sbSqls.ToString();
        }


        public static DataTable ConvertToDataTable<T>(List<T> entities)
        {
            // 创建一个新的DataTable
            DataTable dataTable = new DataTable(typeof(T).Name);

            // 获取实体的所有属性
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 遍历属性，并为DataTable添加相应的列
            foreach (PropertyInfo prop in properties)
            {
                // 添加列到DataTable
                dataTable.Columns.Add(prop.Name, prop.PropertyType);
            }

            // 遍历实体集合，为每个实体添加一行到DataTable
            foreach (T entity in entities)
            {
                DataRow row = dataTable.NewRow();

                foreach (PropertyInfo prop in properties)
                {
                    row[prop.Name] = prop.GetValue(entity, null) ?? DBNull.Value;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public static T WebAPIEntitysToDataEntity<T>(object pWebData) where T : class, new()
        {
            if (pWebData == null)
                throw new ArgumentNullException(nameof(pWebData));

            return WebAPIEntitysToDataEntitys<T>(new object[] { pWebData })[0];
        }

        public static List<T> WebAPIEntitysToDataEntitys<T>(object[] pWebDatas) where T : class, new()
        {
            if (pWebDatas == null)
                throw new ArgumentNullException(nameof(pWebDatas));

            var resultList = new List<T>();

            foreach (var webData in pWebDatas)
            {
                T dataEntity = Activator.CreateInstance<T>();

                // 获取T类型的公共属性
                PropertyInfo[] targetProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // 遍历webData的属性，查找与T类型中同名且类型匹配的属性并赋值
                PropertyInfo[] sourceProperties = webData.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var sourceProp in sourceProperties)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(sourceProp.PropertyType) && sourceProp.PropertyType != typeof(string))
                    {//是集合类型
                        PropertyInfo targetProp = targetProperties.FirstOrDefault(p => p.Name == sourceProp.Name);
                        if (targetProp != null)
                        {
                            // 获取集合元素的类型
                            Type elementType = targetProp.PropertyType.BaseType.GetGenericArguments()[0];

                            // 转换源集合为目标集合类型
                            IEnumerable sourceCollection = (IEnumerable)sourceProp.GetValue(webData);
                            object targetCollection = targetProp.GetValue(dataEntity);

                            foreach (var item in sourceCollection)
                            {
                                object element = Activator.CreateInstance(elementType);

                                // 获取集合元素的属性
                                PropertyInfo[] elementProperties = elementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                                PropertyInfo[] sourceItemProperties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                                foreach (var sourceElementProp in sourceItemProperties)
                                {
                                    PropertyInfo targetElementProp = elementProperties.FirstOrDefault(p => p.Name == sourceElementProp.Name);
                                    if (targetElementProp != null)
                                    {
                                        object value = sourceElementProp.GetValue(item);
                                        targetElementProp.SetValue(element, value);
                                    }
                                }

                                // 将转换后的元素添加到目标集合
                                targetCollection.GetType().GetMethod("Add").Invoke(targetCollection, new[] { element });
                            }
                        }
                    }
                    else
                    {
                        PropertyInfo targetProp = targetProperties.FirstOrDefault(p => p.Name == sourceProp.Name);
                        if (targetProp != null)
                        {
                            // 如果T中有与webData同名且类型匹配的属性，则复制值
                            object value = sourceProp.GetValue(webData);
                            targetProp.SetValue(dataEntity, value);
                        }
                    }
                }

                resultList.Add(dataEntity);
            }

            return resultList;
        }

        /// <summary>
        /// 將DataEntity轉成WebAPIEntity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pEntityDatas"></param>
        /// <returns></returns>
        public static List<T> DataEntitysToWebAPIEntitys<T>(IDataEntity[] pEntityDatas) where T : class, new()
        {
            List<T> list = new List<T>();
            foreach (IDataEntity entityData in pEntityDatas)
            {
                IDataEntityType entityType = entityData.GetDataEntityType();
                T webData = new T();
                PropertyInfo[] webProp = webData.GetType().GetProperties();
                foreach (PropertyInfo webPi in webProp)
                {
                    #region 簡單屬性
                    if (entityType.SimpleProperties.Contains(webPi.Name))
                    {
                        webPi.SetValue(webData, entityType.SimpleProperties[webPi.Name].GetValue((IDataEntityBase)entityData), null);
                    }
                    #endregion
                    #region  集合屬性
                    if (entityType.CollectionProperties.Contains(webPi.Name))
                    {
                        IList entityInfoList = entityType.CollectionProperties[webPi.Name].GetValue(entityData);
                        IList webInfoList = webPi.GetValue(webData, null) as IList;
                        Type webInfoType = webInfoList.GetType().GetGenericArguments().First();
                        PropertyInfo[] webInfoProp = webInfoType.GetProperties();
                        foreach (IDataEntity entityInfo in entityInfoList)
                        {
                            IDataEntityType entityInfoType = entityInfo.GetDataEntityType();
                            object webInfo = Activator.CreateInstance(webInfoType);//創建物件 
                            foreach (PropertyInfo webInfoPi in webInfoProp)
                            {
                                if (entityInfoType.SimpleProperties.Contains(webInfoPi.Name))
                                {
                                    webInfoPi.SetValue(webInfo, entityInfoType.SimpleProperties[webInfoPi.Name].GetValue((IDataEntityBase)entityInfo), null);
                                }
                            }
                            webInfoList.Add(webInfo);
                        }
                    }
                    #endregion
                }
                list.Add(webData);
            }
            return list;
        }


        /// <summary>
        /// 將WebAPIEntity轉成DataEntity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pFormType"></param>
        /// <param name="pFormNumber"></param>
        /// <param name="pWebDatas"></param>
        /// <returns></returns>
        public static List<T> WebAPIEntitysToDataEntitys<T>(string pFormType, string pFormNumber, object[] pWebDatas) where T : class, IDataEntity, new()
        {
            List<T> list = new List<T>();
            foreach (object webData in pWebDatas)
            {
                T t = new T();
                IDataEntity entity = t as IDataEntity;
                IDataEntityType entityType = entity.GetDataEntityType();
                PropertyInfo[] webProp = webData.GetType().GetProperties();
                foreach (PropertyInfo webPi in webProp)
                {
                    #region 簡單屬性
                    if (entityType.SimpleProperties.Contains(webPi.Name))
                    {
                        entityType.SimpleProperties[webPi.Name].SetValue((IDataEntityBase)t, webPi.GetValue(webData, null));
                    }
                    #endregion
                    #region 集合屬性
                    if (entityType.CollectionProperties.Contains(webPi.Name))
                    {
                        IList entityInfoList = entityType.CollectionProperties[webPi.Name].GetValue(entity);
                        IList webInfoList = webPi.GetValue(webData, null) as IList;
                        Type webInfoType = webInfoList.GetType().GetGenericArguments().First();
                        IDataEntityType entityInfoType = entityType.CollectionProperties[webPi.Name].ItemDataEntityType;
                        PropertyInfo[] webInfoProp = webInfoType.GetProperties();
                        foreach (object webInfo in webInfoList)
                        {
                            object entityInfo = entityInfoType.CreateInstance();//創建物件 
                            foreach (PropertyInfo webInfoPi in webInfoProp)
                            {
                                if (entityInfoType.SimpleProperties.Contains(webInfoPi.Name))
                                {
                                    entityInfoType.SimpleProperties[webInfoPi.Name].SetValue((IDataEntityBase)entityInfo, webInfoPi.GetValue(webInfo, null));
                                }
                            }
                            if (entityInfoType.PrimaryKey.DbType == GeneralDbType.Guid)
                            {
                                entityInfoType.PrimaryKey.SetValue((IDataEntityBase)entityInfo, SequentialGuid.NewGuid());
                            }
                            entityInfoList.Add(entityInfo);
                        }
                    }
                    #endregion
                }
                #region PrimaryKey
                if (entityType.PrimaryKey.DbType == GeneralDbType.Guid)
                {
                    entityType.PrimaryKey.SetValue((IDataEntityBase)entity, SequentialGuid.NewGuid());
                }
                #endregion
                #region flagObj
                IFlagObject flagObj = entity as IFlagObject;
                if (flagObj != null)
                {
                    flagObj.Flag = true;
                }
                #endregion
                #region auditObj
                IAuditObject auditObj = entity as IAuditObject;
                if (auditObj != null)
                {
                    auditObj.StateId = Constants.PS02;
                }
                #endregion
                #region DataModifyObj
                Dcms.Common.DataEntities.IDataModifyObject DataModifyObj = entity as Dcms.Common.DataEntities.IDataModifyObject;
                if (DataModifyObj != null)
                {
                    DataModifyObj.CreateDate = DateTime.Now;
                    DataModifyObj.CreateBy = Constants.SYSTEMGUID_USER_ADMINISTRATOR.GetGuid();
                    DataModifyObj.LastModifiedDate = DateTime.Now;
                    DataModifyObj.LastModifiedBy = Constants.SYSTEMGUID_USER_ADMINISTRATOR.GetGuid();
                }
                #endregion
                #region EssObject
                IEssObject essObj = entity as IEssObject;
                if (essObj != null)
                {
                    essObj.IsEss = true;
                    essObj.IsFromEss = true;
                    essObj.EssType = pFormType;
                    essObj.EssNo = pFormNumber;
                }
                #endregion

                t.ExtendedProperties.Add("api", "api");
                list.Add(t);
            }
            return list;
        }

    }





}
