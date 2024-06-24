using BQHRWebApi.Business;
using BQHRWebApi.Common;
using BQHRWebApi.Service;
using Dcms.HR.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Dynamic;

namespace BQHRWebApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AttendanceLeaveController : ControllerBase
    {

        private readonly ILogger<AttendanceLeaveController> _logger;

        public AttendanceLeaveController(ILogger<AttendanceLeaveController> logger)
        {
            _logger = logger;
        }

        [HttpPost("BatchAdd")]
        public ApiResponse AddAttendanceLeave(List<AttendanceLeaveForAPI> input)
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
                if (input != null && input.Count > 0)
                {
                    AttendanceLeaveService service = new AttendanceLeaveService();
                    service.Save(input.ToArray());
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


        [HttpGet("getleaverecordsforapi")]
        public ApiResponse GetLeaveRecordsForAPI(string[] empCodes, DateTime date)
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
                AttendanceLeaveService service = new AttendanceLeaveService();
                Dictionary<int, DataTable> result = service.GetLeaveRecordsForAPI(empCodes,date);
                Dictionary<int, List<ExpandoObject>> dynamicObjects = new Dictionary<int, List<ExpandoObject>>();
                //List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(result);
                return ApiResponse.Success("Success", dynamicObjects);
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }
        }
    }
}
