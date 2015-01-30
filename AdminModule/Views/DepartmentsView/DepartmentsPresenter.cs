using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Threading;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Admin.Views
{
    public class DepartmentsPresenter
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties

        public IDepartmentsView View { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public DepartmentsPresenter()
        {
            
        }

        #endregion Constructors

        /// <summary>
        /// Retrieve the list of Departments from service asynchronously
        /// </summary>
        public void GetDepartments()
        {
            GetDepartmentsFromServiceDelegate fetcher =
                new GetDepartmentsFromServiceDelegate(GetDepartmentsFromService);

            fetcher.BeginInvoke(null, null);
        }

        /// <summary>
        /// Retrieve the list of Department classes from service asynchronously
        /// </summary>
        /// <param name="departmentId"></param>
        public void GetDepartmentClasses(Guid departmentId)
        {
            GetDepartmentClassesFromServiceDelegate fetcher =
                new GetDepartmentClassesFromServiceDelegate(GetDepartmentClassesFromService);

            fetcher.BeginInvoke(departmentId, null, null);
        }

        /// <summary>
        /// Retrieve the list of unassigned classes from service asynchronously
        /// </summary>
        public void GetUnassignedClasses()
        {
            GetUnassignedClassesFromServiceDelegate fetcher =
                new GetUnassignedClassesFromServiceDelegate(GetUnassignedClassesFromService);

            fetcher.BeginInvoke(null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="department"></param>
        public void AddDepartment(CACCCheckInDb.Department department)
        {
            using (CACCCheckInServiceProxy proxy =
               new CACCCheckInServiceProxy())
            {
                proxy.Open();

                proxy.InsertDepartment(department);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="department"></param>
        public void UpdateDepartment(CACCCheckInDb.Department department)
        {
            using (CACCCheckInServiceProxy proxy =
               new CACCCheckInServiceProxy())
            {
                proxy.Open();

                proxy.UpdateDepartment(department);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theClass"></param>
        public void UpdateClass(CACCCheckInDb.Class theClass)
        {
            using (CACCCheckInServiceProxy proxy =
               new CACCCheckInServiceProxy())
            {
                proxy.Open();

                proxy.UpdateClass(theClass);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="department"></param>
        public void DeleteDepartment(CACCCheckInDb.Department department)
        {
            using (CACCCheckInServiceProxy proxy =
               new CACCCheckInServiceProxy())
            {
                proxy.Open();

                proxy.DeleteDepartment(department);
            }
        }

        #region Private Functions

        /// <summary>
        /// 
        /// </summary>
        private delegate void GetDepartmentsFromServiceDelegate();
        private void GetDepartmentsFromService()
        {
            try
            {
                CACCCheckInDb.Departments departments = null;

                logger.Debug("Retrieving GetAllFromDepartment from CACCCheckInServiceProxy");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    departments = new CACCCheckInDb.Departments(proxy.GetAllFromDepartment());
                    departments.CollectionChanged +=
                        new NotifyCollectionChangedEventHandler(DepartmentsCollectionChanged);
                    departments.ItemEndEdit +=
                        new CACCCheckInDb.ItemEndEditEventHandler(DepartmentsItemEndEdit);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DepartmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        // A new Department has started being added to the Department list.
                        // We will set some default values and hook up some
                        // event handler so we can capture if they 
                        // actually save the new Department.
                        foreach (CACCCheckInDb.Department department in e.NewItems)
                        {                            
                            department.Id = Guid.NewGuid();
                            department.Name = "<Enter Department Name>";
                            department.Description = "<Enter Department Description>";
                            department.ItemEndEdit +=
                                new CACCCheckInDb.ItemEndEditEventHandler(DepartmentsItemEndEdit);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        // Departments have been removed from the Department list.
                        // We will delete the Department.
                        foreach (CACCCheckInDb.Department department in e.OldItems)
                        {
                            logger.DebugFormat("Deleting department [{0}]", department.Name);

                            DeleteDepartment(department);
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
        /// Whenever a Department in the collection has had its EndEdit 
        /// </summary>
        /// <param name="sender"></param>
        private void DepartmentsItemEndEdit(IEditableObject sender)
        {
            try
            {
                // Get the Department that has been edited. This could be a new
                // class or an edit to existing Department.
                CACCCheckInDb.Department updatedDepartment = sender as CACCCheckInDb.Department;

                // If the RowTimestamp is null, this must be brand new Department 
                // that hasn't been added to the database yet. If not, this
                // must be an update to existing Department
                if (updatedDepartment.RowTimestamp == null)
                {
                    logger.DebugFormat("Adding department [{0}]", updatedDepartment.Name);

                    AddDepartment(updatedDepartment);

                    logger.DebugFormat("Department [{0}] was added.", updatedDepartment.Name);
                }
                else
                {
                    logger.Debug("Department is being updated.");

                    UpdateDepartment(updatedDepartment);

                    logger.DebugFormat("Department [{0}] was updated.", updatedDepartment.Name);
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
        /// 
        /// </summary>
        /// <param name="departmentId"></param>
        private delegate void GetDepartmentClassesFromServiceDelegate(Guid departmentId);
        private void GetDepartmentClassesFromService(Guid departmentId)
        {
            try
            {
                CACCCheckInDb.ClassBindingList classes = null;

                logger.Debug("Retrieving GetClassesByDeptId from CACCCheckInServiceProxy");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    classes = new CACCCheckInDb.ClassBindingList(proxy.GetClassesByDeptId(departmentId));
                }

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

        /// <summary>
        /// 
        /// </summary>
        private delegate void GetUnassignedClassesFromServiceDelegate();
        private void GetUnassignedClassesFromService()
        {
            try
            {
                CACCCheckInDb.ClassBindingList classes = null;

                logger.Debug("Retrieving GetClassesByDeptId from CACCCheckInServiceProxy");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    classes = new CACCCheckInDb.ClassBindingList(proxy.GetClassesByDeptId(new Nullable<Guid>()));
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.UnassignedClassesDataContext = classes;
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
