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
    public class ResourceItemController : ControllerBase
    {


        private readonly ILogger<ResourceItemController> _logger;

        public ResourceItemController(ILogger<ResourceItemController> logger)
        {
            _logger = logger;
        }

        [HttpPost("batchadd")]
        public ApiResponse AddResourceItem(List<ResourceItem> input)
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
                    ResourceItemService service = new ResourceItemService();
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
        public ApiResponse DeleteResourceItem(string id)
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
                if (!string.IsNullOrEmpty(id))
                {
                    ResourceItemService service = new ResourceItemService();
                    service.DeleteResourceItem(id);
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

        [HttpGet("getresourcegroup")]
        public ApiResponse GetResourceGroup() {
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
                ResourceItemService service = new ResourceItemService();
                DataTable table = service.GetResourceGroup();
                List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(table);
                return ApiResponse.Success("Success", dynamicObjects);
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }
          
        }

        [HttpGet("getisreturnid")]
        public ApiResponse GetIsReturnId()
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
                ResourceItemService service = new ResourceItemService();
                DataTable table = service.GetIsReturnId();
                List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(table);
                return ApiResponse.Success("Success", dynamicObjects);
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }

        }


        [HttpGet("getborrowperiod")]
        public ApiResponse GetBorrowPeriod()
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
                ResourceItemService service = new ResourceItemService();
                DataTable table = service.GetBorrowPeriod();
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
