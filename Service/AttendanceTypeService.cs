using BQHRWebApi.Business;
using Dcms.Common;
using Dcms.HR.Services;
using System.Data;

namespace BQHRWebApi.Service
{
    public class AttendanceTypeService
    {

        public AttendanceType GetAttendanceType(string typeId) {
            DataTable dtType = HRHelper.ExecuteDataTable(string.Format("select * from AttendanceType where AttendanceTypeId='{0}'", typeId));
            List<AttendanceType> myObjects = HRHelper.DataTableToList<AttendanceType>(dtType);
            AttendanceType type = myObjects[0];
            return type;
        }


        /// <summary>
        /// 获得假勤类请假的参数配置
        /// </summary>
        /// <param name="pAttendanceTypeId"></param>
        /// <returns></returns>
        public virtual DataTable GetLeaveConfig(string pAttendanceTypeId)
        {
            DataTable dt = new DataTable();
            List<bool> list = new List<bool>();
           
                string strSql = string.Empty;
                strSql = string.Format("select PassRest,DeductOhter,MinLeaveHours,MinAuditHours,AttendanceUnitId,CalculateModeId,Digits, PassHoliday,PassBreakDay,PassEmptyDay From AttendanceType Where AttendanceTypeId='{0}'", pAttendanceTypeId);
               dt=HRHelper.ExecuteDataTable(strSql);
            return dt;
        }
    }
}
