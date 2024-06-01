//---------------------------------------------------------------- 
//Copyright (C) 2005-2006 Digital China Management System Co.,Ltd
//Http://www.Dcms.com.cn 
// All rights reserved.
//<author>wuyxb</author>
//<createDate>2006-7-12</createDate>
//<description>Global</description>
//---------------------------------------------------------------- 

using BQHRWebApi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dcms.HR.Services {
    /// <summary>
    /// HR通用辅助类
    /// </summary>
    public partial class HRHelper {


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
        public static void ExecuteNonQuery(string pSql) {
            #region 参数检查
            if (string.IsNullOrEmpty(pSql)) {
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
        public static void ExecuteNonQuery(string pSql, SqlParameter[] pParas) {
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
        public static DataTable ExecuteDataTable(string pSql) {
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
        public static DataTable ExecuteDataTable(string pSql, SqlParameter[] pParas) {
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
        public static object ExecuteScalar(string pSql) {
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
        public static object ExecuteScalar(string pSql, SqlParameter[] pParas) {
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
            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(DBConnectionString)) {
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

        /// <summary>
        /// 將DataTable轉List
        /// </summary>
        /// <typeparam name="T">list中的類型</typeparam>
        /// <param name="dt">要轉換的DataTable</param>
        /// <returns></returns>
        public static List<T> DataTableToList<T>(DataTable dt) where T : class, new() {
            List<T> list = new List<T>();
            T t = new T();
            PropertyInfo[] prop = t.GetType().GetProperties();
            //遍歷所有DataTable的行
            foreach (DataRow dr in dt.Rows) {
                t = new T();
                //通過反射獲取T類型的所有成員
                foreach (PropertyInfo pi in prop) {
                    //DataTable列名=屬性名(不分大小寫)
                    if (dt.Columns.IndexOf(pi.Name)>=0) {
                        //屬性值不為空
                        if (dr[pi.Name] != DBNull.Value) {
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

    }
}
