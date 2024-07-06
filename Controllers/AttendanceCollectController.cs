using BQHRWebApi.Business;
using BQHRWebApi.Common;
using BQHRWebApi.Service;
using Dcms.HR.Services;
using Microsoft.AspNetCore.Mvc;

namespace BQHRWebApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AttendanceCollectController : ControllerBase
    {

        private readonly ILogger<AttendanceCollectController> _logger;

        public AttendanceCollectController(ILogger<AttendanceCollectController> logger)
        {
            _logger = logger;
        }

        [HttpPost("BatchAdd")]
        public async Task<ApiResponse> AddAttendanceCollect(AttendanceCollectForAPI[] input)
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
                if (input != null && input.Length > 0)
                {
                    AttendanceCollectService service = new AttendanceCollectService();
                    await service.SaveCollects(input);
                }
                else
                {
                    return ApiResponse.Fail("数据传入格式不正确!");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }
            return ApiResponse.Success();
        }


    }
}
