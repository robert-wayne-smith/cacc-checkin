using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class ClassMovementPresenter
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties

        public IClassMovementView View { get; set; }

        #endregion Properties

        #region Constructors

        public ClassMovementPresenter()
        {
        }

        #endregion Constructors

        /// <summary>
        /// Will retrieve the departments for the department combo boxes
        /// </summary>
        public void GetDataForDepartmentCombos()
        {
            GetDataForDepartmentCombosFromServiceDelegate fetcher =
                new GetDataForDepartmentCombosFromServiceDelegate(GetDataForDepartmentCombosFromService);

            fetcher.BeginInvoke(null, null);
        }

        /// <summary>
        /// Will retrieve the classes for the class combo boxes
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="fromCombo"></param>
        public void GetDataForClassCombo(Guid departmentId, bool fromCombo)
        {
            GetDataForClassComboFromServiceDelegate fetcher =
                new GetDataForClassComboFromServiceDelegate(GetDataForClassComboFromService);

            fetcher.BeginInvoke(departmentId, fromCombo, null, null);
        }

        /// <summary>
        /// Will retrieve the class members for the class lists
        /// </summary>
        /// <param name="departmentName"></param>
        /// <param name="className"></param>
        /// <param name="fromList"></param>
        public void GetDataForClassList(string departmentName, string className, bool fromList)
        {
            GetDataForClassListFromServiceDelegate fetcher =
                new GetDataForClassListFromServiceDelegate(GetDataForClassListFromService);

            fetcher.BeginInvoke(departmentName, className, fromList, null, null);
        }

        /// <summary>
        /// Will move a person from current class to new class
        /// </summary>
        /// <param name="person"></param>
        /// <param name="newClass"></param>
        public void UpdatePersonClassMembership(CACCCheckInDb.PeopleWithDepartmentAndClassView person,
            CACCCheckInDb.Class newClass)
        {            
            try
            {
                logger.DebugFormat("Updating class membership for [{0} {1}]",
                    person.FirstName, person.LastName);

                // Get a reference to ClassMember record for current person
                CACCCheckInDb.ClassMember oldClassMember = (CACCCheckInDb.ClassMember)person;
                // Create a new ClassMember record to move current person to new class
                CACCCheckInDb.ClassMember newClassMember = new CACCCheckInDb.ClassMember();
                // The new ClassId is used along with current ClassRole and PersonId
                newClassMember.ClassId = newClass.Id;
                newClassMember.ClassRole = oldClassMember.ClassRole;
                newClassMember.PersonId = oldClassMember.PersonId;
                
                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    logger.Debug("Calling DeleteClassMember in CACCCheckInService");
                    proxy.DeleteClassMember(oldClassMember);

                    logger.Debug("Calling InsertClassMember in CACCCheckInService");
                    proxy.InsertClassMember(newClassMember);
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
        
        #region Private Functions

        /// <summary>
        /// 
        /// </summary>
        private delegate void GetDataForDepartmentCombosFromServiceDelegate();
        private void GetDataForDepartmentCombosFromService()
        {
            try
            {
                List<CACCCheckInDb.Department> departmentsFrom = null;
                List<CACCCheckInDb.Department> departmentsTo = null;

                logger.Debug("Retrieving GetAllFromDepartment from CACCCheckInService");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    departmentsFrom = proxy.GetAllFromDepartment();
                }

                departmentsTo = new List<CACCCheckInDb.Department>(departmentsFrom);
                
                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.DepartmentFromDataContext = departmentsFrom;
                            View.DepartmentToDataContext = departmentsTo;
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
        /// <param name="fromCombo"></param>
        private delegate void GetDataForClassComboFromServiceDelegate(Guid departmentId, bool fromCombo);
        private void GetDataForClassComboFromService(Guid departmentId, bool fromCombo)
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
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            if (fromCombo)
                            { View.ClassFromDataContext = classes; }
                            else
                            { View.ClassToDataContext = classes; }
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
        /// <param name="departmentName"></param>
        /// <param name="className"></param>
        /// <param name="fromList"></param>
        private delegate void GetDataForClassListFromServiceDelegate(string departmentName, string className, bool fromList);
        private void GetDataForClassListFromService(string departmentName, string className, bool fromList)
        {
            try
            {
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people = null;

                logger.Debug("Retrieving GetPeopleWithDepartmentAndClassByDepartmentAndClass from CACCCheckInService");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    people = proxy.GetPeopleWithDepartmentAndClassByDepartmentAndClass(
                        departmentName, className);
                }

                logger.Debug("Filtering people to only show Class Members and NOT Teachers.");
                var filteredPeople = new ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView>(
                    (from p in people
                     where p.ClassRole.Equals(ClassRoles.Member)
                     select p).ToList());

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            if (fromList)
                            { View.ClassFromListDataContext = filteredPeople; }
                            else
                            { View.ClassToListDataContext = filteredPeople; }
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
