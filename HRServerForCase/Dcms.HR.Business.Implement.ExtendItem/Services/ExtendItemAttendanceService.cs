using Dcms.Common;
using Dcms.Common.Services;
using Dcms.HR.DataEntities;
using System;

namespace Dcms.HR.Services
{
    public partial class ExtendItemService
    {

        public void SaveForAttendanceCollectForEss(AttendanceCollect[] attendanceCollects)
        {
            Factory.GetService<IAttendanceCollectService>().Save(attendanceCollects);
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
