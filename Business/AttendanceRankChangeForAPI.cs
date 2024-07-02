namespace BQHRWebApi.Business
{
    public class AttendanceRankChangeForAPI : DataEntity
    {
        /// <summary>
        /// 返回/设置 员工编码
        /// </summary>
        public string EmployeeCode { get; set; }

        /// <summary>
        /// 工作日期
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// 员工班次
        /// </summary>
        public string AttendanceRankId { get; set; }

        /// <summary>
        /// 节假日类型
        /// </summary>
        public string AttendanceHolidayTypeId { get; set; }
    }
}
