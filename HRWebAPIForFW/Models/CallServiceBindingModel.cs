using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Dcms.HR.Business;
using Dcms.HR.DataEntities;

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