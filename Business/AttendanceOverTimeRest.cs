namespace BQHRWebApi.Business
{
    public class AttendanceOverTimeRest : DataEntity
    {
        public Guid AttendanceOverTimeRestId { get; set; }
        public string EmpCode { get; set; }
        public Guid? EmployeeId { get; set; }//员工Id
        public DateTime BeginDate { get; set; }//开始日期
        public string BeginTime { get; set; }//开始时间
        public DateTime EndDate { get; set; }//结束日期
        public string EndTime { get; set; }//结束时间
        public string? Remark { get; set; }//备注
        public decimal Hours { get; set; }//请假时数

        public bool? Flag { get; set; }

        public bool? DeductOhter { get; set; }

        public string?  StateId  { get; set; }
    }
}

