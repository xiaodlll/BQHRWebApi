using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.HR;
using Dcms.HR.Business.Implement.Properties;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;


namespace BQHRWebApi.Service
{

    public class AttendanceLeaveService : HRService
    {


        public override void Save(DataEntity[] entities)
        {
            foreach (var entity in entities)
            {
                AttendanceLeaveForAPI enty = entity as AttendanceLeaveForAPI;

            }
        }


        #region FWToHRServer

        public async Task<DataTable> GetLeaveHoursForCase(AttendanceLeaveForAPI enty)
        {
            EmployeeService employeeService = new EmployeeService();// Factory.GetService<EmployeeService>();
            string response = "";
            string empId = employeeService.GetEmpIdByCode(enty.EmployeeCode);
            if (empId.CheckNullOrEmpty())
            {
                throw new BusinessRuleException("EmployeeCode 找不到对应的员工");
            }
            enty.EmployeeId = empId.GetGuid();
            enty.AttendanceLeaveId = Guid.NewGuid();

            string s = CheckValue(enty);
            if (!s.CheckNullOrEmpty())
            {
                throw new BusinessRuleException(s);
            }
            AttendanceLeaveForAPI[] arrayEntity = new AttendanceLeaveForAPI[] { enty };
            try
            {
                if (enty.AttendanceTypeId.Equals("406"))
                {
                    //調休假
                    List<AttendanceOverTimeRest> list = this.ChangeToOTRestEntity(arrayEntity);
                    CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
                    callServiceBindingModel.RequestCode = "API_AT_005";
                    APIRequestParameter parameter = new APIRequestParameter();
                    parameter.Name = "formEntity";
                    parameter.Value = JsonConvert.SerializeObject(list.First());

                    callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

                    string json = JsonConvert.SerializeObject(callServiceBindingModel);
                    response = await HttpPostJsonHelper.PostJsonAsync(json);
                }
                else
                {
                    AttendanceLeave leave = ChangeToLeaveEntity(arrayEntity, true, string.Empty, string.Empty).First();

                    CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
                    callServiceBindingModel.RequestCode = "API_AT_002";

                    APIRequestParameter parameter = new APIRequestParameter();
                    parameter.Name = "attendanceLeave";
                    parameter.Value = JsonConvert.SerializeObject(leave);

                    callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

                    string json = JsonConvert.SerializeObject(callServiceBindingModel);
                    response = await HttpPostJsonHelper.PostJsonAsync(json);

                }
                APIExResponse aPIExResponse = JsonConvert.DeserializeObject<APIExResponse>(response);
                if (aPIExResponse != null)
                {
                    if (aPIExResponse.State != "0")
                    {
                        throw new BusinessException(aPIExResponse.Msg);
                    }
                    else
                    {
                        if (!aPIExResponse.ResultValue.CheckNullOrEmpty())
                        {
                            var dt = (DataTable)JsonConvert.DeserializeObject(aPIExResponse.ResultValue.ToString(), typeof(DataTable));

                            return dt;

                        }
                    }
                }
                else
                {
                    throw new Exception(response);
                }
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
            return null;
        }




        public async Task<Dictionary<string, DataTable>> MultiGetLeaveHours(AttendanceLeaveForAPI[] entities)
        {
            EmployeeService employeeService = new EmployeeService();// Factory.GetService<EmployeeService>();
            string response = "";

            List<AttendanceLeaveForAPI> list406=new List<AttendanceLeaveForAPI>();
            List<AttendanceLeaveForAPI> listQJ = new List<AttendanceLeaveForAPI>();

            foreach (AttendanceLeaveForAPI enty in entities)
            {
                string empId = employeeService.GetEmpIdByCode(enty.EmployeeCode);
                if (empId.CheckNullOrEmpty())
                {
                    throw new BusinessException(string.Format("{0}: EmployeeCode {1}  找不到对应的员工", enty.EssNo,enty.EmployeeCode) );
                }
                if (enty.EssNo.CheckNullOrEmpty() )
                {
                    throw new BusinessException("批量获取请假时数时，请记录ESSNo作为数据编号");
                }
                enty.EmployeeId = empId.GetGuid();
                enty.AttendanceLeaveId = Guid.NewGuid();
                string s = CheckValue(enty);
                if (!s.CheckNullOrEmpty())
                {
                    throw new BusinessException(s);
                }
                else
                {
                    if (enty.AttendanceTypeId == "406")
                    {
                        list406.Add(enty);
                    }
                    else
                    {
                        listQJ.Add(enty);
                    }
                }
            }
            try
            {
                if (list406.Count>0)
                {
                    //調休假
                    List<AttendanceOverTimeRest> list = this.ChangeToOTRestEntity(list406.ToArray());
                    CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
                    callServiceBindingModel.RequestCode = "API_AT_015";
                    APIRequestParameter parameter = new APIRequestParameter();
                    parameter.Name = "formEntities";
                    parameter.Value = JsonConvert.SerializeObject(list);

                    callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

                    string json = JsonConvert.SerializeObject(callServiceBindingModel);
                    response = await HttpPostJsonHelper.PostJsonAsync(json);
                }
                if(listQJ.Count>0)
                {
                    AttendanceLeave[] leaves = ChangeToLeaveEntity(listQJ.ToArray(), true, string.Empty, string.Empty).ToArray();

                    CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
                    callServiceBindingModel.RequestCode = "API_AT_019";

                    APIRequestParameter parameter = new APIRequestParameter();
                    parameter.Name = "formEntities";
                    parameter.Value = JsonConvert.SerializeObject(leaves);

                    callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

                    string json = JsonConvert.SerializeObject(callServiceBindingModel);
                    response = await HttpPostJsonHelper.PostJsonAsync(json);

                }
                APIExResponse aPIExResponse = JsonConvert.DeserializeObject<APIExResponse>(response);
                if (aPIExResponse != null)
                {
                    if (aPIExResponse.State != "0")
                    {
                        throw new BusinessException(aPIExResponse.Msg);
                    }
                    else
                    {
                        if (!aPIExResponse.ResultValue.CheckNullOrEmpty())
                        {
                            var dic = (Dictionary<string, DataTable>)JsonConvert.DeserializeObject(aPIExResponse.ResultValue.ToString(), typeof(Dictionary<string,DataTable>));

                            return dic;

                        }
                    }
                }
                else
                {
                    throw new Exception(response);
                }
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
            return null;
        }


        #endregion

        #region  直接处理

        /// <summary>
        /// 檢查參數值
        /// </summary>
        /// <param name="pFormEntity">實體</param>
        private string CheckValue(AttendanceLeaveForAPI pFormEntity)
        {
            StringBuilder ecMsg = new StringBuilder();
            if (pFormEntity.EmployeeId.CheckNullOrEmpty())
            {
                ecMsg.Append("EmployeeId 错误");
            }
            if (pFormEntity.AttendanceTypeId.CheckNullOrEmpty())
            {
                ecMsg.Append("AttendanceTypeId 为空");
            }
            else
            {
                AttendanceTypeService typeSer = new AttendanceTypeService();
                string s = typeSer.GetNameById(pFormEntity.AttendanceTypeId);
                if (s.CheckNullOrEmpty())
                {
                    ecMsg.Append("AttendanceTypeId 不正确");
                }
            }
            if (pFormEntity.BeginDate.CheckNullOrEmpty())
            {
                ecMsg.Append("BeginDate error");
            }
            if (pFormEntity.BeginTime.CheckNullOrEmpty())
            {
                ecMsg.Append("BeginTime error");
            }
            if (pFormEntity.EndDate.CheckNullOrEmpty())
            {
                ecMsg.Append("EndDate error");
            }
            if (pFormEntity.EndTime.CheckNullOrEmpty())
            {
                ecMsg.Append("EndTime error");
            }

            string errorMsg = string.Empty;
            if (ecMsg.Length > 0)
            {
                return errorMsg.ToString();
            }
            else
            {
                DateTime tmpTime = DateTime.Now;
                DateTime tmpTime1 = DateTime.Now;
                if (!DateTime.TryParse(string.Format("{0} {1}", pFormEntity.BeginDate.ToShortDateString(), pFormEntity.BeginTime), out tmpTime))
                {
                    ecMsg.Append("开始日期和开始时间不是正确的日期时间格式");
                }
                if (!DateTime.TryParse(string.Format("{0} {1}", pFormEntity.EndDate.ToShortDateString(), pFormEntity.EndTime), out tmpTime1))
                {
                    ecMsg.Append("结束日期和结束时间不是正确的日期时间格式");
                }

                if (tmpTime1 < tmpTime)
                {

                    ecMsg.Append("请假结束不能小于请假开始");
                }
            }
            if (ecMsg.Length > 0)
            {
                return ecMsg.ToString();
            }
            CheckEntityType entityType = new CheckEntityType();
            if (pFormEntity.AttendanceTypeId.Equals("401"))
            {
                entityType = CheckEntityType.AnnualLeave;
            }
            else if (pFormEntity.AttendanceTypeId == "406")
            {
                entityType = CheckEntityType.OTRest;
            }
            else if (pFormEntity.AttendanceTypeId == "408")
            {
                entityType = CheckEntityType.TWALReg;
            }
            else
            {
                entityType = CheckEntityType.Leave;
            }
            string errMsg = CheckAllTime(pFormEntity.EmployeeId.ToString(), pFormEntity.BeginDate, pFormEntity.BeginTime, pFormEntity.EndDate, pFormEntity.EndTime
                   , entityType, Guid.NewGuid().GetString(), true);
            if (!errMsg.CheckNullOrEmpty())
            {
                ecMsg.Append(errMsg);
            }
            return ecMsg.ToString();
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
            AttendanceTypeService typeService = new AttendanceTypeService();
            AttendanceType atType = typeService.GetAttendanceType(pAttendanceLeave.AttendanceTypeId);
            //假勤项目：请假（AttendanceKind_004）
            if (atType.AttendanceKindId.Equals("AttendanceKind_004") && atType.MaxSalaryHour > 0)
            {


                string beginDate = string.Empty;
                string endDate = string.Empty;
                if (!atType.SalaryPeriodId.CheckNullOrEmpty())
                {//计算周期
                    if (atType.SalaryPeriodId.Equals("SalaryPeriod_001"))
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
                    #region 考虑“合并假勤类型”
                    StringBuilder sbTypeIds = new StringBuilder();
                    sbTypeIds.AppendFormat("'{0}'", pAttendanceLeave.AttendanceTypeId);//添加本身的假勤类型
                    if (!atType.DaysCoverATType.CheckNullOrEmpty())
                    {//合并假勤类型
                        string[] arrayTypeIds = atType.DaysCoverATType.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
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
                          pAttendanceLeave.EmployeeId.ToString(), sbTypeIds.ToString(), beginDate, endDate);
                    #endregion
                    if (!pAttendanceLeave.AttendanceLeaveId.CheckNullOrEmpty())
                    {
                        strSQL += string.Format(" AND Leave.AttendanceLeaveId <> '{0}'", pAttendanceLeave.AttendanceLeaveId.GetString());
                    }


                    //object obj = cmd.ExecuteScalar();
                    DataTable dt = HRHelper.ExecuteDataTable(strSQL);

                    //20140916 modified by huangzj for 22233 A00-20140912004 请假天数用班次时数折算，不再用8小时
                    //按照假勤单位转换最大可请假时数
                    decimal salaryHour = atType.MaxSalaryHour;//最大允许请假时数
                    decimal totalHours = 0m; //请假总时数

                    AttendanceEmpRankService srvEmpRank = new AttendanceEmpRankService();

                    if (atType.AttendanceUnitId.Equals("AttendanceUnit_001"))
                    {//天 
                        #region 单位为天
                        //当前请假时数
                        for (int i = 0; i < pAttendanceLeave.Infos.Count; i++)
                        {
                            //if (!pAttendanceLeave.Infos[i].IsRevoke )//归属日期判断
                            //    totalHours += pAttendanceLeave.Infos[i].Days;

                            if (!pAttendanceLeave.Infos[i].IsRevoke)
                            {
                                var dtEmpRank = srvEmpRank.GetEmpRanks(new string[] { pAttendanceLeave.EmployeeId.ToString() }, pAttendanceLeave.Infos[i].Date, pAttendanceLeave.Infos[i].Date);
                                //判断是否跳过假日或休息日
                                if (atType.PassRest)
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
                                if (atType.PassHoliday)
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
                        #endregion
                    }
                    else
                    {
                        #region 单位为小时或分钟
                        //当前请假时数
                        for (int i = 0; i < pAttendanceLeave.Infos.Count; i++)
                        {
                            if (!pAttendanceLeave.Infos[i].IsRevoke)
                            {
                                var dtEmpRank = srvEmpRank.GetEmpRanks(new string[] { pAttendanceLeave.EmployeeId.ToString() }, pAttendanceLeave.Infos[i].Date, pAttendanceLeave.Infos[i].Date);
                                //判断是否跳过假日或休息日
                                if (atType.PassRest)
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
                                if (atType.PassHoliday)
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

                        if (atType.AttendanceUnitId.Equals("AttendanceUnit_003"))
                        {
                            //分钟 
                            salaryHour = salaryHour / 60;
                        }
                        #endregion
                    }

                    if (totalHours > salaryHour)
                    {
                        EmployeeService empService = new EmployeeService();
                        msg = string.Format("{0},{1},{2},false", empService.GetEmpFiledById(pAttendanceLeave.EmployeeId.ToString(), "Code"), salaryHour.ToString("#.##"), totalHours.ToString("#.##"));
                    }
                }

            }

            #region added by jiangpeng for 任务6979 增加校验：此假勤类型为另外的假勤类型的合并假勤类型
            if (msg.CheckNullOrEmpty() && atType.DaysCoverATType.CheckNullOrEmpty())
            {
                DataTable dtType = new DataTable();


                #region old
                //string strSql = string.Format(@"SELECT AttendanceTypeId,
                //                                       SalaryPeriodId,
                //                                       MaxSalaryHour,
                //                                       DaysCoverAtType,
                //                                       AttendanceUnitId
                //                                FROM   AttendanceType
                //                                WHERE  DaysCoverAtType LIKE '%{0}%'", pAttendanceLeave.AttendanceTypeId);
                #endregion
                //20140604 modified for bug18749 && C01-20140529011 by renping
                string strSql = string.Format(@"SELECT AttendanceTypeId,
                                                           SalaryPeriodId,
                                                           MaxSalaryHour,
                                                           DaysCoverAtType,
                                                           AttendanceUnitId
                                                    FROM   AttendanceType
                                                    WHERE  DaysCoverAtType LIKE '{0};%' or DaysCoverATType LIKE '%;{0};%'", pAttendanceLeave.AttendanceTypeId);

                dtType = HRHelper.ExecuteDataTable(strSql);

                if (dtType != null && dtType.Rows.Count > 0)
                {
                    decimal maxSalaryHour = 0;
                    string salaryPeriodId = string.Empty;
                    string attendanceTypeId = string.Empty;
                    string daysCoverATType = string.Empty;
                    string attendanceUnitId = string.Empty;
                    string strSalaryHour = dtType.Rows[0]["MaxSalaryHour"].ToString();
                    salaryPeriodId = dtType.Rows[0]["SalaryPeriodId"] as string;
                    attendanceTypeId = dtType.Rows[0]["AttendanceTypeId"] as string;
                    daysCoverATType = dtType.Rows[0]["DaysCoverATType"] as string;
                    attendanceUnitId = dtType.Rows[0]["AttendanceUnitId"] as string;
                    decimal.TryParse(strSalaryHour, out maxSalaryHour);
                    if (maxSalaryHour > 0 && !salaryPeriodId.CheckNullOrEmpty() && !attendanceTypeId.CheckNullOrEmpty())
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
                            #region 考虑“合并假勤类型”

                            StringBuilder sbTypeIds = new StringBuilder();
                            sbTypeIds.AppendFormat("'{0}'", attendanceTypeId);//添加本身的假勤类型
                            if (!daysCoverATType.CheckNullOrEmpty())
                            {//合并假勤类型
                                string[] arrayTypeIds = daysCoverATType.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
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
                                  pAttendanceLeave.EmployeeId.ToString(), sbTypeIds.ToString(), beginDate, endDate);
                            #endregion
                            if (!pAttendanceLeave.AttendanceLeaveId.CheckNullOrEmpty())
                            {
                                strSQL += string.Format(" AND Leave.AttendanceLeaveId <> '{0}'", pAttendanceLeave.AttendanceLeaveId.GetString());
                            }


                            DataTable dt = HRHelper.ExecuteDataTable(strSQL);

                            //20140916 modified by huangzj for 22233 A00-20140912004 请假天数用班次时数折算，不再用8小时
                            //按照假勤单位转换最大可请假时数
                            decimal salaryHour = maxSalaryHour;//最大允许请假时数
                            decimal totalHours = 0m; //请假总时数
                            if (attendanceUnitId.Equals("AttendanceUnit_001"))
                            {//天 
                                #region 单位为天
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
                                #endregion
                            }
                            else
                            {
                                #region 单位为小时或分钟
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

                                if (attendanceUnitId.Equals("AttendanceUnit_003"))
                                {
                                    //分钟 
                                    salaryHour = salaryHour / 60;
                                }
                                #endregion
                            }

                            if (totalHours > salaryHour)
                            {
                                EmployeeService empService = new EmployeeService();
                                msg = string.Format("{0},{1},{2},false", empService.GetEmpFiledById(pAttendanceLeave.EmployeeId.ToString(), "Code"), salaryHour.ToString("#.##"), totalHours.ToString("#.##"));
                            }
                        }

                    }
                }
            }
            #endregion

            return msg;

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
            EmployeeService empService = new EmployeeService();
            DataTable dtEmpInfo = empService.GetEmployeeInfoByIds(new string[] { pAttendanceLeave.EmployeeId.ToString() });
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
            DataTable dtParaConfig = HRHelper.ExecuteDataTable(string.Format("select * from HRParaConfig where HRParaConfigId='{0}'", Constants.Para_LeavePeriod + corporationId));
            if (dtParaConfig == null || dtParaConfig.Rows.Count == 0)
            {
                if (!corporationId.GetGuid().Equals(Constants.SYSTEMGUID_CORPORATION_ROOT.GetGuid()))
                {//不是根公司，才去找根公司
                    dtParaConfig = HRHelper.ExecuteDataTable(string.Format("select * from HRParaConfig where HRParaConfigId='{0}'", Constants.Para_LeavePeriod + Constants.SYSTEMGUID_CORPORATION_ROOT));
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
                        ATMonthService monthService = new ATMonthService();
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
                        PeriodDate pD = new PeriodDate();
                        pD.BeginDate = beginDate;
                        pD.EndDate = endDate;
                        periodDateList.Add(pD);
                    }
                }
            }
            return periodDateList;
        }



        /// <summary>
        /// 檢查請假時間與出差申請時間是否重複
        /// </summary>
        /// <param name="formEntities">實體陣列</param>
        /// <returns>無錯誤返回null，有錯誤則返回字典類型(陣列第幾筆, 錯誤訊息)</returns>
        /// <remarks>提示訊息：請假時間與出差申請時間重疊，是否繼續提交表單？</remarks>
        public virtual Dictionary<string, string> CheckBusinessApplyTime(AttendanceLeaveForAPI[] formEntities)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            CheckEntityType entityType = new CheckEntityType();
            bool isUltimate = true;

            foreach (AttendanceLeaveForAPI apiEntity in formEntities)
            {
                if (apiEntity.AttendanceTypeId.Equals("401"))
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

                string msg = this.CheckBusinessApplyTime(apiEntity.EmployeeId.ToString(), apiEntity.BeginDate, apiEntity.BeginTime, apiEntity.EndDate, apiEntity.EndTime, entityType, string.Empty, true);
                if (!msg.CheckNullOrEmpty())
                {
                    dic.Add(apiEntity.EssNo, "提示訊息：請假時間與出差申請時間重疊，是否繼續提交表單？");
                }
            }
            if (dic.Count > 0)
            {
                return dic;
            }
            return null;
        }
        /// <summary>
        /// 校验与出差申请时间是否重复
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
        public string CheckBusinessApplyTime(string pEmployeeId, DateTime pBeginDate, string pBeginTime, DateTime pEndDate, string pEndTime, CheckEntityType pEntityType, string pGuid, bool pIsRuleLeave)
        {
            DataTable dt = new DataTable();
            string errorMsg = string.Empty;
            DateTime tempBegin = DateTime.MinValue;
            DateTime tempEnd = DateTime.MinValue;
            DateTime begin = DateTime.MinValue;
            DateTime end = DateTime.MinValue;
            string typeId = string.Empty;
            DataTable dtApply = new DataTable();
            DataTable dtBusiness = new DataTable();

            StringBuilder sb = new StringBuilder();

            #region 取出差申请时间

            sb.AppendFormat(@"SELECT person.EmployeeId, BusinessApply.BeginDate, BusinessApply.BeginTime, BusinessApply.EndDate, BusinessApply.EndTime, ISNULL(BusinessApply.AttendanceTypeId, '701') AS AttendanceTypeId
                                                  FROM BusinessApplyPerson AS person
                                                  LEFT JOIN BusinessApply ON BusinessApply.BusinessApplyId = person.BusinessApplyId
                                                  WHERE person.EmployeeId = '{0}' 
                                                  AND person.BusinessApplyId NOT IN (
                                                    SELECT main.BusinessApplyId FROM BusinessRegisterinfo as info
                                                    left join BusinessRegister as main on main.BusinessRegisterId=info.BusinessRegisterId
                                                    where IsRevoke=1 
                                                    And info.EmployeeId='{0}' 
                                                    And info.BeginDate >= '{1}' AND info.BeginDate <= '{2}') 
                                                  AND person.BusinessApplyId NOT IN (
                                                      SELECT BusinessApplyId 
                                                      FROM BusinessApply 
                                                      WHERE StateId = 'PlanState_003' AND ApproveResultId = 'OperatorResult_002') 
                                                  AND BusinessApply.BeginDate >= '{1}' AND BusinessApply.BeginDate <= '{2}'  ", pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());

            if (pEntityType == CheckEntityType.Apply && !pGuid.CheckNullOrEmpty())
            {
                sb.AppendFormat(" AND info.BusinessApplyId <> '{0}' ", pGuid);
            }
            #endregion

            EmployeeService empSer = new EmployeeService();
            string empName = empSer.GetEmployeeNameById(pEmployeeId);
            dt = HRHelper.ExecuteDataTable(sb.ToString());


            #region 判断重复
            begin = pBeginDate.AddTimeToDateTime(pBeginTime);
            end = pEndDate.AddTimeToDateTime(pEndTime);
            AttendanceTypeService typeSer = new AttendanceTypeService();

            DateTime beginDate = DateTime.MinValue;
            DateTime endDate = DateTime.MinValue;
            string strBegin = string.Empty;
            string strEnd = string.Empty;
            string strBeginTime = string.Empty;
            string strEndTime = string.Empty;
            string typeName = string.Empty;
            //请规律假
            DateTime beginPoint = DateTime.MinValue;//假期开始点
            DateTime endPoint = DateTime.MinValue;//假期结束点
            DateTime leaveBegin = DateTime.MinValue;//假期开始日期
            DateTime leaveEnd = DateTime.MinValue;//假期结束日期
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    tempBegin = DateTime.MinValue;
                    tempEnd = DateTime.MinValue;
                    beginDate = DateTime.MinValue;
                    endDate = DateTime.MinValue;
                    typeId = row["AttendanceTypeId"] as string;
                    typeName = typeSer.GetNameById(typeId);
                    strBegin = row["BeginDate"].ToString();
                    strEnd = row["EndDate"].ToString();
                    strBeginTime = row["BeginTime"] as string;
                    strEndTime = row["EndTime"] as string;
                    if (!typeId.CheckNullOrEmpty() && !strBegin.CheckNullOrEmpty() && !strEnd.CheckNullOrEmpty()
                        && !strBeginTime.CheckNullOrEmpty() && !strEndTime.CheckNullOrEmpty())
                    {
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
            EmployeeService empServ = new EmployeeService();
            AttendanceEmpRankService empRankServ = new AttendanceEmpRankService();
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
                    entity.StateId = Constants.PS02;
                    entity.OwnerId = entity.EmployeeId.ToString();
                   // entity.EssNo = pFormNumber;
                    if (!entity.EmployeeId.CheckNullOrEmpty())
                    {
                        string employeeId = entity.EmployeeId.ToString();

                        string corporationId = empServ.GetEmpFiledById(employeeId, "CorporationId");
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
                    entity.AttendanceOverTimeRestId = Guid.NewGuid();
                }
                Guid guid = Guid.NewGuid();
                Guid.TryParse(apiEntity.EmployeeId.ToString(), out guid);
                entity.EmployeeId = guid;
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
        /// <summary>
        /// API實體轉為年假實體(AttendanceLeaveForAPI -> AnnualLeaveRegister)
        /// </summary>
        /// <param name="pFormEntities">AttendanceLeaveForAPI實體</param>
        /// <param name="pIsSave">是否為存檔</param>
        /// <param name="pFormType">單別</param>
        /// <param name="pFormNumber">單號</param>
        /// <returns>AnnualLeaveRegister實體</returns>
        private List<AnnualLeaveRegister> ChangeToALRegisterEntity(AttendanceLeaveForAPI[] pFormEntities)
        {
            List<AnnualLeaveRegister> list = new List<AnnualLeaveRegister>();
            AnnualLeaveRegister entity = null;
            foreach (AttendanceLeaveForAPI apiEntity in pFormEntities)
            {
                entity = new AnnualLeaveRegister();
                if (entity.AnnualLeaveRegisterId.CheckNullOrEmpty())
                {
                    entity.AnnualLeaveRegisterId = Guid.NewGuid();
                }
                Guid guid = Guid.NewGuid();
                Guid.TryParse(apiEntity.EmployeeId.ToString(), out guid);
                entity.EmployeeId = guid;
                entity.AttendanceTypeId = apiEntity.AttendanceTypeId;
                entity.BeginDate = apiEntity.BeginDate;
                entity.BeginTime = apiEntity.BeginTime;
                entity.EndDate = apiEntity.EndDate;
                entity.EndTime = apiEntity.EndTime;
                entity.Remark = apiEntity.Remark;
                //Guid.TryParse(apiEntity.FiscalYearId.ToString(), out guid);
                //entity.FiscalYearId =guid;
                if (!entity.EmployeeId.CheckNullOrEmpty())
                {
                    EmployeeService empSer = new EmployeeService();
                    string corporationId = empSer.GetEmpFiledById(entity.EmployeeId.GetString(), "CorporationId");
                    if (!corporationId.CheckNullOrEmpty())
                    {
                        entity.CorporationId = corporationId.GetGuid();
                    }

                    DateTime tempDate = entity.BeginDate;

                    string fiscalYearId = this.GetFisicalYearIdbyDate(entity.EmployeeId.GetString(), tempDate);
                    if (!fiscalYearId.CheckNullOrEmpty())
                    {
                        entity.FiscalYearId = fiscalYearId.GetGuid();
                    }
                }
                list.Add(entity);
            }
            return list;
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

            string strSql = string.Format(@"SELECT   Planemp.FiscalYearId
                                            FROM     AnnualLeavePlanemPloyee AS Planemp
                                                     LEFT JOIN FiscalYear
                                                       ON Planemp.FiscalYearId = FiscalYear.FiscalYearId
                                            WHERE    Planemp.EmployeeId = '{0}'
                                                     AND Planemp.BeginDate <= '{1}'
                                                     AND Planemp.EndDate >= '{1}'
                                                     AND Planemp.Flag = '1'
                                            ORDER BY FiscalYear.[Year] DESC", pEmployeeId, pDate.ToDateFormatString());
            dt = HRHelper.ExecuteDataTable(strSql); ;
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0][0].ToString();
            }
            return string.Empty;
        }


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

            string empName = empSer.GetEmployeeNameById(pEmployeeId);

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
                        else
                        {
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



            return errorMsg;
        }


        /// <summary>
        /// 校验与请假、年假、出差申请、出差登记、特休、调休时间不能重复
        /// (出差申请专用，HR与ESS共用)
        /// </summary>
        /// <param name="pEmployeeId">员工ID</param>
        /// <param name="pBeginDate">开始日期</param>
        /// <param name="pBeginTime">开始时间</param>
        /// <param name="pEndDate">结束日期</param>
        /// <param name="pEndTime">结束时间</param>
        /// <param name="pEntityType">请假类型（出差申请）</param>
        /// <param name="pIsNew">此值已弃用</param>
        /// <param name="pGuid">需要排除的请假ID</param>
        /// <returns>重复信息</returns>
        public virtual string CheckAllTime(string pEmployeeId, DateTime pBeginDate, string pBeginTime, DateTime pEndDate, string pEndTime, CheckEntityType pEntityType, bool pIsNew, string pGuid)
        {
            DataTable dt = new DataTable();
            string errorMsg = string.Empty;
            DateTime tempBegin = DateTime.MinValue;
            DateTime tempEnd = DateTime.MinValue;
            DateTime begin = DateTime.MinValue;
            DateTime end = DateTime.MinValue;
            string typeId = string.Empty;
            DataTable dtApply = new DataTable();
            DataTable dtBusiness = new DataTable();
            //using (IConnectionService conService = Factory.GetService<IConnectionService>())
            //{
            //    IDbCommand cmd = conService.CreateDbCommand();
            StringBuilder sb = new StringBuilder();

            #region 查询SQL
            //年假
            sb.AppendFormat(@"SELECT EmployeeId,
                                           BeginDate,
                                           BeginTime,
                                           EndDate,
                                           EndTime,
                                           '401'      AS AttendanceTypeId
                                    FROM   AnnualLeaveRegisterInfo
                                    WHERE  IsRevoke = '0'
                                           AND AnnualLeaveRegisterId NOT IN (SELECT AnnualLeaveRegisterId
                                                                             FROM   AnnualLeaveRegister
                                                                             WHERE  StateId = 'PlanState_003'
                                                                                    AND ApproveResultId = 'OperatorResult_002')
                                           AND EmployeeId = '{0}'
                                           AND BeginDate >= '{1}'
                                           AND BeginDate <= '{2}' "
                , pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());
            if (pEntityType == CheckEntityType.AnnualLeave && !pGuid.CheckNullOrEmpty())
            {
                sb.AppendFormat(" AND AnnualLeaveRegisterId <> '{0}' ", pGuid);
            }
            sb.AppendLine("UNION ALL ");
            //请假
            sb.AppendFormat(@"SELECT EmployeeId,
                                           BeginDate,
                                           BeginTime,
                                           EndDate,
                                           EndTime,
                                           AttendanceTypeId
                                    FROM   AttendanceLeaveInfo
                                    WHERE  IsRevoke = '0'
                                           AND AttendanceLeaveId NOT IN (SELECT AttendanceLeaveId
                                                                         FROM   AttendanceLeave
                                                                         WHERE  StateId = 'PlanState_003'
                                                                                AND ApproveResultId = 'OperatorResult_002')
                                           AND EmployeeId = '{0}'
                                           AND BeginDate >= '{1}'
                                           AND BeginDate <= '{2}' "
, pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());
            if (pEntityType == CheckEntityType.Leave && !pGuid.CheckNullOrEmpty())
            {
                sb.AppendFormat(" AND AttendanceLeaveId <> '{0}' ", pGuid);
            }
            sb.AppendLine("UNION ALL ");

            sb.AppendFormat(@"SELECT daily.EmployeeId,
                                           daily.BeginDate,
                                           daily.BeginTime,
                                           daily.EndDate,
                                           daily.EndTime,
                                           '406'      AS AttendanceTypeId
                                    FROM   AttendanceotRest rest
                                    left join AttendanceOTRestDaily daily  on rest.AttendanceOverTimeRestId = daily.AttendanceOverTimeRestId
                                    WHERE  rest.AttendanceOvertimeRestId NOT IN (SELECT AttendanceOvertimeRestId
                                                                            FROM   AttendanceotRest
                                                                            WHERE  StateId IN ('PlanState_003','PlanState_004')
                                                                                   AND ApproveResultId = 'OperatorResult_002')
                                           AND daily.EmployeeId = '{0}'
                                           AND daily.BeginDate >= '{1}'
                                           AND daily.BeginDate <= '{2}'
                                           AND daily.IsRevoke = 0"
, pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());

            if (pEntityType == CheckEntityType.OTRest && !pGuid.CheckNullOrEmpty())
            {
                sb.AppendFormat(" AND rest.AttendanceOverTimeRestId <> '{0}' ", pGuid);
            }

            // 20101106 added by jiangpeng for 增加对特休假的校验
            sb.AppendLine("UNION ALL ");
            sb.AppendFormat(@"SELECT EmployeeId,
                                       BeginDate,
                                       BeginTime,
                                       EndDate,
                                       EndTime,
                                       '408'      AS AttendanceTypeId
                                FROM   twalreGinfo
                                WHERE  twalreGid NOT IN (SELECT twalreGid
                                                         FROM   twalreg
                                                         WHERE  StateId = 'PlanState_003'
                                                                AND ApproveResultId = 'OperatorResult_002')
                                       AND EmployeeId = '{0}'
                                       AND BeginDate >= '{1}'
                                       AND BeginDate <= '{2}'
                                       AND IsRevoke = '0'"
, pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());
            if (pEntityType == CheckEntityType.TWALReg && !pGuid.CheckNullOrEmpty())
            {
                sb.AppendFormat(" AND TWALRegId <> '{0}' ", pGuid);
            }
            #endregion
            //cmd.CommandText = sb.ToString();
            //dt.Load(cmd.ExecuteReader());
            dt = HRHelper.ExecuteDataTable(sb.ToString());
            #region 出差登记
            //出差登记
            sb = new StringBuilder();
            sb.AppendFormat(@"SELECT Info.EmployeeId,
                                           Info.BeginDate,
                                           Info.BeginTime,
                                           Info.EndDate,
                                           Info.EndTime,
                                           reg.AttendanceTypeId
                                    FROM   BusinessRegisterInfo AS Info
                                           LEFT JOIN BusinessRegister AS reg
                                             ON Info.BusinessRegisterId = reg.BusinessRegisterId
                                    WHERE  Info.IsRevoke = '0'
                                           AND reg.BusinessRegisterId NOT IN (SELECT BusinessRegisterId
                                                                              FROM   BusinessRegister
                                                                              WHERE  StateId = 'PlanState_003'
                                                                                     AND ApproveResultId = 'OperatorResult_002')
                                           AND reg.EmployeeId = '{0}'
                                           AND Info.BeginDate >= '{1}'
                                           AND Info.BeginDate <= '{2}' "
, pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());
            if (pEntityType == CheckEntityType.Business && !pGuid.CheckNullOrEmpty())
            {
                sb.AppendFormat(" AND reg.BusinessRegisterId <> '{0}' ", pGuid);
            }
            #endregion
            //cmd.CommandText = sb.ToString();
            //dtBusiness.Load(cmd.ExecuteReader());
            dtBusiness = HRHelper.ExecuteDataTable(sb.ToString());

            #region 出差申请
            //出差申请
            sb = new StringBuilder();

            //20150528 modified for 29464 && A00-20150527007 by renping
            sb.AppendFormat(@"SELECT Person.EmployeeId,
                                           BusinessApply.BeginDate,
                                           BusinessApply.BeginTime,
                                           BusinessApply.EndDate,
                                           BusinessApply.EndTime,
                                           Isnull(BusinessApply.AttendanceTypeId,701)  AttendanceTypeId
                                    FROM   BusinessApplyPerson AS Person
                                           LEFT JOIN BusinessApply
                                             ON BusinessApply.BusinessApplyId = Person.BusinessApplyId
                                    WHERE  Person.EmployeeId = '{0}'
                                           AND Person.BusinessApplyId NOT IN (SELECT BusinessApplyId
                                                                              FROM   BusinessRegister  where businessApplyId is not null)
                                           AND Person.BusinessApplyId NOT IN (SELECT BusinessApplyId
                                                                              FROM   BusinessApply
                                                                              WHERE  StateId = 'PlanState_003'
                                                                                     AND ApproveResultId = 'OperatorResult_002')
                                           AND BusinessApply.EndDate >= '{1}'
                                           AND BusinessApply.BeginDate <= '{2}' "
, pEmployeeId, pBeginDate.AddDays(-1).ToDateFormatString(), pEndDate.AddDays(1).ToDateFormatString());
            if (pEntityType == CheckEntityType.Apply && !pGuid.CheckNullOrEmpty())
            {
                sb.AppendFormat(" AND person.BusinessApplyId <> '{0}' ", pGuid);
            }
            #endregion

            //cmd.CommandText = sb.ToString();
            dtApply = HRHelper.ExecuteDataTable(sb.ToString());
            //}
            begin = pBeginDate.AddTimeToDateTime(pBeginTime);
            end = pEndDate.AddTimeToDateTime(pEndTime);
            AttendanceTypeService typeSer = new AttendanceTypeService();
            DateTime beginDate = DateTime.MinValue;
            DateTime endDate = DateTime.MinValue;
            string strBegin = string.Empty;
            string strEnd = string.Empty;
            string strBeginTime = string.Empty;
            string strEndTime = string.Empty;
            if (dt != null && dt.Rows.Count > 0)
            {
                #region 判断请假重复
                foreach (DataRow row in dt.Rows)
                {
                    tempBegin = DateTime.MinValue;
                    tempEnd = DateTime.MinValue;
                    beginDate = DateTime.MinValue;
                    endDate = DateTime.MinValue;
                    typeId = row["AttendanceTypeId"] as string;
                    //strBegin = row["BeginDate"] as string;
                    //strEnd = row["EndDate"] as string;
                    strBegin = row["BeginDate"].ToString();
                    strEnd = row["EndDate"].ToString();
                    strBeginTime = row["BeginTime"] as string;
                    strEndTime = row["EndTime"] as string;
                    if (!typeId.CheckNullOrEmpty() && !strBegin.CheckNullOrEmpty() && !strEnd.CheckNullOrEmpty()
                        && !strBeginTime.CheckNullOrEmpty() && !strEndTime.CheckNullOrEmpty())
                    {
                        if (DateTime.TryParse(strBegin, out beginDate) && DateTime.TryParse(strEnd, out endDate))
                        {
                            if (DateTime.TryParse(beginDate.ToDateFormatString() + " " + strBeginTime, out tempBegin) &&
                                DateTime.TryParse(endDate.ToDateFormatString() + " " + strEndTime, out tempEnd))
                            {
                                if (!CheckTime(begin, end, tempBegin, tempEnd))
                                {
                                    EmployeeService empSer = new EmployeeService();
                                    string empName = empSer.GetEmployeeNameById(pEmployeeId);
                                    errorMsg += string.Format(Resources.AttendanceLeave_LeaveInfoRepeat, empName, tempBegin.ToDateTimeFormatString(false), tempEnd.ToDateTimeFormatString(false), typeSer.GetNameById(typeId));
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            //出差登记
            if (dtBusiness != null && dtBusiness.Rows.Count > 0)
            {
                #region 判断出差登记重复
                foreach (DataRow row in dtBusiness.Rows)
                {
                    tempBegin = DateTime.MinValue;
                    tempEnd = DateTime.MinValue;
                    beginDate = DateTime.MinValue;
                    endDate = DateTime.MinValue;
                    typeId = row["AttendanceTypeId"] as string;
                    //strBegin = row["BeginDate"] as string;
                    //strEnd = row["EndDate"] as string;
                    strBegin = row["BeginDate"].ToString();
                    strEnd = row["EndDate"].ToString();
                    strBeginTime = row["BeginTime"] as string;
                    strEndTime = row["EndTime"] as string;
                    EmployeeService empSer = new EmployeeService();
                    string empName = empSer.GetEmployeeNameById(pEmployeeId);
                    if (!typeId.CheckNullOrEmpty() && !strBegin.CheckNullOrEmpty() && !strEnd.CheckNullOrEmpty()
                        && !strBeginTime.CheckNullOrEmpty() && !strEndTime.CheckNullOrEmpty())
                    {
                        if (DateTime.TryParse(strBegin, out beginDate) && DateTime.TryParse(strEnd, out endDate))
                        {
                            if (DateTime.TryParse(beginDate.ToDateFormatString() + " " + strBeginTime, out tempBegin) &&
                                DateTime.TryParse(endDate.ToDateFormatString() + " " + strEndTime, out tempEnd))
                            {
                                if (!CheckTime(begin, end, tempBegin, tempEnd))
                                {
                                    errorMsg += string.Format(Resources.AttendanceLeave_LeaveInfoRepeat, empName, tempBegin.ToDateTimeFormatString(false), tempEnd.ToDateTimeFormatString(false), typeSer.GetNameById(typeId) + Resources.BusinessRegisterDisplayName);
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            //出差申请
            if (dtApply != null && dtApply.Rows.Count > 0)
            {
                #region 判断出差申请重复
                foreach (DataRow row in dtApply.Rows)
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
                    EmployeeService empSer = new EmployeeService();
                    string empName = empSer.GetEmployeeNameById(pEmployeeId);
                    if (!typeId.CheckNullOrEmpty() && !strBegin.CheckNullOrEmpty() && !strEnd.CheckNullOrEmpty()
                        && !strBeginTime.CheckNullOrEmpty() && !strEndTime.CheckNullOrEmpty())
                    {
                        if (DateTime.TryParse(strBegin, out beginDate) && DateTime.TryParse(strEnd, out endDate))
                        {
                            if (DateTime.TryParse(beginDate.ToDateFormatString() + " " + strBeginTime, out tempBegin) &&
                                DateTime.TryParse(endDate.ToDateFormatString() + " " + strEndTime, out tempEnd))
                            {
                                if (!CheckTime(begin, end, tempBegin, tempEnd))
                                {
                                    errorMsg += string.Format(Resources.AttendanceLeave_LeaveInfoRepeat, empName, tempBegin.ToDateTimeFormatString(false), tempEnd.ToDateTimeFormatString(false), typeSer.GetNameById(typeId) + Resources.BusinessApplyDisplayName);
                                }
                            }
                        }
                    }
                }
                #endregion
            }

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

            string corpId = employeeService.GetEmpFiledById(pEmployeeId, "CorporationId");
            ALPlanService planService = new ALPlanService();
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
                else if (paraDt.Rows.Count > 1)
                {
                    foreach (DataRow r in paraDt.Rows)
                    {
                        if (r["CorporationId"].ToString() == corpId)
                        {
                            if (r["AnnualLeaveUnitId"].ToString().Equals("AnnualLeaveUnit_003"))
                            {
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

            dt = HRHelper.ExecuteDataTable(strSql);

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
        /// <summary>
        /// 根据类型Id获取名称
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetAttendanceTypeNameById(string typeId)
        {

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

      
        #endregion

        #region 销假FW
        //单个保存前检查
        public async Task<String> CheckRevokeForAPI(RevokeLeaveForAPI enty)
        {
            if (enty.AttendanceLeaveInfoIds == null || enty.AttendanceLeaveInfoIds.Length == 0)
            {
                throw new Exception("請假明細不能為空");
            }
            if (enty.AttendanceTypeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("AttendanceTypeId");
            }
            string ids = "'" + Guid.Empty.ToString() + "'";
            for (int i = 0; i < enty.AttendanceLeaveInfoIds.Length; i++)
            {
                ids += ",'" + enty.AttendanceLeaveInfoIds[i] + "'";
            }
            DataTable dt = new DataTable();
            if (enty.AttendanceTypeId == "401")
            {
                dt = HRHelper.ExecuteDataTable(string.Format("select * from AnnualLeaveRegisterInfo where AnnualLeaveRegisterInfoId in ({0})", ids));
            }
            else if (enty.AttendanceTypeId == "406")
            {
                dt = HRHelper.ExecuteDataTable(string.Format("select * from AttendanceOTRestInfo where AttendanceOverTimeRestInfoId in ({0})", ids));
            }
            else
            {
                dt = HRHelper.ExecuteDataTable(string.Format(@"select * from AttendanceLeaveInfo where AttendanceTypeId='{0}'
and AttendanceLeaveInfoId in ({1})", enty.AttendanceTypeId, ids));
            }
            if (dt == null || dt.Rows.Count == 0)
            {
                throw new Exception("請假明細资料找不到");
            }
            string response = "";

            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "AT_XJ_01";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "attendanceLeaveInfoIds";
            parameter.Value = JsonConvert.SerializeObject(enty.AttendanceLeaveInfoIds);

            APIRequestParameter parameter1 = new APIRequestParameter();
            parameter1.Name = "attendanceTypeId";
            parameter1.Value = enty.AttendanceTypeId;

            callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter, parameter1 };

            string json = JsonConvert.SerializeObject(callServiceBindingModel);
            response = await HttpPostJsonHelper.PostJsonAsync(json);
            Dcms.HR.DataEntities.APIExResponse apiExResponse = JsonConvert.DeserializeObject<Dcms.HR.DataEntities.APIExResponse>(response);
            if (apiExResponse.State == "0" && !apiExResponse.Msg.CheckNullOrEmpty())
            {
                return apiExResponse.Msg;
            }
            if (apiExResponse.State == "-1")
            {
                throw new BusinessException(apiExResponse.Msg);
            }
            return "success";
        }

        public async Task<APIExResponse> SaveRevokeForAPI(RevokeLeaveForAPI enty)
        {
            if (enty.AttendanceLeaveInfoIds == null || enty.AttendanceLeaveInfoIds.Length == 0)
            {
                throw new Exception("請假明細不能為空");
            }
            if (enty.AttendanceTypeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("AttendanceTypeId");
            }
            if (enty.EssNo.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("EssNo");
            }
            //if (enty.EssType.CheckNullOrEmpty())
            //{
            //    throw new ArgumentNullException("EssType");
            //}
            EmployeeService empser = new EmployeeService();
            if (enty.AuditEmployeeCode.CheckNullOrEmpty()) {

                throw new ArgumentNullException("AuditEmployeeCode");
            }
            string empId = empser.GetEmpIdByCode(enty.AuditEmployeeCode);
            if (empId.CheckNullOrEmpty())
            {
                throw new BusinessRuleException("审核人" + enty.AuditEmployeeCode + "在HR中不存在！");
            }

            string ids = "'" + Guid.Empty.ToString() + "'";
            for (int i = 0; i < enty.AttendanceLeaveInfoIds.Length; i++)
            {
                ids += ",'" + enty.AttendanceLeaveInfoIds[i] + "'";
            }
            if (enty.EssType.CheckNullOrEmpty()) {
                enty.EssType = "ATXJ";
            }

            DataTable dt = new DataTable();
            if (enty.AttendanceTypeId == "401")
            {
                dt = HRHelper.ExecuteDataTable(string.Format("select * from AnnualLeaveRegisterInfo where AnnualLeaveRegisterInfoId in ({0})", ids));
            }
            else if (enty.AttendanceTypeId == "406")
            {
                dt = HRHelper.ExecuteDataTable(string.Format("select * from AttendanceOTRestInfo where AttendanceOverTimeRestInfoId in ({0})", ids));
            }
            else
            {
                dt = HRHelper.ExecuteDataTable(string.Format(@"select * from AttendanceLeaveInfo where AttendanceTypeId='{0}'
and AttendanceLeaveInfoId in ({1})", enty.AttendanceTypeId, ids));
            }
            if (dt == null || dt.Rows.Count == 0)
            {
                throw new Exception("請假明細资料找不到");
            }

            string response = "";

            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "AT_XJ_02";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "attendanceLeaveInfoIds";
            parameter.Value = JsonConvert.SerializeObject(enty.AttendanceLeaveInfoIds);

            APIRequestParameter parameter1 = new APIRequestParameter();
            parameter1.Name = "attendanceTypeId";
            parameter1.Value = enty.AttendanceTypeId;

            APIRequestParameter parameter2 = new APIRequestParameter();
            parameter2.Name = "formNumber";
            parameter2.Value = enty.EssNo;

            APIRequestParameter parameter3 = new APIRequestParameter();
            parameter3.Name = "formType";
            parameter3.Value = enty.EssType;


            APIRequestParameter parameter4 = new APIRequestParameter();
            parameter4.Name = "auditEmployeeCode";
            parameter4.Value = enty.AuditEmployeeCode;

            APIRequestParameter parameter5 = new APIRequestParameter();
            parameter5.Name = "auditResult";
            bool isAgree = true;
            if (!enty.AuditResult.CheckNullOrEmpty() && enty.AuditResult == false) {
                isAgree = false;
            }
            parameter5.Value = isAgree ;
            
            callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter, parameter1, parameter2, parameter3,parameter4,parameter5 };

            string json = JsonConvert.SerializeObject(callServiceBindingModel);
            response = await HttpPostJsonHelper.PostJsonAsync(json);
            //Dcms.HR.DataEntities.APIExResponse apiExResponse = JsonConvert.DeserializeObject<Dcms.HR.DataEntities.APIExResponse>(response);
            //if (apiExResponse.State == "0" && !apiExResponse.Msg.CheckNullOrEmpty())
            //{
            //    return apiExResponse.Msg;
            //}
            //if (apiExResponse.State == "-1")
            //{
            //    throw new BusinessException(apiExResponse.Msg);
            //}
            //return "success";
            APIExResponse aPIExResponse = JsonConvert.DeserializeObject<APIExResponse>(response);
            if (aPIExResponse != null)
            {
                return aPIExResponse;
            }
            else
            {
                throw new Exception(response);
            }
        }


        #endregion

        #region 批量

        public async Task<Dictionary<string, string>> MultiCheckForAPI(AttendanceLeaveForAPI[] formEntities)
        {
            StringBuilder sbError = new StringBuilder();
            string msg = string.Empty;
            Dictionary<string, string> dicCheck = new Dictionary<string, string>();
            int i = 0;
            List<string> liNo = new List<string>();
            foreach (AttendanceLeaveForAPI enty in formEntities)
            {
                i++;
                if (enty.EssNo.CheckNullOrEmpty())
                {
                    if (dicCheck == null) dicCheck = new Dictionary<string, string>();
                    dicCheck.Add(i.ToString(), ("EssNo 不能为空"));
                }
                else {
                    if (!liNo.Contains(enty.EssNo))
                    {
                        liNo.Add(enty.EssNo);
                    }
                    else {
                        dicCheck.Add(i.ToString(), ("EssNo "+enty.EssNo+" 编号必须唯一"));
                    }
                }
                if (dicCheck != null && dicCheck.Keys.Count > 0)
                {
                    continue;
                }
            }
            if (dicCheck != null && dicCheck.Count > 0)
            {
                return dicCheck;
            }
            i = 0;
            foreach (AttendanceLeaveForAPI enty in formEntities)
            {
                i++;
                EmployeeService employeeService = new EmployeeService();

                string empId = employeeService.GetEmpIdByCode(enty.EmployeeCode);
                if (empId.CheckNullOrEmpty())
                {
                    if (dicCheck == null) dicCheck = new Dictionary<string, string>();
                    dicCheck.Add(enty.EssNo, ("EmpCode 找不到对应的员工"));
                }
                enty.EmployeeId = empId.GetGuid();
                enty.AttendanceLeaveId = Guid.NewGuid();
                string str = this.CheckValue(enty);
                if (!str.CheckNullOrEmpty())
                {
                    if (dicCheck == null) dicCheck = new Dictionary<string, string>();
                    dicCheck.Add(i.ToString(), str);
                }

                if (dicCheck != null && dicCheck.Keys.Count > 0)
                {
                    continue;
                }

                AttendanceLeaveForAPI[] arrayEntity = new AttendanceLeaveForAPI[] { enty };
                //檢查請假時間與出差申請時間是否重複
                dicCheck = this.CheckBusinessApplyTime(arrayEntity);
                if (dicCheck != null && dicCheck.Count > 0)
                {
                    continue;
                }
            }

            if (dicCheck != null && dicCheck.Count > 0)
            {
                return dicCheck;
            }
            string response = "";

            List<AttendanceLeaveForAPI> listQJ = new List<AttendanceLeaveForAPI>();
            List<AttendanceLeaveForAPI> list406 = new List<AttendanceLeaveForAPI>();
            List<AttendanceLeaveForAPI> list401 = new List<AttendanceLeaveForAPI>();
            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();

            foreach (AttendanceLeaveForAPI formEntity in formEntities)
            {
                if (formEntity.AttendanceTypeId.Equals("401"))
                {
                    list401.Add(formEntity);
                }
                else if (formEntity.AttendanceTypeId.Equals("406"))
                {
                    list406.Add(formEntity);
                }
                else
                {
                    listQJ.Add(formEntity);
                }
            }
            //检查
            if (list401.Count > 0)
            {
                //用异常方式返回的
                List<AnnualLeaveRegister> list = this.ChangeToALRegisterEntity(list401.ToArray());
                foreach (AnnualLeaveRegister aR in list)
                {
                    aR.IsEss = true;
                    aR.IsFromEss = true;
                    aR.Flag = true;
                    aR.EssType = string.Empty;
                    aR.EssNo = string.Empty;
                    aR.IsCheckAtType = true;
                    aR.StateId = Constants.PS02;
                    aR.CreateDate = DateTime.Now;
                    aR.CreateBy = Constants.SYSTEMGUID_USER_ADMINISTRATOR.GetGuid();
                    aR.LastModifiedDate = DateTime.Now;
                    aR.LastModifiedBy = Constants.SYSTEMGUID_USER_ADMINISTRATOR.GetGuid();
                    aR.FoundOperationDate = DateTime.Now;
                    aR.FoundUserId = Constants.SYSTEMGUID_USER_ADMINISTRATOR.GetGuid();
                    aR.OwnerId = aR.EmployeeId.GetString();
                    aR.Flag = true;
                    aR.IsEss = true;
                    aR.IsFromEss = true;
                    aR.EssType = "ATQJ";
                    aR.EssNo = "";
                    if (!aR.EmployeeId.CheckNullOrEmpty())
                    {
                        EmployeeService empSer = new EmployeeService();
                        string corporationId = empSer.GetEmpFiledById(aR.EmployeeId.GetString(), "CorporationId");
                        if (!corporationId.CheckNullOrEmpty())
                        {
                            aR.CorporationId = corporationId.GetGuid();
                        }

                        DateTime tempDate = aR.BeginDate;

                        string fiscalYearId = this.GetFisicalYearIdbyDate(aR.EmployeeId.GetString(), tempDate);
                        if (!fiscalYearId.CheckNullOrEmpty())
                        {
                            aR.FiscalYearId = fiscalYearId.GetGuid();
                        }
                    }
                }
                callServiceBindingModel.RequestCode = "API_AT_014";
                APIRequestParameter parameter = new APIRequestParameter();
                parameter.Name = "formEntities";
                parameter.Value = JsonConvert.SerializeObject(list.ToArray());
                callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };
                string json = JsonConvert.SerializeObject(callServiceBindingModel);
                response = await HttpPostJsonHelper.PostJsonAsync(json);
                Dcms.HR.DataEntities.APIExResponse apiExResponse401 = JsonConvert.DeserializeObject<Dcms.HR.DataEntities.APIExResponse>(response);
                if (apiExResponse401.State == "-1")
                {
                    throw new BusinessException(apiExResponse401.Msg);
                }
                else if (apiExResponse401.State == "0" && apiExResponse401.ResultValue.CheckNullOrEmpty())
                {
                    throw new BusinessException(apiExResponse401.ResultValue.ToString());
                }
            }
            if (list406.Count > 0)
            {
                //用异常方式返回的
                List<AttendanceOverTimeRest> list = this.ChangeToOTRestEntity(list406.ToArray());

                callServiceBindingModel.RequestCode = "API_AT_013";

                APIRequestParameter parameter = new APIRequestParameter();
                parameter.Name = "formEntities";
                parameter.Value = JsonConvert.SerializeObject(list.ToArray());

                callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

                string json = JsonConvert.SerializeObject(callServiceBindingModel);
                response = await HttpPostJsonHelper.PostJsonAsync(json);
                Dcms.HR.DataEntities.APIExResponse apiExResponse406 = JsonConvert.DeserializeObject<Dcms.HR.DataEntities.APIExResponse>(response);
                if (apiExResponse406.State == "-1")
                {
                    throw new BusinessException(apiExResponse406.Msg);
                }
                else if (apiExResponse406.State == "0" && !apiExResponse406.ResultValue.CheckNullOrEmpty())
                {
                    throw new BusinessException(apiExResponse406.ResultValue.ToString());
                }
            }
            if (listQJ.Count > 0)
            {
                List<AttendanceLeave> list = ChangeToLeaveEntity(listQJ.ToArray(), true, string.Empty, string.Empty);
                //int j = 0;
                //foreach (AttendanceLeave leave in list)
                //{
                //    j++;
                //    string[] tmpMsg = null;
                //    string msgInfo = this.GetSalaryTimes(leave);
                //    if (msgInfo.IndexOf("false") > -1)
                //    {
                //        tmpMsg = msgInfo.Split(',');
                //        if (dicCheck == null) dicCheck = new Dictionary<int, string>();
                //        dicCheck.Add(j, (string.Format("工号为 {0} 的员工当前请假总时数为 {1}，已超过最大值 {2}", tmpMsg[0], tmpMsg[2], tmpMsg[1])));
                //    }
                //}
                //if (dicCheck != null && dicCheck.Keys.Count > 0)
                //{
                //    return dicCheck;
                //}

                //j = 0;
                //foreach (AttendanceLeave leave in list)
                //{
                //    j++;
                //    msg = "";
                //    try
                //    {
                //        this.CheckForESS(leave);
                //    }
                //    catch (Exception ex)
                //    {
                //        msg = ex.Message;
                //    }
                //    if (!msg.CheckNullOrEmpty())
                //    {
                //        if (dicCheck == null) dicCheck = new Dictionary<int, string>();
                //        dicCheck.Add(j, msg);
                //    }
                //}
                //if (dicCheck != null && dicCheck.Keys.Count > 0)
                //{
                //    return dicCheck;
                //}
                callServiceBindingModel.RequestCode = "API_AT_011";

                APIRequestParameter parameter = new APIRequestParameter();
                parameter.Name = "formEntities";
                parameter.Value = JsonConvert.SerializeObject(list.ToArray());

                callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

                string json = JsonConvert.SerializeObject(callServiceBindingModel);
                response = await HttpPostJsonHelper.PostJsonAsync(json);
                Dcms.HR.DataEntities.APIExResponse apiExResponse = JsonConvert.DeserializeObject<Dcms.HR.DataEntities.APIExResponse>(response);
                if (apiExResponse.State == "-1")
                {
                    throw new BusinessException(apiExResponse.Msg);
                }
                else if (apiExResponse.State == "0" && !apiExResponse.ResultValue.CheckNullOrEmpty())
                {
                    throw new BusinessException(apiExResponse.ResultValue.ToString());
                }
            }

            return null;
        }



        public async Task<string> MultiSaveForAPI( AttendanceLeaveForAPI[] formEntities)
        {
            EmployeeService empser = new EmployeeService();
            string empId = "";

            string formType = "ATQJ";
            StringBuilder sbError = new StringBuilder();
            string msg = string.Empty; string json = string.Empty;
            Dictionary<string, string> dicCheck = new Dictionary<string, string>();
            string response = "";
            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
           // Dictionary<string, string> dicCheck = new Dictionary<string, string>();
            int i = 0;
            List<string> liNo = new List<string>();
            foreach (AttendanceLeaveForAPI enty in formEntities)
            {
                i++;
                if (enty.EssNo.CheckNullOrEmpty())
                {
                    if (dicCheck == null) dicCheck = new Dictionary<string, string>();
                    dicCheck.Add(i.ToString(), ("EssNo 不能为空"));
                }
                else
                {
                    if (!liNo.Contains(enty.EssNo))
                    {
                        liNo.Add(enty.EssNo);
                    }
                    else
                    {
                        dicCheck.Add(i.ToString(), ("EssNo " + enty.EssNo + " 编号必须唯一"));
                    }
                }
                if (dicCheck != null && dicCheck.Keys.Count > 0)
                {
                    continue;
                }
            }
            StringBuilder errors = new StringBuilder();
            if (dicCheck != null && dicCheck.Keys.Count > 0)
            {
                foreach (string key in dicCheck.Keys)
                {
                    errors.Append(string.Format("{0}:{1}", key.ToString(), dicCheck[key].ToString()));
                }
                throw new BusinessRuleException(errors.ToString());

            }
            i = 0;
            foreach (AttendanceLeaveForAPI enty in formEntities)
            {
                i++;
                EmployeeService employeeService = new EmployeeService();

                 empId = employeeService.GetEmpIdByCode(enty.EmployeeCode);
                if (dicCheck == null) dicCheck = new Dictionary<string, string>();
                if (empId.CheckNullOrEmpty())
                {
                    dicCheck.Add(enty.EssNo, ("EmployeeCode " + enty.EmployeeCode + " 找不到对应的员工"));
                    continue;
                }
                enty.EmployeeId = empId.GetGuid();
                if (enty.AuditEmployeeCode.CheckNullOrEmpty())
                {
                    dicCheck.Add(enty.EssNo, ("AuditEmployeeCode 审核人工号不能为空！"));
                    continue;
                }
                empId = empser.GetEmpIdByCode(enty.AuditEmployeeCode);
                if (empId.CheckNullOrEmpty())
                {
                    dicCheck.Add(enty.EssNo, "审核人" + enty.AuditEmployeeCode + "在HR中不存在！");
                    continue;
                }
                else
                {
                    enty.ApproveEmployeeId = empId.GetGuid();
                }
                if (!enty.AuditResult.CheckNullOrEmpty() && enty.AuditResult == false)
                {
                    enty.ApproveResultId = Constants.AuditRefuse;
                }
                else
                {
                    enty.ApproveResultId = Constants.AuditAgree;
                }
                if (enty.EssType.CheckNullOrEmpty()) {
                    enty.EssType = formType;
                }
                enty.AttendanceLeaveId = Guid.NewGuid();
                string str = this.CheckValue(enty);
                if (!str.CheckNullOrEmpty())
                {
                    if (dicCheck == null)
                    {
                        dicCheck = new Dictionary<string, string>();
                    }
                    dicCheck.Add(enty.EssNo, str);
                    continue;
                }

                AttendanceLeaveForAPI[] arrayEntity = new AttendanceLeaveForAPI[] { enty };
                //檢查請假時間與出差申請時間是否重複
                dicCheck = this.CheckBusinessApplyTime(arrayEntity);
                if (dicCheck != null && dicCheck.Count > 0)
                {
                    continue;
                }
            }
          
            if (dicCheck != null && dicCheck.Keys.Count > 0)
            {
                foreach (string key in dicCheck.Keys)
                {
                    errors.Append(string.Format("{0}:{1}", key.ToString(), dicCheck[key].ToString()));
                }
                throw new BusinessRuleException(errors.ToString());

            }

            List<AttendanceLeaveForAPI> listQJ = new List<AttendanceLeaveForAPI>();
            List<AttendanceLeaveForAPI> list406 = new List<AttendanceLeaveForAPI>();
            List<AttendanceLeaveForAPI> list401 = new List<AttendanceLeaveForAPI>();

            foreach (AttendanceLeaveForAPI formEntity in formEntities)
            {
                if (formEntity.AttendanceTypeId.Equals("401"))
                {
                    list401.Add(formEntity);
                }
                else if (formEntity.AttendanceTypeId.Equals("406"))
                {
                    list406.Add(formEntity);
                }
                else
                {
                    listQJ.Add(formEntity);
                }
            }
            //检查
            if (list401.Count > 0)
            {
                //用异常方式返回的
                List<AnnualLeaveRegister> list = this.ChangeToALRegisterEntity(list401.ToArray());
                foreach (AnnualLeaveRegister aR in list)
                {
                    aR.IsEss = true;
                    aR.IsFromEss = true;
                    aR.Flag = true;
                    aR.IsCheckAtType = true;
                    aR.StateId = Constants.PS02;
                    aR.CreateDate = DateTime.Now;
                    aR.CreateBy = Constants.SYSTEMGUID_USER_ADMINISTRATOR.GetGuid();
                    aR.LastModifiedDate = DateTime.Now;
                    aR.LastModifiedBy = Constants.SYSTEMGUID_USER_ADMINISTRATOR.GetGuid();
                    aR.FoundOperationDate = DateTime.Now;
                    aR.FoundUserId = Constants.SYSTEMGUID_USER_ADMINISTRATOR.GetGuid();
                    aR.OwnerId = aR.EmployeeId.GetString();
                    aR.Flag = true;
                    aR.IsEss = true;
                    aR.IsFromEss = true;
                    if (!aR.EmployeeId.CheckNullOrEmpty())
                    {
                        EmployeeService empSer = new EmployeeService();
                        string corporationId = empSer.GetEmpFiledById(aR.EmployeeId.GetString(), "CorporationId");
                        if (!corporationId.CheckNullOrEmpty())
                        {
                            aR.CorporationId = corporationId.GetGuid();
                        }

                        DateTime tempDate = aR.BeginDate;

                        string fiscalYearId = this.GetFisicalYearIdbyDate(aR.EmployeeId.GetString(), tempDate);
                        if (!fiscalYearId.CheckNullOrEmpty())
                        {
                            aR.FiscalYearId = fiscalYearId.GetGuid();
                        }
                    }
                }
                callServiceBindingModel.RequestCode = "API_AT_014";
                APIRequestParameter parameter = new APIRequestParameter();
                parameter.Name = "formEntities";
                parameter.Value = JsonConvert.SerializeObject(list.ToArray());
             
                callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };
                json = JsonConvert.SerializeObject(callServiceBindingModel);
                response = await HttpPostJsonHelper.PostJsonAsync(json);
                Dcms.HR.DataEntities.APIExResponse apiExResponse401 = JsonConvert.DeserializeObject<Dcms.HR.DataEntities.APIExResponse>(response);
                if (apiExResponse401.State == "-1")
                {
                    throw new BusinessException(apiExResponse401.Msg);
                }
                else if (apiExResponse401.State == "0" && apiExResponse401.ResultValue.CheckNullOrEmpty())
                {
                    throw new BusinessException(apiExResponse401.ResultValue.ToString());
                }
            }
            if (list406.Count > 0)
            {
                //用异常方式返回的
                List<AttendanceOverTimeRest> list = this.ChangeToOTRestEntity(list406.ToArray());
                foreach (AttendanceOverTimeRest enty in list)
                {
                    enty.IsEss = true;
                    enty.IsFromEss = true;
                }
                callServiceBindingModel.RequestCode = "API_AT_013";

                APIRequestParameter parameter = new APIRequestParameter();
                parameter.Name = "formEntities";
                parameter.Value = JsonConvert.SerializeObject(list.ToArray());
             

                callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

                json = JsonConvert.SerializeObject(callServiceBindingModel);
                response = await HttpPostJsonHelper.PostJsonAsync(json);
                Dcms.HR.DataEntities.APIExResponse apiExResponse406 = JsonConvert.DeserializeObject<Dcms.HR.DataEntities.APIExResponse>(response);
                if (apiExResponse406.State == "-1")
                {
                    throw new BusinessException(apiExResponse406.Msg);
                }
                else if (apiExResponse406.State == "0" && !apiExResponse406.ResultValue.CheckNullOrEmpty())
                {
                    throw new BusinessException(apiExResponse406.ResultValue.ToString());
                }
            }
            if (listQJ.Count > 0)
            {
                List<AttendanceLeave> list = ChangeToLeaveEntity(listQJ.ToArray(), true, string.Empty, string.Empty);
                int j = 0;
                foreach (AttendanceLeave leave in list)
                {
                    j++;
                    string[] tmpMsg = null;
                    string msgInfo = this.GetSalaryTimes(leave);
                    if (msgInfo.IndexOf("false") > -1)
                    {
                        tmpMsg = msgInfo.Split(',');
                        errors.Append(string.Format("{3}: 工号为 {0} 的员工当前请假总时数为 {1}，已超过最大值 {2}", tmpMsg[0], tmpMsg[2], tmpMsg[1], j.ToString()));
                    }
                }
                if (errors.Length > 0)
                {
                    return errors.ToString();
                }
                errors.Length = 0;
                foreach (AttendanceLeave enty in list)
                {
                    enty.IsEss = true;
                    enty.IsFromEss = true;
                    enty.AttendanceLeaveId = Guid.NewGuid();
                }
                callServiceBindingModel.RequestCode = "API_AT_011";

                APIRequestParameter parameter = new APIRequestParameter();
                parameter.Name = "formEntities";
                parameter.Value = JsonConvert.SerializeObject(list.ToArray());


                callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

                json = JsonConvert.SerializeObject(callServiceBindingModel);
                response = await HttpPostJsonHelper.PostJsonAsync(json);
                Dcms.HR.DataEntities.APIExResponse apiExRes = JsonConvert.DeserializeObject<Dcms.HR.DataEntities.APIExResponse>(response);
                if (apiExRes.State == "-1")
                {
                    throw new BusinessException(apiExRes.Msg);
                }
                else if (apiExRes.State == "0" && !apiExRes.ResultValue.CheckNullOrEmpty())
                {
                    throw new BusinessException(apiExRes.ResultValue.ToString());
                }
            }

            //保存
            if (list401.Count > 0)
            {
                List<AnnualLeaveRegister> list = this.ChangeToALRegisterEntity(list401.ToArray());
                callServiceBindingModel.RequestCode = "API_AT_018";
                APIRequestParameter parameter = new APIRequestParameter();
                parameter.Name = "formEntities";
                parameter.Value = JsonConvert.SerializeObject(list);
              

                callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

               
                json = JsonConvert.SerializeObject(callServiceBindingModel);
                response = await HttpPostJsonHelper.PostJsonAsync(json);
            }
            if (list406.Count > 0)
            {
                List<AttendanceOverTimeRest> list = this.ChangeToOTRestEntity(list406.ToArray());
                callServiceBindingModel.RequestCode = "API_AT_017";

                APIRequestParameter parameter = new APIRequestParameter();
                parameter.Name = "formEntities";
                parameter.Value = JsonConvert.SerializeObject(list);
              
                callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

                json = JsonConvert.SerializeObject(callServiceBindingModel);
                response = await HttpPostJsonHelper.PostJsonAsync(json);
            }
            if (listQJ.Count > 0)
            {
                List<AttendanceLeave> list = this.ChangeToLeaveEntity(listQJ.ToArray(), true, string.Empty, string.Empty);

                callServiceBindingModel.RequestCode = "API_AT_016";

                APIRequestParameter parameter = new APIRequestParameter();
                parameter.Name = "formEntities";
                parameter.Value = JsonConvert.SerializeObject(list);

             

                callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

                json = JsonConvert.SerializeObject(callServiceBindingModel);
                response = await HttpPostJsonHelper.PostJsonAsync(json);
            }
            /*{"State":"-1","Msg":"System.Exception: Dcms.Common.BusinessRuleException: 员工罗江:2020-06-24 09:00至2020-06-24 12:00已存在病假记录;\r\n   在 Dcms.HR.Services.AttendanceOverTimeRestService.DealDailyTimes(AttendanceOverTimeRest adjust)\r\n   在 Dcms.HR.Services.AttendanceOverTimeRestService.SetOTRest(AttendanceOverTimeRest& rest)\r\n   在 Dcms.HR.Services.AttendanceOverTimeRestService.CheckForESS(AttendanceOverTimeRest pOTRest)\r\n   在 Dcms.HR.Services.ExtendItemService.CheckForAT406ForAPI(AttendanceOverTimeRest formEntity)\r\n   在 Dcms.HR.Services.ExtendItemService.InvokeHRServiceEx(String pInput)","ResultType":null,"ResultValue":null}*/
            Dcms.HR.DataEntities.APIExResponse apiExResponse = JsonConvert.DeserializeObject<Dcms.HR.DataEntities.APIExResponse>(response);
            if (apiExResponse.State == "-1")
            {
                throw new BusinessException(apiExResponse.Msg);
            }
            else
            {
                return "success";
            }
        }

        #endregion
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
