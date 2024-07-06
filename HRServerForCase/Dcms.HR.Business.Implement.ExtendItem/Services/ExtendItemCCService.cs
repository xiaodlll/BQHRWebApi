using Dcms.Common;
using Dcms.Common.Core;
using Dcms.Common.Services;
using Dcms.HR.Business.Implement.Properties;
using Dcms.HR.DataEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Dcms.HR.Services
{
    public partial class ExtendItemService
    {
        //AT_CC_01
        public string CheckBusinessApplyForAPI(BusinessApply[] formEntities)
        {
            StringBuilder msgStr = new StringBuilder();
            int i = 0;
            foreach (BusinessApply businessApply in formEntities)
            {
                i++;
                ExceptionCollection ec = SaveBeforeCheck(businessApply, true);
                if (ec.Count > 0)
                {
                    throw new BusinessVerifyException(new string[] { i.ToString() }, ec);
                }

            }
            if (msgStr.Length > 0)
            {
                throw new BusinessRuleException(msgStr.ToString());
            }
            return msgStr.ToString();
        }

        //AT_CC_02
        public void SaveBusinessApplyForAPI(BusinessApply[] formEntities)
        {
            IBusinessApplyService service = Factory.GetService<IBusinessApplyService>();
            IDocumentService<BusinessApply> docSer = service.GetServiceNoPower();

            Dictionary<int, string> dicMsg = new Dictionary<int, string>();
            foreach (BusinessApply businessApply in formEntities)
            {
                foreach (BusinessApplyPerson person in businessApply.Persons)
                {
                    person.BusinessApplyPersonId = Guid.NewGuid();
                }
                foreach (BusinessApplySchedule sch in businessApply.Schedules)
                {
                    sch.BusinessApplyScheduleId = Guid.NewGuid();
                }
                //20160628 modi by songll for Q00-20160607003 35639 35640 35641 添加BusinessApplyAtt数据
                BusinessApply newEnty = SetBusinessApplyAttendance(businessApply);
                docSer.Save(businessApply);
                //   BusinessApply entyNew = docSer.Read(businessApply.BusinessApplyId);
                IAuditObject auditObject = new AttendanceLeave();
                IUserService services = Factory.GetService<IUserService>();
                string employeeId = businessApply.FoundEmployeeId.GetString();
                if (!employeeId.CheckNullOrEmpty())
                {
                    auditObject.ApproveEmployeeId = employeeId.GetGuid();
                    auditObject.ApproveEmployeeName = Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(employeeId);
                }
                auditObject.ApproveDate = DateTime.Now.Date;
                auditObject.ApproveOperationDate = DateTime.Now;
                auditObject.ApproveUserId = (Factory.GetService<ILoginService>()).CurrentUser.UserId.GetGuid();
                auditObject.ApproveResultId = businessApply.ApproveResultId ;
                auditObject.ApproveRemark = "API自动审核同意";
                auditObject.StateId = Constants.PS03;
                service.Audit(new object[] { businessApply.BusinessApplyId }, auditObject);
            }
        }

        /// <summary>
        /// 設定BusinessApplyAttendance
        /// </summary>
        /// <param name="pBusinessApply"></param>
        /// <returns></returns>
        public BusinessApply SetBusinessApplyAttendance(BusinessApply pBusinessApply)
        {
            #region 添加员工出差打卡明细
            DateTime applyB = pBusinessApply.BeginDate.AddTimeToDateTime(pBusinessApply.BeginTime);
            DateTime applyE = pBusinessApply.EndDate.AddTimeToDateTime(pBusinessApply.EndTime);
            DataTable dt = new DataTable();
            List<string> list = pBusinessApply.Persons.Select(t => t.EmployeeId.ToString()).ToList();
            //获得员工的班次信息
            dt = Factory.GetService<IAttendanceEmployeeRankService>().GetEmpsDailyInfo(
                               list.ToArray(), pBusinessApply.BeginDate.Date, pBusinessApply.EndDate.Date);
            if (dt == null || dt.Rows.Count == 0)
            {
                return pBusinessApply;
            }
            pBusinessApply.Attendances.Clear();

            IDocumentService<AttendanceRank> rankService = Factory.GetService<IAttendanceRankService>().GetServiceNoPower();
            AttendanceRank rank = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DateTime tempB = Convert.ToDateTime(dt.Rows[i]["BeginTime"]);
                DateTime tempE = Convert.ToDateTime(dt.Rows[i]["EndTime"]);
                if (tempB >= applyE || tempE <= applyB)
                {
                    //该班次时间不在出差时间段内
                    continue;
                }
                if (tempB < applyB)
                {
                    tempB = applyB;
                }
                if (tempE > applyE)
                {
                    tempE = applyE;
                }

                // 20090427 modified by zhonglei for 是否扣除休息就餐段
                decimal notJobTime = 0;
                DateTime restBegin = DateTime.MinValue;
                DateTime restEnd = DateTime.MinValue;
                rank = rankService.Read(dt.Rows[i]["AttendanceRankId"].ToString());
                for (int j = 0; j < rank.Rests.Count; j++)
                {
                    if (rank.Rests[j].AttendanceRankTypeId.Equals("AttendanceRankType_003") || rank.Rests[j].AttendanceRankTypeId.Equals("AttendanceRankType_002"))
                    {
                        if (rank.Rests[j].NotJobTime)
                        {
                            restBegin = DateTime.Parse(string.Format("{0} {1}", tempB.ToShortDateString(), rank.Rests[j].RestBeginTime));
                            restEnd = DateTime.Parse(string.Format("{0} {1}", tempB.ToShortDateString(), rank.Rests[j].RestEndTime));
                            //出差结束时间在休息就餐段内
                            if (tempE > restBegin && tempE < restEnd && tempB < restBegin)
                            {
                                notJobTime += (decimal)(tempE - restBegin).TotalHours;
                            }
                            //出差开始时间在休息就餐段内
                            else if (tempB > restBegin && tempB < restEnd && tempE > restEnd)
                            {
                                notJobTime += (decimal)(restEnd - tempB).TotalHours;
                            }
                            //出差时间在休息就餐段内 
                            else if (tempB >= restBegin && tempE <= restEnd)
                            {
                                notJobTime += (decimal)(tempE - tempB).TotalHours;
                            }
                            //出差时间包含休息就餐段
                            else if (tempB <= restBegin && tempE >= restEnd)
                            {
                                notJobTime += rank.Rests[j].RestHours;
                            }
                        }
                    }
                }
                if (notJobTime > 0)
                {
                    notJobTime = Math.Round(notJobTime, 2);
                }

                #region MyRegion
                BusinessApplyAttendance tempAT = new BusinessApplyAttendance();
                tempAT.BusinessApplyAttendanceID = Guid.NewGuid();
                tempAT.Flag = true;
                tempAT.EmployeeId = dt.Rows[i]["EmployeeId"].ToString().GetGuid();
                tempAT.Date = tempB.Date;
                tempAT.BeginDate = tempB.Date;
                tempAT.BeginTime = tempB.ToString(Constants.FORMAT_SHORTTIMEPATTERN);
                tempAT.EndDate = tempE.Date;
                tempAT.EndTime = tempE.ToString(Constants.FORMAT_SHORTTIMEPATTERN);
                tempAT.ApplyHours = (decimal)((tempE - tempB).TotalHours) - notJobTime;
                tempAT.AttendanceRankId = dt.Rows[i]["AttendanceRankId"].ToString();
                #endregion
                pBusinessApply.Attendances.Add(tempAT);
            }
            #endregion
            return pBusinessApply;
        }


        /// <summary>
        /// ESS校验出差申请
        /// </summary>
        /// <param name="pApply"></param>
        /// <returns></returns>
        [ExternalSystem("Ess")]
        public string CheckForESS(BusinessApply pApply)
        {
            ExceptionCollection ec = SaveBeforeCheck(pApply, true);
            if (ec.Count > 0)
            {
                throw new BusinessVerifyException(new string[] { pApply.BusinessApplyId.GetString() }, ec);
            }
            return "True";
        }

        /// <summary>
        /// 保存方法前的检查
        /// </summary>
        /// <param name="pDataEntity">出差申请实体</param>
        /// <param name="pIsNewEntity">是否新增</param>
        /// <returns>异常集合ExceptionCollection</returns>
        private ExceptionCollection SaveBeforeCheck(BusinessApply pDataEntity, bool pIsNewEntity)
        {
            ExceptionCollection ec = new ExceptionCollection();
            HRVerifyHelper vh = new HRVerifyHelper(ec);
            string error = this.validateData(pDataEntity, pIsNewEntity);
            if (!error.CheckNullOrEmpty())
            {
                ec.Add(new Exception(error));
            }
            return ec;
        }
        private string validateData(BusinessApply pData, bool pIsNew)
        {
            string errMsg = string.Empty;
            string errSameMsg = string.Empty;
            StringBuilder allErrMsg = new StringBuilder();
            foreach (BusinessApplyPerson itemChild in pData.Persons)
            {
                errMsg = Factory.GetService<IAttendanceLeaveService>().CheckAllTime(itemChild.EmployeeId.GetString(), pData.BeginDate, pData.BeginTime, pData.EndDate, pData.EndTime
                    , CheckEntityType.Apply, pIsNew, pData.BusinessApplyId.GetString());
                if (!errMsg.CheckNullOrEmpty())
                {
                    allErrMsg.AppendLine(errMsg);
                }
                #region 檢查出差申請
                DateTime bDate = pData.BeginDate.AddTimeToDateTime(pData.BeginTime);
                DateTime eDate = pData.EndDate.AddTimeToDateTime(pData.EndTime);
                errSameMsg = IsSameBusinessApply(pData.BusinessApplyId.ToString(), itemChild.EmployeeId.ToString(), bDate, eDate);
                if (!errSameMsg.CheckNullOrEmpty() && allErrMsg.Length == 0)
                {
                    allErrMsg.AppendLine(errSameMsg);
                }
                #endregion
                //if (!allErrMsg.ToString().CheckNullOrEmpty()) {
                //    throw new BusinessRuleException(allErrMsg.ToString());
                //}
            }
            return allErrMsg.ToString();
        }

        //20141113 LinBJ add by 23877 23878 23879 Q00-20141112006 新增一筆出差申請，員工已存在相同時間的出差申請時，應該要做校驗
        /// <summary>
        /// 檢查是否有重複數據
        /// </summary>
        /// <param name="pBusinessApplyId"></param>
        /// <param name="pEmployeeId"></param>
        /// <param name="pBDateTime"></param>
        /// <param name="pEDateTime"></param>
        /// <returns></returns>
        private string IsSameBusinessApply(string pBusinessApplyId, string pEmployeeId, DateTime pBDateTime, DateTime pEDateTime)
        {
            DataTable dt = new DataTable();
            DataTable dtRegister = new DataTable();
            StringBuilder errorMsg = new StringBuilder();
            using (IConnectionService conService = Factory.GetService<IConnectionService>())
            {
                IDbCommand cmd = conService.CreateDbCommand();
                string strSql = string.Format(@"select detail.BusinessApplyId, detail.EmployeeId,main.BeginDate,main.BeginTime,main.EndDate,main.EndTime,main.AttendanceTypeId
                                from BusinessApplyPerson detail
                                left join BusinessApply main on detail.BusinessApplyId=main.BusinessApplyId
                                where 
                                detail.BusinessApplyId <> '{0}' and
                                detail.EmployeeId='{1}' and
                                isNull(main.ApproveResultId,'')<>'OperatorResult_002' and 
                                (main.BeginDate <= '{2}' AND main.EndDate >= '{3}')",
pBusinessApplyId, pEmployeeId, pBDateTime.ToDateFormatString(), pEDateTime.ToDateFormatString());
                cmd.CommandText = strSql;
                dt.Load(cmd.ExecuteReader());
            }
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    IAttendanceTypeService typeSer = Factory.GetService<IAttendanceTypeService>();
                    DateTime beginDate = DateTime.MinValue;
                    DateTime endDate = DateTime.MinValue;
                    string strBegin = row["BeginDate"].ToString();
                    string strEnd = row["EndDate"].ToString();
                    string strBeginTime = row["BeginTime"].ToString();
                    string strEndTime = row["EndTime"].ToString();
                    if (!strBegin.CheckNullOrEmpty() && !strEnd.CheckNullOrEmpty()
                    && !strBeginTime.CheckNullOrEmpty() && !strEndTime.CheckNullOrEmpty())
                    {
                        if (DateTime.TryParse(strBegin, out beginDate) && DateTime.TryParse(strEnd, out endDate))
                        {
                            beginDate = beginDate.AddTimeToDateTime(strBeginTime);
                            endDate = endDate.AddTimeToDateTime(strEndTime);
                            if (!CheckTimeAcross(pBDateTime, pEDateTime, beginDate, endDate))
                            {
                                #region 20150330 add by LinBJ for 27425 27426 A00-20150327003 須排除出差登記已銷假資料
                                using (IConnectionService conService = Factory.GetService<IConnectionService>())
                                {
                                    IDbCommand cmd = conService.CreateDbCommand();
                                    string strSql = string.Format(@"select info.BusinessRegisterInfoId from BusinessRegisterInfo info
                                                        left join BusinessRegister main on main.BusinessRegisterId =info.BusinessRegisterId 
                                                        where  main.BusinessApplyId='{0}' and info.EmployeeId='{1}' and info.IsRevoke = 1",
                                                                    row["BusinessApplyId"].ToString(), pEmployeeId);
                                    cmd.CommandText = strSql;
                                    dtRegister.Load(cmd.ExecuteReader());
                                }
                                if (dtRegister.Rows.Count > 0)
                                {
                                    continue;
                                }
                                #endregion
                                errorMsg.AppendLine(string.Format(Resources.AttendanceLeave_LeaveInfoRepeat, Factory.GetService<IEmployeeServiceEx>().GetEmployeeNameById(pEmployeeId),
                                    beginDate.ToDateTimeFormatString(), endDate.ToDateTimeFormatString(),
                                    Factory.GetService<IAttendanceTypeService>().GetNameById(row["AttendanceTypeId"].ToString())));
                            }
                        }
                    }
                }
            }
            return errorMsg.ToString();
        }

    }
}

