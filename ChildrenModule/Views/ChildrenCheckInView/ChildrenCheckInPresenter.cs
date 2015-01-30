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

namespace CACCCheckIn.Modules.Children.Views
{
    public class ChildrenCheckInPresenter
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IEventAggregator _eventAggregator;
        private readonly ILabelPrinterService _labelPrinterService;
        private const string TargetDepartment = Departments.Children;

        #region Properties

        public IChildrenCheckInView View { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor for ChildrenCheckInPresenter
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="labelPrinterService"></param>
        public ChildrenCheckInPresenter(IEventAggregator eventAggregator,
            ILabelPrinterService labelPrinterService)
        {
            logger.DebugFormat("Creating ChildrenCheckInPresenter.");

            _eventAggregator = eventAggregator;
            _labelPrinterService = labelPrinterService;
        }

        #endregion Constructors

        /// <summary>
        /// Asynchronously call to get list of children
        /// </summary>
        public void GetListOfChildren()
        {
            GetPeopleToCheckInFromServiceDelegate fetcher =
                new GetPeopleToCheckInFromServiceDelegate(GetPeopleToCheckInFromService);

            fetcher.BeginInvoke(null, null);
        }

        /// <summary>
        /// Will retrieve the classes for the exclusive check-in combo
        /// </summary>
        public void GetDataForClassCombo()
        {
            GetDataForClassComboFromServiceDelegate fetcher =
                new GetDataForClassComboFromServiceDelegate(GetDataForClassComboFromService);

            fetcher.BeginInvoke(null, null);
        }

        /// <summary>
        /// Call to see if child is already checked in today to specific class
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public bool IsChildAlreadyCheckedInToday(CACCCheckInDb.PeopleWithDepartmentAndClassView person,
            Guid classId)
        {
            bool isCheckedIntoClassToday = false;

            try
            {
                logger.Debug("Retrieving attendance records for person for today.");

                using (CACCCheckInServiceProxy proxy =
                        new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    logger.Debug("Calling GetAttendanceByPersonIdAndDate on CACCCheckInService.");

                    // First, we get attendance records for person for Today
                    List<CACCCheckInDb.AttendanceWithDetail> records = 
                        proxy.GetAttendanceByPersonIdAndDate(person.PersonId, DateTime.Today);

                    // Then, we see if the attendance was in the specified class
                    isCheckedIntoClassToday = records.Any(r => r.ClassId.Equals(classId));
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

            return isCheckedIntoClassToday;
        }

        /// <summary>
        /// Asynchronously call to check in child
        /// </summary>
        /// <param name="person"></param>
        public void CheckInPerson(CACCCheckInDb.PeopleWithDepartmentAndClassView person)
        {
            CheckInPersonAndPrintLabelDelegate fetcher =
                new CheckInPersonAndPrintLabelDelegate(CheckInPersonAndPrintLabel);

            fetcher.BeginInvoke(person, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="person"></param>
        private delegate void CheckInPersonAndPrintLabelDelegate(CACCCheckInDb.PeopleWithDepartmentAndClassView person);
        public void CheckInPersonAndPrintLabel(CACCCheckInDb.PeopleWithDepartmentAndClassView person)
        {
            try
            {
                logger.DebugFormat("Checking in person: {0} {1}",
                    person.FirstName, person.LastName);

                int newSecurityCode = 0;

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    // No security codes are currently being used in Children department
                    // so, we will leave this commented out
                    //logger.Debug("Calling GetNewSecurityCodeForToday on CACCCheckInService.");
                    //newSecurityCode = proxy.GetNewSecurityCodeForToday();

                    logger.DebugFormat("Inserting attendance record for person: Name=[{0} {1}], SecurityCode=[{2}]",
                        person.FirstName, person.LastName, newSecurityCode);

                    proxy.InsertAttendance(new CACCCheckInDb.Attendance
                    {
                        Date = DateTime.Today,
                        ClassId = person.ClassId,
                        PersonId = person.PersonId,
                        SecurityCode = newSecurityCode
                    });
                }

                if (person.ClassRole.Equals(ClassRoles.Teacher))
                {
                    logger.DebugFormat("Printing label for teacher: Name=[{0} {1}]",
                        person.FirstName, person.LastName);
                    
                    person.SpecialConditions = ClassRoles.Teacher;
                    
                    _labelPrinterService.PrintLabels(null, Constants.ChurchName,
                        0, person);
                }
                else
                {
                    logger.DebugFormat("Printing label for person: Name=[{0} {1}], SecurityCode=[{2}]",
                        person.FirstName, person.LastName, newSecurityCode);

                    _labelPrinterService.PrintLabels(DateTime.Today, Constants.ChurchName,
                        newSecurityCode, person);
                }

                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.CheckInCompleted(person);
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
        /// Gets the list of children to display in the list box. Only want 
        /// to retrieve the Children here.
        /// </summary>
        private delegate void GetPeopleToCheckInFromServiceDelegate();
        private void GetPeopleToCheckInFromService()
        {
            try
            {
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people = null;

                logger.Debug("Retrieving GetPeopleWithDepartmentAndClassByDepartment from CACCCheckInService");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    people = proxy.GetPeopleWithDepartmentAndClassByDepartment(TargetDepartment);
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.PeopleDataContext = people;
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
        /// Gets all the classes in the specified department
        /// </summary>
        private delegate void GetDataForClassComboFromServiceDelegate();
        private void GetDataForClassComboFromService()
        {
            try
            {
                List<CACCCheckInDb.Class> classes = null;

                logger.Debug("Retrieving GetClassesByDeptName from CACCCheckInService");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    classes = proxy.GetClassesByDeptName(TargetDepartment);
                }

                // Add a bogus class to the list, this will be used
                // so user can select it to signify default
                classes.Insert(0, new CACCCheckInDb.Class
                    {
                        Name = String.Empty,
                        Id = new Guid("00000000-0000-0000-0000-000000000000")
                    });

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.ClassesDataContext = classes;
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
