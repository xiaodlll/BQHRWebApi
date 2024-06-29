using Dcms.Common;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Dcms.HR.Services
{
    public partial class ExtendItemService
    {
        public void SaveForAttendanceLeaveForAPI(AttendanceLeave attendanceLeave)
        {
            Factory.GetService<IAttendanceLeaveService>().CheckForESS(attendanceLeave);
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
    }
}

