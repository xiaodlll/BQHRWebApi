namespace BQHRWebApi.Business
{
    public class AttendanceCollectForAPI : DataEntity
    {
        /// <summary>
        /// 员工Code
        /// </summary>
        public string EmployeeCode { get; set; }

        /// <summary>
        /// 刷卡机Code
        ///// </summary>
        //public string? MachineCode { get; set; }

        ///// <summary>
        ///// 卡号
        ///// </summary>
        //public string? CardCode { get; set; }

        /// <summary>
        /// 刷卡时间
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string? Time { get; set; }

        /// <summary>
        /// ESS单别
        /// </summary>
        public string? EssType { get; set; }

        /// <summary>
        /// ESS单号
        /// </summary>
        public string? EssNo { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public string? AuditEmployeeCode { get; set; }

        /// <summary>
        /// 审核结果
        /// </summary>
        public bool? AuditResult { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        
        
    }
}
