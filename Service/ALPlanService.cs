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
    public class ALPlanService:HRService
    {
        public  decimal GetDays(string pEmployeeId, string pFiscalYearId, DateTime pBeginDate, DateTime pEndDate, string pCorId)
        {
            #region 参数检查
            if (pEmployeeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("pEmployeeID");
            }

            if (pFiscalYearId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("pFiscalYearId");
            }
            #endregion
            string sqlS = @" select [Days],[AnnualLeaveUnit] from AnnualLeavePlanEmployee
	 left join AnnualLeavePlan on AnnualLeavePlanEmployee.AnnualLeavePlanId=AnnualLeavePlan.AnnualLeavePlanId
	 where AnnualLeavePlanEmployee.FiscalYearId='{0}'
	 and EmployeeId='{1}'
	{2}
	 and AnnualLeavePlan.CorporationId='{3}'";
            string daySql = "";
            if (pBeginDate != DateTime.MinValue && pEndDate != DateTime.MinValue)
            {
                //(bug9343)如果fiscalYearId和date参数对应的不是同一年，则已财年为准
                DataTable dt_years = new DataTable();
               
                   string strSql = string.Format(@"select year from fiscalYear where fiscalYearId='{0}'", pFiscalYearId);
                
                    dt_years=HRHelper.ExecuteDataTable(strSql);
               
                string year = dt_years.Rows[0][0].ToString();
                if (year.Equals(pBeginDate.Year.ToString()))
                {
                    daySql = string.Format(@" and AnnualLeavePlanEmployee.BeginDate<='{0}'
                         and AnnualLeavePlanEmployee.EndDate>='{1}'", pBeginDate.ToShortDateString(),pEndDate.ToShortDateString());
                }
            }
          
            // 获得所有数据
            DataTable dt = HRHelper.ExecuteDataTable(string.Format(sqlS,pFiscalYearId,pEmployeeId,daySql,pCorId));
            if (dt != null && dt.Rows.Count > 0)
            {//考虑到折算情况
                return Convert.ToDecimal(dt.Rows[dt.Rows.Count - 1][0]);
            }
            else
            {
                dt = HRHelper.ExecuteDataTable(string.Format(sqlS, pFiscalYearId, pEmployeeId, daySql, "688564CE-C44C-4E1B-A58D-A10091B6E77B"));
                if (dt != null && dt.Rows.Count > 0)
                {//考虑到折算情况
                    return Convert.ToDecimal(dt.Rows[dt.Rows.Count - 1][0]);
                }
            }

            return 0;
        }


        /// <summary>
        /// 根据公司获取年假参数
        /// </summary>
        /// <param name="pCorporationId"></param>
        /// <returns></returns>
        public DataTable GetParameterWithNoPower(string pCorporationId)
        {
            if (!pCorporationId.CheckNullOrEmpty())
            {
                string sql = string.Format(@"select * from AnnualLeaveParameter
	 where Flag=1
	 and (CorporationId='688564CE-C44C-4E1B-A58D-A10091B6E77B' or CorporationId='{0}')", pCorporationId);

                DataTable dtParameter = HRHelper.ExecuteDataTable(sql);
                return dtParameter;
            }
            return null;
        }

        // 20090225 added by zhonglei
        /// <summary>
        /// 根据员工ID和财政年度ID返回该年年假的起休区间
        /// </summary>
        /// <param name="pEmployeeId">员工ID</param>
        /// <param name="pFiscalYearId">财政年度ID</param>
        /// <returns></returns>
        public virtual DataTable GetBeginEndDate(string pEmployeeId, string pFiscalYearId, string pCorId)
        {
            #region 参数检查
            if (pEmployeeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("pEmployeeID");
            }

            if (pFiscalYearId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("pFiscalYearId");
            }
            #endregion

            StringBuilder sqlsb = new StringBuilder(string.Format(@"select info.BeginDate,info.EndDate,info.LegalDays,info.WelfareDays,info.AnnualLeaveUnit 
                                                                from AnnualLeavePlanEmployee info
                                                                left join AnnualLeavePlan main on main.AnnualLeavePlanId=info.AnnualLeavePlanId
                                                                where info.EmployeeId='{0}' and info.FiscalYearId='{1}' ", pEmployeeId, pFiscalYearId));
            if (!pCorId.CheckNullOrEmpty())
            {
                sqlsb.AppendLine(string.Format(@" and main.CorporationId='{0}'", pCorId));
            }
            DataTable dt = HRHelper.ExecuteDataTable(sqlsb.ToString());
            return dt;
        }


        /// <summary>
        /// 检查员工当前财年当前日期年假计划 added by zhoug 20150116 for bug 25855 A00-20150108001
        /// </summary>
        /// <param name="pEmployeeId">员工ID</param>
        /// <param name="pFiscalYearId">财年ID</param>
        /// <param name="pDate">归属日期</param>
        /// <returns></returns>
        public virtual string CheckAnnualLeavePlan(string pEmployeeId, string pFiscalYearId, DateTime pDate)
        {
            #region 参数检查
            if (pEmployeeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("pEmployeeId");
            }
            if (pFiscalYearId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("pFiscalYearId");
            }
            #endregion

            string msg = string.Empty;
            DataTable dt;
            EmployeeService employeeService = new EmployeeService ();

            string empName= employeeService.GetEmployeeNameById(pEmployeeId);

            
             DataTable fY= HRHelper.ExecuteDataTable(string.Format("select [year] from fiscalYear where FiscalYearId='{0}'",pFiscalYearId));
            int fYear=DateTime.Now.Year;
            if (fY != null && fY.Rows.Count > 0) {
                fYear =Convert.ToInt32( fY.Rows[0][0].ToString());

            }
          //  FiscalYear fiscalYear = docSvcYear.Read(pFiscalYearId);
            //根据员工、休假年度、休假日期是否存在年假计划
            string sql = string.Format(@"                                       
                                        select main.CorporationId from AnnualLeavePlanEmployee info 
                                        left join AnnualLeavePlan main on info.AnnualLeavePlanid=main.AnnualLeavePlanId
                                        WHERE info.EmployeeId='{0}' 
                                        AND info.FiscalYearId='{1}'
                                        AND ('{2}'>= info.BeginDate AND '{2}'<= info.EndDate)
                                        ", pEmployeeId, pFiscalYearId, pDate.ToDateFormatString());
            dt = HRHelper.ExecuteDataTable(sql);
            if (dt == null || dt.Rows.Count == 0)
            {
                //员工{0} 财年{1} 日期{2} 没有年假计划
                msg = string.Format("员工{0} 财年{1} 日期{2} 没有年假计划", empName, fYear, pDate.ToDateFormatString());
            }
            if (msg.CheckNullOrEmpty())
            {
                string corId = dt.Rows[0]["CorporationId"].ToString();
                //根据员工和休假年度查看年假计划主表是否结余
                sql = string.Format(@"                                       
                                    SELECT ALP.IsBalance
                                    FROM dbo.AnnualLeavePlanEmployee ALPE
                                    LEFT JOIN dbo.AnnualLeavePlan ALP ON ALP.AnnualLeavePlanId=ALPE.AnnualLeavePlanId
                                    WHERE ALPE.EmployeeId='{0}' 
                                    AND ALPE.FiscalYearId='{1}'
                                    AND ALP.CorporationId='{2}'", pEmployeeId, pFiscalYearId, corId);
                dt = HRHelper.ExecuteDataTable(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    bool isBalance;
                    bool.TryParse(dt.Rows[0][0].ToString(), out isBalance);
                    //结余
                    if (isBalance)
                    {
                        //员工{0} 财年{1} 年假计划已经结余不能请年假
                        msg = string.Format("员工{0} 财年{1} 年假计划已经结余不能请年假", empName, fYear);
                    }
                }
            }
            return msg;
        }


    }
}
