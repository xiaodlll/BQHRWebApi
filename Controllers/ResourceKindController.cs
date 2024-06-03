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
    public class ResourceKindController : ControllerBase
    {

        private readonly ILogger<ResourceKindController> _logger;

        public ResourceKindController(ILogger<ResourceKindController> logger)
        {
            _logger = logger;
        }

        [HttpPost("batchadd")]
        public ApiResponse AddResourceKind(List<ResourceKind> input)
        {
            try
            {

                if (input != null && input.Count > 0)
                {
                    ResourceKindService service = new ResourceKindService();
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

        [HttpDelete("delete")]
        public ApiResponse DeleteResourceKind(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    ResourceKindService service = new ResourceKindService();
                    service.DeleteResourceKind(id);
                }
                else
                {
                    return ApiResponse.Fail("id不能为空!");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }
            return ApiResponse.Success();
        }

        [HttpGet("getresourcekind")]
        public ApiResponse GetAllResourceKind()
        {
            try
            {
                ResourceKindService service = new ResourceKindService();
                DataTable table = service.GetAllResourceKind();
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
