﻿namespace BQHRWebApi.Business
{
    public class AttendanceRankChangeForAPI : DataEntity
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
