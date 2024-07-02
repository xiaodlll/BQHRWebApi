using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.HR.Services;
using System.Data;

namespace BQHRWebApi.Service
{
    public class ResourceDetailService : HRService
    {


        public override void Save(DataEntity[] entities)
        {
            foreach (var entity in entities)
            {
                ResourceDetail enty = entity as ResourceDetail;
                if (enty != null)
                {

                }
                SaveResourceDetail(enty);
            }
        }



        public void SaveResourceDetail(ResourceDetail enty)
        {
            DataTable dtItem = new DataTable();
            if (string.IsNullOrEmpty(enty.ResourceItemId))
            {
                throw new Exception("ResourceItemId is null");
            }
            if (string.IsNullOrEmpty(enty.IOType))
            {
                throw new Exception("IOType is null");
            }
            if (string.IsNullOrEmpty(enty.AlertType))
            {
                throw new Exception("AlertType is null");
            }
            if (enty.IOType == "IOType_001" && enty.AlertType == "AlertType_002")//出 出库 项目需要品号管理 资源品号不能为空
            {

                string sqla = string.Format("select Quantity,IsManagement,ResourceItemId,Code from ResourceItem where ResourceItemId='{0}'", enty.ResourceItemId);
                dtItem = HRHelper.ExecuteDataTable(sqla);
                if (dtItem == null || dtItem.Rows.Count == 0)
                {
                    throw new Exception("ResourceItemId is wrong!");
                }
                bool isManagement = false;
                Boolean.TryParse(dtItem.Rows[0]["IsManagement"].ToString(), out isManagement);
                if (isManagement)
                {
                    if (string.IsNullOrEmpty(enty.ResourceItemNoId))
                    {
                        throw new Exception("ResourceItemNoId is null");
                    }
                }
            }


            if (enty.Qty == 0)
            {
                throw new Exception("Qty is null");
            }
            if (string.IsNullOrEmpty(enty.TransDate.ToString()))
            {
                throw new Exception("TransDate is null");
            }

            bool Mayloan = false;
            if (!string.IsNullOrEmpty(enty.ResourceItemNoId))
            {
                Mayloan = GetResourceItemMayloan(enty.ResourceItemId.ToString(), enty.ResourceItemNoId.ToString());
            }
            long maxSerialNo = GetMaxSerialNo(enty.TransDate.ToShortDateString());
            string sql = "";
            if (enty != null)
            {
                if (string.IsNullOrEmpty(enty.SerialNo))
                {
                    if (maxSerialNo != 0)
                    {
                        enty.SerialNo = (maxSerialNo + 1).ToString();
                    }
                    else
                    {
                        enty.SerialNo = enty.TransDate.ToString("yyyyMMdd") + "0001";
                    }
                }
                //DataTable dtItem = new DataTable();
                //sql = string.Format("select Quantity,IsManagement,ResourceItemId,Code from ResourceItem where ResourceItemId='{0}'", enty.ResourceItemId);
                //dtItem = HRHelper.ExecuteDataTable(sql);
                //if (dtItem == null || dtItem.Rows.Count == 0) {
                //    throw new Exception("ResourceItemId is wrong!");
                //}
                //List<ResourceItem> myObjects = HRHelper.DataTableToList<ResourceItem>(dtItem);
                //  ResourceItem resourceItem = myObjects[0];
                decimal itemQuantity = Convert.ToDecimal(dtItem.Rows[0]["Quantity"].ToString());
                bool isManagement = false;
                string strn = "";
                string itemCode = dtItem.Rows[0]["Code"].ToString();
                Boolean.TryParse(dtItem.Rows[0]["IsManagement"].ToString(), out isManagement);
                Dictionary<string, string> dicAlterType = new Dictionary<string, string>();
                DataTable dtAlterType = new DataTable();
                dtAlterType = GetAlertType();
                foreach (DataRow dr in dtAlterType.Rows)
                {
                    if (!dicAlterType.ContainsKey(dr["CodeinfoId"].ToString()))
                    {
                        dicAlterType.Add(dr["CodeinfoId"].ToString(), dr["ScName"].ToString());
                    }
                }
                //Dictionary<string, string> dicAlterType = dtAlterType.ToDictionary<string, string>(row => row["CodeInfoId"].ToString(), row => Convert.ToInt32(row["ScName"]));

                //if (resourceItem.Quantity == null)
                //{
                //    resourceItem.Quantity = new decimal(0);
                //}
                //IO出，且異動不為報廢
                if (enty.IOType.Equals("IOType_001") && !enty.AlertType.Equals("AlertType_004"))
                {
                    // ResourceItem resourceItem = ResourceItemService.GetResourceItem(enty.ResourceItemId);

                    decimal qty = (decimal)itemQuantity - enty.Qty;  //「出」時用
                    if (!string.IsNullOrEmpty(enty.ResourceItemNoId)) //有 资源品号 時進入if
                    {
                        //調整
                        if (isManagement && enty.AlertType.Equals("AlertType_003"))
                        {
                            throw new Exception("要作品号管理的资源项目若要移除,请以[报废]处理!"); //要作品號管理的資源項目若要移除,請以[報廢]處理!
                        }
                        //出庫
                        //if (resourceDetailTemp.IOType.Equals("IOType_001")) {
                        if (Mayloan == false)
                        {
                            throw new Exception("此资源品号已出库或报废, 不可再出库!"); //此資源品號已出庫, 不可再出庫!
                        }
                        else
                        {
                            strn = string.Format("update ResourceItemNo set Mayloan={1} where ResourceItemNoId='{0}'", enty.ResourceItemNoId, 0);
                            HRHelper.ExecuteNonQuery(strn);
                            enty.ResourceId = enty.ResourceItemNoId;
                            var scName = dicAlterType[enty.AlertType].ToString();
                            enty.Remark = enty.TransDate.ToShortDateString() + scName;
                            itemQuantity = qty;
                        }
                    }
                    else //[资源品号]为空白
                    {
                        if (qty < 0)
                        {
                            throw new Exception("此资源项目库存量不足, 不可再出库!"); //此資源項目庫存量不足, 不可再出庫!
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(enty.ResourceItemNoId))
                            {
                                strn = string.Format("update ResourceItemNo set Mayloan={1} where ResourceItemNoId='{0}'", enty.ResourceItemNoId, 0);
                                HRHelper.ExecuteNonQuery(strn);
                            }
                            itemQuantity = qty;
                        }
                    }
                    string str = string.Format("update ResourceItem set Quantity={1} where ResourceItemId='{0}'", enty.ResourceItemId, itemQuantity);
                    HRHelper.ExecuteNonQuery(str);
                }
                else
                {  //進到這段的是[入]或報廢的

                    if (!string.IsNullOrEmpty(enty.ResourceItemNoId))
                    { //有 资源品号 時進入if
                        if (!enty.AlertType.Equals("AlertType_004"))
                        {
                            if (!string.IsNullOrEmpty(enty.ResourceItemNoId))
                            {
                                strn = string.Format("update ResourceItemNo set Mayloan={1} where ResourceItemNoId='{0}'", enty.ResourceItemNoId, 1);
                                HRHelper.ExecuteNonQuery(strn);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(enty.ResourceItemNoId))
                            {
                                strn = string.Format("update ResourceItemNo set Mayloan={1} where ResourceItemNoId='{0}'", enty.ResourceItemNoId, 0);
                                HRHelper.ExecuteNonQuery(strn);
                            }
                            if (!Mayloan)
                            {
                                //提示，此資源品號已出庫或報廢, 不可再報廢!
                                throw new Exception("此资源品号已出库或报废, 不可再报废!");
                            }
                            itemQuantity = (decimal)itemQuantity - enty.Qty;  //「出」時用

                            strn = string.Format("update ResourceItem set Quantity={1} where ResourceItemId='{0}'", enty.ResourceItemId, itemQuantity);
                            HRHelper.ExecuteNonQuery(strn);
                        }
                        enty.ResourceId = enty.ResourceItemNoId;
                    }
                    else if (enty.AlertType.Equals("AlertType_004") && string.IsNullOrEmpty(enty.ResourceItemNoId))
                    {
                        decimal qty = (decimal)itemQuantity - enty.Qty;  //「出」時用

                        if (qty < 0)
                        {
                            throw new Exception("此资源项目库存量不足, 不可再报废!"); //此資源項目庫存量不足, 不可再出庫!
                        }
                        else
                        {
                            string str = string.Format("update ResourceItem set Quantity={1} where ResourceItemId='{0}'", enty.ResourceItemId, qty);
                            HRHelper.ExecuteNonQuery(str);
                        }
                    }
                }
                enty.ResourceDetailId = Guid.NewGuid().ToString();

                sql = HRHelper.GenerateSqlInsert(enty, "ResourceDetail");
                HRHelper.ExecuteNonQuery(sql);

                if (enty.IOType.Equals("IOType_002"))
                {

                    //入庫
                    int qty = Convert.ToInt32(enty.Qty);
                    itemQuantity = itemQuantity + qty;
                    if (enty != null && isManagement &&
                        (enty.AlertType.Equals("AlertType_001") || enty.AlertType.Equals("AlertType_003")))
                    {
                        List<ResourceDetail> arrResourceDetail = new List<ResourceDetail>();
                        List<ResourceItemNo> arrResourceItemNo = new List<ResourceItemNo>();
                        List<string> insertSqlList = new List<string>();

                        enty.Qty = 1;
                        enty.CorporationId = "688564CE-C44C-4E1B-A58D-A10091B6E77B";
                        arrResourceDetail.Add(enty);


                        HRHelper.ExecuteNonQuery(string.Format("delete from ResourceDetail where ResourceDetailId='{2}';" +
                            "update ResourceItem set Quantity={1} where ResourceItemId='{0}'", enty.ResourceItemId, itemQuantity, enty.ResourceDetailId));
                        string sqlIn = "";
                        //arrResourceDetail[0] = enty;
                        for (int i = 0; i < qty; i++)
                        {
                            ResourceDetail resourceDetailTemp = new ResourceDetail();
                            resourceDetailTemp.ResourceDetailId = Guid.NewGuid().ToString();
                            resourceDetailTemp.ResourceItemId = enty.ResourceItemId;
                            resourceDetailTemp.SerialNo = (Convert.ToInt64(enty.SerialNo) + i + 1).ToString();
                            resourceDetailTemp.TransDate = enty.TransDate;
                            resourceDetailTemp.IOType = enty.IOType;
                            resourceDetailTemp.AlertType = enty.AlertType;
                            resourceDetailTemp.Remark = enty.Remark;
                            resourceDetailTemp.ResourceFrom = enty.ResourceFrom;
                            resourceDetailTemp.Qty = 1;
                            resourceDetailTemp.CorporationId = "688564CE-C44C-4E1B-A58D-A10091B6E77B";
                            arrResourceDetail.Add(resourceDetailTemp);
                            sqlIn = HRHelper.GenerateSqlInsert(resourceDetailTemp, "ResourceDetail");
                            insertSqlList.Add(sqlIn);
                        }
                        var resourceItemNoCount = HRHelper.ExecuteScalar(string.Format(@" SELECT count(ResourceItemNoId) from ResourceItemNo where ResourceItemId='{0}' ", enty.ResourceItemId));
                        for (int i = 0; i < qty; i++)
                        {
                            ResourceItemNo resourceItemNo = new ResourceItemNo();
                            resourceItemNo.ResourceItemNoId = Guid.NewGuid().ToString();
                            arrResourceDetail[i].ResourceItemNoId = resourceItemNo.ResourceItemNoId;
                            arrResourceDetail[i].ResourceId = resourceItemNo.ResourceItemNoId;
                            resourceItemNo.ResourceItemId = enty.ResourceItemId;
                            resourceItemNo.SerialNo = (int)resourceItemNoCount + 1;
                            string item = ((int)resourceItemNoCount + 1).ToString().PadLeft(4, '0');
                            resourceItemNo.Item = itemCode + "_" + item;
                            resourceItemNo.InDate = enty.TransDate;
                            resourceItemNo.Mayloan = true;
                            arrResourceItemNo.Add(resourceItemNo);
                            resourceItemNoCount = (int)resourceItemNoCount + 1;

                            sqlIn = HRHelper.GenerateSqlInsert(resourceItemNo, "ResourceItemNo");
                            insertSqlList.Add(sqlIn);
                        }

                        HRHelper.ExecuteNonQueryWithTrans(insertSqlList);
                        //string str = string.Format("update ResourceItem set Quantity={1} where ResourceItemId='{0}'", enty.ResourceItemId, resourceItem.Quantity);
                        //HRHelper.ExecuteNonQuery(str);
                        //DataTable resourceDetailTable = HRHelper.ConvertToDataTable(arrResourceDetail);
                        //HRHelper.BulkCopyInsert(dataTable);

                        //DataTable resourceItemNoTable = HRHelper.ConvertToDataTable(arrResourceItemNo);
                        //HRHelper.BulkCopyInsert(resourceItemNoTable);

                        //ResourceItemNoService service = new ResourceItemNoService();
                        //service.Save(arrResourceItemNo.ToArray());
                    }
                    else if (enty.AlertType.Equals("AlertType_001") || enty.AlertType.Equals("AlertType_003"))
                    {
                        string str = string.Format("update ResourceItem set Quantity={1} where ResourceItemId='{0}'", enty.ResourceItemId, itemQuantity);
                        HRHelper.ExecuteNonQuery(str);
                    }

                }


            }

        }





        /// <summary>
        /// 获取资源项目品號是否借出
        /// </summary>
        /// <param name="resourceItemId"></param>
        /// <param name="resourceItemNoId"></param>
        /// <returns></returns>
        public virtual bool GetResourceItemMayloan(string resourceItemId, string resourceItemNoId)
        {
            {
                bool Mayloan = false;
                string str = @"SELECT Mayloan
                            FROM ResourceItemNo
                            WHERE ResourceItemId = '{0}' AND ResourceItemNoId = '{1}';";
                string sql = string.Format(str, resourceItemId, resourceItemNoId);

                DataTable dt = HRHelper.ExecuteDataTable(sql);
                if (dt == null || dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["Mayloan"].ToString() == "False")
                    {
                        Mayloan = false;
                    }
                    else
                    {
                        Mayloan = true;
                    }
                }
                return Mayloan;
            }
        }



        public void DeleteResourceDetail(string id)
        {
            HRHelper.ExecuteNonQuery(string.Format("delete from ResourceDetail where ResourceDetailId='{0}'", id));
        }
        /// <summary>
        /// I/O别
        /// </summary>
        /// <returns></returns>
        public DataTable GetIOType()
        {
            DataTable dt = HRHelper.GetCodeInfo("IOType");
            return dt;
        }
        /// <summary>
        /// 移动别
        /// </summary>
        /// <returns></returns>
        public DataTable GetAlertType()
        {
            DataTable dt = HRHelper.GetCodeInfo("AlertType");
            return dt;
        }

        /// <summary>
        /// 来源单别
        /// </summary>
        /// <returns></returns>
        public DataTable GetResourceFrom()
        {
            DataTable dt = HRHelper.GetCodeInfo("ResourceFrom");
            return dt;
        }


        /// <summary>
        /// 获取最大异动序号
        /// </summary>
        /// <returns>TransDate</returns>
        public virtual long GetMaxSerialNo(string transDate)
        {

            string str = string.Format("select distinct Max(SerialNo) SerialNo  from ResourceDetail where SerialNo like '{0}%' ", transDate);

            object obj = HRHelper.ExecuteScalar(str);
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                return Convert.ToInt64(obj);
            }
            else
            {
                return 0;
            }

        }
    }
}
