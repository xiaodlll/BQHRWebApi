namespace Dcms.HR.Services
{
    public class BusinessException: Exception
    {
        public BusinessException(string error):base(error) { 
        }
    }

    public class AuthorizationException : Exception
    {
        public AuthorizationException(string error) : base(error)
        {
        }
    }
    
}
