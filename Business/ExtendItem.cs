using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using System.Globalization;
using Newtonsoft.Json;

namespace BQHRWebApi.Business
{

    public class APIRequest : ICloneable
    {
        [JsonProperty("ServiceType")]
        public string ServiceType { get; set; }

        [JsonProperty("Method")]
        public string Method { get; set; }

        [JsonProperty("Parameters")]
        public APIRequestParameter[] Parameters { get; set; }

        public object Clone()
        {
            APIRequest aPIRequest = new APIRequest();
            aPIRequest.ServiceType = ServiceType;
            aPIRequest.Method = Method;
            if (Parameters != null && Parameters.Length > 0)
            {
                List<APIRequestParameter> list = new List<APIRequestParameter>();
                APIRequestParameter[] parameters = Parameters;
                foreach (APIRequestParameter aPIRequestParameter in parameters)
                {
                    list.Add(aPIRequestParameter.Clone() as APIRequestParameter);
                }

                aPIRequest.Parameters = list.ToArray();
            }

            return aPIRequest;
        }
    }
    public class APIRequestParameter : ICloneable
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        public object Clone()
        {
            APIRequestParameter aPIRequestParameter = new APIRequestParameter();
            aPIRequestParameter.Name = Name;
            aPIRequestParameter.Type = Type;
            aPIRequestParameter.Value = Value;
            return aPIRequestParameter;
        }
    }
    public class APIResponse
    {
        [JsonProperty("State")]
        public string State { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("ResultType")]
        public string ResultType { get; set; }

        [JsonProperty("ResultValue")]
        public string ResultValue { get; set; }
    }
    public class APIExResponse
    {
        [JsonProperty("State")]
        public string State { get; set; }

        [JsonProperty("Msg")]
        public string Msg { get; set; }

        [JsonProperty("ResultType")]
        public string ResultType { get; set; }

        [JsonProperty("ResultValue")]
        public object ResultValue { get; set; }
    }
}
