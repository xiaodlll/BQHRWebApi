using Dcms.Common.Torridity;
using Dcms.Common.Torridity.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dcms.HR.DataEntities
{

    /// <summary>
    /// 个案附加实体
    /// </summary>
    [DataEntity(PrimaryKey = "ExtendItemId")]
    [Serializable()]
    [Description("个案附加")]
    public class ExtendItem : DataEntity
    {

        public const string TYPE_KEY = "ExtendItem";

        #region Simple Property
        private string _extendItemId;
        /// <summary>
        /// 返回/设置 主键
        /// </summary>
        [SimpleProperty(DbType = GeneralDbType.Guid)]
        public string ExtendItemId
        {
            get { return _extendItemId; }
            set
            {
                if (_extendItemId != value)
                {
                    _extendItemId = value;
                    OnPropertyChanged("ExtendItemId");
                }
            }
        }
        #endregion
    }

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
