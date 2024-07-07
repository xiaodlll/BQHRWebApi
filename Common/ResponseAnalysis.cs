using BQHRWebApi.Business;
using Dcms.HR.DataEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BQHRWebApi.Common
{
    public class ResponseAnalysis
    {
        public static ApiResponse ToApiResponse(APIExResponse aPIExResponse)
        {
            if (aPIExResponse.State == "0")
            {
                //如果明细全部成功,则ok，否则失败
                bool allSuccess = true;
                if (aPIExResponse.ResultValue != null)
                {
                    string str = aPIExResponse.ResultValue.ToString();
                    if (!string.IsNullOrEmpty(str) && IsValidJson(str))
                    {
                        JArray jsonObject = JsonConvert.DeserializeObject<JArray>(str);
                        if (jsonObject != null)
                        {
                            foreach (JObject item in jsonObject)
                            {
                                if (item["Success"].ToString() != "True")
                                {
                                    allSuccess = false;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (allSuccess)
                {
                    return ApiResponse.Success("Success");
                }
                else
                {
                    return ApiResponse.Fail("Fail", "", aPIExResponse.ResultValue);
                }

            }
            else
            {
                return ApiResponse.Fail(aPIExResponse.Msg, "", aPIExResponse.ResultValue);
            }
        }

        private static bool IsValidJson(string input)
        {
            input = input.Trim();
            if ((input.StartsWith("{") && input.EndsWith("}")) || (input.StartsWith("[") && input.EndsWith("]")))
            {
                try
                {
                    JsonConvert.DeserializeObject(input);
                    return true;
                }
                catch (JsonException)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
