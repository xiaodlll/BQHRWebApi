using Dcms.Common;
using Dcms.Common.Services;
using Dcms.HR.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dcms.HR.Services
{
    public partial class ExtendItemService
    {
        public string CheckBusinessApplyForAPI(BusinessApply[] formEntities)
        {
            StringBuilder msgStr= new StringBuilder();
            int i = 0;
            foreach (BusinessApply businessApply in formEntities)
            {
                i++;
             string s=  Factory.GetService<IBusinessApplyService>().CheckForESS(businessApply);
                if (!s.CheckNullOrEmpty()) {
                   // dicMsg.Add(i, s);
                    msgStr.Append(string.Format("{0}:{1}",i,s));
                }
            }
            if (msgStr.Length>0)
            {
                throw new BusinessRuleException(msgStr.ToString());
            }
            return msgStr.ToString();
        }


        public void SaveBusinessApplyForAPI(BusinessApply[] formEntities)
        {
            IBusinessApplyService service = Factory.GetService<IBusinessApplyService>();
            IDocumentService<BusinessApply> docSer = service.GetServiceNoPower();

            Dictionary<int, string> dicMsg = new Dictionary<int, string>();
            foreach (BusinessApply businessApply in formEntities)
            {
               string s= service.CheckForESS(businessApply);
                if (!s.CheckNullOrEmpty()) {
                    throw new BusinessRuleException(s);
                }
                service.SaveForESS(businessApply);
                BusinessApply entyNew = docSer.Read(businessApply.BusinessApplyId);
                IAuditObject auditObject = new BusinessApply();
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
                service.Audit(new object[] { entyNew.BusinessApplyId }, auditObject);
            }
        }
    }
}

