using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.HR.Services;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Reflection;
using System.Text;

namespace BQHRWebApi.Service
{
    public class ResourceItemNoService : HRService
    {


        public override void Save(DataEntity[] entities)
        {
            foreach (var entity in entities)
            {
                ResourceItemNo enty = entity as ResourceItemNo;
                SaveResourceItemNo(enty);
            }
        }

       
        public void SaveResourceItemNo(ResourceItemNo enty)
        {
            enty.ResourceItemNoId = Guid.NewGuid().ToString();
            string sql = HRHelper.GenerateSqlInsert(enty, "ResourceItemNo");
            HRHelper.ExecuteNonQuery(sql);
        }


        public void DeleteResourceItemNo(string id)
        {
            HRHelper.ExecuteNonQuery(string.Format("delete from ResourceItemNo where ResourceItemNoId='{0}'", id));
        }


        /// <summary>
        /// 根据资源项目获取品号资料
        /// </summary>
        /// <returns></returns>
        public DataTable GetItemInfoByItemId(string itemId)
        {
            string sql = string.Format(" select ResourceItemNoId,Item,SerialNo from ResourceItemNo where ResourceItemId='{0}' order by SerialNo",itemId);
            DataTable dt = HRHelper.ExecuteDataTable(sql);
            return dt;
        }
    }
}
