using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using Newtonsoft.Json;
using System.Data;

namespace BQHRWebApi.Service
{
    public class AttendanceOverTimePlanService : HRService
    {
        public async void CheckForESS(DataEntity[] entities)
        {

            List<AttendanceOverTimePlan> attendanceCollects = GetHREntiteies(entities);

            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_003";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "attendanceOverTimePlans";
            parameter.Value = JsonConvert.SerializeObject(attendanceCollects);

            callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

            string json = JsonConvert.SerializeObject(callServiceBindingModel);
            string response = await HttpPostJsonHelper.PostJsonAsync(json);

            APIExResponse aPIExResponse = JsonConvert.DeserializeObject<APIExResponse>(response);

            if (aPIExResponse != null)
            {
                if (aPIExResponse.State != "0")
                {
                    throw new BusinessException(aPIExResponse.Msg);
                }
            }
            else
            {
                throw new Exception(response);
            }
        }

        public override async void Save(DataEntity[] entities)
        {
            List<AttendanceOverTimePlan> attendanceCollects = GetHREntiteies(entities);

            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_004";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "attendanceOverTimePlans";
            parameter.Value = JsonConvert.SerializeObject(attendanceCollects);

            callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

            string json = JsonConvert.SerializeObject(callServiceBindingModel);
            string response = await HttpPostJsonHelper.PostJsonAsync(json);

            APIExResponse aPIExResponse = JsonConvert.DeserializeObject<APIExResponse>(response);

            if (aPIExResponse != null)
            {
                if (aPIExResponse.State != "0")
                {
                    throw new BusinessException(aPIExResponse.Msg);
                }
            }
            else
            {
                throw new Exception(response);
            }
        }


        private List<AttendanceOverTimePlan> GetHREntiteies(DataEntity[] entities)
        {
            List<AttendanceOverTimePlan> attendanceCollects = new List<AttendanceOverTimePlan>();
            foreach (AttendanceOverTimePlanForAPI enty in entities)
            {
                foreach (var item in enty.OverTimeInfos)
                {
                    if (item.AttendanceOverTimeInfoId.CheckNullOrEmpty())
                    {
                        item.AttendanceOverTimeInfoId = Guid.NewGuid();
                    }
                }
            }

            foreach (AttendanceOverTimePlanForAPI enty in entities)
            {
                var attendanceCollect = HRHelper.WebAPIEntitysToDataEntity<AttendanceOverTimePlan>(enty);
                DataTable dtEmp = GetEmpInfoByCode(enty.EmployeeCode);
                if (dtEmp == null && dtEmp.Rows.Count > 0)
                {

                    attendanceCollect.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                    attendanceCollect.CorporationId = dtEmp.Rows[0]["CorporationId"].ToString().GetGuid();

                    attendanceCollect.IsEss = true;
                    attendanceCollect.Flag = true;
                }
                else
                {
                    throw new BusinessRuleException("找不到对应的员工:" + enty.EmployeeCode);
                }
                if (enty.OverTimeInfos != null)
                {
                    foreach (var item in enty.OverTimeInfos)
                    {
                        var overTimeInfo = attendanceCollect.OverTimeInfos.Where(a => a.AttendanceOverTimeInfoId == item.AttendanceOverTimeInfoId).FirstOrDefault();

                        dtEmp = GetEmpInfoByCode(item.EmployeeCode);
                        if (dtEmp == null && dtEmp.Rows.Count > 0)
                        {

                            overTimeInfo.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                            overTimeInfo.CorporationId = dtEmp.Rows[0]["CorporationId"].ToString().GetGuid();
                        }
                        else
                        {
                            throw new BusinessRuleException("找不到对应的员工:" + enty.EmployeeCode);
                        }
                    }
                }
                attendanceCollects.Add(attendanceCollect);
            }

            return attendanceCollects;
        }

        private DataTable GetEmpInfoByCode(string employeeCode)
        {
            DataTable dt = HRHelper.ExecuteDataTable(string.Format(@"select Employee.EmployeeId,CnName as EmployeeName,Employee.DepartmentId,Department.Name as DepartmentName,
Employee.CostCenterId,CostCenter.Code as CostCenterCode,Employee.CorporationId
from Employee
left join Department on Department.DepartmentId=Employee.DepartmentId
left join Corporation on Corporation.CorporationId=Employee.CorporationId
left join CostCenter on CostCenter.CostCenterId=Employee.CostCenterId
where Employee.Code='{0}'", employeeCode));
            return dt;
        }
    }
}
