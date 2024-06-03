using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.HR.Services;
using System.Data;
using System.Data.SqlClient;

namespace BQHRWebApi.Service
{
    public class ResourceKindService : HRService
    {


        public override void Save(DataEntity[] entities)
        {
            foreach (var entity in entities)
            {
                ResourceKind enty = entity as ResourceKind;
                SaveResourceKind(enty);
            }
        }

        public void SaveResourceKind(ResourceKind enty)
        {
            if (string.IsNullOrEmpty(enty.Code))
            {
                throw new Exception("code is null");
            }
            if (string.IsNullOrEmpty(enty.Name))
            {
                throw new Exception("name is null");
            }
            string repeatSql = string.Format("select * from ResourceKind where code ='{0}' or name ='{1}'", enty.Code.Trim(),enty.Name.Trim());
            DataTable dt = HRHelper.ExecuteDataTable(repeatSql) ;
            if (dt!=null&&dt.Rows.Count>0) {
                throw new Exception("编码或名称已存在");
            }
            string sql = @" insert into ResourceKind(ResourceKindId,CorporationId,Code,Name,Remark,Flag,CreateBy,CreateDate,LastModifiedBy,LastModifiedDate,OwnerId)
				 values(NEWID(),'688564CE-C44C-4E1B-A58D-A10091B6E77B',@Code,@Name,@Remark,1,'98385A19-5BA6-43E5-BD0A-6A727F2E9C35',GETDATE(),'98385A19-5BA6-43E5-BD0A-6A727F2E9C35',GETDATE(),'Bad')
";
            List<SqlParameter> listPara = new List<SqlParameter>();

            AddParaWithValue(listPara, "@Code", DbType.String, enty.Code.ToString().Trim());
            AddParaWithValue(listPara, "@Name", DbType.String, enty.Name.ToString().Trim());
            AddParaWithValue(listPara, "@Remark", DbType.String, (enty.Remark == null ? "" : enty.Remark.Trim()));
            HRHelper.ExecuteNonQuery(sql, listPara.ToArray());
        }

       

        public  void DeleteResourceKind(string id) {
            HRHelper.ExecuteNonQuery(string.Format("delete from ResourceKind where ResourceKindId='{0}'",id));
        }

        /// <summary>
        /// 获取资源大类
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllResourceKind() {
            return HRHelper.ExecuteDataTable("select ResourceKindId,Code,Name from ResourceKind where flag=1");
        }

    }
}
