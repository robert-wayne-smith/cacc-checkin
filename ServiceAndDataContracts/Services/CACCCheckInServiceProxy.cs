using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using log4net;

namespace ServiceAndDataContracts
{
    public class CACCCheckInServiceProxy : ClientBase<ICACCCheckInService>, ICACCCheckInService, IDisposable
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region ICACCCheckInService Members

        public List<string> GetAllFromFamilyRole()
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetAllFromFamilyRole();
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetAllFromFamilyRole took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.Department> GetAllFromDepartment()
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetAllFromDepartment();
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetAllFromDepartment took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public CACCCheckInDb.Department InsertDepartment(CACCCheckInDb.Department department)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.InsertDepartment(department);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.InsertDepartment took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public CACCCheckInDb.Department UpdateDepartment(CACCCheckInDb.Department department)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.UpdateDepartment(department);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.UpdateDepartment took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public void DeleteDepartment(CACCCheckInDb.Department department)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                Channel.DeleteDepartment(department);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.DeleteDepartment took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByDepartmentAndDateRange(Guid departmentId,
            DateTime attendanceStartDate, DateTime attendanceEndDate)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetAttendanceByDepartmentAndDateRange(departmentId, attendanceStartDate,
                    attendanceEndDate);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetAttendanceByDateRange took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByPersonId(Guid personId)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetAttendanceByPersonId(personId);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetAttendanceByPersonId took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByPersonIdAndDate(Guid personId, DateTime attendanceDate)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetAttendanceByPersonIdAndDate(personId, attendanceDate);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetAttendanceByPersonIdAndDate took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        //public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByClassIdAndDate(Guid classId, DateTime attendanceDate)
        //{
        //    Stopwatch sw = new Stopwatch();
        //    try
        //    {
        //        sw.Start();
        //        return Channel.GetAttendanceByClassIdAndDate(classId, attendanceDate);
        //    }
        //    finally
        //    {
        //        sw.Stop();
        //        logger.DebugFormat("Call to CACCCheckInService.GetAttendanceByClassIdAndDate took [{0}] ms",
        //            sw.ElapsedMilliseconds);
        //    }
        //}

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByDepartmentAndClassIdAndDateRange(
            Guid departmentId, Guid classId,
            DateTime attendanceStartDate, DateTime attendanceEndDate)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetAttendanceByDepartmentAndClassIdAndDateRange(departmentId, classId,
                    attendanceStartDate, attendanceEndDate);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetAttendanceByClassIdAndDateRange took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByFamilyIdAndDate(Guid familyId, DateTime attendanceDate)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetAttendanceByFamilyIdAndDate(familyId, attendanceDate);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetAttendanceByFamilyIdAndDate took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByDeptId(Guid departmentId)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetAttendanceByDeptId(departmentId);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetAttendanceByDeptId took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public CACCCheckInDb.Attendance InsertAttendance(CACCCheckInDb.Attendance record)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.InsertAttendance(record);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.InsertAttendance took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public CACCCheckInDb.Attendance UpdateAttendance(CACCCheckInDb.Attendance updatedRecord)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.UpdateAttendance(updatedRecord);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.UpdateAttendance took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public void DeleteAttendance(CACCCheckInDb.Attendance record)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                Channel.DeleteAttendance(record);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.DeleteAttendance took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        //public List<CACCCheckInDb.Class> GetAllFromClass()
        //{
        //    Stopwatch sw = new Stopwatch();
        //    try
        //    {
        //        sw.Start();
        //        return Channel.GetAllFromClass();
        //    }
        //    finally
        //    {
        //        sw.Stop();
        //        logger.DebugFormat("Call to CACCCheckInService.GetAllFromClass took [{0}] ms",
        //            sw.ElapsedMilliseconds);
        //    }
        //}

        public List<CACCCheckInDb.Class> GetClassesByDeptId(Nullable<Guid> departmentId)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetClassesByDeptId(departmentId);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetClassesByDeptId took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.Class> GetClassesByDeptName(string departmentName)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetClassesByDeptName(departmentName);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetClassesByDeptName took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public CACCCheckInDb.Class InsertClass(CACCCheckInDb.Class theClass)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.InsertClass(theClass);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.InsertClass took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public CACCCheckInDb.Class UpdateClass(CACCCheckInDb.Class updatedClass)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.UpdateClass(updatedClass);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.UpdateClass took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public void DeleteClass(CACCCheckInDb.Class theClass)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                Channel.DeleteClass(theClass);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.DeleteClass took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.Person> GetAllFromPeople()
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetAllFromPeople();
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetAllFromPeople took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.Person> GetPeopleByFamilyId(Guid familyId)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetPeopleByFamilyId(familyId);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetPeopleByFamilyId took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetAllFamiliesInDepartment(string departmentName)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetAllFamiliesInDepartment(departmentName);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetAllFamiliesInDepartment took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.Person> GetPeopleByDeptIdAndClassId(Guid departmentId, Guid classId)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetPeopleByDeptIdAndClassId(departmentId, classId);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetPeopleByDeptIdAndClassId took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public CACCCheckInDb.Person InsertPeople(CACCCheckInDb.Person person)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.InsertPeople(person);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.InsertPeople took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public CACCCheckInDb.Person UpdatePeople(CACCCheckInDb.Person updatedPerson)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.UpdatePeople(updatedPerson);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.UpdatePeople took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public void DeletePeople(CACCCheckInDb.Person person)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                Channel.DeletePeople(person);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.DeletePeople took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        //public List<CACCCheckInDb.ClassMember> GetAllFromClassMember()
        //{
        //    Stopwatch sw = new Stopwatch();
        //    try
        //    {
        //        sw.Start();
        //        return Channel.GetAllFromClassMember();
        //    }
        //    finally
        //    {
        //        sw.Stop();
        //        logger.DebugFormat("Call to CACCCheckInService.GetAllFromClassMember took [{0}] ms",
        //            sw.ElapsedMilliseconds);
        //    }
        //}

        //public List<CACCCheckInDb.ClassMember> GetClassMembersByClassId(Guid classId)
        //{
        //    Stopwatch sw = new Stopwatch();
        //    try
        //    {
        //        sw.Start();
        //        return Channel.GetClassMembersByClassId(classId);
        //    }
        //    finally
        //    {
        //        sw.Stop();
        //        logger.DebugFormat("Call to CACCCheckInService.GetClassMembersByClassId took [{0}] ms",
        //            sw.ElapsedMilliseconds);
        //    }
        //}

        //public List<CACCCheckInDb.ClassMember> GetClassMembersByClassIdAndClassRole(Guid classId, CACCCheckInDb.ClassRole role)
        //{
        //    Stopwatch sw = new Stopwatch();
        //    try
        //    {
        //        sw.Start();
        //        return Channel.GetClassMembersByClassIdAndClassRole(classId, role);
        //    }
        //    finally
        //    {
        //        sw.Stop();
        //        logger.DebugFormat("Call to CACCCheckInService.GetClassMembersByClassIdAndClassRole took [{0}] ms",
        //            sw.ElapsedMilliseconds);
        //    }
        //}

        //public List<CACCCheckInDb.ClassMember> GetClassMembersByClassRole(CACCCheckInDb.ClassRole role)
        //{
        //    Stopwatch sw = new Stopwatch();
        //    try
        //    {
        //        sw.Start();
        //        return Channel.GetClassMembersByClassRole(role);
        //    }
        //    finally
        //    {
        //        sw.Stop();
        //        logger.DebugFormat("Call to CACCCheckInService.GetClassMembersByClassRole took [{0}] ms",
        //            sw.ElapsedMilliseconds);
        //    }
        //}

        //public List<CACCCheckInDb.ClassMember> GetClassMembersByDepartmentId(Guid departmentId)
        //{
        //    Stopwatch sw = new Stopwatch();
        //    try
        //    {
        //        sw.Start();
        //        return Channel.GetClassMembersByDepartmentId(departmentId);
        //    }
        //    finally
        //    {
        //        sw.Stop();
        //        logger.DebugFormat("Call to CACCCheckInService.GetClassMembersByDepartmentId took [{0}] ms",
        //            sw.ElapsedMilliseconds);
        //    }
        //}

        //public List<CACCCheckInDb.ClassMember> GetClassMembersByDepartmentIdAndClassId(Guid departmentId, Guid classId)
        //{
        //    Stopwatch sw = new Stopwatch();
        //    try
        //    {
        //        sw.Start();
        //        return Channel.GetClassMembersByDepartmentIdAndClassId(departmentId, classId);
        //    }
        //    finally
        //    {
        //        sw.Stop();
        //        logger.DebugFormat("Call to CACCCheckInService.GetClassMembersByDepartmentIdAndClassId took [{0}] ms",
        //            sw.ElapsedMilliseconds);
        //    }
        //}

        public CACCCheckInDb.ClassMember InsertClassMember(CACCCheckInDb.ClassMember classMember)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.InsertClassMember(classMember);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.InsertClassMember took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        //public CACCCheckInDb.ClassMember UpdateClassMember(CACCCheckInDb.ClassMember updatedClassMember)
        //{
        //    Stopwatch sw = new Stopwatch();
        //    try
        //    {
        //        sw.Start();
        //        return Channel.UpdateClassMember(updatedClassMember);
        //    }
        //    finally
        //    {
        //        sw.Stop();
        //        logger.DebugFormat("Call to CACCCheckInService.UpdateClassMember took [{0}] ms",
        //            sw.ElapsedMilliseconds);
        //    }
        //}

        public void DeleteClassMember(CACCCheckInDb.ClassMember classMember)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                Channel.DeleteClassMember(classMember);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.DeleteClassMember took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }
        
        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetPeopleWithDepartmentAndClass()
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetPeopleWithDepartmentAndClass();
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetPeopleWithDepartmentAndClass took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetPeopleWithDepartmentAndClassByDepartment(
            string departmentName)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetPeopleWithDepartmentAndClassByDepartment(departmentName);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetPeopleWithDepartmentAndClassByDepartment took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetPeopleWithDepartmentAndClassByDepartmentAndClass(
            string departmentName, string className)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetPeopleWithDepartmentAndClassByDepartmentAndClass(departmentName, className);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetPeopleWithDepartmentAndClassByDepartmentAndClass took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetFamilyMembersByFamilyId(Guid familyId)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetFamilyMembersByFamilyId(familyId);
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetFamilyMembersByFamilyId took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        public int GetNewSecurityCodeForToday()
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                return Channel.GetNewSecurityCodeForToday();
            }
            finally
            {
                sw.Stop();
                logger.DebugFormat("Call to CACCCheckInService.GetNewSecurityCodeForToday took [{0}] ms",
                    sw.ElapsedMilliseconds);
            }
        }

        //public bool CheckSecurityCodeForDate(DateTime date, int securityCode)
        //{
        //    Stopwatch sw = new Stopwatch();
        //    try
        //    {
        //        sw.Start();
        //        return Channel.CheckSecurityCodeForDate(date, securityCode);
        //    }
        //    finally
        //    {
        //        sw.Stop();
        //        logger.DebugFormat("Call to CACCCheckInService.CheckSecurityCodeForDate took [{0}] ms",
        //            sw.ElapsedMilliseconds);
        //    }
        //}
        
        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (this.State == CommunicationState.Faulted)
            {
                this.Abort();
            }
            else
            {
                this.Close();
            }
        }

        #endregion
    }
}
