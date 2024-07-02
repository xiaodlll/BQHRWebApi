using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using Newtonsoft.Json;
using System.Data;

namespace BQHRWebApi.Service
{
    public class BusinessRegisterService : HRService
    {
        public async void CheckForESS(DataEntity[] entities)
        {
            List<BusinessRegister> businessRegisters = GetHREntiteies(entities);

            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_007";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "businessRegisters";
            parameter.Value = HRJsonConverter.SerializeAExcludingParentInDetails<BusinessRegister>(businessRegisters.ToArray());

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
            List<BusinessRegister> businessRegisters = GetHREntiteies(entities);

            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_008";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "businessRegisters";
            parameter.Value = HRJsonConverter.SerializeAExcludingParentInDetails<BusinessRegister>(businessRegisters.ToArray());

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


        private List<BusinessRegister> GetHREntiteies(DataEntity[] entities)
        {
            List<BusinessRegister> businessRegisters = new List<BusinessRegister>();
            foreach (BusinessRegisterForAPI enty in entities)
            {
                foreach (var item in enty.RegisterInfos)
                {
                    if (item.BusinessRegisterInfoId.CheckNullOrEmpty())
                    {
                        item.BusinessRegisterInfoId = Guid.NewGuid();
                    }
                }
            }

            foreach (BusinessRegisterForAPI enty in entities)
            {
                var businessRegister = HRHelper.WebAPIEntitysToDataEntity<BusinessRegister>(enty);
                if (businessRegister.BusinessRegisterId.CheckNullOrEmpty())
                {
                    businessRegister.BusinessRegisterId = Guid.NewGuid();
                }
                DataTable dtEmp = GetEmpInfoByCode(enty.EmployeeCode);
                if (dtEmp != null && dtEmp.Rows.Count > 0)
                {
                    businessRegister.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                }
                else
                {
                    throw new BusinessRuleException("找不到对应的员工:" + enty.EmployeeCode);
                }
                if (enty.RegisterInfos != null)
                {
                    foreach (var item in enty.RegisterInfos)
                    {
                        var registerInfo = businessRegister.RegisterInfos.Where(a => a.BusinessRegisterInfoId == item.BusinessRegisterInfoId).FirstOrDefault();

                        dtEmp = GetEmpInfoByCode(item.EmployeeCode);
                        if (dtEmp != null && dtEmp.Rows.Count > 0)
                        {

                            registerInfo.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                            registerInfo.CorporationId = dtEmp.Rows[0]["CorporationId"].ToString().GetGuid();
                        }
                        else
                        {
                            throw new BusinessRuleException("找不到对应的员工:" + enty.EmployeeCode);
                        }
                        registerInfo.Flag = true;
                    }
                }
                businessRegisters.Add(businessRegister);
            }

            return businessRegisters;
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
