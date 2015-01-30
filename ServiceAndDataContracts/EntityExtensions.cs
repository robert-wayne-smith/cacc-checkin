using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ServiceAndDataContracts;

namespace CACCCheckInDb
{
    public delegate void ItemEndEditEventHandler(IEditableObject sender);

    #region Department Extensions

    public class Departments : ObservableCollection<Department>
    {
        public Departments()
            : base()
        {}

        public Departments(IEnumerable<Department> collection)
            : base(collection)
        {}

        public Departments(List<Department> list)
        {
            foreach(Department department in list)
            {
                // handle any EndEdit events relating to this item
                department.ItemEndEdit += new ItemEndEditEventHandler(ItemEndEditHandler);

                base.Add(department);
            }
        }
        
        void ItemEndEditHandler(IEditableObject sender)
        {
            // simply forward any EndEdit events
            if (ItemEndEdit != null)
            {
                ItemEndEdit(sender);
            }
        }

        #region Events

        public event ItemEndEditEventHandler ItemEndEdit;

        #endregion
    }


    public partial class Department : IEditableObject, ICloneable
    {
        private Department copy;
        private bool editing;

        #region Events

        public event ItemEndEditEventHandler ItemEndEdit;

        #endregion

        #region IEditableObject Members

        public void BeginEdit()
        {
            if (!editing)
            {
                editing = true;
                
                if (null == copy)
                {
                    copy = new Department();
                }

                copy = this.MemberwiseClone() as Department;
            }
        }

        public void CancelEdit()
        {
            if (editing)
            {
                CopyFrom(copy);
                
                editing = false;
            }
        }

        public void EndEdit()
        {
            if (editing)
            {
                System.Diagnostics.Trace.WriteLine("EndEdit: " + this.Name);
                editing = false;
                copy = null;

                if (ItemEndEdit != null)
                {
                    ItemEndEdit(this);
                }
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return (Department)this.MemberwiseClone();
        }

        #endregion

        public void CopyFrom(Department department)
        {
            if (department != null)
            {
                this.Id = department.Id;
                this.Name = department.Name;
                this.Description = department.Description;
            }
        }
    }

    #endregion Department Extensions

    #region Class Extensions

    public class ClassBindingList : SortableBindingList<Class>
    {       
        public ClassBindingList() : base() { }
        public ClassBindingList(List<Class> classes)
        {
            foreach (Class c in classes)
            { base.Add(c); }
        }
        public ClassBindingList(IEnumerable<Class> classes)
        {
            foreach(Class c in classes)
            { base.Add(c); }
        }

        protected override object AddNewCore()
        {
            Class c = new Class();
            c.Id = Guid.NewGuid();
            c.Name = "<Enter Class Name>";
            c.Description = "<Enter Class Description>";
            this.Add(c);

            return c;
        }
    }

    public class Classes : ObservableCollection<Class>
    {
        public Classes()
            : base()
        { }

        public Classes(IEnumerable<Class> collection)
            : base(collection)
        { }

        public Classes(List<Class> list)
        {
            foreach (Class aClass in list)
            {
                // handle any EndEdit events relating to this item
                aClass.ItemEndEdit += new ItemEndEditEventHandler(ItemEndEditHandler);

                base.Add(aClass);
            }
        }

        void ItemEndEditHandler(IEditableObject sender)
        {
            // simply forward any EndEdit events
            if (ItemEndEdit != null)
            {
                ItemEndEdit(sender);
            }
        }

        #region Events

        public event ItemEndEditEventHandler ItemEndEdit;

        #endregion
    }


    public partial class Class : IEditableObject, ICloneable
    {
        private Class copy;
        private bool editing;

        #region Events

        public event ItemEndEditEventHandler ItemEndEdit;

        #endregion

        #region IEditableObject Members

        public void BeginEdit()
        {
            if (!editing)
            {
                editing = true;

                if (null == copy)
                {
                    copy = new Class();
                }

                copy = this.MemberwiseClone() as Class;
            }
        }

        public void CancelEdit()
        {
            if (editing)
            {
                CopyFrom(copy);

                editing = false;
            }
        }

        public void EndEdit()
        {
            if (editing)
            {
                editing = false;
                copy = null;

                if (ItemEndEdit != null)
                {
                    ItemEndEdit(this);
                }
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return (Class)this.MemberwiseClone();
        }

        #endregion

        public void CopyFrom(Class aClass)
        {
            if (aClass != null)
            {
                this.Id = aClass.Id;
                this.Description = aClass.Description;
                this.Name = aClass.Name;
            }
        }
    }

    #endregion Class Extensions

    #region Attendance Extensions

    /// <summary>
    /// Custom collection for AttendanceWithDetail 
    /// </summary>
    public class AttendanceRecords : ObservableCollection<AttendanceWithDetail>
    {
        public AttendanceRecords()
            : base()
        { }

        public AttendanceRecords(IEnumerable<AttendanceWithDetail> collection)
            : base(collection)
        { }

        public AttendanceRecords(List<AttendanceWithDetail> list)
            : base(list)
        { }
    }

    public partial class AttendanceWithDetail
    {
        public static explicit operator Attendance(AttendanceWithDetail r)
        {
            return new Attendance()
            {
                Date = r.Date,
                PersonId = r.PersonId,
                ClassId = r.ClassId,
                SecurityCode = r.SecurityCode                
            };
        }

        public static explicit operator PeopleWithDepartmentAndClassView(AttendanceWithDetail r)
        {
            return new PeopleWithDepartmentAndClassView()
            { 
                DepartmentId = r.DeptId,
                DepartmentName = r.DeptName,
                ClassId = r.ClassId,
                ClassName = r.ClassName,
	            ClassRole = r.ClassRole,
		        PersonId = r.PersonId,
		        FirstName = r.FirstName,
		        LastName = r.LastName,
		        PhoneNumber = r.PhoneNumber,
		        SpecialConditions = r.SpecialConditions,
		        FamilyId = r.FamilyId
            };
        }
    }

    #endregion Attendance Extensions

    #region Person Extensions

    public class People : ObservableCollection<Person>
    {
        public People()
            : base()
        { }

        public People(IEnumerable<Person> collection)
            : base(collection)
        { }

        public People(List<Person> list)
        {
            foreach (Person person in list)
            {
                // handle any EndEdit events relating to this item
                person.ItemEndEdit += new ItemEndEditEventHandler(ItemEndEditHandler);

                base.Add(person);
            }
        }

        void ItemEndEditHandler(IEditableObject sender)
        {
            // simply forward any EndEdit events
            if (ItemEndEdit != null)
            {
                ItemEndEdit(sender);
            }
        }

        #region Events

        public event ItemEndEditEventHandler ItemEndEdit;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="person"></param>
        public static CACCCheckInDb.Person InsertPerson(CACCCheckInDb.Person person)
        {
            using (CACCCheckInServiceProxy proxy =
               new CACCCheckInServiceProxy())
            {
                proxy.Open();

                person = proxy.InsertPeople(person);
            }

            return person;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="person"></param>
        public static void UpdatePerson(CACCCheckInDb.Person person)
        {
            using (CACCCheckInServiceProxy proxy =
               new CACCCheckInServiceProxy())
            {
                proxy.Open();

                proxy.UpdatePeople(person);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="person"></param>
        public static void DeletePerson(CACCCheckInDb.Person person)
        {
            using (CACCCheckInServiceProxy proxy =
               new CACCCheckInServiceProxy())
            {
                proxy.Open();

                proxy.DeletePeople(person);
            }
        }
    }

    public partial class Person : IEditableObject, ICloneable
    {
        private Person copy;
        private bool editing;

        #region Events

        public event ItemEndEditEventHandler ItemEndEdit;

        #endregion

        #region IEditableObject Members

        public void BeginEdit()
        {
            if (!editing)
            {
                editing = true;

                if (null == copy)
                {
                    copy = new Person();
                }

                copy = this.MemberwiseClone() as Person;
            }
        }

        public void CancelEdit()
        {
            if (editing)
            {
                CopyFrom(copy);

                editing = false;
            }
        }

        public void EndEdit()
        {
            if (editing)
            {
                editing = false;
                copy = null;

                if (ItemEndEdit != null)
                {
                    ItemEndEdit(this);
                }
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return (Person)this.MemberwiseClone();
        }

        #endregion

        public void CopyFrom(Person person)
        {
            if (person != null)
            {
                this.FamilyId = person.FamilyId;
                this.FamilyRole = person.FamilyRole;
                this.FirstName = person.FirstName;
                this.Id = person.Id;
                this.LastName = person.LastName;
                this.PhoneNumber = person.PhoneNumber;
                this.SpecialConditions = person.SpecialConditions;
            }
        }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            Person p = (Person)obj;
            return (Id == p.Id);
        }
        
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    #endregion Person Extensions

    #region PeopleWithDepartmentAndClassView Extensions

    public class PeopleWithDepartmentAndClassViewBindingList : SortableBindingList<PeopleWithDepartmentAndClassView>
    {
        public PeopleWithDepartmentAndClassViewBindingList() : base() { }
        public PeopleWithDepartmentAndClassViewBindingList(List<PeopleWithDepartmentAndClassView> people)
        {
            foreach (PeopleWithDepartmentAndClassView p in people)
            {
                p.NeedsPersist = false;
                base.Add(p);
            }
        }

        protected override object AddNewCore()
        {
            PeopleWithDepartmentAndClassView p = new PeopleWithDepartmentAndClassView();
            p.PersonId = Guid.NewGuid();
            p.FirstName = "<Enter First Name>";
            p.LastName = "<Enter Last Name>";
            this.Add(p);

            return p;
        }
    }

    public partial class PeopleWithDepartmentAndClassView : IEditableObject, ICloneable
    {
        private PeopleWithDepartmentAndClassView copy;
        private bool editing;

        #region Events

        public event ItemEndEditEventHandler ItemEndEdit;

        #endregion
        
        private string _SecurityCode;

        partial void OnSecurityCodeChanging(string value);
        partial void OnSecurityCodeChanged();

        public string SecurityCode
        {
            get
            {
                return this._SecurityCode;
            }
            set
            {
                if ((this._SecurityCode != value))
                {
                    this.OnSecurityCodeChanging(value);
                    this._SecurityCode = value;
                    this.OnSecurityCodeChanged();
                }
            }
        }
        
        public bool NeedsPersist { get; set; }

        public static explicit operator Class(PeopleWithDepartmentAndClassView p)
        {
            return new Class()
            {
                DeptId = p.DepartmentId,
                Description = String.Empty,
                Id = p.ClassId,
                Name = p.ClassName
            };
        }

        public static explicit operator Department(PeopleWithDepartmentAndClassView p)
        {
            return new Department()
            {
                Id = p.DepartmentId,
                Description = String.Empty,
                Name = p.DepartmentName
            };
        }

        public static explicit operator ClassMember(PeopleWithDepartmentAndClassView p)
        {
            return new ClassMember()
            {
                ClassId = p.ClassId,
                ClassRole = p.ClassRole,
                PersonId = p.PersonId
            };
        }

        public static explicit operator Person(PeopleWithDepartmentAndClassView p)
        {
            return new Person()
            {
                FamilyId = p.FamilyId,
                FamilyRole = p.FamilyRole,
                FirstName = p.FirstName,
                Id = p.PersonId,
                LastName = p.LastName,
                PhoneNumber = p.PhoneNumber,
                SpecialConditions = p.SpecialConditions
            };
        }

        #region IEditableObject Members

        public void BeginEdit()
        {
            if (!editing)
            {
                editing = true;

                if (null == copy)
                {
                    copy = new PeopleWithDepartmentAndClassView();
                }

                copy = this.MemberwiseClone() as PeopleWithDepartmentAndClassView;
            }
        }

        public void CancelEdit()
        {
            if (editing)
            {
                CopyFrom(copy);

                editing = false;
            }
        }

        public void EndEdit()
        {
            if (editing)
            {
                editing = false;
                copy = null;

                if (ItemEndEdit != null)
                {
                    ItemEndEdit(this);
                }
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            PeopleWithDepartmentAndClassView person =
                (PeopleWithDepartmentAndClassView)this.MemberwiseClone();
            person.NeedsPersist = false;
            return person;
        }

        #endregion

        public void CopyFrom(PeopleWithDepartmentAndClassView person)
        {
            if (person != null)
            {
                this.ClassId = person.ClassId;
                this.ClassName = person.ClassName;
                this.ClassRole = person.ClassRole;
                this.DepartmentId = person.DepartmentId;
                this.DepartmentName = person.DepartmentName;
                this.FamilyId = person.FamilyId;
                this.FamilyRole = person.FamilyRole;
                this.FirstName = person.FirstName;
                this.LastName = person.LastName;
                this.PersonId = person.PersonId;
                this.PhoneNumber = person.PhoneNumber;
                this.SecurityCode = person.SecurityCode;
                this.SpecialConditions = person.SpecialConditions;
                this.NeedsPersist = false;
            }
        }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            PeopleWithDepartmentAndClassView p = (PeopleWithDepartmentAndClassView)obj;
            return (PersonId == p.PersonId);
        }

        public override int GetHashCode()
        {
            return PersonId.GetHashCode();
        }
    }

    #endregion PeopleWithDepartmentAndClassView Extensions

    #region FamilyRole Extensions

    public partial class FamilyRole
    {
        public List<string> GetAllFamilyRoles()
        {
            List<string> records = null;

            try
            {
                using (CACCCheckInServiceProxy proxy =
                        new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    records = proxy.GetAllFromFamilyRole();
                }
            }
            catch (Exception)
            {}
            
            return records;
        }
    }

    #endregion FamilyRole Extensions
}
