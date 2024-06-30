namespace BQHRWebApi.Business
{
    /// <summary>
    /// 出差登记实体
    /// </summary>
    [Serializable()]
    public class BusinessRegisterForAPI : DataEntity
    {


        /// <summary>
        /// 返回/设置 登记方式
        /// </summary>
        public int RegisterMode { get; set; }

        /// <summary>
        /// 返回/设置 出差申请ID
        /// </summary>
        public System.Guid BusinessApplyId { get; set; }

        /// <summary>
        /// 返回/设置 开始日期
        /// </summary>
        public System.DateTime BeginDate { get; set; }

        /// <summary>
        /// 返回/设置 开始时间
        /// </summary>
        public string BeginTime { get; set; }

        /// <summary>
        /// 返回/设置 结束日期
        /// </summary>
        public System.DateTime EndDate { get; set; }

        /// <summary>
        /// 返回/设置 结束时间
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 返回/设置 员工ID
        /// </summary>
        public System.Guid EmployeeId { get; set; }

        /// <summary>
        /// 返回/设置 出差类别ID
        /// </summary>
        public string AttendanceTypeId { get; set; }

        /// <summary>
        /// 返回/设置 出差地点
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 返回/设置 出差原因
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 返回/设置 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 返回/设置 是否扣除休息就餐段
        /// </summary>
        public bool IsCheckRest { get; set; }
        /// <summary>
        /// 返回/设置 是否扣除非在崗時間
        /// </summary>
        public bool IsDeductOther { get; set; }
        /// <summary>
        /// 返回/设置 扣除休息班次内加班就餐段
        /// </summary>
        public bool DeductRestOfOvertimeMeal { get; set; }

        /// <summary>
        /// 返回/设置 计划状态ID
        /// </summary>
        public string StateId { get; set; }

        /// <summary>
        /// 返回/设置 审核日期
        /// </summary>
        public System.DateTime ApproveDate { get; set; }

        /// <summary>
        /// 返回/设置 审核人ID
        /// </summary>
        public System.Guid ApproveEmployeeId { get; set; }


        /// <summary>
        /// 返回/设置 審核人名稱
        /// </summary>
        public string ApproveEmployeeName { get; set; }

        /// <summary>
        /// 返回/设置 审核结果ID
        /// </summary>
        public string ApproveResultId { get; set; }

        /// <summary>
        /// 返回 出差信息-集合
        /// </summary>
        public List<BusinessRegisterInfoForAPI> RegisterInfos { get; set; }

        /// <summary>
        /// 建構
        /// </summary>
        public BusinessRegisterForAPI()
        {
            RegisterInfos = new List<BusinessRegisterInfoForAPI>();
        }

    }


    /// <summary>
    /// 出差信息实体
    /// </summary>
    [Serializable()]
    public class BusinessRegisterInfoForAPI
    {

        /// <summary>
        /// 返回/设置 员工ID
        /// </summary>
        public System.Guid EmployeeId { get; set; }

        /// <summary>
        /// 返回/设置 班次ID
        /// </summary>
        public string AttendanceRankId { get; set; }

        /// <summary>
        /// 返回/设置 计划日期
        /// </summary>
        public System.DateTime Date { get; set; }

        /// <summary>
        /// 返回/设置 开始日期
        /// </summary>
        public System.DateTime BeginDate { get; set; }

        /// <summary>
        /// 返回/设置 开始时间
        /// </summary>
        public string BeginTime { get; set; }

        /// <summary>
        /// 返回/设置 结束日期
        /// </summary>
        public System.DateTime EndDate { get; set; }

        /// <summary>
        /// 返回/设置 结束时间
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 返回/设置 出差天数
        /// </summary>
        public decimal Days { get; set; }

        /// <summary>
        /// 返回/设置 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 返回/设置 是否销假
        /// </summary>
        public bool IsRevoke { get; set; }

        /// <summary>
        /// 返回/设置 销假日期
        /// </summary>
        public System.DateTime RevokeDate { get; set; }

        /// <summary>
        /// 返回/设置 销假人ID
        /// </summary>
        public System.Guid RevokeEmployeeId { get; set; }

        /// <summary>
        /// 返回/设置 销假人名称
        /// </summary>
        public string RevokeEmployeeName { get; set; }

        /// <summary>
        /// 返回/设置 销假批注
        /// </summary>
        public string RevokeRemark { get; set; }

        /// <summary>
        /// ESS销假状态
        /// </summary>
        public string EssRevokeStatus { get; set; }
    }
}
