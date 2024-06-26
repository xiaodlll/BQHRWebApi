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
    }
}
