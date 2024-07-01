using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.Common.Core;
using Dcms.Common.Services;
using Dcms.Common.Torridity.Query;
using Dcms.HR;
using Dcms.HR.Business.Implement.Properties;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BQHRWebApi.Service { 
    public class BusinessApplyService : HRService
    {
        public override async void Save(DataEntity[] entities)
        {
            foreach (BusinessApplyForAPI enty in entities)
            {
                DataTable dtCorp = GetCorpInfoByCode(enty.CorpCode);

                if (dtCorp == null && dtCorp.Rows.Count > 0)
                {
                    enty.CorporationId = dtCorp.Rows[0]["CorporationId"].ToString().GetGuid();
                }
                else
                {
                    throw new BusinessRuleException("找不到对应的公司:" + enty.CorpCode);
                }
                foreach (BusinessApplyPersonForAPI person in enty.Persons)
                {
                    DataTable dtEmp = GetEmpInfoByCode(person.EmployeeCode);

                    if (dtEmp == null && dtEmp.Rows.Count > 0) {
                        person.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                    }
                    else
                    {
                        throw new BusinessRuleException("找不到对应的员工:" + person.EmployeeCode);
                    }
                    if (!person.DeputyEmployeeCode.CheckNullOrEmpty())
                    {
                        dtEmp = GetEmpInfoByCode(person.DeputyEmployeeCode);
                        if (dtEmp == null && dtEmp.Rows.Count > 0)
                        {
                            person.DeputyEmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                        }
                        else
                        {
                            throw new BusinessRuleException("找不到对应的员工:" + person.DeputyEmployeeCode);
                        }
                    }
                }

                //foreach (BusinessApplyAttendanceForAPI person in enty.Attendances)
                //{
                //    DataTable dtEmp = GetEmpInfoByCode(person.EmployeeCode);

                //    if (dtEmp == null && dtEmp.Rows.Count > 0)
                //    {
                //        person.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                //    }
                //    else
                //    {
                //        throw new BusinessRuleException("找不到对应的员工:" + person.EmployeeCode);
                //    }
                //}
            }
            BusinessApply[] entys = HRHelper.WebAPIEntitysToDataEntitys<BusinessApply>(entities).ToArray();

            foreach (BusinessApply business in entys)
            {
                business.FoundDate = DateTime.Now.Date;
                business.StateId = Constants.PS03;
                business.BusinessApplyId =Guid.NewGuid();

            }
            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_CC_02";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "attendanceCollects";
            parameter.Value = JsonConvert.SerializeObject(entities);

            callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

            string json = JsonConvert.SerializeObject(callServiceBindingModel);
            string response = await HttpPostJsonHelper.PostJsonAsync(json);

            APIExResponse aPIExResponse = JsonConvert.DeserializeObject<APIExResponse>(response);

            if (aPIExResponse != null)
            {
                if (aPIExResponse.State != "0" )
                {
                    throw new BusinessException(aPIExResponse.Msg);
                }
                else if (aPIExResponse.State == "0" && !aPIExResponse.ResultValue.CheckNullOrEmpty())
                {
                    throw new BusinessException(aPIExResponse.ResultValue.ToString());
                }
            }
            else
            {
                throw new Exception(response);
            }
        }




        private string CheckData(BusinessApplyForAPI enty)
        {
            StringBuilder msg = new StringBuilder();
            DataTable dtCorp = GetCorpInfoByCode(enty.CorpCode);
            if (dtCorp != null && dtCorp.Rows.Count > 0)
            {
                enty.CorporationId = dtCorp.Rows[0]["CorporationId"].ToString().GetGuid();
            }
            else
            {
                msg.Append("找不到对应的公司:" + enty.CorpCode);
            }
            DataTable dtEmp = GetEmpInfoByCode(enty.FoundEmpCode);

            if (dtEmp != null && dtEmp.Rows.Count > 0)
            {
                enty.FoundEmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
            }
            else
            {
                msg.Append("找不到对应的员工:" + enty.FoundEmpCode);
            }
            DateTime tmpTime = DateTime.Now;
            DateTime tmpTime1 = DateTime.Now;
            if (!DateTime.TryParse(string.Format("{0} {1}", enty.BeginDate.ToShortDateString(), enty.BeginTime), out tmpTime))
            {
                msg.Append("开始日期和开始时间不是正确的日期时间格式");
            }
            if (!DateTime.TryParse(string.Format("{0} {1}", enty.EndDate.ToShortDateString(), enty.EndTime), out tmpTime1))
            {
                msg.Append("结束日期和结束时间不是正确的日期时间格式");
            }

            if (tmpTime1 < tmpTime)
            {
                msg.Append("请假结束不能小于请假开始");
            }
            // 出差人员不能为空
            if (enty.Persons.Count == 0)
            {
                msg.Append(Resources.Business_PersonIsNotNull);
            }
            else
            {
                List<string> listIn = new List<string>();
                //出差人员集合
                List<string> listOut = new List<string>();
                foreach (BusinessApplyPersonForAPI person in enty.Persons)
                {
                    dtEmp = GetEmpInfoByCode(person.EmployeeCode);

                    if (dtEmp != null && dtEmp.Rows.Count > 0)
                    {
                        person.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                        if (listOut.Contains(dtEmp.Rows[0]["EmployeeId"].ToString()))
                        {
                            msg.Append(string.Format(Resources.Business_BusinessEmpRepeat, person.EmployeeCode));
                        }
                        else
                        {
                            listOut.Add(dtEmp.Rows[0]["EmployeeId"].ToString());
                        }
                    }
                    else
                    {
                        throw new BusinessRuleException("找不到对应的员工:" + person.EmployeeCode);
                    }
                    if (!person.DeputyEmployeeCode.CheckNullOrEmpty())
                    {
                        dtEmp = GetEmpInfoByCode(person.DeputyEmployeeCode);
                        if (dtEmp != null && dtEmp.Rows.Count > 0)
                        {
                            person.DeputyEmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                            if (listOut.Contains(dtEmp.Rows[0]["EmployeeId"].ToString()))
                            {
                                //ec.Add(new Exception(string.Format("{0} 出差中不能做为代理人员", (Factory.GetService<IEmployeeServiceEx>().Read(item.DeputyEmployeeId)).CnName)));
                                msg.Append(string.Format(Resources.Business_DeputyEmpError, person.DeputyEmployeeCode));
                            }
                            else
                            {
                                if (!person.DeputyEmployeeId.CheckNullOrEmpty() && !listIn.Contains(dtEmp.Rows[0]["EmployeeId"].ToString()))
                                {
                                    listIn.Add(dtEmp.Rows[0]["EmployeeId"].ToString());
                                }
                            }
                        }
                        else
                        {
                            msg.Append("找不到对应的员工:" + person.DeputyEmployeeCode);
                        }
                    }
                }
            }

            #region 出差安排检查
            #region 检验出差安排不能为空
            if (enty.Schedules.Count == 0)
            {
                string schedules = Resources.Error_SchedulesDate.Substring(0, 4);
                string msg2 = string.Format(Resources.Error_RecordIsNullOrEmpty, schedules);
                msg.Append(msg2);
            }
            #endregion
            if (enty.Schedules.Count > 0)
            {
                foreach (BusinessApplyScheduleForAPI item in enty.Schedules)
                {
                    if (item.BeginDate <= item.EndDate && //出差安排开始日期小于等于结束日期
                        item.BeginDate.Date >= enty.BeginDate.Date &&//开始日期大于出差开始日期
                        item.EndDate.Date <= enty.EndDate.Date)
                    {//结束日期小于出差结束日期
                        continue;
                    }
                    else
                    {
                       msg.Append((string.Format(Resources.Business_ScheduleError, item.BeginDate.ToDateFormatString(), item.EndDate.ToDateFormatString())));
                    }
                }
            }
            #endregion

            if (msg.Length <= 0) {
                foreach (BusinessApplyPersonForAPI itemChild in enty.Persons)
                {
                    AttendanceLeaveService attendanceLeaveService = new AttendanceLeaveService();
                    string  errMsg = attendanceLeaveService.CheckAllTime(itemChild.EmployeeId.ToString(), enty.BeginDate, enty.BeginTime, enty.EndDate, enty.EndTime
                        , CheckEntityType.Apply, true,enty.BusinessApplyId.ToString());
                    if (!errMsg.CheckNullOrEmpty())
                    {
                        msg.Append(errMsg);
                    }
                    #region 檢查出差申請
                    DateTime bDate = enty.BeginDate.AddTimeToDateTime(enty.BeginTime);
                    DateTime eDate = enty.EndDate.AddTimeToDateTime(enty.EndTime);
                    string  errSameMsg = IsSameBusinessApply(enty.BusinessApplyId.ToString(), itemChild.EmployeeId.ToString(), bDate, eDate);
                    if (!errSameMsg.CheckNullOrEmpty() && msg.Length == 0)
                    {
                        msg.AppendLine(errSameMsg);
                    }
                    #endregion
                    //if (!allErrMsg.ToString().CheckNullOrEmpty()) {
                    //    throw new BusinessRuleException(allErrMsg.ToString());
                    //}
                }
            }
        
            return msg.ToString();
        }
        private string IsSameBusinessApply(string pBusinessApplyId, string pEmployeeId, DateTime pBDateTime, DateTime pEDateTime)
        {
            DataTable dt = new DataTable();
            DataTable dtRegister = new DataTable();
            StringBuilder errorMsg = new StringBuilder();
            //using (IConnectionService conService = Factory.GetService<IConnectionService>())
            //{
            //    IDbCommand cmd = conService.CreateDbCommand();
                string strSql = string.Format(@"select detail.BusinessApplyId, detail.EmployeeId,main.BeginDate,main.BeginTime,main.EndDate,main.EndTime,main.AttendanceTypeId
                                from BusinessApplyPerson detail
                                left join BusinessApply main on detail.BusinessApplyId=main.BusinessApplyId
                                where 
                                detail.BusinessApplyId <> '{0}' and
                                detail.EmployeeId='{1}' and
                                isNull(main.ApproveResultId,'')<>'OperatorResult_002' and 
                                (main.BeginDate <= '{2}' AND main.EndDate >= '{3}')",
pBusinessApplyId, pEmployeeId, pBDateTime.ToDateFormatString(), pEDateTime.ToDateFormatString());
            dt = HRHelper.ExecuteDataTable(strSql);
            //}
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    AttendanceTypeService typeSer = new AttendanceTypeService ();
                    DateTime beginDate = DateTime.MinValue;
                    DateTime endDate = DateTime.MinValue;
                    string strBegin = row["BeginDate"].ToString();
                    string strEnd = row["EndDate"].ToString();
                    string strBeginTime = row["BeginTime"].ToString();
                    string strEndTime = row["EndTime"].ToString();
                    if (!strBegin.CheckNullOrEmpty() && !strEnd.CheckNullOrEmpty()
                    && !strBeginTime.CheckNullOrEmpty() && !strEndTime.CheckNullOrEmpty())
                    {
                        if (DateTime.TryParse(strBegin, out beginDate) && DateTime.TryParse(strEnd, out endDate))
                        {
                            beginDate = beginDate.AddTimeToDateTime(strBeginTime);
                            endDate = endDate.AddTimeToDateTime(strEndTime);
                            if (!CheckTimeAcross(pBDateTime, pEDateTime, beginDate, endDate))
                            {
                                #region 20150330 add by LinBJ for 27425 27426 A00-20150327003 須排除出差登記已銷假資料
                                //using (IConnectionService conService = Factory.GetService<IConnectionService>())
                                //{
                                    //IDbCommand cmd = conService.CreateDbCommand();
                                     strSql = string.Format(@"select info.BusinessRegisterInfoId from BusinessRegisterInfo info
                                                        left join BusinessRegister main on main.BusinessRegisterId =info.BusinessRegisterId 
                                                        where  main.BusinessApplyId='{0}' and info.EmployeeId='{1}' and info.IsRevoke = 1",
                                                                    row["BusinessApplyId"].ToString(), pEmployeeId);
                                dtRegister = HRHelper.ExecuteDataTable(strSql);
                                //    cmd.CommandText = strSql;
                                //    dtRegister.Load(cmd.ExecuteReader());
                                //}
                                if (dtRegister.Rows.Count > 0)
                                {
                                    continue;
                                }
                                #endregion
                                EmployeeService empSer = new EmployeeService();
                                AttendanceTypeService attendanceTypeService = new AttendanceTypeService();
                                string empname = empSer.GetEmployeeNameById(pEmployeeId);
                                errorMsg.Append(string.Format(Resources.AttendanceLeave_LeaveInfoRepeat, empname,
                                    beginDate.ToDateTimeFormatString(), endDate.ToDateTimeFormatString(),
                                    attendanceTypeService.GetNameById(row["AttendanceTypeId"].ToString())));
                            }
                        }
                    }
                }
            }
            return errorMsg.ToString();
        }

        public virtual bool CheckTimeAcross(DateTime pBegin1, DateTime pEnd1, DateTime pBegin2, DateTime pEnd2)
        {
            if ((pBegin1 <= pBegin2 && pEnd1 > pBegin2) || (pBegin1 > pBegin2 && pBegin1 < pEnd2))
            {
                return false;
            }

            return true;
        }

        //单个保存前检查
        public async Task<string> CheckForCCSQ(BusinessApplyForAPI[] entities)
        {
            StringBuilder msgs = new StringBuilder();
            int i = 0;
            foreach (BusinessApplyForAPI enty in entities) {
                i++;
                enty.BusinessApplyId=Guid.NewGuid();
                string s = CheckData(enty);
                if (!s.CheckNullOrEmpty()) {
                    msgs.Append(string.Format("{0} {1}", i, s.ToString()));
                }
            }
             if(msgs.Length>0) return msgs.ToString();
            return "";
        }


        public async  void SaveForCCSQ(string formNumber, BusinessApplyForAPI[] entities)
        {
            StringBuilder msgs = new StringBuilder();
            int i = 0;
            foreach (BusinessApplyForAPI enty in entities)
            {
                i++;
                enty.BusinessApplyId = Guid.NewGuid();
                string s = CheckData(enty);
                if (!s.CheckNullOrEmpty())
                {
                    msgs.Append(string.Format("{0} {1}", i, s.ToString()));
                }
            }
            if (msgs.Length > 0) 
                throw new BusinessRuleException( msgs.ToString());

            List<BusinessApply> entys = HRHelper.WebAPIEntitysToDataEntitys<BusinessApply>("", "", entities);
            foreach (BusinessApply business in entys)
            {
                business.IsEss = true;
                business.IsFromEss = true;
                business.EssNo = formNumber;
                business.EssType = "ATCC";
                business.FoundDate = DateTime.Now.Date;
                business.StateId = Constants.PS02;
                business.BusinessApplyId = Guid.NewGuid();
                foreach (BusinessApplyPerson person in business.Persons) { 
                  person.BusinessApplyPersonId = Guid.NewGuid();
                }
                foreach (BusinessApplySchedule sch in business.Schedules) { 
                  sch.BusinessApplyScheduleId= Guid.NewGuid();  
                }
            }
            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "AT_CC_02";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "formEntities";
            parameter.Value = JsonConvert.SerializeObject(entys);

            callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

            string json = JsonConvert.SerializeObject(callServiceBindingModel);
            string response = await HttpPostJsonHelper.PostJsonAsync(json);

            APIExResponse aPIExResponse = JsonConvert.DeserializeObject<APIExResponse>(response);

            if (aPIExResponse != null)
            {
                if (aPIExResponse.State != "0")
                {
                    throw new BusinessException(aPIExResponse.Msg);
                }
                else if (aPIExResponse.State == "0" && !aPIExResponse.ResultValue.CheckNullOrEmpty())
                {
                    throw new BusinessException(aPIExResponse.ResultValue.ToString());
                }
            }
            else
            {
                throw new Exception(response);
            }
        }
   

        private DataTable GetEmpInfoByCode(string employeeCode)
        {
            DataTable dt = HRHelper.ExecuteDataTable(string.Format(@"select EmployeeId from Employee where Code='{0}'", employeeCode));
            return dt;
        }

        private DataTable GetCorpInfoByCode(string corpCode)
        {
            DataTable dt = HRHelper.ExecuteDataTable(string.Format(@"select CorporationId from Corporation where Code='{0}'", corpCode));
            return dt;
        }


    }
}
