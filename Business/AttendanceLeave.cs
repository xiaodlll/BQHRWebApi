namespace BQHRWebApi.Business
{
    public class AttendanceLeaveForAPI : DataEntity
    {
        public string EmpCode { get; set; }
        public Guid? EmployeeId { get; set; }//员工Id
        public string? AttendanceTypeId { get; set; }//请假类型
        public DateTime BeginDate { get; set; }//开始日期
        public string BeginTime { get; set; }//开始时间
        public DateTime EndDate { get; set; }//结束日期
        public string EndTime { get; set; }//结束时间
        public string? Remark { get; set; }//备注
        public decimal TotalHours { get; set; }//请假时数
        public string? Unit { get; set; }//请假单位
        /// <summary>
        /// ESS单别
        /// </summary>
        public string? EssType { get; set; }
        //public Guid? FiscalYearId { get; set; }
        /// <summary>
        /// ESS单号
        /// </summary>
        public string? EssNo { get; set; }

        public Guid? AttendanceLeaveId { get; set; }

        public string? AuditEmployeeCode { get; set; }

        public bool? AuditResult { get; set; }

        public Guid? ApproveEmployeeId { get; set; }
        public string? ApproveResultId { get; set; }

    }


    public class RevokeLeaveForAPI : DataEntity
    {
        public string AttendanceTypeId { get; set; }//请假类型
        /// <summary>
        /// ESS单别
        /// </summary>
        public string? EssType { get; set; }
        /// <summary>
        /// ESS单号
        /// </summary>
        public string? EssNo { get; set; }

        public string? AuditEmployeeCode { get; set; }

        public bool? AuditResult { get; set; }

        public Guid? ApproveEmployeeId { get; set; }
        public string? ApproveResultId { get; set; }

        public string[] AttendanceLeaveInfoIds { get; set; }

    }


}
