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

        public void CheckForBusinessRegisterForEss(BusinessRegister[] businessRegisters)
        {
            foreach (var item in businessRegisters)
            {
                string pEmployeeIds = string.Empty;

                foreach (var detail in item.RegisterInfos)
                {
                    pEmployeeIds += detail.EmployeeId.GetString() + "|";
                }

                Factory.GetService<IBusinessRegisterService>().CheckForESS(item.EssType, item.EssNo, item.RegisterMode,
            item.BusinessApplyId.GetString(), pEmployeeIds.TrimEnd('|'), item.AttendanceTypeId, item.Location, item.BeginDate, item.BeginTime, item.EndDate, item.EndTime, 0, item.Remark);
            }
        }

        public void SaveForBusinessRegisterForEss(BusinessRegister[] businessRegisters)
        {
            foreach (var item in businessRegisters)
            {
                string pEmployeeIds = string.Empty;

                foreach (var detail in item.RegisterInfos)
                {
                    pEmployeeIds += detail.EmployeeId.GetString() + "|";
                }

                Factory.GetService<IBusinessRegisterService>().SaveForESS(item.EssType, item.EssNo, item.RegisterMode,
            item.BusinessApplyId.GetString(), pEmployeeIds.TrimEnd('|'), item.AttendanceTypeId, item.Location, item.BeginDate, item.BeginTime, item.EndDate, item.EndTime, 0, item.Remark);
            }
        }
    }
}
