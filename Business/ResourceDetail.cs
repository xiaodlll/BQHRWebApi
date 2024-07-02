namespace BQHRWebApi.Business
{
    public class ResourceDetail : DataEntity
    {
        public string? ResourceDetailId { get; set; }//資源項目異動明細ID

        public string? ResourceItemNoId { get; set; }//資源品號明細ID

        public string ResourceItemId { get; set; }//资源项目ID

        public string SerialNo { get; set; }//異動序號

        public DateTime TransDate { get; set; }//異動日期

        public string IOType { get; set; }//I/o別

        public string AlertType { get; set; }//異動別

        public decimal Qty { get; set; }//數量

        public string? ResourceId { get; set; }//來源單據ID

        public string? ResourceFrom { get; set; }//來源單別

        public string? Remark { get; set; }//備註
        public string? CorporationId { get; set; }


    }
}
