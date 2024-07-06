using Dcms.Common;
using Dcms.Common.Services;
using Dcms.HR.DataEntities;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Transactions;
using System.Xml.Linq;

namespace Dcms.HR.Services
{
    public partial class ExtendItemService
    {

        public void SaveForAttendanceCollectForEss(AttendanceCollect[] attendanceCollects)
        {
            bool hasError = false;
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

                        IAuditObject auditObject = new AttendanceOverTimePlan();
                        auditObject.ApproveEmployeeId = item.ApproveEmployeeId;
                        auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(item.ApproveEmployeeId.GetString());

                        auditObject.ApproveDate = DateTime.Now.Date;
                        auditObject.ApproveOperationDate = DateTime.Now;
                        auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
                        auditObject.ApproveResultId = auditObject.ApproveResultId;
                        auditObject.ApproveRemark = "API自动审核同意";
                        auditObject.StateId = Constants.PS03;
                        service.Audit(new object[] { attendanceCollectId }, auditObject);

                        jObject["EssNo"] = item.EssNo;
                        jObject["Success"] = true;
                        jObject["Msg"] = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        jObject["EssNo"] = item.EssNo;
                        jObject["Success"] = false;
                        jObject["Msg"] = ex.Message;
                        hasError = true;
                        scope.Dispose();
                        continue;
                    }
                    scope.Complete();
                    jArrayResult.Add(jObject);
                }
            }
            if (hasError)
            {
                throw new Exception(jArrayResult.ToString());
            }
        }

        public void CheckForAttendanceOverTimePlanForEss(AttendanceOverTimePlan[] attendanceOverTimePlans)
        {
            foreach (var item in attendanceOverTimePlans)
            {
                Factory.GetService<IAttendanceOverTimePlanService>().CheckForESS(item);
            }
        }

        public void SaveForAttendanceOverTimePlanForEss(AttendanceOverTimePlan[] attendanceOverTimePlans)
        {
            foreach (var item in attendanceOverTimePlans)
            {
                IAttendanceOverTimePlanService service = Factory.GetService<IAttendanceOverTimePlanService>();
                service.SaveForESS(item);

                IAuditObject auditObject = new AttendanceOverTimePlan();
                IUserService services = Factory.GetService<IUserService>();
                string employeeId = services.GetEmployeeIdOfUser();
                if (!employeeId.CheckNullOrEmpty())
                {
                    auditObject.ApproveEmployeeId = employeeId.GetGuid();
                    auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(employeeId);
                }
                auditObject.ApproveDate = DateTime.Now.Date;
                auditObject.ApproveOperationDate = DateTime.Now;
                auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
                auditObject.ApproveResultId = Constants.AuditAgree;
                auditObject.ApproveRemark = "API自动审核同意";
                auditObject.StateId = Constants.PS03;
                service.Audit(new object[] { item.AttendanceOverTimePlanId }, auditObject);
            }
        }
    }
}
