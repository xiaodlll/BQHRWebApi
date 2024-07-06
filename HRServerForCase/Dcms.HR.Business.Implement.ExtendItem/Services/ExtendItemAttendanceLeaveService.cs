using Dcms.Common;
using Dcms.Common.Core;
using Dcms.Common.DataEntities;
using Dcms.Common.Services;
using Dcms.HR.Business.Implement.Properties;
using Dcms.HR.DataEntities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Linq;

namespace Dcms.HR.Services
{
    public partial class ExtendItemService
    {
        #region 请假
        public void CheckForAttendanceLeaveForAPI(AttendanceLeave attendanceLeave)
        {
            Factory.GetService<IAttendanceLeaveService>().CheckForESS(attendanceLeave);
        }

        public void SaveAttendanceLeaveForAPI(AttendanceLeave attendanceLeave)
        {
            IAttendanceLeaveService service = Factory.GetService<IAttendanceLeaveService>();
            IDocumentService<AttendanceLeave> docSer = service.GetServiceNoPower();
            service.SaveForESS(attendanceLeave);
            AttendanceLeave entyNew = docSer.Read(attendanceLeave.AttendanceLeaveId);
            IAuditObject auditObject = new AttendanceLeave();
            IUserService services = Factory.GetService<IUserService>();
            string employeeId = services.GetEmployeeIdOfUser();
            if (!employeeId.CheckNullOrEmpty())
            {
                auditObject.ApproveEmployeeId = employeeId.GetGuid();
                auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(employeeId);
            }
            auditObject.ApproveDate = DateTime.Now.Date;
            auditObject.ApproveOperationDate = DateTime.Now;
            auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
            auditObject.ApproveResultId = Constants.AuditAgree;
            auditObject.ApproveRemark = "API自动审核同意";
            auditObject.StateId = Constants.PS03;
            service.Audit(new object[] { entyNew.AttendanceLeaveId }, auditObject);
        }

        public void CheckForAT406ForAPI(AttendanceOverTimeRest formEntity)
        {
            Factory.GetService<IAttendanceOverTimeRestService>().CheckForESS(formEntity);
        }

        public void SaveAT406ForAPI(AttendanceOverTimeRest formEntity)
        {
            IAttendanceOverTimeRestService service = Factory.GetService<IAttendanceOverTimeRestService>();
            IDocumentService<AttendanceOverTimeRest> docSer = service.GetServiceNoPower(); ;
            service.SaveForESS(formEntity);
            AttendanceOverTimeRest entyNew = docSer.Read(formEntity.AttendanceOverTimeRestId);
            IAuditObject auditObject = new AttendanceOverTimeRest();
            IUserService services = Factory.GetService<IUserService>();
            string employeeId = services.GetEmployeeIdOfUser();
            if (!employeeId.CheckNullOrEmpty())
            {
                auditObject.ApproveEmployeeId = employeeId.GetGuid();
                auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(employeeId);
            }
            auditObject.StateId = Constants.PS03;
            auditObject.ApproveDate = DateTime.Now.Date;
            auditObject.ApproveOperationDate = DateTime.Now;
            auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
            auditObject.ApproveResultId = Constants.AuditAgree;
            auditObject.ApproveRemark = "API自动审核同意";
            service.Audit(new object[] { entyNew.AttendanceOverTimeRestId }, auditObject);
        }

        public void CheckForAT401ForAPI(AnnualLeaveRegister formEntity)
        {
            Factory.GetService<IAnnualLeaveRegisterService>().CheckForESS(formEntity);
        }

        public void SaveAT401ForAPI(AnnualLeaveRegister formEntity)
        {
            IAnnualLeaveRegisterService service = Factory.GetService<IAnnualLeaveRegisterService>();
            IDocumentService<AnnualLeaveRegister> docSer = service;

            service.SaveForESS(formEntity);
            AnnualLeaveRegister entyNew = docSer.Read(formEntity.AnnualLeaveRegisterId);
            IAuditObject auditObject = new AnnualLeaveRegister();
            IUserService services = Factory.GetService<IUserService>();
            string employeeId = services.GetEmployeeIdOfUser();
            if (!employeeId.CheckNullOrEmpty())
            {
                auditObject.ApproveEmployeeId = employeeId.GetGuid();
                auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(employeeId);
            }
            auditObject.ApproveDate = DateTime.Now.Date;
            auditObject.ApproveOperationDate = DateTime.Now;
            auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
            auditObject.ApproveResultId = Constants.AuditAgree;
            auditObject.StateId = Constants.PS03;
            auditObject.ApproveRemark = "API自动审核同意";
            //this.Submit(new object[] { e.DataEntity.AnnualLeaveRegisterId });
            service.Audit(new object[] { entyNew.AnnualLeaveRegisterId }, auditObject);


        }


        public string BatchCheckAT401ForAPI(AnnualLeaveRegister[] formEntities)
        {
            StringBuilder sb = new StringBuilder();
            foreach (AnnualLeaveRegister enty in formEntities)
            {
                enty.IsEss = true;
                enty.IsFromEss = true;
                enty.Flag = true;
                enty.EssType = string.Empty;
                enty.EssNo = string.Empty;
                try
                {
                    Factory.GetService<IAnnualLeaveRegisterService>().CheckForESS(enty);
                }
                catch (Exception ex)
                {
                    sb.AppendLine(ex.Message);
                }
            }
            return sb.ToString();
        }

        public void BatchSaveAT401ForAPI(AnnualLeaveRegister[] formEntities)
        {
            IAnnualLeaveRegisterService service = Factory.GetService<IAnnualLeaveRegisterService>();
            IDocumentService<AnnualLeaveRegister> docSer = service;
            IAuditObject auditObject = new AnnualLeaveRegister();
            //  IUserService services = Factory.GetService<IUserService>();

            foreach (AnnualLeaveRegister enty in formEntities)
            {
                enty.IsEss = true;
                enty.IsFromEss = true;
                enty.Flag = true;
                //string employeeId = GetEmpIdByCode(auditEmployeeCode);// Factory.GetService<IEmployeeServiceEx>().GetEmployeeIdByCode(auditEmployeeCode);
                //if (!employeeId.CheckNullOrEmpty())
                //{
                auditObject.ApproveEmployeeId = enty.ApproveEmployeeId;
                auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(enty.ApproveEmployeeId.GetString());
                //}
                auditObject.ApproveDate = DateTime.Now.Date;
                auditObject.ApproveOperationDate = DateTime.Now;
                auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
                auditObject.ApproveResultId = enty.ApproveResultId;
                auditObject.StateId = Constants.PS03;
                auditObject.ApproveRemark = "API自动审核同意";
                Factory.GetService<IAnnualLeaveRegisterService>().CheckForESS(enty);
            }
            foreach (AnnualLeaveRegister enty in formEntities)
            {
                service.SaveForESS(enty);
                AnnualLeaveRegister entyNew = docSer.Read(enty.AnnualLeaveRegisterId);
                service.Audit(new object[] { entyNew.AnnualLeaveRegisterId }, auditObject);
            }


        }


        public string BatchCheckAT406ForAPI(AttendanceOverTimeRest[] formEntities)
        {
            StringBuilder sb = new StringBuilder();
            foreach (AttendanceOverTimeRest enty in formEntities)
            {
                enty.IsEss = true;
                enty.IsFromEss = true;
                enty.Flag = true;
                enty.EssType = string.Empty;
                enty.EssNo = string.Empty;
                try
                {
                    Factory.GetService<IAttendanceOverTimeRestService>().CheckForESS(enty);
                }
                catch (Exception ex)
                {
                    sb.AppendLine(ex.Message);
                }
            }
            return sb.ToString();
        }

        public void BatchSaveAT406ForAPI(AttendanceOverTimeRest[] formEntities)
        {
            foreach (AttendanceOverTimeRest enty in formEntities)
            {
                enty.IsEss = true;
                enty.IsFromEss = true;
                enty.Flag = true;
                Factory.GetService<IAttendanceOverTimeRestService>().CheckForESS(enty);
            }
            IAttendanceOverTimeRestService service = Factory.GetService<IAttendanceOverTimeRestService>();
            IDocumentService<AttendanceOverTimeRest> docSer = service;

            IAuditObject auditObject = new AnnualLeaveRegister();
            //  IUserService services = Factory.GetService<IUserService>();
            //string employeeId = GetEmpIdByCode(auditEmployeeCode);//  Factory.GetService<IEmployeeServiceEx>().GetEmployeeIdByCode(auditEmployeeCode);
            //if (!employeeId.CheckNullOrEmpty())
            //{
            //    auditObject.ApproveEmployeeId = employeeId.GetGuid();
            //    auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(employeeId);
            //}
            auditObject.ApproveDate = DateTime.Now.Date;
            auditObject.ApproveOperationDate = DateTime.Now;
            auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
            //  auditObject.ApproveResultId =auditResult==true? Constants.AuditAgree:Constants.AuditRefuse;
            auditObject.StateId = Constants.PS03;
            auditObject.ApproveRemark = "API自动审核同意";

            foreach (AttendanceOverTimeRest enty in formEntities)
            {
                auditObject.ApproveResultId = enty.ApproveResultId;
                auditObject.ApproveEmployeeId = enty.ApproveEmployeeId;
                auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(enty.ApproveEmployeeId.GetString());
                service.SaveForESS(enty);
                AttendanceOverTimeRest entyNew = docSer.Read(enty.AttendanceOverTimeRestId);
                service.Audit(new object[] { entyNew.AttendanceOverTimeRestId }, auditObject);

            }
        }


        public string BatchCheckATQJForAPI(AttendanceLeave[] formEntities)
        {
            StringBuilder sb = new StringBuilder();
            foreach (AttendanceLeave enty in formEntities)
            {
                enty.IsEss = true;
                enty.IsFromEss = true;
                enty.Flag = true;
                enty.EssType = string.Empty;
                enty.EssNo = string.Empty;
                try
                {
                    Factory.GetService<IAttendanceLeaveService>().CheckForESS(enty);
                }
                catch (Exception ex)
                {
                    sb.AppendLine(ex.Message);
                }
            }
            return sb.ToString();
        }

        public void BatchSaveATQJForAPI(AttendanceLeave[] formEntities)
        {
            List<string> saveIds = new List<string>();
            StringBuilder msgStr = new StringBuilder();
            IAttendanceLeaveService service = Factory.GetService<IAttendanceLeaveService>();
            IDocumentService<AttendanceLeave> docSer = service.GetServiceNoPower();
            IEmployeeServiceEx empSer = Factory.GetService<IEmployeeServiceEx>();
            IAuditObject auditObject = new AttendanceLeave();
            auditObject.ApproveDate = DateTime.Now.Date;
            auditObject.ApproveOperationDate = DateTime.Now;
            auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
            // auditObject.ApproveResultId = auditResult == true ? Constants.AuditAgree : Constants.AuditRefuse;
            auditObject.ApproveRemark = "API自动审核同意";

            bool hasError = false;
            JArray jArrayResult = new JArray();

            //try
            //{
            foreach (AttendanceLeave entity in formEntities)
            {
                //using (TransactionScope scope = new TransactionScope())
                //{
                //    JObject jObject = new JObject();
                try
                {
                    auditObject.ApproveEmployeeId = entity.ApproveEmployeeId;
                    auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(entity.ApproveEmployeeId.GetString());
                    auditObject.ApproveResultId = entity.ApproveResultId;
                    entity.AttendanceLeaveId = Guid.NewGuid();
                    service.SaveForESS(entity);
                    saveIds.Add(entity.AttendanceLeaveId.GetString());
                    AttendanceLeave entyNew = docSer.Read(entity.AttendanceLeaveId);
                    service.Audit(new object[] { entyNew.AttendanceLeaveId }, auditObject);

                    //jObject["EssNo"] = entity.EssNo;
                    //jObject["Success"] = true;
                    //jObject["Msg"] = string.Empty;
                }
                catch (Exception ex)
                {
                    //jObject["EssNo"] = entity.EssNo;
                    //jObject["Success"] = false;
                    //jObject["Msg"] = ex.Message;
                    //hasError = true;
                    //scope.Dispose();
                    msgStr.AppendFormat("ESSNo:{0} error：{1}", entity.EssNo, ex.Message.ToString());
                    continue;
                }

                //scope.Complete();
                //jArrayResult.Add(jObject);
            }

            if (msgStr.Length > 0)
            {
                throw new Exception(msgStr.ToString());
            }

        }
        //if (hasError)
        //{
        //    throw new Exception(jArrayResult.ToString());
        //}
        //}
        //catch (Exception ex) {
        //    if (saveIds.Count > 0)
        //    {
        //        foreach (string sid in saveIds)
        //        {
        //            docSer.Delete(sid);
        //        }
        //    }
        //    throw new BusinessRuleException(ex.Message.ToString());
        //}
        //}


        public DataTable GetLeaveHoursForAPI(AttendanceLeave attendanceLeave)
        {
            DataTable dt = Factory.GetService<IAttendanceLeaveService>().GetLeaveHoursForGP(attendanceLeave.EmployeeId.GetString(),
                attendanceLeave.BeginDate,
                attendanceLeave.BeginTime,
                attendanceLeave.EndDate, attendanceLeave.EndTime, attendanceLeave.AttendanceTypeId);
            return dt;
        }

        public DataTable GetRestHoursForAPI(AttendanceOverTimeRest formEntity)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Hours");
            dt.Columns.Add("Unit");
            decimal hours = Factory.GetService<IAttendanceOverTimeRestService>().GetHoursForESS(formEntity);
            dt.Rows.Add(hours.ToString("#.##"), "小时");
            return dt;
        }


        public Dictionary<int, DataTable> BatchGetLeaveHours(AttendanceLeave[] formEntities)
        {
            IAttendanceLeaveService leaveSer = Factory.GetService<IAttendanceLeaveService>();
            Dictionary<int, DataTable> dicHours = new Dictionary<int, DataTable>();
            foreach (AttendanceLeave apiEntity in formEntities)
            {
                int number = Array.IndexOf(formEntities, apiEntity);
                dicHours.Add(number, GetLeaveHoursForAPI(apiEntity));
            }
            return dicHours;
        }

        public Dictionary<int, DataTable> BatchGetRestHours(AttendanceOverTimeRest[] formEntities)
        {
            Dictionary<int, DataTable> dicHours = new Dictionary<int, DataTable>();
            foreach (AttendanceOverTimeRest apiEntity in formEntities)
            {
                int number = Array.IndexOf(formEntities, apiEntity);
                dicHours.Add(number, GetRestHoursForAPI(apiEntity));
            }
            return dicHours;
        }

        #endregion

        #region 销假

        public virtual DataTable GetAttLeaveInfoByIdsForAPI(string[] attendanceLeaveInfoIds, string attendanceTypeId)
        {
            try
            {
                if (attendanceLeaveInfoIds == null || attendanceLeaveInfoIds.Length == 0)
                {
                    return new DataTable();
                }
                if (attendanceTypeId.CheckNullOrEmpty())
                {
                    throw new ArgumentNullException("attendanceTypeId");
                }
                StringBuilder sb = new StringBuilder();
                foreach (string str in attendanceLeaveInfoIds)
                {
                    sb.AppendFormat(",'{0}'", str);
                }
                sb.Remove(0, 1);
                string pAtttendanceLeaveInfoIds = sb.ToString();
                DataTable dt = new DataTable();

                string strSql = string.Empty;

                // IDocumentService<AttendanceType> typeService = Factory.GetService<IAttendanceTypeService>().GetServiceNoPower();
                string attkind = HRHelper.ExecuteDataTable(string.Format("select AttendanceKindId from AttendanceType where attendanceTypeId='{0}'", attendanceTypeId)).Rows[0][0].ToString();//typeService.Read(attendanceTypeId).AttendanceKindId;

                #region 执行sql
                if (attkind.Equals("AttendanceKind_007"))
                {//出差
                    strSql = string.Format(@"select BusinessRegisterInfoId as AttendanceLeaveInfoId,info.EmployeeId,AttendanceType.[Name] as TypeName,
                                        convert(varchar,info.BeginDate,111) as BeginDate,info.BeginTime,convert(varchar,info.EndDate,111) as EndDate,info.EndTime, info.Days as Hours,
                                        register.AttendanceTypeId,(SELECT ScName FROM codeinfo WHERE CodeInfoId='AnnualLeaveUnit_003') as AttendanceUnit,info.BusinessRegisterId as AttendanceLeaveId
                                        from BusinessRegisterInfo as info
                                        LEFT JOIN BusinessRegister AS register ON info.BusinessRegisterId = register.BusinessRegisterId
                                        Left join AttendanceType on register.AttendanceTypeId=AttendanceType.AttendanceTypeId
                                        Where BusinessRegisterInfoId in ({0}) and info.IsRevoke=0 ", pAtttendanceLeaveInfoIds);
                }

                else if (attendanceTypeId.Equals("401"))
                {
                    //20120828 modified for 9189 
                    strSql = string.Format(@"select AnnualLeaveRegisterInfoId as AttendanceLeaveInfoId,info.EmployeeId,AttendanceType.[Name] as TypeName,
                                        convert(varchar,info.BeginDate,111) as BeginDate,info.BeginTime,convert(varchar,info.EndDate,111) as EndDate,info.EndTime, info.Days as Hours,
                                        info.AttendanceTypeId,codeinfo.ScName as AttendanceUnit,info.AnnualLeaveRegisterId as AttendanceLeaveId
                                        from AnnualLeaveRegisterInfo as info
                                        Left join AttendanceType on info.AttendanceTypeId=AttendanceType.AttendanceTypeId
                                        Left join codeinfo on info.AnnualLeaveUnit=codeinfo.codeinfoId
                                        Where AnnualLeaveRegisterInfoId in ({0}) and info.IsRevoke=0 ", pAtttendanceLeaveInfoIds);
                }

                else if (attendanceTypeId.Equals("406"))
                {//调休
                    strSql = string.Format(@"select AttendanceLeaveInfoId as AttendanceLeaveInfoId, info.EmployeeId,AttendanceType.[Name] as TypeName,
                                                convert(varchar,info.BeginDate,111) as BeginDate,info.BeginTime,convert(varchar,info.EndDate,111) as EndDate,info.EndTime, info.Hours as Hours,
                                                info.AttendanceTypeId,(SELECT ScName FROM codeinfo WHERE CodeInfoId='AnnualLeaveUnit_003') as AttendanceUnit,info.AttendanceOverTimeRestId as AttendanceLeaveId
                                        from AttendanceOTRestDaily as info
                                        Left join AttendanceType on info.AttendanceTypeId=AttendanceType.AttendanceTypeId
                                        Where AttendanceLeaveInfoId in ({0}) and info.IsRevoke=0  ", pAtttendanceLeaveInfoIds);
                }
                else
                {
                    strSql = string.Format(@"select AttendanceLeaveInfoId,info.EmployeeId,AttendanceType.[Name] as TypeName,
                                        convert(varchar,info.BeginDate,111) as BeginDate,info.BeginTime,convert(varchar,info.EndDate,111) as EndDate,info.EndTime, info.Hours,info.AttendanceTypeId,
                                        codeinfo.ScName as AttendanceUnit,info.AttendanceLeaveId
                                        from AttendanceLeaveInfo as info
                                        Left join AttendanceType on info.AttendanceTypeId=AttendanceType.AttendanceTypeId
                                        Left join codeinfo on attendanceType.attendanceUnitId=codeinfo.codeinfoId
                                        Where AttendanceLeaveInfoId in ({0}) and info.IsRevoke=0 ", pAtttendanceLeaveInfoIds);
                }
                #endregion


                dt = HRHelper.ExecuteDataTable(strSql);
                //}
                return dt;

            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.Message);
            }
        }

        /// <summary>
        /// 特休與調休未休結算校驗
        /// </summary>
        /// <param name="pInfoIds"></param>
        /// <param name="pAttendanceTypeId"></param>
        /// <returns></returns>
        public virtual string CheckSettlementForEss(string pInfoIds, string pAttendanceTypeId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("'" + Guid.Empty.ToString() + "'");
            foreach (string s in pInfoIds.Split('|'))
            {
                if (!s.CheckNullOrEmpty())
                {
                    sb.AppendFormat(",'{0}'", s);
                }
            }

            //20190509 aded by yingchun for Q00-20190506003 : 
            //調整ESS銷假申請，若是審核同意時，銷假的假勤類型為特休假／加班調休假時，
            //當對應的特休計劃資料或調休計劃資料已經被結算時，顯示提示訊息：
            //員工XXX 假勤類型 於銷假開始日期~銷假結束日期的特休計劃/調休計劃資料已結算,請先刪除結算資料再審核
            string strErrorMsg = string.Empty;
            string TypeName = Factory.GetService<IAttendanceTypeService>().GetNameById(pAttendanceTypeId);
            if (pAttendanceTypeId.Equals("406"))
            {
                string sql = string.Format(@"SELECT AttendanceOTRestDaily.AjustIdAndHours,AttendanceOTRestDaily.AttendanceLeaveInfoId,
                                                       AttendanceOTRestDaily.BeginDate,AttendanceOTRestDaily.EndDate,
                                                       Employee.EmployeeId AS EmpId,Employee.CnName AS EmpName
                                                FROM AttendanceOTRestDaily 
                                                LEFT JOIN Employee ON AttendanceOTRestDaily.EmployeeId = Employee.EmployeeId 
                                                WHERE AttendanceLeaveInfoId in ({0}) ", sb.ToString());
                DataTable dtOTRest = HRHelper.ExecuteDataTable(sql);
                if (dtOTRest != null && dtOTRest.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtOTRest.Rows)
                    {
                        StringBuilder sbAdjust = new StringBuilder();
                        string ajustIdAndHours = dr["AjustIdAndHours"].ToString();
                        if (!string.IsNullOrEmpty(ajustIdAndHours))
                        {
                            string[] arrAjustIdAndHours = ajustIdAndHours.Split(';');
                            for (int i = 0; i < arrAjustIdAndHours.Length; i++)
                            {
                                if (string.IsNullOrEmpty(arrAjustIdAndHours[i]))
                                {
                                    continue;
                                }
                                string ajustId = arrAjustIdAndHours[i].Split(',')[0];
                                sbAdjust.AppendFormat(",'{0}'", ajustId);
                            }
                        }
                        if (sbAdjust.Length > 0)
                        {
                            sbAdjust.Remove(0, 1);

                            ////校驗是否有調休結算資料
                            //sql = string.Format(@"SELECT AttendanceUnLeaveHoursId
                            //                      FROM AttendanceUnLeaveHours
                            //                      WHERE AttendanceUnLeaveHours.Flag=1 AND AttendanceUnLeaveHours.SettlementMode='3' 
                            //                      AND AttendanceUnLeaveHours.AttendanceOTAdjustId in ({0})", sbAdjust.ToString());
                            //DataTable dtUnLeave = HRHelper.ExecuteDataTable(sql);
                            //if (dtUnLeave != null && dtUnLeave.Rows.Count > 0)
                            //{
                            //    string msg = "员工{0} 假勤类型 {1}，于销假开始日期～销假结束日期的调休计划资料已结算，请先删除结算资料再审核".Replace("銷假開始日期", "{2}").Replace("銷假結束日期", "{3}").Replace("销假开始日期", "{2}").Replace("销假结束日期", "{3}").Replace("the start date", "{2}").Replace("the end date", "{3}");
                            //    //員工{0} 假勤類型 {1}，於{2}～{3}的調休計劃資料已結算，請先刪除結算資料再審核
                            //    strErrorMsg += string.Format(msg, dr["EmpName"].ToString(), TypeName, DateTime.Parse(dr["BeginDate"].ToString()).ToDateFormatString(), DateTime.Parse(dr["EndDate"].ToString()).ToDateFormatString()) + "\n";
                            //}
                        }
                    }
                }
            }
            return strErrorMsg;
        }


        /// <summary>
        /// 檢查銷假資料
        /// </summary>
        /// <param name="attendanceLeaveInfoIds">請假明細ID數組</param>
        /// <param name="attendanceTypeId">假勤類型ID</param>
        /// <returns>錯誤訊息</returns>
       // [ExternalSystem("API"), APICode("AT_XJ_001")]
        public virtual string CheckRevokeForAPI(string[] attendanceLeaveInfoIds, string attendanceTypeId)
        {
            try
            {
                if (attendanceLeaveInfoIds == null || attendanceLeaveInfoIds.Length == 0)
                {
                    throw new Exception("請假明細不能為空");
                }
                if (attendanceTypeId.CheckNullOrEmpty())
                {
                    throw new ArgumentNullException("attendanceTypeId");
                }
                //檢查考勤月
                DataTable dt = this.GetAttLeaveInfoByIdsForAPI(attendanceLeaveInfoIds, attendanceTypeId);
                if (dt == null || dt.Rows.Count <= 0)
                {
                    throw new Exception("找不到需要销假的請假明細");
                }
                string error = string.Empty;
                StringBuilder sb = new StringBuilder();
                IATMonthService atSrv = Factory.GetService<IATMonthService>();
                foreach (DataRow dr in dt.Rows)
                {
                    error = atSrv.CheckIsClose(new string[] { dr["EmployeeId"].ToString() }, DateTime.Parse(dr["BeginDate"].ToString()), DateTime.Parse(dr["BeginDate"].ToString()));
                    if (!error.CheckNullOrEmpty())
                        sb.AppendLine(error);
                }
                int mainCount = dt.AsEnumerable().Select(t => t["AttendanceLeaveId"].ToString()).Distinct().Count();
                if (mainCount > 1)
                {
                    sb.AppendLine("明細Id須來自同一張主表申請單");
                }
                if (dt.Rows.Count != attendanceLeaveInfoIds.Count())
                {
                    var notFindId = attendanceLeaveInfoIds.Select(t => t.GetGuid()).Except(dt.AsEnumerable().Select(t => t["AttendanceLeaveInfoId"].ToString().GetGuid()));
                    foreach (Guid id in notFindId)
                    {
                        sb.AppendLine(string.Format("找不到明細Id:{0}的資料", id.ToString()));
                    }
                }
                if (!sb.ToString().CheckNullOrEmpty())
                {
                    throw new BusinessRuleException(sb.ToString());
                }

                sb = new StringBuilder();
                foreach (string str in attendanceLeaveInfoIds)
                {
                    sb.AppendFormat("|{0}", str);
                }
                sb.Remove(0, 1);
                //特休與調休未休結算校驗
                error = CheckSettlementForEss(sb.ToString(), attendanceTypeId);
                if (!string.IsNullOrEmpty(error))
                {
                    throw new BusinessRuleException(error);
                }
                //檢查明細是否已審核
                if (CheckAuditedForEss(sb.ToString(), attendanceTypeId))
                    throw new BusinessRuleException(string.Format(@"存在已審核明細"));

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new BusinessRuleException(ex.ToString());
            }
        }

        /// <summary>
        /// 保存銷假資料
        /// </summary>
        /// <param name="formType">單別</param>
        /// <param name="formNumber">單號</param>
        /// <param name="attendanceLeaveInfoIds">請假明細ID數組</param>
        /// <param name="attendanceTypeId">假勤類型ID</param>
        /// <returns>錯誤訊息</returns>
       // [ExternalSystem("API"), APICode("AT_XJ_002")]
        public virtual string SaveRevokeForAPI(string formType, string formNumber, string auditEmployeeCode, bool auditResult, string[] attendanceLeaveInfoIds, string attendanceTypeId)
        {
            //bool hasError = false;
            //JArray jArrayResult = new JArray();
            //using (TransactionScope scope = new TransactionScope())
            //{
            //    JObject jObject = new JObject();
                try
                {
                    if (attendanceLeaveInfoIds == null || attendanceLeaveInfoIds.Length == 0)
                    {
                        throw new Exception("請假明細不能為空");
                    }
                    if (formType.CheckNullOrEmpty())
                    {
                        throw new ArgumentNullException("formType");
                    }
                    if (formNumber.CheckNullOrEmpty())
                    {
                        throw new ArgumentNullException("formNumber");
                    }
                    if (attendanceTypeId.CheckNullOrEmpty())
                    {
                        throw new ArgumentNullException("attendanceTypeId");
                    }

                    DataTable dt = this.GetAttLeaveInfoByIdsForAPI(attendanceLeaveInfoIds, attendanceTypeId);
                    StringBuilder error = new StringBuilder();
                    int mainCount = dt.AsEnumerable().Select(t => t["AttendanceLeaveId"].ToString()).Distinct().Count();
                    if (mainCount > 1)
                    {
                        error.AppendLine("明細Id須來自同一張主表申請單");
                    }
                    if (dt.Rows.Count != attendanceLeaveInfoIds.Count())
                    {
                        var notFindId = attendanceLeaveInfoIds.Except(dt.AsEnumerable().Select(t => t["AttendanceLeaveInfoId"].ToString()));
                        foreach (string id in notFindId)
                        {
                            error.AppendLine(string.Format("找不到明細Id:{0}的資料", id));
                        }
                    }
                    if (!error.ToString().CheckNullOrEmpty())
                    {
                        throw new BusinessRuleException(error.ToString());
                    }

                    StringBuilder sb = new StringBuilder();
                    foreach (string str in attendanceLeaveInfoIds)
                    {
                        sb.AppendFormat("|{0}", str);
                    }
                    sb = sb.Remove(0, 1);
                    this.UpdateEssRevokeStatus(formType, formNumber, sb.ToString(), attendanceTypeId, "Create");


                    #region 保存之后就审核
                    string s = AuditRevokeForAPI(formType, formNumber, auditEmployeeCode, auditResult, attendanceTypeId, "Agree");
                    if (!s.CheckNullOrEmpty())
                    {
                        throw new BusinessRuleException(s.ToString());
                    }

                    #endregion

                    //jObject["EssNo"] = formNumber;
                    //jObject["Success"] = true;
                    //jObject["Msg"] = string.Empty;
                }
                catch (Exception ex)
                {
                    //jObject["EssNo"] = formNumber;
                    //jObject["Success"] = false;
                    //jObject["Msg"] = ex.Message;
                    //hasError = true;
                    //scope.Dispose();
                     throw new BusinessRuleException(ex.ToString());
                }
                //scope.Complete();
                //jArrayResult.Add(jObject);
            //}
            //if (hasError)
            //{
            //    throw new Exception(jArrayResult.ToString());
            //}
            return "sucess";
        }

        /// <summary>
        /// 審核銷假資料
        /// </summary>
        /// <param name="formType">單別</param>
        /// <param name="formNumber">單號</param>
        /// <param name="auditEmployeeCode">登入人工號</param>
        /// <param name="auditResult">審核結果(true表示通過,false表示不通過)</param>
        /// <param name="attendanceTypeId">假勤類型ID</param>
        /// <param name="remark">銷假備註</param>
        /// <returns>錯誤訊息</returns>
        public virtual string AuditRevokeForAPI(string formType, string formNumber, string auditEmployeeCode, bool auditResult, string attendanceTypeId, string remark)
        {
            try
            {
                string[] attendanceLeaveInfoIds = GetLeaveInfoId(formType, formNumber, attendanceTypeId);

                StringBuilder _sb = new StringBuilder();
                foreach (string str in attendanceLeaveInfoIds)
                {
                    _sb.AppendFormat("|{0}", str);
                }
                _sb = _sb.Remove(0, 1);
                //檢查明細是否已審核
                if (CheckAuditedForEss(_sb.ToString(), attendanceTypeId))
                    throw new BusinessRuleException(string.Format(@"存在已審核明細"));

                if (auditResult)
                {

                    //檢查考勤月
                    DataTable atDt = this.GetAttLeaveInfoByIdsForAPI(attendanceLeaveInfoIds, attendanceTypeId);
                    string error = string.Empty;
                    StringBuilder sb = new StringBuilder();
                    IATMonthService atSrv = Factory.GetService<IATMonthService>();
                    foreach (DataRow dr in atDt.Rows)
                    {
                        error = atSrv.CheckIsClose(new string[] { dr["EmployeeId"].ToString() }, DateTime.Parse(dr["BeginDate"].ToString()), DateTime.Parse(dr["BeginDate"].ToString()));
                        if (!error.CheckNullOrEmpty())
                            sb.AppendLine(error);
                    }
                    if (!sb.ToString().CheckNullOrEmpty())
                    {
                        throw new BusinessRuleException(sb.ToString());
                    }


                    this.UpdateEssRevokeStatus(formType, formNumber, _sb.ToString(), attendanceTypeId, "Agree");

                    // if (!auditEmployeeCode.CheckNullOrEmpty()) { }
                    string auditEmpId = GetEmpIdByCode(auditEmployeeCode);//  Factory.GetService<IEmployeeServiceEx>().GetEmployeeIdByCode(auditEmployeeCode);
                    string auditEmpCnName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(auditEmpId);
                    List<string> listInfo = new List<string>();
                    sb.Append("'" + Guid.Empty.ToString() + "'");
                    foreach (string s in attendanceLeaveInfoIds)
                    {
                        if (!s.CheckNullOrEmpty())
                        {
                            sb.AppendFormat(",'{0}'", s);
                            listInfo.Add(s.ToLower());
                        }
                    }
                    IDocumentService<AttendanceType> typeService = Factory.GetService<IAttendanceTypeService>().GetServiceNoPower();
                    string attkind = typeService.Read(attendanceTypeId).AttendanceKindId;
                    //string attkind = HRHelper.ExecuteDataTable("select AttendanceKindId from AttendanceType where attendanceTypeId='" + attendanceTypeId + "'").Rows[0][0].ToString();// typeService.Read(attendanceTypeId).AttendanceKindId;

                    #region 拼接可用明细
                    if (attkind.Equals("AttendanceKind_007"))
                    {//出差
                        using (ITransactionService tran = Factory.GetService<ITransactionService>())
                        {
                            string strSql = string.Format(@"update BusinessRegisterInfo set IsRevoke='1',RevokeDate='{1}',RevokeRemark='{4}' ,RevokeEmployeeId = '{2}', RevokeEmployeeName = '{3}'
                                         Where BusinessRegisterInfoId in ({0})", sb.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), auditEmpId, auditEmpCnName, remark);
                            HRHelper.ExecuteNonQuery(strSql);
                            tran.Complete();
                        }
                    }
                    else if (attendanceTypeId.Equals("406"))
                    {//调休
                        using (ITransactionService tran = Factory.GetService<ITransactionService>())
                        {

                            string[] listIds = _sb.ToString().Split('|');
                            if (listIds.Length > 0)
                            {
                                string strSql = "select AttendanceOverTimeRestId from AttendanceOTRestDaily where attendanceLeaveinfoId='" + listIds[0] + "'";
                                DataTable dt = HRHelper.ExecuteDataTable(strSql);
                                string otRestId = dt.Rows[0][0].ToString();
                                IDocumentService<AttendanceOverTimeRest> otSer = Factory.GetService<IAttendanceOverTimeRestService>().GetServiceNoPower();
                                IAttendanceOverTimeRestService otSer2 = Factory.GetService<IAttendanceOverTimeRestService>();
                                AttendanceOverTimeRest otRest = otSer.Read(otRestId, true);
                                foreach (AttendanceLeaveInfo var in otRest.DailyInfo)
                                {
                                    if (sb.ToString().ToLower().Contains(var.AttendanceLeaveInfoId.GetString().ToLower()))
                                    {
                                        var.RevokeDate = System.DateTime.Now;
                                        var.RevokeRemark = remark;
                                        var.RevokeEmployeeId = auditEmpId.GetGuid();
                                        var.RevokeEmployeeName = auditEmpCnName;
                                        var.RevokeUserId = Constants.SYSTEMGUID_USER_ADMINISTRATOR.GetGuid(); //操作人
                                        var.RevokeOperationDate = System.DateTime.Now;
                                        var.IsRevoke = true;
                                        var.RevokeRemark = remark;
                                    }
                                }

                                otSer2.SaveForRevoke(otRest, listIds);
                            }

                            tran.Complete();
                        }
                    }

                    else if (attendanceTypeId.Equals("401"))
                    {
                        using (ITransactionService tran = Factory.GetService<ITransactionService>())
                        {
                            string strSql = string.Format("Select AnnualLeaveRegisterId From AnnualLeaveRegisterInfo Where AnnualLeaveRegisterInfoId in ({0})", sb.ToString());
                            DataTable dt = HRHelper.ExecuteDataTable(strSql);
                            string registerId = string.Empty;
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                registerId = dt.Rows[0][0].ToString();
                            }
                            AnnualLeaveRegister register = null;
                            IDocumentService<AnnualLeaveRegister> regService = Factory.GetService<IAnnualLeaveRegisterService>().GetServiceNoPower();
                            if (!registerId.CheckNullOrEmpty())
                            {
                                register = regService.Read(registerId);
                            }
                            if (register != null)
                            {
                                IAnnualLeaveRegisterService regSer = Factory.GetService<IAnnualLeaveRegisterService>();
                                regSer.ModfiyBalanceByRevoke(register, listInfo);
                                strSql = string.Format(@"Update AnnualLeaveRegisterInfo set IsRevoke='1',RevokeDate='{1}',RevokeRemark='{3}' ,RevokeEmployeeId = '{2}', RevokeEmployeeName = '{3}' Where AnnualLeaveRegisterInfoId in ({0})", sb.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), auditEmpId, auditEmpCnName, remark);
                                HRHelper.ExecuteNonQuery(strSql);
                            }
                            tran.Complete();
                        }
                    }

                    else
                    {
                        using (ITransactionService tran = Factory.GetService<ITransactionService>())
                        {
                            //this.SetSpecialNew(listInfo.ToArray());
                            string strSql = string.Format(@"update AttendanceLeaveInfo set IsRevoke='1',RevokeDate='{1}',RevokeRemark='{4}' ,RevokeEmployeeId = '{2}', RevokeEmployeeName = '{3}'
                                         Where AttendanceLeaveInfoId in ({0})", sb.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), auditEmpId, auditEmpCnName, remark);
                            HRHelper.ExecuteNonQuery(strSql);
                            //20140904 modified for 21983 21984 21985 && A00-20140901009 by renping
                            this.SetSpecialNew(listInfo.ToArray());
                            tran.Complete();
                        }

                        //20150129 added by lidong 旗舰版销假新增年假结余表处理
                        AttendanceLeave pLeaves = GetLeavebyInfoId(listInfo[0]);
                        if (attendanceTypeId.Equals("401"))
                        {
                            this.ModfiyBalanceByRevoke(pLeaves, listInfo);
                        }

                        #region 20130905add
                        IDocumentService<AttendanceLeave> docSer = Factory.GetService<IAttendanceLeaveService>().GetServiceNoPower();
                        AttendanceLeave pLeave = GetLeavebyInfoId(listInfo[0]);
                        //20130905 added by wangyan  for 任务13287 更新请假总时数，销假时数，有效时数栏位
                        // a.请假总时数:“个人请假信息”附档中请假时数之和
                        decimal totalHours = 0;
                        //b.销假时数：“个人销假信息”附档中请假时数之和
                        decimal cancelHours = 0;
                        //c.有效请假时数：请假总时数-销假时数
                        decimal effectiveHour = 0;

                        foreach (AttendanceLeaveInfo info in pLeave.Infos)
                        {
                            //总时数=明细时数之和
                            totalHours += info.Hours;
                            if (info.IsRevoke)
                            {
                                //销假时数=所有销假的明细时数之和
                                cancelHours += info.Hours;
                            }
                        }
                        effectiveHour = totalHours - cancelHours;

                        pLeave.TotalHours = totalHours;
                        pLeave.CancelHours = cancelHours;
                        pLeave.EffectiveHours = effectiveHour;
                        #endregion

                        docSer.Save(pLeave);

                    }
                    #endregion
                }
                else
                {
                    this.UpdateEssRevokeStatus(formType, formNumber, _sb.ToString(), attendanceTypeId, "Disagree");
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }


        // [ExternalSystem("API"), APICode("AT_XJ_03")]
        public virtual void SaveForRevoke(string formType, string formNumber, string auditEmployeeCode, bool auditResult, string[] attendanceLeaveInfoIds, string attendanceTypeId)
        {
            if (attendanceLeaveInfoIds == null || attendanceLeaveInfoIds.Length == 0)
            {
                throw new Exception("请假明细不能为空");
            }
            if (formType.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("formType");
            }
            if (formNumber.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("formNumber");
            }
            if (attendanceTypeId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("attendanceTypeId");
            }

            DataTable dt = this.GetAttLeaveInfoByIdsForAPI(attendanceLeaveInfoIds, attendanceTypeId);
            StringBuilder error = new StringBuilder();
            int mainCount = dt.AsEnumerable().Select(t => t["AttendanceLeaveId"].ToString()).Distinct().Count();
            if (mainCount > 1)
            {
                error.AppendLine("明细Id必须来自同一张主表申请单");
            }
            if (dt.Rows.Count != attendanceLeaveInfoIds.Count())
            {
                var notFindId = attendanceLeaveInfoIds.Except(dt.AsEnumerable().Select(t => t["AttendanceLeaveInfoId"].ToString()));
                foreach (string id in notFindId)
                {
                    error.AppendLine(string.Format("找不到明細Id:{0}的資料", id));
                }
            }
            if (!error.ToString().CheckNullOrEmpty())
            {
                throw new BusinessRuleException(error.ToString());
            }
            string maiId = "";
            StringBuilder sb = new StringBuilder();
            List<string> ids = new List<string>();
            foreach (string str in attendanceLeaveInfoIds)
            {
                sb.AppendFormat("|{0}", str);
                ids.Add(str);
            }
            sb = sb.Remove(0, 1);

            bool hasError = false;
            JArray jArrayResult = new JArray();
            using (TransactionScope scope = new TransactionScope())
            {
                JObject jObject = new JObject();
                try
                {
                    this.UpdateEssRevokeStatus(formType, formNumber, sb.ToString(), attendanceTypeId, "Create");

                    if (attendanceTypeId == "401")
                    {

                        IDocumentService<AnnualLeaveRegister> docnj = Factory.GetService<IAnnualLeaveRegisterService>().GetServiceNoPower();
                        IAnnualLeaveRegisterService njSer = Factory.GetService<IAnnualLeaveRegisterService>();
                        DataTable dtMain = HRHelper.ExecuteDataTable(string.Format("select AnnualLeaveRegisterId from AnnualLeaveRegisterInfo where AnnualLeaveRegisterInfo='{0}'", attendanceLeaveInfoIds[0]));
                        maiId = dtMain.Rows[0][0].ToString();
                        AnnualLeaveRegister rnty = docnj.Read(maiId);
                        njSer.Revoke(rnty, ids);
                    }
                    else if (attendanceTypeId == "406")
                    {
                        IDocumentService<AttendanceOverTimeRest> docnj = Factory.GetService<IAttendanceOverTimeRestService>().GetServiceNoPower();
                        IAttendanceOverTimeRestService njSer = Factory.GetService<IAttendanceOverTimeRestService>();
                        DataTable dtMain = HRHelper.ExecuteDataTable(string.Format("select AttendanceOTRestDaily.AttendanceOverTimeRestId from AttendanceOTRestDaily where AttendanceLeaveInfoId='{0}'", attendanceLeaveInfoIds[0]));
                        maiId = dtMain.Rows[0][0].ToString();
                        AttendanceOverTimeRest rnty = docnj.Read(maiId);
                        njSer.SaveForRevoke(rnty, attendanceLeaveInfoIds);
                    }
                    else
                    {
                        IDocumentService<AttendanceLeave> docnj = Factory.GetService<IAttendanceLeaveService>().GetServiceNoPower();
                        IAttendanceLeaveService njSer = Factory.GetService<IAttendanceLeaveService>();
                        DataTable dtMain = HRHelper.ExecuteDataTable(string.Format("select AttendanceLeaveId from AttendanceLeaveInfo where AttendanceLeaveInfoId='{0}'", attendanceLeaveInfoIds[0]));
                        maiId = dtMain.Rows[0][0].ToString();
                        AttendanceLeave rnty = docnj.Read(maiId);
                        string employeeId = GetEmpIdByCode(auditEmployeeCode);// Factory.GetService<IEmployeeServiceEx>().GetEmployeeIdByCode(auditEmployeeCode);

                        bool isClose = false;
                        foreach (AttendanceLeaveInfo var in rnty.Infos)
                        {
                            if (attendanceLeaveInfoIds.Contains(var.AttendanceLeaveInfoId.GetString()))
                            {
                                if (var.Date != DateTime.MinValue)
                                {
                                    isClose = Factory.GetService<IAttendanceSumLogService>().CheckATorPAClosebyEmpAndDate(rnty.EmployeeId.GetString(), var.Date);
                                }

                                if (isClose)
                                {
                                    throw new BusinessRuleException(string.Format(Resources.ErrorMsg_SalaryOrAttendanceSumIsClose, Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(rnty.EmployeeId.GetString()), var.Date.ToDateFormatString()));
                                }
                                var.RevokeEmployeeId = employeeId.GetGuid();
                                var.RevokeEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(employeeId); ;
                                var.RevokeDate = DateTime.Now;

                                var.RevokeRemark = "API销假自动审核同意";


                                var.RevokeUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid(); //操作人
                                var.RevokeOperationDate = System.DateTime.Now;
                                var.IsRevoke = true;
                            }
                            njSer.SaveForRevoke(rnty, attendanceLeaveInfoIds);
                            string isg = "'" + Guid.Empty.ToString() + "'";
                            for (int i = 0; i < attendanceLeaveInfoIds.Length; i++)
                            {
                                isg += ",'" + attendanceLeaveInfoIds[i] + "'";
                            }
                            HRHelper.ExecuteNonQuery(string.Format("update AttendanceLeaveInfo set IsRevoke='1' where  AttendanceLeaveInfoId in ({0}) ", isg));
                        }

                    }
                    jObject["EssNo"] = formNumber;
                    jObject["Success"] = true;
                    jObject["Msg"] = string.Empty;
                }
                catch (Exception ex)
                {
                    jObject["EssNo"] = formNumber;
                    jObject["Success"] = false;
                    jObject["Msg"] = ex.Message;
                    hasError = true;
                    scope.Dispose();
                }
                scope.Complete();
                jArrayResult.Add(jObject);
            }

            if (hasError)
            {
                throw new Exception(jArrayResult.ToString());
            }

        }


        /// <summary>
        /// 確認是否已審核銷假單
        /// </summary>
        /// <param name="pAttendanceLeaveInfoIds"></param>
        public bool CheckAuditedForEss(string pAttendanceLeaveInfoIds, string pAttendanceTypeId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("'" + Guid.Empty.ToString() + "'");
            foreach (string s in pAttendanceLeaveInfoIds.Split('|'))
            {
                if (!s.CheckNullOrEmpty())
                {
                    sb.AppendFormat(",'{0}'", s);
                }
            }
            IDocumentService<AttendanceType> typeService = Factory.GetService<IAttendanceTypeService>().GetServiceNoPower();
            string attkind = typeService.Read(pAttendanceTypeId).AttendanceKindId;
            //var attkind = HRHelper.ExecuteDataTable("select AttendanceKindId from AttendanceType where attendanceTypeId='" + pAttendanceTypeId + "'").Rows[0][0].ToString();// typeService.Read(attendanceTypeId).AttendanceKindId;

            if (attkind.Equals("AttendanceKind_007"))//出差
            {
                #region 出差
                DataTable dt = new DataTable();
                //using (IConnectionService conService = Factory.GetService<IConnectionService>())
                //{
                //    IDbCommand cmd = conService.CreateDbCommand();
                //判斷明細是否已銷假
                string strSql = string.Format(@"select * from BusinessRegisterInfo 
                                                    where BusinessRegisterInfoId in ({0}) and (IsRevoke = 1 or (RevokeDate is not null))"
                                                , sb.ToString());
                HRHelper.ExecuteDataTable(strSql);
                //    cmd.CommandText = strSql;
                //    dt.Load(cmd.ExecuteReader());
                //}
                if (dt != null && dt.Rows.Count > 0)
                {
                    return true;
                }
                #endregion
            }
            else if (pAttendanceTypeId.Equals("406"))//调休
            {
                #region 調休
                DataTable dt = new DataTable();
                //using (IConnectionService conService = Factory.GetService<IConnectionService>())
                //{
                //    IDbCommand cmd = conService.CreateDbCommand();
                //判斷明細是否已銷假
                string strSql = string.Format(@"select * from AttendanceOTRestDaily 
                                                    where attendanceLeaveinfoId in ({0}) and (IsRevoke = 1 or (RevokeDate is not null))"
                                                , sb.ToString());
                HRHelper.ExecuteDataTable(strSql);
                //    cmd.CommandText = strSql;
                //    dt.Load(cmd.ExecuteReader());
                //}
                if (dt != null && dt.Rows.Count > 0)
                {
                    return true;
                }
                #endregion
            }

            else if (pAttendanceTypeId.Equals("401"))
            {
                #region 年休
                DataTable dt = new DataTable();
                //using (IConnectionService conService = Factory.GetService<IConnectionService>())
                //{
                //    IDbCommand cmd = conService.CreateDbCommand();
                //判斷明細是否已銷假
                string strSql = string.Format(@"Select * From AnnualLeaveRegisterInfo 
                                                    Where AnnualLeaveRegisterInfoId in ({0}) and (IsRevoke = 1 or (RevokeDate is not null))"
                                                , sb.ToString());
                HRHelper.ExecuteDataTable(strSql);
                //cmd.CommandText = strSql;
                //dt.Load(cmd.ExecuteReader());
                //}
                if (dt != null && dt.Rows.Count > 0)
                {
                    return true;
                }
                #endregion
            }

            else
            {
                #region 請假
                //判斷明細是否已銷假
                string strSql = string.Format(@"Select * From AttendanceLeaveInfo 
                                                    Where AttendanceLeaveInfoId in ({0}) and (IsRevoke = 1 or (RevokeDate is not null))"
                                                , sb.ToString());
                DataTable dt = HRHelper.ExecuteDataTable(strSql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return true;
                }
                #endregion
            }
            return false;
        }

        public virtual void UpdateEssRevokeStatus(string pFormId, string pFormNo, string pAttendanceLeaveInfoIds, string pAttendanceTypeId, string essStatus)
        {
            StringBuilder sb = new StringBuilder();
            List<string> listInfo = new List<string>();
            sb.Append("'" + Guid.Empty.ToString() + "'");
            foreach (string s in pAttendanceLeaveInfoIds.Split('|'))
            {
                if (!s.CheckNullOrEmpty())
                {
                    sb.AppendFormat(",'{0}'", s);
                    listInfo.Add(s);
                }
            }

            //IDocumentService<AttendanceType> typeService = Factory.GetService<IAttendanceTypeService>();
            //string attkind = typeService.Read(pAttendanceTypeId).AttendanceKindId;
            string attkind = HRHelper.ExecuteDataTable("select AttendanceKindId from AttendanceType where attendanceTypeId='" + pAttendanceTypeId + "'").Rows[0][0].ToString();// typeService.Read(attendanceTypeId).AttendanceKindId;

            string strSql = string.Empty;
            string strStatus = pFormId + "-" + pFormNo;

            if (attkind.Equals("AttendanceKind_007")) //出差
            {
                #region 出差撤销状态sql
                switch (essStatus)
                {
                    case "Create":
                        strStatus += Resources.WS_ESSRevokeStatus_001;
                        strSql = string.Format("UPDATE BusinessRegisterInfo SET essrevokeStatus='{0}' WHERE BusinessRegisterInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                    case "Agree":  //审核同意
                        strStatus += Resources.WS_ESSRevokeStatus_002;
                        strSql = string.Format("UPDATE BusinessRegisterInfo SET essrevokeStatus='{0}' WHERE BusinessRegisterInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                    case "Disagree": //审核不同意
                    case "WithDraw": //抽单
                        strStatus = "";
                        strSql = string.Format("UPDATE BusinessRegisterInfo SET essrevokeStatus='{0}' WHERE BusinessRegisterInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                }
                #endregion
            }
            else if (pAttendanceTypeId == "406") //调休假
            {
                #region 调休假撤销状态sql
                switch (essStatus)
                {
                    case "Create":
                        strStatus += Resources.WS_ESSRevokeStatus_001;
                        strSql = string.Format("UPDATE AttendanceOTRestDaily SET EssRevokeStatus='{0}' WHERE AttendanceLeaveInfoId IN({1})", strStatus, sb.ToString());
                        break;
                    case "Agree":  //审核同意
                        strStatus += Resources.WS_ESSRevokeStatus_002;
                        strSql = string.Format("UPDATE AttendanceOTRestDaily SET EssRevokeStatus='{0}' WHERE AttendanceLeaveInfoId IN({1})", strStatus, sb.ToString());
                        break;
                    case "Disagree": //审核不同意
                    case "WithDraw": //抽单
                        strStatus = "";
                        strSql = string.Format("UPDATE AttendanceOTRestDaily SET EssRevokeStatus='{0}' WHERE AttendanceLeaveInfoId IN({1})", strStatus, sb.ToString());
                        break;
                }
                #endregion
            }
            else if (pAttendanceTypeId == "408") //特休
            {
                #region 特休撤销状态sql
                switch (essStatus)
                {
                    case "Create":
                        strStatus += Resources.WS_ESSRevokeStatus_001;
                        strSql = string.Format("UPDATE TWALRegInfo SET EssRevokeStatus='{0}' WHERE TWALRegInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                    case "Agree":  //审核同意
                        strStatus += Resources.WS_ESSRevokeStatus_002;
                        strSql = string.Format("UPDATE TWALRegInfo SET EssRevokeStatus='{0}' WHERE TWALRegInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                    case "Disagree": //审核不同意
                    case "WithDraw": //抽单
                        strStatus = "";
                        strSql = string.Format("UPDATE TWALRegInfo SET EssRevokeStatus='{0}' WHERE TWALRegInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                }
                #endregion
            }

            else if (pAttendanceTypeId.Equals("401"))
            {
                #region 年假撤销状态sql
                switch (essStatus)
                {
                    case "Create":
                        strStatus += Resources.WS_ESSRevokeStatus_001;
                        strSql = string.Format("UPDATE AnnualLeaveRegisterInfo SET EssRevokeStatus='{0}' WHERE AnnualLeaveRegisterInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                    case "Agree":  //审核同意
                        strStatus += Resources.WS_ESSRevokeStatus_002;
                        strSql = string.Format("UPDATE AnnualLeaveRegisterInfo SET EssRevokeStatus='{0}' WHERE AnnualLeaveRegisterInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                    case "Disagree": //审核不同意
                    case "WithDraw": //抽单
                        strStatus = "";
                        strSql = string.Format("UPDATE AnnualLeaveRegisterInfo SET EssRevokeStatus='{0}' WHERE AnnualLeaveRegisterInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                }
                #endregion
            }

            else   //请假
            {
                #region 请假撤销状态sql
                switch (essStatus)
                {
                    case "Create":
                        strStatus += Resources.WS_ESSRevokeStatus_001;
                        strSql = string.Format("UPDATE AttendanceLeaveInfo SET EssRevokeStatus='{0}' WHERE AttendanceLeaveInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                    case "Agree":  //审核同意
                        strStatus += Resources.WS_ESSRevokeStatus_002;
                        strSql = string.Format("UPDATE AttendanceLeaveInfo SET EssRevokeStatus='{0}' WHERE AttendanceLeaveInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                    case "Disagree": //审核不同意
                    case "WithDraw": //抽单
                        strStatus = "";
                        strSql = string.Format("UPDATE AttendanceLeaveInfo SET EssRevokeStatus='{0}' WHERE AttendanceLeaveInfoId IN ({1})", strStatus, sb.ToString());
                        break;
                }
                #endregion
            }

            //using (ITransactionService tran = Factory.GetService<ITransactionService>())
            //{
            //    using (IConnectionService conService = Factory.GetService<IConnectionService>())
            //    {
            //        IDbCommand cmd = conService.CreateDbCommand();
            //        cmd.CommandText = strSql;
            //        cmd.ExecuteNonQuery();
            //    }
            //    tran.Complete();
            //}
            HRHelper.ExecuteNonQuery(strSql);
        }


        public virtual void SetSpecialNew(string[] pLeaveInfoIds)
        {
            if (pLeaveInfoIds.Length == 0)
            {
                throw new BusinessRuleException("AttendanceLeaveInfoIds is Empty");
            }
            List<string> listInfoIds = new List<string>();
            foreach (string str in pLeaveInfoIds)
            {
                listInfoIds.Add(str);
            }
            string infoId = pLeaveInfoIds[0];
            AttendanceLeave pLeave = this.GetLeavebyInfoId(infoId);
            IDocumentService<AttendanceType> typeSer = Factory.GetService<IAttendanceTypeService>().GetServiceNoPower();
            AttendanceType attype = typeSer.Read(pLeave.AttendanceTypeId);
            //string attkind = HRHelper.ExecuteDataTable("select AttendanceKindId from AttendanceType where attendanceTypeId='" + pLeave.AttendanceTypeId + "'").Rows[0][0].ToString();// typeService.Read(attendanceTypeId).AttendanceKindId;

            string employeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(pLeave.EmployeeId.GetString());
            if (attype != null && attype.AttendanceKindId.Equals("AttendanceKind_011"))
            {
                string specialId = string.Empty;
                decimal specialHours = 0;
                DataTable tempDt = null;
                IATSpecialHolidaySetService setSpecialSer = Factory.GetService<IATSpecialHolidaySetService>();
                IDocumentService<ATSpecialHolidaySet> docAtSpecial = setSpecialSer.GetServiceNoPower();
                ATSpecialHolidaySet tempSet = null;
                foreach (AttendanceLeaveInfo info in pLeave.Infos)
                {
                    if (listInfoIds.Contains(info.AttendanceLeaveInfoId.GetString().ToLower()))
                    {
                        if (!info.SpecialSetIdAndHours.CheckNullOrEmpty())
                        {
                            string[] arr1 = info.SpecialSetIdAndHours.Split(';');
                            if (arr1.Length > 0)
                            {
                                foreach (string tempStr in arr1)
                                {
                                    string[] arr2 = tempStr.Split(',');
                                    if (arr2.Length == 2)
                                    {
                                        specialId = arr2[0];
                                        specialHours = Convert.ToDecimal(arr2[1]);

                                        tempSet = null;
                                        try
                                        {
                                            tempSet = docAtSpecial.Read(specialId);
                                        }
                                        catch
                                        {
                                            //吃掉内存，因为没有引用，可能存在用到的特殊假记录被删除了
                                        }
                                        if (tempSet != null)
                                        {
                                            // 20131216 added by jiangpeng for 已经是天了，不需要折算 for bug 14926 (旧bug相关:bug12920)
                                            //if (attype.AttendanceUnitId.Equals("AttendanceUnit_001"))
                                            //{
                                            //    specialHours = specialHours / tempSet.DaySTHours;
                                            //    specialHours = Math.Round(specialHours, 3);
                                            //}
                                            decimal oldActualDays = tempSet.ActualDays;
                                            if (tempSet.IsOnceOver)
                                            {
                                                #region 20140716 add by LinBJ 19882 19884 19886 C01-20140714013 by 特殊假如果是單次休完，銷假需全部明細銷假才能歸還時數
                                                bool isAllRevoke = true;
                                                foreach (AttendanceLeaveInfo item in pLeave.Infos)
                                                {
                                                    if (item.IsRevoke == false)
                                                    {
                                                        isAllRevoke = false;
                                                        break;
                                                    }
                                                }
                                                if (isAllRevoke)
                                                {
                                                    tempSet.RemaiderDays = tempSet.Amount;
                                                    tempSet.ActualDays = 0;
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                tempSet.ActualDays = tempSet.ActualDays - specialHours;
                                                if (tempSet.ActualDays < 0)
                                                    tempSet.ActualDays = 0;

                                                tempSet.RemaiderDays = tempSet.Amount - tempSet.ActualDays;
                                            }

                                            #region 20180711 add by LinBJ for Q00-20180709001 增加Log
                                            string msg = string.Format("{0}员工销假{1} {2} ~ {3} {4} {5}数量为{6}，回写{7}，原已休数量{8}，更新后已休数量{9}，可休剩余数量{10}"
                                            , employeeName, info.BeginDate.ToDateFormatString(), info.BeginTime, info.EndDate.ToDateFormatString(), info.EndTime
                                            , attype.Name, oldActualDays - tempSet.ActualDays, tempSet.ATSpecialHolidaySetId.ToString(), oldActualDays, tempSet.ActualDays, tempSet.RemaiderDays);
                                            if (tempSet.ExtendedProperties.ContainsKey("InfoStrList"))
                                            {
                                                List<string> infoStrList = tempSet.ExtendedProperties["InfoStrList"] as List<string>;
                                                infoStrList.Add(msg);
                                            }
                                            else
                                            {
                                                List<string> infoStrList = new List<string>();
                                                infoStrList.Add(msg);
                                                tempSet.ExtendedProperties.Add("InfoStrList", infoStrList);
                                            }
                                            if (!tempSet.LeaveInfoIds.CheckNullOrEmpty())
                                            {
                                                string[] infoArray = tempSet.LeaveInfoIds.Split(',');
                                                if (infoArray.Length > 0)
                                                {
                                                    List<string> infoIdList = infoArray.ToList();
                                                    if (infoIdList.Contains(info.AttendanceLeaveInfoId.ToString()))
                                                    {
                                                        infoIdList.Remove(info.AttendanceLeaveInfoId.ToString());
                                                        tempSet.LeaveInfoIds = string.Join(",", infoIdList.ToArray());
                                                    }
                                                }
                                            }
                                            #endregion
                                            docAtSpecial.Save(tempSet);
                                        }
                                    }
                                }
                            }
                        }
                        //清空请假明细的特殊假ID和Hours值
                        UpdateLeaveInfoSpecial(info.AttendanceLeaveInfoId.GetString(), string.Empty);
                    }
                }
            }

        }

        protected AttendanceLeave GetLeavebyInfoId(string pInfoId)
        {
            DataTable dt = new DataTable();
            using (IConnectionService conSer = Factory.GetService<IConnectionService>())
            {
                IDbCommand cmd = conSer.CreateDbCommand();
                string strSql = string.Empty;
                strSql = string.Format("select AttendanceLeaveId From AttendanceLeaveInfo Where AttendanceLeaveInfoId='{0}'", pInfoId);
                cmd.CommandText = strSql;
                dt.Load(cmd.ExecuteReader());
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                string attendanceLeaveId = dt.Rows[0][0].ToString();
                IDocumentService<AttendanceLeave> leaveSer = Factory.GetService<IAttendanceLeaveService>().GetServiceNoPower();
                return leaveSer.Read(attendanceLeaveId);
            }
            throw new Exception("No AttendanceLeaveId");
        }

        protected void UpdateLeaveInfoSpecial(string pLeaveInfoId, string pIdAndHours)
        {
            DataTable dtLeave = new DataTable();
            using (IConnectionService conSer = Factory.GetService<IConnectionService>())
            {
                IDbCommand cmd = conSer.CreateDbCommand();
                cmd.CommandText = string.Format("UPDATE AttendanceLeaveInfo SET SpecialSetIdAndHours = '{0}' Where AttendanceLeaveInfoId='{1}'", pIdAndHours, pLeaveInfoId);
                dtLeave.Load(cmd.ExecuteReader());
            }
        }

        //销假时更新年假结余表
        protected void ModfiyBalanceByRevoke(AttendanceLeave pDataEntity, List<string> infoIds)
        {
            IEmployeeServiceEx iEmployeeServiceEx = Factory.GetService<IEmployeeServiceEx>();
            string corporationId = iEmployeeServiceEx.GetCorporationIdById(pDataEntity.EmployeeId.GetString());
            IAnnualLeaveBalanceService balanceService = Factory.GetService<IAnnualLeaveBalanceService>();
            AnnualLeaveBalance alBalance = balanceService.GetThisAnnualLeaveBalance(pDataEntity.FiscalYearId.GetString(), pDataEntity.EmployeeId.GetString());
            if (alBalance != null)
            {

                DateTime balanceEndDate = alBalance.BalanceEndDate;//结余截至日期
                DateTime registerBeginDate = pDataEntity.BeginDate;//开始日期
                decimal beforeDays = 0m;
                decimal afterDays = 0m;
                decimal oldAction = alBalance.ActualDays;
                // 20120423  added by jiangpeng for 更新前先判断单位
                AnnualLeaveParameter para = Factory.GetService<IAnnualLeaveParameterService>().GetParameterByEmpId(pDataEntity.EmployeeId.GetString());
                string daysName = "Days";
                if (para != null && para.AnnualLeaveUnitId != null && para.AnnualLeaveUnitId.Equals("AnnualLeaveUnit_003"))
                {
                    daysName = "Hours";
                }
                foreach (AttendanceLeaveInfo info in pDataEntity.Infos)
                {
                    if (infoIds.Contains(info.AttendanceLeaveInfoId.GetString()))
                    {
                        if (daysName.Equals("Hours"))
                        {
                            if (info.BeginDate <= balanceEndDate)
                            {
                                beforeDays += info.Hours;
                            }
                            else
                            {
                                afterDays += info.Hours;
                            }
                            //20110527 added by songyj for 本年已休=在财政年度内员工所休的所有年假信息之和，包括结余已休
                            alBalance.ActualDays -= info.Hours;
                        }
                        else
                        {
                            if (info.BeginDate <= balanceEndDate)
                            {
                                beforeDays += info.Days;
                            }
                            else
                            {
                                afterDays += info.Days;
                            }
                            //20110527 added by songyj for 本年已休=在财政年度内员工所休的所有年假信息之和，包括结余已休
                            alBalance.ActualDays -= info.Days;
                        }
                    }
                }
                #region 20181105 add by LinBJ for A00-20181029001 年假優先歸還給本年時數
                if (oldAction - alBalance.BalanceActualDays >= beforeDays && beforeDays > 0 && alBalance.BalanceActualDays >= beforeDays)
                {
                    decimal thisYearAction = oldAction - alBalance.BalanceActualDays;
                    if (thisYearAction > beforeDays)
                    {
                        afterDays += beforeDays;
                        beforeDays = 0;
                    }
                    else
                    {
                        beforeDays -= thisYearAction;
                        afterDays += thisYearAction;
                    }
                }
                #endregion
                if (beforeDays > 0)
                {
                    if (alBalance.BalanceActualDays >= beforeDays)
                    {
                        alBalance.BalanceActualDays -= beforeDays;
                        //20110513 added by songyj for 本年已休=在财政年度内员工所休的所有年假信息之和，包括结余已休
                        //alBalance.ActualDays -= beforeDays;
                    }
                    else
                    {
                        afterDays = beforeDays - alBalance.BalanceActualDays + afterDays;
                        alBalance.BalanceActualDays = 0;
                    }
                    alBalance.BalanceRemaiderDays = alBalance.BalanceDays - alBalance.BalanceActualDays - alBalance.BalanceVoidDays;
                }
                //alBalance.ActualDays -= afterDays;
                //alBalance.PlanDays = alBalance.ThisYearDays + alBalance.BalanceRemaiderDays ;//本年可休天数
                alBalance.RemainderDays = alBalance.PlanDays - alBalance.ActualDays;//本年未休天数

                IAnnualLeaveParameterService iAnnualLeaveParameterService = Factory.GetService<IAnnualLeaveParameterService>();

                if (iAnnualLeaveParameterService.GetParameterIdByCorporationIdWithNoPower(corporationId).IsOnce)
                {
                    alBalance.RemainderDays = 0;
                    alBalance.BalanceRemaiderDays = 0;
                }

                IDocumentService<AnnualLeaveBalance> docService = balanceService.GetServiceNoPower();
                //balanceService.Save(alBalance);
                docService.Save(alBalance);

            }
        }

        public string[] GetLeaveInfoId(string formType, string formNumber, string attendanceTypeId)
        {
            IDocumentService<AttendanceType> typeService = Factory.GetService<IAttendanceTypeService>().GetServiceNoPower(); ;
            string attkind = typeService.Read(attendanceTypeId).AttendanceKindId;

            string strSql = string.Empty;
            string strStatus = formType + "-" + formNumber;

            if (attkind.Equals("AttendanceKind_007")) //出差
            {
                strSql = string.Format(@"SELECT BusinessRegisterInfoId _id FROM BusinessRegisterInfo WHERE EssRevokeStatus like '{0}%'", strStatus);
            }
            else if (attendanceTypeId == "406") //调休假
            {
                strSql = string.Format(@"SELECT AttendanceLeaveInfoId _id FROM AttendanceOTRestDaily WHERE EssRevokeStatus like '{0}%'", strStatus);
            }
            else if (attendanceTypeId == "408") //特休
            {
                strSql = string.Format(@"SELECT TWALRegInfoId _id FROM TWALRegInfo WHERE EssRevokeStatus like '{0}%'", strStatus);
            }
            else if (attendanceTypeId.Equals("401"))
            {
                strSql = string.Format(@"SELECT AnnualLeaveRegisterInfoId _id FROM AnnualLeaveRegisterInfo WHERE EssRevokeStatus like '{0}%'", strStatus);
            }
            else //请假
            {
                strSql = string.Format(@"SELECT AttendanceLeaveInfoId _id FROM AttendanceLeaveInfo WHERE EssRevokeStatus like '{0}%'", strStatus);
            }
            DataTable dt = HRHelper.ExecuteDataTable(strSql);
            if (!dt.CheckNullOrEmpty() && dt.Rows.Count > 0)
            {
                return dt.AsEnumerable().Select(r => r.Field<Guid>("_id").ToString()).ToArray();
            }
            throw new BusinessRuleException(string.Format("找不到單別({0})與單號({1})對應的請假申請id", formType, formNumber));
        }

        public string GetEmpIdByCode(string code) {
            DataTable dt = HRHelper.ExecuteDataTable("select employeeid from employee where code='"+code+"'");
            return dt.Rows[0][0].ToString();
        }

        #endregion

    }
}

