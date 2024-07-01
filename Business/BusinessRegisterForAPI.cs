namespace BQHRWebApi.Business
{
    /// <summary>
    /// 出差登记实体
    /// </summary>
    [Serializable()]
    public class BusinessRegisterForAPI : DataEntity
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
        /// 返回/设置 登记方式
        /// </summary>
        public int? RegisterMode { get; set; }

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
        /// 返回/设置 员工编码
        /// </summary>
        public string EmployeeCode { get; set; }

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
        public string? Reason { get; set; }

        /// <summary>
        /// 返回/设置 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 返回 出差信息-集合
        /// </summary>
        public List<BusinessRegisterInfoForAPI>? RegisterInfos { get; set; }

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
        /// 主键Id
        /// </summary>
        public Guid? BusinessRegisterInfoId { get; set; }

        /// <summary>
        /// 返回/设置 员工编码
        /// </summary>
        public string EmployeeCode { get; set; }

    }
}
