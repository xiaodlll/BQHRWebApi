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
    public class ResourceDetailController : ControllerBase
    {
        private readonly ILogger<ResourceDetailController> _logger;

        public ResourceDetailController(ILogger<ResourceDetailController> logger)
        {
            _logger = logger;
        }

        [HttpPost("batchadd")]
        public ApiResponse AddResourceDetail(List<ResourceDetail> input)
        {
            try
            {
                if (input != null && input.Count > 0)
                {
                    ResourceDetailService service = new ResourceDetailService();
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
       

        [HttpGet("getalerttype")]
        public ApiResponse GetAlertType()
        {
            try
            {
                ResourceDetailService service = new ResourceDetailService();
                DataTable table = service.GetAlertType();
                List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(table);
                return ApiResponse.Success("Success", dynamicObjects);
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }

        }

        [HttpGet("getiotype")]
        public ApiResponse GetIOType()
        {
            try
            {
                ResourceDetailService service = new ResourceDetailService();
                DataTable table = service.GetIOType();
                List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(table);
                return ApiResponse.Success("Success", dynamicObjects);
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }

        }

        [HttpGet("getresourcefrom")]
        public ApiResponse GetResourceFrom()
        {
            try
            {
                ResourceDetailService service = new ResourceDetailService();
                DataTable table = service.GetResourceFrom();
                List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(table);
                return ApiResponse.Success("Success", dynamicObjects);
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }

        }
    }
}
