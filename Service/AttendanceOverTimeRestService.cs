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
using Dcms.HR.DataEntities;

using Dcms.Common;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Resources;
using Dcms.Common.DataEntities;
using Dcms.Common.Services;
using Dcms.HR;
using System.Drawing;

namespace BQHRWebApi.Service
{

    public class AttendanceOverTimeRestService : HRService
    {

        /// <summary>
        /// 校驗調休假資料(與請假合併，不對外提供接口)
        /// </summary>
        /// <param name="formEntity">實體</param>
        /// <returns>>無錯誤返回空字串，有錯誤則返回錯誤訊息</returns>
        /// <remarks>實體參數EmployeeId,BeginDate,BeginTime,EndDate,EndTime,Remark,Hours</remarks>
        public  string CheckForAPI(AttendanceOverTimeRest formEntity)
        {
            string msg = string.Empty;
            try
            {
                if (formEntity.AttendanceOverTimeRestId.CheckNullOrEmpty())
                {
                    formEntity.AttendanceOverTimeRestId = Guid.NewGuid();
                }
                formEntity.DeductOhter = false;
           
                formEntity.StateId = "PlanState_001";
              
                formEntity.Flag = true;
              
                this.CheckForESS(formEntity);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return msg;
        }


        /// <summary>
        /// 獲取調休假時數(與請假合併，不對外提供接口)
        /// (調休的API方法與請假合併，若要另外開放調休API接口，須增加新的實體AttendanceOverTimeRestForAPI，並調整傳送參數實體名稱)
        /// </summary>
        /// <param name="formEntity">實體</param>
        /// <returns>返回調休假時數, 單位</returns>
        /// <remarks>實體參數EmployeeId,BeginDate,BeginTime,EndDate,EndTime</remarks>
        public  DataTable GetRestHoursForAPI(AttendanceOverTimeRest formEntity)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Hours");
            dt.Columns.Add("Unit");
            dt.Rows.Add(this.GetHoursForESS(formEntity), "小時");
            return dt;
        }
        public  decimal GetHoursForESS(AttendanceOverTimeRest pOTRest)
        {
            //校验调休是否重复
            //20141016 added by lidong S00-20140521007调整重复校验方法
            //        string errMsg = Factory.GetService<IAttendanceLeaveService>().CheckAllTime(pOTRest.EmployeeId.GetString(), pOTRest.BeginDate, pOTRest.BeginTime
            //, pOTRest.EndDate, pOTRest.EndTime, CheckEntityType.OTRest, true, string.Empty);

            #region  验证班次
            //add by shenzy 2017-4-6 for Q00-20161229003
            AttendanceEmpRankService empRankService = new AttendanceEmpRankService();

            string noRank = empRankService.CheckEmpHasRank(pOTRest.EmployeeId.ToString(), ConvertDateTime(pOTRest.BeginDate.ToString("yyyy-MM-dd"), pOTRest.BeginTime), ConvertDateTime(pOTRest.EndDate.ToString("yyyy-MM-dd"), pOTRest.EndTime));
            if (!string.IsNullOrEmpty(noRank))
            {
                throw new Exception(noRank);
            }
            #endregion
            AttendanceLeaveService leaveSer = new AttendanceLeaveService();
            string errMsg = leaveSer.CheckAllTime(pOTRest.EmployeeId.GetString(), pOTRest.BeginDate, pOTRest.BeginTime
    , pOTRest.EndDate, pOTRest.EndTime, CheckEntityType.OTRest, pOTRest.AttendanceOverTimeRestId.GetString(), true);
            if (!errMsg.CheckNullOrEmpty())
            {
                throw new BusinessRuleException(errMsg);
            }

            //校驗時數 20170512 yingchun for S00-20170207012_39172+39173
            SetOTRest(ref pOTRest);

            decimal hours = this.DealDailyTimes(pOTRest);
            return decimal.Parse(hours.ToString("#0.00"));
        }

        //转换时间格式 T
        private DateTime ConvertDateTime(string pDate, string pTime)
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

        public  string CheckForESS(AttendanceOverTimeRest pOTRest)
        {
            //add by shenzy 2016-11-22 for A00-20161111004
            SetOTRest(ref pOTRest);
            //
            ExceptionCollection ec = SaveBeforeCheck(pOTRest, true, true);
            if (ec.Count > 0)
            {
                throw new BusinessVerifyException(new string[] { pOTRest.AttendanceOverTimeRestId.GetString() }, ec);
            }
            AttendanceLeaveService leaveSer = new AttendanceLeaveService();
            //校验调休是否重复
            //20141016 added by lidong S00-20140521007调整重复校验方法
            //        string errMsg = Factory.GetService<IAttendanceLeaveService>().CheckAllTime(pOTRest.EmployeeId.GetString(), pOTRest.BeginDate, pOTRest.BeginTime
            //, pOTRest.EndDate, pOTRest.EndTime, CheckEntityType.OTRest, true, string.Empty);
            string errMsg = leaveSer.CheckAllTime(pOTRest.EmployeeId.ToString(), pOTRest.BeginDate, pOTRest.BeginTime
    , pOTRest.EndDate, pOTRest.EndTime, CheckEntityType.OTRest, string.Empty, true);
            if (!errMsg.CheckNullOrEmpty())
            {
                throw new BusinessRuleException(errMsg);
            }

            //校验时数
            SetOTRest(ref pOTRest);

            //20150415 added for 27765 && Q00-20150409006 by renping 移到SaveBeforeCheck用归属日期验证
            foreach (var info in pOTRest.DailyInfo)
            {
                string errorMsg = leaveSer.CheckHasRankChangeData(new string[] { info.EmployeeId.GetString() }, info.Date, info.Date);
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    throw new BusinessRuleException(errorMsg);
                }
            }

            //20210119 added by yccho for Q00-20210114002 : 增加時數校驗判斷
            IDocumentService<AttendanceOTAdjust> adjustService2 = Factory.GetService<IAttendanceOTAdjustService>();
            List<AttendanceOTAdjust> list = new List<AttendanceOTAdjust>();
            AttendanceOTAdjust temp = null;

            foreach (AttendanceOverTimeRestInfo item in pOTRest.RestInfo)
            {
                var tempAdjusts = list.Where(t => t.AttendanceOTAdjustId == item.AttendanceOTAdjust);
                if (tempAdjusts.Count() > 0)
                {
                    temp = tempAdjusts.First();
                }
                else
                {
                    temp = adjustService2.Read(item.AttendanceOTAdjust);
                }
                temp.ActualAdjustHours += item.Hours;
                if (temp.ActualAdjustHours > temp.AdjustHours)
                {
                    throw new BusinessRuleException(Resources.AttendanceOverTimeRest_NotEnoughHour);
                }

                if (tempAdjusts.Count() == 0)
                {
                    temp.ExtendedProperties.Add("AttendanceOverTimeRest", "AttendanceOverTimeRest");
                    list.Add(temp);
                }
            }

            return "True";
        }

        /// <summary>
        /// 保存方法前的检查
        /// </summary>
        /// <param name="pDataEntity">加班计划实体</param>
        /// <returns>异常集合ExceptionCollection</returns>
        public ExceptionCollection SaveBeforeCheck(AttendanceOverTimeRest pDataEntity, bool pIsNewEntity, bool pIsESS)
        {
            ExceptionCollection ec = new ExceptionCollection();
            HRVerifyHelper vh = new HRVerifyHelper(ec);

            #region 主实体检查
            vh.StringNotNullOrEmpty(pDataEntity, "EmployeeId");
            vh.DateTimeNotIsEmpty(pDataEntity, "BeginDate");
            vh.StringNotNullOrEmpty(pDataEntity, "BeginTime");
            vh.DateTimeNotIsEmpty(pDataEntity, "EndDate");
            vh.StringNotNullOrEmpty(pDataEntity, "EndTime");
            vh.GreaterThan<decimal>(pDataEntity, "Hours", 0m, true);
            #endregion

            #region 明细检查
            foreach (AttendanceOverTimeRestInfo item in pDataEntity.RestInfo)
            {
                vh.StringNotNullOrEmpty(item, "AttendanceOTAdjust");
                //vh.DateTimeNotIsEmpty(item, "BeginDate");
                //vh.StringNotNullOrEmpty(item, "BeginTime");
                //vh.DateTimeNotIsEmpty(item, "EndDate");
                //vh.StringNotNullOrEmpty(item, "EndTime");
                vh.GreaterThan<decimal>(item, "Hours", 0m, true);
            }
            #endregion

            //检查考勤是否关账
            if (ec.Count == 0)
            {
                ATMonthService atMonthSer = new ATMonthService ();
                //明细检查
                string errorMsg = string.Empty;
                foreach (AttendanceLeaveInfo info in pDataEntity.DailyInfo)
                {
                  
                    errorMsg = atMonthSer.CheckESSIsClose(new string[] { info.EmployeeId.GetString() }, info.Date, info.Date);
                   
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        throw new BusinessRuleException(errorMsg);
                    }
                }
                #region 20140929 deleted by huangzj for 在客户端校验是否有记录重复　22436 S00-20140521007
                ////校验调休是否重复
                //string errMsg = Factory.GetService<IAttendanceLeaveService>().CheckAllTime(pDataEntity.EmployeeId.GetString(), pDataEntity.BeginDate, pDataEntity.BeginTime, pDataEntity.EndDate, pDataEntity.EndTime
                //    , CheckEntityType.OTRest, pIsNewEntity, pDataEntity.AttendanceOverTimeRestId.GetString());
                #endregion
                AttendanceLeaveService lSer = new AttendanceLeaveService();
                string errMsg = GetSalaryTimes(pDataEntity);
                if (!errMsg.CheckNullOrEmpty())
                {
                    ec.Add(new BusinessRuleException(errMsg));
                }

            }
            return ec;
        }

        public virtual string GetSalaryTimes(AttendanceOverTimeRest pAttendanceOverTimeRest)
        {
            string msg = string.Empty;
            AttendanceTypeService typeService = new AttendanceTypeService();
            //IDocumentService<AttendanceType> typeService = Factory.GetService<IAttendanceTypeService>().GetServiceNoPower();
            // IAttendanceTypeService typeServiceEx = Factory.GetService<IAttendanceTypeService>();
            AttendanceType atType = typeService.GetAttendanceType("406");
            //假勤项目：请假（AttendanceKind_004）
            if (atType.MaxSalaryHour > 0 && atType.IsUse)
            {
                //using (IConnectionService con = Factory.GetService<IConnectionService>())
                //{
                //    IDbCommand cmd = con.CreateDbCommand();
                    string beginDate = string.Empty;
                    string endDate = string.Empty;
                    if (!atType.SalaryPeriodId.CheckNullOrEmpty())
                    {//计算周期
                        if (atType.SalaryPeriodId.Equals("SalaryPeriod_001"))
                        {
                            //年
                            beginDate = string.Format("{0}-1-1 0:00:00", pAttendanceOverTimeRest.BeginDate.Year);
                            endDate = (DateTime.Parse(beginDate).AddYears(1).AddSeconds(-1)).ToString(Constants.FORMAT_DATETIMEPATTERN);
                        }
                        else
                        {
                            //月
                            beginDate = string.Format("{0}-{1}-1 0:00:00", pAttendanceOverTimeRest.BeginDate.Year, pAttendanceOverTimeRest.BeginDate.Month);
                            endDate = (DateTime.Parse(beginDate).AddMonths(1).AddSeconds(-1)).ToString(Constants.FORMAT_DATETIMEPATTERN);
                        }

                        //20170622 Modfyied by jiangpeng for bug 41176 & A00-20170621003 修改sql 去掉已销假的时数，并使用明细的date来确定日期范围 
                        string strSQL = string.Format(@"select sum(AttendanceOTRestDaily.Hours) from AttendanceOTRestDaily 
														left join attendanceotrest on AttendanceOTRestDaily.AttendanceOverTimeRestId=attendanceotrest.AttendanceOverTimeRestId
														WHERE   attendanceotrest.employeeid = '{0}' and AttendanceOTRestDaily.Date between '{1}' and '{2}'
														 and ((attendanceotrest.stateid = 'PlanState_003' 
                                                                   AND attendanceotrest.approveresultid = 'OperatorResult_001' ) 
                                                                  OR (attendanceotrest.stateid = 'PlanState_004' 
                                                                       AND attendanceotrest.approveresultid = 'OperatorResult_001' ) 
                                                                  OR (attendanceotrest.stateid IN ( 
                                                                       'PlanState_001','PlanState_002'))) 
                                                           AND attendanceotrest.flag = 1 AND AttendanceOTRestDaily.IsRevoke = '0'", pAttendanceOverTimeRest.EmployeeId.GetString(), beginDate, endDate);
                        if (!pAttendanceOverTimeRest.AttendanceOverTimeRestId.CheckNullOrEmpty())
                        {
                            strSQL += string.Format(" AND AttendanceOTRestDaily.AttendanceOverTimeRestId <> '{0}'", pAttendanceOverTimeRest.AttendanceOverTimeRestId.GetString());
                        }

                       // cmd.CommandText = strSQL;
                        object obj = HRHelper.ExecuteScalar(strSQL);
                        //当前调休时数
                        decimal currentHours = 0.0m;
                        for (int i = 0; i < pAttendanceOverTimeRest.RestInfo.Count; i++)
                        {
                            currentHours += pAttendanceOverTimeRest.RestInfo[i].Hours;
                        }
                        //调休总时数
                        decimal totalHours = currentHours;
                        if (obj != null && obj != DBNull.Value)
                        {
                            totalHours += (decimal)obj;
                        }

                        //按照假勤单位转换最大可调休时数
                        decimal salaryHour = atType.MaxSalaryHour;//最大允许调休时数
                        if (atType.AttendanceUnitId.Equals("AttendanceUnit_001"))
                        {
                            //天
                            salaryHour = salaryHour * 8;//一天默认为八小时
                        }
                        else if (atType.AttendanceUnitId.Equals("AttendanceUnit_003"))
                        {
                            //分钟 
                            salaryHour = salaryHour / 60;
                        }

                        if (totalHours > salaryHour)
                        {
                            EmployeeService empService = new EmployeeService ();
                        string empcode = empService.GetEmpFiledById(pAttendanceOverTimeRest.EmployeeId.GetString(),"Code");
                           // IDocumentService<Employee> docEmp = Factory.GetService<IEmployeeServiceEx>().GetServiceNoPower();
                            msg = string.Format("工号为 {0} 的员工当前请假总时数为 {1}，已超过最大值 {2}", empcode, totalHours.ToString("#.##"), salaryHour.ToString("#.##"));
                            //empService.GetEmployeeCodeById(pAttendanceOverTimeRest.EmployeeId), 
                        }
                    }
                //}
            }
            return msg;
        }


        /// <summary>
        /// 加班调休数据处理
        /// </summary>
        /// <param name="rests"></param>
        public void SetOTRest(ref AttendanceOverTimeRest rest)
        {
            decimal tempHours = 0;  //调休时数
            tempHours = this.DealDailyTimes(rest);
            rest.Hours = tempHours;
            this.SetRestInfo(rest, tempHours);
        }


        //计算调休每日时数
        private decimal DealDailyTimes(AttendanceOverTimeRest adjust)
        {

            bool isPassRest = false;  //是否跳过休息日
            bool isDeductOhter = false;  //是否扣除班次以外（休息、就餐）时间
            AttendanceTypeService typeService = new AttendanceTypeService ();
            AttendanceLeaveService leaveService = new AttendanceLeaveService ();
            #region 20110628 modified by songyj for 添加调休时数折算算法：同请假算法（和UI端代码一致）
            decimal minLeaveHours = 1;  //最小核算量
            decimal minAuditHours = 1;  //最小审核量

            DataTable dtLeaveConfig = typeService.GetLeaveConfig("406");
            if (dtLeaveConfig != null && dtLeaveConfig.Rows.Count > 0)
            {
                if (dtLeaveConfig.Rows[0][0].ToString().Equals("1") || dtLeaveConfig.Rows[0][0].ToString().ToUpper().Equals("TRUE"))
                {
                    isPassRest = true;
                }
                if (dtLeaveConfig.Rows[0][1].ToString().Equals("1") || dtLeaveConfig.Rows[0][1].ToString().ToUpper().Equals("TRUE"))
                {
                    isDeductOhter = true;
                }
                Decimal.TryParse(dtLeaveConfig.Rows[0][2].ToString(), out minLeaveHours);
                Decimal.TryParse(dtLeaveConfig.Rows[0][3].ToString(), out minAuditHours);
            }
            #endregion

            //初始化调休时数
            decimal tempHours = 0;
            //取当前调休申请
            adjust.StateId = "PlanState_002";
            //移除调休每日时间安排
            adjust.DailyInfo.Clear();
            //调休开始时间
            DateTime adjustBegin = adjust.BeginDate.AddTimeToDateTime(adjust.BeginTime);
            //调休结束时间
            DateTime adjustEnd = adjust.EndDate.AddTimeToDateTime(adjust.EndTime);
            //每日班次服务
         //   atten rankService = Factory.GetService<IAttendanceEmployeeRankService>();

            // 20101014 added by jiangpeng for 增加校验
            //string errMsg = Factory.GetService<IAttendanceLeaveService>().CheckAllTime(adjust.EmployeeId.GetString(), adjust.BeginDate, adjust.BeginTime
            //    , adjust.EndDate, adjust.EndTime, CheckEntityType.OTRest, (adjust.AttendanceOverTimeRestId).CheckNullOrEmpty() ? true : false, string.Empty);
            //20160224 modi by songll for A00-20160219002
            string errMsg = leaveService.CheckAllTime(adjust.EmployeeId.GetString(), adjust.BeginDate, adjust.BeginTime
                , adjust.EndDate, adjust.EndTime, CheckEntityType.OTRest, adjust.AttendanceOverTimeRestId.GetString(), true);
            if (!errMsg.CheckNullOrEmpty())
                throw new BusinessRuleException(errMsg);

            //员工调休期间相关每日班次
            //DataTable dtRankInfo = rankService.GetEmpsDailyInfo(new string[] { adjust.EmployeeId.GetString() },
            //    adjustBegin.AddDays(-1), adjustEnd.AddDays(1));

            //20150311 modified for 26977 && Q00-20150309001 by renping
            DataTable dtRankInfo;
         
                dtRankInfo = rankService.GetEmpsDailyInfo(new string[] { adjust.EmployeeId.GetString() },
                adjustBegin.AddDays(-1), adjustEnd.AddDays(1));
                IDocumentService<AttendanceRank> docRankService = Factory.GetService<IAttendanceRankService>();
                Dictionary<string, bool> dicIsBelongBefore = new Dictionary<string, bool>();  //是否归属前一天字典
                bool isBelongBefore = false;
                #region 20150312 edit by LinBJ 27704~27706 Q00-20150310004 不論是否要扣除休息班段，都需循環整個班段
                //班次相关时间
                //Dictionary<string, List<string[]>> restDic = Factory.GetService<IAttendanceRankService>().GetRankRestTime();
                Dictionary<string, List<string[]>> restDic = Factory.GetService<IAttendanceRankService>().GetNormalRankRest();
                //循环员工每日班次
                foreach (DataRow item in dtRankInfo.Rows)
                {
                    string rankId = item["AttendanceRankId"].ToString();//班次Id
                    string employeeRankId = item["AttendanceEmployeeRankId"].ToString();//员工班次Id

                    #region 20090722 added by jiangpeng for 是否扣除休息日
                    if (this.IsRestInfo.ContainsKey(rankId) &&
                        this.IsRestInfo[rankId])
                    { //休息 
                        if (isPassRest)
                            continue;
                    }
                    #endregion
                    //20120104 added by songyj for 判断是否有班次
                    if (!restDic.ContainsKey(rankId))
                    {
                        continue;
                    }
                    List<string[]> restTimes = restDic[rankId];
                    DateTime rankDate = Convert.ToDateTime(item["Date"]);//归属日期
                    DateTime RankBegin = Convert.ToDateTime(item["BeginTime"]);//班次开始时间
                    //循环班次相关时间

                    #region 20110809 added by songyj for 班段调休改为整班次调休
                    List<DateTime> leaveTimeList = new List<DateTime>();
                    decimal leaveHours = 0;
                    DateTime leaveBegin = DateTime.MinValue;
                    DateTime leaveEnd = DateTime.MinValue;
                    //20110525 added by songyj for 对于弹性班段的调休处理，备注上要添加说明
                    string strFlexRemark = "";
                    foreach (string[] restTime in restTimes)
                    {
                        //在岗开始
                        DateTime workBegin = Convert.ToDateTime(item["BeginTime"]).AddTimeToDateTime(restTime[0]);
                        //在岗结束
                        DateTime workEnd = Convert.ToDateTime(item["BeginTime"]).AddTimeToDateTime(restTime[1]);
                        bool notJobTime = bool.Parse(restTime[4]);
                        
                      
                            if (workBegin < RankBegin)
                            {
                                workBegin = workBegin.AddDays(1);
                            }
                            if (workEnd < workBegin)
                            {
                                workEnd = workEnd.AddDays(1);
                            }
                       
                        //在岗时间
                        //20110715 modified by songyj for 整班段调休时取班段时数：RestHours
                        //decimal workHours = Convert.ToDecimal((workEnd - workBegin).TotalHours);
                        decimal workHours = Convert.ToDecimal(restTime[3]);
                        //取时间交集
                        Rectangle result = GetIntersect(new DateTime[] { adjustBegin, adjustEnd }, new DateTime[] { workBegin, workEnd });

                        #region 20110525 added by songyj for 处理加班调休时数时加入弹性班次的判断
                        int beginCode = -1;
                        int endCode = -1;
                        PeriodDate period = Factory.GetService<IAttendanceRollcallService>().GetFlexBeginEndDate(rankId, adjust.EmployeeId.GetString(), RankBegin.Date, ref beginCode, ref endCode, adjust.AttendanceOverTimeRestId.GetString());
                        int rankCode = int.Parse(restTime[2]);
                        //string strFlexRemark = "";
                        if (period != null && beginCode != -1 && endCode != -1)
                        {
                            if (rankCode == endCode)
                            {//最后一个正常班段
                                //班次的最后一个班段的结束时间不等于弹性之后的时间，则添加备注
                                if (period.EndDate != workEnd)
                                {
                                    //根据刷卡数据判断为弹性上班的，并且请假开始时间≠班次开始时间时，才在请假明细信息的备注中写入
                                    DateTime rankBegin = Convert.ToDateTime(item["BeginTime"]);//班次开始
                                    if (adjustBegin != rankBegin)
                                    {
                                        result = GetIntersect(new DateTime[] { adjustBegin, adjustEnd }, new DateTime[] { workBegin, period.EndDate });//弹性班次的最后一个正常班段的结束时间
                                        strFlexRemark = string.Format(Resources.OTResult_FlexRemark, period.BeginDate.ToTimeFormatString(), period.EndDate.ToTimeFormatString());
                                        #region S00-20150609002 29817 对3段班(上午、休息、下午)，并且下午整班段请假时的班次时间平移逻辑进行调整
                                        IDocumentService<AttendanceRank> docRankSer = Factory.GetService<IAttendanceRankService>().GetServiceNoPower();
                                        AttendanceRank rank = docRankSer.Read(rankId);
                                        // List<RankRestinfo> restList = this.ranksInfo[rankId].OrderBy(rest => rest.RankCode).ToList();
                                        if (restTimes.Count == 3 && rank.IsFlexFirstEnd)
                                        {
                                            if (restTimes[1][4] == "True"
                                              && restTimes[0][4] == "False"
                                              && restTimes[2][4] == "False")
                                            {
                                                if (adjust.BeginTime == restTimes[2][0] && adjust.EndTime == restTimes[2][1])
                                                {
                                                    int min = (period.BeginDate - rankBegin).Minutes;
                                                    strFlexRemark = string.Format(Resources.OTResult_FlexRemark, period.BeginDate.ToTimeFormatString(),
                                                      Convert.ToDateTime(item["BeginTime"]).AddTimeToDateTime(restTimes[0][1]).AddMinutes(min).ToTimeFormatString());
                                                    if (min < 0)
                                                    {
                                                        result = GetIntersect(new DateTime[] { adjustBegin, adjustEnd }, new DateTime[] { workBegin, workEnd });//弹性班次的最后一个正常班段的结束时间
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                        #endregion

                        if (result.IsEmpty || result.Left == result.Right)
                        {
                            //没有交集，不需要处理调休
                        }
                        else if (isDeductOhter && notJobTime)
                        {
                            continue;//休息段須扣除
                        }
                        else
                        {//调休区间
                            DateTime leaveBegin2 = DateTime.MinValue.AddMinutes(result.Left);
                            DateTime leaveEnd2 = DateTime.MinValue.AddMinutes(result.Right);
                            leaveTimeList.Add(leaveBegin2);
                            leaveTimeList.Add(leaveEnd2);

                            //需要判断是否满班次
                            if (leaveBegin2.CompareTo(workBegin) == 0 && leaveEnd2.CompareTo(workEnd) == 0)
                            {//满班次
                                leaveHours += workHours;
                            }
                            else
                            {
                                leaveHours += CalculateHours(leaveBegin2, leaveEnd2);
                            }
                        }
                    }
                    if (leaveHours > 0)
                    {
                        leaveTimeList.Sort();
                        leaveBegin = leaveTimeList[0];
                        leaveEnd = leaveTimeList[leaveTimeList.Count - 1];

                        AttendanceLeaveInfo info = new AttendanceLeaveInfo();
                        info.AttendanceRankId = rankId;
                        info.AttendanceTypeId = "406";
                        info.EmployeeId = adjust.EmployeeId;
                        info.EmployeeRankId = employeeRankId.GetGuid();
                        info.BeginDate = leaveBegin.Date;
                        info.EndDate = leaveEnd.Date;
                        info.BeginTime = leaveBegin.ToString(Constants.FORMAT_SHORTTIMEPATTERN);
                        info.EndTime = leaveEnd.ToString(Constants.FORMAT_SHORTTIMEPATTERN);
                        info.Hours = leaveHours;
                        //20110628 modified by songyj for 修改时数折算算法：同请假算法
                        //20120821 modified for bug9042
                        //info.Hours = this.GetAmountHours(minLeaveHours, minAuditHours, info.Hours);
                        info.Hours = this.GetAmountHours(minLeaveHours, minAuditHours, Math.Round(info.Hours, 8));
                        info.Date = rankDate.Date;
                        //20110525 added by songyj for 对于弹性班段的加班调休处理，备注上要添加说明
                        //20141125 LinBJ Add by Q00-20141103010 24390 24391 
                        //若有弹性再加判请假明细是否有请假开始时间!＝班次开始时间的数据，则该天班次有弹性，请假明细备注中追加写入相关弹性信息
                        IDocumentService<AttendanceRank> ATRankService = Factory.GetService<IAttendanceRankService>().GetServiceNoPower();
                        AttendanceRank rank = ATRankService.Read(rankId);
                        if (!strFlexRemark.CheckNullOrEmpty() && info.BeginTime != rank.WorkBeginTime)
                            info.Remark = strFlexRemark;
                        adjust.DailyInfo.Add(info);
                        tempHours += info.Hours;
                    }
                    #endregion
                }
                #endregion
            


           
            return Math.Round(tempHours, 2);
        }


    }



    /// <summary>
    /// 班次内区间相关
    /// </summary>
    class RankRestinfo
    {

        /// <summary>
        /// 班段类型
        /// </summary>
        public string? AttendanceRankType;

        /// <summary>
        /// 班段时数
        /// </summary>
        public double? RestHours;

        /// <summary>
        /// 班段开始时间
        /// </summary>
        public string? RestBegin;

        /// <summary>
        /// 班段结束时间
        /// </summary>
        public string? RestEnd;

        //20110720 added by songyj for 计算请假时数时与客户端逻辑相同
        /// <summary>
        /// 是否扣除在时数
        /// </summary>
        public bool? NotJobTime;

        //20110520 added by songyj for 判断弹性班次的依据
        /// <summary>
        /// 班段编号
        /// </summary>
        public int? RankCode;
    }
    class IdAndHours
    {
        public string? Id { get; set; }
        public decimal? Hours { get; set; }
    }
}
