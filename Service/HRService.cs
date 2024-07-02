using BQHRWebApi.Business;
using System.Data;
using System.Data.SqlClient;

namespace BQHRWebApi.Common
{
    public class HRService
    {

        public HRService()
        {

        }

        protected void AddParaWithValue(List<SqlParameter> listPara, string pParaName, DbType dbType, object pValue)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = pParaName;
            sqlParameter.DbType = dbType;
            sqlParameter.Value = pValue;
            listPara.Add(sqlParameter);
        }

        public virtual void Save(DataEntity[] entities)
        {
            throw new NotImplementedException();
        }



    }
}
