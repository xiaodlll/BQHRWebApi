namespace BQHRWebApi.Common
{
    public class BusinessException: Exception
    {
        public BusinessException(string error):base(error) { 
        }
    }
}
