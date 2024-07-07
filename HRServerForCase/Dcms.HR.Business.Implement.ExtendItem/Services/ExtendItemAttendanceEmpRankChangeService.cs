using Dcms.Common;
using Dcms.Common.Services;
using Dcms.HR.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dcms.HR.Services
{
    public partial class ExtendItemService
    {

        public void CheckForAttendanceRankChangeForEss(AttendanceEmployeeRank[] attendanceEmployeeRanks)
        {
            foreach (var item in attendanceEmployeeRanks)
            {
                string id = GetAttendanceEmployeeRankId(item.EmployeeId.GetString(), item.Date);
                if (string.IsNullOrEmpty(id))
                {
                    throw new BusinessRuleException(string.Format("找不到员工{0} 在{1}现有的班次。",Factory.GetService<IEmployeeServiceEx>().GetEmployeeCodeById(item.EmployeeId.GetString()), item.Date.ToString("yyyy-MM-dd")));
                }
            }
        }

        public void SaveForAttendanceRankChangeForEss(AttendanceEmployeeRank[] attendanceEmployeeRanks)
        {
            foreach (var item in attendanceEmployeeRanks)
            {
                IAttendanceEmployeeRankService service = Factory.GetService<IAttendanceEmployeeRankService>();
                IDocumentService<AttendanceEmployeeRank> docService = service;
                string id = GetAttendanceEmployeeRankId(item.EmployeeId.GetString(), item.Date);
                if (string.IsNullOrEmpty(id))
                {
                    throw new BusinessRuleException(string.Format("找不到员工{0} 在{1}现有的班次。", Factory.GetService<IEmployeeServiceEx>().GetEmployeeCodeById(item.EmployeeId.GetString()), item.Date.ToString("yyyy-MM-dd")));
                }
                AttendanceEmployeeRank atEmpRank = docService.Read(id);
                atEmpRank.AttendanceRankId = item.AttendanceRankId;
                atEmpRank.AttendanceHolidayTypeId = item.AttendanceHolidayTypeId;
                atEmpRank.IsChange = true;

                service.CustomSaveRank(new string[] { atEmpRank.EmployeeId.GetString() }, atEmpRank.AttendanceRankId, atEmpRank.AttendanceSpellId.GetString(), atEmpRank.Date, atEmpRank.Date, true, true, atEmpRank.Remark, string.Empty);
            }
        }

        private string GetAttendanceEmployeeRankId(string pEmployeeId, DateTime pDate)
        {
            var dt = HRHelper.ExecuteDataTable(string.Format("select AttendanceEmployeeRankId from AttendanceEmpRank where EmployeeId='{0}' and [Date]='{1}'", pEmployeeId, pDate.ToString("yyyy-MM-dd")));
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0][0].ToString();
            }
            return null;
        }
    }
}
