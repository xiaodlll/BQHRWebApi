using System.ComponentModel;

namespace BQHRWebApi.Business
{
    public class AttendanceRank
    {
        // 摘要:
        //     返回/设置 提前弹性分钟数
        [Description("提前弹性分钟数")]
        public decimal AdvanceFlexMinutes { get; set; }
        //
        // 摘要:
        //     返回/设置 提前弹性单次分钟数
        [Description("提前弹性单次分钟数")]
        public decimal AdvanceOnceFlexMinutes { get; set; }
        [Description("可休息分鐘數")]
        public decimal AllownRestMinutes { get; set; }
        //
        // 摘要:
        //     返回/设置 分配原因
        public string AssignReason { get; set; }
        //
        // 摘要:
        //     返回/设置 班次类型ID
        [Description("班次类型ID")]
        public string AttendanceRankId { get; set; }
        //
        // 摘要:
        //     返回/设置 班次类型ID
        [Description("班次津贴ID")]
        public string AttendanceTypeId { get; set; }
        [Description("法定休息班")]
        public bool BreakDay { get; set; }
        //
        // 摘要:
        //     返回/设置 班次编码
        [Description("班次编码")]
        public string Code { get; set; }
        //
        // 摘要:
        //     返回/设置 所属公司ID
        [Description("所属公司ID")]
        public Guid CorporationId { get; set; }
        //
        // 摘要:
        //     返回/设置 创建人
        public Guid CreateBy { get; set; }
        //
        // 摘要:
        //     返回/设置 创建日期
        [Description("班次类型表")]
        public DateTime CreateDate { get; set; }
        //
        // 摘要:
        //     返回/设置 延后弹性分钟数
        [Description("延后弹性分钟数")]
        public decimal DelayFlexMinutes { get; set; }
        //
        // 摘要:
        //     返回/设置 延后弹性单次分钟数
        [Description("延后弹性单次分钟数")]
        public decimal DelayOnceFlexMinutes { get; set; }
        [Description("空班")]
        public bool EmptyDay { get; set; }
        //
        // 摘要:
        //     返回/设置 是否可用
        public bool Flag { get; set; }
        //
        // 摘要:
        //     返回/设置 是否提前弹性
        [Description("是否提前弹性")]
        public bool IsAdvanceFlex { get; set; }
        [Description("归属前一天")]
        public bool IsBelongToBefore { get; set; }
        [Description("依刷卡判斷彈性")]
        public bool IsCollectFelx { get; set; }
        [Description("工作時數要扣休息時間 ")]
        public bool IsDeductTime { get; set; }
        //
        // 摘要:
        //     返回/设置 是否延后弹性
        [Description("是否延后弹性")]
        public bool IsDelayFlex { get; set; }
        [Description("ESS中显示")]
        public bool IsDisPlayInEss { get; set; }
        [Description("第一个正常班段结束时间允许弹性")]
        public bool IsFlexFirstEnd { get; set; }
        //
        // 摘要:
        //     返回/设置 是否弹性班次
        [Description("是否弹性班次")]
        public bool IsFlexRank { get; set; }
        [Description("工作时间包含夜间工时")]
        public bool IsNightWork { get; set; }
        //
        // 摘要:
        //     返回/设置 是否加班ID
        [Description("是否加班ID")]
        public string IsOverTimeId { get; set; }
        //
        // 摘要:
        //     返回/设置 是否跨天ID
        [Description("是否跨天ID")]
        public string IsOverZeroId { get; set; }
        //
        // 摘要:
        //     返回/设置标记该班次是否是休息类班次
        [Description("休息班次")]
        public bool IsRestRank { get; set; }
        [Description("是否賣場班次")]
        public bool IsSalesRank { get; set; }
        [Description("是否停用")]
        public bool IsStop { get; set; }
        //
        // 摘要:
        //     返回/设置 是否系统
        [Description("是否系统")]
        public bool IsSystem { get; set; }
        [Description("列入總工時計算")]
        public bool IsWorkTime { get; set; }
        //
        // 摘要:
        //     返回/设置 在岗时数
        [Description("在岗时数")]
        public decimal JobHours { get; set; }
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
        //     返回/设置 迟到起算方式
        [Description("迟到起算方式")]
        public string LateCalculateMode { get; set; }
        [Description("最小核算量分鐘")]
        public decimal MinAmount { get; set; }
        //
        // 摘要:
        //     返回/设置 班次名称
        [Description("班次名称")]
        public string Name { get; set; }
        [Description("夜间工时起算时间")]
        public string NightWorkTime { get; set; }
        [Description("僅產生累時計算參數")]
        public bool OnlyActualSalaryKey { get; set; }
        //
        // 摘要:
        //     返回/设置 所有者ID
        public string OwnerId { get; set; }
        //
        // 摘要:
        //     返回/设置 班次图例ID
        [Description("班次图例ID")]
        public string RankLegendId { get; set; }
        //
        // 摘要:
        //     返回/设置 备注
        [Description("备注")]
        public string Remark { get; set; }
        //
        // 摘要:
        //     返回 班次休息时间---集合

        public List<AttendanceRankRest> Rests { get; set; }
        public AttendanceRank()
        {
            Rests = new List<AttendanceRankRest>();
        }

        [Description("彈性點名計算方式")]
        public int RollCallForFelx { get; set; }
        //
        // 摘要:
        //     返回/设置 班次简称
        [Description("班次简称")]
        public string ShortName { get; set; }
        //
        // 摘要:
        //     返回/设置 工作开始时间
        [Description("工作开始时间")]
        public string WorkBeginTime { get; set; }
        //
        // 摘要:
        //     返回/设置 工作结束时间
        public string WorkEndTime { get; set; }
        //
        // 摘要:
        //     返回/设置 工作时数
        [Description("工作时数")]
        public decimal WorkHours { get; set; }
    }

    public class AttendanceRankRest {
        // 摘要:
        //     返回/设置 班次休息时间ID
        [Description("班次时间ID")]
        public Guid AttendanceRankRestId { get; set; }
        //
        // 摘要:
        //     返回/设置 班段类别
        [Description("班段类别")]
        public string AttendanceRankTypeId { get; set; }
        //
        // 摘要:
        //     返回/设置 班段编号
        [Description("班段编号")]
        public int Code { get; set; }
        //
        // 摘要:
        //     返回/设置 所属公司ID
        public Guid CorporationId { get; set; }
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
        //     返回/设置 就餐假勤类型
        [Description("就餐假勤类型")]
        public string DinnerAttendanceType { get; set; }
        //
        // 摘要:
        //     返回/设置 就餐类型
        [Description("就餐类型")]
        public string DinnerTypeId { get; set; }
        //
        // 摘要:
        //     返回/设置 自定义早退假勤类型
        [Description("自定义早退假勤类型")]
        public string EarlyAttendanceType { get; set; }
        //
        // 摘要:
        //     返回/设置 是否可用
        public bool Flag { get; set; }
        //
        // 摘要:
        //     返回/设置 是否依刷卡判断下班考勤
        [Description("是否依刷卡判断下班考勤")]
        public bool IsCardOffDuty { get; set; }
        //
        // 摘要:
        //     返回/设置 是否依刷卡判断上班考勤
        [Description("是否依刷卡判断上班考勤")]
        public bool IsCardOnDuty { get; set; }
        //
        // 摘要:
        //     返回/设置 早退是否启用自定义假勤类型
        [Description("早退是否启用自定义假勤类型")]
        public bool IsEarlyForStart { get; set; }
        //
        // 摘要:
        //     返回/设置 迟到是否启用自定义假勤类型
        [Description("迟到是否启用自定义假勤类型")]
        public bool IsLateForStart { get; set; }
        //
        // 摘要:
        //     返回/设置 是否允许迟到转全天旷工
        [Description("是否允许迟到转全天旷工")]
        public bool IsLateFullAbsent { get; set; }
        //
        // 摘要:
        //     返回/设置 是否允许迟到转半天旷工
        [Description("是否允许迟到转半天旷工")]
        public bool IsLateHalfAbsent { get; set; }
        //
        // 摘要:
        //     返回/设置 請假允許遲到忽略分鐘數
        [Description("請假允許遲到忽略分鐘數")]
        public bool IsLateOverLookIncloudLeave { get; set; }
        //
        // 摘要:
        //     返回/设置 是否允许早退转全天旷工
        [Description("是否允许早退转全天旷工")]
        public bool IsLeaveFullAbsent { get; set; }
        //
        // 摘要:
        //     返回/设置 是否允许早退转半天旷工
        [Description("是否允许早退转半天旷工")]
        public bool IsLeaveHalfAbsent { get; set; }
        //
        // 摘要:
        //     返回/设置 請假允許早退忽略分鐘數
        [Description("請假允許早退忽略分鐘數")]
        public bool IsLeaveOverLookIncloudLeave { get; set; }
        //
        // 摘要:
        //     返回/设置 单次未刷卡是否启用自定义假勤类型
        [Description("单次未刷卡是否启用自定义假勤类型")]
        public bool IsNoCardForAbsent { get; set; }
        //
        // 摘要:
        //     返回/设置 提前是否算加班
        [Description("提前是否算加班")]
        public bool IsOTAdvance { get; set; }
        //
        // 摘要:
        //     返回/设置 延后是否算加班
        [Description("延后是否算加班")]
        public bool IsOTDelay { get; set; }
        //
        // 摘要:
        //     返回/设置 是否跨天ID
        [Description("是否跨天ID")]
        public string IsOverZeroId { get; set; }
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
        //     返回/设置 自定义迟到假勤类型
        [Description("自定义迟到假勤类型")]
        public string LateAttendanceType { get; set; }
        //
        // 摘要:
        //     返回/设置 迟到转全天旷工的分钟数
        [Description("迟到转全天旷工的分钟数")]
        public decimal LateFullAbsentTimes { get; set; }
        //
        // 摘要:
        //     返回/设置 迟到转半天旷工的分钟数
        [Description("迟到转半天旷工的分钟数")]
        public decimal LateHalfAbsentTimes { get; set; }
        //
        // 摘要:
        //     返回/设置 班段迟到允许忽略最大分钟数
        [Description("班段迟到允许忽略最大分钟数")]
        public decimal LateOverlookTimes { get; set; }
        //
        // 摘要:
        //     返回/设置 早退转全体旷工的分钟数
        [Description("早退转全天旷工的分钟数")]
        public decimal LeaveFullAbsentTimes { get; set; }
        //
        // 摘要:
        //     返回/设置 早退转半天旷工的分钟数
        [Description("早退转半天旷工的分钟数")]
        public decimal LeaveHalfAbsentTimes { get; set; }
        //
        // 摘要:
        //     返回/设置 班段早退允许忽略最大分钟数
        [Description("班段早退允许忽略最大分钟数")]
        public decimal LeaveOverlookTimes { get; set; }
        //
        // 摘要:
        //     返回/设置 上班允许最早提前刷卡分钟
        [Description("上班允许最早提前刷卡分钟")]
        public decimal MaxTimes { get; set; }
        //
        // 摘要:
        //     返回/设置 上班允许最迟延后刷卡分钟
        [Description("上班允许最迟延后刷卡分钟")]
        public decimal MaxTimesEnd { get; set; }
        //
        // 摘要:
        //     返回/设置 最小加班计算起算分钟
        [Description("最小加班计算起算分钟")]
        public decimal MinOTTimes { get; set; }
        //
        // 摘要:
        //     返回/设置 下班允许最晚刷卡分钟数
        [Description("下班允许最晚刷卡分钟")]
        public decimal MinTimes { get; set; }
        //
        // 摘要:
        //     返回/设置 下班允许最早刷卡分钟数
        [Description("下班允许最早刷卡分钟")]
        public decimal MinTimesBegin { get; set; }
        //
        // 摘要:
        //     返回/设置 班段名称
        [Description("班段名称")]
        public string Name { get; set; }
        //
        // 摘要:
        //     返回/设置 自定义旷工假勤类型
        [Description("自定义旷工假勤类型")]
        public string NoCardForABsentType { get; set; }
        //
        // 摘要:
        //     返回/设置 不算在在岗时间内
        [Description("不算在在岗时间内")]
        public bool NotJobTime { get; set; }
        //
        // 摘要:
        //     返回/设置 加班审核开始时间
        [Description("加班审核开始时间")]
        public bool OTAduitBegin { get; set; }
        //
        // 摘要:
        //     返回/设置 加班刷卡提前分钟数
        [Description("加班刷卡提前分钟数")]
        public decimal OTAdvanceMin { get; set; }
        //
        // 摘要:
        //     返回/设置 加班开始时间最小审核量
        [Description("加班开始时间最小审核量")]
        public decimal OTBeginMinAmount { get; set; }
        //
        // 摘要:
        //     返回/设置 加班开始时间
        [Description("加班开始时间")]
        public string OtBeginTime { get; set; }
        //
        // 摘要:
        //     返回/设置 加班刷卡推后分钟数
        [Description("加班刷卡推后分钟数")]
        public decimal OTDelayMin { get; set; }
        //
        // 摘要:
        //     返回/设置 加班最大分钟数
        [Description("加班最大分钟数")]
        public decimal OTMaxMin { get; set; }
        //
        // 摘要:
        //     返回/设置 加班扣除用餐时数
        [Description("加班扣除用餐时数")]
        public decimal OTNotDinnertimes { get; set; }
        //
        // 摘要:
        //     返回/设置 班段间隔时间
        [Description("班段间隔时间")]
        public decimal RankDistanceMinutes { get; set; }
        //
        // 摘要:
        //     返回/设置 备注
        [Description("备注")]
        public string Remark { get; set; }
        //
        // 摘要:
        //     返回/设置 开始时间
        [Description("开始时间")]
        public string RestBeginTime { get; set; }
        //
        // 摘要:
        //     返回/设置 结束时间
        [Description("结束时间")]
        public string RestEndTime { get; set; }
        //
        // 摘要:
        //     返回/设置 时数
        [Description("时数")]
        public decimal RestHours { get; set; }
        //
        // 摘要:
        //     返回/设置 加班单次核算分钟数
        [Description("加班单次核算分钟数")]
        public decimal SingleOTTimes { get; set; }
    }
}
