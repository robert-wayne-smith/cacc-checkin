using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Threading;
using ServiceAndDataContracts;
using Infrastructure;
using log4net;

namespace CACCCheckIn.Modules.Admin.Views
{
    public class AttendancePresenter
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ILabelPrinterService _labelPrinterService;

        #region Properties

        public IAttendanceView View { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="labelPrinterService"></param>
        public AttendancePresenter(ILabelPrinterService labelPrinterService)
        {
            _labelPrinterService = labelPrinterService;
        }

        #endregion Constructors
        
        /// <summary>
        /// Asynchronously retrieves the departments
        /// </summary>
        public void GetDepartments()
        {
            GetDepartmentsFromServiceDelegate fetcher =
                new GetDepartmentsFromServiceDelegate(GetDepartmentsFromService);

            fetcher.BeginInvoke(null, null);
        }

        /// <summary>
        /// Asynchronously retrieves attendance records for the department
        /// </summary>
        public void GetAttendanceByDeptId(Guid departmentId)
        {
            GetAttendanceByDeptIdFromServiceDelegate fetcher =
                new GetAttendanceByDeptIdFromServiceDelegate(GetAttendanceByDeptIdFromService);

            fetcher.BeginInvoke(departmentId, null, null);
        }

        /// <summary>
        /// Reprints the label for attendance record
        /// </summary>
        /// <param name="record"></param>
        public void ReprintLabel(CACCCheckInDb.AttendanceWithDetail record)
        {
            InnerReprintLabelDelegate fetcher =
                new InnerReprintLabelDelegate(InnerReprintLabel);

            fetcher.BeginInvoke(record, null, null);
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

        private delegate void GetAttendanceByDeptIdFromServiceDelegate(Guid departmentId);
        private void GetAttendanceByDeptIdFromService(Guid departmentId)
        {
            try
            {
                CACCCheckInDb.AttendanceRecords attendance = null;

                logger.Debug("Retrieving GetAttendanceByDeptId from CACCCheckInService");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    attendance = new CACCCheckInDb.AttendanceRecords(proxy.GetAttendanceByDeptId(departmentId));

                    logger.Debug("Hooking up CollectionChanged event handler for attendance records");

                    attendance.CollectionChanged +=
                        new NotifyCollectionChangedEventHandler(AttendanceRecordsCollectionChanged);
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.AttendanceDataContext = attendance;
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
        /// Handles collection changed events for attendance records
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AttendanceRecordsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        // Attendance records have been removed from the Attendance list.
                        // We will delete the Attendance record.
                        foreach (CACCCheckInDb.AttendanceWithDetail record in e.OldItems)
                        {
                            logger.DebugFormat("Deleting attendance record for [{0} {1}]",
                                record.FirstName, record.LastName);

                            DeleteAttendance((CACCCheckInDb.Attendance)record);
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        break;
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
        /// Deletes attendance record
        /// </summary>
        /// <param name="record"></param>
        private void DeleteAttendance(CACCCheckInDb.Attendance record)
        {
            using (CACCCheckInServiceProxy proxy =
               new CACCCheckInServiceProxy())
            {
                proxy.Open();

                proxy.DeleteAttendance(record);
            }
        }

        /// <summary>
        /// Reprints the label for attendance record
        /// </summary>
        /// <param name="record"></param>
        private delegate void InnerReprintLabelDelegate(CACCCheckInDb.AttendanceWithDetail record);
        private void InnerReprintLabel(CACCCheckInDb.AttendanceWithDetail record)
        {
            try
            {
                logger.DebugFormat("Reprinting label for person: Name=[{0} {1}], SecurityCode=[{2}]",
                    record.FirstName, record.LastName, record.SecurityCode);

                _labelPrinterService.PrintLabels(record.Date, Constants.ChurchName,
                    record.SecurityCode, (CACCCheckInDb.PeopleWithDepartmentAndClassView)record);

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
