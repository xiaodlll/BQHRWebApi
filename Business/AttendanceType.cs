namespace BQHRWebApi.Business
{
    public class AttendanceType
    {

        // 摘要:
        //     返回/设置 分配原因
        public string AssignReason { get; set; }
        //
        // 摘要:
        //     返回/设置 关联的异常(专为津贴)
        public string AssociateAbnormitys { get; set; }
        //
        // 摘要:
        //     返回/设置 关联的班次(专为津贴)(以;隔开)
        public string AssociateRanks { get; set; }
        //
        // 摘要:
        //     返回/设置 关联的班次名称(专为津贴)(以;隔开)
        public string AssociateRanksName { get; set; }
        //
        // 摘要:
        //     返回/设置 假勤项目ID
        public string AttendanceKindId { get; set; }
        //
        // 摘要:
        //     返回/设置 班次ID
        public string AttendanceRankId { get; set; }
        //
        // 摘要:
        //     返回/设置 假勤类型ID
        public string AttendanceTypeId { get; set; }
        //
        // 摘要:
        //     返回/设置 假勤单位ID
        public string AttendanceUnitId { get; set; }
        //
        // 摘要:
        //     返回/设置 开始日期
        public int BeginDate { get; set; }
        //
        // 摘要:
        //     返回/设置 开始时间(专为津贴)
        public string BeginTime { get; set; }
        //
        // 摘要:
        //     返回/设置 进位方式
        public string CalculateModeId { get; set; }
        //
        // 摘要:
        //     返回/设置 转换旷工类型
        public string ChangeAbsentType { get; set; }
        //
        // 摘要:
        //     返回/设置 假勤编码
        public string Code { get; set; }
        //
        // 摘要:
        //     返回/设置 所属公司ID
        public Guid CorporationId { get; set; }
        public bool CountDayOff { get; set; }
        //
        // 摘要:
        //     返回/设置 创建人
        public Guid CreateBy { get; set; }
        //
        // 摘要:
        //     返回/设置 创建日期
        public DateTime CreateDate { get; set; }
        //
        // 摘要:
        //     返回/设置 最多天数包含其他假勤项核算量
        public string DaysCoverATType { get; set; }
        //
        // 摘要:
        //     返回/设置 是否扣除班次以外(休息，就餐)时间 默认：是
        public bool DeductOhter { get; set; }
        public bool DeductOTCheckHours { get; set; }
        //
        // 摘要:
        //     返回/设置 小数位数
        public int Digits { get; set; }
        //
        // 摘要:
        //     返回/设置 结束日期
        public int EndDate { get; set; }
        //
        // 摘要:
        //     返回/设置 是否可用
        public bool Flag { get; set; }
        //
        // 摘要:
        //     返回/设置 假日类加班免税时数
        public decimal HolidayOTTimes { get; set; }
        //
        // 摘要:
        //     返回/设置 开始时间区间(专为津贴)
        public decimal InterzoneBegin { get; set; }
        //
        // 摘要:
        //     返回/设置 结束时间区间(专为津贴)
        public decimal InterzoneEnd { get; set; }
        //
        // 摘要:
        //     返回/设置 时间段以审核点起算
        public bool InterzoneType { get; set; }
        //
        // 摘要:
        //     返回/设置 是否调休ID
        public string IsAdjust { get; set; }
        //
        // 摘要:
        //     返回/设置 是否需要审核(专为津贴)
        public bool IsAllownceAudit { get; set; }
        //
        // 摘要:
        //     返回/设置 出差是否需要申请Id
        public string IsApplyId { get; set; }
        //
        // 摘要:
        //     返回/设置 是否与异常关联(专为津贴)
        public bool IsAssociateAbnormity { get; set; }
        //
        // 摘要:
        //     返回/设置 是否关联加班(专为津贴)
        public bool IsAssociateOT { get; set; }
        //
        // 摘要:
        //     返回/设置 是否与班次关联(专为津贴)
        public bool IsAssociateRank { get; set; }
        //
        // 摘要:
        //     返回/设置 是否关联班次时间(专为津贴)
        public bool IsAssociateRT { get; set; }
        //
        // 摘要:
        //     返回/设置 是否审核开始时间(专为津贴)
        public bool IsAuditBegin { get; set; }
        //
        // 摘要:
        //     返回/设置 是否按时段计算津贴(专为津贴)
        public bool IsBeginEnd { get; set; }
        //
        // 摘要:
        //     返回/设置 是否扣除调休计划时数
        public bool IsCheckAdjustHours { get; set; }
        public bool IsContainsAbnormal { get; set; }
        //
        // 摘要:
        //     是否提前請假
        public bool IsFlexForOutTime { get; set; }
        //
        // 摘要:
        //     返回/设置 是否生成节假日假勤类型数据
        public bool IsGenerateData { get; set; }
        //
        // 摘要:
        //     返回/设置 當月單日請完
        public bool IsOnlyForOneDay { get; set; }
        //
        // 摘要:
        //     返回/设置 是否带薪ID
        public string IsSalaryId { get; set; }
        //
        // 摘要:
        //     返回/设置 是否ESS中显示
        public bool IsShowEss { get; set; }
        //
        // 摘要:
        //     返回/设置 是否在报表中显示
        public bool IsShowReport { get; set; }
        public bool IsSync { get; set; }
        //
        // 摘要:
        //     返回/设置 是否系统
        public bool IsSystem { get; set; }
        //
        // 摘要:
        //     返回/设置 是否使用
        public bool IsUse { get; set; }
        //
        // 摘要:
        //     返回/设置是否可见
        public bool IsVisible { get; set; }
        //
        // 摘要:
        //     返回/设置 最后修改人
        public Guid LastModifiedBy { get; set; }
        //
        // 摘要:
        //     返回/设置 最后修改日期
        public DateTime LastModifiedDate { get; set; }
        //
        // 摘要:
        //     返回/设置最多带薪时数
        public decimal MaxSalaryHour { get; set; }
        //
        // 摘要:
        //     返回/设置 开始时间(专为津贴)
        public string MergeATType { get; set; }
        //
        // 摘要:
        //     返回/设置 最小审核量
        public decimal MinAuditHours { get; set; }
        //
        // 摘要:
        //     返回/设置 最小核算量
        public decimal MinLeaveHours { get; set; }
        //
        // 摘要:
        //     返回/设置 假勤名称
        public string Name { get; set; }
        //
        // 摘要:
        //     返回/设置 开始时间(专为津贴)
        public string NewBeginTime { get; set; }
        //
        // 摘要:
        //     返回/设置 结束时间(专为津贴)
        public string NewEndTime { get; set; }
        //
        // 摘要:
        //     返回/设置 不扣全勤假勤项
        public string NoItems { get; set; }
        //
        // 摘要:
        //     返回/设置 加班津貼時數計算方式
        public string OTHoursMode { get; set; }
        //
        // 摘要:
        //     返回/设置 所有者Id
        public string OwnerId { get; set; }
        //
        // 摘要:
        //     返回/设置 請假跳過休息日
        public bool PassBreakDay { get; set; }
        //
        // 摘要:
        //     返回/设置 請假跳過休息日
        public bool PassEmptyDay { get; set; }
        public bool PassHoliday { get; set; }
        //
        // 摘要:
        //     返回/设置 是否跳过假日
        public bool PassRest { get; set; }
        //
        // 摘要:
        //     返回/设置 考勤异常计算最小分钟数
        public decimal RankAbnormityBegin { get; set; }
        //
        // 摘要:
        //     返回/设置 考勤异常计算最大分钟数
        public decimal RankAbnormityEnd { get; set; }
        //
        // 摘要:
        //     返回/设置 调休系数
        public decimal Rate { get; set; }
        //
        // 摘要:
        //     返回/设置 备注
        public string Remark { get; set; }
        //
        // 摘要:
        //     返回/设置 休息班次计算包含班次内加班就餐段
        public bool RestOfOvertimeMeal { get; set; }
        //
        // 摘要:
        //     返回/设置 计算周期ID
        public string SalaryPeriodId { get; set; }
        //
        // 摘要:
        //     返回/设置 适用性别
        public string SexType { get; set; }
        //
        // 摘要:
        //     返回/设置 假勤简称
        public string ShortName { get; set; }
        public int TypeTag { get; set; }
    }
}
