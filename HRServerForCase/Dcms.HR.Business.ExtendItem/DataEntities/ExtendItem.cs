using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using Dcms.Common.Torridity;
using Dcms.Common.Torridity.Metadata;
using System.Globalization;

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
}
