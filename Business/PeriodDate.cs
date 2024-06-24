namespace BQHRWebApi.Business
{
    public class PeriodDate : DataEntity
    {

        public DateTime BeginDate { get; set; }= DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.MaxValue;
    }
}
