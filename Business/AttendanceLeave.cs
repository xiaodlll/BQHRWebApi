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

        //public Guid? FiscalYearId { get; set; }
        public Guid? AttendanceLeaveId { get; set; }


    }



}
