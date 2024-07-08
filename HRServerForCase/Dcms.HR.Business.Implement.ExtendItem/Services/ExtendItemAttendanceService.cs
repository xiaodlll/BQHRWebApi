using Dcms.Common;
using Dcms.Common.Services;
using Dcms.HR.DataEntities;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Transactions;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Dcms.HR.Services
{
    public partial class ExtendItemService
    {

        public string SaveForAttendanceCollectForEss(AttendanceCollect[] attendanceCollects)
        {
            JArray jArrayResult = new JArray();
            foreach (var item in attendanceCollects)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    JObject jObject = new JObject();
                    try
                    {
                        string attendanceCollectId = string.Empty;
                        IAttendanceCollectService service = Factory.GetService<IAttendanceCollectService>();
                        service.SaveForESS(item.EmployeeId.GetString(), item.Date, item.Date, item.Time, item.AttendanceTypeId, item.Remark, item.EmployeeId.GetString(), item.Time, item.Time, item.EssType, item.EssNo, 1);
                        DataTable dt1 = HRHelper.ExecuteDataTable(string.Format("select top 1 AttendanceCollectId from AttendanceCollect where [Date]='{0}' and [Time]='{1}' and EmployeeId='{2}' and EssNo='{3}' order by CreateDate desc", 
                           DateTime.Parse( item.Date.ToString("yyyy-MM-dd")+" "+ item.Time).ToString("yyyy-MM-dd HH:mm:ss"), item.Time, item.EmployeeId.GetString(), item.EssNo));
                        if(dt1.Rows.Count > 0)
                        {
                            attendanceCollectId = dt1.Rows[0][0].ToString();
                        }

                        if (!(item.ApproveEmployeeId.CheckNullOrEmpty()))
                        {
                            IAuditObject auditObject = new AttendanceOverTimePlan();
                            auditObject.ApproveEmployeeId = item.ApproveEmployeeId;
                            auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(item.ApproveEmployeeId.GetString());
                            auditObject.ApproveDate = DateTime.Now.Date;
                            auditObject.ApproveOperationDate = DateTime.Now;
                            auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
                            auditObject.ApproveResultId = item.ApproveResultId;
                            auditObject.ApproveRemark = "API自动审核同意";
                            auditObject.StateId = Constants.PS03;
                            service.Audit(new object[] { attendanceCollectId }, auditObject);
                        }

                        jObject["EssNo"] = item.EssNo;
                        jObject["Success"] = true;
                        jObject["Msg"] = string.Empty;
                        jArrayResult.Add(jObject);
                    }
                    catch (Exception ex)
                    {
                        jObject["EssNo"] = item.EssNo;
                        jObject["Success"] = false;
                        jObject["Msg"] = ex.Message;
                        jArrayResult.Add(jObject);
                        scope.Dispose();
                        continue;
                    }
                    scope.Complete();
                }
            }
            return jArrayResult.ToString();
        }

        public string CheckForAttendanceOverTimePlanForEss(AttendanceOverTimePlan[] attendanceOverTimePlans)
        {
            IAttendanceEmployeeRankService rankService = Factory.GetService<IAttendanceEmployeeRankService>();
            JArray jArrayResult = new JArray();
            foreach (var item in attendanceOverTimePlans)
            {
                JObject jObject = new JObject();
                try
                {
                    foreach (var detail in item.OverTimeInfos)
                        SetAttRankAndType(rankService, detail);
                    Factory.GetService<IAttendanceOverTimePlanService>().CheckForESS(item);
                    jObject["EssNo"] = item.EssNo;
                    jObject["Success"] = true;
                    jObject["Msg"] = string.Empty;
                    jArrayResult.Add(jObject);
                }
                catch (Exception ex)
                {
                    jObject["EssNo"] = item.EssNo;
                    jObject["Success"] = false;
                    jObject["Msg"] = ex.Message;
                    jArrayResult.Add(jObject);
                }
            }
            return jArrayResult.ToString();
        }

        public string SaveForAttendanceOverTimePlanForEss(AttendanceOverTimePlan[] attendanceOverTimePlans)
        {
            IAttendanceEmployeeRankService rankService = Factory.GetService<IAttendanceEmployeeRankService>();
            JArray jArrayResult = new JArray();
            foreach (var item in attendanceOverTimePlans)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    JObject jObject = new JObject();
                    try
                    {
                        foreach (var detail in item.OverTimeInfos)
                            SetAttRankAndType(rankService, detail);
                        string attendanceCollectId = item.AttendanceOverTimePlanId.GetString();
                        IAttendanceOverTimePlanService service = Factory.GetService<IAttendanceOverTimePlanService>();
                        service.SaveForESS(item);
                        if (!(item.ApproveEmployeeId.CheckNullOrEmpty()))
                        {
                            IAuditObject auditObject = new AttendanceOverTimePlan();
                            auditObject.ApproveEmployeeId = item.ApproveEmployeeId;
                            auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(item.ApproveEmployeeId.GetString());
                            auditObject.ApproveDate = DateTime.Now.Date;
                            auditObject.ApproveOperationDate = DateTime.Now;
                            auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
                            auditObject.ApproveResultId = item.ApproveResultId;
                            auditObject.ApproveRemark = "API自动审核同意";
                            auditObject.StateId = Constants.PS03;
                            service.Audit(new object[] { attendanceCollectId }, auditObject);
                        }
                        jObject["EssNo"] = item.EssNo;
                        jObject["Success"] = true;
                        jObject["Msg"] = string.Empty;
                        jArrayResult.Add(jObject);
                    }
                    catch (Exception ex)
                    {
                        jObject["EssNo"] = item.EssNo;
                        jObject["Success"] = false;
                        jObject["Msg"] = ex.Message;
                        jArrayResult.Add(jObject);
                        scope.Dispose();
                        continue;
                    }
                    scope.Complete();
                }
            }
            return jArrayResult.ToString();
        }

        private void SetAttRankAndType(IAttendanceEmployeeRankService rankService, AttendanceOverTimeInfo detail)
        {

            //获取attendanceTypeId和attendanceRankId
            DataTable dtEmpAttRank = rankService.GetEmpsDailyInfo(new string[] { detail.EmployeeId.ToString() }, detail.BeginDate.Date.AddDays(-1.0), detail.EndDate.Date.AddDays(1.0));
            if (dtEmpAttRank != null && dtEmpAttRank.Rows.Count > 0)
            {

                detail.AttendanceRankId = dtEmpAttRank.Rows[0]["AttendanceRankId"].ToString();
            }
            else
            {
                throw new BusinessRuleException("找不到对应的员工的班次。");
            }
            DataTable dtEmpAttType = rankService.GetEmpRankCalendar(new string[] { detail.EmployeeId.ToString() }, detail.BeginDate.Date.AddDays(-1.0), detail.EndDate.Date.AddDays(1.0));
            if (dtEmpAttType != null && dtEmpAttType.Rows.Count > 0)
            {
                string holidayTypeId = dtEmpAttType.Rows[0]["HolidayTypeId"].ToString();
                if (holidayTypeId == "HolidayKind_003")
                {
                    detail.AttendanceTypeId = "502";
                }
                else if (holidayTypeId == "HolidayKind_001")
                {
                    detail.AttendanceTypeId = "501";
                }
                else if (holidayTypeId == "HolidayKind_004")
                {
                    detail.AttendanceTypeId = "504";
                }
                else
                {
                    detail.AttendanceTypeId = "503";
                }
            }
            else
            {
                throw new BusinessRuleException("找不到对应的员工的假勤类型。");
            }
        }

    }

}
