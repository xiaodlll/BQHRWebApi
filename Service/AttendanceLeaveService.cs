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
using System.Drawing;

namespace BQHRWebApi.Service
{

    public class AttendanceLeaveService : HRService
    {


        public override void Save(DataEntity[] entities)
        {
            foreach (var entity in entities)
            {
                AttendanceLeaveForAPI enty = entity as AttendanceLeaveForAPI;

                SaveAttendanceLeave(enty);
            }
        }


        public void SaveAttendanceLeave(AttendanceLeaveForAPI enty)
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


        public virtual string CheckForAPI(AttendanceLeaveForAPI formEntity)
        {
            this.CheckValue(formEntity);

            string msg = string.Empty;

            AttendanceLeaveForAPI[] arrayEntity = new AttendanceLeaveForAPI[] { formEntity };

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
                //List<AttendanceOverTimeRest> list = this.ChangeToOTRestEntity(arrayEntity);
                //AttendanceOverTimeRestService restSer=new AttendanceOverTimeRestService ();
                //msg = restSer.CheckForAPI(list.First());
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
                        throw new BusinessRuleException(string.Format("工号为 {0} 的员工当前请假总时数为 {1}，已超过最大值 {2}", tmpMsg[0], tmpMsg[2], tmpMsg[1]));
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
        /// 请假资料数据校验
        /// </summary>
        /// <param name="pAttendanceLeave">要校验的请假对象</param>
        /// <returns>错误</returns>
        public virtual void CheckForESS(AttendanceLeave pAttendanceLeave)
        {
            //检查数据，有ESS检查可以写在这里...
           // IDocumentService<AttendanceType> service = Factory.GetService<IAttendanceTypeService>();
        
            AttendanceTypeService typeService = new AttendanceTypeService();
            AttendanceType type=typeService.GetAttendanceType(pAttendanceLeave.AttendanceTypeId);
            //DataTable dtType = HRHelper.ExecuteDataTable(string.Format("select * from AttendanceType where AttendanceTypeId='{0}'", pAttendanceLeave.AttendanceTypeId));

            //List<AttendanceType> myObjects = HRHelper.DataTableToList<AttendanceType>(dtType);
            //type=myObjects[0];
            if (type != null)
            {//判断
                if (string.Equals(type.IsSalaryId, Constants.TRUE_ID))
                { //带薪
                    decimal max = type.MaxSalaryHour;
                    if (type.AttendanceUnitId.Equals("AttendanceUnit_001"))
                    {
                        //天
                        max = max * 8;
                    }
                    else if (type.AttendanceUnitId.Equals("AttendanceUnit_003"))
                    {
                        //分钟 
                        max = max / 60;

                    }
                    if (!max.Equals(0m))
                    { //0 为无限制
                        decimal totalHours = 0m;
                        foreach (AttendanceLeaveInfo obj in pAttendanceLeave.Infos)
                        {
                            if (!obj.IsRevoke)//未销假
                                totalHours += obj.Hours;
                        }

                        if (totalHours > max)
                            throw new BusinessRuleException("带薪假期超出最大带薪假期时数");//{$Resources$}

                    }

                }
                if (!type.SexType.CheckNullOrEmpty() && !type.SexType.Equals("SexType_001"))
                {
                    EmployeeService employeeService = new EmployeeService();
                    string sexType = employeeService.GetEmpFiledById(pAttendanceLeave.EmployeeId.GetString(), "genderId");
                    string empName = employeeService.GetEmployeeNameById(pAttendanceLeave.EmployeeId.GetString());
                    if (!sexType.CheckNullOrEmpty())
                    {
                        //（假勤适用性别为男，员工性别为女）或（假勤适用性别为女，员工性别为男）时，抛异常
                        if ((type.SexType.Equals("SexType_002") && sexType.Equals("Gender_002")) || (type.SexType.Equals("SexType_003") && sexType.Equals("Gender_001")))
                        {
                            throw new BusinessRuleException(string.Format("员工{0}性别与请假类型适用性别不符", empName));
                        }
                    }
                }

                 
            }
             

           ExceptionCollection ec = this.SaveBeforeCheck(pAttendanceLeave,true);
            //20141017 added by lidong 22436 S00-20140521007
            //20150316 modify by lidong 将是否是请假中的请规律假改为false A00-20150312007
            string errMsg = this.CheckAllTime(pAttendanceLeave.EmployeeId.GetString(), pAttendanceLeave.BeginDate, pAttendanceLeave.BeginTime, pAttendanceLeave.EndDate, pAttendanceLeave.EndTime, CheckEntityType.Leave, pAttendanceLeave.AttendanceLeaveId.GetString(), false);
            if (!errMsg.CheckNullOrEmpty())
                ec.Add(new Exception(errMsg));

            //20180309 yingchun for A00-20180308001 : 請年假時需校驗該月公司考勤是否已關帳
            if (pAttendanceLeave.AttendanceTypeId.Equals("401"))
            {
               // IATMonthService atMonthSer = Factory.GetService<IATMonthService>();
               ATMonthService atMonthSer = new ATMonthService();
                errMsg = atMonthSer.CheckIsClose(new string[] { pAttendanceLeave.EmployeeId.GetString() }, pAttendanceLeave.BeginDate, pAttendanceLeave.EndDate);
                if (!errMsg.CheckNullOrEmpty())
                    ec.Add(new Exception(errMsg));
            }

            if (ec.Count > 0)
            {
                throw new BusinessVerifyException(pAttendanceLeave, ec);
            }
            if (pAttendanceLeave.AttendanceTypeId.Equals("401"))
            {
                this.SaveAlRegister(pAttendanceLeave, true);
            }


            if (ec.Count > 0)
            {
                throw new BusinessVerifyException(pAttendanceLeave, ec);
            }
            UpdateSpecialDataNew(pAttendanceLeave, true);
        }
        /// <summary>
        /// 保存方法前的检查
        /// </summary>
        /// <param name="pDataEntity">请假申请实体</param>
        /// <returns>异常集合ExceptionCollection</returns>
        public ExceptionCollection SaveBeforeCheck(AttendanceLeave pDataEntity, bool pIsEss)
        {
            ExceptionCollection ec = new ExceptionCollection();
          
            bool isRuleLeave = false;
          EmployeeService employeeService= new EmployeeService();
            string corpId = employeeService.GetEmpFiledById(pDataEntity.EmployeeId.GetString(), "CorporationId");
            pDataEntity.CorporationId = corpId.GetGuid(); 
           
            if (!pDataEntity.AttendanceTypeId.Equals("401"))
                //20121219 added by songyj for 清除明细
                pDataEntity.Infos.Clear();

            DateTime begin = pDataEntity.BeginDate;//假期开始
            DateTime end = pDataEntity.EndDate;//假期结束

            if (!isRuleLeave)
            { //折算
                if (!pDataEntity.AttendanceTypeId.Equals("401"))
                {
                  
                   pDataEntity = SetLeaveInfos(pDataEntity, ConvertDateTime(begin.ToShortDateString(), pDataEntity.BeginTime), ConvertDateTime(end.ToShortDateString(), pDataEntity.EndTime));
                   
                }
            }
            else
            { //规律假
                TimeSpan ts = end - begin;
                int days = ts.Days; //间隔天数
                DateTime beginPoint, endPoint;
                for (int i = 0; i <= days; i++)
                {
                    beginPoint = begin.AddDays(i);
                    endPoint = beginPoint; //时间结束点
                    if (ConvertDateTime("", pDataEntity.BeginTime).CompareTo(ConvertDateTime("", pDataEntity.EndTime)) >= 0)
                    {//跨天 
                        endPoint = endPoint.AddDays(1);
                    }
                    if (endPoint.CompareTo(end) <= 0)
                    {//循环的截至日期小于等于请假的结束日期
                        if (!pDataEntity.AttendanceTypeId.Equals("401"))
                        {
                           
                                pDataEntity = SetLeaveInfos(pDataEntity, ConvertDateTime(beginPoint.ToShortDateString(), pDataEntity.BeginTime), ConvertDateTime(endPoint.ToShortDateString(), pDataEntity.EndTime));
                            
                        }
                    }
                }
            }

            if (!pDataEntity.AttendanceTypeId.Equals("401") && pDataEntity.Infos.Count == 0)
            {
                //如果没排班，则报异常员工未排班
                AttendanceEmpRankService empRankService = Factory.GetService<AttendanceEmpRankService>();
                DataTable dtEmpRank = empRankService.GetEmpRanks(new string[] { pDataEntity.EmployeeId.GetString() }, pDataEntity.BeginDate, pDataEntity.EndDate);
                if (dtEmpRank == null || dtEmpRank.Rows.Count == 0)
                {
                    throw new BusinessRuleException("员工未排班");
                }
                throw new BusinessRuleException("输入时段内不存在需要请假的时间区间");
            }
             

            ATMonthService atMonthSer = Factory.GetService<ATMonthService>();
            StringBuilder sbError = new StringBuilder();
            string errorMsg = string.Empty;
         
            //20140808 modified for 上面的方法有问题 20801 & C01-20140806013 by renping
            foreach (AttendanceLeaveInfo info in pDataEntity.Infos)
            {
              
                    errorMsg = atMonthSer. CheckIsClose(new string[] { info.EmployeeId.GetString() }, info.Date, info.Date);
                

                if (!string.IsNullOrEmpty(errorMsg))
                {
                    sbError.Append(errorMsg);
                }

                //20150415 added for 27765 && Q00-20150409006 by renping 移到SaveBeforeCheck用归属日期验证
                errorMsg = CheckHasRankChangeData(new string[] { info.EmployeeId.GetString() }, info.Date, info.Date);
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    sbError.Append(errorMsg);
                }
            }
            if (sbError.Length > 0)
            {
                pDataEntity.Infos.Clear();//add by LinBJ Bug 10390 多次保存时提示信息出现一次即可
                ec.Add(new Exception(sbError.ToString()));
            }
             
            return ec;
        }
        public virtual string CheckHasRankChangeData(string[] pEmpIds, DateTime pBeginDate, DateTime pEndDate)
        {

            string errorMessage = "";
            string emps = "'" + Guid.Empty.ToString() + "'";
            for (int i = 0; i < pEmpIds.Length; i++)
            {
                emps += ",'" + pEmpIds[i] + "'";
            }
            string sql = string.Format(@"
                            SELECT  employee.CnName , AttendanceRankChange.Date
                            FROM    AttendanceRankChange
                                    LEFT JOIN Employee ON AttendanceRankChange.EmployeeId = Employee.EmployeeId
                            WHERE   StateId = 'PlanState_002' AND '{0}' <= AttendanceRankChange.Date  AND '{1}' >= AttendanceRankChange.Date
                                    AND AttendanceRankChange.EmployeeId in ({2})", pBeginDate.ToString("yyyy-MM-dd"), pEndDate.ToString("yyyy-MM-dd"), emps);

            DataTable dt = HRHelper.ExecuteDataTable(sql);
          

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    errorMessage += dt.Rows[i][0].ToString() + " " + DateTime.Parse(dt.Rows[i][1].ToString()).ToString("yyyy-MM-dd") + " ,";
                }
                return errorMessage.TrimEnd(',') + "存在班次变更待审核数据";
            }
    
            return "";
        }


        /// <summary>
        /// 产生明细方法
        /// </summary>
        /// <param name="pLeave">请假实体</param>
        /// <param name="pBegin">开始时间</param>
        /// <param name="pEnd">结束时间</param>
        public virtual AttendanceLeave SetLeaveInfos(AttendanceLeave pLeave, DateTime pBegin, DateTime pEnd)
        {
          //  IAttendanceLeaveService leaveService = Factory.GetService<IAttendanceLeaveService>();
            AttendanceTypeService typeService = Factory.GetService<AttendanceTypeService>();
            List<AttendanceLeaveInfo> listLeaveInfo = GetLeaveInfos(pLeave.EmployeeId.ToString(), pLeave.AttendanceTypeId, pBegin, pEnd, pLeave.AttendanceLeaveId.GetString());
            foreach (AttendanceLeaveInfo leaveInfo in listLeaveInfo)
            {
                leaveInfo.Remark += pLeave.Remark;
                if (pLeave.AttendanceTypeId == "401")
                {
                    leaveInfo.FiscalYearId = pLeave.FiscalYearId;
                }
                pLeave.Infos.Add(leaveInfo);
            }

            //比较当前员工的请假时数是否大于最大带薪时数
            string msg = GetSalaryTimes(pLeave);
            if (msg.IndexOf("false") > -1)
            {
                string[] tmpMsg = msg.Split(',');
                //20140916 added by huangzj for 22233 A00-20140912004 假勤单位是天 需要报错报出天
              
                AttendanceType atType = typeService.GetAttendanceType(pLeave.AttendanceTypeId);
                if (atType.AttendanceUnitId.Equals("AttendanceUnit_001"))
                {
                    //假勤单位是天 需要报错报出天
                    throw new Exception(string.Format("工号为 {0} 的员工当前请假总天数为 {1}，已超过最大值 {2}", tmpMsg[0], tmpMsg[2], tmpMsg[1]));
                }
                throw new BusinessRuleException(string.Format("工号为 {0} 的员工当前请假总时数为 {1}，已超过最大值 {2}", tmpMsg[0], tmpMsg[2], tmpMsg[1]));
            }
            return pLeave;
        }


        /// <summary>
        /// 標準产生明细方法
        /// </summary>
        /// <param name="pLeave">请假实体</param>
        /// <param name="pBegin">开始时间</param>
        /// <param name="pEnd">结束时间</param>
        public virtual List<AttendanceLeaveInfo> GetLeaveInfos(string pEmpId, string pAttendanceTypeId, DateTime pBeginDateTime, DateTime pEndDateTime, string pMainId = "")
        {
            List<AttendanceLeaveInfo> listLeaveInfo = new List<AttendanceLeaveInfo>();
            // 获取日期段中日期的假日类型
            //IAttendanceLeaveService leaveService = Factory.GetService<IAttendanceLeaveService>();
            AttendanceTypeService typeService = Factory.GetService<AttendanceTypeService>();
            AttendanceLeaveInfo info = new AttendanceLeaveInfo();
            AttendanceEmpRankService empRankSer = Factory.GetService<AttendanceEmpRankService>();
            //IDocumentService<AttendanceEmployeeRank> empRankService = Factory.GetService<IAttendanceEmployeeRankService>();
            //IDocumentService<AttendanceRank> docRankSer = Factory.GetService<IAttendanceRankService>().GetServiceNoPower();
            DataTable empRankDt = empRankSer.GetEmpRanks(new string[] { pEmpId }, pBeginDateTime.AddDays(-1), pEndDateTime);
            Dictionary<string, AttendanceRank> dicRank = new Dictionary<string, AttendanceRank>();
            //跳过节日(1)
            bool isPassHoliday = false; //休假是否跳过節日
            bool isPassRest = false;  //休假是否跳过假日
            bool isPassBreakDay = false;//休假是否跳过休息日
            bool isDeductOhter = false;  //是否扣除班次以外（休息、就餐）时间 20100714改成是否扣除在岗时数
            bool isPassEmptyDay = false; //請假跳過空班
            decimal minLeaveHours = 1;  //最小核算量
            decimal minAuditHours = 1;  //最小审核量
            Dictionary<string, bool> dicIsBelongBefore = new Dictionary<string, bool>();  //是否归属前一天字典
            bool isBelongBefore = false;
            string calculateMode = string.Empty;  //进位方式
            int digits = 0;  //小数位数
            Dictionary<string, List<RankRestinfo>> ranksInfo2 = new Dictionary<string, List<RankRestinfo>>();
            DataTable dtLeaveConfig = typeService.GetLeaveConfig(pAttendanceTypeId);
             

            if (pBeginDateTime.CompareTo(pEndDateTime) == 0)
            { //相同时间
                return listLeaveInfo;
            }
             

            if (dtLeaveConfig != null && dtLeaveConfig.Rows.Count > 0)
            {
                isPassRest = dtLeaveConfig.Rows[0]["PassRest"].ToString().ToBool();
                isPassHoliday = dtLeaveConfig.Rows[0]["PassHoliday"].ToString().ToBool();
                isPassBreakDay = dtLeaveConfig.Rows[0]["PassBreakDay"].ToString().ToBool();
                isDeductOhter = dtLeaveConfig.Rows[0]["DeductOhter"].ToString().ToBool();
                isPassEmptyDay = dtLeaveConfig.Rows[0]["PassEmptyDay"].ToString().ToBool();
                 
                Decimal.TryParse(dtLeaveConfig.Rows[0][2].ToString(), out minLeaveHours);
                Decimal.TryParse(dtLeaveConfig.Rows[0][3].ToString(), out minAuditHours);
                calculateMode = dtLeaveConfig.Rows[0][5].ToString();
                int.TryParse(dtLeaveConfig.Rows[0][6].ToString(), out digits);
            }
            if (calculateMode.CheckNullOrEmpty())
                calculateMode = "CalculateMode_004";  //无
            if (digits == 0)
                digits = 2; //默认保留两位小数

            if (empRankDt != null && empRankDt.Rows.Count > 0)
            {
                string rankId = string.Empty;
                string atHolidayType = string.Empty;
                DateTime rankBegin = DateTime.MinValue;
                DateTime rankEnd = DateTime.MinValue;
                DateTime restBegin = DateTime.MinValue;
                DateTime restEnd = DateTime.MinValue;
                DateTime rankDate = DateTime.MinValue;//归属日期
                decimal workHours = 0; //工作时数
                foreach (DataRow var in empRankDt.Rows)
                {
                    rankId = var["AttendanceRankId"].ToString();
                    atHolidayType = var["AttendanceHolidayTypeId"].ToString();
                    workHours = 0;
                    decimal.TryParse(var["WorkHours"].ToString(), out workHours);

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
                     

                    // 处理班段相关
                    if (!ranksInfo2.ContainsKey(rankId))
                    {
                        AttendanceRankService rankSer = Factory.GetService<AttendanceRankService>();
                        DataTable rankRestDt = rankSer.GetRankRestInfo(rankId);
                        if (rankRestDt.Rows.Count > 0)
                        {
                            isBelongBefore = false;
                            Boolean.TryParse(rankRestDt.Rows[0]["IsBelongToBefore"].ToString(), out isBelongBefore);
                            if (!dicIsBelongBefore.ContainsKey(rankId))
                            {
                                dicIsBelongBefore.Add(rankId, isBelongBefore);
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
                                    throw new BusinessRuleException(string.Format("班次：{0} 班段存在错误", item["Name"].ToString()));
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
                            ranksInfo2.Add(rankId, tempList);
                        }
                        // 20081210 modified by zhonglei for 没有班次明细则抛出异常
                        else
                        {
                          
                            throw new BusinessRuleException(string.Format("班次编码为 {0} 的班次明细不能为空", rankSer.GetRankCodeById(rankId)));
                        }
                    }
                     

                    // 处理请假(班段请假改为整班次请假)

                    if (DateTime.TryParse(var["Date"].ToString(), out rankBegin))
                    {
                        rankDate = rankBegin;
                        if (DateTime.TryParse(rankBegin.ToDateFormatString() + " " + var["WorkBeginTime"].ToString(), out rankBegin) &&
                            DateTime.TryParse(rankBegin.ToDateFormatString() + " " + var["WorkEndTime"].ToString(), out rankEnd))
                        {
                            if (dicIsBelongBefore.ContainsKey(rankId))
                            {
                                isBelongBefore = dicIsBelongBefore[rankId];
                                if (isBelongBefore)
                                {
                                    rankBegin = rankBegin.AddDays(1);
                                }
                            }
                            if (rankEnd <= rankBegin)
                                rankEnd = rankEnd.AddDays(1);
                            List<DateTime> leaveTimeList = new List<DateTime>();
                            decimal leaveHours = 0;
                            //对于弹性班段的请假处理，备注上要添加说明
                            string strFlexRemark = string.Empty;
                            DateTime flexBegin = DateTime.MinValue; //彈性開始時間
                            DateTime flexEnd = DateTime.MinValue; //彈性結束時間
                            DateTime leaveBegin = DateTime.MinValue;
                            DateTime leaveEnd = DateTime.MinValue;
                            // 彈性處理
                            int beginCode = -1;
                            int endCode = -1;
                            AttendanceRankService rankser = new AttendanceRankService();
                            if (!dicRank.ContainsKey(rankId))
                            {
                                dicRank.Add(rankId, rankser.GetAttendanceRank(rankId));
                            }
                            AttendanceRank rank = dicRank[rankId];
                            PeriodDate period = null;
                          
                             
                            foreach (RankRestinfo item in ranksInfo2[rankId])
                            {
                                //调整为判断是否扣除在岗时数
                                if (isDeductOhter && (item.NotJobTime!=null && item.NotJobTime==true))
                                    continue;
                                // 20130819 added by jiangpeng for bug 13365 加班就餐段不参与计算
                                if (item.AttendanceRankType.Equals("AttendanceRankType_004"))
                                {
                                    continue;
                                }
                                if (DateTime.TryParse(rankBegin.ToDateFormatString() + " " + item.RestBegin, out restBegin) &&
                                    DateTime.TryParse(rankBegin.ToDateFormatString() + " " + item.RestEnd, out restEnd))
                                {
                                    restBegin = (restBegin < rankBegin) ? restBegin.AddDays(1) : restBegin;
                                    restEnd = (restEnd < restBegin) ? restEnd.AddDays(1) : restEnd;

                                    Rectangle result = GetIntersect(new DateTime[] { pBeginDateTime, pEndDateTime }, new DateTime[] { restBegin, restEnd });

                                    // 处理请假时数时加入弹性班次的判断
                                    //int beginCode = -1;
                                    //int endCode = -1;
                                    //PeriodDate period = Factory.GetService<IAttendanceRollcallService>().GetFlexBeginEndDate(rankId, pEmpId, rankBegin.Date, ref beginCode, ref endCode, pAttendanceTypeId);
                                    //20160505 edit by LinBJ for 35070~35074 重寫彈性邏輯
                                    bool isFlexFirstLeave = false;
                                    if (period != null && beginCode != -1 && endCode != -1)
                                    {
                                        if (item.RankCode == beginCode || item.RankCode == endCode)
                                        {
                                            if (pBeginDateTime.TimeOfDay != rankBegin.TimeOfDay || (period.BeginDate < rankBegin && rank.IsCollectFelx))
                                            {
                                                DateTime newRestBegin = restBegin;
                                                DateTime newRestEnd = restEnd;
                                                if (item.RankCode == beginCode)
                                                {
                                                    newRestBegin = period.BeginDate;
                                                }
                                                if (item.RankCode == endCode)
                                                {
                                                    newRestEnd = period.EndDate;
                                                }
                                                result = GetIntersect(new DateTime[] { pBeginDateTime, pEndDateTime }, new DateTime[] { newRestBegin, newRestEnd });//弹性班次的最后一个正常班段的结束时间
                                                strFlexRemark = string.Format("今天是弹性上班，弹性上班开始时间：【{0}】，弹性上班结束时间：【{1}】。", period.BeginDate.ToTimeFormatString(), period.EndDate.ToTimeFormatString());
                                                flexBegin = period.BeginDate;
                                                flexEnd = period.EndDate;
                                                // 20150706 LinBJ add by S00-20150609002 29817 对3段班(上午、休息、下午)，并且下午整班段请假时的班次时间平移逻辑进行调整
                                                List<RankRestinfo> restList = ranksInfo2[rankId].OrderBy(rest => rest.RankCode).ToList();
                                                //20150908 Added by liuyuana for 31871&V01-20150907001：班次上增加参数IsFlexFirstEnd来控制新的逻辑是否生效
                                                if (restList.Count() >= 3 && restList.Where(t => t.AttendanceRankType == "AttendanceRankType_001").Count() == 2 && rank.IsFlexFirstEnd)
                                                {
                                                    if (restList[1].AttendanceRankType != "AttendanceRankType_001"
                                                      && restList[0].AttendanceRankType == "AttendanceRankType_001"
                                                      && restList[2].AttendanceRankType == "AttendanceRankType_001")
                                                    {
                                                        if (pBeginDateTime.ToTimeFormatString() == restList[2].RestBegin && pEndDateTime.ToTimeFormatString() == restList[2].RestEnd && restList[2].RankCode == endCode)
                                                        {
                                                            double min = (period.BeginDate - rankBegin).TotalMinutes;
                                                            strFlexRemark = string.Format("今天是弹性上班，弹性上班开始时间：【{0}】，弹性上班结束时间：【{1}】。", period.BeginDate.ToTimeFormatString(),
                                                               rankBegin.AddHours((double)(restList[0].RestHours)).AddMinutes(min).ToTimeFormatString());
                                                            isFlexFirstLeave = true;
                                                            flexBegin = period.BeginDate;
                                                            flexEnd = rankBegin.AddHours((double)(restList[0].RestHours)).AddMinutes(min);
                                                            result = GetIntersect(new DateTime[] { pBeginDateTime, pEndDateTime }, new DateTime[] { restBegin, restEnd });
                                                        }
                                                    }
                                                }
                                                 
                                                restBegin = newRestBegin;
                                                restEnd = newRestEnd;
                                            }
                                        }
                                    }
                                     

                                    if (result.IsEmpty || result.Left == result.Right)
                                    {//无需请假
                                    }
                                    else
                                    {//请假时间区间
                                        //  明细处理
                                        DateTime leaveBegin2 = DateTime.MinValue.AddMinutes(result.Left);
                                        DateTime leaveEnd2 = DateTime.MinValue.AddMinutes(result.Right);
                                        leaveTimeList.Add(leaveBegin2);
                                        leaveTimeList.Add(leaveEnd2);

                                        //需要判断是否满班次
                                        //20141024 edit by LinBJ for 23205 C01-20141023013 彈性班不適用滿班次邏輯
                                        if (leaveBegin2.CompareTo(restBegin) == 0 && leaveEnd2.CompareTo(restEnd) == 0 && strFlexRemark.CheckNullOrEmpty())
                                        {//满班次
                                            leaveHours += (decimal)item.RestHours;
                                        }
                                        else
                                        {
                                            if (isFlexFirstLeave)
                                            {
                                                //20210908 add by LinBJ for A00-20210831001 彈性下午班段全請需抓班次時數
                                                leaveHours += (decimal)item.RestHours;
                                            }
                                            else
                                            {
                                                leaveHours += CalculateHours(leaveBegin2, leaveEnd2);
                                            }
                                        }
                                         
                                    }
                                }
                            }
                            // 20110105 added by jianpeng for 合并请假记录
                            if (leaveHours > 0)
                            {
                                //设置请假的班次时间
                                leaveTimeList.Sort();
                                leaveBegin = leaveTimeList[0];
                                leaveEnd = leaveTimeList[leaveTimeList.Count - 1];
                                info = new AttendanceLeaveInfo();
                                info.AttendanceRankId = rankId;
                                info.AttendanceTypeId = pAttendanceTypeId;
                                info.EmployeeId = pEmpId.GetGuid();
                                info.EmployeeRankId = var["AttendanceEmployeeRankId"].ToString().GetGuid();
                                info.BeginDate = leaveBegin.Date;
                                info.EndDate = leaveEnd.Date;
                                info.BeginTime = leaveBegin.ToString(Constants.FORMAT_SHORTTIMEPATTERN);
                                info.EndTime = leaveEnd.ToString(Constants.FORMAT_SHORTTIMEPATTERN);
                                info.Date = rankDate.Date;
                                info.Hours = leaveHours;
                                // 折算算法（与特休相同）
                                decimal hours = GetAmountHours(minLeaveHours, minAuditHours, leaveHours);
                                info.Hours = Math.Round(hours, 4, MidpointRounding.AwayFromZero);
                                 
                                // 年假資訊
                                if (pAttendanceTypeId == "401")
                                {
                                   EmployeeService employeeService = new EmployeeService();
                                    string corporationId = employeeService.GetEmpFiledById(pEmpId,"CorporationId").ToString();
                                    //IAnnualLeaveParameterService parameterServiceEx = Factory.GetService<IAnnualLeaveParameterService>();
                                    //AnnualLeaveParameter parameter = parameterServiceEx.GetParameterIdByCorporationId(corporationId);
                                    ALPlanService planService = new ALPlanService();
                                    string parameterUnit = planService.GetParameterUnit(corporationId);
                                    info.AnnualLeaveUnit = parameterUnit;
                                    //info.FiscalYearId = pLeave.FiscalYearId;
                                    if (!info.AnnualLeaveUnit.Equals("AnnualLeaveUnit_003"))
                                    {
                                        info.Days = CalculateDays(workHours, info.AnnualLeaveUnit, leaveBegin, leaveEnd, info.Hours);
                                    }
                                }
                                else
                                {
                                    info.Days = CalcuteHours(calculateMode, 2, info.Hours, workHours, true);//小时进位转换为天
                                }
                                 

                                //对于弹性班段的请假处理，备注上要添加说明 
                                //20141125 LinBJ Add by Q00-20141103010 24390 24391 
                                //若有弹性再加判请假明细是否有请假开始时间!＝班次开始时间的数据，则该天班次有弹性，请假明细备注中追加写入相关弹性信息
                                if (!strFlexRemark.CheckNullOrEmpty() && (info.BeginTime != rank.WorkBeginTime || (period != null && period.BeginDate < rankBegin && rank.IsCollectFelx)))
                                {
                                    info.Remark = strFlexRemark;
                                }

                                listLeaveInfo.Add(info);//加入明细
                            }
                             
                        }
                    }
                     
                }
            }
             
            return listLeaveInfo;
        }

        //小时转化为天/天转化为小时
        protected decimal CalcuteHours(string pAddMode, int pDigits, decimal pLeaveHours, decimal pWorkHours, bool isChangeDays)
        {
            decimal hours = 0;
            if (isChangeDays)
                hours = pLeaveHours / pWorkHours;
            else
                hours = pLeaveHours * pWorkHours;
            decimal tempDec = Convert.ToDecimal(Math.Pow(10, pDigits));
            switch (pAddMode)
            {
                case "CalculateMode_001":
                    //四舍五入
                    hours = this.MyRound(hours, pDigits);
                    break;
                case "CalculateMode_002":
                    //无条件舍
                    hours = Math.Floor(hours * tempDec) / tempDec;
                    break;
                case "CalculateMode_003":
                    //无条件入
                    hours = Math.Ceiling(hours * tempDec) / tempDec;
                    break;
                default:
                    break;
            }
            return hours;
        }

        /// <summary>
        /// 四舍五入
        /// </summary> 
        protected decimal MyRound(decimal value, int digit)
        {
            try
            {
                decimal vt = Convert.ToDecimal(Math.Pow(10, digit));
                decimal vx = Convert.ToDecimal(value * vt);

                vx += 0.5M;
                return (Math.Floor(vx) / vt);
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException("Formula_C1Round" + Constants.ENTER_SIGN + ex.Message);
            }
        }

        /// <summary>
        /// 根据请假最小核算量和最小审核量设置请假时数
        /// </summary>
        /// <param name="pMinAmount">最小核算量</param>
        /// <param name="pMinAuditAmount">最小审核量</param>
        /// <param name="pHours">请假时数</param>
        /// <returns></returns>
        protected decimal GetAmountHours(decimal pMinAmount, decimal pMinAuditAmount, decimal pHours)
        {
            //20150224 add by LinBJ 26711 26712 A00-20150225001 BY 小數進位差值問題
            pHours = Decimal.Round(pHours, 8);
            decimal resultAmount = 0M;
            if (pMinAmount == 0M)
            {
                if (pMinAuditAmount == 0M)
                {
                    resultAmount = pHours;
                }
                else
                {
                    //取整改为无条件入
                    resultAmount = Math.Ceiling(pHours / pMinAuditAmount) * pMinAuditAmount;
                }
            }
            else
            {
                if (pMinAuditAmount == 0M)
                {
                    if (pHours <= pMinAmount)
                    {
                        resultAmount = pMinAmount;
                    }
                    else
                    {
                        resultAmount = pHours;
                    }
                }
                else
                {
                    if (pHours <= pMinAmount)
                    {
                        resultAmount = pMinAmount;
                    }
                    else
                    {
                        //取整改为无条件入
                        resultAmount = Math.Ceiling((pHours - pMinAmount) / pMinAuditAmount) * pMinAuditAmount + pMinAmount;
                    }
                }
            }
            return resultAmount;
        }

        //计算天数
        protected decimal CalculateDays(decimal pWorkHours, string pAlUnit, DateTime pBegin, DateTime pEnd, decimal pRestHour)
        {
            decimal i = pRestHour;
            // 按天核算
            switch (pAlUnit)
            {
                case "AnnualLeaveUnit_001"://0.5天
                    i = Math.Round(i, 1);
                    if (i <= pWorkHours / 2)
                    {
                        i = 0.5m;
                    }
                    else
                    {
                        i = 1m;
                    }
                    break;
                case "AnnualLeaveUnit_002"://1天
                    i = 1m;
                    break;
                case "AnnualLeaveUnit_003"://1小时
                    break;
                default:
                    break;
            }
            return i;
        }

        /// <summary>
        /// 比较当前员工的请假时数是否大于最大带薪时数
        /// </summary>
        /// <param name="pAttendanceLeave"></param>
        /// <returns></returns>
        public virtual string GetSalaryTimes(AttendanceLeave pAttendanceLeave)
        {

            //20140924　added by huangzj for 当请假是休息班时会报错，此修改没有单子
            if (pAttendanceLeave.Infos.Count == 0)
                return "";

            string msg = string.Empty;
            //IDocumentService<AttendanceType> typeService = Factory.GetService<IAttendanceTypeService>().GetServiceNoPower();
            EmployeeService employeeService = new EmployeeService();
            string attendanceTypeSql = string.Format("select * from AttendanceType where AttendanceTypeId='{0}'", pAttendanceLeave.AttendanceTypeId);
            DataTable attendanceTypeTable = HRHelper.ExecuteDataTable(attendanceTypeSql);
            if (attendanceTypeTable == null || attendanceTypeTable.Rows.Count == 0)
            {
                //是否抛出异常
            }
            string attendanceKindId = attendanceTypeTable.Rows[0]["AttendanceKindId"] + "";
            int maxSalaryHour = int.Parse(attendanceTypeTable.Rows[0]["MaxSalaryHour"] + "");
            string salaryPeriodId = attendanceTypeTable.Rows[0]["SalaryPeriodId"] + "";
            string daysCoverATType = attendanceTypeTable.Rows[0]["DaysCoverATType"] + "";
            string employeeName = employeeService.GetEmployeeNameById(pAttendanceLeave.EmployeeId.ToString());
            string attendanceUnitId = attendanceTypeTable.Rows[0]["AttendanceUnitId"] + "";
            bool passRest = bool.Parse(attendanceTypeTable.Rows[0]["PassRest"] + "");
            bool passHoliday = bool.Parse(attendanceTypeTable.Rows[0]["PassHoliday"] + "");
            bool isOnlyForOneDay = bool.Parse(attendanceTypeTable.Rows[0]["IsOnlyForOneDay"] + "");
            string mergeATType = attendanceTypeTable.Rows[0]["MergeATType"] + "";
            string name = attendanceTypeTable.Rows[0]["Name"] + "";
            //AttendanceType atType = typeService.Read(pAttendanceLeave.AttendanceTypeId);
            //假勤项目：请假（AttendanceKind_004）
            if (attendanceKindId.Equals("AttendanceKind_004") && maxSalaryHour > 0)
            {


                string beginDate = string.Empty;
                string endDate = string.Empty;
                if (!salaryPeriodId.CheckNullOrEmpty())
                {//计算周期
                    if (salaryPeriodId.Equals("SalaryPeriod_001"))
                    {
                        //年
                        beginDate = string.Format("{0}-1-1 0:00:00", pAttendanceLeave.BeginDate.Year);
                        endDate = (DateTime.Parse(beginDate).AddYears(1).AddSeconds(-1)).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        //月
                        beginDate = string.Format("{0}-{1}-1 0:00:00", pAttendanceLeave.BeginDate.Year, pAttendanceLeave.BeginDate.Month);
                        endDate = (DateTime.Parse(beginDate).AddMonths(1).AddSeconds(-1)).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    // 考虑“合并假勤类型”
                    StringBuilder sbTypeIds = new StringBuilder();
                    sbTypeIds.AppendFormat("'{0}'", pAttendanceLeave.AttendanceTypeId);//添加本身的假勤类型
                    if (!daysCoverATType.CheckNullOrEmpty())
                    {//合并假勤类型
                        string[] arrayTypeIds = daysCoverATType.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrayTypeIds.Length > 0)
                        {
                            foreach (string id in arrayTypeIds)
                            {
                                if (!id.Equals(pAttendanceLeave.AttendanceTypeId))
                                {//排除本身的假勤类型
                                    sbTypeIds.AppendFormat(",'{0}'", id);
                                }
                            }
                        }
                    }
                    //排除销假和审核不同意的请假
                    string strSQL = string.Format(@"SELECT SUM(Info.Hours) as Hours,SUM(Info.Days) as Days
                                                        FROM AttendanceLeaveInfo Info 
                                                        INNER JOIN AttendanceLeave Leave ON Leave.AttendanceLeaveId = Info.AttendanceLeaveId 
                                                        INNER JOIN AttendanceType AtType ON AtType.AttendanceTypeId = Leave.AttendanceTypeId 
                                                        WHERE Leave.EmployeeId = '{0}' AND Leave.AttendanceTypeId IN ({1}) AND Info.IsRevoke = 0 AND 
                                                              Leave.AttendanceLeaveId NOT IN (
                                                              SELECT AttendanceLeaveId 
                                                              FROM AttendanceLeave 
                                                              WHERE StateId = 'PlanState_003' AND ApproveResultId = 'OperatorResult_002') AND 
                                                              Leave.BeginDate BETWEEN '{2}' AND '{3}' AND Leave.Flag = 1",
                          pAttendanceLeave.EmployeeId.GetString(), sbTypeIds.ToString(), beginDate, endDate);
                     
                    if (!pAttendanceLeave.AttendanceLeaveId.CheckNullOrEmpty())
                    {
                        strSQL += string.Format(" AND Leave.AttendanceLeaveId <> '{0}'", pAttendanceLeave.AttendanceLeaveId.GetString());
                    }


                    DataTable dt = HRHelper.ExecuteDataTable(strSQL);

                    //20140916 modified by huangzj for 22233 A00-20140912004 请假天数用班次时数折算，不再用8小时
                    //按照假勤单位转换最大可请假时数
                    decimal salaryHour = maxSalaryHour;//最大允许请假时数
                    decimal totalHours = 0m; //请假总时数
                    AttendanceEmpRankService srvEmpRank = new AttendanceEmpRankService();

                    if (attendanceUnitId.Equals("AttendanceUnit_001"))
                    {//天 
                        // 单位为天
                        //当前请假时数
                        for (int i = 0; i < pAttendanceLeave.Infos.Count; i++)
                        {
                            //if (!pAttendanceLeave.Infos[i].IsRevoke )//归属日期判断
                            //    totalHours += pAttendanceLeave.Infos[i].Days;

                            if (!pAttendanceLeave.Infos[i].IsRevoke)
                            {
                                var dtEmpRank = srvEmpRank.GetEmpRanks(new string[] { pAttendanceLeave.EmployeeId.GetString() }, pAttendanceLeave.Infos[i].Date, pAttendanceLeave.Infos[i].Date);
                                //判断是否跳过假日或休息日
                                if (passRest)
                                {
                                    if (!pAttendanceLeave.Infos[i].AttendanceRankId.CheckNullOrEmpty())
                                    {
                                        if (dtEmpRank != null && dtEmpRank.Rows.Count > 0)
                                        {
                                            var holidayTypeId = dtEmpRank.Rows[0]["AttendanceHolidayTypeId"].ToString();
                                            if (holidayTypeId.Equals("DefaultHolidayType004") || holidayTypeId.Equals("DefaultHolidayType005"))
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }

                                //判断是否跳过节日
                                if (passHoliday)
                                {
                                    if (!pAttendanceLeave.Infos[i].AttendanceRankId.CheckNullOrEmpty())
                                    {
                                        if (dtEmpRank != null && dtEmpRank.Rows.Count > 0)
                                        {
                                            var holidayTypeId = dtEmpRank.Rows[0]["AttendanceHolidayTypeId"].ToString();
                                            if (holidayTypeId.Equals("DefaultHolidayType002") || holidayTypeId.Equals("DefaultHolidayType003"))
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }

                                totalHours += pAttendanceLeave.Infos[i].Days;
                            }
                        }

                        //请假总时数
                        decimal obj = 0m;
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            decimal.TryParse(dt.Rows[0]["Days"].ToString(), out obj);
                            totalHours += obj;
                        }
                         
                    }
                    else
                    {
                        // 单位为小时或分钟
                        //当前请假时数
                        for (int i = 0; i < pAttendanceLeave.Infos.Count; i++)
                        {
                            if (!pAttendanceLeave.Infos[i].IsRevoke)
                            {
                                var dtEmpRank = srvEmpRank.GetEmpRanks(new string[] { pAttendanceLeave.EmployeeId.GetString() }, pAttendanceLeave.Infos[i].Date, pAttendanceLeave.Infos[i].Date);
                                //判断是否跳过假日或休息日
                                if (passRest)
                                {
                                    if (!pAttendanceLeave.Infos[i].AttendanceRankId.CheckNullOrEmpty())
                                    {
                                        if (dtEmpRank != null && dtEmpRank.Rows.Count > 0)
                                        {
                                            var holidayTypeId = dtEmpRank.Rows[0]["AttendanceHolidayTypeId"].ToString();
                                            if (holidayTypeId.Equals("DefaultHolidayType004") || holidayTypeId.Equals("DefaultHolidayType005"))
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }

                                //判断是否跳过节日
                                if (passHoliday)
                                {
                                    if (!pAttendanceLeave.Infos[i].AttendanceRankId.CheckNullOrEmpty())
                                    {
                                        if (dtEmpRank != null && dtEmpRank.Rows.Count > 0)
                                        {
                                            var holidayTypeId = dtEmpRank.Rows[0]["AttendanceHolidayTypeId"].ToString();
                                            if (holidayTypeId.Equals("DefaultHolidayType002") || holidayTypeId.Equals("DefaultHolidayType003"))
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }


                                totalHours += pAttendanceLeave.Infos[i].Hours;
                            }
                        }
                        //请假总时数
                        decimal obj = 0m;
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            decimal.TryParse(dt.Rows[0]["Hours"].ToString(), out obj);
                            totalHours += obj;
                        }

                        if (attendanceUnitId.Equals("AttendanceUnit_003"))
                        {
                            //分钟 
                            salaryHour = salaryHour / 60;
                        }
                         
            }

                    if (totalHours > salaryHour)
                    {
                        string employeeCode = employeeService.GetEmpFiledById(pAttendanceLeave.EmployeeId.GetString(), "EmployeeCode");

                        msg = string.Format("{0},{1},{2},false", employeeCode, salaryHour.ToString("#.##"), totalHours.ToString("#.##"));
                    }
                }

            }

            // added by jiangpeng for 任务6979 增加校验：此假勤类型为另外的假勤类型的合并假勤类型
            if (msg.CheckNullOrEmpty() && daysCoverATType.CheckNullOrEmpty())
            {
                string strSql = string.Format(@"SELECT AttendanceTypeId,
                                                           SalaryPeriodId,
                                                           MaxSalaryHour,
                                                           DaysCoverAtType,
                                                           AttendanceUnitId
                                                    FROM   AttendanceType
                                                    WHERE  DaysCoverAtType LIKE '{0};%' or DaysCoverATType LIKE '%;{0};%'", pAttendanceLeave.AttendanceTypeId);
                DataTable dtType = HRHelper.ExecuteDataTable(strSql);
                if (dtType != null && dtType.Rows.Count > 0)
                {
                    decimal maxSalaryHour_1 = 0;
                    string salaryPeriodId_1 = string.Empty;
                    string attendanceTypeId = string.Empty;
                    string daysCoverATType_1 = string.Empty;
                    string attendanceUnitId_1 = string.Empty;
                    string strSalaryHour = dtType.Rows[0]["maxSalaryHour_1"].ToString();
                    salaryPeriodId_1 = dtType.Rows[0]["salaryPeriodId_1"] as string;
                    attendanceTypeId = dtType.Rows[0]["AttendanceTypeId"] as string;
                    daysCoverATType_1 = dtType.Rows[0]["daysCoverATType_1"] as string;
                    attendanceUnitId_1 = dtType.Rows[0]["attendanceUnitId_1"] as string;
                    decimal.TryParse(strSalaryHour, out maxSalaryHour_1);
                    if (maxSalaryHour_1 > 0 && !salaryPeriodId_1.CheckNullOrEmpty() && !attendanceTypeId.CheckNullOrEmpty())
                    {
                        string beginDate = string.Empty;
                        string endDate = string.Empty;
                        if (!salaryPeriodId_1.CheckNullOrEmpty())
                        {//计算周期
                            if (salaryPeriodId_1.Equals("SalaryPeriod_001"))
                            {
                                //年
                                beginDate = string.Format("{0}-1-1 0:00:00", pAttendanceLeave.BeginDate.Year);
                                endDate = (DateTime.Parse(beginDate).AddYears(1).AddSeconds(-1)).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                //月
                                beginDate = string.Format("{0}-{1}-1 0:00:00", pAttendanceLeave.BeginDate.Year, pAttendanceLeave.BeginDate.Month);
                                endDate = (DateTime.Parse(beginDate).AddMonths(1).AddSeconds(-1)).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            // 考虑“合并假勤类型”

                            StringBuilder sbTypeIds = new StringBuilder();
                            sbTypeIds.AppendFormat("'{0}'", attendanceTypeId);//添加本身的假勤类型
                            if (!daysCoverATType_1.CheckNullOrEmpty())
                            {//合并假勤类型
                                string[] arrayTypeIds = daysCoverATType_1.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                                if (arrayTypeIds.Length > 0)
                                {
                                    foreach (string id in arrayTypeIds)
                                    {
                                        if (!id.Equals(attendanceTypeId))
                                        {//排除本身的假勤类型
                                            sbTypeIds.AppendFormat(",'{0}'", id);
                                        }
                                    }
                                }
                            }
                            //排除销假和审核不同意的请假
                            string strSQL = string.Format(@"SELECT SUM(Info.Hours) as Hours ,SUM(Info.Days) as Days
                                                        FROM AttendanceLeaveInfo Info 
                                                        INNER JOIN AttendanceLeave Leave ON Leave.AttendanceLeaveId = Info.AttendanceLeaveId 
                                                        INNER JOIN AttendanceType AtType ON AtType.AttendanceTypeId = Leave.AttendanceTypeId 
                                                        WHERE Leave.EmployeeId = '{0}' AND Leave.AttendanceTypeId IN ({1}) AND Info.IsRevoke = 0 AND 
                                                              Leave.AttendanceLeaveId NOT IN (
                                                              SELECT AttendanceLeaveId 
                                                              FROM AttendanceLeave 
                                                              WHERE StateId = 'PlanState_003' AND ApproveResultId = 'OperatorResult_002') AND 
                                                              Leave.BeginDate BETWEEN '{2}' AND '{3}' AND Leave.Flag = 1",
                                  pAttendanceLeave.EmployeeId.GetString(), sbTypeIds.ToString(), beginDate, endDate);
                             
                            if (!pAttendanceLeave.AttendanceLeaveId.CheckNullOrEmpty())
                            {
                                strSQL += string.Format(" AND Leave.AttendanceLeaveId <> '{0}'", pAttendanceLeave.AttendanceLeaveId.GetString());
                            }


                            //object obj = cmd.ExecuteScalar();
                            DataTable dt = HRHelper.ExecuteDataTable(strSQL);

                            //20140916 modified by huangzj for 22233 A00-20140912004 请假天数用班次时数折算，不再用8小时
                            //按照假勤单位转换最大可请假时数
                            decimal salaryHour = maxSalaryHour_1;//最大允许请假时数
                            decimal totalHours = 0m; //请假总时数
                            if (attendanceUnitId_1.Equals("AttendanceUnit_001"))
                            {//天 
                                // 单位为天
                                //当前请假时数
                                for (int i = 0; i < pAttendanceLeave.Infos.Count; i++)
                                {
                                    if (!pAttendanceLeave.Infos[i].IsRevoke)
                                        totalHours += pAttendanceLeave.Infos[i].Days;
                                }

                                //请假总时数
                                decimal obj = 0m;
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    decimal.TryParse(dt.Rows[0]["Days"].ToString(), out obj);
                                    totalHours += obj;
                                }
                                 
                            }
                            else
                            {
                                // 单位为小时或分钟
                                //当前请假时数
                                for (int i = 0; i < pAttendanceLeave.Infos.Count; i++)
                                {
                                    if (!pAttendanceLeave.Infos[i].IsRevoke)
                                        totalHours += pAttendanceLeave.Infos[i].Hours;
                                }
                                //请假总时数
                                decimal obj = 0m;
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    decimal.TryParse(dt.Rows[0]["Hours"].ToString(), out obj);
                                    totalHours += obj;
                                }

                                if (attendanceUnitId_1.Equals("AttendanceUnit_003"))
                                {
                                    //分钟 
                                    salaryHour = salaryHour / 60;
                                }
                                 
                            }

                            if (totalHours > salaryHour)
                            {
                                string employeeCode = employeeService.GetEmpFiledById(pAttendanceLeave.EmployeeId.GetString(), "EmployeeCode");
                                msg = string.Format("{0},{1},{2},false", employeeCode, salaryHour.ToString("#.##"), totalHours.ToString("#.##"));
                            }
                        }
                    }
                }
            }
             
            // 20201210 add by LinBJ for S00-20201112001 新增勾選項,以控管此假勤僅能當月單日請完
            if (msg.CheckNullOrEmpty() && salaryPeriodId == "SalaryPeriod_002" && isOnlyForOneDay)
            {
                string empCode = string.Empty;
                List<PeriodDate> periodDateList = this.GetPeriodDate(pAttendanceLeave, salaryPeriodId, ref empCode);
                List<string> atList = new List<string>(new string[] { pAttendanceLeave.AttendanceTypeId });
                StringBuilder sb = new StringBuilder();
                atList.AddRange(mergeATType.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                foreach (var item in periodDateList)
                {
                    var thisMonthLeave = pAttendanceLeave.Infos.Where(t => t.Date.IsBetween(item.BeginDate, item.EndDate));
                    if (thisMonthLeave.Count() > 1)
                    {
                        string errorMsg = string.Format("员工{0} {1} 每月仅可请一次"
                        , employeeName, name);



                        if (!sb.ToString().Contains(errorMsg))
                        {
                            sb.AppendLine(errorMsg);
                        }
                    }
                    DataTable dt = GetMergeATTypeLeave(pAttendanceLeave.EmployeeId.ToString(), item.BeginDate, item.EndDate, atList);
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr["AttendanceLeaveId"].ToString().GetGuid() != pAttendanceLeave.AttendanceLeaveId)
                        {
                            DateTime oldDate = dr["Date"].ToString().ToDateTime();
                            var oldData = pAttendanceLeave.Infos.Where(t => t.Date == oldDate);
                            if (!oldData.Any())
                            {
                                string errorMsg = string.Format("员工{0} {1} 已有{2}资料，无法请假", dr["CnName"].ToString()
                                               , oldDate.ToDateFormatString(), dr["Name"].ToString());
                                if (!sb.ToString().Contains(errorMsg))
                                {
                                    sb.AppendLine(errorMsg);
                                }
                            }
                        }
                    }
                }
                if (sb.Length > 0)
                {
                    throw new BusinessRuleException(sb.ToString());
                }
            }
             
            return msg;

        }
        /// <summary>
        /// 取得合併統計的請假
        /// </summary>
        /// <param name="pEmpId"></param>
        /// <param name="pBDate"></param>
        /// <param name="pEnd"></param>
        /// <param name="pListATType"></param>
        /// <returns></returns>
        protected DataTable GetMergeATTypeLeave(string pEmpId, DateTime pBDate, DateTime pEnd, List<string> pListATType)
        {
            string sql = string.Format(@"select main.AttendanceLeaveId,info.EmployeeId,emp.CnName,info.AttendanceTypeId,atType.Name,info.Date from AttendanceLeaveInfo info
                                        left join AttendanceLeave main on main.AttendanceLeaveId=info.AttendanceLeaveId
                                        left join Employee emp on emp.EmployeeId=info.EmployeeId
                                        left join AttendanceType atType on atType.AttendanceTypeId=info.AttendanceTypeId
                                        where isnull(main.ApproveResultId,'')!='OperatorResult_002' and info.IsRevoke=0
                                        and info.EmployeeId='{0}' and info.Date between '{1}' and '{2}'
                                        and info.AttendanceTypeId in ({3})", pEmpId, pBDate.ToDateFormatString(), pEnd.ToDateFormatString(), HRHelper.GetArrayToStrBySQL(pListATType));
            return HRHelper.ExecuteDataTable(sql);
        }

        //获取相交区间
        protected Rectangle GetIntersect(DateTime[] pLeave, DateTime[] pWork)
        {
            DateTime st = DateTime.MinValue;
            Rectangle r1 = Rectangle.FromLTRB(GetTotalMin(pLeave[0], st), 0, GetTotalMin(pLeave[1], st), 0);
            Rectangle r2 = Rectangle.FromLTRB(GetTotalMin(pWork[0], st), 0, GetTotalMin(pWork[1], st), 0);
            Rectangle result = Rectangle.Intersect(r1, r2);

            return result;
        }
        protected int GetTotalMin(DateTime pValue, DateTime pST)
        {
            TimeSpan ts = pValue.Subtract(pST);

            return (int)ts.TotalMinutes;
        }

        protected decimal CalculateHours(DateTime pBegin, DateTime pEnd)
        {
            TimeSpan ts = pEnd - pBegin;
            double i = ts.TotalHours;

            return Convert.ToDecimal(i);
        }
        /// <summary>
        /// 获取请假的统计区间
        /// </summary>
        /// <param name="pAttendanceLeave">当前请假信息</param>
        /// <param name="pSalaryPeriodId">请假的计算周期ID</param>
        /// <param name="pEmpCode">顺便返回员工工号</param>
        /// <returns>请假校验区间</returns>
        protected List<PeriodDate> GetPeriodDate(AttendanceLeave pAttendanceLeave, string pSalaryPeriodId, ref string pEmpCode)
        {
            if (pSalaryPeriodId.CheckNullOrEmpty())
            {
                return new List<PeriodDate>();
            }
            //获取员工所属公司ID
            EmployeeService empService = Factory.GetService<EmployeeService>();
            DataTable dtEmpInfo = empService.GetEmployeeInfoByIds(new string[] { pAttendanceLeave.EmployeeId.GetString() });
            string corporationId = string.Empty;
            string empCode = string.Empty;
            if (dtEmpInfo != null && dtEmpInfo.Rows.Count > 0)
            {
                corporationId = dtEmpInfo.Rows[0]["CorporationId"].ToString();
                pEmpCode = dtEmpInfo.Rows[0]["Code"].ToString();
            }
            //统计区间
            List<PeriodDate> periodDateList = new List<PeriodDate>();
            //查找参数：先找所属公司，没有，则查找根公司
           // IHRParaConfigService paraConfigService = Factory.GetService<IHRParaConfigService>();
            DataTable dtParaConfig =HRHelper.ExecuteDataTable(string.Format("select * from HRParaConfig where HRParaConfigId='{0}'",Constants.Para_LeavePeriod + corporationId));
            if (dtParaConfig == null || dtParaConfig.Rows.Count == 0)
            {
                if (!corporationId.GetGuid().Equals(Constants.SYSTEMGUID_CORPORATION_ROOT.GetGuid()))
                {//不是根公司，才去找根公司
                    dtParaConfig = HRHelper.ExecuteDataTable(string.Format("select * from HRParaConfig where HRParaConfigId='{0}'",Constants.Para_LeavePeriod + Constants.SYSTEMGUID_CORPORATION_ROOT));
                }
            }
            if (dtParaConfig != null && dtParaConfig.Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(dtParaConfig.Rows[0]["Content"].ToString()))
                {
                    if (dtParaConfig.Rows[0]["Content"].ToString().Equals("LeavePeriod_001"))
                    {//考勤期间
                        List<DateTime> dateList = new List<DateTime>();
                        foreach (AttendanceLeaveInfo info in pAttendanceLeave.Infos)
                        {
                            if (!dateList.Contains(info.Date))
                            {
                                dateList.Add(info.Date);
                            }
                        }
                        dateList.Sort();
                        ATMonthService monthService = Factory.GetService<ATMonthService>();
                        if (pSalaryPeriodId.Equals("SalaryPeriod_001"))
                        {//年
                            periodDateList = monthService.GetBeginEndDate(corporationId, dateList[0], dateList[dateList.Count - 1], true);
                        }
                        else
                        {//月
                            periodDateList = monthService.GetBeginEndDate(corporationId, dateList[0], dateList[dateList.Count - 1], false);
                        }
                    }
                }
            }


            if (periodDateList.Count == 0)
            {//自然月
                List<DateTime> dateList = new List<DateTime>();
                foreach (AttendanceLeaveInfo info in pAttendanceLeave.Infos)
                {//以明细的归属日期为准（之所以循环，是因为可能出现跨月的情况）
                    if (!dateList.Contains(info.Date))
                    {
                        dateList.Add(info.Date);

                        DateTime beginDate = DateTime.MinValue;
                        DateTime endDate = DateTime.MinValue;
                        if (pSalaryPeriodId.Equals("SalaryPeriod_001"))
                        {//年
                            beginDate = DateTime.Parse(string.Format("{0}-1-1", info.Date.Year));
                            endDate = beginDate.AddYears(1).AddDays(-1);
                        }
                        else
                        {//月
                            beginDate = DateTime.Parse(string.Format("{0}-{1}-1", info.Date.Year, info.Date.Month));
                            endDate = beginDate.AddMonths(1).AddDays(-1);
                        }
                        PeriodDate pD= new PeriodDate();
                        pD.BeginDate = beginDate;
                        pD.EndDate=endDate;
                        periodDateList.Add(pD);
                    }
                }
            }
            return periodDateList;
        }

        /// <summary>
        /// 檢查參數值
        /// </summary>
        /// <param name="pFormEntity">實體</param>
        private void CheckValue(AttendanceLeaveForAPI pFormEntity)
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
        public virtual Dictionary<int, string> CheckBusinessApplyTime(AttendanceLeaveForAPI[] formEntities)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            CheckEntityType entityType = new CheckEntityType();
            bool isUltimate =true;

            foreach (AttendanceLeaveForAPI apiEntity in formEntities)
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

                AttendanceLeaveForAPI[] arrayEntity = new AttendanceLeaveForAPI[] { apiEntity };

                // ESS表單上的檢查
                Dictionary<int, string> dicCheck = new Dictionary<int, string>();
                //檢查請假時間與出差申請時間是否重複
                dicCheck = this.CheckBusinessApplyTime(arrayEntity);

                if (dicCheck == null && dicCheck.Count== 0)
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
        /// API實體轉為請假實體(AttendanceLeaveForAPI -> AttendanceLeave)
        /// </summary>
        /// <param name="pFormEntities">AttendanceLeaveForAPI實體</param>
        /// <param name="pIsSave">是否為存檔</param>
        /// <param name="pFormType">單別</param>
        /// <param name="pFormNumber">單號</param>
        /// <returns>AttendanceLeave實體</returns>
        private List<AttendanceLeave> ChangeToLeaveEntity(AttendanceLeaveForAPI[] pFormEntities, bool pIsSaveOrCheck, string pFormType, string pFormNumber)
        {
            List<AttendanceLeave> list = new List<AttendanceLeave>();
           // IAnnualLeaveRegisterService alRegisterServ = Factory.GetService<IAnnualLeaveRegisterService>();
            EmployeeService empServ = new EmployeeService ();
           AttendanceEmpRankService  empRankServ =new AttendanceEmpRankService ();
            bool isCir = false;// HRHelper.IsCirculationIndustry;
            bool isUltimate = true;

            if (pFormType.CheckNullOrEmpty() || pFormNumber.CheckNullOrEmpty())
            {
                list = HRHelper.WebAPIEntitysToDataEntitys<AttendanceLeave>(pFormEntities);
            }
            if (pIsSaveOrCheck)
            {
                foreach (AttendanceLeave entity in list)
                {
                    entity.IsCheckAtType = true;
                    entity.StateId = "PlanState_00";
                    entity.OwnerId = entity.EmployeeId.GetString();         
                    if (!entity.EmployeeId.CheckNullOrEmpty())
                    {
                        string employeeId = entity.EmployeeId.GetString();

                        string corporationId = empServ.GetEmpFiledById(employeeId,"CorporationId");
                        if (!corporationId.CheckNullOrEmpty())
                        {
                            entity.CorporationId = corporationId.GetGuid();
                        }

                        DateTime tempDate = entity.BeginDate;
                        if (!isUltimate && !isCir)
                        {
                            DataTable dtEmpRank = empRankServ.GetEmpRanks(new string[] { employeeId }, tempDate.AddDays(-1), tempDate);
                            if (dtEmpRank != null && dtEmpRank.Rows.Count > 0 && Convert.ToBoolean(dtEmpRank.Rows[0]["IsBelongToBefore"].ToString()))
                            {
                                tempDate = tempDate.AddDays(-1);
                            }
                        }
                        string fiscalYearId = GetFisicalYearIdbyDate(employeeId, tempDate);
                     
                        if (!fiscalYearId.CheckNullOrEmpty())
                        {
                            entity.FiscalYearId = fiscalYearId.GetGuid();
                        }
                    }
                }
            }
            return list;
        }


        /// <summary>
        /// 獲取請假時數
        /// </summary>
        /// <param name="formEntity">實體</param>
        /// <returns>返回Hours:時數,Unit:單位</returns>
        public virtual DataTable GetLeaveHoursForAPI(AttendanceLeaveForAPI formEntity)
        {
            this.CheckValue(formEntity);

            DataTable dtHour = new DataTable();
            //AttendanceOverTimeRestService restService = new AttendanceOverTimeRestService();    
            if (formEntity.AttendanceTypeId.Equals("406"))
            {
                //調休假
                //List<AttendanceOverTimeRest> list = this.ChangeToOTRestEntity(new AttendanceLeaveForAPI[] { formEntity });
                //dtHour = restService.GetRestHoursForAPI(list.First());
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
        private List<AttendanceOverTimeRest> ChangeToOTRestEntity(AttendanceLeaveForAPI[] pFormEntities)
        {
            List<AttendanceOverTimeRest> list = new List<AttendanceOverTimeRest>();
            AttendanceOverTimeRest entity = null;
            foreach (AttendanceLeaveForAPI apiEntity in pFormEntities)
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
            // 校验参数
            if (pEmployeeId.CheckNullOrEmpty())
                throw new ArgumentNullException("pEmployeeId");
            if (pDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("pDate");
            }
             
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

            // 年假 
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

                    // 取结余作废时数
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


            // 特殊假
            DataTable dtAtSpecialHolidaySet = new DataTable();
          
                // 20100919 增加校验请假最后一天是否是跨天班
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

            //// 20101106 added by jiangpeng for 台湾特休假

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

            // 
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
            // 校验参数
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

            // 获取特修参数的计算方式
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
                // 查询SQL
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

            // 判断重复
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
                                    // 规律假
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
            // 参数检查
            if (pEmployeeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("Employee is null");
            }
             
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
            // 20120320 验证班次
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


            // 20101116 added by jiangpeng for 先更新年假结余表
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
             
            string checkStr = null;
           
           // IATMonthService atMonthSer = Factory.GetService<IATMonthService>();
            StringBuilder sbError = new StringBuilder();
            string errorMsg = string.Empty;
            //foreach (AttendanceLeaveInfo info in pDataEntity.Infos)
            //{
            //    errorMsg = atMonthSer.CheckIsClosedByEmployeeIdDate(info.EmployeeId.GetString(), info.Date);
            //    if (!string.IsNullOrEmpty(errorMsg))
            //    {
            //        sbError.Append(errorMsg);
            //    }
            //}
            if (sbError.Length > 0)
            {
                throw new BusinessRuleException(sbError.ToString());
            }
            //校验时间重复
            checkStr =CheckAllTime(pDataEntity.EmployeeId.GetString(), pDataEntity.BeginDate,
                pDataEntity.BeginTime, pDataEntity.EndDate, pDataEntity.EndTime, CheckEntityType.Leave, pDataEntity.AttendanceLeaveId.GetString(), pDataEntity.AttendanceLeaveId.CheckNullOrEmpty() ? true : false);
            if (!checkStr.CheckNullOrEmpty())
                throw new BusinessRuleException(checkStr);

            if (pDataEntity.Dirty)
            {//对主操作
                if (pDataEntity.IsRevoke)
                {//销过假了
                    throw new BusinessRuleException(Resources.Error_ModifyOnRevoked);
                }
        /// <summary>
        /// 计算时数
        /// </summary>
        /// <param name="pLeave">请假实体</param>
        /// <returns>时数</returns>
        public virtual decimal GetLeaveHours(AttendanceLeave pLeave)
        {
            decimal hours = 0M;
            if (pLeave.Infos.Count == 0)
                {
                    pLeave = this.SetLeaveInfos(pLeave, ConvertDateTime(pLeave.BeginDate.ToShortDateString(), pLeave.BeginTime), ConvertDateTime(pLeave.EndDate.ToShortDateString(), pLeave.EndTime));

            }
            if (pLeave.Infos.Count == 0)
            {
                // 20170412 modified by yingchun for A00-20170103002_38796+38797+38798
                //如果没排班，则报异常员工未排班
                AttendanceEmpRankService empRankService = Factory.GetService<AttendanceEmpRankService>();
                DataTable dtEmpRank = empRankService.GetEmpRanks(new string[] { pLeave.EmployeeId.GetString() }, pLeave.BeginDate, pLeave.EndDate);
                if (dtEmpRank == null || dtEmpRank.Rows.Count == 0)
                {
                    throw new BusinessRuleException("员工未排班");
                }
                throw new BusinessRuleException("输入时段内不存在需要请假的时间区间");//輸入時段內不存在需要請假的時間區間
                //return hours;

            }
            foreach (AttendanceLeaveInfo info in pLeave.Infos)
            {
                hours += info.Hours;
            }
            return hours;
        }

        protected void UpdateSpecialDataNew(AttendanceLeave leave, bool pIsCheckInly)
        {
            EmployeeService employeeService = new EmployeeService();
            string employeeName = employeeService.GetEmployeeNameById(leave.EmployeeId.GetString());
           // IDocumentService<AttendanceType> typeService = Factory.GetService<IAttendanceTypeService>().GetServiceNoPower();
           AttendanceTypeService typeService = new AttendanceTypeService();   
            AttendanceType attype = typeService.GetAttendanceType(leave.AttendanceTypeId);
            List<ATSpecialHolidaySet> listAtSpecial = new List<ATSpecialHolidaySet>();
            Dictionary<string, ATSpecialHolidaySet> dicAtSpecial = new Dictionary<string, ATSpecialHolidaySet>();
            ATSpecialHolidaySetService atSpecialSer= new ATSpecialHolidaySetService();
            //IATSpecialHolidaySetService atSpecialSer = Factory.GetService<IATSpecialHolidaySetService>();
            //IDocumentService<ATSpecialHolidaySet> docAtSpecial = atSpecialSer;
            DataTable specialDt = new DataTable();
            if (attype != null && attype.AttendanceKindId.Equals("AttendanceKind_011"))
            {
                decimal specialHours = 0;  //可休总时数
                DataTable tempDt = new DataTable();
                string specialId = string.Empty;  //特珠假Id
                string leaveInfoIds = string.Empty;  //请假明细Id
                StringBuilder sbIdAndHours = new StringBuilder();
                Dictionary<string, string> dicInfoIdAndHours = new Dictionary<string, string>();
                // 逻辑更改，验证特殊假是否可以申请
                //循环列表，找出可休的那个特殊假设置
                decimal RemainHours = 0;
                decimal infoHours = 0;  //明细时数
                ATSpecialHolidaySet atSet = null;
                List<Guid> listSpecIds = new List<Guid>();  //记录单次休完的ID
                // 记录需休 与 可休
                decimal totalHours = 0;
                decimal hasHours = 0;
                foreach (AttendanceLeaveInfo info in leave.Infos)
                {
                    totalHours += info.Hours;
                }

                    //根据公司获得参数
                    AnnualLeaveParameter parameter = iAnnualLeaveParameterServer.GetParameterIdByCorporationId(corporationId);
                    IAnnualLeaveBalanceService balanceSer = Factory.GetService<IAnnualLeaveBalanceService>();

                foreach (AttendanceLeaveInfo info in leave.Infos)
                {
                    specialHours = 0;
                    infoHours = info.Hours;
                    sbIdAndHours = new StringBuilder();
                    //20150824 Modified by liuyuana for 31651&A00-20150821002
                    //specialDt = this.GetSpecialLeave(info.EmployeeId.GetString(), leave.AttendanceTypeId, info.Date);
                    specialDt = this.GetSpecialLeave(leave);
                    for (int i = 0; i < specialDt.Rows.Count; i++)
                    {
                        specialId = specialDt.Rows[i]["ATSpecialHolidaySetId"].ToString();
                        if (dicAtSpecial.ContainsKey(specialId))
                        {
                            atSet = dicAtSpecial[specialId];
                        }
                        else
                    {
                            atSet = atSpecialSer.GetATSpecialHolidaySet(specialId);

                    }
                        if (attype.AttendanceUnitId.Equals("AttendanceUnit_001"))
                        {
                            if (atSet.DaySTHours > 0)
                    {
                                RemainHours = atSet.RemaiderDays * atSet.DaySTHours;
                    }
                    else
                    {
                                RemainHours = atSet.RemaiderDays;
                                infoHours = info.Days;
                            }
                        }
                        else if (attype.AttendanceUnitId.Equals("AttendanceUnit_002"))
                        {
                            RemainHours = atSet.RemaiderDays;
                        }
                        else if (attype.AttendanceUnitId.Equals("AttendanceUnit_003"))
                        {
                            RemainHours = atSet.RemaiderDays / 60;
                    }
                        specialHours += RemainHours;
                        if (!info.IsRevoke && atSet.RemaiderDays > 0)
                        {
                            if (infoHours > 0)
                            {
                                decimal oldActualDays = atSet.ActualDays;
                                infoHours -= RemainHours;
                                if (infoHours >= 0)
                        {
                                    sbIdAndHours.AppendFormat(string.Format("{0},{1};", atSet.ATSpecialHolidaySetId, atSet.RemaiderDays.ToString()));
                                    atSet.RemaiderDays = 0;
                                    atSet.ActualDays = atSet.Amount;
                        }
                        else
                        {

                                    if (atSet.IsOnceOver)
                                    {
                                        if (!listSpecIds.Contains(atSet.ATSpecialHolidaySetId))
                                        {
                                            listSpecIds.Add(atSet.ATSpecialHolidaySetId);
                                        }
                        }
                        // balanceDays = planService.GetBalanceDaysPreYears(pDataEntity.EmployeeId.GetString(), pDataEntity.FiscalYearId.GetString());//加上结余,不能从结余表里取,从本表取上一年的

                        bEndDate = Factory.GetService<IAnnualLeaveBalanceService>().getBalanceEndDate(pDataEntity.FiscalYearId.GetString(), pDataEntity.EmployeeId.GetString(), corporationId);

                                    if (attype.AttendanceUnitId.Equals("AttendanceUnit_001"))
                                    {
                                        if (atSet.DaySTHours > 0)
                                        {
                                            sbIdAndHours.AppendFormat(string.Format("{0},{1};", atSet.ATSpecialHolidaySetId, ((RemainHours + infoHours) / atSet.DaySTHours).ToString()));
                                            atSet.RemaiderDays = (-infoHours) / atSet.DaySTHours;
                                        }
                                        else
                                        {
                                            sbIdAndHours.AppendFormat(string.Format("{0},{1};", atSet.ATSpecialHolidaySetId, (RemainHours + infoHours).ToString()));
                                            atSet.RemaiderDays = (-infoHours);
                                        }
                                    }
                                    else if (attype.AttendanceUnitId.Equals("AttendanceUnit_002"))
                                    {
                                        sbIdAndHours.AppendFormat(string.Format("{0},{1};", atSet.ATSpecialHolidaySetId, (RemainHours + infoHours).ToString()));
                                        atSet.RemaiderDays = (-infoHours);
                                    }
                                    else if (attype.AttendanceUnitId.Equals("AttendanceUnit_003"))
                                    {
                                        sbIdAndHours.AppendFormat(string.Format("{0},{1};", atSet.ATSpecialHolidaySetId, ((RemainHours + infoHours) * 60).ToString()));
                                        atSet.RemaiderDays = (-infoHours);
                                    }
                                    atSet.ActualDays = atSet.Amount - atSet.RemaiderDays;
                                    //}
                    }
                                // 20180711 add by LinBJ for Q00-20180709001 增加Log
                                string msg = string.Empty;
                                if (atSet.IsOnceOver)
                    {
                                    msg = string.Format("{0}员工新增{1} {2} ~ {3} {4} {5}数量为{6}，回写{7}，原已休数量{8}，更新后已休数量{9}，可休剩余数量{10}"
                                         , employeeName, info.BeginDate.ToDateFormatString(), info.BeginTime, info.EndDate.ToDateFormatString(), info.EndTime
                                         , attype.Name, atSet.Amount, atSet.ATSpecialHolidaySetId.ToString(), 0, atSet.Amount, 0);
                    }
                                else
                    {
                                    msg = string.Format("{0}员工新增{1} {2} ~ {3} {4} {5}数量为{6}，回写{7}，原已休数量{8}，更新后已休数量{9}，可休剩余数量{10}"
                                    , employeeName, info.BeginDate.ToDateFormatString(), info.BeginTime, info.EndDate.ToDateFormatString(), info.EndTime
                                    , attype.Name, atSet.ActualDays - oldActualDays, atSet.ATSpecialHolidaySetId.ToString(), oldActualDays, atSet.ActualDays, atSet.RemaiderDays);
                                }
                                bool hasStr = false;
                                if (atSet.ExtendedProperties.Count > 0)
                        {
                                    foreach (ExterFields f in atSet.ExtendedProperties)
                            {
                                        if (f.Name == "InfoStrList")
                                {
                                            f.Value += msg;
                                            hasStr = true;
                                        }
                                }
                                    if (!hasStr)
                                    {
                                        ExterFields fNew = new ExterFields();
                                        fNew.Name = "InfoStrList";
                                        fNew.Value = "msg";
                                        atSet.ExtendedProperties.Add(fNew);
                            }
                        }
                                //if (atSet.ExtendedProperties.ContainsKey("InfoStrList"))
                                //{
                                //    List<string> infoStrList = atSet.ExtendedProperties["InfoStrList"] as List<string>;
                                //    infoStrList.Add(msg);
                                //}
                                //else
                                //{
                                //    List<string> infoStrList = new List<string>();
                                //    infoStrList.Add(msg);
                                //    atSet.ExtendedProperties.Add("InfoStrList", infoStrList);
                                //}
                                if (!atSet.LeaveInfoIds.CheckNullOrEmpty())
                        {
                                    string[] infoArray = atSet.LeaveInfoIds.Split(',');
                                    if (infoArray.Length > 0)
                            {
                                        List<string> infoIdList = infoArray.ToList();
                                        if (!infoIdList.Contains(info.AttendanceLeaveInfoId.ToString()))
                                {
                                            infoIdList.Add(info.AttendanceLeaveInfoId.ToString());
                                            atSet.LeaveInfoIds = string.Join(",", infoIdList.ToArray());
                                        }
                                }
                                    else
                                    {
                                        atSet.LeaveInfoIds = string.Join(",", new string[] { info.AttendanceLeaveInfoId.ToString() });
                            }
                        }
                                else
                                {
                                    atSet.LeaveInfoIds = string.Join(",", new string[] { info.AttendanceLeaveInfoId.ToString() });
                    }

                                dicAtSpecial[specialId] = atSet;
                            }
                            else
                            {
                                break;
                            }
                        }

                    }
                    //此处做统计 mark20121220 处一并报出
                    if (infoHours > 0)
                    {
                        hasHours += info.Hours - infoHours;
                    }
                    else
                    {
                        hasHours += info.Hours;
                    }

                    if (!dicInfoIdAndHours.ContainsKey(info.AttendanceLeaveInfoId.GetString()))
                    {
                        dicInfoIdAndHours.Add(info.AttendanceLeaveInfoId.GetString(), sbIdAndHours.ToString());
                    }
                    }

                //20180223 added by yingchun for Q00-20180222002 : 退回重辦後時數沒有還回去，導致時數不足                
                if (leave.IsEss && pIsCheckInly && !leave.AttendanceLeaveId.CheckNullOrEmpty())
                {
                    string sql = "SELECT TotalHours FROM AttendanceLeave WHERE AttendanceLeaveId = @AttendanceLeaveId";
                    List<SqlParameter> listParameter = new List<SqlParameter>();
                    listParameter.Add(new SqlParameter("@AttendanceLeaveId", SqlDbType.UniqueIdentifier, -1) { Value = leave.AttendanceLeaveId });
                    DataTable dt = HRHelper.ExecuteDataTable(sql, listParameter.ToArray());
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        hasHours += Decimal.Parse(dt.Rows[0]["TotalHours"].ToString());
                    }
                    }

                //mark20121220
                if (hasHours < totalHours)
                        {
                    bool IsEmployeeDimission = false;
                    if (atSet.ExtendedProperties.Count > 0)
                            {
                        foreach (ExterFields f in atSet.ExtendedProperties)
                                {
                            if (f.Name.Equals("IsEmployeeDimission"))
                                    {
                                IsEmployeeDimission = true;
                                    }
                                }
                            }
                    //如果是離職存檔不提示 added by zhoug 20150303 for bug 26798&26797&26799 Q00-20150302002
                    //  if (!leave.ExtendedProperties.Contains("IsEmployeeDimission"))
                    if (IsEmployeeDimission)
                            {
                        //时数不足
                        throw new BusinessRuleException(string.Format("员工 {0} 特殊假 {1} 的可休时数为 {2}，本次请假 {3}，剩余可休时数不足", employeeName, attype.Name, hasHours.ToString(), totalHours.ToString()));
                    }
                }
                 

                if (!pIsCheckInly)
                                {
                    //更新请假明细
                    foreach (KeyValuePair<string, string> keyValue in dicInfoIdAndHours)
                                    {
                        UpdateLeaveInfoSpecial(keyValue.Key, keyValue.Value);
                                    }
                    //更新特殊假数据
                    List<ATSpecialHolidaySet> listTemp = new List<ATSpecialHolidaySet>();
                    foreach (string str in dicAtSpecial.Keys)
                    {
                        //在此处处理单次休完
                        if (listSpecIds.Contains(str.GetGuid()))
                        {
                            dicAtSpecial[str].ActualDays = dicAtSpecial[str].Amount;
                            dicAtSpecial[str].RemaiderDays = 0;
                                }
                       // dicAtSpecial[str].ExtendedProperties.Add("ForLeave", "ForLeave");
                        listTemp.Add(dicAtSpecial[str]);
                            }
                    //  docAtSpecial.Save(listTemp.ToArray());

                    string sql = HRHelper.GenerateSqlInsertMulti(listTemp, "ATSpecialHolidaySet");
                    HRHelper.ExecuteNonQuery(sql);
                    }

                    }
                    if (pDataEntity.AttendanceTypeId.CheckNullOrEmpty())
                        pDataEntity.AttendanceTypeId = "401";
                }

        /// <summary>
        /// 获取特殊假设置
        /// </summary>
        /// <param name="pLeave"></param>
        /// <returns></returns>
        public virtual DataTable GetSpecialLeave(AttendanceLeave pLeave)
                {
            DataTable dtRank = new DataTable();
            DataTable dt = new DataTable();
            
                StringBuilder sb = new StringBuilder();
                //判段请假的结束时间是不是跨天班，如果是的就把结束日期减一天
                string strSql = string.Format(@"SELECT IsOverZeroId
                                                    FROM   AttendanceemPrank AS emPrank
                                                           LEFT JOIN AttendanceRank AS Rank
                                                             ON emPrank.AttendanceRankId = Rank.AttendanceRankId
                                                    WHERE  emPrank.EmployeeId = '{0}'
                                                           AND emPrank.DATE = '{1}'", pLeave.EmployeeId.GetString(), pLeave.EndDate.AddDays(-1).ToDateFormatString());
                
                dtRank=HRHelper.ExecuteDataTable(strSql);

                sb.AppendFormat(@"SELECT EmployeeId,Amount,DaySTHours,IsOnceOver,ATSpecialHolidaySetId,
                                    ActualDays,RemaiderDays,LeaveInfoIds
                                    FROM ATSpecialHolidaySet  
                                    WHERE EmployeeId = '{0}' AND AttendanceTypeId = '{1}' ", pLeave.EmployeeId.GetString(), pLeave.AttendanceTypeId);

                // 20090907 marked by jiangpeng 修正繁体机上DateTime转换错误
                sb.AppendFormat("AND BeginDate <= '{0}' ", pLeave.BeginDate.ToString("yyy-MM-dd HH:mm:ss"));
                DateTime endDate = pLeave.EndDate;
                if (dtRank != null && dtRank.Rows.Count > 0 && dtRank.Rows[0][0].ToString().Equals("TrueFalse_001"))
                {
                    if (pLeave.EndDate > pLeave.BeginDate)
                {
                        endDate = pLeave.EndDate.AddDays(-1);
                }
            }
                sb.AppendFormat("AND EndDate >= '{0}' ", endDate.ToString("yyy-MM-dd HH:mm:ss"));
                sb.Append("AND RemaiderDays >0 ");
                sb.Append("ORDER BY BeginDate");

                dt=HRHelper.ExecuteDataTable(sb.ToString());
            return dt;
        }

        public void UpdateLeaveInfoSpecial(string pLeaveInfoId, string pIdAndHours)
        {
                HRHelper.ExecuteNonQuery( string.Format("UPDATE AttendanceLeaveInfo SET SpecialSetIdAndHours = '{0}' Where AttendanceLeaveInfoId='{1}'", pIdAndHours, pLeaveInfoId));
        }

        /// <summary>
        /// 根据类型Id获取名称
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetAttendanceTypeNameById(string typeId) {
          
            if (typeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("typeId Error");
            }
             

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
    #region 輔助類
    /// <summary>
    /// 班次内区间相关
    /// </summary>
    public class RankRestinfo
    {
        /// <summary>
        /// 班段类型
        /// </summary>
        public string AttendanceRankType;

        /// <summary>
        /// 班段时数
        /// </summary>
        public double RestHours;

        /// <summary>
        /// 班段开始时间
        /// </summary>
        public string RestBegin;

        /// <summary>
        /// 班段结束时间
        /// </summary>
        public string RestEnd;

        //20110720 added by songyj for 计算请假时数时与客户端逻辑相同
        /// <summary>
        /// 是否扣除在时数
        /// </summary>
        public bool NotJobTime;

        //20110520 added by songyj for 判断弹性班次的依据
        /// <summary>
        /// 班段编号
        /// </summary>
        public int RankCode;
    }

    /// <summary>
    /// 班次内区间相关
    /// </summary>
    public class AnnualRankRestinfo
    {
        /// <summary>
        /// 班段类型
        /// </summary>
        public string AttendanceRankType;

        /// <summary>
        /// 班段时数
        /// </summary>
        public double RestHours;

        /// <summary>
        /// 班段开始时间
        /// </summary>
        public DateTime RestBegin;

        /// <summary>
        /// 班段结束时间
        /// </summary>
        public DateTime RestEnd;

        /// <summary>
        /// 是否扣除在岗时数
        /// </summary>
        public bool IsNotJobTime;
    }
    #endregion
}
