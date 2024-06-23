using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.HR.Services;
using BQHRWebApi.Business;
using BQHRWebApi.Common;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Reflection;
using System.Text;


using Dcms.Common;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Resources;

namespace BQHRWebApi.Service
{

    public class AttendanceLeaveService : HRService
    {


        public override void Save(DataEntity[] entities)
        {
            foreach (var entity in entities)
            {
                AttendanceLeave enty = entity as AttendanceLeave;

                SaveAttendanceLeave(enty);
            }
        }


        public void SaveAttendanceLeave(AttendanceLeave enty)
        {
            DataTable dtItem = new DataTable();
            if (string.IsNullOrEmpty(enty.EmpCode))
            {
                throw new Exception("EmpCode is null");
            }
            if (enty.BeginDate == DateTime.MinValue)
            {
                throw new Exception("BeginDate is null");
            }
            if (string.IsNullOrEmpty(enty.BeginTime))
            {
                throw new Exception("BeginTime is null");
            }
            DateTime tmpTime = DateTime.Now;
            if (!DateTime.TryParse(string.Format("{0} {1}", enty.BeginDate.ToShortDateString(), enty.BeginTime), out tmpTime))
            {
                throw new Exception("开始日期和开始时间不是正确的日期时间格式");
            }
            if (enty.EndDate == DateTime.MinValue)
            {
                throw new Exception("EndDate error");
            }
            if (string.IsNullOrEmpty(enty.EndTime))
            {
                throw new Exception("EndTime error");
            }
            if (!DateTime.TryParse(string.Format("{0} {1}", enty.EndDate.ToShortDateString(), enty.EndTime), out tmpTime))
            {
                throw new Exception("结束日期和结束时间不是正确的日期时间格式");
            }


            if (enty.TotalHours == 0m)//请假时数不能是0
            {
                throw new Exception("TotalHours is 0");
            }
            if (string.IsNullOrEmpty(enty.AttendanceTypeId.ToString()))
            {
                throw new Exception("AttendanceTypeId is null");
            }

            string sql = "";
            if (enty != null)
            {

                sql = HRHelper.GenerateSqlInsert(enty, "AttendanceLeave");
                HRHelper.ExecuteNonQuery(sql);


            }

        }


        public virtual string CheckForAPI(AttendanceLeave formEntity)
        {
            this.CheckValue(formEntity);

            string msg = string.Empty;

            AttendanceLeave[] arrayEntity = new AttendanceLeave[] { formEntity };

            #region ESS表單上的檢查
            Dictionary<int, string> dicCheck = new Dictionary<int, string>();
            //檢查請假時間與出差申請時間是否重複
            dicCheck = this.CheckBusinessApplyTime(arrayEntity);
            if (dicCheck != null && dicCheck.Count > 0)
            {
                return dicCheck.Values.First();
            }
            #endregion

            if (formEntity.AttendanceTypeId.Equals("406"))
            {
                List<AttendanceOverTimeRest> list = this.ChangeToOTRestEntity(arrayEntity);
                AttendanceOverTimeRestService restSer=new AttendanceOverTimeRestService ();
                msg = restSer.CheckForAPI(list.First());
            }
            //else if (formEntity.AttendanceTypeId.Equals("408"))
            //{
            //    List<TWALReg> list = this.ChangeToTWALRegEntity(arrayEntity);
            //    msg = Factory.GetService<ITWALRegService>().CheckForAPI(list.First());
            //}
            else
            {
                AttendanceLeave leave = ChangeToLeaveEntity(arrayEntity, true, string.Empty, string.Empty).First();
                try
                {
                    string[] tmpMsg = null;
                    string msgInfo = this.GetSalaryTimes(leave);
                    if (msgInfo.IndexOf("false") > -1)
                    {
                        tmpMsg = msgInfo.Split(',');
                        throw new BusinessRuleException(string.Format(Resources.AT_VerifySalaryHours, tmpMsg[0], tmpMsg[2], tmpMsg[1]));
                    }
                    this.CheckForESS(leave);
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }
            }
            return msg;
        }


        /// <summary>
        /// 檢查參數值
        /// </summary>
        /// <param name="pFormEntity">實體</param>
        private void CheckValue(AttendanceLeave pFormEntity)
        {
            ExceptionCollection ec = new ExceptionCollection();
            if (pFormEntity.EmployeeId.CheckNullOrEmpty())
            {
                ec.Add(new ArgumentNullException("AttendanceLeave.EmployeeId"));
            }
            if (pFormEntity.AttendanceTypeId.CheckNullOrEmpty())
            {
                ec.Add(new ArgumentNullException("AttendanceLeave.AttendanceTypeId"));
            }
            if (pFormEntity.BeginDate.CheckNullOrEmpty())
            {
                ec.Add(new ArgumentNullException("AttendanceLeave.BeginDate"));
            }
            if (pFormEntity.BeginTime.CheckNullOrEmpty())
            {
                ec.Add(new ArgumentNullException("AttendanceLeave.BeginTime"));
            }
            if (pFormEntity.EndDate.CheckNullOrEmpty())
            {
                ec.Add(new ArgumentNullException("AttendanceLeave.EndDate"));
            }
            if (pFormEntity.EndTime.CheckNullOrEmpty())
            {
                ec.Add(new ArgumentNullException("AttendanceLeave.EndTime"));
            }

            string errorMsg = string.Empty;
            if (ec.Count > 0)
            {
                foreach (Exception ex in ec)
                {
                    errorMsg += ex.Message.Replace("\r\n", "").Replace("\n", "") + "\r\n";
                }
                throw new BusinessRuleException(errorMsg);
            }
        }


        /// <summary>
        /// 檢查請假時間與出差申請時間是否重複
        /// </summary>
        /// <param name="formEntities">實體陣列</param>
        /// <returns>無錯誤返回null，有錯誤則返回字典類型(陣列第幾筆, 錯誤訊息)</returns>
        /// <remarks>提示訊息：請假時間與出差申請時間重疊，是否繼續提交表單？</remarks>
        public virtual Dictionary<int, string> CheckBusinessApplyTime(AttendanceLeave[] formEntities)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            CheckEntityType entityType = new CheckEntityType();
            bool isUltimate =true;

            foreach (AttendanceLeave apiEntity in formEntities)
            {
                if (apiEntity.AttendanceTypeId.Equals("401") && !isUltimate)
                {
                    entityType = CheckEntityType.AnnualLeave;
                }
                else if (apiEntity.AttendanceTypeId == "406")
                {
                    entityType = CheckEntityType.OTRest;
                }
                else if (apiEntity.AttendanceTypeId == "408")
                {
                    entityType = CheckEntityType.TWALReg;
                }
                else
                {
                    entityType = CheckEntityType.Leave;
                }
                string msg = this.CheckBusinessApplyTime(apiEntity.EmployeeId.ToString()(), apiEntity.BeginDate, apiEntity.BeginTime, apiEntity.EndDate, apiEntity.EndTime, entityType, string.Empty, true);
                if (!msg.CheckNullOrEmpty())
                {
                    dic.Add(Array.IndexOf(formEntities, apiEntity), "提示訊息：請假時間與出差申請時間重疊，是否繼續提交表單？");
                }
            }
            if (dic.Count > 0)
            {
                return dic;
            }
            return null;
        }



        /// <summary>
        /// 獲取請假時數
        /// </summary>
        /// <param name="formEntity">實體</param>
        /// <returns>返回Hours:時數,Unit:單位</returns>
        public virtual DataTable GetLeaveHoursForAPI(AttendanceLeave formEntity)
        {
            this.CheckValue(formEntity);

            DataTable dtHour = new DataTable();
            AttendanceOverTimeRestService restService = new AttendanceOverTimeRestService();    
            if (formEntity.AttendanceTypeId.Equals("406"))
            {
                //調休假
                List<AttendanceOverTimeRest> list = this.ChangeToOTRestEntity(new AttendanceLeave[] { formEntity });
                dtHour = restService.GetRestHoursForAPI(list.First());
            }
            else
            {
                dtHour = this.GetLeaveHoursForGP(string.Empty, formEntity.EmployeeId.ToString(), formEntity.BeginDate, formEntity.BeginTime, formEntity.EndDate, formEntity.EndTime, formEntity.AttendanceTypeId);
            }
            foreach (DataRow dr in dtHour.Rows)
            {
                dr["Hours"] = decimal.Parse(dr["Hours"].ToString()).ToString("#0.0000");
            }
            return dtHour;
        }

        /// <summary>
        /// API實體轉為調休假實體(AttendanceLeaveForAPI -> AttendanceOverTimeRest)
        /// </summary>
        /// <param name="pFormEntities">AttendanceLeaveForAPI實體</param>
        /// <param name="pIsSave">是否為存檔</param>
        /// <param name="pFormType">單別</param>
        /// <param name="pFormNumber">單號</param>
        /// <returns>AttendanceOverTimeRest實體</returns>
        private List<AttendanceOverTimeRest> ChangeToOTRestEntity(AttendanceLeave[] pFormEntities)
        {
            List<AttendanceOverTimeRest> list = new List<AttendanceOverTimeRest>();
            AttendanceOverTimeRest entity = null;
            foreach (AttendanceLeave apiEntity in pFormEntities)
            {
                entity = new AttendanceOverTimeRest();
                if (entity.AttendanceOverTimeRestId.CheckNullOrEmpty())
                {
                    entity.AttendanceOverTimeRestId = SequentialGuid.NewGuid();
                }
                entity.EmployeeId = apiEntity.EmployeeId;
                entity.BeginDate = apiEntity.BeginDate;
                entity.BeginTime = apiEntity.BeginTime;
                entity.EndDate = apiEntity.EndDate;
                entity.EndTime = apiEntity.EndTime;
                entity.Remark = apiEntity.Remark;
                entity.Hours = apiEntity.TotalHours;
                //entity.DeputyEmployeeId
                list.Add(entity);
            }
            return list;
        }

       
        public virtual DataTable GetLeaveInfoForGP(string pEmployeeId, DateTime pDate)
        {
            //返回DataTable包含列:TypeName(假勤类型),BeginDate(有效开始日期),
            //EndDate(有效结束日期),Hours(总时数),RemainHours(剩余时数)
            #region 校验参数
            if (pEmployeeId.CheckNullOrEmpty())
                throw new ArgumentNullException("pEmployeeId");
            if (pDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("pDate");
            }
            #endregion
            string unit = string.Empty;  //单位

            DataTable dt = new DataTable();
            dt.Columns.Add("TypeName");
            //dt.Columns.Add("BeginDate");
            dt.Columns.Add("BeginDate", typeof(System.DateTime));
            //dt.Columns.Add("EndDate");
            dt.Columns.Add("EndDate", typeof(System.DateTime));
            dt.Columns.Add("Hours");
            dt.Columns.Add("ActualHours");
            dt.Columns.Add("RemainHours");
            dt.Columns.Add("BalanceVoidDays");
            dt.Columns.Add("Unit");
            dt.Columns.Add("ThisYearAmount");
            dt.Columns.Add("BalanceNextYear");
            //20141202 added for 24508 && Q00-20141201003 by renping
            dt.Columns.Add("UnitId");
            //20150609 added for 29326 && S00-20150522002 by renping
            dt.Columns.Add("BalanceLastYear");
            dt.Columns.Add("BalanceEndDate");
            dt.Columns.Add("BalanceActual");
            //20170302 added by yingchun for S00-20170207010_39232+39233+39234
            dt.Columns.Add("IsBalanceSettlement");  //結轉結算碼 Length:15
            dt.Columns.Add("IsTWALSettlement");     //特休結算碼 Length:16

            DataRow newRow;
            DateTime begin = DateTime.MinValue;
            DateTime end = DateTime.MinValue;

            DateTime yearBegin = new DateTime(pDate.Year, 1, 1);
            DateTime yearEnd = (new DateTime(pDate.Year + 1, 1, 1)).AddDays(-1);

            #region 年假 
            //获取这个人所在的公司 eidt by shenzy for
            EmployeeService empService = new EmployeeService();
       
            string corporationId = empService.GetEmpFiledById(pEmployeeId,"CorporationId");
            //通过日期取属于哪个财年,一般都是有值的

            DataTable dtFiscalYears = new DataTable();
            //using (IConnectionService conSer = Factory.GetService<IConnectionService>())
            //{
            //    IDbCommand cmd = conSer.CreateDbCommand();
                string strSql = string.Empty;
                strSql = string.Format(@"SELECT   Planemp.FiscalYearId
                                            FROM     AnnualLeavePlanemPloyee AS Planemp
                                                     LEFT JOIN FiscalYear
                                                       ON Planemp.FiscalYearId = FiscalYear.FiscalYearId
                                            WHERE    Planemp.EmployeeId = '{0}'
                                                     AND Planemp.BeginDate <= '{1}'
                                                     AND Planemp.EndDate >= '{2}'
                                                     --AND Planemp.Flag = '1'
                                                     AND Planemp.CorporationId = '{3}'
                                            ORDER BY Planemp.EndDate ", pEmployeeId, yearEnd.ToDateFormatString(), yearBegin.ToDateFormatString(), corporationId);
            //  cmd.CommandText = strSql;
            dtFiscalYears = HRHelper.ExecuteDataTable(strSql) ;
            //}
            ALPlanService aLPlanService = new ALPlanService();

            for (int i = 0; i < dtFiscalYears.Rows.Count; i++)
            {
                string fiscalYearId = dtFiscalYears.Rows[i]["FiscalYearId"].ToString();

                //一共的天数(结余＋本年可休的天数)
                decimal totalDays;
                if (!fiscalYearId.CheckNullOrEmpty())
                {
                    //根据公司获得参数

                    DataTable dtParam = aLPlanService.GetParameterWithNoPower(corporationId);

                    if (dtParam == null && dtParam.Rows.Count == 0)
                    {
                        throw new BusinessRuleException("员工所在公司没有年假参数设置");
                    }
                    if (dtParam.Rows.Count == 1) { 
                       unit = dtParam.Rows[0]["AnnualLeaveUnitId"].ToString();//年假计算最小单位
                    }
                    else
                    {
                        foreach(DataRow r in dtParam.Rows)
                        {
                            if (r["CorporationId"].ToString() == corporationId) {
                                unit = r["AnnualLeaveUnitId"].ToString();//年假计算最小单位
                            }
                        }

                    }
                  
                    totalDays = aLPlanService.GetDays(pEmployeeId, fiscalYearId, pDate, pDate,corporationId);
                  

                   
                    decimal leftDays = GetLeftDays(fiscalYearId, pEmployeeId,"");

                    if (leftDays < 0)
                        leftDays = 0;
                    decimal leavingDays = totalDays - leftDays;//剩余天数

                    #region 取结余作废时数
                    decimal balanceVoidDays = 0;  //结余作废
                    DataTable dtBalance = new DataTable();
                    decimal balanceDays = 0;
                    decimal planDays = 0;
                    decimal actualDays = 0;
                    decimal remaiderDays = 0;
                  
                      
                        string tempSql = string.Format(@"SELECT BalanceDays,
                                                               PlanDays,
                                                               ActualDays,
                                                               RemainderDays,
                                                               BalanceVoidDays
                                                        FROM   AnnualLeaveBalance
                                                        WHERE  FiscalYearId = '{0}' AND EmployeeId = '{1}'", fiscalYearId, pEmployeeId);
                        if (!corporationId.CheckNullOrEmpty())
                        {
                            tempSql += string.Format(@" and CorporationId='{0}'", corporationId);
                        }
                      
                        dtBalance=HRHelper.ExecuteDataTable(tempSql);
                  
                    if (dtBalance != null && dtBalance.Rows.Count > 0)
                    {
                        balanceDays = Convert.ToDecimal(dtBalance.Rows[0]["BalanceDays"].ToString());
                        planDays = Convert.ToDecimal(dtBalance.Rows[0]["PlanDays"].ToString());
                        actualDays = Convert.ToDecimal(dtBalance.Rows[0]["ActualDays"].ToString());
                        remaiderDays = Convert.ToDecimal(dtBalance.Rows[0]["RemainderDays"].ToString());
                        balanceVoidDays = Convert.ToDecimal(dtBalance.Rows[0]["BalanceVoidDays"].ToString());
                    }
                    #endregion

                    DataTable dtBeginEndInfo =aLPlanService. GetBeginEndDate(pEmployeeId, fiscalYearId, corporationId);
                    if (dtBeginEndInfo != null && dtBeginEndInfo.Rows.Count > 0)
                    {
                        DateTime.TryParse(dtBeginEndInfo.Rows[0]["BeginDate"].ToString(), out begin);
                        DateTime.TryParse(dtBeginEndInfo.Rows[0]["EndDate"].ToString(), out end);
                    }

                    newRow = dt.NewRow();
                    newRow["TypeName"] = GetAttendanceTypeNameById("401"); ;
                    //newRow["BeginDate"] = begin.ToDateFormatString();
                    //newRow["EndDate"] = end.ToDateFormatString();
                    newRow["BeginDate"] = begin.Date;
                    newRow["EndDate"] = end.Date;
                    if (dtBalance != null && dtBalance.Rows.Count > 0)
                    {
                        newRow["Hours"] = planDays.ToString();
                        newRow["ActualHours"] = actualDays.ToString();
                        newRow["RemainHours"] = remaiderDays.ToString();
                        newRow["BalanceVoidDays"] = balanceVoidDays.ToString();
                    }
                    else
                    {
                        newRow["Hours"] = totalDays.ToString();
                        newRow["ActualHours"] = leftDays.ToString();
                        newRow["RemainHours"] = leavingDays.ToString();
                        newRow["BalanceVoidDays"] = balanceVoidDays.ToString();
                    }

                    if (unit.Equals("AnnualLeaveUnit_001"))
                        unit = "AnnualLeaveUnit_002";

                    DataTable dtUnit = HRHelper.ExecuteDataTable(string.Format("select * from codeinfo where codeinfoId='{0}'",unit));// codeInfoSer.GetCodeInfoNameById(unit);
                    if (dtUnit != null && dtUnit.Rows.Count > 0)
                    {
                        newRow["Unit"] = dtUnit.Rows[0]["ScName"].ToString();
                    }
                    else
                    {
                        newRow["Unit"] = "";
                    }
                    newRow["ThisYearAmount"] = "";
                    newRow["BalanceNextYear"] = "";
                    //20141202 added for 24508 && Q00-20141201003 by renping
                    newRow["UnitId"] = unit;
                    dt.Rows.Add(newRow);
                }
            }
            #endregion

            #region 特殊假
            DataTable dtAtSpecialHolidaySet = new DataTable();
          
                #region 20100919 增加校验请假最后一天是否是跨天班
                DataTable dtRank = new DataTable();
                //判段请假的结束时间是不是跨天班，如果是的就把结束日期减一天
                strSql = string.Format(@"SELECT IsOverZeroId
                                        FROM   AttendanceemPrank AS emPrank
                                               LEFT JOIN AttendanceRank AS Rank
                                                 ON emPrank.AttendanceRankId = Rank.AttendanceRankId
                                        WHERE  emPrank.EmployeeId = '{0}'
                                               AND emPrank.DATE = '{1}'", pEmployeeId, pDate.AddDays(-1).ToDateFormatString());
             
            dtRank = HRHelper.ExecuteDataTable(strSql);

                DateTime endDate = pDate;
                if (dtRank != null && dtRank.Rows.Count > 0 && dtRank.Rows[0][0].ToString().Equals("TrueFalse_001"))
                {
                    endDate = pDate.AddDays(-1);
                }
                #endregion

                strSql = string.Format(@"SELECT   AtSpecialHolidaySet.AttendanceTypeId,
                                                 AttendanceType.[Name]                AS TypeName,
                                                 AtSpecialHolidaySet.BeginDate,
                                                 AtSpecialHolidaySet.EndDate,
                                                 AtSpecialHolidaySet.Amount,
                                                 AtSpecialHolidaySet.reMaiderDays
                                        FROM     AtSpecialHolidaySet
                                                 LEFT JOIN AttendanceType
                                                   ON AtSpecialHolidaySet.AttendanceTypeId = AttendanceType.AttendanceTypeId
                                        WHERE    EmployeeId = '{0}'
                                                 AND AtSpecialHolidaySet.BeginDate <= '{1}'
                                                 AND AtSpecialHolidaySet.EndDate >= '{2}'
                                        ORDER BY AtSpecialHolidaySet.EndDate", pEmployeeId, yearEnd.ToDateFormatString(), yearBegin.ToDateFormatString());
              
                dtAtSpecialHolidaySet=HRHelper.ExecuteDataTable(strSql);
                if (dtAtSpecialHolidaySet != null && dtAtSpecialHolidaySet.Rows.Count > 0)
                {
                    DateTime specBegin = DateTime.MinValue;
                    DateTime specEnd = DateTime.MinValue;
                    DataTable dtLeave = new DataTable();
                    //decimal tempHours = 0;
                    DataTable dtLeaveUnit = null;
                    foreach (DataRow specRow in dtAtSpecialHolidaySet.Rows)
                    {
                        DateTime.TryParse(specRow["BeginDate"].ToString(), out specBegin);
                        DateTime.TryParse(specRow["EndDate"].ToString(), out specEnd);

                        newRow = dt.NewRow();
                        newRow["TypeName"] = specRow["TypeName"].ToString();

                        newRow["BeginDate"] = specBegin.Date;
                        newRow["EndDate"] = specEnd.Date;
                        newRow["Hours"] = specRow["Amount"].ToString();
                        //newRow["RemainHours"] = leavingDays.ToString();
                        newRow["ActualHours"] = (Convert.ToDecimal(specRow["Amount"].ToString()) - Convert.ToDecimal(specRow["RemaiderDays"].ToString())).ToString();
                        newRow["RemainHours"] = specRow["RemaiderDays"].ToString();
                        newRow["BalanceVoidDays"] = "";
                        unit = "AnnualLeaveUnit_003";
                        //下sql取特殊假的单位
                        dtLeaveUnit = new DataTable();
                        strSql = string.Format(@"select AttendanceUnitId from AttendanceType
                                                 Where AttendanceTypeId='{0}'", specRow["AttendanceTypeId"].ToString());
                     
                        dtLeaveUnit=HRHelper.ExecuteDataTable(strSql);
                        if (dtLeaveUnit != null && dtLeaveUnit.Rows.Count > 0)
                        {
                            unit = dtLeaveUnit.Rows[0][0].ToString();
                        }
                        DataTable dtUnit = HRHelper.ExecuteDataTable(string.Format("select * from codeinfo where codeinfoId='{0}'", unit)); 
                        if (dtUnit != null && dtUnit.Rows.Count > 0)
                        {
                            newRow["Unit"] = dtUnit.Rows[0]["ScName"].ToString();
                        }
                        else
                        {
                            newRow["Unit"] = "";
                        }
                        newRow["ThisYearAmount"] = "";
                        newRow["BalanceNextYear"] = "";
                        //20141202 added for 24508 && Q00-20141201003 by renping
                        newRow["UnitId"] = unit;
                        dt.Rows.Add(newRow);
                    }
                
            }
            #endregion


            //#region 20101106 added by jiangpeng for 台湾特休假

            //DataTable dtPlan = new DataTable();
           
            //    //此sql如果需要加字段请在后面加，以免打乱顺序，谢谢
            //    //20170302 added by yingchun for S00-20170207010_39232+39233+39234 : 增加結轉結算碼及(IsBalanceSettlement)特休結算碼(IsTWALSettlement)
            //    //20150609 added for 29326 && S00-20150522002 by renping
            //    strSql = string.Format(@"SELECT BalanceLastYear,
            //                                    BalanceEndDate,
            //                                    BalanceActual,
            //                                    BalanceRemainder,
            //                                    BalanceVoid,
            //                                    ThisYearAmount,
            //                                    ActualAmount,
            //                                    RemainderAmount,
            //                                    BeginDate,
            //                                    EndDate,
            //                                    ThisYearAmount,
            //                                    BalanceNextYear,
            //                                    BalanceLastYear,
            //                                    BalanceEndDate,
            //                                    BalanceActual,
            //                                    IsBalanceSettlement, IsTWALSettlement
            //                            FROM   twAlplAnInfo
            //                            WHERE  EmployeeId = '{0}'
            //                                   AND BeginDate <= '{1}'
            //                                   AND EndDate >= '{2}'
            //                            ORDER BY twAlplAnInfo.EndDate
            //                            ", pEmployeeId, yearEnd.ToDateFormatString(), yearBegin.ToDateFormatString());

            //dtPlan = HRHelper.ExecuteDataTable(strSql);
            //if (dtPlan != null && dtPlan.Rows.Count > 0)
            //{
            //    for (int i = 0; i < dtPlan.Rows.Count; i++)
            //    {
            //        //20110322 added by wuyxb for 取计划开始结束日期

            //        DateTime.TryParse(dtPlan.Rows[i]["BeginDate"].ToString(), out begin);
            //        DateTime.TryParse(dtPlan.Rows[i]["EndDate"].ToString(), out end);

            //        TWALPara para = Factory.GetService<ITWALParaEmpService>().GetParaByEmployeeId(pEmployeeId);

            //        newRow = dt.NewRow();
            //        newRow["TypeName"] = GetAttendanceTypeNameById("408"); ;
            //        newRow["BeginDate"] = begin.Date;
            //        newRow["EndDate"] = end.Date;
            //        newRow["Hours"] = dtPlan.Rows[i][5].ToString();
            //        newRow["ActualHours"] = dtPlan.Rows[i][6].ToString();
            //        newRow["RemainHours"] = dtPlan.Rows[i][7].ToString();
            //        newRow["BalanceVoidDays"] = dtPlan.Rows[i][4].ToString();
            //        if (para != null)
            //            unit = para.TWALUnitId;
            //        DataTable dtUnit = HRHelper.ExecuteDataTable(string.Format("select * from codeinfo where codeinfoId='{0}'", unit));
            //        if (dtUnit != null && dtUnit.Rows.Count > 0)
            //        {
            //            newRow["Unit"] = dtUnit.Rows[0]["ScName"].ToString();
            //        }
            //        else
            //        {
            //            newRow["Unit"] = "";
            //        }
            //        newRow["ThisYearAmount"] = dtPlan.Rows[i]["ThisYearAmount"].ToString();
            //        newRow["BalanceNextYear"] = dtPlan.Rows[i]["BalanceNextYear"].ToString();
            //        //20141202 added for 24508 && Q00-20141201003 by renping
            //        newRow["UnitId"] = unit;
            //        //20150609 added for 29326 && S00-20150522002 by renping
            //        newRow["BalanceLastYear"] = dtPlan.Rows[i]["BalanceLastYear"].ToString();
            //        newRow["BalanceEndDate"] = dtPlan.Rows[i]["BalanceEndDate"].ToString();
            //        newRow["BalanceActual"] = dtPlan.Rows[i]["BalanceActual"].ToString();
            //        //20170302 added by yingchun for S00-20170207010_39232+39233+39234
            //        newRow["IsBalanceSettlement"] = dtPlan.Rows[i]["IsBalanceSettlement"].ToString();
            //        newRow["IsTWALSettlement"] = dtPlan.Rows[i]["IsTWALSettlement"].ToString();
            //        dt.Rows.Add(newRow);
            //    }
            //}

            //#endregion
            return dt;

        }


        /// <summary>
        /// 獲取當年度的請假紀錄
        /// </summary>
        /// <param name="employeeIds">員工ID</param>
        /// <param name="date">日期</param>
        /// <returns>
        /// 返回字典類型(陣列第幾筆, {請假紀錄資料})
        /// 請假紀錄資料：
        /// TypeName(假勤类型),BeginDate(开始日期),BeginTime(开始时间),EndDate(结束日期),EndTime(结束时间),StateId(审核状态ID),remark(备注)
        /// </returns>
        public virtual Dictionary<int, DataTable> GetLeaveRecordsForAPI(string[] empCodes, DateTime date)
        {
            Dictionary<int, DataTable> dic = new Dictionary<int, DataTable>();
            foreach (string emp in empCodes)
            {
                var empId = HRHelper.ExecuteScalar(string.Format("select employeeid from employee where code='{0}'", emp));

                DataTable dt = this.GetLeaveRecordsForAPI(empId.ToString(), date);
                if (dt != null && dt.Rows.Count > 0)
                {
                    dic.Add(Array.IndexOf(empCodes, empId), dt);
                }
            }
            if (dic.Count > 0)
            {
                return dic;
            }
            return null;
        }

        /// <summary>
        /// 獲取員工當年度審核同意獲待審核的請假紀錄(增加調休假紀錄)
        /// </summary>
        /// <param name="pEmployeeId"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public virtual DataTable GetLeaveRecordsForAPI(string pEmployeeId, DateTime pDate)
        {
            #region 校验参数
            if (pEmployeeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("员工Id异常");
            }
            if (pDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("日期异常");
            }
            #endregion

            DateTime begin = DateTime.MinValue;
            DateTime end = DateTime.MinValue;
           //通过日期取属于哪个财年,一般都是有值的
            DataTable dtFiscalYear = HRHelper.ExecuteDataTable(string.Format(@"select FiscalYearId,BeginEndDate_BeginDate as beginDate,BeginEndDate_EndDate as EndDate from FiscalYear
	 where BeginEndDate_BeginDate<='{0}'
	 and BeginEndDate_EndDate>='{0}'", pDate.ToShortDateString()));


            if (dtFiscalYear != null && dtFiscalYear.Rows.Count > 0)
            {
                begin = Convert.ToDateTime(dtFiscalYear.Rows[0]["BeginDate"].ToString());
                end = Convert.ToDateTime(dtFiscalYear.Rows[0]["EndDate"].ToString());
            }
            else
            {
                begin = new DateTime(pDate.Year, 1, 1);
                end = new DateTime(pDate.Year + 1, 1, 1).AddDays(-1);
            }

            #region 获取特修参数的计算方式
            DateTime begin_para = begin;
            DateTime end_para = end;
           
            string strSql = string.Format(@"select calculateMode from TWALPara 
                        where twalParaId=(select top 1 twalParaId from TWALParaEmp 
                        where employeeid='{0}')
                        ", pEmployeeId);
            DataTable dt_calMode = HRHelper.ExecuteDataTable(strSql);
        
            if (dt_calMode.Rows.Count > 0) { 
                if (dt_calMode.Rows[0][0].ToString().Equals("2"))
                {
                         strSql = string.Format(@"select top 1 beginDate,endDate from TWALPlanInfo 
                                where beginDate<='{1}' AND endDate>='{1}'
                                and employeeId='{0}'", pEmployeeId, pDate.Date.ToString("yyyy-MM-dd"));
                    DataTable dt_date =HRHelper.ExecuteDataTable(strSql);
                    if (dt_date.Rows.Count > 0)
                    {
                        begin_para = DateTime.Parse(dt_date.Rows[0][0].ToString());
                        end_para = DateTime.Parse(dt_date.Rows[0][1].ToString());
                    }
                }
            }
            #endregion

                strSql = string.Format(@"SELECT   TEMP.*, CodeInfo.ScName AS StateName
FROM     (
        SELECT tp.[Name] AS TypeName,
                Info.BeginDate,
                Info.BeginTime,
                Info.EndDate,
                Info.EndTime,
                AttendanceOTRest.StateId,
                AttendanceOTRest.remark
         FROM   AttendanceOTRest
                LEFT JOIN AttendanceOTRestDaily AS Info
                    ON AttendanceOTRest.AttendanceOverTimeRestId = Info.AttendanceOverTimeRestId
                LEFT JOIN AttendanceType AS tp
                    ON Info.AttendanceTypeId = tp.AttendanceTypeId
         WHERE  ((AttendanceOTRest.StateId = 'PlanState_003'
                    AND AttendanceOTRest.ApproveResultId = 'OperatorResult_001'
                    AND Info.IsRevoke = '0')
                    OR AttendanceOTRest.StateId = 'PlanState_002')
                AND AttendanceOTRest.EmployeeId = '{0}'
                AND AttendanceOTRest.BeginDate >= '{1}'
                AND AttendanceOTRest.BeginDate <= '{2}'
          UNION ALL

         SELECT tp.[Name]               AS TypeName,
                 Info.BeginDate,
                 Info.BeginTime,
                 Info.EndDate,
                 Info.EndTime,
                 AttendanceLeave.StateId,
                 AttendanceLeave.remark
          FROM   AttendanceLeave
                 LEFT JOIN AttendanceLeaveInfo AS Info
                   ON AttendanceLeave.AttendanceLeaveId = Info.AttendanceLeaveId
                 LEFT JOIN AttendanceType AS tp
                   ON AttendanceLeave.AttendanceTypeId = tp.AttendanceTypeId
          WHERE  ((AttendanceLeave.StateId = 'PlanState_003'
                   AND AttendanceLeave.ApproveResultId = 'OperatorResult_001'
                   AND Info.IsRevoke = '0')
                   OR AttendanceLeave.StateId = 'PlanState_002')
                 AND AttendanceLeave.EmployeeId = '{0}'
                 AND AttendanceLeave.BeginDate >= '{1}'
                 AND AttendanceLeave.BeginDate <= '{2}'
          UNION ALL
          SELECT tp.[Name]                   AS TypeName,
                 Info.BeginDate,
                 Info.BeginTime,
                 Info.EndDate,
                 Info.EndTime,
                 AnnualLeaveRegister.StateId,
                 Info.Remark
          FROM   AnnualLeaveRegister
                 LEFT JOIN AnnualLeaveRegisterInfo AS Info
                   ON AnnualLeaveRegister.AnnualLeaveRegisterId = Info.AnnualLeaveRegisterId
                 LEFT JOIN AttendanceType AS tp
                   ON AnnualLeaveRegister.AttendanceTypeId = tp.AttendanceTypeId
          WHERE  ((AnnualLeaveRegister.StateId = 'PlanState_003'
                   AND AnnualLeaveRegister.ApproveResultId = 'OperatorResult_001'
                   AND Info.IsRevoke = '0')
                   OR AnnualLeaveRegister.StateId = 'PlanState_002')
                 AND AnnualLeaveRegister.EmployeeId = '{0}'
                 AND AnnualLeaveRegister.BeginDate >= '{1}'
                 AND AnnualLeaveRegister.BeginDate <= '{2}'
          UNION ALL
          SELECT tp.[Name]       AS TypeName,
                 Info.BeginDate,
                 Info.BeginTime,
                 Info.EndDate,
                 Info.EndTime,
                 twalreg.StateId,
                 Info.Remark
          FROM   twalreg
                 LEFT JOIN twalreGinfo AS Info
                   ON twalreg.twalreGid = Info.twalreGid
                 LEFT JOIN AttendanceType AS tp
                   ON twalreg.AttendanceTypeId = tp.AttendanceTypeId
          WHERE  ((twalreg.StateId = 'PlanState_003'
                   AND twalreg.ApproveResultId = 'OperatorResult_001'
                   AND Info.IsRevoke = '0')
                   OR twalreg.StateId = 'PlanState_002')
                 AND twalreg.EmployeeId = '{0}'
                 AND twalreg.BeginDate >= '{3}'
                 AND twalreg.BeginDate <= '{4}') AS TEMP
LEFT JOIN CodeInfo ON TEMP.StateId = CodeInfo.CodeInfoId
ORDER BY BeginDate DESC"
                    , pEmployeeId, begin.ToDateFormatString(), end.ToDateFormatString(), begin_para.ToDateFormatString(), end_para.ToDateFormatString());

            DataTable dt=HRHelper.ExecuteDataTable(strSql);
            return dt;
        }


        /// <summary>
        /// 校验与请假、年假、出差登记、调休、特休时间是否重复(不包含出差申请)
        /// (请假、年假、调休、特休专用,HR与ESS共用)
        /// </summary>
        /// <param name="pEmployeeId">员工ID</param>
        /// <param name="pBeginDate">开始日期</param>
        /// <param name="pBeginTime">开始时间</param>
        /// <param name="pEndDate">结束日期</param>
        /// <param name="pEndTime">结束时间</param>
        /// <param name="pEntityType">请假类型（年假、请假、调休、特休）</param>
        /// <param name="pGuid">不同请假类型的ID</param>
        /// <param name="pIsRuleLeave">是否是请假中的请规律假</param>
        /// <returns>重复信息</returns>
        public virtual string CheckAllTime(string pEmployeeId, DateTime pBeginDate, string pBeginTime, DateTime pEndDate, string pEndTime, CheckEntityType pEntityType, string pGuid, bool pIsRuleLeave)
        {
            DataTable dt = new DataTable();
            string errorMsg = string.Empty;
            DateTime tempBegin = DateTime.MinValue;
            DateTime tempEnd = DateTime.MinValue;
            DateTime begin = DateTime.MinValue;
            DateTime end = DateTime.MinValue;
            string typeId = string.Empty;
            string typeName = string.Empty;
            DataTable dtApply = new DataTable();
            DataTable dtBusiness = new DataTable();
        
                StringBuilder sb = new StringBuilder();
                #region 查询SQL
                //年假：排除审核未同意（401：年假）
                sb.AppendFormat(@"SELECT EmployeeId, BeginDate, BeginTime, EndDate, EndTime, ISNULL(AttendanceTypeId,'401') AS AttendanceTypeId 
                                  FROM AnnualLeaveRegisterInfo
                                  WHERE IsRevoke = '0' AND AnnualLeaveRegisterId NOT IN (
	                                  SELECT AnnualLeaveRegisterId 
	                                  FROM AnnualLeaveRegister 
	                                  WHERE StateId = 'PlanState_003' AND ApproveResultId = 'OperatorResult_002')
                                  AND EmployeeId = '{0}' AND BeginDate >= '{1}' AND BeginDate <= '{2}' ", pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());
                if (pEntityType == CheckEntityType.AnnualLeave && !pGuid.CheckNullOrEmpty())
                {
                    sb.AppendFormat(" AND AnnualLeaveRegisterId <> '{0}' ", pGuid);
                }
                sb.AppendLine("UNION ALL ");
                //请假：排除审核未同意
                sb.AppendFormat(@"SELECT EmployeeId, BeginDate, BeginTime, EndDate, EndTime, AttendanceTypeId 
                                  FROM AttendanceLeaveInfo
                                  WHERE IsRevoke = '0' AND AttendanceLeaveId NOT IN (
                                      SELECT AttendanceLeaveId 
                                      FROM AttendanceLeave 
                                      WHERE StateId = 'PlanState_003' AND ApproveResultId = 'OperatorResult_002')
                                  AND EmployeeId = '{0}' AND BeginDate >= '{1}' AND BeginDate <= '{2}' ", pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());
                if (pEntityType == CheckEntityType.Leave && !pGuid.CheckNullOrEmpty())
                {
                    sb.AppendFormat(" AND AttendanceLeaveId <> '{0}' ", pGuid);
                }
                sb.AppendLine("UNION ALL ");
                sb.AppendFormat(@"SELECT info.EmployeeId, info.BeginDate, info.BeginTime, info.EndDate, info.EndTime, ISNULL(info.AttendanceTypeId,'406') AS AttendanceTypeId
                                  FROM AttendanceOTRestDaily AS info
                                  LEFT OUTER JOIN AttendanceOTRest AS rest ON rest.AttendanceOverTimeRestId = info.AttendanceOverTimeRestId
                                  WHERE info.AttendanceOverTimeRestId NOT IN (
	                                  SELECT AttendanceOverTimeRestId 
	                                  FROM AttendanceOTRest 
	                                  WHERE StateId IN ('PlanState_003','PlanState_004') AND ApproveResultId = 'OperatorResult_002')
                                  AND info.EmployeeId = '{0}' AND info.BeginDate >= '{1}' AND info.BeginDate <= '{2}' AND info.IsRevoke = '0' ", pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());
                if (pEntityType == CheckEntityType.OTRest && !pGuid.CheckNullOrEmpty())
                {
                    sb.AppendFormat(" AND info.AttendanceOverTimeRestId <> '{0}' ", pGuid);
                }
                sb.AppendLine("UNION ALL ");
                //特休假：排除审核未同意（408：特休假）
                sb.AppendFormat(@"SELECT EmployeeId, BeginDate, BeginTime, EndDate, EndTime, ISNULL(AttendanceTypeId,'408') AS AttendanceTypeId 
                                  FROM TWALRegInfo
                                  WHERE IsRevoke='0' AND TWALRegId NOT IN (
                                      SELECT TWALRegId 
                                      FROM TWALReg 
                                      WHERE StateId = 'PlanState_003' AND ApproveResultId = 'OperatorResult_002')
                                  AND EmployeeId = '{0}' AND BeginDate >= '{1}' AND BeginDate <= '{2}' ", pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());
                if (pEntityType == CheckEntityType.TWALReg && !pGuid.CheckNullOrEmpty())
                {
                    sb.AppendFormat(" AND TWALRegId <> '{0}' ", pGuid);
                }
                sb.AppendLine("UNION ALL ");
                //出差登记
                sb.AppendFormat(@"SELECT info.EmployeeId, info.BeginDate, info.BeginTime, info.EndDate, info.EndTime, ISNULL(reg.AttendanceTypeId,'701') AS AttendanceTypeId 
                                  FROM BusinessRegisterInfo AS info
                                  LEFT OUTER JOIN BusinessRegister AS reg ON reg.BusinessRegisterId = info.BusinessRegisterId
                                  WHERE info.IsRevoke = '0' AND reg.BusinessRegisterId NOT IN (
	                                  SELECT BusinessRegisterId 
	                                  FROM BusinessRegister 
	                                  WHERE StateId = 'PlanState_003' AND ApproveResultId = 'OperatorResult_002')
                                  AND reg.EmployeeId = '{0}' AND info.BeginDate >= '{1}' AND info.BeginDate <= '{2}' ", pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());
                if (pEntityType == CheckEntityType.Business && !pGuid.CheckNullOrEmpty())
                {
                    sb.AppendFormat(" AND reg.BusinessRegisterId <> '{0}' ", pGuid);
                }
            #endregion

            dt = HRHelper.ExecuteDataTable(sb.ToString());

            #region 判断重复
            begin = pBeginDate.AddTimeToDateTime(pBeginTime);
            end = pEndDate.AddTimeToDateTime(pEndTime);
          //  IAttendanceTypeService typeSer = Factory.GetService<IAttendanceTypeService>();
            DateTime beginDate = DateTime.MinValue;
            DateTime endDate = DateTime.MinValue;
            string strBegin = string.Empty;
            string strEnd = string.Empty;
            string strBeginTime = string.Empty;
            string strEndTime = string.Empty;
            //请规律假
            DateTime beginPoint = DateTime.MinValue;//假期开始点
            DateTime endPoint = DateTime.MinValue;//假期结束点
            DateTime leaveBegin = DateTime.MinValue;//假期开始日期
            DateTime leaveEnd = DateTime.MinValue;//假期结束日期
            EmployeeService empSer = new EmployeeService();

            string empName=empSer.GetEmployeeNameById(pEmployeeId);

            DataTable typeTable = new DataTable();
            typeTable = HRHelper.ExecuteDataTable("select AttendanceTypeId,Code,Name from AttendanceType");
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    tempBegin = DateTime.MinValue;
                    tempEnd = DateTime.MinValue;
                    beginDate = DateTime.MinValue;
                    endDate = DateTime.MinValue;
                    typeId = row["AttendanceTypeId"] as string;
                    strBegin = row["BeginDate"].ToString();
                    strEnd = row["EndDate"].ToString();
                    strBeginTime = row["BeginTime"] as string;
                    strEndTime = row["EndTime"] as string;
                    if (!typeId.CheckNullOrEmpty() && !strBegin.CheckNullOrEmpty() && !strEnd.CheckNullOrEmpty()
                        && !strBeginTime.CheckNullOrEmpty() && !strEndTime.CheckNullOrEmpty())
                    {
                        DataRow[] typerows = typeTable.Select(string.Format("AttendanceTypeId ='{0}'", typeId));
                        if (typerows.Length > 0)
                        {
                            typeName = typerows[0]["Name"].ToString();
                        }
                        else {
                            throw new Exception("假勤类型id错误");
                        }
                        if (DateTime.TryParse(strBegin, out beginDate) && DateTime.TryParse(strEnd, out endDate))
                        {
                            if (DateTime.TryParse(beginDate.ToDateFormatString() + " " + strBeginTime, out tempBegin) &&
                                DateTime.TryParse(endDate.ToDateFormatString() + " " + strEndTime, out tempEnd))
                            {
                                //请规律假：每天校验一次
                                if ((pEntityType == CheckEntityType.Leave || pEntityType == CheckEntityType.TWALReg) && pIsRuleLeave)
                                {
                                    #region 规律假
                                    leaveBegin = pBeginDate.Date;
                                    leaveEnd = pEndDate.Date;
                                    beginPoint = pBeginDate.AddTimeToDateTime(pBeginTime);
                                    endPoint = pBeginDate.AddTimeToDateTime(pEndTime);

                                    if (beginPoint.CompareTo(endPoint) >= 0)
                                    {//跨天
                                        leaveEnd = pEndDate.AddDays(-1);
                                    }
                                    while (leaveBegin <= leaveEnd)
                                    {
                                        beginPoint = leaveBegin.AddTimeToDateTime(pBeginTime);
                                        endPoint = leaveBegin.AddTimeToDateTime(pEndTime);

                                        if (beginPoint.CompareTo(endPoint) >= 0)
                                        {//跨天
                                            endPoint = endPoint.AddDays(1);
                                        }
                                        if (!CheckTime(beginPoint, endPoint, tempBegin, tempEnd))
                                        {
                                            errorMsg += string.Format("员工{0}:{1}至{2}已存在{3}记录;", empName, tempBegin.ToDateTimeFormatString(false), tempEnd.ToDateTimeFormatString(false), typeName);
                                        }
                                        leaveBegin = leaveBegin.AddDays(1);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    if (!CheckTime(begin, end, tempBegin, tempEnd))
                                    {
                                        errorMsg += string.Format("员工{0}:{1}至{2}已存在{3}记录;", empName, tempBegin.ToDateTimeFormatString(false), tempEnd.ToDateTimeFormatString(false), typeName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion


            return errorMsg;
        }

        /// <summary>
        /// 检查时间是否交叉,false有交叉，true无交叉
        /// </summary>
        /// <param name="pRankBegin"></param>
        /// <param name="pRankEnd"></param>
        /// <param name="pPlanBegin"></param>
        /// <param name="pPlanEnd"></param>
        /// <returns></returns>
        protected bool CheckTime(DateTime pBegin1, DateTime pEnd1, DateTime pBegin2, DateTime pEnd2)
        {
            if ((pBegin1 <= pBegin2 && pEnd1 > pBegin2) || (pBegin1 > pBegin2 && pBegin1 < pEnd2))
            {
                return false;
            }
            return true;
        }

        // 查询这个人在该年度已经休的年假天数
        public decimal GetLeftDays(string pFiscalYearId, string pEmployeeId, string pAttendanceLeaveId)
        {
            #region 参数检查
            if (pEmployeeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("Employee is null");
            }
            #endregion
            EmployeeService employeeService = new EmployeeService();

            string corpId = employeeService.GetEmpFiledById(pEmployeeId,"CorporationId");
            ALPlanService planService = new ALPlanService ();
            DataTable paraDt = planService.GetParameterWithNoPower(corpId);
           // AnnualLeaveParameter para = Factory.GetService<IAnnualLeaveParameterService>().GetParameterByEmpIdWithNoPower(pEmployeeId);
            //先判断单位
            string daysName = "Days";
            if (paraDt != null && paraDt.Rows.Count > 0)
            {
                if (paraDt.Rows.Count == 1 &&
                    paraDt.Rows[0]["AnnualLeaveUnitId"].ToString().Equals("AnnualLeaveUnit_003"))
                {
                    daysName = "Hours";
                }
                else if(paraDt.Rows.Count >1) {
                    foreach (DataRow r in paraDt.Rows) {
                        if (r["CorporationId"].ToString() == corpId) {
                            if (r["AnnualLeaveUnitId"].ToString().Equals("AnnualLeaveUnit_003")){
                                daysName = "Hours";
                            }
                        }
                    }
                }
            }
            //回聘資料
            string reSQL = string.Format(@"select OldReportDate,LastDimissionDate,NewReportDate,OldCorporationId,NewCorporationId,* from EmployeeRehiring
                                            inner join FiscalYear on Year(NewReportDate)=FiscalYear.Year
                                            where EmployeeId='{0}' and FiscalYearId='{1}'", pEmployeeId, pFiscalYearId);
            DataTable reDT = HRHelper.ExecuteDataTable(reSQL);
         
            DataTable dt = new DataTable();
         
                string strSql = string.Empty;
                strSql = string.Format(@"select {0} from AttendanceLeaveInfo
                                            where AttendanceLeaveId not in
                                            (select AttendanceLeaveId From AttendanceLeave Where StateId='PlanState_003' and ApproveResultId='OperatorResult_002')
                                            and EmployeeId='{1}' and FiscalYearId='{2}' and IsRevoke='0' and AttendanceTypeId='401'", daysName, pEmployeeId, pFiscalYearId);
                //20180302 added by yingchun for A00-20180227002 : 退回重辦後，時數尚未還，導致可休時數不足
                if (!pAttendanceLeaveId.CheckNullOrEmpty())
                {
                    strSql += string.Format(" And AttendanceLeaveId <> '{0}' ", pAttendanceLeaveId);
                }

                //取回聘區間
                if (reDT.Rows.Count > 0 && !corpId.CheckNullOrEmpty())
                {
                    if (reDT.Rows[0]["OldCorporationId"].ToString() != reDT.Rows[0]["NewCorporationId"].ToString())
                    {
                        if (reDT.Rows[0]["OldCorporationId"].ToString() == corpId)
                        {
                            strSql += string.Format(@" and AttendanceLeaveInfo.Date between '{0}' and '{1}'",
                               Convert.ToDateTime(reDT.Rows[0]["OldReportDate"].ToString()).ToDateFormatString(),
                               Convert.ToDateTime(reDT.Rows[0]["LastDimissionDate"].ToString()).ToDateFormatString());
                        }
                        if (reDT.Rows[0]["NewCorporationId"].ToString() == corpId)
                        {
                            strSql += string.Format(@" and AttendanceLeaveInfo.Date >='{0}'",
                                Convert.ToDateTime(reDT.Rows[0]["NewReportDate"].ToString()).ToDateFormatString());
                        }
                    }
                }

               dt=HRHelper.ExecuteDataTable(strSql);
            
            if (dt != null && dt.Rows.Count > 0)
            {
                decimal leftDays = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //leftDays += Convert.ToDecimal(dt.Rows[i]["Hours"]);
                    leftDays += Convert.ToDecimal(dt.Rows[i][daysName]);
                }
                return leftDays;
            }
            return 0m;
        }

        //转换时间格式 T
        protected DateTime ConvertDateTime(string pDate, string pTime)
        {
            if (pDate.CheckNullOrEmpty())
            {
                pDate = DateTime.Now.ToShortDateString();
            }
            DateTime value;
            string format = string.Format("{0} {1}", pDate, pTime);
            DateTime.TryParse(format, out value);

            return value;
        }

        public virtual DataTable GetLeaveHoursForGP(string pAttendanceLeaveId, string pEmployeeId, DateTime pBeginDate, string pBeginTime, DateTime pEndDate, string pEndTime, string pAttendanceTypeId)
        {
            AttendanceEmpRankService attendanceEmpRankService = new AttendanceEmpRankService();
            #region 20120320 验证班次
            string noRank = attendanceEmpRankService.CheckEmpHasRank(pEmployeeId, ConvertDateTime(pBeginDate.ToString("yyyy-MM-dd"), pBeginTime), ConvertDateTime(pEndDate.ToString("yyyy-MM-dd"), pEndTime));
            if (!string.IsNullOrEmpty(noRank))
            {
                throw new Exception(noRank);
            }
            #endregion

            decimal totalHours = 0;  //总时数
            string unit = string.Empty;  //单位
            DataTable dtHoursAndUnit = new DataTable();
            dtHoursAndUnit.Columns.Add("Hours");
            dtHoursAndUnit.Columns.Add("Unit");

            ALPlanService planService = new ALPlanService();    
            EmployeeService empService = new EmployeeService();
            if (pAttendanceTypeId.Equals("401"))
            {
                string ccorporationId = empService.GetEmpFiledById(pEmployeeId,"CorporationId");
                DataTable paraDt=planService.GetParameterWithNoPower(ccorporationId);
                // AnnualLeaveParameter para = Factory.GetService<IAnnualLeaveParameterService>().GetParameterByEmpIdWithNoPower(pEmployeeId);
                string alUnit = string.Empty
                   ;
                if (paraDt != null) {
                    if (paraDt.Rows.Count == 1)
                    {
                        alUnit = paraDt.Rows[0]["AnnualLeaveUnitId"].ToString();//年假计算最小单位
                    }
                    else {
                        foreach (DataRow row in paraDt.Rows) {
                            if (row["CorporationId"].ToString().Equals(ccorporationId)) {
                                alUnit = row["AnnualLeaveUnitId"].ToString();//年假计算最小单位
                            }
                        }
                    }
                }
                
               //  alUnit = para.AnnualLeaveUnitId;//年假计算最小单位
                unit = alUnit;

               // IAnnualLeaveRegisterService tRegService = Factory.GetService<IAnnualLeaveRegisterService>();
                string tFiscalYearId = GetFisicalYearIdbyDate(pEmployeeId, pBeginDate);
                AttendanceLeave attLeave = new AttendanceLeave();
                attLeave.EmployeeId = pEmployeeId.GetGuid();
                attLeave.BeginDate = pBeginDate;
                attLeave.BeginTime = pBeginTime;
                attLeave.EndDate = pEndDate;
                attLeave.EndTime = pEndTime;
                attLeave.AttendanceTypeId = pAttendanceTypeId;
                attLeave.FiscalYearId = tFiscalYearId.GetGuid();
                attLeave.AttendanceLeaveId = !pAttendanceLeaveId.CheckNullOrEmpty() ? pAttendanceLeaveId.GetGuid() : SequentialGuid.NewGuid();
                attLeave.IsEss = true;
                this.SaveAlRegister(attLeave,true);

                decimal hours = 0M;
                foreach (AttendanceLeaveInfo info in attLeave.Infos)
                {
                    if (unit.Equals("AnnualLeaveUnit_003"))
                    {
                        hours += info.Hours;
                    }
                    else
                    {
                        hours += info.Days;
                    }
                }
                totalHours = hours;
            }
            else if (pAttendanceTypeId.Equals("408"))
            {
            
            }
            else
            {
                unit = "AnnualLeaveUnit_003";

                AttendanceLeave attLeave = new AttendanceLeave();
                attLeave.EmployeeId = pEmployeeId.GetGuid();
                attLeave.BeginDate = pBeginDate;
                attLeave.BeginTime = pBeginTime;
                attLeave.EndDate = pEndDate;
                attLeave.EndTime = pEndTime;
                attLeave.AttendanceTypeId = pAttendanceTypeId;
                attLeave.AttendanceLeaveId = !pAttendanceLeaveId.CheckNullOrEmpty() ? pAttendanceLeaveId.GetGuid() : SequentialGuid.NewGuid();
                attLeave.IsEss = true;
                totalHours = this.GetLeaveHours(attLeave);

                //20180207 yingchun for A00-20180201001 : 特殊假若是可休時數不足時，在計算時即顯示：請假核算量超出，不能請假
                UpdateSpecialDataNew(attLeave, true);
            }
            DataRow newRow = dtHoursAndUnit.NewRow();
            int tempHours = (int)totalHours;
            if (tempHours == totalHours)
                newRow["Hours"] = string.Format("{0:0.0000}", tempHours);
            else
                newRow["Hours"] = string.Format("{0:0.0000}", totalHours);
           
            if (unit.Equals("AnnualLeaveUnit_001"))
                unit = "AnnualLeaveUnit_002";

            DataTable dtUnit = HRHelper.ExecuteDataTable(string.Format("select * from codeinfo where codeinfoId='{0}'",unit)) ;
            if (dtUnit != null && dtUnit.Rows.Count > 0)
            {
                newRow["Unit"] = dtUnit.Rows[0]["ScName"].ToString();
            }
            else
            {
                newRow["Hours"] = "";
            }
            dtHoursAndUnit.Rows.Add(newRow);
            return dtHoursAndUnit;
        }

        public virtual void SaveAlRegister(AttendanceLeave pDataEntity, bool pIsForCheck)
        {
            AttendanceEmpRankService attendanceEmpRankService = new AttendanceEmpRankService();
            //20180302 added by yingchun for A00-20180227001 : 員工沒有年假計畫
            if (pDataEntity.FiscalYearId.CheckNullOrEmpty())
            {
                DateTime date = DateTime.MinValue;
                DateTime leaveBegin = pDataEntity.BeginDate.AddTimeToDateTime(pDataEntity.BeginTime);
                DataTable empRankDt = attendanceEmpRankService.GetEmpRanks(new string[] { pDataEntity.EmployeeId.ToString() }, pDataEntity.BeginDate.AddDays(-1), pDataEntity.BeginDate);
                foreach (DataRow dr in empRankDt.Rows)
                {
                    DateTime tpBDate = new DateTime();
                  
                    DateTime.TryParse(dr["Date"].ToString(), out tpBDate);
                    DateTime bDate = tpBDate.Date.AddTimeToDateTime(dr["WorkBeginTime"].ToString());
                    DateTime eDate = tpBDate.Date.AddTimeToDateTime(dr["WorkEndTime"].ToString());
                    if (bDate > eDate)
                    {
                        eDate = eDate.AddDays(1);
                    }
                    if (leaveBegin >= bDate && leaveBegin <= eDate)
                    {
                        date = Convert.ToDateTime(dr["Date"].ToString());
                        break;
                    }
                }
                string tFiscalYearId = string.Empty;
                if (date == DateTime.MinValue)
                {
                    tFiscalYearId = GetFisicalYearIdbyDate(pDataEntity.EmployeeId.ToString(), pDataEntity.BeginDate);
                }
                else
                {
                    tFiscalYearId = GetFisicalYearIdbyDate(pDataEntity.EmployeeId.ToString(), date);
                }

                if (tFiscalYearId.CheckNullOrEmpty())
                {
                    EmployeeService empSer=new EmployeeService();
                    string employeeName = empSer.GetEmployeeNameById(pDataEntity.EmployeeId.ToString());
                    throw new BusinessRuleException(string.Format("员工{0}没有年假计划！", employeeName));
                }
                else
                {
                    pDataEntity.FiscalYearId = tFiscalYearId.GetGuid();
                }
            }


            //检查员工当前财年当前日期年假计划 added by zhoug 20150116 for bug 25855 A00-20150108001
            //20180507 modified by Yangyaoming for Q00-20180428001 年假校驗日期錯誤
         ALPlanService aLPlanService = new ALPlanService(); 
            string msg = aLPlanService.CheckAnnualLeavePlan(pDataEntity.EmployeeId.ToString(), pDataEntity.FiscalYearId.ToString(), pDataEntity.BeginDate);
            if (!msg.CheckNullOrEmpty()) {
                throw new BusinessRuleException(msg);
            }


            #region 20101116 added by jiangpeng for 先更新年假结余表
            DataTable tempDT = new DataTable();
            if (!pIsForCheck)
            {
              
                //    string strSql = string.Empty;
                //    strSql = string.Format(@"select AttendanceLeaveId From AttendanceLeave where AttendanceLeaveId = '{0}'", pDataEntity.AttendanceLeaveId.ToString());
                   
                //tempDT = HRHelper.ExecuteDataTable(strSql);
               
                //if (tempDT != null && tempDT.Rows.Count > 0)
                //{
                //    IDocumentService<AttendanceLeave> tempDocSer = Factory.GetService<IAttendanceLeaveService>().GetServiceNoPower();
                //    tempReg = tempDocSer.Read(pDataEntity.AttendanceLeaveId.GetString());
                //    if (tempReg.AttendanceTypeId.Equals("401"))
                //    {
                //        ModifyALBalanceRepeal(tempReg);
                //    }
                //    // ModifyALBalanceRepeal(tempReg);
                //}
            }
            #endregion
            string checkStr = null;
           
           // IATMonthService atMonthSer = Factory.GetService<IATMonthService>();
            StringBuilder sbError = new StringBuilder();
            string errorMsg = string.Empty;
            foreach (AttendanceLeaveInfo info in pDataEntity.Infos)
            {
                errorMsg = atMonthSer.CheckIsClosedByEmployeeIdDate(info.EmployeeId.GetString(), info.Date);
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    sbError.Append(errorMsg);
                }
            }
            if (sbError.Length > 0)
            {
                throw new BusinessRuleException(sbError.ToString());
            }
            //校验时间重复
            checkStr = Factory.GetService<IAttendanceLeaveService>().CheckAllTime(pDataEntity.EmployeeId.GetString(), pDataEntity.BeginDate,
                pDataEntity.BeginTime, pDataEntity.EndDate, pDataEntity.EndTime, CheckEntityType.Leave, pDataEntity.AttendanceLeaveId.GetString(), pDataEntity.AttendanceLeaveId.CheckNullOrEmpty() ? true : false);
            if (!checkStr.CheckNullOrEmpty())
                throw new BusinessRuleException(checkStr);

            if (pDataEntity.Dirty)
            {//对主操作
                if (pDataEntity.IsRevoke)
                {//销过假了
                    throw new BusinessRuleException(Resources.Error_ModifyOnRevoked);
                }
                else
                {

                    //一共的天数(结余＋本年可休的天数)
                    decimal totalDays;
                    decimal balanceDays = 0m;

                    //获取这个人所在的公司
                    IDocumentService<Employee> empService = Factory.GetService<IEmployeeServiceEx>();
                    IEmployeeServiceEx service = empService as IEmployeeServiceEx;
                    string corporationId = service.GetEmployeeInfoById(pDataEntity.EmployeeId.GetString()).Rows[0]["CorporationId"].ToString();

                    #region ModifyBy wangcheng for 调用其他实现采取非工厂调用

                    IAnnualLeaveParameterService iAnnualLeaveParameterServer = Factory.GetService<IAnnualLeaveParameterService>();
                    #endregion

                    //根据公司获得参数
                    AnnualLeaveParameter parameter = iAnnualLeaveParameterServer.GetParameterIdByCorporationId(corporationId);
                    IAnnualLeaveBalanceService balanceSer = Factory.GetService<IAnnualLeaveBalanceService>();

                    if (parameter == null)
                    {
                        parameter = iAnnualLeaveParameterServer.GetParameterIdByCorporationId(Constants.SYSTEMGUID_CORPORATION_ROOT);
                        if (parameter == null) { throw new BusinessRuleException(Resources.AL_TheEmpCorpAnnualLeaveParameterNotExist); }
                    }
                    IAnnualLeavePlanService planService = Factory.GetService<IAnnualLeavePlanService>();
                    //totalDays = planService.GetDays(pDataEntity.EmployeeId.GetString(), pDataEntity.FiscalYearId.GetString(), pDataEntity.BeginDate, pDataEntity.EndDate);
                    PeriodDate period = Factory.GetService<IAttendanceEmployeeRankService>().GetRankBeginAndEnd(pDataEntity.EmployeeId.GetString(), pDataEntity.EndDate.AddDays(-1));
                    DateTime tempEndDateTime = DateTime.MinValue;
                    tempEndDateTime = pDataEntity.EndDate.AddTimeToDateTime(pDataEntity.EndTime);
                    //if (tempEndDateTime <= period.EndDate)
                    if (period != null && tempEndDateTime <= period.EndDate)
                    {
                        totalDays = planService.GetDays(pDataEntity.EmployeeId.GetString(), pDataEntity.FiscalYearId.GetString(), pDataEntity.BeginDate, pDataEntity.EndDate.AddDays(-1), corporationId);
                    }
                    else
                    {
                        totalDays = planService.GetDays(pDataEntity.EmployeeId.GetString(), pDataEntity.FiscalYearId.GetString(), pDataEntity.BeginDate, pDataEntity.EndDate, corporationId);
                    }
                    DateTime bEndDate = Constants.MAXDATEVALUE;
                    //结余管理里面的数据
                    AnnualLeaveBalance balanceEntity = null;
                    balanceEntity = balanceSer.GetThisAnnualLeaveBalance(pDataEntity.FiscalYearId.GetString(), pDataEntity.EmployeeId.GetString(), corporationId);
                    if (parameter.IsBalance)
                    {//剩余年假结转下一年
                        //totalDays += planService.GetBalanceDaysPreYears(pDataEntity.EmployeeId.GetString(), pDataEntity.FiscalYearId.GetString());//加上结余,不能从结余表里取,从本表取上一年的
                        //20140107 added by wangyan  for 年假处理 分结转日期前与结转日期后
                        if (balanceEntity != null)
                        {
                            balanceDays = balanceEntity.BalanceDays;//直接取结余表里的结余天数
                        }
                        else
                        {

                            //20170718 added by cutemeow for 41572 Q00-20170714004 如果結轉表找不到資料，只去找員工該年度的時數
                            balanceDays = 0;

                        }
                        // balanceDays = planService.GetBalanceDaysPreYears(pDataEntity.EmployeeId.GetString(), pDataEntity.FiscalYearId.GetString());//加上结余,不能从结余表里取,从本表取上一年的

                        bEndDate = Factory.GetService<IAnnualLeaveBalanceService>().getBalanceEndDate(pDataEntity.FiscalYearId.GetString(), pDataEntity.EmployeeId.GetString(), corporationId);

                    }
                    //added by jiangpeng for 20120423 判断可休天数前先判断单位 

                    string daysName = "Days";
                    if (parameter != null && parameter.AnnualLeaveUnitId != null && parameter.AnnualLeaveUnitId.Equals("AnnualLeaveUnit_003"))
                    {
                        daysName = "Hours";
                    }
                    decimal bcDays = 0;  //本次已休天数
                    decimal bcDaysB = 0;  //本次已休天数 结转日之后的

                    if (tempReg != null)
                    {
                        if (daysName.Equals("Hours"))
                        {
                            foreach (AttendanceLeaveInfo info in tempReg.Infos)
                            {
                                if (!info.IsRevoke)
                                {
                                    bcDays += info.Hours;
                                }
                            }
                        }
                        else
                        {
                            foreach (AttendanceLeaveInfo info in tempReg.Infos)
                            {
                                if (!info.IsRevoke)
                                {
                                    bcDays += info.Days;
                                }
                            }
                        }
                    }

                    totalDays += balanceDays;

                    //20180302 modified by yingchun for A00-20180227002 : ESSF07退回重辦後，年假時數尚未還，導致可休時數不足
                    decimal leftDays = 0;
                    if (pDataEntity.IsEss && pIsForCheck)
                    {
                        leftDays = this.GetLeftDays(pDataEntity.FiscalYearId.GetString(), pDataEntity.EmployeeId.GetString(), pDataEntity.AttendanceLeaveId.GetString()) - bcDays;
                    }
                    else
                    {
                        leftDays = this.GetLeftDays(pDataEntity.FiscalYearId.GetString(), pDataEntity.EmployeeId.GetString()) - bcDays;
                    }

                    // 年假再次保存时会报错,如果是负数是不正确的
                    if (leftDays < 0)
                        leftDays = 0;
                    decimal leavingDays = totalDays - leftDays;//待休的天数
                    decimal thisYearLeaving = 0m;//本年剩下几天
                    decimal balanceLeaving = 0m;//结转剩下几天

                    if (balanceEntity != null)
                    {
                        //20141231 modified by huangzj for 25670 C01-20141230009 本年剩下几天=本年计划 -（本年已休-结转已休）
                        thisYearLeaving = balanceEntity.ThisYearDays - (balanceEntity.ActualDays - balanceEntity.BalanceActualDays);//本年计划5天减去 -（本年已休5天-结转已休2天） 本年 可休2天
                        balanceLeaving = balanceEntity.BalanceDays - balanceEntity.BalanceActualDays;//上年结转-结转已休
                    }


                    // 20081209 modified by zhonglei for 验证未休不补
                    if (parameter.IsOnce && leftDays > 0)
                    {
                        throw new BusinessRuleException(Resources.AL_IsOnce);
                    }

                    //清除掉明细
                    pDataEntity.Infos.Clear();

                    //////加载明细记录///////
                    this.SetRegisterInfos(pDataEntity, leavingDays);

                    // 20101118 added by jiangpeng 
                    decimal currentDays = 0;
                    #region 20120510 modified by songyj for 单位统一
                    for (int a = 0; a < pDataEntity.Infos.Count; a++)
                    {
                        if (!pDataEntity.Infos[a].IsRevoke)
                        {
                            if (daysName.Equals("Hours"))
                            {
                                currentDays += pDataEntity.Infos[a].Hours;
                                if (bEndDate.Date != Constants.MAXDATEVALUE)
                                {
                                    if (pDataEntity.Infos[a].Date > bEndDate)
                                    {
                                        bcDaysB += pDataEntity.Infos[a].Hours;//截止日之後的是5天
                                    }
                                }
                            }
                            else
                            {
                                currentDays += pDataEntity.Infos[a].Days;
                                if (bEndDate.Date != Constants.MAXDATEVALUE)
                                {
                                    if (pDataEntity.Infos[a].Date > bEndDate)
                                    {
                                        bcDaysB += pDataEntity.Infos[a].Days;//截止日之後的是5天
                                    }
                                }
                            }

                        }
                    }
                    #endregion
                    if (currentDays > leavingDays)
                    {
                        throw new BusinessRuleException(string.Format(Resources.AL_RegisterNoDays, empService.Read(pDataEntity.EmployeeId.GetString()).CnName, (Math.Round(leavingDays, 1)).ToString(),
                                    (Math.Round(currentDays, 1)).ToString(), ((char)10).ToString(), ((char)13).ToString()));
                    }
                    if (bcDaysB > thisYearLeaving)
                    {
                        throw new Exception(string.Format(Resources.AL_RegisterNoDays, empService.Read(pDataEntity.EmployeeId).CnName, (Math.Round(thisYearLeaving, 1)).ToString(),
                                   (Math.Round(bcDaysB, 1)).ToString(), ((char)10).ToString(), ((char)13).ToString()));
                    }
                    if (pDataEntity.AttendanceTypeId.CheckNullOrEmpty())
                        pDataEntity.AttendanceTypeId = "401";
                }

                //本来就休息，无需请假
                if (pDataEntity.Infos.Count == 0)
                {
                    throw new BusinessRuleException(string.Format(Resources.Business_EmpRankNotExist, "", "") + "或" + Resources.Error_Notleave);
                }

                //更新年假结余
                if (!pIsForCheck)
                {
                    ModifyALBalanceAudit(pDataEntity);
                }
            }

        }


        /// <summary>
        /// 根据类型Id获取名称
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetAttendanceTypeNameById(string typeId) {
            #region 参数检查
            if (typeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("typeId Error");
            }
            #endregion

            DataTable dt = HRHelper.ExecuteDataTable(string.Format("select name from AttendanceType where AttendanceTypeId='{0}'", typeId));

            if (dt != null && dt.Rows.Count > 0)
            {

                return dt.Rows[0][0].ToString();
            }

            return string.Empty;
        }

        // 20101229 added by jiangpeng
        /// <summary>
        /// 通过开始日期找到计划所在区间的财年
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public virtual string GetFisicalYearIdbyDate(string pEmployeeId, DateTime pDate)
        {
            DataTable dt = new DataTable();
          
             string  strSql = string.Format(@"SELECT   Planemp.FiscalYearId
                                            FROM     AnnualLeavePlanemPloyee AS Planemp
                                                     LEFT JOIN FiscalYear
                                                       ON Planemp.FiscalYearId = FiscalYear.FiscalYearId
                                            WHERE    Planemp.EmployeeId = '{0}'
                                                     AND Planemp.BeginDate <= '{1}'
                                                     AND Planemp.EndDate >= '{1}'
                                                     AND Planemp.Flag = '1'
                                            ORDER BY FiscalYear.[Year] DESC", pEmployeeId, pDate.ToDateFormatString());
             dt=HRHelper.ExecuteDataTable(strSql); ;
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0][0].ToString();
            }
            return string.Empty;
        }


        /// <summary>
        /// 来源单别
        /// </summary>
        /// <returns></returns>
        public DataTable GetResourceFrom()
        {
            DataTable dt = HRHelper.GetCodeInfo("ResourceFrom");
            return dt;
        }
    }

}
