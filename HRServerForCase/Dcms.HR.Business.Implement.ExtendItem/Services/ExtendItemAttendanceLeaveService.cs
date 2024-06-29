using Dcms.Common;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Dcms.Common.Services;
using Dcms.Common.Torridity.Metadata;
using Dcms.Common.Torridity;
using System.Data.SqlClient;

namespace Dcms.HR.Services
{
    public partial class ExtendItemService
    {
        public void CheckForAttendanceLeaveForAPI(AttendanceLeave attendanceLeave)
        {
            Factory.GetService<IAttendanceLeaveService>().CheckForESS(attendanceLeave);
        }

        public void SaveAttendanceLeaveForAPI(AttendanceLeave attendanceLeave)
        {
            IAttendanceLeaveService service = Factory.GetService<IAttendanceLeaveService>();
            IDocumentService<AttendanceLeave> docSer = service;
            service.SaveForESS(attendanceLeave);
            AttendanceLeave entyNew = docSer.Read(attendanceLeave.AttendanceLeaveId);
            IAuditObject auditObject = new AttendanceLeave();
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
            service.Audit(new object[] { entyNew.AttendanceLeaveId }, auditObject);
        }

        public void CheckForAT406ForAPI(AttendanceOverTimeRest formEntity)
        {
            Factory.GetService<IAttendanceOverTimeRestService>().CheckForESS(formEntity);
        }

        public void SaveAT406ForAPI(AttendanceOverTimeRest formEntity)
        {
            IAttendanceOverTimeRestService service = Factory.GetService<IAttendanceOverTimeRestService>();
            IDocumentService<AttendanceOverTimeRest> docSer = service;
            service.SaveForESS(formEntity);
            AttendanceOverTimeRest entyNew = docSer.Read(formEntity.AttendanceOverTimeRestId);
            IAuditObject auditObject = new AttendanceOverTimeRest();
            IUserService services = Factory.GetService<IUserService>();
            string employeeId = services.GetEmployeeIdOfUser();
            if (!employeeId.CheckNullOrEmpty())
            {
                auditObject.ApproveEmployeeId = employeeId.GetGuid();
                auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(employeeId);
            }
            auditObject.StateId = Constants.PS03;
            auditObject.ApproveDate = DateTime.Now.Date;
            auditObject.ApproveOperationDate = DateTime.Now;
            auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
            auditObject.ApproveResultId = Constants.AuditAgree;
            auditObject.ApproveRemark = "API自动审核同意";
            service.Audit(new object[] { entyNew.AttendanceOverTimeRestId }, auditObject);
        }

        public void CheckForAT401ForAPI(AnnualLeaveRegister formEntity)
        {
            Factory.GetService<IAnnualLeaveRegisterService>().CheckForESS(formEntity);
        }
        public void SaveAT401ForAPI(AnnualLeaveRegister formEntity)
        {
            IAnnualLeaveRegisterService service = Factory.GetService<IAnnualLeaveRegisterService>();
            IDocumentService<AnnualLeaveRegister> docSer = service;

            service.SaveForESS(formEntity);
            AnnualLeaveRegister entyNew = docSer.Read(formEntity.AnnualLeaveRegisterId);
            IAuditObject auditObject = new AnnualLeaveRegister();
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
            auditObject.StateId = Constants.PS03;
            auditObject.ApproveRemark = "API自动审核同意";
            //this.Submit(new object[] { e.DataEntity.AnnualLeaveRegisterId });
            service.Audit(new object[] { entyNew.AnnualLeaveRegisterId }, auditObject);


        }

        public DataTable GetLeaveHoursForAPI(AttendanceLeave attendanceLeave)
        {
            //DataTable GetLeaveHoursForGP(string pEmployeeId, DateTime pBeginDate, string pBeginTime, DateTime pEndDate, string pEndTime, string pAttendanceTypeId)
            DataTable dt= Factory.GetService<IAttendanceLeaveService>().GetLeaveHoursForGP(attendanceLeave.EmployeeId.GetString(),
                attendanceLeave.BeginDate,
                attendanceLeave.BeginTime,
                attendanceLeave.EndDate, attendanceLeave.EndTime,attendanceLeave.AttendanceTypeId);
            return dt;
        }

        public DataTable GetRestHoursForAPI(AttendanceOverTimeRest formEntity)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Hours");
            dt.Columns.Add("Unit");
            decimal hours = Factory.GetService<IAttendanceOverTimeRestService>().GetHoursForESS(formEntity);
            dt.Rows.Add(hours.ToString("#.##"), "小時");
            return dt;
        }


    }
}

