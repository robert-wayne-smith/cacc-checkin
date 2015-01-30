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
    public class PersonsPresenter
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties

        public IPersonsView View { get; set; }

        #endregion Properties

        #region Constructors

        public PersonsPresenter()
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
        /// 
        /// </summary>
        /// <param name="departmentId"></param>
        public void GetDataForClassCombo(Guid departmentId)
        {
            GetDataForClassComboFromServiceDelegate fetcher =
                new GetDataForClassComboFromServiceDelegate(GetDataForClassComboFromService);

            fetcher.BeginInvoke(departmentId, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetPeopleByDeptIdAndClassId(Guid departmentId, Guid classId)
        {
            GetPeopleByDeptIdAndClassIdFromServiceDelegate fetcher =
                new GetPeopleByDeptIdAndClassIdFromServiceDelegate(GetPeopleByDeptIdAndClassIdFromService);

            fetcher.BeginInvoke(departmentId, classId, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="person"></param>
        public void GetFamilyForPerson(CACCCheckInDb.Person person)
        {
            GetFamilyFromServiceDelegate fetcher =
                new GetFamilyFromServiceDelegate(GetFamilyFromService);

            fetcher.BeginInvoke(person, null, null);
        }

        #region Private Functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classMember"></param>
        public void InsertClassMember(CACCCheckInDb.ClassMember classMember)
        {
            using (CACCCheckInServiceProxy proxy =
               new CACCCheckInServiceProxy())
            {
                proxy.Open();

                proxy.InsertClassMember(classMember);
            }
        }

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

        /// <summary>
        /// 
        /// </summary>
        private delegate void GetPeopleByDeptIdAndClassIdFromServiceDelegate(Guid departmentId, Guid classId);
        private void GetPeopleByDeptIdAndClassIdFromService(Guid departmentId, Guid classId)
        {
            try
            {
                CACCCheckInDb.People people = null;

                logger.Debug("Retrieving GetAllFromPeople from CACCCheckInService");

                using (CACCCheckInServiceProxy proxy =
                    new CACCCheckInServiceProxy())
                {
                    proxy.Open();

                    people = new CACCCheckInDb.People(proxy.GetPeopleByDeptIdAndClassId(
                        departmentId, classId));
                    people.CollectionChanged += 
                        new NotifyCollectionChangedEventHandler(PeopleCollectionChanged);
                    people.ItemEndEdit +=
                        new CACCCheckInDb.ItemEndEditEventHandler(PeopleItemEndEdit);
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
        /// Whenever people are added or removed from the collection, 
        /// we will get notified here and can handle the event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PeopleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        // A new Person has started being added to the People list.
                        // We will set some default values and hook up some
                        // event handler so we can capture if they 
                        // actually save the new Person.    
                        foreach (CACCCheckInDb.Person person in e.NewItems)
                        {
                            CACCCheckInDb.Person thePerson = (CACCCheckInDb.Person)person;
                            person.Id = Guid.NewGuid();
                            person.FirstName = "<Enter First Name>";
                            person.LastName = "<Enter Last Name>";
                            person.FamilyId = Guid.NewGuid();
                            person.PhoneNumber = String.Empty;
                            person.ItemEndEdit +=
                                new CACCCheckInDb.ItemEndEditEventHandler(PeopleItemEndEdit);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        // People have been removed from the People list.
                        // We will delete the Person.
                        foreach (CACCCheckInDb.Person person in e.OldItems)
                        {
                            logger.DebugFormat("Deleting person [{0} {1}]",
                                person.FirstName, person.LastName);

                            CACCCheckInDb.People.DeletePerson(person);
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FamilyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        // People have been added to the Family list.
                        // We will update FamilyId to add the Person
                        // to Family.
                        foreach (CACCCheckInDb.Person person in e.NewItems)
                        {
                            logger.DebugFormat("Updating person [{0} {1}] FamilyId",
                                person.FirstName, person.LastName);

                            CACCCheckInDb.People.UpdatePerson(person);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        // People have been removed from the Family list.
                        // We will update FamilyId to separate the Person
                        // from Family.
                        foreach (CACCCheckInDb.Person person in e.OldItems)
                        {
                            logger.DebugFormat("Updating person [{0} {1}] FamilyId",
                                person.FirstName, person.LastName);
                            
                            CACCCheckInDb.People.UpdatePerson(person);
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
        /// Whenever a Person in the collection has had its EndEdit 
        /// </summary>
        /// <param name="sender"></param>
        private void PeopleItemEndEdit(IEditableObject sender)
        {
            try
            {
                // Get the Person that has been edited. This could be a new
                // class or an edit to existing Person.
                CACCCheckInDb.Person updatedPerson = sender as
                    CACCCheckInDb.Person;

                // If the RowTimestamp is null, this must be brand new Person 
                // that hasn't been added to the database yet. If not, this
                // must be an update to existing Person
                if (updatedPerson.RowTimestamp == null)
                {
                    logger.DebugFormat("Adding person [{0} {1}]",
                        updatedPerson.FirstName, updatedPerson.LastName);

                    updatedPerson = CACCCheckInDb.People.InsertPerson(updatedPerson);

                    View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.PersonInsertCompleted(updatedPerson);
                            return null;
                        }), null);

                    CACCCheckInDb.ClassMember newClassMember = new CACCCheckInDb.ClassMember();
                    newClassMember.ClassId = View.CurrentClass.Id;
                    newClassMember.PersonId = updatedPerson.Id;
                    newClassMember.ClassRole = ClassRoles.Member;
                    InsertClassMember(newClassMember);

                    logger.DebugFormat("Person [{0} {1}] was added.", 
                        updatedPerson.FirstName, updatedPerson.LastName);
                }
                else
                {
                    logger.Debug("Person is being updated.");

                    CACCCheckInDb.People.UpdatePerson(updatedPerson);

                    logger.DebugFormat("Person [{0} {1}] was updated.",
                        updatedPerson.FirstName, updatedPerson.LastName);
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
        /// <param name="person"></param>
        private delegate void GetFamilyFromServiceDelegate(CACCCheckInDb.Person person);
        private void GetFamilyFromService(CACCCheckInDb.Person person)
        {
            try
            {
                CACCCheckInDb.People people = null;

                if (person != null)
                {
                    logger.Debug("Retrieving GetPeopleByFamilyId from CACCCheckInService");

                    using (CACCCheckInServiceProxy proxy =
                        new CACCCheckInServiceProxy())
                    {
                        proxy.Open();

                        if (person.FamilyId.HasValue)
                        {
                            people = new CACCCheckInDb.People(
                                proxy.GetPeopleByFamilyId(person.FamilyId.Value));

                            people.CollectionChanged +=
                                new NotifyCollectionChangedEventHandler(FamilyCollectionChanged);
                            people.ItemEndEdit +=
                                new CACCCheckInDb.ItemEndEditEventHandler(PeopleItemEndEdit);
                        }
                    }
                }

                Debug.Assert(View != null);
                View.ViewDispatcher.BeginInvoke(DispatcherPriority.DataBind,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            View.FamilyDataContext = people;
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
