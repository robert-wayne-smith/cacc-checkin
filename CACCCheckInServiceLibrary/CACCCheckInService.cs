using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;

namespace CACCCheckInServiceLibrary
{
    public class CACCCheckInService : ServiceAndDataContracts.ICACCCheckInService
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Log4NetWriter logwriter = new Log4NetWriter(MethodBase.GetCurrentMethod().DeclaringType);
        
        #region ICACCCheckInService Members

        #region ClassRole Operations
        
        #endregion ClassRole Operations

        #region FamilyRole Operations

        /// <summary>
        /// Retrieves all FamilyRoles from the database
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllFromFamilyRole()
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.Debug("Querying CACCCheckIn DB for ALL FamilyRole records.");
            List<string> familyRoles = (from role in db.FamilyRoles
                select role.Role).OrderBy(r => r).ToList();
            
            return familyRoles;
        }

        #endregion FamilyRole Operations

        #region Department Operations

        /// <summary>
        /// Gets all the departments from the database
        /// </summary>
        /// <returns></returns>
        public List<CACCCheckInDb.Department> GetAllFromDepartment()
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.Debug("Querying CACCCheckIn DB for ALL Department records.");
            List<CACCCheckInDb.Department> departments = (from dpt in db.Departments
                                                          select dpt).ToList();

            return departments;
        }

        /// <summary>
        /// Inserts a new department into database
        /// </summary>
        /// <param name="department"></param>
        /// <returns></returns>
        public CACCCheckInDb.Department InsertDepartment(CACCCheckInDb.Department department)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.DebugFormat("Inserting Department: [{0}]", department.Name);
            db.Departments.InsertOnSubmit(department);

            db.SubmitChanges();

            return db.Departments.Where(dpt =>
                dpt.Id.Equals(department.Id)).Single<CACCCheckInDb.Department>();
        }

        /// <summary>
        /// Updates a department in the database
        /// </summary>
        /// <param name="updatedDepartment"></param>
        /// <returns></returns>
        public CACCCheckInDb.Department UpdateDepartment(CACCCheckInDb.Department updatedDepartment)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.DebugFormat("Querying for Department: [{0}]", updatedDepartment.Name);
            CACCCheckInDb.Department currentDepartment = (from dpt in db.Departments
                                            where dpt.Id == updatedDepartment.Id
                                            select dpt).Single<CACCCheckInDb.Department>();

            logger.DebugFormat("Updating Department: [{0}][{1}]",
                updatedDepartment.Name, updatedDepartment.Description);
            currentDepartment.Name = updatedDepartment.Name;
            currentDepartment.Description = updatedDepartment.Description;

            db.SubmitChanges();

            return (from dpt in db.Departments
                    where dpt.Id.Equals(updatedDepartment.Id)
                    select dpt).Single<CACCCheckInDb.Department>();
        }

        /// <summary>
        /// Deletes the specified department from database
        /// </summary>
        /// <param name="department"></param>
        public void DeleteDepartment(CACCCheckInDb.Department department)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;
            
            try
            {
                logger.DebugFormat("Attaching Department: [{0}] to DataContext", department.Name);
                db.Departments.Attach(department);
                logger.DebugFormat("Deleting Department: [{0}]", department.Name);
                db.Departments.DeleteOnSubmit(department);

                db.SubmitChanges();
            }
            catch (ChangeConflictException ex)
            {
                logger.Error("ChangeConflictException:", ex);
                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                {
                    occ.Resolve(RefreshMode.OverwriteCurrentValues);
                }
            }
        }

        #endregion Department Operations

        #region Attendance Operations

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByDepartmentAndDateRange(Guid departmentId, 
            DateTime attendanceStartDate, DateTime attendanceEndDate)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            logger.DebugFormat("CACCCheckIn DB connection string: [{0}].", db.Connection.ConnectionString);            
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB for Attendance records in department [{0}] between start date: [{1}] and end date: [{2}].",
                departmentId.ToString("B"), attendanceStartDate.ToShortDateString(), attendanceEndDate.ToShortDateString());
            List<CACCCheckInDb.AttendanceWithDetail> records = (from r in db.AttendanceWithDetails
                                                                where r.DeptId.Equals(departmentId)
                                                                where (r.Date >= attendanceStartDate &&
                                                                    r.Date <= attendanceEndDate)
                                                                select r).ToList();

            return records;
        }

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByPersonId(Guid personId)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            logger.DebugFormat("CACCCheckIn DB connection string: [{0}].", db.Connection.ConnectionString);
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB for Attendance records for PersonId: [{0}].",
                personId.ToString("B"));
            List<CACCCheckInDb.AttendanceWithDetail> records = (from r in db.AttendanceWithDetails
                                                                where r.PersonId.Equals(personId)
                                                                select r).ToList();

            return records;
        }

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByPersonIdAndDate(Guid personId,
            DateTime attendanceDate)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            logger.DebugFormat("CACCCheckIn DB connection string: [{0}].", db.Connection.ConnectionString);
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB for Attendance records for PersonId: [{0}] and date: [{1}].",
                personId.ToString("B"), attendanceDate.ToShortDateString());
            List<CACCCheckInDb.AttendanceWithDetail> records = (from r in db.AttendanceWithDetails
                                                                where r.PersonId.Equals(personId) &
                                                                    r.Date.Equals(attendanceDate)
                                                                select r).ToList();

            return records;
        }

        //public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByClassIdAndDate(Guid classId,
        //    DateTime attendanceDate)
        //{
        //    logger.Debug("Opening DataContext to CACCCheckIn DB.");
        //    CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
        //    //logger.DebugFormat("CACCCheckIn DB connection string: [{0}].", db.Connection.ConnectionString);
        //    db.Log = logwriter;

        //    logger.DebugFormat("Querying CACCCheckIn DB for Attendance records for ClassId: [{0}] and date: [{1}].",
        //        classId.ToString("B"), attendanceDate.ToShortDateString());
        //    List<CACCCheckInDb.AttendanceWithDetail> records = (from r in db.AttendanceWithDetails
        //                                                        where r.ClassId.Equals(classId) &
        //                                                              r.Date.Equals(attendanceDate)
        //                                                        select r).ToList();

        //    return records; 
        //}

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByDepartmentAndClassIdAndDateRange(
            Guid departmentId, Guid classId,
            DateTime attendanceStartDate, DateTime attendanceEndDate)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            logger.DebugFormat("CACCCheckIn DB connection string: [{0}].", db.Connection.ConnectionString);
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB for Attendance records in DepartmentId: [{0}] ClassId: [{1}] between start date: [{2}] and end date: [{3}].",
                departmentId.ToString("B"), classId.ToString("B"), attendanceStartDate.ToShortDateString(), attendanceEndDate.ToShortDateString());
            List<CACCCheckInDb.AttendanceWithDetail> records = (from r in db.AttendanceWithDetails
                                                                where r.DeptId.Equals(departmentId)
                                                                where r.ClassId.Equals(classId)
                                                                where (r.Date >= attendanceStartDate &&
                                                                    r.Date <= attendanceEndDate)
                                                                select r).ToList();

            return records; 
        }

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByFamilyIdAndDate(Guid familyId,
            DateTime attendanceDate)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            logger.DebugFormat("CACCCheckIn DB connection string: [{0}].", db.Connection.ConnectionString);
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB for Attendance records for FamilyId: [{0}] and date: [{1}].",
                familyId.ToString("B"), attendanceDate.ToShortDateString());
            List<CACCCheckInDb.AttendanceWithDetail> records = (from r in db.AttendanceWithDetails
                                                         where r.FamilyId.Equals(familyId) &
                                                               r.Date.Equals(attendanceDate)
                                                         select r).ToList();

            return records; 
        }

        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceByDeptId(Guid departmentId)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            logger.DebugFormat("CACCCheckIn DB connection string: [{0}].", db.Connection.ConnectionString);
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB for Attendance records for DeptId: [{0}].",
                departmentId.ToString("B"));
            List<CACCCheckInDb.AttendanceWithDetail> records = (from r in db.AttendanceWithDetails
                                                                where r.DeptId.Equals(departmentId)
                                                                where r.Date >= DateTime.Now.Subtract(TimeSpan.FromDays(180)).Date
                                                                orderby r.Date descending
                                                                select r).ToList();

            return records;
        }

        public CACCCheckInDb.Attendance InsertAttendance(CACCCheckInDb.Attendance record)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            logger.DebugFormat("CACCCheckIn DB connection string: [{0}].", db.Connection.ConnectionString);
            db.Log = logwriter;

            logger.DebugFormat("Inserting Attendance for PersonId: [{0}]", record.PersonId.ToString("B"));
            db.Attendances.InsertOnSubmit(record);

            db.SubmitChanges();

            return db.Attendances.Where(r =>
                r.Date.Equals(record.Date) &&
                r.ClassId.Equals(record.ClassId) &&
                r.PersonId.Equals(record.PersonId)).Single<CACCCheckInDb.Attendance>();
        }

        public CACCCheckInDb.Attendance UpdateAttendance(CACCCheckInDb.Attendance updatedRecord)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            logger.DebugFormat("CACCCheckIn DB connection string: [{0}].", db.Connection.ConnectionString);
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB for Attendance with Date=[{0}], ClassId=[{1}] and PersonId=[{2}].",
                updatedRecord.Date.ToShortDateString(), updatedRecord.ClassId.ToString("B"), updatedRecord.PersonId.ToString("B"));
            CACCCheckInDb.Attendance currentRecord = (from r in db.Attendances
                                                      where r.Date.Equals(updatedRecord.Date) &&
                                                            r.ClassId.Equals(updatedRecord.ClassId) &&
                                                            r.PersonId.Equals(updatedRecord.PersonId)
                                                      select r).Single<CACCCheckInDb.Attendance>();

            logger.DebugFormat("Updating Attendance SecurityCode: [{0}]",
                updatedRecord.SecurityCode);
            currentRecord.SecurityCode = updatedRecord.SecurityCode;
            
            db.SubmitChanges();

            return db.Attendances.Where(r =>
                r.Date.Equals(updatedRecord.Date) &&
                r.ClassId.Equals(updatedRecord.ClassId) &&
                r.PersonId.Equals(updatedRecord.PersonId)).Single<CACCCheckInDb.Attendance>();
        }

        public void DeleteAttendance(CACCCheckInDb.Attendance record)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            logger.DebugFormat("CACCCheckIn DB connection string: [{0}].", db.Connection.ConnectionString);
            db.Log = logwriter;

            try
            {
                logger.DebugFormat("Querying CACCCheckIn DB for Attendance with Date=[{0}], ClassId=[{1}] and PersonId=[{2}].",
                    record.Date.ToShortDateString(), record.ClassId.ToString("B"), record.PersonId.ToString("B"));
                CACCCheckInDb.Attendance recordToDelete = (from r in db.Attendances
                                                          where r.Date.Equals(record.Date) &&
                                                                r.ClassId.Equals(record.ClassId) &&
                                                                r.PersonId.Equals(record.PersonId)
                                                          select r).Single<CACCCheckInDb.Attendance>();

                logger.DebugFormat("Deleting Attendance record for PersonId: [{0}]", record.PersonId);
                db.Attendances.DeleteOnSubmit(recordToDelete);

                db.SubmitChanges();
            }
            catch (ChangeConflictException ex)
            {
                logger.Error("ChangeConflictException:", ex);
                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                {
                    occ.Resolve(RefreshMode.OverwriteCurrentValues);
                }
            }
        }

        #endregion Attendance Operations

        #region Class Operations

        //public List<CACCCheckInDb.Class> GetAllFromClass()
        //{
        //    logger.Debug("Opening DataContext to CACCCheckIn DB.");
        //    CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
        //    db.Log = logwriter;

        //    //DataLoadOptions dlo = new DataLoadOptions();
        //    //dlo.LoadWith<CACCCheckInDb.Class>(r => r.ClassMembers);
        //    //dlo.LoadWith<CACCCheckInDb.Class>(r => r.Department);
        //    //db.LoadOptions = dlo;

        //    logger.Debug("Querying CACCCheckIn DB for ALL Class records.");
        //    List<CACCCheckInDb.Class> classes = (from c in db.Classes
        //                                              select c).ToList();

        //    return classes;
        //}
       
        public List<CACCCheckInDb.Class> GetClassesByDeptId(Nullable<Guid> departmentId)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            //DataLoadOptions dlo = new DataLoadOptions();
            //dlo.LoadWith<CACCCheckInDb.Class>(r => r.ClassMembers);
            //dlo.LoadWith<CACCCheckInDb.Class>(r => r.Department);
            //db.LoadOptions = dlo;

            List<CACCCheckInDb.Class> classes;

            if (departmentId.HasValue)
            {
                logger.DebugFormat("Querying CACCCheckIn DB for Classes with DeptId=[{0}].",
                    departmentId.Value.ToString("B"));
                classes = (from c in db.Classes
                             where c.DeptId.Equals(departmentId.Value)
                             select c).ToList();
            }
            else
            {
                logger.Debug("Querying CACCCheckIn DB for Classes with DeptId=[null].");
                classes = (from c in db.Classes
                           where c.DeptId.HasValue.Equals(false)
                           select c).ToList();
            }

            return classes;
        }

        public List<CACCCheckInDb.Class> GetClassesByDeptName(string departmentName)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            DataLoadOptions dlo = new DataLoadOptions();
            dlo.LoadWith<CACCCheckInDb.Class>(r => r.Department);
            db.LoadOptions = dlo;

            logger.DebugFormat("Querying CACCCheckIn DB for Classes with Department.Name=[{0}].",
                departmentName);
            List<CACCCheckInDb.Class> classes = (from c in db.Classes
                           where c.Department.Name.Equals(departmentName)
                           select c).ToList();

            return classes;
        }

        public CACCCheckInDb.Class InsertClass(CACCCheckInDb.Class theClass)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.DebugFormat("Inserting Class: [{0}]", theClass.Name);
            db.Classes.InsertOnSubmit(theClass);

            db.SubmitChanges();

            return db.Classes.Where(c =>
                c.Id.Equals(theClass.Id)).Single<CACCCheckInDb.Class>();
        }

        public CACCCheckInDb.Class UpdateClass(CACCCheckInDb.Class updatedClass)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            CACCCheckInDb.Class currentClass = (from c in db.Classes
                                                 where c.Id.Equals(updatedClass.Id)
                                                 select c).Single<CACCCheckInDb.Class>();

            logger.DebugFormat("Updating Class: [{0}]",
                updatedClass.Name);
            currentClass.DeptId = updatedClass.DeptId;
            currentClass.Name = updatedClass.Name;
            currentClass.Description = updatedClass.Description;

            db.SubmitChanges();

            return db.Classes.Where(c =>
                c.Id.Equals(updatedClass.Id)).Single<CACCCheckInDb.Class>();
        }

        public void DeleteClass(CACCCheckInDb.Class theClass)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            try
            {
                logger.DebugFormat("Attaching Class: [{0}] to DataContext", theClass.Name);
                db.Classes.Attach(theClass);
                logger.DebugFormat("Deleting Class: [{0}]", theClass.Name);
                db.Classes.DeleteOnSubmit(theClass);

                db.SubmitChanges();
            }
            catch (ChangeConflictException ex)
            {
                logger.Error("ChangeConflictException:", ex);
                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                {
                    occ.Resolve(RefreshMode.OverwriteCurrentValues);
                }
            }
        }

        #endregion Class Operations

        #region People Operations

        public List<CACCCheckInDb.Person> GetAllFromPeople()
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.Debug("Querying CACCCheckIn DB for ALL Person records.");
            List<CACCCheckInDb.Person> people = (from p in db.Persons
                                                 select p).ToList();

            return people;
        }

        public List<CACCCheckInDb.Person> GetPeopleByFamilyId(Guid familyId)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB for People with FamilyId=[{0}].",
                familyId.ToString("B"));
            List<CACCCheckInDb.Person> people = (from p in db.Persons
                                                 where p.FamilyId.Equals(familyId)
                                                 select p).ToList();

            return people;
        }

        public List<CACCCheckInDb.Person> GetPeopleByDeptIdAndClassId(Guid departmentId, Guid classId)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB for People with DepartmentId=[{0}].",
                departmentId.ToString("B"));
            List<CACCCheckInDb.Person> people = (from cm in db.ClassMembers
                                                 join p in db.Persons on cm.PersonId equals p.Id
                                                 join c in db.Classes on cm.ClassId equals c.Id
                                                 join d in db.Departments on c.DeptId equals d.Id
                                                 where cm.ClassRole.Equals("Member")
                                                 where d.Id.Equals(departmentId)
                                                 where c.Id.Equals(classId)
                                                 select p).ToList();

            return people;
        }

        public CACCCheckInDb.Person InsertPeople(CACCCheckInDb.Person person)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.DebugFormat("Inserting Person: [{0} {1}]", person.FirstName,
                person.LastName);
            db.Persons.InsertOnSubmit(person);

            db.SubmitChanges();

            return db.Persons.Where(p =>
                p.Id.Equals(person.Id)).Single<CACCCheckInDb.Person>();
        }

        public CACCCheckInDb.Person UpdatePeople(CACCCheckInDb.Person updatedPerson)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB for Person with PersonId=[{0}].",
                updatedPerson.Id.ToString("B"));
            CACCCheckInDb.Person currentPerson = (from p in db.Persons
                                                 where p.Id.Equals(updatedPerson.Id)
                                                 select p).Single<CACCCheckInDb.Person>();

            logger.DebugFormat("Updating Person: [{0} {1}]",
                updatedPerson.FirstName, updatedPerson.LastName);
            currentPerson.FirstName = updatedPerson.FirstName;
            currentPerson.LastName = updatedPerson.LastName;
            currentPerson.PhoneNumber = updatedPerson.PhoneNumber;
            currentPerson.FamilyId = updatedPerson.FamilyId;
            currentPerson.FamilyRole = updatedPerson.FamilyRole;
            currentPerson.SpecialConditions = updatedPerson.SpecialConditions;

            db.SubmitChanges();

            return db.Persons.Where(p =>
                p.Id.Equals(updatedPerson.Id)).Single<CACCCheckInDb.Person>();
        }

        public void DeletePeople(CACCCheckInDb.Person person)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            try
            {
                logger.DebugFormat("Attaching Person: [{0} {1}] to DataContext",
                    person.FirstName, person.LastName);
                db.Persons.Attach(person);
                logger.DebugFormat("Deleting Person: [{0} {1}]",
                    person.FirstName, person.LastName);
                db.Persons.DeleteOnSubmit(person);

                db.SubmitChanges();
            }
            catch (ChangeConflictException ex)
            {
                logger.Error("ChangeConflictException:", ex);
                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                {
                    occ.Resolve(RefreshMode.OverwriteCurrentValues);
                }
            }
        }

        #endregion People Operations

        #region ClassMember Operations

        //public List<CACCCheckInDb.ClassMember> GetAllFromClassMember()
        //{
        //    logger.Debug("Opening DataContext to CACCCheckIn DB.");
        //    CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
        //    db.Log = logwriter;

        //    DataLoadOptions dlo = new DataLoadOptions();
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Class);
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Person);
        //    dlo.LoadWith<CACCCheckInDb.Class>(c => c.Department);
        //    db.LoadOptions = dlo;

        //    logger.Debug("Querying CACCCheckIn DB for ALL ClassMember records.");
        //    List<CACCCheckInDb.ClassMember> members = (from cm in db.ClassMembers
        //                                         select cm).ToList();

        //    return members;
        //}

        //public List<CACCCheckInDb.ClassMember> GetClassMembersByClassId(Guid classId)
        //{
        //    logger.Debug("Opening DataContext to CACCCheckIn DB.");
        //    CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
        //    db.Log = logwriter;

        //    DataLoadOptions dlo = new DataLoadOptions();
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Class);            
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Person);
        //    dlo.LoadWith<CACCCheckInDb.Class>(c => c.Department);
        //    db.LoadOptions = dlo;

        //    logger.DebugFormat("Querying CACCCheckIn DB for ClassMember with ClassId=[{0}].",
        //        classId.ToString("B"));
        //    List<CACCCheckInDb.ClassMember> members = (from cm in db.ClassMembers
        //                                               where cm.ClassId.Equals(classId)
        //                                               select cm).ToList();

        //    return members;
        //}

        //public List<CACCCheckInDb.ClassMember> GetClassMembersByClassIdAndClassRole(Guid classId,
        //    CACCCheckInDb.ClassRole role)
        //{
        //    logger.Debug("Opening DataContext to CACCCheckIn DB.");
        //    CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
        //    db.Log = logwriter;

        //    DataLoadOptions dlo = new DataLoadOptions();
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Class);
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Person);
        //    dlo.LoadWith<CACCCheckInDb.Class>(c => c.Department);
        //    db.LoadOptions = dlo;

        //    logger.DebugFormat("Querying CACCCheckIn DB for ClassMember with ClassId=[{0}] and ClassRole=[{1}].",
        //        classId.ToString("B"), role.Role);
        //    List<CACCCheckInDb.ClassMember> members = (from cm in db.ClassMembers
        //                                               where cm.ClassId.Equals(classId) &&
        //                                                    cm.ClassRole.Equals(role.Role)
        //                                               select cm).ToList();

        //    return members;
        //}

        //public List<CACCCheckInDb.ClassMember> GetClassMembersByClassRole(CACCCheckInDb.ClassRole role)
        //{
        //    logger.Debug("Opening DataContext to CACCCheckIn DB.");
        //    CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
        //    db.Log = logwriter;

        //    DataLoadOptions dlo = new DataLoadOptions();
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Class);
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Person);
        //    dlo.LoadWith<CACCCheckInDb.Class>(c => c.Department);
        //    db.LoadOptions = dlo;

        //    logger.DebugFormat("Querying CACCCheckIn DB for ClassMember with ClassRole=[{0}].",
        //        role.Role);
        //    List<CACCCheckInDb.ClassMember> members = (from cm in db.ClassMembers
        //                                               where cm.ClassRole.Equals(role.Role)
        //                                               select cm).ToList();

        //    return members;
        //}

        //public List<CACCCheckInDb.ClassMember> GetClassMembersByDepartmentId(Guid departmentId)
        //{
        //    logger.Debug("Opening DataContext to CACCCheckIn DB.");
        //    CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
        //    db.Log = logwriter;

        //    DataLoadOptions dlo = new DataLoadOptions();
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Class);
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Person);
        //    dlo.LoadWith<CACCCheckInDb.Class>(c => c.Department);
        //    db.LoadOptions = dlo;

        //    logger.DebugFormat("Querying CACCCheckIn DB for ClassMember with DepartmentId=[{0}].",
        //        departmentId.ToString("B"));
        //    List<CACCCheckInDb.ClassMember> members = (from cm in db.ClassMembers
        //                                               where cm.Class.DeptId.Equals(departmentId)
        //                                               select cm).ToList();

        //    return members;
        //}

        //public List<CACCCheckInDb.ClassMember> GetClassMembersByDepartmentIdAndClassId(Guid departmentId,
        //    Guid classId)
        //{
        //    logger.Debug("Opening DataContext to CACCCheckIn DB.");
        //    CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
        //    db.Log = logwriter;

        //    DataLoadOptions dlo = new DataLoadOptions();
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Class);
        //    dlo.LoadWith<CACCCheckInDb.ClassMember>(cm => cm.Person);
        //    dlo.LoadWith<CACCCheckInDb.Class>(c => c.Department);
        //    db.LoadOptions = dlo;

        //    logger.DebugFormat("Querying CACCCheckIn DB for ClassMember with DepartmentId=[{0}] and ClassId=[{1}].",
        //        departmentId.ToString("B"), classId.ToString("B"));
        //    List<CACCCheckInDb.ClassMember> members = (from cm in db.ClassMembers
        //                                               where cm.ClassId.Equals(classId) && 
        //                                                    cm.Class.DeptId.Equals(departmentId)
        //                                               select cm).ToList();

        //    return members;
        //}

        public CACCCheckInDb.ClassMember InsertClassMember(CACCCheckInDb.ClassMember classMember)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.DebugFormat("Inserting ClassMember with PersonId=[{0}].",
                classMember.PersonId.ToString("B"));
            db.ClassMembers.InsertOnSubmit(classMember);

            db.SubmitChanges();

            return db.ClassMembers.Where(cm =>
                cm.ClassId.Equals(classMember.ClassId) &&
                cm.PersonId.Equals(classMember.PersonId)).Single<CACCCheckInDb.ClassMember>();
        }

        //public CACCCheckInDb.ClassMember UpdateClassMember(CACCCheckInDb.ClassMember updatedClassMember)
        //{
        //    logger.Debug("Opening DataContext to CACCCheckIn DB.");
        //    CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
        //    db.Log = logwriter;

        //    logger.DebugFormat("Querying CACCCheckIn DB for ClassMember with ClassId=[{0}] and PersonId=[{1}].",
        //        updatedClassMember.ClassId.ToString("B"), updatedClassMember.PersonId.ToString("B"));
        //    CACCCheckInDb.ClassMember currentClassMember = (from cm in db.ClassMembers
        //                                               where cm.ClassId.Equals(updatedClassMember.ClassId) &&
        //                                                     cm.PersonId.Equals(updatedClassMember.PersonId)
        //                                               select cm).SingleOrDefault<CACCCheckInDb.ClassMember>();

        //    logger.Debug("Updating ClassMember.");
        //    currentClassMember.ClassRole = updatedClassMember.ClassRole;
            
        //    db.SubmitChanges();

        //    return db.ClassMembers.Where(cm =>
        //        cm.ClassId.Equals(updatedClassMember.ClassId) &&
        //        cm.PersonId.Equals(updatedClassMember.PersonId)).Single<CACCCheckInDb.ClassMember>();
        //}

        public void DeleteClassMember(CACCCheckInDb.ClassMember classMember)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            try
            {
                logger.DebugFormat("Querying CACCCheckIn DB for ClassMember with ClassId=[{0}] and PersonId=[{1}].",
                    classMember.ClassId.ToString("B"), classMember.PersonId.ToString("B"));
                CACCCheckInDb.ClassMember classMemberToDelete = (from cm in db.ClassMembers
                                                        where cm.ClassId.Equals(classMember.ClassId) &&
                                                              cm.PersonId.Equals(classMember.PersonId)
                                                        select cm).Single<CACCCheckInDb.ClassMember>();

                logger.Debug("Deleting ClassMember.");
                db.ClassMembers.DeleteOnSubmit(classMemberToDelete);
                db.SubmitChanges();
            }
            catch (ChangeConflictException ex)
            {
                logger.Error("ChangeConflictException:", ex);
                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                {
                    occ.Resolve(RefreshMode.OverwriteCurrentValues);
                }
            } 
        }

        #endregion ClassMember Operations

        #region PeopleWithDepartmentAndClassView Operations

        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetPeopleWithDepartmentAndClass()
        {
            return GetPeopleWithDepartmentAndClassByDepartmentAndClass(null, null);
        }

        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetPeopleWithDepartmentAndClassByDepartment(
            string departmentName)
        {
            return GetPeopleWithDepartmentAndClassByDepartmentAndClass(departmentName, null);
        }

        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetPeopleWithDepartmentAndClassByDepartmentAndClass(
            string departmentName, string className)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            StringBuilder sb = new StringBuilder("Querying CACCCheckIn DB PeopleWithDepartmentAndClassView.");
            var query = from r in db.PeopleWithDepartmentAndClassViews select r;

            if (!String.IsNullOrEmpty(departmentName))
            {
                sb.AppendFormat(" Department=[{0}]", departmentName);
                query = from r in query where r.DepartmentName.Equals(departmentName) select r;
            }
            
            if (!String.IsNullOrEmpty(className))
            {
                sb.AppendFormat(" Class=[{0}]", className);
                query = from r in query where r.ClassName.Equals(className) select r;
            }

            sb.Append(". Ordering by LastName and FirstName and returning list.");
            query = from r in query orderby r.LastName, r.FirstName select r;

            List<CACCCheckInDb.PeopleWithDepartmentAndClassView> records = query.ToList();
            
            logger.Debug(sb.ToString());
            return records;
        }

        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetFamilyMembersByFamilyId(Guid familyId)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB PeopleWithDepartmentAndClassView for all people with FamilyId=[{0}] and returning list.",
                familyId.ToString("B"));
            var people = from p in db.PeopleWithDepartmentAndClassViews
                where p.FamilyId.Equals(familyId)
                select p;

            return people.ToList();
        }

        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> GetAllFamiliesInDepartment(string departmentName)
        {
            logger.Debug("Opening DataContext to CACCCheckIn DB.");
            CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
            db.Log = logwriter;

            logger.DebugFormat("Querying CACCCheckIn DB for all People assigned to FamilyId in Department.");
            var familyIdsInDepartment = (from p in db.PeopleWithDepartmentAndClassViews
                                        where p.FamilyId.HasValue
                                        where p.DepartmentName.Equals(departmentName)
                                        select p.FamilyId).Distinct();

            var people = from p in db.PeopleWithDepartmentAndClassViews
                         where p.DepartmentName.Equals(departmentName) || p.DepartmentName.Equals("Adult")
                         where familyIdsInDepartment.Contains(p.FamilyId.Value) 
                         select p;


            return people.ToList();
        }

        #endregion PeopleWithDepartmentAndClassView Operations

        #region SecurityCode Operations

        private object securityCodeLock = new object();

        /// <summary>
        /// This function handles getting new security codes as people check in.
        /// At the first check in for a day, it will reset the security code counter.
        /// </summary>
        /// <returns></returns>
        public int GetNewSecurityCodeForToday()
        {
            lock (securityCodeLock)
            {
                logger.Debug("Opening DataContext to CACCCheckIn DB.");
                CACCCheckInDb.CACCCheckInDbDataContext db = new CACCCheckInDb.CACCCheckInDbDataContext();
                db.Log = logwriter;

                logger.DebugFormat("Checking to see if security code already exists for [{0}].", DateTime.Today);
                // First thing to do if see if a security code already exists for the day.
                int securityCodesForDate = (from sc in db.SecurityCodes
                                            where sc.GenerationDate.Equals(DateTime.Today)
                                            select sc).Count();

                // If no security codes, this is a check-in for a new day.
                // We will reset the security code counter before starting to generate 
                // new security codes.
                if (0 == securityCodesForDate)
                {
                    logger.DebugFormat("Resetting security code counter because this is first check in [{0}].", DateTime.Today);
                    db.ResetSecurityCode();
                }

                logger.DebugFormat("Inserting new security code record for [{0}] and returning next security code.", DateTime.Today);
                CACCCheckInDb.SecurityCodes newSecurityCode = new CACCCheckInDb.SecurityCodes { GenerationDate = DateTime.Today };
                db.SecurityCodes.InsertOnSubmit(newSecurityCode);
                db.SubmitChanges();

                return newSecurityCode.SecurityCode;
            }
        }

        //public bool CheckSecurityCodeForDate(DateTime date, int SecurityCode)
        //{
        //    return false;
        //}

        #endregion SecurityCode Operations

        #endregion
    }
}
