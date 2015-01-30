using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Threading;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Admin.Views
{
    public class ClassesPresenter
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties

        public IClassesView View { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClassesPresenter()
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
        /// 
        /// </summary>
        /// <param name="newClass"></param>
        public void AddClass(CACCCheckInDb.Class newClass)
        {
            using (CACCCheckInServiceProxy proxy =
               new CACCCheckInServiceProxy())
            {
                proxy.Open();

                proxy.InsertClass(newClass);
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
        /// <param name="theClass"></param>
        public void DeleteClass(CACCCheckInDb.Class theClass)
        {
            using (CACCCheckInServiceProxy proxy =
               new CACCCheckInServiceProxy())
            {
                proxy.Open();

                proxy.DeleteClass(theClass);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="departmentId"></param>
        private delegate void GetDepartmentClassesFromServiceDelegate(Guid departmentId);
        private void GetDepartmentClassesFromService(Guid departmentId)
        {
            try
            {
                CACCCheckInDb.Classes classes = null;

                logger.Debug("Retrieving GetClassesByDeptId from CACCCheckInServiceProxy");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    classes = new CACCCheckInDb.Classes(proxy.GetClassesByDeptId(departmentId));
                    classes.CollectionChanged +=
                        new NotifyCollectionChangedEventHandler(ClassesCollectionChanged);
                    classes.ItemEndEdit +=
                        new CACCCheckInDb.ItemEndEditEventHandler(ClassesItemEndEdit);
                }

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

        /// <summary>
        /// Whenever classes are added or removed from the collection, 
        /// we will get notified here and can handle the event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ClassesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        // A new Class has started being added to the Class list.
                        // We will set some default values and hook up some
                        // event handler so we can capture if they 
                        // actually save the new Class.
                        foreach (CACCCheckInDb.Class aClass in e.NewItems)
                        {
                            aClass.Id = Guid.NewGuid();
                            aClass.Name = "<Enter Class Name>";
                            aClass.Description = "<Enter Class Description>";
                            aClass.DeptId = View.CurrentDepartment.Id;
                            aClass.ItemEndEdit +=
                                new CACCCheckInDb.ItemEndEditEventHandler(ClassesItemEndEdit);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        // Classes have been removed from the Class list.
                        // We will delete the Class.
                        foreach (CACCCheckInDb.Class aClass in e.OldItems)
                        {
                            logger.DebugFormat("Deleting class [{0}]", aClass.Name);

                            DeleteClass(aClass);
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
        /// Whenever a Class in the collection has had its EndEdit 
        /// </summary>
        /// <param name="sender"></param>
        private void ClassesItemEndEdit(IEditableObject sender)
        {
            try
            {
                // Get the Class that has been edited. This could be a new
                // class or an edit to existing Class.
                CACCCheckInDb.Class updatedClass = sender as CACCCheckInDb.Class;

                // If the RowTimestamp is null, this must be brand new Class 
                // that hasn't been added to the database yet. If not, this
                // must be an update to existing class
                if (updatedClass.RowTimestamp == null)
                {
                    logger.DebugFormat("Adding class [{0}]", updatedClass.Name);

                    AddClass(updatedClass);

                    logger.DebugFormat("Class [{0}] was added.", updatedClass.Name);
                }
                else
                {
                    logger.Debug("Class is being updated.");

                    UpdateClass(updatedClass);

                    logger.DebugFormat("Class [{0}] was updated.", updatedClass.Name);
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

        #endregion Private Functions
    }
}
