using BQHRWebApi.Business;
using Dcms.Common;
using Dcms.HR.Services;
using System.Data;
using Dcms.HR.DataEntities;

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
        /// 根据员工ID获取员工姓名
        /// </summary>
        /// <param name="pEmployeeId"></param>
        /// <returns></returns>
        public string GetNameById(string pTypeId)
        {
            #region 参数检查
            if (pTypeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("pTypeId Error");
            }
            #endregion

            DataTable dt = HRHelper.ExecuteDataTable(string.Format("select Name from AttendanceType where AttendanceTypeId='{0}'", pTypeId));

            if (dt != null && dt.Rows.Count > 0)
            {

                return dt.Rows[0][0].ToString();
            }

            return string.Empty;

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
                strSql = string.Format("select PassRest,DeductOhter,MinLeaveHours,MinAuditHours,AttendanceUnitId,CalculateModeId,Digits, " +
                    "PassHoliday From AttendanceType Where AttendanceTypeId='{0}'", pAttendanceTypeId);
               dt=HRHelper.ExecuteDataTable(strSql);
            return dt;
        }
    }
}
