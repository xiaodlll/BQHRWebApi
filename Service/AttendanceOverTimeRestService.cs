using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.HR.Services;
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
                    formEntity.AttendanceOverTimeRestId = SequentialGuid.NewGuid();
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

            string errMsg = Factory.GetService<IAttendanceLeaveService>().CheckAllTime(pOTRest.EmployeeId.GetString(), pOTRest.BeginDate, pOTRest.BeginTime
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

            bool isPassHoliday = false; //休假是否跳过節日
            bool isPassRest = false;  //休假是否跳过假日
            bool isPassBreakDay = false;//休假是否跳过休息日
            bool isDeductOhter = false;  //是否扣除班次以外（休息、就餐）时间
            bool isPassEmptyDay = false; //請假跳過空班
            IAttendanceTypeService typeService = Factory.GetService<IAttendanceTypeService>();
            #region 20110628 modified by songyj for 添加调休时数折算算法：同请假算法（和UI端代码一致）
            decimal minLeaveHours = 1;  //最小核算量
            decimal minAuditHours = 1;  //最小审核量

            DataTable dtLeaveConfig = typeService.GetLeaveConfig("406");
            if (dtLeaveConfig != null && dtLeaveConfig.Rows.Count > 0)
            {
                #region 20170322 add by LinBJ for S00-20170306005 增加跳過節日、休息日選項
                isPassRest = dtLeaveConfig.Rows[0]["PassRest"].ToString().ToBool();
                isPassHoliday = dtLeaveConfig.Rows[0]["PassHoliday"].ToString().ToBool();
                isPassBreakDay = dtLeaveConfig.Rows[0]["PassBreakDay"].ToString().ToBool();
                isDeductOhter = dtLeaveConfig.Rows[0]["DeductOhter"].ToString().ToBool();
                isPassEmptyDay = dtLeaveConfig.Rows[0]["PassEmptyDay"].ToString().ToBool();
                #endregion
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
            IAttendanceEmployeeRankService rankService = Factory.GetService<IAttendanceEmployeeRankService>();
            IDocumentService<AttendanceRank> docRankSer = Factory.GetService<IAttendanceRankService>().GetServiceNoPower();
            Dictionary<string, AttendanceRank> dicRank = new Dictionary<string, AttendanceRank>();
            // 20101014 added by jiangpeng for 增加校验
            //string errMsg = Factory.GetService<IAttendanceLeaveService>().CheckAllTime(adjust.EmployeeId.GetString(), adjust.BeginDate, adjust.BeginTime
            //    , adjust.EndDate, adjust.EndTime, CheckEntityType.OTRest, (adjust.AttendanceOverTimeRestId).CheckNullOrEmpty() ? true : false, string.Empty);
            //20160224 modi by songll for A00-20160219002
            string errMsg = Factory.GetService<IAttendanceLeaveService>().CheckAllTime(adjust.EmployeeId.GetString(), adjust.BeginDate, adjust.BeginTime
                , adjust.EndDate, adjust.EndTime, CheckEntityType.OTRest, adjust.AttendanceOverTimeRestId.GetString(), true);
            if (!errMsg.CheckNullOrEmpty())
                throw new BusinessRuleException(errMsg);

            //员工调休期间相关每日班次
            //DataTable dtRankInfo = rankService.GetEmpsDailyInfo(new string[] { adjust.EmployeeId.GetString() },
            //    adjustBegin.AddDays(-1), adjustEnd.AddDays(1));

            //20150311 modified for 26977 && Q00-20150309001 by renping
            DataTable dtRankInfo;
            if (HRHelper.IsCirculationIndustry)
            {
                //dtRankInfo = rankService.GetEmpRankInfoCI(new string[] { adjust.EmployeeId.GetString() },
                //adjustBegin.AddDays(-1), adjustEnd.AddDays(1));
                //20150506 modified by lidong A00-20150422010 因最小核算量&最小审核量问题，调整流通版计算方法
                DataTable empRankDt = rankService.GetCIEmpRank(adjust.EmployeeId.GetString(), adjustBegin.AddDays(-1), adjustEnd.AddDays(1));
                IDocumentService<AttendanceRank> docRank = Factory.GetService<IAttendanceRankService>();
                if (empRankDt != null && empRankDt.Rows.Count > 0)
                {
                    //班次开始时间（流通包没有弹性班次，所以这一块逻辑可以删除）
                    //DateTime rankBegin = DateTime.MinValue;
                    Dictionary<string, string> dicRankBegin = new Dictionary<string, string>();

                    DateTime restBegin = DateTime.MinValue;
                    DateTime restEnd = DateTime.MinValue;
                    List<string> listRankId = new List<string>();  //班次数组
                    DateTime tempRestEnd = DateTime.MinValue;
                    DateTime date = DateTime.MinValue;
                    string atHolidayType = string.Empty;
                    foreach (DataRow var in empRankDt.Rows)
                    {
                        if (DateTime.TryParse(var["Date"].ToString(), out date))
                        {
                            //20140916 modified by huangzj for 22225 A00-20140912004 请假天数用班次时数折算，不再用8小时
                            decimal workHours = 0m;
                            listRankId = new List<string>();
                            atHolidayType = var["AttendanceHolidayTypeId"].ToString();
                            if (!var["AttendanceRankId"].ToString().CheckNullOrEmpty())
                            {
                                listRankId.Add(var["AttendanceRankId"].ToString());
                                if (docRank.Contains(var["AttendanceRankId"].ToString()))
                                {
                                    AttendanceRank rankTemp = docRank.Read(var["AttendanceRankId"].ToString(), true);
                                    workHours += rankTemp.WorkHours;
                                }
                            }
                            if (!var["AttendanceRankId2"].ToString().CheckNullOrEmpty())
                            {
                                listRankId.Add(var["AttendanceRankId2"].ToString());
                                if (docRank.Contains(var["AttendanceRankId2"].ToString()))
                                {
                                    AttendanceRank rankTemp = docRank.Read(var["AttendanceRankId2"].ToString(), true);
                                    workHours += rankTemp.WorkHours;
                                }
                            }
                            if (!var["AttendanceRankId3"].ToString().CheckNullOrEmpty())
                            {
                                listRankId.Add(var["AttendanceRankId3"].ToString());
                                if (docRank.Contains(var["AttendanceRankId3"].ToString()))
                                {
                                    AttendanceRank rankTemp = docRank.Read(var["AttendanceRankId3"].ToString(), true);
                                    workHours += rankTemp.WorkHours;
                                }
                            }
                            tempRestEnd = DateTime.MinValue;
                            foreach (string rankId in listRankId)
                            {
                                //#region 是否扣除休息日20100714改为是否跳过休息班次
                                //if (this.IsRestInfo.ContainsKey(rankId) &&
                                //    this.IsRestInfo[rankId]) { //休息 
                                //    if (isPassRest)
                                //        continue;
                                //}
                                //#endregion
                                #region 20170322 add by LinBJ for S00-20170306005 增加跳過節日、休息日選項
                                if (atHolidayType == "DefaultHolidayType003" && isPassHoliday)
                                {
                                    continue;//跳過節日
                                }
                                if (atHolidayType == "DefaultHolidayType004" && isPassRest)
                                {
                                    continue;//跳過假日
                                }
                                if (atHolidayType == "DefaultHolidayType005" && isPassBreakDay)
                                {
                                    continue;//跳過休息日
                                }
                                if (var["EmptyDay"].ToString().ToBool() && isPassEmptyDay)
                                {
                                    continue;//跳過空班
                                }
                                #endregion
                                #region 处理班段相关
                                if (!this.ranksInfo.ContainsKey(rankId))
                                {
                                    IAttendanceRankService rankSer = Factory.GetService<IAttendanceRankService>();
                                    DataTable rankRestDt = rankSer.GetRankRestInfo(rankId);
                                    if (rankRestDt.Rows.Count > 0)
                                    {
                                        if (!dicRankBegin.ContainsKey(rankId))
                                        {
                                            dicRankBegin.Add(rankId, rankRestDt.Rows[0]["WorkBeginTime"].ToString());
                                        }

                                        List<RankRestinfo> tempList = new List<RankRestinfo>();
                                        RankRestinfo tempRest = new RankRestinfo();
                                        double tempDouble = 0;
                                        foreach (DataRow item in rankRestDt.Rows)
                                        {
                                            tempRest = new RankRestinfo();
                                            //班段类型
                                            tempRest.AttendanceRankType = item["AttendanceRankTypeId"].ToString();
                                            //班段时数
                                            if (!double.TryParse(item["RestHours"].ToString(), out tempDouble))
                                                throw new BusinessRuleException(string.Format(Resources.Error_RankDetail, item["Name"].ToString()));
                                            tempRest.RestHours = tempDouble;
                                            //班段开始时间
                                            tempRest.RestBegin = item["RestBeginTime"].ToString();
                                            //班段结束时间
                                            tempRest.RestEnd = item["RestEndTime"].ToString();
                                            tempRest.NotJobTime = item["NotJobTime"].ToString().ToUpper().Equals("TRUE") || item["NotJobTime"].ToString().Equals("1") ? true : false;
                                            //20110520 added by songyj for 判断弹性班次的依据
                                            tempRest.RankCode = int.Parse(item["Code"].ToString());
                                            tempList.Add(tempRest);
                                        }
                                        this.ranksInfo.Add(rankId, tempList);
                                    }
                                    // 20081210 modified by zhonglei for 没有班次明细则抛出异常
                                    else
                                    {
                                        IDocumentService<AttendanceRank> docService = rankSer;
                                        throw new BusinessRuleException(string.Format(Resources.AT_RankInfoNotNull, docService.Read(rankId).Code));
                                    }
                                }
                                #endregion

                                #region 处理请假(班段请假改为整班次请假)
                                //DateTime.TryParse(date.ToDateFormatString() + " " + dicRankBegin[rankId], out rankBegin);

                                List<DateTime> leaveTimeList = new List<DateTime>();
                                decimal leaveHours = 0;
                                //对于弹性班段的请假处理，备注上要添加说明
                                string strFlexRemark = string.Empty;
                                DateTime leaveBegin = DateTime.MinValue;
                                DateTime leaveEnd = DateTime.MinValue;
                                foreach (RankRestinfo item in this.ranksInfo[rankId])
                                {
                                    //调整为判断是否扣除在岗时数
                                    if (isDeductOhter && item.NotJobTime)
                                        continue;
                                    if (item.AttendanceRankType == "AttendanceRankType_004")
                                    {
                                        continue;
                                    }
                                    if (DateTime.TryParse(date.ToDateFormatString() + " " + item.RestBegin, out restBegin) &&
                                        DateTime.TryParse(date.ToDateFormatString() + " " + item.RestEnd, out restEnd))
                                    {
                                        if (restBegin < tempRestEnd)
                                        {
                                            restBegin = restBegin.AddDays(1);
                                        }
                                        if (restEnd < restBegin)
                                        {
                                            restEnd = restEnd.AddDays(1);
                                        }
                                        tempRestEnd = restEnd;

                                        Rectangle result = GetIntersect(new DateTime[] { adjustBegin, adjustEnd }, new DateTime[] { restBegin, restEnd });

                                        if (result.IsEmpty || result.Left == result.Right)
                                        {//无需请假
                                        }
                                        else
                                        {//请假时间区间
                                            #region  明细处理
                                            DateTime leaveBegin2 = DateTime.MinValue.AddMinutes(result.Left);
                                            DateTime leaveEnd2 = DateTime.MinValue.AddMinutes(result.Right);
                                            leaveTimeList.Add(leaveBegin2);
                                            leaveTimeList.Add(leaveEnd2);

                                            //需要判断是否满班次
                                            if (leaveBegin2.CompareTo(restBegin) == 0 && leaveEnd2.CompareTo(restEnd) == 0)
                                            {//满班次
                                                leaveHours += (decimal)item.RestHours;
                                            }
                                            else
                                            {
                                                leaveHours += CalculateHours(leaveBegin2, leaveEnd2);
                                            }

                                            #endregion
                                        }
                                    }
                                }
                                #region 20110105 added by jianpeng for 合并请假记录
                                if (leaveHours > 0)
                                {
                                    //设置请假的班次时间
                                    leaveTimeList.Sort();
                                    leaveBegin = leaveTimeList[0];
                                    leaveEnd = leaveTimeList[leaveTimeList.Count - 1];
                                    AttendanceLeaveInfo info = new AttendanceLeaveInfo();
                                    info.AttendanceRankId = rankId;
                                    info.AttendanceTypeId = "406";
                                    info.EmployeeId = adjust.EmployeeId;
                                    info.EmployeeRankId = var["AttendanceEmployeeRankId"].ToString().GetGuid();
                                    info.BeginDate = leaveBegin.Date;
                                    info.EndDate = leaveEnd.Date;
                                    info.BeginTime = leaveBegin.ToString(Constants.FORMAT_SHORTTIMEPATTERN);
                                    info.EndTime = leaveEnd.ToString(Constants.FORMAT_SHORTTIMEPATTERN);
                                    info.Hours = leaveHours;
                                    info.Hours = this.GetAmountHours(minLeaveHours, minAuditHours, Math.Round(info.Hours, 8));
                                    info.Date = date.Date;
                                    //对于弹性班段的请假处理，备注上要添加说明
                                    //20141125 LinBJ Add by Q00-20141103010 24390 24391 
                                    //若有弹性再加判请假明细是否有请假开始时间!＝班次开始时间的数据，则该天班次有弹性，请假明细备注中追加写入相关弹性信息
                                    IDocumentService<AttendanceRank> ATRankService = Factory.GetService<IAttendanceRankService>().GetServiceNoPower();
                                    AttendanceRank rank = ATRankService.Read(rankId);
                                    if (!strFlexRemark.CheckNullOrEmpty() && info.BeginTime != rank.WorkBeginTime)
                                    {
                                        info.Remark = strFlexRemark;
                                    }
                                    info.Remark += adjust.Remark;
                                    adjust.DailyInfo.Add(info);//加入明细
                                    tempHours += info.Hours;
                                }
                                #endregion
                                #endregion
                            }
                        }
                    }
                }
            }
            else
            {
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
                    string atHolidayType = item["AttendanceHolidayTypeId"].ToString();
                    if (!dicRank.ContainsKey(rankId))
                    {
                        dicRank.Add(rankId, docRankSer.Read(rankId));
                    }
                    AttendanceRank rank = dicRank[rankId];
                    //#region 20090722 added by jiangpeng for 是否扣除休息日
                    //if (this.IsRestInfo.ContainsKey(rankId) &&
                    //    this.IsRestInfo[rankId]) { //休息 
                    //    if (isPassRest)
                    //        continue;
                    //}
                    //#endregion
                    #region 20170322 add by LinBJ for S00-20170306005 增加跳過節日、休息日選項
                    if (atHolidayType == "DefaultHolidayType003" && isPassHoliday)
                    {
                        continue;//跳過節日
                    }
                    if (atHolidayType == "DefaultHolidayType004" && isPassRest)
                    {
                        continue;//跳過假日
                    }
                    if (atHolidayType == "DefaultHolidayType005" && isPassBreakDay)
                    {
                        continue;//跳過休息日
                    }
                    if (item["EmptyDay"].ToString().ToBool() && isPassEmptyDay)
                    {
                        continue;//跳過空班
                    }
                    #endregion
                    //20120104 added by songyj for 判断是否有班次
                    if (!restDic.ContainsKey(rankId))
                    {
                        continue;
                    }
                    #region 取是否归属前一天
                    //if (!dicIsBelongBefore.ContainsKey(rankId)) {
                    //    IAttendanceRankService rankSer = Factory.GetService<IAttendanceRankService>();
                    //    DataTable rankRestDt = rankSer.GetRankRestInfo(rankId);
                    //    if (rankRestDt.Rows.Count > 0) {
                    //        isBelongBefore = false;
                    //        Boolean.TryParse(rankRestDt.Rows[0]["IsBelongToBefore"].ToString(), out isBelongBefore);
                    //        if (!dicIsBelongBefore.ContainsKey(rankId)) {
                    //            dicIsBelongBefore.Add(rankId, isBelongBefore);
                    //        }
                    //    }
                    //}
                    #endregion
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
                    DateTime flexBegin = DateTime.MinValue; //彈性開始時間
                    DateTime flexEnd = DateTime.MinValue; //彈性結束時間
                    int beginCode = -1;
                    int endCode = -1;
                    bool isFlexFirstLeave = false;
                    PeriodDate period = Factory.GetService<IAttendanceRollcallService>().GetFlexBeginEndDate(rankId, adjust.EmployeeId.GetString(), rankDate, ref beginCode, ref endCode, adjust.AttendanceOverTimeRestId.GetString());
                    foreach (string[] restTime in restTimes)
                    {
                        //在岗开始
                        DateTime workBegin = Convert.ToDateTime(item["BeginTime"]).AddTimeToDateTime(restTime[0]);
                        //在岗结束
                        DateTime workEnd = Convert.ToDateTime(item["BeginTime"]).AddTimeToDateTime(restTime[1]);
                        bool notJobTime = bool.Parse(restTime[4]);
                        //20150311 added for 26977 && Q00-20150309001 by renping
                        if (!HRHelper.IsCirculationIndustry)
                        {
                            if (workBegin < RankBegin)
                            {
                                workBegin = workBegin.AddDays(1);
                            }
                            if (workEnd < workBegin)
                            {
                                workEnd = workEnd.AddDays(1);
                            }
                        }
                        else
                        {//20150323 added by lidong for A00-20150320005
                            if (restTime[2] != item["Code"].ToString())
                            {
                                continue;
                            }
                        }
                        //在岗时间
                        //20110715 modified by songyj for 整班段调休时取班段时数：RestHours
                        //decimal workHours = Convert.ToDecimal((workEnd - workBegin).TotalHours);
                        decimal workHours = Convert.ToDecimal(restTime[3]);
                        // 20170411 added by LinBJ for A00-20170313003 加班就餐段不参与计算
                        if (restTime[5].ToString() == "AttendanceRankType_004")
                        {
                            continue;
                        }
                        //取时间交集
                        Rectangle result = GetIntersect(new DateTime[] { adjustBegin, adjustEnd }, new DateTime[] { workBegin, workEnd });

                        #region 20110525 added by songyj for 处理加班调休时数时加入弹性班次的判断

                        int rankCode = int.Parse(restTime[2]);
                        //20200529 add by LinBJ for A00-20200526001 調整請假銷假申請作業，若是班次為彈性班次，第二天的請假時間不可判斷為彈性
                        if (adjust.DailyInfo.Count() > 0)
                        {
                            period = null;
                        }
                        //20190603 add by LinBJ for A00-20190530004 若是該天為第二張請假的單據時,送單後不可再增加判斷當天是否彈性班次
                        if (period != null && rank.RollCallForFelx == 1)
                        {
                            if (ChkEmpDateLeave(adjust.EmployeeId.GetString(), RankBegin.Date, adjust.AttendanceOverTimeRestId.ToString()))
                            {
                                period = null;
                            }
                        }
                        if (period != null && beginCode != -1 && endCode != -1)
                        {
                            if (rankCode == beginCode || rankCode == endCode)
                            {
                                DateTime rankBegin = Convert.ToDateTime(item["BeginTime"]);//班次开始
                                if (adjustBegin != rankBegin || (period.BeginDate < rankBegin && rank.IsCollectFelx))
                                {
                                    DateTime newRestBegin = workBegin;
                                    DateTime newRestEnd = workEnd;
                                    if (rankCode == beginCode)
                                    {
                                        newRestBegin = period.BeginDate;
                                    }
                                    if (rankCode == endCode)
                                    {
                                        newRestEnd = period.EndDate;
                                    }
                                    result = GetIntersect(new DateTime[] { adjustBegin, adjustEnd }, new DateTime[] { newRestBegin, newRestEnd });//弹性班次的最后一个正常班段的结束时间
                                    strFlexRemark = string.Format(Resources.OTResult_FlexRemark, period.BeginDate.ToTimeFormatString(), period.EndDate.ToTimeFormatString());
                                    flexBegin = period.BeginDate;
                                    flexEnd = period.EndDate;
                                    #region 20150706 LinBJ add by S00-20150609002 29817 对3段班(上午、休息、下午)，并且下午整班段请假时的班次时间平移逻辑进行调整
                                    List<AttendanceRankRest> restList = rank.Rests.OrderBy(rest => rest.Code).ToList();
                                    if (restList.Count() >= 3 && restList.Where(t => t.AttendanceRankTypeId == "AttendanceRankType_001").Count() == 2)
                                    {
                                        if (restList[1].AttendanceRankTypeId != "AttendanceRankType_001"
                                          && restList[0].AttendanceRankTypeId == "AttendanceRankType_001"
                                          && restList[2].AttendanceRankTypeId == "AttendanceRankType_001")
                                        {
                                            if (adjust.BeginTime == restList[2].RestBeginTime && adjust.EndTime == restList[2].RestEndTime)
                                            {
                                                double min = (period.BeginDate - rankBegin).TotalMinutes;
                                                strFlexRemark = string.Format(Resources.OTResult_FlexRemark, period.BeginDate.ToTimeFormatString(),
                                                   rankBegin.AddHours((double)restList[0].RestHours).AddMinutes(min).ToTimeFormatString());
                                                isFlexFirstLeave = true;
                                                flexBegin = period.BeginDate;
                                                flexEnd = rankBegin.AddHours((double)restList[0].RestHours).AddMinutes(min);
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
                                if (isFlexFirstLeave)
                                {
                                    //20210908 add by LinBJ for A00-20210831001 彈性下午班段全請需抓班次時數
                                    leaveHours += workHours;
                                }
                                else
                                {
                                    leaveHours += CalculateHours(leaveBegin2, leaveEnd2);
                                }
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
                        if (!strFlexRemark.CheckNullOrEmpty() && (info.BeginTime != rank.WorkBeginTime || (period != null && period.BeginDate < RankBegin && rank.IsCollectFelx)))
                        {
                            info.Remark = strFlexRemark;
                            info.FlexBeginDate = flexBegin;
                            info.FlexEndDate = flexEnd;
                        }
                        info.Remark += adjust.Remark;
                        adjust.DailyInfo.Add(info);
                        tempHours += info.Hours;
                    }
                    #endregion
                }
                #endregion
            }

          
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
