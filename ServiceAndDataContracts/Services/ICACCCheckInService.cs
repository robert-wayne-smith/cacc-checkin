using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace ServiceAndDataContracts
{
    [ServiceContract(Namespace="http://churchatcanyoncreek.com")]
    public interface ICACCCheckInService
    {
        #region ClassRole Operations

        #endregion ClassRole Operations

        #region FamilyRole Operations

        [OperationContract]
        List<string> GetAllFromFamilyRole();

        #endregion FamilyRole Operations

        #region Department Operations

        [OperationContract]
        List<CACCCheckInDb.Department> GetAllFromDepartment();

        [OperationContract]
        CACCCheckInDb.Department InsertDepartment(CACCCheckInDb.Department department);

        [OperationContract]
        CACCCheckInDb.Department UpdateDepartment(CACCCheckInDb.Department department);

        [OperationContract]
        void DeleteDepartment(CACCCheckInDb.Department department);

        #endregion Department Operations

        #region Attendance Operations

        [OperationContract]
        List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByDepartmentAndDateRange(Guid departmentId,
            DateTime attendanceStartDate, DateTime attendanceEndDate);

        [OperationContract]
        List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByPersonId(Guid personId);

        [OperationContract]
        List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByPersonIdAndDate(Guid personId, DateTime attendanceDate);

        //[OperationContract]
        //List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByClassIdAndDate(Guid classId, DateTime attendanceDate);

        [OperationContract]
        List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByDepartmentAndClassIdAndDateRange(
            Guid departmentId, Guid classId,
            DateTime attendanceStartDate, DateTime attendanceEndDate);

        [OperationContract]
        List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByFamilyIdAndDate(Guid familyId, DateTime attendanceDate);

        [OperationContract]
        List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByDeptId(Guid departmentId);

        [OperationContract]
        CACCCheckInDb.Attendance InsertAttendance(CACCCheckInDb.Attendance record);

        [OperationContract]
        CACCCheckInDb.Attendance UpdateAttendance(CACCCheckInDb.Attendance updatedRecord);

        [OperationContract]
        void DeleteAttendance(CACCCheckInDb.Attendance record);

        #endregion Attendance Operations

        #region Class Operations

        //[OperationContract]
        //List<CACCCheckInDb.Class> GetAllFromClass();

        [OperationContract]
        List<CACCCheckInDb.Class> GetClassesByDeptId(Nullable<Guid> departmentId);

        [OperationContract]
        List<CACCCheckInDb.Class> GetClassesByDeptName(string departmentName);

        [OperationContract]
        CACCCheckInDb.Class InsertClass(CACCCheckInDb.Class theClass);

        [OperationContract]
        CACCCheckInDb.Class UpdateClass(CACCCheckInDb.Class updatedClass);

        [OperationContract]
        void DeleteClass(CACCCheckInDb.Class theClass);

        #endregion Class Operations

        #region People Operations

        [OperationContract]
        List<CACCCheckInDb.Person> GetAllFromPeople();

        [OperationContract]
        List<CACCCheckInDb.Person> GetPeopleByFamilyId(Guid familyId);

        [OperationContract]
        List<CACCCheckInDb.Person> GetPeopleByDeptIdAndClassId(Guid departmentId, Guid classId);

        [OperationContract]
        CACCCheckInDb.Person InsertPeople(CACCCheckInDb.Person person);

        [OperationContract]
        CACCCheckInDb.Person UpdatePeople(CACCCheckInDb.Person updatedPerson);

        [OperationContract]
        void DeletePeople(CACCCheckInDb.Person person);

        #endregion People Operations

        #region ClassMember Operations

        //[OperationContract]
        //List<CACCCheckInDb.ClassMember> GetAllFromClassMember();

        //[OperationContract]
        //List<CACCCheckInDb.ClassMember> GetClassMembersByClassId(Guid classId);

        //[OperationContract]
        //List<CACCCheckInDb.ClassMember> GetClassMembersByClassIdAndClassRole(Guid classId,
        //    CACCCheckInDb.ClassRole role);

        //[OperationContract]
        //List<CACCCheckInDb.ClassMember> GetClassMembersByClassRole(CACCCheckInDb.ClassRole role);

        //[OperationContract]
        //List<CACCCheckInDb.ClassMember> GetClassMembersByDepartmentId(Guid departmentId);

        //[OperationContract]
        //List<CACCCheckInDb.ClassMember> GetClassMembersByDepartmentIdAndClassId(Guid departmentId,
        //    Guid classId);

        [OperationContract]
        CACCCheckInDb.ClassMember InsertClassMember(CACCCheckInDb.ClassMember classMember);

        //[OperationContract]
        //CACCCheckInDb.ClassMember UpdateClassMember(CACCCheckInDb.ClassMember updatedClassMember);

        [OperationContract]
        void DeleteClassMember(CACCCheckInDb.ClassMember classMember);

        #endregion ClassMember Operations

        #region PeopleWithDepartmentAndClassView Operations

        [OperationContract]
        List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetPeopleWithDepartmentAndClass();
        
        [OperationContract]
        List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetPeopleWithDepartmentAndClassByDepartment(
            string departmentName);

        [OperationContract]
        List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetPeopleWithDepartmentAndClassByDepartmentAndClass(
            string departmentName, string className);

        [OperationContract]
        List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetFamilyMembersByFamilyId(Guid familyId);

        [OperationContract]
        List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetAllFamiliesInDepartment(string departmentName);

        #endregion PeopleWithDepartmentAndClassView Operations

        #region SecurityCode Operations

        [OperationContract]
        int GetNewSecurityCodeForToday();

        //[OperationContract]
        //bool CheckSecurityCodeForDate(DateTime date, int SecurityCode);

        #endregion SecurityCode Operations
    }
}
