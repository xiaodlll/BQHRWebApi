using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.HR.Services;
using Microsoft.AspNetCore.Http.Extensions;
using System.Data;
using System.Linq.Expressions;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;

namespace BQHRWebApi.Service
{
    public class AttendanceEmpRankService : HRService
    {

        /// <summary>
        /// 校验员工是否有班次
        /// </summary>
        /// <param name="pEmployeeIds"></param>
        /// <param name="pBeginDate"></param>
        /// <param name="pEndDate"></param>
        /// <returns></returns>
        public virtual string CheckEmpHasRank(string pEmployeeIds, DateTime pBeginDate, DateTime pEndDate)
        {
            string[] employeeids = pEmployeeIds.Split('|');
            if (employeeids.Length == 0)
            {
                employeeids = new string[] { pEmployeeIds };
            }
            return CheckEmpHasRank(employeeids, pBeginDate, pEndDate);
        }
        /// <summary>
        /// 校验员工是否有班次
        /// </summary>
        /// <param name="pEmployeeIds"></param>
        /// <param name="pBeginDate"></param>
        /// <param name="pEndDate"></param>
        /// <returns></returns>
        public virtual string CheckEmpHasRank(string[] pEmployeeIds, DateTime pBeginDate, DateTime pEndDate)
        {
            StringBuilder sbMsg = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            string tempString = string.Empty;
            DataRow[] rows = null;

            foreach (string str in pEmployeeIds)
            {
                if (str.CheckNullOrEmpty()) continue;
                sb.AppendFormat(",'{0}'", str);
            }
            if (sb.Length > 0)
                sb.Remove(0, 1);
            DataTable dt = new DataTable();
            //20110902 added by songyj for 跨天请假的问题
            DataTable dtEndDateRank = new DataTable();
            DataRow[] drEndDateRanks = null;
            //pEndDate：原来传入的参数是日期，现在传入的是日期+时间
            DateTime tempEnd = pEndDate;
            //还原为日期
            pEndDate = tempEnd.Date;

          
                //20141117 added by lidong for 增加已审核同意判断 Q00-20141107006
             string   strSql = string.Format(@"Select EmployeeId,Date From AttendanceEmpRank Where EmployeeId in ({0}) And Date >='{1}' and Date<='{2}'
                                        and StateId='PlanState_003' and ApproveResultId='OperatorResult_001'  ", sb.ToString(), pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.ToDateFormatString());
             dt = HRHelper.ExecuteDataTable(strSql);
                //20110902 added by songyj for 查找请假的结束日期减一天的排版是否是跨天的
                //20140901 add by LinBJ for 增加歸屬前一天的班次判斷
                strSql = string.Format(@"Select empRank.EmployeeId,empRank.Date, 
                                                   CAST((CAST(CONVERT(CHAR, empRank.Date, 20) AS CHAR(11)) + rank.WorkBeginTime) AS DATETIME) AS BeginTime,
                                                CAST((CAST(CONVERT(CHAR, DATEADD(DAY,1,empRank.Date), 20) AS CHAR(11)) + rank.WorkEndTime) AS DATETIME) AS EndTime
                                     From AttendanceEmpRank AS empRank 
                                             Left Join AttendanceRank AS rank on rank.AttendanceRankId = empRank.AttendanceRankId
                                        Where empRank.EmployeeId in ({0}) And empRank.Date = '{1}' And (rank.IsOverZeroId = 'TrueFalse_001' or rank.IsBelongToBefore = 'True')
                                                and empRank.StateId='PlanState_003' and empRank.ApproveResultId='OperatorResult_001' ",
                sb.ToString(), pEndDate.AddDays(-1).ToDateFormatString());
               
                dtEndDateRank=HRHelper.ExecuteDataTable (strSql);
            
            int num = 0;
            DateTime tempBegin = DateTime.MinValue;
            List<DateTime> tempDate = null;
            EmployeeService empSer = new EmployeeService();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (string str in pEmployeeIds)
                {
                    if (str.CheckNullOrEmpty()) continue;
                    //tempBegin = pBeginDate;//20120419 modified 结束用了date 开始也用date
                    tempBegin = pBeginDate.Date;
                    tempDate = new List<DateTime>();
                    while (tempBegin <= pEndDate)
                    {
                        rows = dt.Select(string.Format("EmployeeId = '{0}' AND Date = '{1}'", str, tempBegin.ToDateFormatString()));
                        if (rows.Length == 0)
                        {
                            //20110902 added by songyj for 请假的结束日期减一天的班次跨天的话，且请假最后一天的时间属于前天的班次，则最后一天没有排版也可以
                            if (tempBegin == pEndDate)
                            {
                                drEndDateRanks = dtEndDateRank.Select(string.Format("EmployeeId = '{0}' AND EndTime >= '{1}'", str, tempEnd.ToDateTimeFormatString()));
                                if (drEndDateRanks != null && drEndDateRanks.Length > 0)
                                {
                                    tempBegin = tempBegin.AddDays(1);
                                    continue;
                                }
                            }
                            tempDate.Add(tempBegin);
                        }

                        tempBegin = tempBegin.AddDays(1);
                    }
                    if (tempDate.Count > 0)
                    {
                        num++;
                        tempString = string.Empty;
                        foreach (DateTime temp in tempDate)
                        {
                            tempString += string.Format("{0},", temp.ToDateFormatString());
                        }
                        tempString = tempString.Remove(tempString.Length - 1, 1);
                       string empName=empSer.GetEmployeeNameById(str);
                        sbMsg.AppendFormat("员工:{0}在 {1} 没有班次安排", empName, tempString);
                        sbMsg.Append("\r\n");
                        if (num > 20)
                        {
                            //人员过多只显示20个人的,防止撑满屏幕。。。。。。
                            sbMsg.Append("......");
                            break;
                        }
                    }
                }

            }
            else
            {
                foreach (string str in pEmployeeIds)
                {
                    if (str.CheckNullOrEmpty()) continue;
                    string empName = empSer.GetEmployeeNameById(str);
                    sbMsg.AppendFormat("员工:{0}在 {1} 没有班次安排", empName , pBeginDate.ToDateFormatString() + " - " + pEndDate.ToDateFormatString());
                    sbMsg.Append("\r\n");
                }
            }


            return sbMsg.ToString();
        }


        public virtual DataTable GetEmpRanks(string[] pEmployeeIds, DateTime pBeginDate, DateTime pEndDate)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in pEmployeeIds)
            {
                if (str.CheckNullOrEmpty()) continue;
                sb.AppendFormat(",'{0}'", str);
            }
            if (sb.Length > 0)
                sb.Remove(0, 1);
            string sql = string.Format(@"SELECT attendanceemprank.employeeid,
       attendanceemprank.attendancerankid,
       attendanceemprank.date,
       attendancerank.isrestrank,
       attendancerank.isoverzeroid,
       attendancerank.workbegintime,
       attendancerank.workendtime,
       attendancerank.workhours
FROM   attendanceemprank
       LEFT JOIN attendancerank
              ON attendancerank.attendancerankid =
                 attendanceemprank.attendancerankid
WHERE  attendanceemprank.employeeid IN ( {2} )
       AND attendanceemprank.date BETWEEN '{0}' AND '{1}'
       AND attendanceemprank.stateid = 'PlanState_003'
       AND attendanceemprank.approveresultid = 'OperatorResult_001' ",
                                                       pBeginDate.ToDateFormatString(),
                                                       pEndDate.ToDateFormatString(),
                                                       sb.ToString());
                return HRHelper.ExecuteDataTable(sql);
            
        }

    }
}
