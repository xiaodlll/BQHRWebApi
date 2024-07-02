using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.HR.Services;
using System.Data;

namespace BQHRWebApi.Service
{
    public class ResourceItemService : HRService
    {


        public override void Save(DataEntity[] entities)
        {
            foreach (var entity in entities)
            {
                ResourceItem enty = entity as ResourceItem;
                SaveResourceItem(enty);
            }
        }

        //public static string GenerateSqlInsert<T>(T obj, string tableName)
        //{
        //    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    var columns = new StringBuilder();
        //    var values = new StringBuilder();

        //    foreach (var property in properties)
        //    {
        //        var propertyValue = property.GetValue(obj);

        //        if (propertyValue != null)
        //        {
        //            columns.AppendFormat("[{0}],", property.Name);
        //            if (propertyValue is string || propertyValue is char)
        //            {
        //                values.AppendFormat("'{0}',", propertyValue.ToString().Replace("'", "''"));
        //            }
        //            else if (propertyValue is Boolean) {
        //                bool tf = false;
        //                Boolean.TryParse(propertyValue.ToString(), out tf);
        //                values.AppendFormat("'{0}',", tf==true?0:1);
        //            }
        //            else
        //            {
        //                values.AppendFormat("{0},", propertyValue);
        //            }
        //        }
        //    }

        //    if (columns.Length > 0)
        //    {
        //        columns.Length--; // 移除最后一个逗号
        //    }
        //    if (values.Length > 0)
        //    {
        //        values.Length--; // 移除最后一个逗号
        //    }

        //    return string.Format("INSERT INTO [{0}] ({1},Flag,CreateBy,CreateDate,LastModifiedBy,LastModifiedDate,OwnerId) VALUES ({2},1,'98385A19-5BA6-43E5-BD0A-6A727F2E9C35',GETDATE(),'98385A19-5BA6-43E5-BD0A-6A727F2E9C35',GETDATE(),'Bad');", tableName, columns, values);
        //}


        public void SaveResourceItem(ResourceItem enty)
        {
            if (string.IsNullOrEmpty(enty.Code))
            {
                throw new Exception("code is null");
            }
            if (string.IsNullOrEmpty(enty.Name))
            {
                throw new Exception("name is null");
            }
            if (string.IsNullOrEmpty(enty.ResourceKindId))
            {
                throw new Exception("ResourceKindId is null");
            }
            if (string.IsNullOrEmpty(enty.IsReturnId))
            {
                throw new Exception("IsReturnId is null");
            }
            string repeatSql = string.Format("select * from ResourceItem where code ='{0}' or name ='{1}'", enty.Code.Trim(), enty.Name.Trim());
            DataTable dt = HRHelper.ExecuteDataTable(repeatSql);
            if (dt != null && dt.Rows.Count > 0)
            {
                throw new Exception("编码或名称已存在");
            }
            enty.ResourceItemId = Guid.NewGuid().ToString();
            string sql = HRHelper.GenerateSqlInsert(enty, "ResourceItem");

            // string sql = @" insert into ResourceItem(ResourceItemId,CorporationId,Code,Name,Remark,Flag,CreateBy,CreateDate,LastModifiedBy,LastModifiedDate,OwnerId)
            //values(NEWID(),'688564CE-C44C-4E1B-A58D-A10091B6E77B',@Code,@Name,@Remark,1,'98385A19-5BA6-43E5-BD0A-6A727F2E9C35',GETDATE(),'98385A19-5BA6-43E5-BD0A-6A727F2E9C35',GETDATE(),'Bad')
            //";
            //List<SqlParameter> listPara = new List<SqlParameter>();

            //AddParaWithValue(listPara, "@Code", DbType.String, enty.Code.ToString().Trim());
            //AddParaWithValue(listPara, "@Name", DbType.String, enty.Name.ToString().Trim());
            //AddParaWithValue(listPara, "@Remark", DbType.String, (enty.Remark == null ? "" : enty.Remark.Trim()));
            //HRHelper.ExecuteNonQuery(sql, listPara.ToArray());
            HRHelper.ExecuteNonQuery(sql);
        }


        public void DeleteResourceItem(string id)
        {
            HRHelper.ExecuteNonQuery(string.Format("delete from ResourceItem where ResourceItemId='{0}'", id));
        }
        /// <summary>
        /// 资源细项
        /// </summary>
        /// <returns></returns>
        public DataTable GetResourceGroup()
        {
            DataTable dt = HRHelper.GetCodeInfo("ResourceGroup");
            return dt;
        }
        /// <summary>
        /// 是否归还
        /// </summary>
        /// <returns></returns>
        public DataTable GetIsReturnId()
        {
            DataTable dt = HRHelper.GetCodeInfo("TrueFalse");
            return dt;
        }

        /// <summary>
        /// 借用期限
        /// </summary>
        /// <returns></returns>
        public DataTable GetBorrowPeriod()
        {
            DataTable dt = HRHelper.GetCodeInfo("BorrowPeriod");
            return dt;
        }

        public ResourceItem GetResourceItem(string id)
        {
            string sql = string.Format("select * from ResourceItem where ResourceItemId='{0}'", id);
            DataTable dt = HRHelper.ExecuteDataTable(sql);
            List<ResourceItem> myObjects = HRHelper.DataTableToList<ResourceItem>(dt);
            //  List<ResourceItem> dynamicObjects = HRHelper.DataTableToList(dt);
            return myObjects[0];
        }
    }
}
