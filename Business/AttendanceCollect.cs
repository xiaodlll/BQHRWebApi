namespace BQHRWebApi.Business
{
    public class AttendanceCollect: DataEntity
    {
        /// <summary>
        /// 员工编码
        /// </summary>
        public string EmployeeCode { get; set; }

        
        /// <summary>
        /// 刷卡时间
        /// </summary>
        public DateTime AttTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
