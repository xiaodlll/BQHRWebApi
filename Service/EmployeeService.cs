using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.HR.Services;
using Microsoft.AspNetCore.Http.Extensions;
using System.Data;
using System.Linq.Expressions;
using System.Resources;

namespace BQHRWebApi.Service
{
    public class EmployeeService : HRService
    {
        public EmployeeService() { }

        /// <summary>
        /// 根据员工ID获取员工姓名
        /// </summary>
        /// <param name="pEmployeeId"></param>
        /// <returns></returns>
        public  string GetEmployeeNameById(string pEmployeeId)
        {
            #region 参数检查
            if (pEmployeeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("pEmployeeId Error");
            }
            #endregion
         
            DataTable dt = HRHelper.ExecuteDataTable(string.Format("select CnName from employee where employeeid='{0}'",pEmployeeId));

            if (dt != null && dt.Rows.Count > 0)
            {

                return dt.Rows[0][0].ToString();
            }

            return string.Empty;

        }

        public string GetEmpFiledById(string pEmployeeId,string pField) {
            #region 参数检查
            if (pEmployeeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("pEmployeeId Error");
            }
            #endregion

            DataTable dt = HRHelper.ExecuteDataTable(string.Format("select {1} from employee where employeeid='{0}'", pEmployeeId, pField));

            if (dt != null && dt.Rows.Count > 0)
            {

                return dt.Rows[0][0].ToString();
            }

            return string.Empty;
        }

    }
}
