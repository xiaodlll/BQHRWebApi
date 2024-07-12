using BQHRWebApi.Business;
using BQHRWebApi.Common;
using BQHRWebApi.Service;
using Dcms.Common;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Dynamic;
using System.Text;

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


        //[HttpPost("checkbeforesaveforapi")]
        //public async Task<ApiResponse> CheckBeforeSaveForAPI(AttendanceLeaveForAPI input)
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
        //        AttendanceLeaveService service = new AttendanceLeaveService();
        //         string messg=  await service.SaveCheckForAPI(input);
        //        if (messg != "")
        //        {
        //            return ApiResponse.Fail(messg);
        //        }
        //         return ApiResponse.Success("Success");

        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
        //    }
        //}

        [HttpPost("gethoursforapi")]
        public async Task<ApiResponse> GetHoursForAPI(AttendanceLeaveForAPI input)
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
                DataTable messg = await service.GetLeaveHoursForCase(input);

                List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(messg);
                return ApiResponse.Success("Success", dynamicObjects);


            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException || ex is BusinessRuleException) ? ex.Message : ex.ToString());
            }
        }

        [HttpPost("batchgethoursforapi")]
        public async Task<ApiResponse> BatchGetHoursForAPI(AttendanceLeaveForAPI[] input)
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
                    AttendanceLeaveService service = new AttendanceLeaveService();
                    Dictionary<string, DataTable> messg = await service.MultiGetLeaveHours(input);

                    var dynamicObjects = HRHelper.ConvertDicToExpandoObjects(messg);
                    return ApiResponse.Success("Success", dynamicObjects);

                }
                else
                {
                    return ApiResponse.Fail("数据传入格式不正确!");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException || ex is BusinessRuleException) ? ex.Message : ex.ToString());
            }
        }

        //[HttpPost("saveforapi")]
        //public async Task<ApiResponse> SaveForAPI(string formNumber, AttendanceLeaveForAPI input)
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
        //        AttendanceLeaveService service = new AttendanceLeaveService();
        //        string messg = await service.SaveForAPI(formNumber, input);

        //        return ApiResponse.Success("Success");

        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
        //    }
        //}

        //[HttpPost("getfiscalyear")]
        //public ApiResponse GetFiscalYear()
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
        //        AttendanceLeaveService service = new AttendanceLeaveService();
        //        DataTable table = service.GetFiscalYear();
        //        List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(table);
        //        return ApiResponse.Success("Success", dynamicObjects);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
        //    }

        //}

        //[HttpPost("getleaverecordsforapi")]
        //public ApiResponse GetLeaveRecordsForAPI(string[] empCodes, DateTime date)
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
        //        AttendanceLeaveService service = new AttendanceLeaveService();
        //        Dictionary<int, DataTable> result = service.GetLeaveRecordsForAPI(empCodes,date);
        //        Dictionary<int, List<ExpandoObject>> dynamicObjects = new Dictionary<int, List<ExpandoObject>>();
        //        //List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(result);
        //        return ApiResponse.Success("Success", dynamicObjects);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
        //    }
        //}

        //[HttpPost("checkforapi")]
        //public ApiResponse CheckForAPI(List<AttendanceLeaveForAPI> input) {
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
        //        AttendanceLeaveService service = new AttendanceLeaveService();
        //        string result = service.CheckForAPI(input);
        //       // Dictionary<int, List<ExpandoObject>> dynamicObjects = new Dictionary<int, List<ExpandoObject>>();
        //        //List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(result);
        //        if(result.CheckNullOrEmpty())
        //          return ApiResponse.Success("Success");
        //        else return ApiResponse.Fail(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
        //    }
        //}

        //[HttpPost("getleavehoursforapi")]
        //public ApiResponse GetLeaveHoursForAPI(AttendanceLeaveForAPI formEntity) {
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
        //        AttendanceLeaveService service = new AttendanceLeaveService();
        //        DataTable table = service.GetLeaveHoursForAPI(formEntity);
        //        List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(table);
        //        return ApiResponse.Success("Success", dynamicObjects);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
        //    }
        //}



        [HttpPost("batchcheckleaves")]
        public async Task<ApiResponse> BatchCheckLeaves(AttendanceLeaveForAPI[] input)
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
                    AttendanceLeaveService service = new AttendanceLeaveService();
                    Dictionary<string, string> reT = await service.MultiCheckForAPI(input.ToArray());
                    StringBuilder s = new StringBuilder();
                    if (reT != null && reT.Keys.Count > 0)
                    {
                        List<ExpandoObject> dynamicObjects = HRHelper.ConvertToExpandoObjects(reT);
                        return ApiResponse.Success("Success", dynamicObjects);
                    }
                    return ApiResponse.Success("Success");
                }
                else
                {
                    return ApiResponse.Fail("数据传入格式不正确!");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException || ex is BusinessRuleException) ? ex.Message : ex.ToString());
            }
            return ApiResponse.Success();
        }


        [HttpPost("batchsaveleaves")]
        public async Task<ApiResponse> MultiSaveLeaves(AttendanceLeaveForAPI[] input)
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
                    AttendanceLeaveService service = new AttendanceLeaveService();
                    string s = await service.MultiSaveForAPI(input);
                    return ApiResponse.Success("Success", s);
                }
                else
                {
                    return ApiResponse.Fail("数据传入格式不正确!");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException || ex is BusinessRuleException) ? ex.Message : ex.ToString());
            }
            return ApiResponse.Success();
        }


        [HttpPost("checkrevokeforapi")]
        public async Task<ApiResponse> CheckRevokeForAPI(RevokeLeaveForAPI revokeEnty)
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
                string messg = await service.CheckRevokeForAPI(revokeEnty);
                if (messg.CheckNullOrEmpty() || messg == "success")
                {
                    return ApiResponse.Success("Success");
                }
                return ApiResponse.Fail(messg);
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException || ex is BusinessRuleException) ? ex.Message : ex.ToString());
            }
        }

        [HttpPost("saverevokeforapi")]
        public async Task<ApiResponse> SaveRevokeForAPI(RevokeLeaveForAPI revokeEnty)
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
              
                APIExResponse aPIExResponse = await service.SaveRevokeForAPI(revokeEnty);
                //if (messg.CheckNullOrEmpty() || messg == "success")
                //{
                //    return ApiResponse.Success("Success");
                //}
                //return ApiResponse.Fail(messg);
                return ResponseAnalysis.ToApiResponse(aPIExResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException || ex is BusinessRuleException) ? ex.Message : ex.ToString());
            }
        }

    }
}
