namespace BQHRWebApi.Business
{
    public class AttendanceOverTimePlanForAPI : DataEntity
    {
        /// <summary>
        /// ESS单别
        /// </summary>
        public string? EssType { get; set; }

        /// <summary>
        /// ESS单号
        /// </summary>
        public string? EssNo { get; set; }

        /// <summary>
        /// 公司编码
        /// </summary>
        public string CorporationCode { get; set; }

        /// <summary>
        /// 员工编码
        /// </summary>
        public string EmployeeCode { get; set; }

        /// <summary>
        /// 计划编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 计划名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 制定日期
        /// </summary>
        public DateTime FoundDate { get; set; }

        /// <summary>
        /// 加班原因
        /// </summary>
        public string? Reason { get; set; }

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

        /// <summary>
        /// 明细
        /// </summary>
        public List<AttendanceOverTimeInfoForAPI>? OverTimeInfos { get; set; }

    }

    public class AttendanceOverTimeInfoForAPI : DataEntity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        public Guid? AttendanceOverTimeInfoId { get; set; }

        /// <summary>
        /// 员工编码
        /// </summary>
        public string EmployeeCode { get; set; }

        /// <summary>
        /// 加班原因
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// 加班日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? BeginDate { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string? BeginTime { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string? EndTime { get; set; }

        /// <summary>
        /// 加班时数
        /// </summary>
        public decimal Hours { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }

    public class AttendanceOTHourForAPI
    {
        /// <summary>
        /// ESS单号
        /// </summary>
        public string EssNo { get; set; }

        /// <summary>
        /// 员工编码
        /// </summary>
        public string EmployeeCode { get; set; }

        /// <summary>
        /// 加班日期
        /// </summary>
        public DateTime OTDate { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 加班归属日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 去掉用餐时间
        /// </summary>
        public int? IsRemoveDinner { get; set; }

    }
}
