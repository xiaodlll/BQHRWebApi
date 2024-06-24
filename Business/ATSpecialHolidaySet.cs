using System.ComponentModel;

namespace BQHRWebApi.Business
{
    public class ATSpecialHolidaySet
    {
        public ATSpecialHolidaySet()
        {
            ExtendedProperties = new List<ExterFields>();
        }

        [Description("已休数量")]
        public decimal ActualDays { get; set; }
        [Description("可休息量")]
        public decimal Amount { get; set; }
        public string AssignReason { get; set; }
        [Description("员工特殊假设置ID")]
        public Guid ATSpecialHolidaySetId { get; set; }
        [Description("假勤类型")]
        public string AttendanceTypeId { get; set; }
        [Description("生效日期")]
        public DateTime BeginDate { get; set; }
        public Guid CorporationId { get; set; }
        public Guid CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        [Description("日标准工时数")]
        public decimal DaySTHours { get; set; }
        [Description("员工ID")]
        public Guid EmployeeId { get; set; }
        [Description("失效日期")]
        public DateTime EndDate { get; set; }
        public bool Flag { get; set; }
        [Description("是否單次休完")]
        public bool IsOnceOver { get; set; }
        public Guid LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        [Description("请假单号")]
        public string LeaveInfoIds { get; set; }
        public string OwnerId { get; set; }
        [Description("未休数量")]
        public decimal RemaiderDays { get; set; }
        [Description("备注")]
        public string Remark { get; set; }

        public List<ExterFields> ExtendedProperties { get; set; }
      
    }
    public class ExterFields { 
      public string Name { get; set; }
      public string Value {  get; set; }

    }

}
