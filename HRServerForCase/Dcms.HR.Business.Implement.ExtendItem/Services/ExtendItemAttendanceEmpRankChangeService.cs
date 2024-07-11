using Dcms.Common;
using Dcms.Common.Core;
using Dcms.Common.Services;
using Dcms.Common.Torridity.Query;
using Dcms.Common.Torridity;
using Dcms.HR.Business.Implement.Properties;
using Dcms.HR.DataEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Transactions;

namespace Dcms.HR.Services
{
    public partial class ExtendItemService
    {

        public string CheckForAttendanceRankChangeForEss(AttendanceEmployeeRank[] attendanceEmployeeRanks)
        {
            JArray jArrayResult = new JArray();
            foreach (var item in attendanceEmployeeRanks)
            {
                JObject jObject = new JObject();
                try
                {
                    DateTime beginDate = DateTime.Parse(item.ExtendedProperties["BeginDate"].ToString()).Date;
                    string id = GetAttendanceEmployeeRankId(item.EmployeeId.GetString(), beginDate);
                    if (string.IsNullOrEmpty(id))
                    {
                        throw new BusinessRuleException(string.Format("找不到员工{0} 在{1}现有的班次。", Factory.GetService<IEmployeeServiceEx>().GetEmployeeCodeById(item.EmployeeId.GetString()), item.Date.ToString("yyyy-MM-dd")));
                    }
                    jObject["EssNo"] = item.EssNo;
                    jObject["Success"] = true;
                    jObject["Msg"] = string.Empty;
                    jArrayResult.Add(jObject);
                }
                catch (Exception ex)
                {
                    jObject["EssNo"] = item.EssNo;
                    jObject["Success"] = false;
                    jObject["Msg"] = ex.Message;
                    jArrayResult.Add(jObject);
                }
            }
            return jArrayResult.ToString();
        }

        public string SaveForAttendanceRankChangeForEss(AttendanceEmployeeRank[] attendanceEmployeeRanks)
        {
            JArray jArrayResult = new JArray();
            foreach (var item in attendanceEmployeeRanks)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    JObject jObject = new JObject();
                    try
                    {
                        IAttendanceEmployeeRankService service = Factory.GetService<IAttendanceEmployeeRankService>();
                        IDocumentService<AttendanceEmployeeRank> docService = service;
                        DateTime beginDate = DateTime.Parse(item.ExtendedProperties["BeginDate"].ToString()).Date;
                        DateTime endDate = DateTime.Parse(item.ExtendedProperties["EndDate"].ToString()).Date;
                        string id = GetAttendanceEmployeeRankId(item.EmployeeId.GetString(), beginDate);
                        if (string.IsNullOrEmpty(id))
                        {
                            throw new BusinessRuleException(string.Format("找不到员工{0} 在{1}现有的班次。", Factory.GetService<IEmployeeServiceEx>().GetEmployeeCodeById(item.EmployeeId.GetString()), item.Date.ToString("yyyy-MM-dd")));
                        }
                        AttendanceEmployeeRank atEmpRank = docService.Read(id);
                        atEmpRank.AttendanceRankId = item.AttendanceRankId;
                        atEmpRank.AttendanceHolidayTypeId = item.AttendanceHolidayTypeId;

                        bool holidayType1 = true;
                        if (item.ExtendedProperties["HolidayType1"]!=null)
                        {
                            bool.TryParse(item.ExtendedProperties["HolidayType1"].ToString(),out holidayType1);
                        }
                        bool holidayType2 = true;
                        if (item.ExtendedProperties["HolidayType2"] != null)
                        {
                            bool.TryParse(item.ExtendedProperties["HolidayType2"].ToString(), out holidayType2);
                        }
                        string restId = string.Empty;
                        if (item.ExtendedProperties["HolidayAttendanceRankId"] != null)
                        {
                            restId = item.ExtendedProperties["HolidayAttendanceRankId"].ToString();
                        }

                        service.CustomSaveRank(new string[] { atEmpRank.EmployeeId.GetString() }, atEmpRank.AttendanceRankId, atEmpRank.AttendanceSpellId.GetString(), beginDate, endDate, holidayType1, holidayType2, atEmpRank.Remark, restId);

                        //更新审核人
                        if (!(item.ApproveEmployeeId.CheckNullOrEmpty()))
                        {
                            string sqlUpdate = string.Format(@"update AttendanceRankChange set ApproveEmployeeId ='{0}',ApproveResultId='{1}' 
where EmployeeId='{2}' and [Date]>='{3}' and [Date]<='{4}' and NewATRankId='{5}'", item.ApproveEmployeeId.GetString(), item.ApproveResultId, atEmpRank.EmployeeId.GetString(), beginDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), item.AttendanceRankId);
                            HRHelper.ExecuteNonQuery(sqlUpdate);
                        }
                        jObject["EssNo"] = item.EssNo;
                        jObject["Success"] = true;
                        jObject["Msg"] = string.Empty;
                        jArrayResult.Add(jObject);
                    }
                    catch (Exception ex)
                    {
                        jObject["EssNo"] = item.EssNo;
                        jObject["Success"] = false;
                        jObject["Msg"] = ex.Message;
                        jArrayResult.Add(jObject);
                        scope.Dispose();
                        continue;
                    }
                    scope.Complete();
                }
            }
            return jArrayResult.ToString();
        }

        private string GetAttendanceEmployeeRankId(string pEmployeeId, DateTime pDate)
        {
            var dt = HRHelper.ExecuteDataTable(string.Format("select AttendanceEmployeeRankId from AttendanceEmpRank where EmployeeId='{0}' and [Date]='{1}'", pEmployeeId, pDate.ToString("yyyy-MM-dd")));
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0][0].ToString();
            }
            return null;
        }
    }
}
