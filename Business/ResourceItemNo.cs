namespace BQHRWebApi.Business
{
    public class ResourceItemNo : DataEntity
    {
        public string? ResourceItemNoId { get; set; }

        public string? ResourceItemId { get; set; }

        public int SerialNo { get; set; }

        public string? Item { get; set; }

        public string? ApplyEmployeeId { get; set; }

        public DateTime? InDate { get; set; }

        public DateTime? OutDate { get; set; }
        public bool Mayloan { get; set; }
    }
}
