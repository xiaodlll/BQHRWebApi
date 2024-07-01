using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace BQHRWebApi.Service
{
    public class AttendanceCollectService : HRService
    {
        public override async void Save(DataEntity[] entities)
        {
            AttendanceCollect[] attendanceCollects = HRHelper.WebAPIEntitysToDataEntitys<AttendanceCollect>(entities).ToArray();

            foreach (AttendanceCollect enty in attendanceCollects)
            {
                DataTable dtEmp = GetEmpInfoByCode(enty.EmployeeCode);
                if (dtEmp != null && dtEmp.Rows.Count>0) {

                    enty.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                    enty.EmployeeName = dtEmp.Rows[0]["EmployeeName"].ToString();
                    enty.DepartmentId = dtEmp.Rows[0]["DepartmentId"].ToString().GetGuid();
                    enty.DepartmentName = dtEmp.Rows[0]["DepartmentName"].ToString();
                    enty.CostCenterId = dtEmp.Rows[0]["CostCenterId"].ToString().GetGuid();
                    enty.CostCenterCode = dtEmp.Rows[0]["CostCenterCode"].ToString();
                    enty.CorporationId = dtEmp.Rows[0]["CorporationId"].ToString().GetGuid();

                    enty.MachineId = GetMachineId(enty.MachineCode);
                    enty.CardId = GetCardId(enty.CardCode, enty.Date);
                    enty.IsEss = true;
                    enty.Flag = true;
                }else
                {
                    throw new BusinessRuleException("找不到对应的员工:"+ enty.EmployeeCode);
                }
            }

            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_002";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "attendanceCollects";
            parameter.Value = JsonConvert.SerializeObject(entities);

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


        private void CheckData(BusinessApplyForAPI enty) { 
           
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

        private Guid GetCardId(string cardCode ,DateTime date)
        {
            Guid guid = Guid.Empty;
            object obj = HRHelper.ExecuteScalar(string.Format("select CardId from [Card] where CardNo='{0}' and UseTypeId in ('UseType_001','UseType_003') and '{1}' between BeginDate and EndDate and Flag=1", cardCode, date.ToString("yyyy-MM-dd HH:mm:ss")));
            if (obj != null) { 
                guid = (Guid)obj;
            }
            return guid;
        }

        private Guid GetMachineId(string machineCode)
        {
            Guid guid = Guid.Empty;
            object obj = HRHelper.ExecuteScalar(string.Format("select MachineId from [Machine] where Code='{0}' and Flag=1", machineCode));
            if (obj != null)
            {
                guid = (Guid)obj;
            }
            return guid;
        }
    }
}
