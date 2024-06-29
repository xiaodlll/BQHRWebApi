using BQHRWebApi.Business;
using Dcms.HR.Services;
using System.Data;
using Dcms.HR.DataEntities;

namespace BQHRWebApi.Service
{
    public class ATSpecialHolidaySetService
    {
        public ATSpecialHolidaySetService() { }


        public ATSpecialHolidaySet GetATSpecialHolidaySet(string pId)
        {
            DataTable dtEnty = HRHelper.ExecuteDataTable(string.Format("select * from ATSpecialHolidaySet where ATSpecialHolidaySetid='{0}'", pId));
            List<ATSpecialHolidaySet> myObjects = HRHelper.DataTableToList<ATSpecialHolidaySet>(dtEnty);
            ATSpecialHolidaySet type = myObjects[0];
            return type;
        }
    }
}
