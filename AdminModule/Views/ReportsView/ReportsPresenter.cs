using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Logging;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Admin.Views
{
    public enum ReportTypes
    {
         ClassAttendanceDuringDateRange,
         ClassAttendanceCountDuringDateRange,
         AttendanceRecordForPerson
    }

    public class ReportsPresenter
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties

        public IReportPresenterView View { get; set; }

        #endregion Properties

        #region Constructors

        public ReportsPresenter()
        {
        }

        #endregion Constructors

        public void GetDepartments()
        {
            GetDepartmentsFromServiceDelegate fetcher =
                new GetDepartmentsFromServiceDelegate(GetDepartmentsFromService);

            fetcher.BeginInvoke(null, null);
        }

        public void GetDepartmentClasses(Guid departmentId)
        {
            GetDepartmentClassesFromServiceDelegate fetcher =
                new GetDepartmentClassesFromServiceDelegate(GetDepartmentClassesFromService);

            fetcher.BeginInvoke(departmentId, null, null);
        }

        public void GetDepartmentPeople(string departmentName)
        {
            GetDepartmentPeopleFromServiceDelegate fetcher =
                new GetDepartmentPeopleFromServiceDelegate(GetDepartmentPeopleFromService);

            fetcher.BeginInvoke(departmentName, null, null);
        }

        public void GetPersonAttendanceOnDate(Guid personId, DateTime? attendanceDate)
        {
            GetPersonAttendanceOnDateFromServiceDelegate fetcher =
                new GetPersonAttendanceOnDateFromServiceDelegate(GetPersonAttendanceOnDateFromService);

            fetcher.BeginInvoke(personId, attendanceDate, null, null);
        }

        public void GetClassAttendanceInDateRange(Guid departmentId, Guid classId,
            DateTime? attendanceStartDate, DateTime? attendanceEndDate)
        {
            GetClassAttendanceInDateRangeFromServiceDelegate fetcher =
                new GetClassAttendanceInDateRangeFromServiceDelegate(GetClassAttendanceInDateRangeFromService);

            fetcher.BeginInvoke(departmentId, classId, attendanceStartDate, attendanceEndDate, null, null);
        }

        #region Private Functions

        private delegate void GetDepartmentsFromServiceDelegate();
        private void GetDepartmentsFromService()
        {
            try
            {
                List<CACCCheckInDb.Department> departments = null;

                logger.Debug("Retrieving GetAllFromDepartment from CACCCheckInServiceProxy");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    departments = proxy.GetAllFromDepartment();
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
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

        private delegate void GetDepartmentClassesFromServiceDelegate(Guid departmentId);
        private void GetDepartmentClassesFromService(Guid departmentId)
        {
            try
            {
                List<CACCCheckInDb.Class> classes = null;

                logger.Debug("Retrieving GetClassesByDeptId from CACCCheckInServiceProxy");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    classes = proxy.GetClassesByDeptId(departmentId);
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

        private delegate void GetDepartmentPeopleFromServiceDelegate(string departmentName);
        private void GetDepartmentPeopleFromService(string departmentName)
        {
            try
            {
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people = null;

                logger.Debug("Retrieving GetPeopleWithDepartmentAndClassByDepartment from CACCCheckInServiceProxy");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    people = proxy.GetPeopleWithDepartmentAndClassByDepartment(departmentName);
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.DepartmentPeopleDataContext = people;
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

        private delegate void GetPersonAttendanceOnDateFromServiceDelegate(Guid personId, DateTime? attendanceDate);
        private void GetPersonAttendanceOnDateFromService(Guid personId, DateTime? attendanceDate)
        {
            try
            {
                List<CACCCheckInDb.AttendanceWithDetail> records = null;

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    if (attendanceDate.HasValue)
                    {
                        logger.Debug("Retrieving GetAttendanceByPersonIdAndDate from CACCCheckInServiceProxy");

                        records = proxy.GetAttendanceByPersonIdAndDate(personId,
                            attendanceDate.Value);
                    }
                    else
                    {
                        logger.Debug("Retrieving GetAttendanceByPersonId from CACCCheckInServiceProxy");

                        records = proxy.GetAttendanceByPersonId(personId);
                    }
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.ReportDataContext = records;
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

        private delegate void GetClassAttendanceInDateRangeFromServiceDelegate(Guid departmentId, Guid classId,
            DateTime? attendanceStartDate, DateTime? attendanceEndDate);
        private void GetClassAttendanceInDateRangeFromService(Guid departmentId, Guid classId,
            DateTime? attendanceStartDate, DateTime? attendanceEndDate)
        {
            try
            {
                List<CACCCheckInDb.AttendanceWithDetail> records = null;

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    if (classId.Equals(new Guid("00000000-0000-0000-0000-000000000000")))
                    {
                        logger.Debug("Retrieving GetAttendanceByDepartmentAndDateRange from CACCCheckInServiceProxy");

                        records = proxy.GetAttendanceByDepartmentAndDateRange(departmentId,
                            attendanceStartDate.Value, attendanceEndDate.Value);
                    }
                    else
                    {
                        logger.Debug("Retrieving GetAttendanceByDepartmentAndClassIdAndDateRange from CACCCheckInServiceProxy");

                        records = proxy.GetAttendanceByDepartmentAndClassIdAndDateRange(
                            departmentId, classId,
                            attendanceStartDate.Value, attendanceEndDate.Value);
                    }
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.ReportDataContext = records;
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
