using Dcms.HR.Services;
using Neway.License.Service;

namespace BQHRWebApi.Common
{
    public class Authorization
    {
        public static void CheckAuthorization()
        {
            return;

            string licensePath = AppDomain.CurrentDomain.BaseDirectory + @"\License.ini";

            if (File.Exists(licensePath) == false)
            {
                throw new AuthorizationException("系统未能获取到有效的授权信息");
            }

            var licenseService = new LicenseService();
            LicenseRequest licenseRequest = licenseService.CheckAuthorization(licensePath);
            if (licenseRequest.IsSuceed == false)
            {
                throw new AuthorizationException(string.Format("系统未能获取到有效的授权信息！{0}", licenseRequest.Message));

            }

            #region  验证名称和服务器硬件数据

            string license = licenseRequest.License;
            string hospData = license.Split('&')[0];
            string[] hospDataArr = hospData.Split('|');
            string hospName = hospDataArr[0];
            if (hospDataArr.Length < 1)
            {
                throw new AuthorizationException("系统未能获取到有效的授权信息");
            }
            #endregion

            #region 验证授权数据

            string authorizationData = license.Split('&')[1];
            string[] authorizationDataArr = authorizationData.Split('|');
            string versionType = authorizationDataArr[0];
            string endDate = authorizationDataArr[1];
            string controlType = authorizationDataArr[2];
            int warningDays = int.Parse(authorizationDataArr[3]);
            endDate = endDate.Replace("��", "").Replace("-", "");
            string endDateStr = endDate.Substring(0, 4) + "-" + endDate.Substring(4, 2) + "-" + endDate.Substring(6, 2);
            DateTime now = DateTime.Now;
            int remainDays = DateTime.Parse(endDateStr).Subtract(now.Date).Days; //软件授权剩余使用天数（不包含当天）
            if (versionType == "1") //试用版
            {
                throw new AuthorizationException(string.Format("试用版（{0}到期）",
                    DateTime.Parse(endDateStr).AddDays(1).ToString("yyyy年MM月dd日")));
            }
            if (remainDays < 0) //已经过期
            {
                if (versionType == "0") //正式版
                {
                    if (controlType != "0") //无法使用
                    {
                        throw new AuthorizationException(string.Format("本系统售后服务已于{0}到期，请尽早与开发商联系。", endDateStr));
                    }
                }
                else //试用版
                {
                    throw new AuthorizationException(string.Format("本系统试用版授已于{0}到期，请尽早与开发商联系。", endDateStr));
                }
            }
            #endregion

        }
    }
}
