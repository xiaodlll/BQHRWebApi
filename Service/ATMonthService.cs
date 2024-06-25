using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.HR.Services;
using Microsoft.AspNetCore.Http.Extensions;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using BQHRWebApi.Business;

namespace BQHRWebApi.Service
{
    public class ATMonthService : HRService
    {
        public virtual string CheckIsClose(string[] pEmployeeIds, DateTime pBeginDate, DateTime pEndDate)
        {
            DataTable dt = CheckCloseInfo(pEmployeeIds, pBeginDate, pEndDate);
            StringBuilder sb = new StringBuilder();
            foreach (DataRow dr in dt.Rows)
            {
                sb.AppendLine(dr["ErrorMsg"].ToString());
            }
            return sb.ToString();
        }


        private DataTable CheckCloseInfo(string[] pEmployeeIds, DateTime pBeginDate, DateTime pEndDate)
        {
            #region 員工考勤關帳
           // IAttendanceEmpCloseService empCloseService = Factory.GetService<IAttendanceEmpCloseService>();
            DataTable dt = CheckEmpCloseInfo(pEmployeeIds, pBeginDate, pEndDate);
            #endregion
            StringBuilder sb = new StringBuilder();
            foreach (string str in pEmployeeIds)
            {
                if (str.CheckNullOrEmpty()) continue;
                sb.AppendFormat(",'{0}'", str);
            }
            if (sb.Length > 0)
                sb.Remove(0, 1);
            //公司關帳判斷
            #region 找員工所屬公司和資料
            //根据员工找公司ID
            DataTable dtEmpCorp =HRHelper.ExecuteDataTable(string.Format(@" select employeeid,cnname,code,corporationId from employee where employeeid in ({0})",sb.ToString()));  //员工公司集合
           
            #endregion
            #region 變數宣告
            StringBuilder sbError = new StringBuilder();  //存储错误信息
            DataTable dtAllCor = GetATMonthByDate(pBeginDate, pEndDate);//所有公司考勤月
            Dictionary<string, DataTable> dicCorpAtMonth = new Dictionary<string, DataTable>();//公司考勤月集合
            string corporationId = string.Empty;
            DateTime beginDate = DateTime.MinValue;
            DateTime endDate = DateTime.MinValue;
            bool isClose = false;
            string errorMsg = "员工: {0} 日期: {1} - {2} 考勤期间已关账";
            if (pBeginDate == pEndDate)
            {
                errorMsg = "员工: {0} 日期: {1} 所属考勤期间已关账";
            }
            #endregion
            #region 循環員工
            foreach (string empId in pEmployeeIds)
            {
                DataTable dtTempAtMonth = null;
                DataRow[] empRows = dtEmpCorp.Select(string.Format("EmployeeId = '{0}'", empId));
                if (empRows.Length > 0)
                {
                    corporationId = empRows[0]["CorporationId"].ToString();
                    if (!dicCorpAtMonth.ContainsKey(corporationId))
                    {
                        dtTempAtMonth = GetAtMonthByCorpIdToRoot(corporationId, pBeginDate, pEndDate, dtAllCor);
                        dicCorpAtMonth.Add(corporationId, dtTempAtMonth);
                    }
                    else
                    {
                        dtTempAtMonth = dicCorpAtMonth[corporationId];
                    }
                    if (dtTempAtMonth != null && dtTempAtMonth.Rows.Count > 0)
                    {
                        #region 有找到公司考勤月
                        foreach (DataRow row in dtTempAtMonth.Rows)
                        {
                            corporationId = row["CorporationId"].ToString();
                            //判断BeginDate,EndDate,IsClose值是否异常
                            if (DateTime.TryParse(row["BeginDate"].ToString(), out beginDate)
                                && DateTime.TryParse(row["EndDate"].ToString(), out endDate)
                                && Boolean.TryParse(row["IsClose"].ToString(), out isClose))
                            {
                                //判断该区间已关账并且开始结束与传进来的开始结束是否交叉
                                if (isClose && !CheckDateAcross(pBeginDate, pEndDate, beginDate, endDate))
                                {
                                    DataRow dr = dt.NewRow();
                                    dr["BeginDate"] = row["BeginDate"];
                                    dr["EndDate"] = row["EndDate"];
                                    dr["CorporationId"] = corporationId.GetGuid();
                                    dr["EmployeeId"] = empId.GetGuid();
                                    //直接报公司的错误
                                    if (pBeginDate == pEndDate)
                                    {
                                        dr["ErrorMsg"] = string.Format(errorMsg, empRows[0]["CnName"].ToString(), pBeginDate.ToDateFormatString());
                                        //sb.AppendFormat(errorMsg, empRows[0]["CnName"].ToString(), pBeginDate.ToDateFormatString());
                                    }
                                    else
                                    {
                                        dr["ErrorMsg"] = string.Format(errorMsg, empRows[0]["CnName"].ToString(), beginDate.ToDateFormatString(), endDate.ToDateFormatString());
                                        //sb.AppendFormat(errorMsg, empRows[0]["CnName"].ToString(), beginDate.ToDateFormatString(), endDate.ToDateFormatString());
                                    }
                                    dt.Rows.Add(dr);
                                    //sb.AppendLine();
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            #endregion
            return dt;
        }

        /// <summary>
        /// 根据员工所属公司ID和归属日期区间获取考勤期间的开始和结束日期
        /// </summary>
        /// <param name="pCorporationId">员工所属公司ID</param>
        /// <param name="pBeginDate">开始日期</param>
        /// <param name="pEndDate">结束日期</param>
        /// <param name="pIsYear">是否查找归属日期所在的年段的开始结束日期</param>
        /// <remarks>使用范围：校验员工的请假时数是否大于最大带薪时数</remarks>
        /// <returns>开始和结束日期区间</returns>
        public virtual List<PeriodDate> GetBeginEndDate(string pCorporationId, DateTime pBeginDate, DateTime pEndDate, bool pIsYear)
        {
            List<PeriodDate> retValue = new List<PeriodDate>();
            string sql = string.Format(@"select CorporationId,BeginDate,EndDate,Year from ATMonth
                                        where '{0}' <= EndDate and '{1}' >=BeginDate
                                        order by BeginDate", pBeginDate.ToDateFormatString(), pEndDate.ToDateFormatString());
            DataTable dt = HRHelper.ExecuteDataTable(sql);
            var corData = dt.AsEnumerable().Where(t => t["CorporationId"].ToString() == pCorporationId);
            if (!corData.Any())
            {
                corData = dt.AsEnumerable().Where(t => t["CorporationId"].ToString() == Constants.SYSTEMGUID_CORPORATION_ROOT);
            }

            if (pIsYear)
            {
                var yearCorData = corData.GroupBy(t => t["Year"].ToString().ToInt());
                foreach (var item in yearCorData)
                {
                    sql = string.Format(@"select BeginDate,EndDate from ATMonth
                                        where CorporationId='{0}' and [Year]={1}
                                        order by BeginDate", item.First()["CorporationId"].ToString(), item.Key);
                    DataTable yearDt = HRHelper.ExecuteDataTable(sql);
                    PeriodDate pD = new PeriodDate();
                    pD.BeginDate = yearDt.AsEnumerable().First()["BeginDate"].ToString().ToDateTime();
                    pD.EndDate=yearDt.AsEnumerable().Last()["EndDate"].ToString().ToDateTime();
                    retValue.Add(pD );
                }
            }
            else
            {
                foreach (DataRow dr in corData)
                {
                    DateTime bDate = dr["BeginDate"].ToString().ToDateTime();
                    DateTime eDate = dr["EndDate"].ToString().ToDateTime();
                    PeriodDate pD = new PeriodDate();
                    pD.BeginDate = bDate;
                    pD.EndDate = eDate;
                    retValue.Add(pD);
                }
            }
            return retValue;
        }
        /// <summary>
        /// 取得考勤期間所有公司考勤月
        /// </summary>
        /// <param name="pBeginDate"></param>
        /// <param name="pEndDate"></param>
        /// <returns></returns>
        private DataTable GetATMonthByDate(DateTime pBeginDate, DateTime pEndDate)
        {
            DataTable dt = new DataTable();
           
             string sql= string.Format(@"SELECT [atmonth].[corporationid],
       [ATMonth_Corporation_CorporationId].[name],
       [atmonth].[begindate],
       [atmonth].[enddate],
       [atmonth].[isclose]
FROM   [atmonth] AS [ATMonth]
       LEFT JOIN [corporation] AS [ATMonth_Corporation_CorporationId]
              ON [atmonth].[corporationid] =
                 [ATMonth_Corporation_CorporationId].[corporationid]
WHERE  ( '{0}' BETWEEN [atmonth].[begindate] AND [atmonth].[enddate]
          OR '{1}' BETWEEN [atmonth].[begindate] AND [atmonth].[enddate] ) ", pBeginDate.Date, pEndDate.Date);
             
             dt=HRHelper.ExecuteDataTable(sql);
            return dt;
        }

        /// <summary>
        /// 获得公司某段区间的记录(往上找直到集團)
        /// </summary>
        /// <param name="pCorporationId">公司ID</param>
        /// <param name="pBeginDate">开始日期</param>
        /// <param name="pEndDate">结束日期</param>
        /// <returns>考勤期间集合（"CorporationId", "CorporationId.Name", "BeginDate", "EndDate", "IsClose"）</returns>
        private DataTable GetAtMonthByCorpIdToRoot(string pCorporationId, DateTime pBeginDate, DateTime pEndDate, DataTable pDt)
        {
            if (pDt == null || pDt.Rows.Count == 0)
            {
                return pDt;
            }
            else
            {
                DataRow[] corRows = pDt.Select(string.Format("CorporationId = '{0}'", pCorporationId));
                DataTable dt = pDt.Clone();
                foreach (DataRow row in corRows)
                {
                    dt.ImportRow(row);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt;
                }
                else if (pCorporationId == Constants.SYSTEMGUID_CORPORATION_ROOT)
                {
                    return dt;
                }
                else {
                    return null;
                }
                
            }

        }

        /// <summary>
        /// 检查员工的月度关账是否关账
        /// </summary>
        /// <param name="pEmployeeIds">员工ID数组</param>
        /// <param name="pBeginDate">开始日期</param>
        /// <param name="pEndDate">结束日期</param>
        /// <returns>關帳訊息</returns>
        public virtual DataTable CheckEmpCloseInfo(string[] pEmployeeIds, DateTime pBeginDate, DateTime pEndDate)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("BeginDate", typeof(DateTime));
            dt.Columns.Add("EndDate", typeof(DateTime));
            dt.Columns.Add("CorporationId", typeof(Guid));
            dt.Columns.Add("EmployeeId", typeof(Guid));
            dt.Columns.Add("ErrorMsg", typeof(string));
            DataTable dtEmpCloseInfo = GetEmployeeCloseInfo(pEmployeeIds, pBeginDate, pEndDate);
            if (dtEmpCloseInfo != null && dtEmpCloseInfo.Rows.Count > 0)
            {
                string empId = string.Empty;
                string empName = string.Empty;
                DateTime beginDate = DateTime.MinValue;
                DateTime endDate = DateTime.MinValue;
                foreach (DataRow row in dtEmpCloseInfo.Rows)
                {
                    empId = row["EmployeeId"].ToString();
                    empName = row["CnName"].ToString();
                    //判断BeginDate,EndDate值是否异常
                    if (DateTime.TryParse(row["BeginDate"].ToString(), out beginDate)
                        && DateTime.TryParse(row["EndDate"].ToString(), out endDate))
                    {
                        //判断该区间已关账并且开始结束与传进来的开始结束是否交叉
                        if (!CheckDateAcross(pBeginDate, pEndDate, beginDate, endDate))
                        {
                            DataRow dr = dt.NewRow();
                            dr["BeginDate"] = row["BeginDate"];
                            dr["EndDate"] = row["EndDate"];
                            dr["CorporationId"] = Guid.Empty;
                            dr["EmployeeId"] = empId.GetGuid();

                            if (pBeginDate == pEndDate)
                            {
                                dr["ErrorMsg"] = string.Format("员工: {0} 员工月度关账 已关账", string.Format("{0} {1}", empName, pBeginDate.ToDateFormatString()));
                            }
                            else
                            {
                                dr["ErrorMsg"] = string.Format("员工: {0} 员工月度关账 已关账", string.Format("{0} {1} ~ {2}", empName, beginDate.ToDateFormatString(), endDate.ToDateFormatString()));
                            }
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            return dt;
        }

        private DataTable GetEmployeeCloseInfo(string[] pEmployeeIds, DateTime pBeginDate, DateTime pEndDate)
        {

            StringBuilder sb = new StringBuilder();
            foreach (string str in pEmployeeIds)
            {
                if (str.CheckNullOrEmpty()) continue;
                sb.AppendFormat(",'{0}'", str);
            }
            if (sb.Length > 0)
                sb.Remove(0, 1);
            DataTable dtEmpCloseInfo = new DataTable();  //员工公司集合

            string sql = string.Format(@"SELECT attendanceempclose.employeeid,
       employee.cnname,
       atmonth.begindate,
       atmonth.enddate
FROM   attendanceempclose
       LEFT JOIN employee
              ON attendanceempclose.employeeid = employee.employeeid
       LEFT JOIN atmonth
              ON attendanceempclose.atmonthid = atmonth.atmonthid
WHERE  attendanceempclose.flag = 1
       AND attendanceempclose.isclose = 1
and attendanceempclose.employeeid in ({0})
       AND atmonth.begindate >= '{1}'
       AND atmonth.enddate <= '{2}' ",sb.ToString(), pBeginDate.Date.AddDays(-31), pEndDate.Date.AddDays(31));
            dtEmpCloseInfo = HRHelper.ExecuteDataTable(sql);
            return dtEmpCloseInfo;
        }


        public virtual bool CheckDateAcross(DateTime pBegin1, DateTime pEnd1, DateTime pBegin2, DateTime pEnd2)
        {
            if (pBegin1 > pEnd2 || pEnd1 < pBegin2)
            {
                return true;
            }
            return false;
        }
    }
}
