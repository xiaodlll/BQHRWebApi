using Dcms.HR.DataEntities;
using System.ComponentModel;

namespace BQHRWebApi.Business
{
    public class AttendanceLeaveForAPI : DataEntity
    {
        public string? EmpCode { get; set; }
        public Guid? EmployeeId { get; set; }//员工Id
        public string AttendanceTypeId { get; set; }//请假类型
        public DateTime BeginDate { get; set; }//开始日期
        public string?  BeginTime { get; set; }//开始时间
        public DateTime EndDate { get; set; }//结束日期
        public string? EndTime { get; set; }//结束时间
        public string? Remark { get; set; }//备注
        public decimal TotalHours { get; set; }//请假时数
        public string? Unit { get; set; }//请假单位

        public Guid? FiscalYearId { get; set; }
        public Guid? AttendanceLeaveId { get; set; }
        public bool? IsEss { get; set; }

    }


    public class AttendanceLeave : DataEntity
    {
        public System.Guid AttendanceLeaveId { get; set; }

        public System.Guid EmployeeId { get; set; }

        public string? AttendanceTypeId { get; set; }


        public System.DateTime BeginDate { get; set; }

        public string? BeginTime { get; set; }

        public System.DateTime EndDate { get; set; }
        public string? EndTime { get; set; }

        public string? Remark { get; set; }

        public string? StateId { get; set; }

        public System.DateTime   ApproveDate { get; set; }

        public System.Guid ApproveEmployeeId { get; set; }

        public string ApproveEmployeeName { get; set; }

        public string? ApproveRemark { get; set; }

        public System.DateTime ApproveOperationDate { get; set; }

        public System.Guid ApproveUserId { get; set; }

        public System.DateTime CreateDate { get; set; }

        public System.DateTime LastModifiedDate { get; set; }

        public System.Guid CreateBy { get; set; }

        public System.Guid LastModifiedBy { get; set; }

        public bool Flag { get; set; }

        public System.Guid CorporationId { get; set; }


        public List<AttendanceLeaveInfo> Infos { get; set; }
        public AttendanceLeave()
        {
            Infos = new List<AttendanceLeaveInfo>();
        }

        public string? ApproveResultId { get; set; }

        public System.DateTime ConfirmOperationDate { get; set; }

        public System.Guid ConfirmUserId { get; set; }

        public System.DateTime SubmitOperationDate { get; set; }

        public System.Guid SubmitUserId { get; set; }

        public string CauseId { get; set; }

        public int AttendanceDayType { get; set; }
        public string AssignReason { get; set; }

        public string OwnerId { get; set; }

        public bool DeductOhter { get; set; }

        public bool PassRest { get; set; }

        public bool IsCheckAtType { get; set; }
        public bool WholeDay { get; set; }
        public decimal hours { get; set; }
        public bool IsUndo { get; set; }
        public bool IsRevoke { get; set; }
        public System.Guid FiscalYearId { get; set; }
        public System.Guid DeputyEmployeeId { get; set; }

        public bool IsEss { get; set; }
        public bool TsEF { get; set; }

        public bool IsRegular { get; set; }

        public decimal IotalHours { get; set; }
        public decimal CancelHours { get; set; }
        public decimal EffectiveHours { get; set; }


    }

    public class AttendanceLeaveInfo {
        public System.Guid AttendanceLeaveInfoId { get; set; }

        public System.Guid EmployeeId { get; set; }

        public string? AttendanceTypeId { get; set; }

        public string? AttendanceRankId { get; set; }

        public System.DateTime BeginDate { get; set; }

        public string? BeginTime { get; set; }

        public System.DateTime EndDate { get; set; }

        public string?  EndTime { get; set; }

        public decimal Hours { get; set; }

        public string? Remark { get; set; }

        public System.DateTime RevokeDate { get; set; }

        public System.Guid RevokeEmployeeId { get; set; }

        public string? RevokeEmployeeName { get; set; }

        public string?    RevokeRemark { get; set; }

        public System.DateTime RevokeOperationDate { get; set; }

        public System.Guid RevokeUserId { get; set; }

        public System.DateTime CreateDate { get; set; }

        public System.DateTime LastModifiedDate { get; set; }

        public System.Guid CreateBy { get; set; }

        public System.Guid LastModifiedBy { get; set; }

        public bool Flag { get; set; }
        public bool IsRevoke { get; set; }
        public System.Guid CorprationId { get; set; }

        public System.Guid EmployeeRankId { get; set; }
        public System.DateTime Date { get; set; }
        public System.Guid FiscalYearId { get; set; }

        public string? AnnualLeaveUnit { get; set; }
        public decimal OldDays { get; set; }

        public decimal Days { get; set; }

        public string? SpecialSetIdAndHours { get; set; }


        public string? AjustIdAndHours { get; set; }

        public string? EssRevokeStatus { get; set; }


    }

}
