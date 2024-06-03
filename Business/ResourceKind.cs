namespace BQHRWebApi.Business
{
    public class ResourceKind: DataEntity
    {
        public string? ResourceKindId { get; set; }

        public string? CorporationId { get; set; }
        /// <summary>
        /// 资源大类编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 资源大类名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
