using BQHRWebApi.Business;
using BQHRWebApi.Common;
using BQHRWebApi.Service;
using Dcms.Common;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using Microsoft.AspNetCore.Mvc;

namespace BQHRWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class AttendanceRankChangeController
    {
        [HttpPost("checkbeforesaveforapi")]
        public async Task<ApiResponse> CheckBeforeSaveForAPI(List<AttendanceRankChangeForAPI> input)
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
                AttendanceRankChangeService service = new AttendanceRankChangeService();
                APIExResponse aPIExResponse = await service.CheckForESS(input.ToArray());

                return ResponseAnalysis.ToApiResponse(aPIExResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException || ex is BusinessRuleException) ? ex.Message : ex.ToString());
            }
        }

        [HttpPost("saveforapi")]
        public async Task<ApiResponse> SaveForAPI(List<AttendanceRankChangeForAPI> input)
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
                AttendanceRankChangeService service = new AttendanceRankChangeService();
                APIExResponse aPIExResponse = await service.BatchSave(input.ToArray());

                return ResponseAnalysis.ToApiResponse(aPIExResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException || ex is BusinessRuleException) ? ex.Message : ex.ToString());
            }
        }

    }
}
