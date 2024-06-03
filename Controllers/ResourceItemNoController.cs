using BQHRWebApi.Business;
using BQHRWebApi.Common;
using BQHRWebApi.Service;
using Dcms.HR.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Dynamic;

namespace BQHRWebApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ResourceItemNoController : ControllerBase
    {


        private readonly ILogger<ResourceItemNoController> _logger;

        public ResourceItemNoController(ILogger<ResourceItemNoController> logger)
        {
            _logger = logger;
        }

        [HttpPost("batchadd")]
        public ApiResponse AddResourceItemNo(List<ResourceItemNo> input)
        {
            try
            {

                if (input != null && input.Count > 0)
                {
                    ResourceItemNoService service = new ResourceItemNoService();
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
        public ApiResponse DeleteResourceItemNo(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    ResourceItemNoService service = new ResourceItemNoService();
                    service.DeleteResourceItemNo(id);
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

        [HttpGet("getiteminfobyitemid")]
        public ApiResponse GetItemInfoByItemId(string itemId)
        {
            try
            {
                ResourceItemNoService service = new ResourceItemNoService();
                DataTable table = service.GetItemInfoByItemId(itemId);
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
