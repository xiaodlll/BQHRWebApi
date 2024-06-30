using Dcms.Common;
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
            return msgStr.ToString();
        }


        public void SaveBusinessApplyForAPI(BusinessApply[] formEntities)
        {
            Dictionary<int, string> dicMsg = new Dictionary<int, string>();
            foreach (BusinessApply businessApply in formEntities)
            {
                Factory.GetService<IBusinessApplyService>().CheckForESS(businessApply);
                Factory.GetService<IBusinessApplyService>().SaveForESS(businessApply);
            }
        }
    }
}

