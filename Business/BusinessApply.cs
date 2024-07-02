#region 程序集 Dcms.HR.Business.Business.dll, v4.0.30319
// F:\SourceCenter\Product\V5.1 10\Export\Dcms.HR.Business.Business.dll
#endregion

namespace BQHRWebApi.Business
{
    using System;
    using System.Collections.Generic;


    /// <summary>
    /// 出差申請實體
    /// </summary>
    [Serializable()]
    public class BusinessApplyForAPI : DataEntity
    {
        /// <summary>
        /// 建構
        /// </summary>
        public BusinessApplyForAPI()
        {
            Persons = new List<BusinessApplyPersonForAPI>();

            Schedules = new List<BusinessApplyScheduleForAPI>();

            //Attendances = new List<BusinessApplyAttendanceForAPI>();
        }
        /// <summary>
        /// 返回/設置 所屬公司ID
        /// </summary>
        public System.Guid? CorporationId { get; set; }


        public System.String CorpCode { get; set; }
        /// <summary>
        /// 返回/設置 申請單編碼
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 返回/設置 申請單名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 返回/設置 開始日期
        /// </summary>
        public System.DateTime BeginDate { get; set; }

        /// <summary>
        /// 返回/設置 開始時間
        /// </summary>
        public string BeginTime { get; set; }

        /// <summary>
        /// 返回/設置 結束日期
        /// </summary>
        public System.DateTime EndDate { get; set; }

        /// <summary>
        /// 返回/設置 結束時間
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 返回/設置 出差類別ID
        /// </summary>
        public string AttendanceTypeId { get; set; }

        /// <summary>
        /// 返回/設置 出差地點
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 返回/設置 出差原因
        /// </summary>
        public string Reason { get; set; }


        /// <summary>
        /// 返回/設置 是否登記
        /// </summary>
        public bool? IsRegister { get; set; }

        /// <summary>
        /// 返回/設置 是否扣除休息就餐段
        /// </summary>
        public bool? IsCheckRest { get; set; }



        /// <summary>
        /// 返回/設置 備註
        /// </summary>
        public string? Remark { get; set; }

        public string FoundEmpCode { get; set; }

        public Guid? FoundEmployeeId { get; set; }


        public Guid? BusinessApplyId { get; set; }

        #region Collection properties
        /// <summary>
        /// 返回 出差人員-集合
        /// </summary>
        public List<BusinessApplyPersonForAPI> Persons { get; set; }

        /// <summary>
        /// 返回 出差安排-集合
        /// </summary>
        public List<BusinessApplyScheduleForAPI> Schedules { get; set; }

        /// <summary>
        /// 返回 刷卡計畫-集合
        /// </summary>
        //public List<BusinessApplyAttendanceForAPI> Attendances { get; set; }
        #endregion




    }


    ///// <summary>
    ///// 刷卡計畫實體
    ///// </summary>
    //[Serializable()]
    //public class BusinessApplyAttendanceForAPI
    //{
    //    /// <summary>
    //    /// 返回/設置 員工ID
    //    /// </summary>
    //    public System.Guid? EmployeeId { get; set; }

    //    /// <summary>
    //    /// 返回/設置 員工ID
    //    /// </summary>
    //    public System.String EmployeeCode { get; set; }
    //    /// <summary>
    //    /// 返回/設置 計畫日期
    //    /// </summary>
    //    public System.DateTime Date { get; set; }

    //    /// <summary>
    //    /// 返回/設置 開始日期
    //    /// </summary>
    //    public System.DateTime BeginDate { get; set; }

    //    /// <summary>
    //    /// 返回/設置 開始時間
    //    /// </summary>
    //    public string BeginTime { get; set; }

    //    /// <summary>
    //    /// 返回/設置 結束日期
    //    /// </summary>
    //    public System.DateTime EndDate { get; set; }

    //    /// <summary>
    //    /// 返回/設置 結束時間
    //    /// </summary>
    //    public string EndTime { get; set; }


    //    /// <summary>
    //    /// 返回/設置 出差時數
    //    /// </summary>
    //    public decimal? ApplyHours { get; set; }
    //    /// <summary>
    //    /// 返回/設置 刷卡說明
    //    /// </summary>
    //    public string? Remark { get; set; }

    //}


    /// <summary>
    /// 出差安排實體
    /// </summary>
    [Serializable()]
    public class BusinessApplyScheduleForAPI
    {

        /// <summary>
        /// 返回/設置 開始日期
        /// </summary>
        public System.DateTime BeginDate { get; set; }

        /// <summary>
        /// 返回/設置 結束日期
        /// </summary>
        public System.DateTime EndDate { get; set; }


        /// <summary>
        /// 返回/設置 拜訪對象
        /// </summary>
        public string? VisitObject { get; set; }

        /// <summary>
        /// 返回/設置 出差安排
        /// </summary>
        public string? Remark { get; set; }

    }

    /// <summary>
    /// 出差人員實體
    /// </summary>
    [Serializable()]
    public class BusinessApplyPersonForAPI
    {


        /// <summary>
        /// 返回/設置 員工ID
        /// </summary>
        public System.Guid? EmployeeId { get; set; }

        /// <summary>
        /// 返回/設置 主要代理人ID
        /// </summary>
        public System.Guid? DeputyEmployeeId { get; set; }

        /// <summary>
        /// 返回/設置 員工ID
        /// </summary>
        public System.String EmployeeCode { get; set; }

        /// <summary>
        /// 返回/設置 主要代理人ID
        /// </summary>
        public System.String? DeputyEmployeeCode { get; set; }
        /// <summary>
        /// 返回/設置 是否登記
        /// </summary>
        public bool? IsRegister { get; set; }

        /// <summary>
        /// 返回/設置 代理事項說明
        /// </summary>
        public string? Remark { get; set; }

    }
}
