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
    public class TeachersPresenter
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ILabelPrinterService _labelPrinterService;

        #region Properties

        public ITeachersView View { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labelPrinterService"></param>
        public TeachersPresenter(ILabelPrinterService labelPrinterService)
        {
            _labelPrinterService = labelPrinterService;
        }

        #endregion Constructors
        
        public void GetDataForDepartmentCombo()
        {
            GetDataForDepartmentComboFromServiceDelegate fetcher =
                new GetDataForDepartmentComboFromServiceDelegate(GetDataForDepartmentComboFromService);

            fetcher.BeginInvoke(null, null);
        }

        public void GetDataForAdultList(string targetDepartment)
        {
            GetDataForAdultListFromServiceDelegate fetcher =
                new GetDataForAdultListFromServiceDelegate(GetDataForAdultListFromService);

            fetcher.BeginInvoke(targetDepartment, null, null);
        }
        
        public void GetDataForClassCombo(Guid departmentId)
        {
            GetDataForClassComboFromServiceDelegate fetcher =
                new GetDataForClassComboFromServiceDelegate(GetDataForClassComboFromService);

            fetcher.BeginInvoke(departmentId, null, null);
        }

        public void GetDataForTeacherList(string departmentName, string className)
        {
            GetDataForTeacherListFromServiceDelegate fetcher =
                new GetDataForTeacherListFromServiceDelegate(GetDataForTeacherListFromService);

            fetcher.BeginInvoke(departmentName, className, null, null);
        }

        public void PrintTeacherLabels(List<CACCCheckInDb.PeopleWithDepartmentAndClassView> teachers)
        {
            PrintTeacherLabelsDelegate fetcher =
                new PrintTeacherLabelsDelegate(InnerPrintTeacherLabels);

            fetcher.BeginInvoke(teachers, null, null);
        }

        public void PrintTeacherLabelsForDepartment(string departmentName)
        {
            PrintTeacherLabelsForDepartmentDelegate fetcher =
                new PrintTeacherLabelsForDepartmentDelegate(InnerPrintTeacherLabelsForDepartment);

            fetcher.BeginInvoke(departmentName, null, null);
        }

        /// <summary>
        /// Will add a person as teacher to specified class
        /// </summary>
        /// <param name="person"></param>
        /// <param name="newClass"></param>
        public void AddPersonAsTeacherToClass(CACCCheckInDb.PeopleWithDepartmentAndClassView person,
            CACCCheckInDb.Class theClass)
        {
            try
            {
                logger.DebugFormat("Updating class membership for [{0} {1}]",
                    person.FirstName, person.LastName);

                // Get a reference to ClassMember record for current person
                CACCCheckInDb.ClassMember classMember = (CACCCheckInDb.ClassMember)person;

                // The ClassId is used along with ClassRole to update ClassMember record
                classMember.ClassId = theClass.Id;
                classMember.ClassRole = ClassRoles.Teacher;

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    logger.Debug("Calling InsertClassMember in CACCCheckInService");
                    proxy.InsertClassMember(classMember);
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
        /// Will delete a person as teacher from specified class
        /// </summary>
        /// <param name="person"></param>
        /// <param name="newClass"></param>
        public void DeletePersonAsTeacherFromClass(CACCCheckInDb.PeopleWithDepartmentAndClassView person,
            CACCCheckInDb.Class theClass)
        {
            try
            {
                logger.DebugFormat("Updating class membership for [{0} {1}]",
                    person.FirstName, person.LastName);

                // Get a reference to ClassMember record for current person
                CACCCheckInDb.ClassMember classMember = (CACCCheckInDb.ClassMember)person;

                // The ClassId is used along with ClassRole to update ClassMember record
                classMember.ClassId = theClass.Id;
                classMember.ClassRole = ClassRoles.Teacher;

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    logger.Debug("Calling DeleteClassMember in CACCCheckInService");
                    proxy.DeleteClassMember(classMember);
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

        private delegate void GetDataForDepartmentComboFromServiceDelegate();
        private void GetDataForDepartmentComboFromService()
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
                            View.DepartmentDataContext = departments;
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
        private delegate void GetDataForAdultListFromServiceDelegate(string targetDepartment);
        private void GetDataForAdultListFromService(string targetDepartment)
        {
            try
            {
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people = null;

                logger.Debug("Retrieving GetPeopleWithDepartmentAndClassByDepartment from CACCCheckInService");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    if (targetDepartment.Equals(Departments.MOPS))
                    {
                        people = proxy.GetPeopleWithDepartmentAndClassByDepartmentAndClass(
                            Departments.Adult, "MOPS Adults");
                    }
                    else
                    {
                        people = proxy.GetPeopleWithDepartmentAndClassByDepartmentAndClass(
                            Departments.Adult, "Adult");
                    }
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.AdultsDataContext =
                                new ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView>(
                                    people);
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


        private delegate void GetDataForClassComboFromServiceDelegate(Guid departmentId);
        private void GetDataForClassComboFromService(Guid departmentId)
        {
            try
            {
                List<CACCCheckInDb.Class> classes = null;

                logger.Debug("Retrieving GetClassByDeptId from CACCCheckInService");

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
                            View.ClassDataContext = classes;
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

        private delegate void GetDataForTeacherListFromServiceDelegate(string departmentName, string className);
        private void GetDataForTeacherListFromService(string departmentName, string className)
        {
            List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people = null;

            logger.Debug("Retrieving GetPeopleWithDepartmentAndClassByDepartmentAndClass from CACCCheckInService");

            using (CACCCheckInServiceProxy proxy =
                new CACCCheckInServiceProxy())
            {
                proxy.Open();

                people = proxy.GetPeopleWithDepartmentAndClassByDepartmentAndClass(departmentName,
                    className);
            }

            var filteredPeople = (from p in people
                                  where p.ClassRole.Equals(ClassRoles.Teacher)
                                 select p).ToList();
                     
            Debug.Assert(View != null);
            View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                new DispatcherOperationCallback(
                    delegate(object arg)
                    {
                        View.TeachersDataContext =
                            new ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView>(
                                filteredPeople);
                        return null;
                    }), null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="people"></param>
        private delegate void PrintTeacherLabelsDelegate(List<CACCCheckInDb.PeopleWithDepartmentAndClassView> teachers);
        private void InnerPrintTeacherLabels(List<CACCCheckInDb.PeopleWithDepartmentAndClassView> teachers)
        {
            try
            {
                logger.DebugFormat("Printing labels for [{0}] teachers.",
                    teachers.Count);

                teachers.ForEach(t => t.SpecialConditions = ClassRoles.Teacher);

                _labelPrinterService.PrintLabels(null, Constants.ChurchName,
                    0, teachers);
                
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                            new DispatcherOperationCallback(
                                delegate(object arg)
                                {
                                    View.LabelPrintingCompleted();
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
        private delegate void PrintTeacherLabelsForDepartmentDelegate(string departmentName);
        private void InnerPrintTeacherLabelsForDepartment(string departmentName)
        {
            List<CACCCheckInDb.PeopleWithDepartmentAndClassView> teachers = null;

            try
            {
                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    teachers = proxy.GetPeopleWithDepartmentAndClassByDepartment(departmentName);                    
                }

                teachers = teachers.FindAll(p => p.ClassRole.Equals(ClassRoles.Teacher));

                logger.DebugFormat("Printing labels for [{0}] teachers.",
                    teachers.Count);

                teachers.ForEach(t => t.SpecialConditions = ClassRoles.Teacher);

                _labelPrinterService.PrintLabels(null, Constants.ChurchName,
                    0, teachers);

                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                            new DispatcherOperationCallback(
                                delegate(object arg)
                                {
                                    View.LabelPrintingCompleted();
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
