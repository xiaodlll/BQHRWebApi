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
    public class BusinessApplyController : ControllerBase
    {


        private readonly ILogger<BusinessApplyController> _logger;

        public BusinessApplyController(ILogger<BusinessApplyController> logger)
        {
            _logger = logger;
        }

        //[HttpPost("batchccsq")]
        //public ApiResponse AddBusinessApply(List<BusinessApplyForAPI> input)
        //{
        //    try
        //    {
        //        Authorization.CheckAuthorization();
        //    }
        //    catch (AuthorizationException aEx)
        //    {
        //        return ApiResponse.Fail("授权:" + aEx.Message);
        //    }

        //    try
        //    {

        //        if (input != null && input.Count > 0)
        //        {
        //            BusinessApplyService service = new BusinessApplyService();
        //            service.Save(input.ToArray());
        //        }
        //        else
        //        {
        //            return ApiResponse.Fail("数据传入格式不正确!");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
        //    }
        //    return ApiResponse.Success();
        //}

        [HttpPost("batchcheckforccsq")]
        public async Task<ApiResponse> CheckCCSQForAPI(BusinessApplyForAPI[] input)
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
                BusinessApplyService service = new BusinessApplyService();
              string s= await service.CheckForCCSQ(input.ToArray());
                if (s != "")
                {
                    return ApiResponse.Fail(s);
                }
                return ApiResponse.Success("Success");

            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }
        }

        [HttpPost("batchsaveforccsq")]
        public async Task<ApiResponse> SaveCCSQForAPI(string formNumber, BusinessApplyForAPI[] input)
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
                BusinessApplyService service = new BusinessApplyService();
                 service.SaveForCCSQ(formNumber, input);
                return ApiResponse.Success("Success");

            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }
        }

    }
}
