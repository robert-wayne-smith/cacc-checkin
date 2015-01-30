using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Logging;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Admin.Views
{
    public class FamilyQuickEntryPresenter
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ILabelPrinterService _labelPrinterService;

        #region Properties

        public IFamilyQuickEntryView View { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labelPrinterService"></param>
        public FamilyQuickEntryPresenter(ILabelPrinterService labelPrinterService)
        {
            _labelPrinterService = labelPrinterService;
        }

        #endregion Constructors

        /// <summary>
        /// Will retrieve the departments for the department combo box
        /// </summary>
        public void GetDataForDepartmentCombos()
        {
            GetDepartmentsFromServiceDelegate fetcher =
                new GetDepartmentsFromServiceDelegate(GetDepartmentsFromService);

            fetcher.BeginInvoke(null, null);
        }

        /// <summary>
        /// Will retrieve the classes for the class combo box
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="fromCombo"></param>
        public void GetDataForClassCombo(Guid departmentId)
        {
            //GetDepartmentClassesFromServiceDelegate fetcher =
            //   new GetDepartmentClassesFromServiceDelegate(GetDepartmentClassesFromService);

            //fetcher.BeginInvoke(departmentId, null, null);
            GetDepartmentClassesFromService(departmentId);
        }

        public void GetAllFamiliesInDepartment(string departmentName)
        {
            GetAllFamiliesInDepartmentFromServiceDelegate fetcher =
                new GetAllFamiliesInDepartmentFromServiceDelegate(GetAllFamiliesInDepartmentFromService);

            fetcher.BeginInvoke(departmentName, null, null);
        }

        /// <summary>
        /// Save and check in the list of people if flag is set
        /// </summary>
        /// <param name="people"></param>
        /// <param name="checkIn"></param>
        public void SaveAndCheckInPeople(List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people,
            bool checkIn)
        {
            SaveAndCheckInPeopleDelegate fetcher =
                new SaveAndCheckInPeopleDelegate(InnerSaveAndCheckInPeople);

            fetcher.BeginInvoke(people, checkIn, null, null);
        }

        #region Private Functions

        private delegate void GetDepartmentsFromServiceDelegate();
        private void GetDepartmentsFromService()
        {
            try
            {
                List<CACCCheckInDb.Department> departments = null;

                logger.Debug("Retrieving GetAllFromDepartment from CACCCheckInService");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    departments = proxy.GetAllFromDepartment();
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.Send,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.DepartmentsDataContext = departments;
                            return null;
                        }), null);

            }
            catch (Exception ex)
            {
                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.DisplayExceptionDetail(ex);
                            return null;
                        }), null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="departmentId"></param>
        private delegate void GetDepartmentClassesFromServiceDelegate(Guid departmentId);
        private void GetDepartmentClassesFromService(Guid departmentId)
        {
            try
            {
                List<CACCCheckInDb.Class> classes = null;

                logger.Debug("Retrieving GetClassesByDeptId from CACCCheckInService");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    classes = proxy.GetClassesByDeptId(departmentId);
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.Send,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.DepartmentClassesDataContext = classes;
                            return null;
                        }), null);

            }
            catch (Exception ex)
            {
                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.DisplayExceptionDetail(ex);
                            return null;
                        }), null);
            }
        }

        private delegate void GetAllFamiliesInDepartmentFromServiceDelegate(string departmentName);
        private void GetAllFamiliesInDepartmentFromService(string departmentName)
        {
            try
            {
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people = null;

                logger.Debug("Retrieving GetAllFamiliesInDepartment from CACCCheckInService");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    people = proxy.GetAllFamiliesInDepartment(departmentName);
                }

                List<Family> families = new List<Family>();
                
                // Group all the people into families based on FamilyId
                IEnumerable<IGrouping<Guid?, CACCCheckInDb.PeopleWithDepartmentAndClassView>> familyGroups =
                    people.GroupBy(r => r.FamilyId);
                
                // Loop through all the family groups and build a List of Family
                foreach (IGrouping<Guid?, CACCCheckInDb.PeopleWithDepartmentAndClassView> familyGroup in familyGroups)
                {                    
                    if (!familyGroup.Key.HasValue) continue;

                    Family family = new Family();
                    family.Id = familyGroup.Key.Value;
                    
                    // Loop through all family members in current family group
                    foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView familyMember in familyGroup)
                    {                        
                        // We will base the family name on LastName of adult in certain roles.
                        if (!String.IsNullOrWhiteSpace(family.Name) &&
                            (familyMember.FamilyRole.Equals("Adult") ||
                            familyMember.FamilyRole.Equals("Mother") ||
                            familyMember.FamilyRole.Equals("Father") ||
                            familyMember.FamilyRole.Equals("HeadOfHousehold")))
                        {
                            family.Name = familyMember.LastName;
                        }
                        
                        family.Members.Add(familyMember);
                    }

                    // If we didn't get a family name for some reason from adults in family
                    // we will just base it on LastName of first person in the family group
                    if (String.IsNullOrWhiteSpace(family.Name))
                    {
                        family.Name = familyGroup.First().LastName;
                    }

                    families.Add(family);
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.Send,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.FamiliesDataContext = families;
                            return null;
                        }), null);

            }
            catch (Exception ex)
            {
                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.DisplayExceptionDetail(ex);
                            return null;
                        }), null);
            }
        }

        /// <summary>
        /// Will add a person
        /// </summary>
        /// <param name="person"></param>
        private void AddPerson(CACCCheckInDb.PeopleWithDepartmentAndClassView person)
        {
            logger.DebugFormat("Adding person [{0} {1}]",
                person.FirstName, person.LastName);

            // Get a reference to ClassMember record for current person
            CACCCheckInDb.Person newPerson = (CACCCheckInDb.Person)person;

            using (CACCCheckInServiceProxy proxy =
                new CACCCheckInServiceProxy())
            {
                proxy.Open();

                logger.Debug("Calling InsertPeople in CACCCheckInService");
                proxy.InsertPeople(newPerson);
            }
        }

        /// <summary>
        /// Will add a person to class
        /// </summary>
        /// <param name="person"></param>
        private void AddPersonClassMembership(CACCCheckInDb.PeopleWithDepartmentAndClassView person)
        {
            logger.DebugFormat("Adding class membership for [{0} {1}]",
                person.FirstName, person.LastName);

            // Get a reference to ClassMember record for current person
            CACCCheckInDb.ClassMember newClassMember = (CACCCheckInDb.ClassMember)person;

            using (CACCCheckInServiceProxy proxy =
                new CACCCheckInServiceProxy())
            {
                proxy.Open();

                logger.Debug("Calling InsertClassMember in CACCCheckInService");
                proxy.InsertClassMember(newClassMember);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="people"></param>
        /// <param name="checkIn"></param>
        private delegate void SaveAndCheckInPeopleDelegate(List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people,
            bool checkIn);
        private void InnerSaveAndCheckInPeople(List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people,
            bool checkIn)
        {
            try
            {
                foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in people)
                {
                    AddPerson(person);
                    AddPersonClassMembership(person);
                }

                if (checkIn)
                {
                    using (CACCCheckInServiceProxy proxy =
                        new CACCCheckInServiceProxy())
                    {
                        proxy.Open();

                        int currentSecurityCodeForFamily = proxy.GetNewSecurityCodeForToday();

                        foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in people)
                        {
                            if (!String.IsNullOrEmpty(person.SecurityCode))
                            {
                                logger.DebugFormat("Skipping check-in of person: {0} {1}. Already checked in with security code: {2}",
                                   person.FirstName, person.LastName, person.SecurityCode);

                                continue;
                            }

                            logger.DebugFormat("Checking in person: {0} {1}",
                               person.FirstName, person.LastName);

                            person.SecurityCode = currentSecurityCodeForFamily.ToString();

                            logger.DebugFormat("Inserting attendance record for person: Name=[{0} {1}], SecurityCode=[{2}]",
                                person.FirstName, person.LastName, currentSecurityCodeForFamily);

                            proxy.InsertAttendance(new CACCCheckInDb.Attendance
                            {
                                Date = DateTime.Today,
                                ClassId = person.ClassId,
                                PersonId = person.PersonId,
                                SecurityCode = currentSecurityCodeForFamily
                            });

                            logger.DebugFormat("Printing label for person: Name=[{0} {1}], SecurityCode=[{2}]",
                                person.FirstName, person.LastName, currentSecurityCodeForFamily);

                            _labelPrinterService.PrintLabels(DateTime.Today, Constants.ChurchName,
                                currentSecurityCodeForFamily, person);

                            View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                                new DispatcherOperationCallback(
                                    delegate(object arg)
                                    {
                                        View.CheckInCompleted(person);
                                        return null;
                                    }), null);
                        }
                    }
                }
                
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                            new DispatcherOperationCallback(
                                delegate(object arg)
                                {
                                    View.ProcessingCompleted();
                                    return null;
                                }), null);
            }
            catch (Exception ex)
            {
                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.DisplayExceptionDetail(ex);
                            return null;
                        }), null);
            }
        }

        #endregion Private Functions
    }
}
