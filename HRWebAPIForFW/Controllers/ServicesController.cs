using Dcms.HR.DataEntities;
using HRWebApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace HRWebApi.Controllers
{
    [RoutePrefix("api/services")]
    public class ServicesController : ApiController
    {

        // POST api/services
        public async Task<IHttpActionResult> Post(CallServiceBindingModel model)
        {
            IHttpActionResult httpActionResult;

            APIExResponse result = new APIExResponse();
            try
            {
                //LoggerHelper.Info(string.Format("Post Request:{0}", JsonConvert.SerializeObject(model).ToString()));
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var apiRequest = ApiConfig.GetApiByCode(model.RequestCode);
                if (apiRequest == null)
                {
                    throw new ApplicationException(model.RequestCode);
                }
                else
                {
                    if (apiRequest.Parameters != null && apiRequest.Parameters.Length > 0)
                    {
                        var dic = apiRequest.Parameters.ToDictionary(p => p.Name);
                        if (model.Parameters != null && model.Parameters.Length > 0)
                        {
                            foreach (APIRequestParameter para in model.Parameters)
                            {
                                if (dic.ContainsKey(para.Name))
                                {
                                    dic[para.Name].Value = para.Value; //合并参数
                                }
                            }
                        }
                        foreach (var para in dic.Values)
                        {
                            BuildParameterValue(para);   //解析{@me}
                        }
                        apiRequest.Parameters = dic.Values.ToArray();
                    }
                }

                result = await HRServerHelper.CallServiceEx(apiRequest);

            }
            catch (Exception ex)
            {
                result.State = "-1";
                result.Msg = ex.Message;
            }
            return Ok(result);
        }

        void BuildParameterValue(APIRequestParameter para)
        {
            if (para.Value == null) return;

            //if (para.Value.Equals("{@me}"))
            //{
            //    para.Value = User.Identity.GetUserId();
            //}
            //else if (para.Value.Equals("{@username}"))
            //{
            //    para.Value = User.Identity.GetUserName();
            //}
        }

    }
}
