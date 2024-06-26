using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Dcms.HR.Business;
using Dcms.HR.DataEntities;
using Newtonsoft.Json;

namespace BQHRWebApi
{
    public class CallServiceBindingModel
    {
        [Required]
        [Display(Name = "请求Json编码")]
        public string RequestCode { get; set; }

        [Display(Name = "参数")]
        public APIRequestParameter[] Parameters { get; set; }
    }

    public class APIRequestParameter
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

    }
}