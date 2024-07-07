using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using Newtonsoft.Json;
using System.Data;

namespace BQHRWebApi.Service
{
    public class AttendanceRankChangeService : HRService
    {
        public async Task<APIExResponse> CheckForESS(DataEntity[] entities)
        {
            List<AttendanceEmployeeRank> attendanceEmployeeRanks = GetHREntiteies(entities);

            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_005";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "attendanceRankChanges";
            parameter.Value = HRJsonConverter.SerializeAExcludingParentInDetails<AttendanceEmployeeRank>(attendanceEmployeeRanks.ToArray());

            callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

            string json = JsonConvert.SerializeObject(callServiceBindingModel);
            string response = await HttpPostJsonHelper.PostJsonAsync(json);

            APIExResponse aPIExResponse = JsonConvert.DeserializeObject<APIExResponse>(response);

            if (aPIExResponse != null)
            {
                return aPIExResponse;
            }
            else
            {
                throw new Exception(response);
            }
        }

        public async Task<APIExResponse> BatchSave(DataEntity[] entities)
        {
            List<AttendanceEmployeeRank> attendanceEmployeeRanks = GetHREntiteies(entities);

            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_006";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "attendanceRankChanges";
            parameter.Value = HRJsonConverter.SerializeAExcludingParentInDetails<AttendanceEmployeeRank>(attendanceEmployeeRanks.ToArray());

            callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

            string json = JsonConvert.SerializeObject(callServiceBindingModel);
            string response = await HttpPostJsonHelper.PostJsonAsync(json);

            APIExResponse aPIExResponse = JsonConvert.DeserializeObject<APIExResponse>(response);

            if (aPIExResponse != null)
            {
                return aPIExResponse;
            }
            else
            {
                throw new Exception(response);
            }
        }


        private List<AttendanceEmployeeRank> GetHREntiteies(DataEntity[] entities)
        {
            List<AttendanceEmployeeRank> attendanceEmployeeRanks = new List<AttendanceEmployeeRank>();
            foreach (AttendanceRankChangeForAPI enty in entities)
            {
                var attendanceEmployeeRank = HRHelper.WebAPIEntitysToDataEntity<AttendanceEmployeeRank>(enty);
                DataTable dtEmp = GetEmpInfoByCode(enty.EmployeeCode);
                if (dtEmp != null && dtEmp.Rows.Count > 0)
                {
                    attendanceEmployeeRank.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                }
                else
                {
                    throw new BusinessRuleException("找不到对应的员工:" + enty.EmployeeCode);
                }
                attendanceEmployeeRank.IsEss = true;
                attendanceEmployeeRank.Flag = true;
                attendanceEmployeeRank.IsFromEss = true;
                attendanceEmployeeRank.StateId = "PlanState_002";
                if (!(enty.AuditEmployeeCode.CheckNullOrEmpty()))
                {
                    DataTable dtEmp1 = GetEmpInfoByCode(enty.AuditEmployeeCode);
                    if (dtEmp1 != null && dtEmp1.Rows.Count > 0)
                    {
                        attendanceEmployeeRank.ApproveEmployeeId = dtEmp1.Rows[0]["EmployeeId"].ToString().GetGuid();
                    }
                    else
                    {
                        throw new BusinessRuleException("找不到对应的员工:" + enty.AuditEmployeeCode);
                    }
                }
                if (enty.AuditResult != null && enty.AuditResult == true)
                {
                    attendanceEmployeeRank.ApproveResultId = "OperatorResult_001";
                }
                else
                {
                    attendanceEmployeeRank.ApproveResultId = "OperatorResult_002";
                }
                attendanceEmployeeRanks.Add(attendanceEmployeeRank);
            }

            return attendanceEmployeeRanks;
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
