using Dcms.HR.DataEntities;
using System.ComponentModel.DataAnnotations;

namespace HRWebApi.Models
{
    public class CallServiceBindingModel
    {
        [Required]
        [Display(Name = "请求Json编码")]
        public string RequestCode { get; set; }

        [Display(Name = "参数")]
        public APIRequestParameter[] Parameters { get; set; }
    }

}