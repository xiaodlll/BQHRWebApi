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
        public async Task CheckForESS(DataEntity[] entities)
        {

            List<AttendanceOverTimePlan> attendanceCollects = GetHREntiteies(entities);

            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_003";



            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "attendanceOverTimePlans";
            parameter.Value = HRJsonConverter.SerializeAExcludingParentInDetails<AttendanceOverTimePlan>(attendanceCollects.ToArray());

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

        public async Task BatchSave(DataEntity[] entities)
        {
            List<AttendanceOverTimePlan> attendanceCollects = GetHREntiteies(entities);

            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_004";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "attendanceOverTimePlans";
            parameter.Value = HRJsonConverter.SerializeAExcludingParentInDetails<AttendanceOverTimePlan>(attendanceCollects.ToArray());

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
                var attendanceOTPlan = HRHelper.WebAPIEntitysToDataEntity<AttendanceOverTimePlan>(enty);
                if (attendanceOTPlan.AttendanceOverTimePlanId.CheckNullOrEmpty())
                {
                    attendanceOTPlan.AttendanceOverTimePlanId = Guid.NewGuid();
                }
                attendanceOTPlan.StateId = "PlanState_003";
                DataTable dtEmp = GetEmpInfoByCode(enty.EmployeeCode);
                if (dtEmp != null && dtEmp.Rows.Count > 0)
                {

                    attendanceOTPlan.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                    attendanceOTPlan.FoundEmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                    attendanceOTPlan.CorporationId = dtEmp.Rows[0]["CorporationId"].ToString().GetGuid();

                    attendanceOTPlan.IsEss = true;
                    attendanceOTPlan.Flag = true;
                    attendanceOTPlan.IsFromEss = true;
                    attendanceOTPlan.StateId = "PlanState_002";
                    if (!(enty.AuditEmployeeCode.CheckNullOrEmpty()))
                    {
                        DataTable dtEmp1 = GetEmpInfoByCode(enty.AuditEmployeeCode);
                        if (dtEmp1 != null && dtEmp1.Rows.Count > 0)
                        {
                            attendanceOTPlan.ApproveEmployeeId = dtEmp1.Rows[0]["EmployeeId"].ToString().GetGuid();
                        }
                        else
                        {
                            throw new BusinessRuleException("找不到对应的员工:" + enty.AuditEmployeeCode);
                        }
                    }
                    if (enty.AuditResult != null && enty.AuditResult == true)
                    {
                        attendanceOTPlan.ApproveResultId = "OperatorResult_001";
                    }
                    else
                    {
                        attendanceOTPlan.ApproveResultId = "OperatorResult_002";
                    }
                }
                else
                {
                    throw new BusinessRuleException("找不到对应的员工:" + enty.EmployeeCode);
                }
                if (enty.OverTimeInfos != null)
                {
                    foreach (var item in enty.OverTimeInfos)
                    {
                        var overTimeInfo = attendanceOTPlan.OverTimeInfos.Where(a => a.AttendanceOverTimeInfoId == item.AttendanceOverTimeInfoId).FirstOrDefault();

                        dtEmp = GetEmpInfoByCode(item.EmployeeCode);
                        if (dtEmp != null && dtEmp.Rows.Count > 0)
                        {

                            overTimeInfo.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                            overTimeInfo.CorporationId = dtEmp.Rows[0]["CorporationId"].ToString().GetGuid();
                        }
                        else
                        {
                            throw new BusinessRuleException("找不到对应的员工:" + enty.EmployeeCode);
                        }
                        overTimeInfo.Flag = true;
                    }
                }
                attendanceCollects.Add(attendanceOTPlan);
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
