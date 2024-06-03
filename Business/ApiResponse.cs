namespace BQHRWebApi.Business
{
    public class ApiResponse
    {
        /// <summary>
        /// 响应
        /// </summary>
        public ApiResponse() { 
        
        }

        /// <summary>
        /// 0.成功 ,-1 失败
        /// </summary>
        public int result
        {
            get; set;
        }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string errorMsg
        {
            get; set;
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string description
        {
            get; set;
        }

        /// <summary>
        /// 返回值
        /// </summary>
        public Object data
        {
            get; set;
        }

        public static ApiResponse Fail(string error, string des = "调用失败!")
        {
            ApiResponse apiResponse = new ApiResponse();
            apiResponse.result = -1;
            apiResponse.errorMsg = error;
            apiResponse.description = des;
            return apiResponse;
        }

        public static ApiResponse Success(string des = "调用成功!", Object data=null)
        {
            ApiResponse apiResponse = new ApiResponse();
            apiResponse.result = 0;
            apiResponse.errorMsg = string.Empty;
            apiResponse.description = des;
            apiResponse.data = data;
            return apiResponse;
        }
    }
}
