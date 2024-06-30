using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.Common.Services;
using Dcms.Common.Torridity.Query;
using Dcms.HR;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace BQHRWebApi.Service { 
    public class BusinessApplyService : HRService
    {
        public override async void Save(DataEntity[] entities)
        {
            foreach (BusinessApplyForAPI enty in entities)
            {
                DataTable dtCorp = GetCorpInfoByCode(enty.CorpCode);

                if (dtCorp == null && dtCorp.Rows.Count > 0)
                {
                    enty.CorporationId = dtCorp.Rows[0]["CorporationId"].ToString().GetGuid();
                }
                else
                {
                    throw new BusinessRuleException("找不到对应的公司:" + enty.CorpCode);
                }
                foreach (BusinessApplyPersonForAPI person in enty.Persons)
                {
                    DataTable dtEmp = GetEmpInfoByCode(person.EmployeeCode);

                    if (dtEmp == null && dtEmp.Rows.Count > 0) {
                        person.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                    }
                    else
                    {
                        throw new BusinessRuleException("找不到对应的员工:" + person.EmployeeCode);
                    }
                    if (!person.DeputyEmployeeCode.CheckNullOrEmpty())
                    {
                        dtEmp = GetEmpInfoByCode(person.DeputyEmployeeCode);
                        if (dtEmp == null && dtEmp.Rows.Count > 0)
                        {
                            person.DeputyEmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                        }
                        else
                        {
                            throw new BusinessRuleException("找不到对应的员工:" + person.DeputyEmployeeCode);
                        }
                    }
                }

                foreach (BusinessApplyAttendanceForAPI person in enty.Attendances)
                {
                    DataTable dtEmp = GetEmpInfoByCode(person.EmployeeCode);

                    if (dtEmp == null && dtEmp.Rows.Count > 0)
                    {
                        person.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                    }
                    else
                    {
                        throw new BusinessRuleException("找不到对应的员工:" + person.EmployeeCode);
                    }
                }
            }
            BusinessApply[] entys = HRHelper.WebAPIEntitysToDataEntitys<BusinessApply>(entities).ToArray();

            foreach (BusinessApply business in entys)
            {
                business.FoundDate = DateTime.Now.Date;
                business.StateId = Constants.PS03;
                business.BusinessApplyId =Guid.NewGuid();

            }
            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_CC_02";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "attendanceCollects";
            parameter.Value = JsonConvert.SerializeObject(entities);

            callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

            string json = JsonConvert.SerializeObject(callServiceBindingModel);
            string response = await HttpPostJsonHelper.PostJsonAsync(json);

            APIExResponse aPIExResponse = JsonConvert.DeserializeObject<APIExResponse>(response);

            if (aPIExResponse != null)
            {
                if (aPIExResponse.State != "0" )
                {
                    throw new BusinessException(aPIExResponse.Msg);
                }
                else if (aPIExResponse.State == "0" && !aPIExResponse.ResultValue.CheckNullOrEmpty())
                {
                    throw new BusinessException(aPIExResponse.ResultValue.ToString());
                }
            }
            else
            {
                throw new Exception(response);
            }
        }

        //单个保存前检查
        public async Task<string> CheckForCCSQ(BusinessApplyForAPI[] entities)
        {
          
            foreach (BusinessApplyForAPI enty in entities)
            {
                DataTable dtCorp = GetCorpInfoByCode(enty.CorpCode);
                if (dtCorp != null && dtCorp.Rows.Count > 0)
                {
                    enty.CorporationId = dtCorp.Rows[0]["CorporationId"].ToString().GetGuid();
                }
                else
                {
                    throw new BusinessRuleException("找不到对应的公司:" + enty.CorpCode);
                }
                DataTable dtEmp = GetEmpInfoByCode(enty.FoundEmpCode);

                if (dtEmp != null && dtEmp.Rows.Count > 0)
                {
                    enty.FoundEmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                }
                else
                {
                    throw new BusinessRuleException("找不到对应的员工:" + enty.FoundEmpCode);
                }
                foreach (BusinessApplyPersonForAPI person in enty.Persons)
                {
                    dtEmp = GetEmpInfoByCode(person.EmployeeCode);

                    if (dtEmp != null && dtEmp.Rows.Count > 0)
                    {
                        person.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                    }
                    else
                    {
                        throw new BusinessRuleException("找不到对应的员工:" + person.EmployeeCode);
                    }
                    if (!person.DeputyEmployeeCode.CheckNullOrEmpty())
                    {
                        dtEmp = GetEmpInfoByCode(person.DeputyEmployeeCode);
                        if (dtEmp != null && dtEmp.Rows.Count > 0)
                        {
                            person.DeputyEmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                        }
                        else
                        {
                            throw new BusinessRuleException("找不到对应的员工:" + person.DeputyEmployeeCode);
                        }
                    }
                }

                foreach (BusinessApplyAttendanceForAPI person in enty.Attendances)
                {
                     dtEmp = GetEmpInfoByCode(person.EmployeeCode);

                    if (dtEmp != null && dtEmp.Rows.Count > 0)
                    {
                        person.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                    }
                    else
                    {
                        throw new BusinessRuleException("找不到对应的员工:" + person.EmployeeCode);
                    }
                }
            }
            //  BusinessApply[] entys = HRHelper.DataEntitysToWebAPIEntitys<BusinessApply>(entities).ToArray();
            List<BusinessApply> entys = HRHelper.WebAPIEntitysToDataEntitys<BusinessApply>("", "", entities);

            foreach (BusinessApply business in entys)
            {
                business.FoundDate = DateTime.Now.Date;
                business.StateId = Constants.PS03;
                business.BusinessApplyId = Guid.NewGuid();

            }
            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_CC_001";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "formEntities";
            parameter.Value = JsonConvert.SerializeObject(entities);

            callServiceBindingModel.Parameters = new APIRequestParameter[] { parameter };

            string json = JsonConvert.SerializeObject(callServiceBindingModel);
            string response = await HttpPostJsonHelper.PostJsonAsync(json);

            APIExResponse aPIExResponse = JsonConvert.DeserializeObject<APIExResponse>(response);
            //{"State":"-1","Msg":"API_CC_001","ResultType":null,"ResultValue":null}

            if (aPIExResponse != null)
            {
                if (aPIExResponse.State != "0"&& !aPIExResponse.Msg.CheckNullOrEmpty())
                {
                    throw new BusinessException(aPIExResponse.Msg);
                }
                else if (aPIExResponse.State == "0" && !aPIExResponse.ResultValue.CheckNullOrEmpty())
                {
                    return (aPIExResponse.ResultValue.ToString());
                }
            }
            else
            {
                throw new Exception(response);
            }
            return "";
        }


        public async  void SaveForCCSQ(string formNumber, BusinessApplyForAPI[] entities)
        {
            if (formNumber.CheckNullOrEmpty())
            {
                throw new BusinessRuleException("流程编号不能为空");
            }
            if (HRHelper.isExistFormNumber("BusinessApply", "ATCC", formNumber))
            {
                throw new BusinessRuleException("流程编号在出差申请中已经存在");
            }
            foreach (BusinessApplyForAPI enty in entities)
            {
                DataTable dtCorp = GetCorpInfoByCode(enty.CorpCode);

                if (dtCorp != null && dtCorp.Rows.Count > 0)
                {
                    enty.CorporationId = dtCorp.Rows[0]["CorporationId"].ToString().GetGuid();
                }
                else
                {
                    throw new BusinessRuleException("找不到对应的公司:" + enty.CorpCode);
                }
                foreach (BusinessApplyPersonForAPI person in enty.Persons)
                {
                    DataTable dtEmp = GetEmpInfoByCode(person.EmployeeCode);

                    if (dtEmp != null && dtEmp.Rows.Count > 0)
                    {
                        person.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                    }
                    else
                    {
                        throw new BusinessRuleException("找不到对应的员工:" + person.EmployeeCode);
                    }
                    if (!person.DeputyEmployeeCode.CheckNullOrEmpty())
                    {
                        dtEmp = GetEmpInfoByCode(person.DeputyEmployeeCode);
                        if (dtEmp != null && dtEmp.Rows.Count > 0)
                        {
                            person.DeputyEmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                        }
                        else
                        {
                            throw new BusinessRuleException("找不到对应的员工:" + person.DeputyEmployeeCode);
                        }
                    }
                }

                foreach (BusinessApplyAttendanceForAPI person in enty.Attendances)
                {
                    DataTable dtEmp = GetEmpInfoByCode(person.EmployeeCode);

                    if (dtEmp != null && dtEmp.Rows.Count > 0)
                    {
                        person.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                    }
                    else
                    {
                        throw new BusinessRuleException("找不到对应的员工:" + person.EmployeeCode);
                    }
                }
            }
            List<BusinessApply> entys = HRHelper.WebAPIEntitysToDataEntitys<BusinessApply>("", "", entities);


            foreach (BusinessApply business in entys)
            {
                business.IsEss = true;
                business.EssNo = formNumber;
                business.EssType = "ATCC";

                business.FoundDate = DateTime.Now.Date;
                business.StateId = Constants.PS03;
                business.BusinessApplyId = Guid.NewGuid();

            }
            CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
            callServiceBindingModel.RequestCode = "API_CC_02";

            APIRequestParameter parameter = new APIRequestParameter();
            parameter.Name = "formEntities";
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
                else if (aPIExResponse.State == "0" && !aPIExResponse.ResultValue.CheckNullOrEmpty())
                {
                    throw new BusinessException(aPIExResponse.ResultValue.ToString());
                }
            }
            else
            {
                throw new Exception(response);
            }
        }


        private DataTable GetEmpInfoByCode(string employeeCode)
        {
            DataTable dt = HRHelper.ExecuteDataTable(string.Format(@"select EmployeeId from Employee where Code='{0}'", employeeCode));
            return dt;
        }

        private DataTable GetCorpInfoByCode(string corpCode)
        {
            DataTable dt = HRHelper.ExecuteDataTable(string.Format(@"select CorporationId from Corporation where Code='{0}'", corpCode));
            return dt;
        }


    }
}
