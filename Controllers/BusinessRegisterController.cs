using BQHRWebApi.Business;
using BQHRWebApi.Common;
using BQHRWebApi.Service;
using Dcms.HR.Services;
using Microsoft.AspNetCore.Mvc;

namespace BQHRWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class BusinessRegisterController
    {
        [HttpPost("checkbeforesaveforapi")]
        public async Task<ApiResponse> CheckBeforeSaveForAPI(List<BusinessRegisterForAPI> input)
        {
            try
            {
                Authorization.CheckAuthorization();
            }
            catch (AuthorizationException aEx)
            {
                return ApiResponse.Fail("授权:" + aEx.Message);
            }

            try
            {
                BusinessRegisterService service = new BusinessRegisterService();
                service.CheckForESS(input.ToArray());

                return ApiResponse.Success("Success");
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }
        }

        [HttpPost("saveforapi")]
        public async Task<ApiResponse> SaveForAPI(List<BusinessRegisterForAPI> input)
        {
            try
            {
                Authorization.CheckAuthorization();
            }
            catch (AuthorizationException aEx)
            {
                return ApiResponse.Fail("授权:" + aEx.Message);
            }

            try
            {
                BusinessRegisterService service = new BusinessRegisterService();
                service.Save(input.ToArray());

                return ApiResponse.Success("Success");

            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }
        }

    }
}
