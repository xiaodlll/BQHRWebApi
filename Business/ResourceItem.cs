namespace BQHRWebApi.Business
{
    /// <summary>
    /// 资源项目
    /// </summary>
    public class ResourceItem : DataEntity
    {
        public string? ResourceItemId { get; set; }
        public string? CorporationId { get; set; }
        /// <summary>
        /// 是否需要归还
        /// </summary>
        public string IsReturnId { get; set; }

        /// <summary>
        /// 资源大类
        /// </summary>
        public string ResourceKindId { get; set; }

        /// <summary>
        /// 期初金额
        /// </summary>
        public decimal? Sum{ get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark{ get; set; }
        /// <summary>
        /// 项目编码
        /// </summary>
        public string Code{ get; set; }
        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name{ get; set; }
        /// <summary>
        /// 资源分类细项
        /// </summary>
        public string? ResourceGroup{ get; set; }
        /// <summary>
        /// 規格型號
        /// </summary>
        public string? Model{ get; set; }
        /// <summary>
        /// 借用期限
        /// </summary>
        public string? BorrowPeriod{ get; set; }
        /// <summary>
        /// 資源品項單位
        /// </summary>
        public string? ItemUnit{ get; set; }
        /// <summary>
        /// 目前庫存量
        /// </summary>
        public decimal? Quantity{ get; set; }
        /// <summary>
        /// 要作品號管理
        /// </summary>
        public bool IsManagement{ get; set; }
       
        /// <summary>
        /// 单价
        /// </summary>
        public decimal? Price{ get; set; }

    }
}
