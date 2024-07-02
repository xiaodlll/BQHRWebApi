using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.HR.Services;
using System.Data;
using System.Text;

namespace BQHRWebApi.Service
{
    public class EmployeeService : HRService
    {
        public EmployeeService() { }

        public string GetEmpIdByCode(string empCode)
        {
            #region 参数检查
            if (empCode.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("empCode Error");
            }
            #endregion

            DataTable dt = HRHelper.ExecuteDataTable(string.Format("select EmployeeId from employee where Code='{0}'", empCode));

            if (dt != null && dt.Rows.Count > 0)
            {

                return dt.Rows[0][0].ToString();
            }

            return string.Empty;
        }
        /// <summary>
        /// 根据员工ID获取员工姓名
        /// </summary>
        /// <param name="pEmployeeId"></param>
        /// <returns></returns>
        public string GetEmployeeNameById(string pEmployeeId)
        {
            #region 参数检查
            if (pEmployeeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("pEmployeeId Error");
            }
            #endregion

            DataTable dt = HRHelper.ExecuteDataTable(string.Format("select CnName from employee where employeeid='{0}'", pEmployeeId));

            if (dt != null && dt.Rows.Count > 0)
            {

                return dt.Rows[0][0].ToString();
            }

            return string.Empty;

        }

        public string GetEmpFiledById(string pEmployeeId, string pField)
        {
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


        public DataTable GetEmployeeInfoByIds(string[] pEmployeeIds)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in pEmployeeIds)
            {
                if (str.CheckNullOrEmpty()) continue;
                sb.AppendFormat(",'{0}'", str);
            }
            if (sb.Length > 0)
                sb.Remove(0, 1);

            return HRHelper.ExecuteDataTable(string.Format("select * from employee where employeeid in ({0})", sb.ToString()));
        }


    }
}
