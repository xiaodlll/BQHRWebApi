using Dcms.Common;
using Dcms.HR.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                Factory.GetService<IAttendanceOverTimePlanService>().SaveForESS(item);
            }
        }
    }
}
