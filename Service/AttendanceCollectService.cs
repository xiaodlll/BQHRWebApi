using BQHRWebApi.Business;
using BQHRWebApi.Common;
using Dcms.Common;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Newtonsoft.Json;
using Serilog.Core;
using System.Data;

namespace BQHRWebApi.Service
{
    public class AttendanceCollectService : HRService
    {
        public override async void Save(DataEntity[] entities)
        {
            AttendanceCollect[] attendanceCollects = HRHelper.WebAPIEntitysToDataEntitys<AttendanceCollect>(entities).ToArray();
            DateTime tmpDt = DateTime.Now;
           
            foreach (AttendanceCollect enty in attendanceCollects)
            {
                DateTime.TryParse(enty.Date.AddTimeToDateTime(enty.Time).ToDateFormatString(),out tmpDt);
                DataTable dt = GetEmpCardInfo(enty.EmployeeCode,tmpDt);
                if (dt != null && dt.Rows.Count > 0)
                {
                    //有效的卡信息
                    enty.EmployeeCode = dt.Rows[0][0].ToString();
                    enty.EmployeeName = dt.Rows[0]["CnName"].ToString();
                    enty.DepartmentId = dt.Rows[0][1].ToString().GetGuid();
                    enty.DepartmentName = dt.Rows[0][2].ToString();
                    enty.CorporationId = dt.Rows[0]["CorporationId"].ToString().GetGuid();
                    enty.CostCenterId = dt.Rows[0][3].ToString().GetGuid();
                    enty.CostCenterCode = dt.Rows[0][4].ToString();
                    enty.CardId = dt.Rows[0][5].ToString().GetGuid();
                    enty.CardCode = dt.Rows[0][6].ToString();
                }
                else
                {
                    DataTable dtEmp = GetEmpInfoByCode(enty.EmployeeCode);
                    if (dtEmp != null && dtEmp.Rows.Count > 0)
                    {

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
                    }
                    else
                    {
                        throw new BusinessRuleException("找不到对应的员工:" + enty.EmployeeCode);
                    }
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


        private void CheckData(BusinessApplyForAPI enty)
        {

        }
        private DataTable GetEmpInfoByCode(string employeeCode)
        {
            DataTable dt = HRHelper.ExecuteDataTable(string.Format(@"select Employee.EmployeeId,CnName as EmployeeName,
Employee.DepartmentId,Department.Name as DepartmentName,
Employee.CostCenterId,CostCenter.Code as CostCenterCode,Employee.CorporationId
from Employee
left join Department on Department.DepartmentId=Employee.DepartmentId
left join Corporation on Corporation.CorporationId=Employee.CorporationId
left join CostCenter on CostCenter.CostCenterId=Employee.CostCenterId
where Employee.Code='{0}'
", employeeCode));
            return dt;
        }

        private DataTable GetEmpCardInfo(string employeeCode,DateTime pDate) {
            string sql = string.Format(@"SELECT  [Card].[CardId] ,
        [Card].[CardNo] ,
        [Card].[EmployeeId] ,
        [Card_Employee_EmployeeId].[CnName] ,
        [Card_Employee_EmployeeId].[Code] as EmpCode,
        [Card_Employee_EmployeeId].[DepartmentId] ,
        [Card_Employee_EmployeeId_Department_DepartmentId].[Name] ,
        [Card_Employee_EmployeeId].[CostCenterId] ,
        [Card_Employee_EmployeeId_CostCenter_CostCenterId].[Code] as CostCode,
        [Card_Employee_EmployeeId].[CorporationId]
FROM    [Card] AS [Card]
        LEFT  JOIN [Employee] AS [Card_Employee_EmployeeId] ON [Card].[EmployeeId] = [Card_Employee_EmployeeId].[EmployeeId]
        LEFT  JOIN [Department] AS [Card_Employee_EmployeeId_Department_DepartmentId] ON [Card_Employee_EmployeeId].[DepartmentId] = [Card_Employee_EmployeeId_Department_DepartmentId].[DepartmentId]
        LEFT  JOIN [CostCenter] AS [Card_Employee_EmployeeId_CostCenter_CostCenterId] ON [Card_Employee_EmployeeId].[CostCenterId] = [Card_Employee_EmployeeId_CostCenter_CostCenterId].[CostCenterId]
WHERE   (  [Card_Employee_EmployeeId].Code ='{0}' )
        AND ( [Card].[BeginDate] <= '{1}' )
        AND ( [Card].[EndDate] >= '{1}' )
        AND ( [Card].[UserKindId] = 'UserKind_002' )
        AND ( [Card].[UseTypeId] = 'UseType_001' )", employeeCode,pDate.ToDateFormatString());
            return HRHelper.ExecuteDataTable(sql);
        }

        private Guid GetCardId(string cardCode, DateTime date)
        {
            Guid guid = Guid.Empty;
            object obj = HRHelper.ExecuteScalar(string.Format("select CardId from [Card] where CardNo='{0}' and UseTypeId in ('UseType_001','UseType_003') and '{1}' between BeginDate and EndDate and Flag=1", cardCode, date.ToString("yyyy-MM-dd HH:mm:ss")));
            if (obj != null)
            {
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

        public async Task<ApiResponse> SaveCollects(AttendanceCollectForAPI[] input)
        {
            try
            {
                Authorization.CheckAuthorization();
            }
            catch (AuthorizationException aEx)
            {
                return ApiResponse.Fail("授权:" + aEx.Message);
            }

            try
            {
                if (input != null && input.Length > 0)
                {
                    AttendanceCollect[] attendanceCollects = HRHelper.WebAPIEntitysToDataEntitys<AttendanceCollect>(input).ToArray();
                    DateTime tmpDt = DateTime.Now;

                    foreach (AttendanceCollect enty in attendanceCollects)
                    {
                        DateTime.TryParse(enty.Date.AddTimeToDateTime(enty.Time).ToDateFormatString(), out tmpDt);
                        DataTable dt = GetEmpCardInfo(enty.EmployeeCode, tmpDt);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            enty.EmployeeId = dt.Rows[0]["EmployeeId"].ToString().GetGuid();

                            //有效的卡信息
                            enty.EmployeeCode = dt.Rows[0]["EmpCode"].ToString();
                            enty.EmployeeName = dt.Rows[0]["CnName"].ToString();
                            enty.DepartmentId = dt.Rows[0]["DepartmentId"].ToString().GetGuid();
                            enty.DepartmentName = dt.Rows[0]["Name"].ToString();
                            enty.CorporationId = dt.Rows[0]["CorporationId"].ToString().GetGuid();
                            enty.CostCenterId = dt.Rows[0]["CostCenterId"].ToString().GetGuid();
                            enty.CostCenterCode = dt.Rows[0]["CostCode"].ToString();
                            enty.CardId = dt.Rows[0]["CardId"].ToString().GetGuid();
                            enty.CardCode = dt.Rows[0]["CardNo"].ToString();
                        }
                        else
                        {
                            DataTable dtEmp = GetEmpInfoByCode(enty.EmployeeCode);
                            if (dtEmp != null && dtEmp.Rows.Count > 0)
                            {

                                enty.EmployeeId = dtEmp.Rows[0]["EmployeeId"].ToString().GetGuid();
                                enty.EmployeeName = dtEmp.Rows[0]["EmployeeName"].ToString();
                                enty.DepartmentId = dtEmp.Rows[0]["DepartmentId"].ToString().GetGuid();
                                enty.DepartmentName = dtEmp.Rows[0]["DepartmentName"].ToString();
                                enty.CostCenterId = dtEmp.Rows[0]["CostCenterId"].ToString().GetGuid();
                                enty.CostCenterCode = dtEmp.Rows[0]["CostCenterCode"].ToString();
                                enty.CorporationId = dtEmp.Rows[0]["CorporationId"].ToString().GetGuid();
                                if (!enty.MachineCode.CheckNullOrEmpty())
                                {
                                    enty.MachineId = GetMachineId(enty.MachineCode);
                                }
                                if (!enty.CardCode.CheckNullOrEmpty())
                                {
                                    enty.CardId = GetCardId(enty.CardCode, enty.Date);
                                }
                             
                            }
                            else
                            {
                                throw new BusinessRuleException("找不到对应的员工:" + enty.EmployeeCode);
                            }
                        }
                        enty.IsEss = true;
                        enty.Flag = true;
                        enty.IsFromEss = true;
                        enty.StateId = "PlanState_003";
                        enty.ApproveResultId = "OperatorResult_001";
                        enty.IsManual = true;
                        enty.IsForAttendance = true;
                    }

                    CallServiceBindingModel callServiceBindingModel = new CallServiceBindingModel();
                    callServiceBindingModel.RequestCode = "API_002";

                    APIRequestParameter parameter = new APIRequestParameter();
                    parameter.Name = "attendanceCollects";
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
                else
                {
                    return ApiResponse.Fail("数据传入格式不正确!");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.Fail((ex is BusinessException) ? ex.Message : ex.ToString());
            }
            return ApiResponse.Success();
        }
    }
}
