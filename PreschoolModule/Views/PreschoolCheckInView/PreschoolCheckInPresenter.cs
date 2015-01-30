using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Preschool.Views
{
    public class PreschoolCheckInPresenter
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IEventAggregator _eventAggregator;
        private readonly ILabelPrinterService _labelPrinterService;
        private const string TargetDepartment = Departments.Preschool;

        #region Properties

        public IPreschoolCheckInView View { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor for PreschoolCheckInPresenter
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="labelPrinterService"></param>
        public PreschoolCheckInPresenter(IEventAggregator eventAggregator,
            ILabelPrinterService labelPrinterService)
        {
            logger.DebugFormat("Creating PreschoolCheckInPresenter.");

            _eventAggregator = eventAggregator;
            _labelPrinterService = labelPrinterService;
        }

        #endregion Constructors

        /// <summary>
        /// Asynchronously call to get people
        /// </summary>
        public void GetPeople()
        {
            GetPeopleFromServiceDelegate fetcher =
                new GetPeopleFromServiceDelegate(GetPeopleFromService);

            fetcher.BeginInvoke(null, null);
        }

        /// <summary>
        /// Retrieve the attendance records of family for Today
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public List<CACCCheckInDb.AttendanceWithDetail> GetAttendanceRecordsForFamilyToday(CACCCheckInDb.PeopleWithDepartmentAndClassView person)
        {
            List<CACCCheckInDb.AttendanceWithDetail> records = null;

            try
            {
                logger.Debug("Retrieving attendance records for family for today.");

                using (CACCCheckInServiceProxy proxy =
                        new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    logger.Debug("Calling GetAttendanceByFamilyIdAndDate on CACCCheckInService.");

                    records = proxy.GetAttendanceByFamilyIdAndDate(person.FamilyId.GetValueOrDefault(),
                        DateTime.Today);
                }
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

            return records;
        }

        /// <summary>
        /// Asynchronously call to get family for person
        /// </summary>
        /// <param name="person"></param>
        public void GetFamilyForPerson(CACCCheckInDb.PeopleWithDepartmentAndClassView person)
        {
            GetFamilyFromServiceDelegate fetcher =
                new GetFamilyFromServiceDelegate(GetFamilyFromService);

            fetcher.BeginInvoke(person, null, null);
        }

        /// <summary>
        /// Asynchronously call to check in people
        /// </summary>
        /// <param name="people"></param>
        /// <param name="currentSecurityCodeForFamily"></param>
        public void CheckInPeople(List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people,
            int currentSecurityCodeForFamily)
        {
            CheckInPeopleAndPrintLabelsDelegate fetcher =
                new CheckInPeopleAndPrintLabelsDelegate(CheckInPeopleAndPrintLabels);

            fetcher.BeginInvoke(people, currentSecurityCodeForFamily, null, null);
        }

        /// <summary>
        /// Calls to update Person in database
        /// </summary>
        /// <param name="person"></param>
        public void UpdatePerson(CACCCheckInDb.Person person)
        {
            try
            {
                using (CACCCheckInServiceProxy proxy =
                   new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    logger.Debug("Calling UpdatePeople on CACCCheckInService.");

                    proxy.UpdatePeople(person);
                }
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
        /// Checks in list of people and prints labels for them
        /// </summary>
        /// <param name="people"></param>
        /// <param name="currentSecurityCodeForFamily"></param>
        private delegate void CheckInPeopleAndPrintLabelsDelegate(List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people,
            int currentSecurityCodeForFamily);
        public void CheckInPeopleAndPrintLabels(List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people,
            int currentSecurityCodeForFamily)
        {
            try
            {
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> peopleToPrint =
                    new List<CACCCheckInDb.PeopleWithDepartmentAndClassView>();
                
                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    if (currentSecurityCodeForFamily == 0)
                    {
                        logger.Debug("Calling GetNewSecurityCodeForToday on CACCCheckInService.");

                        currentSecurityCodeForFamily = proxy.GetNewSecurityCodeForToday();
                    }

                    // Loop through all the people, set security code and insert attendance record
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

                        // We want to print a label for this person,
                        // so lets add to List for printing later
                        peopleToPrint.Add(person);

                        logger.DebugFormat("Inserting attendance record for person: Name=[{0} {1}], SecurityCode=[{2}]",
                            person.FirstName, person.LastName, currentSecurityCodeForFamily);

                        proxy.InsertAttendance(new CACCCheckInDb.Attendance
                        {
                            Date = DateTime.Today,
                            ClassId = person.ClassId,
                            PersonId = person.PersonId,
                            SecurityCode = currentSecurityCodeForFamily
                        });
                        
                        View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                            new DispatcherOperationCallback(
                                delegate(object arg)
                                {
                                    View.CheckInCompleted(person);
                                    return null;
                                }), null);
                    }
                }

                // Print labels for all the people
                _labelPrinterService.PrintLabels(DateTime.Today, Constants.ChurchName,
                    currentSecurityCodeForFamily, peopleToPrint);


                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                            new DispatcherOperationCallback(
                                delegate(object arg)
                                {
                                    View.CheckInCompleted();
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

        #region Private Functions

        /// <summary>
        /// Gets the list of people to display in the list box. Only want 
        /// to retrieve the Adults here.
        /// </summary>
        private delegate void GetPeopleFromServiceDelegate();
        private void GetPeopleFromService()
        {
            try
            {
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people = null;

                logger.Debug("Retrieving GetPeopleWithDepartmentAndClassByDepartment from CACCCheckInService");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    // We retrieve just the Adult members here. 
                    people = proxy.GetPeopleWithDepartmentAndClassByDepartmentAndClass(Departments.Adult,
                        "Adult");
                }

                // Filter the list of adults to make sure we only get Member records. If we
                // don't do this, it is possible we might get multiple records for people
                // who are serve in Member and Teacher class roles.
                var filteredPeople = (from p in people
                                      where p.ClassRole.Equals(ClassRoles.Member)
                                      select p).ToList();

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.PeopleDataContext = filteredPeople;
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
        /// Retrieves the Family members for a specified person. We will filter to 
        /// only get members in Adult or Preschool departments.
        /// </summary>
        /// <param name="person"></param>
        private delegate void GetFamilyFromServiceDelegate(CACCCheckInDb.PeopleWithDepartmentAndClassView person);
        private void GetFamilyFromService(CACCCheckInDb.PeopleWithDepartmentAndClassView person)
        {
            try
            {
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people = null;

                if (person != null)
                {
                    using (CACCCheckInServiceProxy proxy =
                        new CACCCheckInServiceProxy())
                    {
                        proxy.Open();

                        if (person.FamilyId.HasValue)
                        {
                            logger.DebugFormat("Retrieving GetFamilyMembersByFamilyId from CACCCheckInService for FamilyId=[{0}]",
                                person.FamilyId.Value);
                            people = proxy.GetFamilyMembersByFamilyId(person.FamilyId.Value);
                        }
                        else
                        {
                            logger.DebugFormat("Retrieving GetFamilyMembersByFamilyId from CACCCheckInService for PersonId=[{0}]",
                                person.PersonId);
                            people = proxy.GetFamilyMembersByFamilyId(person.PersonId);
                        }
                    }                   
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            // We will filter to get ONLY Family members who are
                            // either in Adult or Preschool department. These are
                            // the only valid departments for Preschool check-in 
                            // screen. Also, we make sure only records where
                            // ClassRole is Member are needed.
                            View.FamilyDataContext = people.FindAll(p => 
                                (p.DepartmentName.Equals(Departments.Adult) ||
                                 p.DepartmentName.Equals(Departments.Preschool)) &&
                                 p.ClassRole.Equals(ClassRoles.Member));
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
