using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.HR.Services;
using System.Data;
using System.Data.SqlClient;

namespace BQHRWebApi.Service
{
    public class AttendanceCollectService : HRService
    {
      

        public override void Save(DataEntity[] entities)
        {
            foreach (var entity in entities)
            {
                AttendanceCollect attendance = entity as AttendanceCollect;
                SaveAttendance(attendance);
            }
        }

        public void SaveAttendance(AttendanceCollect attendance)
        {
            string sql = "INSERT INTO AttendanceCollect (EmployeeId, [Date],[Time], Remark)" +
                " VALUES (@EmployeeId, @Date,@Time, @Remark)";
            List<SqlParameter> listPara = new List<SqlParameter>();

            AddParaWithValue(listPara, "@EmployeeId", DbType.Guid, GetEmployeeIdByCode(attendance.EmployeeCode));
            AddParaWithValue(listPara, "@Date", DbType.DateTime, attendance.AttTime.ToString("yyyy-MM-dd HH:mm:ss"));
            AddParaWithValue(listPara, "@Time", DbType.DateTime, attendance.AttTime.ToString("HH:mm"));
            AddParaWithValue(listPara, "@Remark", DbType.String, (attendance.Remark == null ? "" : attendance.Remark));
            HRHelper.ExecuteNonQuery(sql, listPara.ToArray());
        }

        private Guid GetEmployeeIdByCode(string employeeCode)
        {
            Guid guid = Guid.Parse(HRHelper.ExecuteScalar(string.Format("select EmployeeId from Employee where Code='{0}'", employeeCode)).ToString());
            return guid;
        }
    }
}
