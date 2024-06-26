namespace BQHRWebApi.Business
{
    public class AttendanceCollect: DataEntity
    {
        /// <summary>
        /// 员工Id
        /// </summary>
        public string EmployeeId { get; set; }

        
        /// <summary>
        /// 刷卡时间
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string? Time { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
