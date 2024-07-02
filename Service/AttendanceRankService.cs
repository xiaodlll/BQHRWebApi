using Dcms.Common;
using Dcms.HR.DataEntities;
using Dcms.HR.Services;
using System.Data;

namespace BQHRWebApi.Service
{
    public class AttendanceRankService
    {

        public AttendanceRankService() { }

        public DataTable GetRankRestInfo(string pRankId)
        {
            string sql = string.Format(@"SELECT Parent.NAME,
       Parent.attendancerankid,
       Parent.workbegintime,
       Parent.workendtime,
       Parent.workhours,
       Parent.jobhours,
       Parent.isrestrank,
       Parent.isbelongtobefore,
       maxtimes,
       maxtimesend,
       iscardonduty,
       restbegintime,
       restendtime,
       resthours,
       mintimesbegin,
       mintimes,
       iscardoffduty,
       lateoverlooktimes,
       islatehalfabsent,
       latehalfabsenttimes,
       islatefullabsent,
       islatefullabsent,
       notjobtime,
       latefullabsenttimes,
       leaveoverlooktimes,
       isleavehalfabsent,
       leavehalfabsenttimes,
       isleavefullabsent,
       leavefullabsenttimes,
       isotadvance,
       isotdelay,
       minottimes,
       singleottimes,
       otnotdinnertimes,
       otadvancemin,
       otdelaymin,
       otmaxmin,
       attendanceranktypeid,
       dinnertypeid,
       rankdistanceminutes,
       dinnerattendancetype,
       islateforstart,
       lateattendancetype,
       isearlyforstart,
       earlyattendancetype,
       attendancerankrestid,
       otaduitbegin,
       otbegintime,
       otbeginminamount,
       attendancerankrest.code,
       Parent.issalesrank,
       Parent.isovertimeid
FROM   attendancerankrest
       LEFT JOIN attendancerank parent
              ON attendancerankrest.attendancerankid = parent.attendancerankid
WHERE  parent.flag = 1
	and parent.AttendanceRankId='{0}' ", pRankId);
            DataTable dt = HRHelper.ExecuteDataTable(sql);
            return dt;
        }

        public string GetRankCodeById(string pRankId)
        {
            #region 参数检查
            if (pRankId.CheckNullOrEmpty())
            {
                throw new ArgumentNullException("pRankId Error");
            }
            #endregion

            DataTable dt = HRHelper.ExecuteDataTable(string.Format("select Code from AttendanceRank where AttendanceRankid='{0}'", pRankId));

            if (dt != null && dt.Rows.Count > 0)
            {

                return dt.Rows[0][0].ToString();
            }

            return string.Empty;

        }



        public AttendanceRank GetAttendanceRank(string rankId)
        {
            DataTable dtRank = HRHelper.ExecuteDataTable(string.Format("select * from AttendanceRank where AttendanceRankId='{0}'", rankId));
            List<AttendanceRank> myObjects = HRHelper.DataTableToList<AttendanceRank>(dtRank);
            AttendanceRank rank = myObjects[0];

            DataTable dtRankRest = HRHelper.ExecuteDataTable(string.Format("select * from AttendanceRankRest where AttendanceRankId='{0}'", rankId));
            List<AttendanceRankRest> myRestObjects = HRHelper.DataTableToList<AttendanceRankRest>(dtRankRest);
            foreach (AttendanceRankRest info in myRestObjects)
            {
                rank.Rests.Add(info);
            }

            return rank;
        }
    }
}
