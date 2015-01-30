using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Admin.Views
{
    /// <summary>
    /// Interaction logic for ClassMovementView.xaml
    /// </summary>
    public partial class ClassMovementView : UserControl, IClassMovementView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ClassMovementPresenter _presenter;
        private IEventAggregator _eventAggregator;        
        private readonly IConfigurationService _configurationService;
        private ICollectionView _departmentFromComboBoxView;
        private ICollectionView _departmentToComboBoxView;
        private ICollectionView _classFromComboBoxView;
        private ICollectionView _classToComboBoxView;
        private ICollectionView _classFromListView;
        private ICollectionView _classToListView;

        public const string ViewName = "ClassMovementView";

        public ClassMovementView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The constructor for the ClassMovementView. Wires up all the necessary
        /// objects and events needed by this View.
        /// </summary>
        /// <param name="presenter"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="configurationService"></param>
        public ClassMovementView(ClassMovementPresenter presenter, IEventAggregator eventAggregator,
            IConfigurationService configurationService) : this()
        {
            _presenter = presenter;
            _presenter.View = this;
            _eventAggregator = eventAggregator;
            _configurationService = configurationService;

            logger.Debug("Subscribing to ShellRefreshRequestedEvent.");

            ShellRefreshRequestedEvent shellRefreshRequestedEvent = _eventAggregator.GetEvent<ShellRefreshRequestedEvent>();
            shellRefreshRequestedEvent.Subscribe(OnShellRefreshRequested, ThreadOption.UIThread);
        }

        /// <summary>
        /// Performs actions when this UserControl loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Debug("UserControl loading.");

            UpdateShellStatusBar("Class movement view loaded.");

            GetDataForView();
        }

        /// <summary>
        /// Whenever the ShellRefreshRequestedEvent fires, this will handle the event.
        /// </summary>
        /// <param name="args"></param>
        public void OnShellRefreshRequested(EventArgs args)
        {
            logger.Debug("ShellRefreshRequestedEvent was fired. Refreshing data.");

            GetDataForView();
        }

        /// <summary>
        /// Retrieves data used in View
        /// </summary>
        private void GetDataForView()
        {
            CloseErrorDetail();

            ChangeStateOfMovementButtons(false);

            ShowProcessing(true);

            logger.Debug("Getting list of departments for ComboBox.");
            _presenter.GetDataForDepartmentCombos();
        }

        /// <summary>
        /// Sends updated string to Shell status bar
        /// </summary>
        /// <param name="message"></param>
        public void UpdateShellStatusBar(string message)
        {
            StatusBarUpdateEventArgs eventArgs = new StatusBarUpdateEventArgs();
            eventArgs.StatusMessage = message;
            _eventAggregator.GetEvent<ShellStatusBarUpdateEvent>().Publish(eventArgs);
        }

        /// <summary>
        /// Hides the ErrorDetailView panel.
        /// </summary>
        private void CloseErrorDetail()
        {
            ErrorDetailView.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// If user clicks the close button on the ErrorDetailView panel, this will
        /// handle the event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorDetailCloseButton_Click(object sender, RoutedEventArgs e)
        {
            GetDataForView();
        }

        /// <summary>
        /// Turns the processing control on and off. This shows the user
        /// that activity is happening and stops any further activity until
        /// complete
        /// </summary>
        /// <param name="isContentProcessing"></param>
        private void ShowProcessing(bool isContentProcessing)
        {
            processingControl.IsContentProcessing = isContentProcessing;

            if (!isContentProcessing) { UpdateShellStatusBar(String.Empty); }
        }

        #region IClassMovementView Members

        /// <summary>
        /// This property allows access to the View Dispatcher object so that Presenter
        /// can update UI from different threads.
        /// </summary>
        public Dispatcher ViewDispatcher
        {
            get { return this.Dispatcher; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CACCCheckInDb.Department> DepartmentFromDataContext
        {
            get
            {
                return (List<CACCCheckInDb.Department>)_departmentFromComboBoxView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _departmentFromComboBoxView = CollectionViewSource.GetDefaultView(value);
                _departmentFromComboBoxView.SortDescriptions.Add(new SortDescription("Name",
                    ListSortDirection.Ascending));
                
                string targetDeptName = _configurationService.GetTargetDepartment();
                CACCCheckInDb.Department targetDept = ((List<CACCCheckInDb.Department>)
                    _departmentFromComboBoxView.SourceCollection).Find(d => d.Name.Equals(targetDeptName));

                if (null == targetDept)
                {
                    _departmentFromComboBoxView.MoveCurrentToFirst();
                }
                else
                {                    
                    _departmentFromComboBoxView.MoveCurrentTo(targetDept);
                }

                departmentFromComboBox.ItemsSource = _departmentFromComboBoxView;                
                departmentFromComboBox.SelectedIndex = _departmentFromComboBoxView.CurrentPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CACCCheckInDb.Class> ClassFromDataContext
        {
            get
            {
                return (List<CACCCheckInDb.Class>)_classFromComboBoxView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _classFromComboBoxView = CollectionViewSource.GetDefaultView(value);
                _classFromComboBoxView.SortDescriptions.Add(new SortDescription("Name",
                    ListSortDirection.Ascending));

                _classFromComboBoxView.MoveCurrentToFirst();
                classFromComboBox.ItemsSource = _classFromComboBoxView;
                classFromComboBox.SelectedIndex = _classFromComboBoxView.CurrentPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CACCCheckInDb.Department> DepartmentToDataContext
        {
            get
            {
                return (List<CACCCheckInDb.Department>)_departmentToComboBoxView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _departmentToComboBoxView = CollectionViewSource.GetDefaultView(value);
                _departmentToComboBoxView.SortDescriptions.Add(new SortDescription("Name",
                    ListSortDirection.Ascending));

                string targetDeptName = _configurationService.GetTargetDepartment();
                CACCCheckInDb.Department targetDept = ((List<CACCCheckInDb.Department>)
                    _departmentToComboBoxView.SourceCollection).Find(d => d.Name.Equals(targetDeptName));

                if (null == targetDept)
                {
                    _departmentToComboBoxView.MoveCurrentToFirst();
                }
                else
                {
                    _departmentToComboBoxView.MoveCurrentTo(targetDept);
                }

                departmentToComboBox.ItemsSource = _departmentToComboBoxView;               
                departmentToComboBox.SelectedIndex = _departmentToComboBoxView.CurrentPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CACCCheckInDb.Class> ClassToDataContext
        {
            get
            {
                return (List<CACCCheckInDb.Class>)_classToComboBoxView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _classToComboBoxView = CollectionViewSource.GetDefaultView(value);
                _classToComboBoxView.SortDescriptions.Add(new SortDescription("Name",
                    ListSortDirection.Ascending));

                _classToComboBoxView.MoveCurrentToFirst();
                classToComboBox.ItemsSource = _classToComboBoxView;
                classToComboBox.SelectedIndex = _classToComboBoxView.CurrentPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView> ClassFromListDataContext
        {
            get
            {
                return (ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView>)_classFromListView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _classFromListView = CollectionViewSource.GetDefaultView(value);
                ClassFromList.ItemsSource = _classFromListView;
                _classFromListView.SortDescriptions.Add(new SortDescription("LastName",
                    ListSortDirection.Ascending));
                _classFromListView.SortDescriptions.Add(new SortDescription("FirstName",
                    ListSortDirection.Ascending));

                _classFromListView.MoveCurrentToFirst();
                Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            try
                            {
                                if (_classFromListView.CurrentPosition >= 0)
                                { ClassFromList.SelectedIndex = _classFromListView.CurrentPosition; }
                                if (null != _classFromListView.CurrentItem)
                                { ClassFromList.ScrollIntoView(_classFromListView.CurrentItem); }
                            }
                            catch (NullReferenceException ex)
                            {
                                logger.Error("ScrollIntoView Exception: ", ex);
                            }

                            return null;
                        }), null);

                _classFromListView.CollectionChanged +=
                    new NotifyCollectionChangedEventHandler(_classFromListView_CollectionChanged);

                ShowProcessing(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _classFromListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CACCCheckInDb.Department selectedFromDepartment = (CACCCheckInDb.Department)departmentFromComboBox.SelectedItem;
            CACCCheckInDb.Class selectedFromClass = (CACCCheckInDb.Class)classFromComboBox.SelectedItem;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // New members have been added to this class. We will update the class, department
                    // details for this person and then update the person in the database.
                    // This will effectively remove the person from the previous class.
                    foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in e.NewItems)
                    {
                        logger.DebugFormat("Adding [{0} {1}] to class [{2}]",
                            person.FirstName, person.LastName, selectedFromClass.Name);

                        _presenter.UpdatePersonClassMembership(person, selectedFromClass);

                        person.DepartmentId = selectedFromDepartment.Id;
                        person.DepartmentName = selectedFromDepartment.Name;
                        person.ClassId = selectedFromClass.Id;
                        person.ClassName = selectedFromClass.Name;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in e.OldItems)
                    {
                        logger.DebugFormat("Removing [{0} {1}] from class [{2}]",
                            person.FirstName, person.LastName, selectedFromClass.Name);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView> ClassToListDataContext
        {
            get
            {
                return (ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView>)_classToListView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _classToListView = CollectionViewSource.GetDefaultView(value);
                ClassToList.ItemsSource = _classToListView;
                _classToListView.SortDescriptions.Add(new SortDescription("LastName",
                    ListSortDirection.Ascending));
                _classToListView.SortDescriptions.Add(new SortDescription("FirstName",
                    ListSortDirection.Ascending));

                _classToListView.MoveCurrentToFirst();
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            try
                            {
                                if (_classToListView.CurrentPosition >= 0)
                                { ClassToList.SelectedIndex = _classToListView.CurrentPosition; }
                                if (null != _classToListView.CurrentItem)
                                { ClassToList.ScrollIntoView(_classToListView.CurrentItem); }
                            }
                            catch (NullReferenceException ex)
                            {
                                logger.Error("ScrollIntoView Exception: ", ex);
                            }

                            return null;
                        }), null);

                _classToListView.CollectionChanged += 
                    new NotifyCollectionChangedEventHandler(_classToListView_CollectionChanged);

                ShowProcessing(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _classToListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CACCCheckInDb.Department selectedToDepartment = (CACCCheckInDb.Department)departmentToComboBox.SelectedItem;
            CACCCheckInDb.Class selectedToClass = (CACCCheckInDb.Class)classToComboBox.SelectedItem;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // New members have been added to this class. We will update the class, department
                    // details for this person and then update the person in the database.
                    // This will effectively remove the person from the previous class.
                    foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in e.NewItems)
                    {
                        logger.DebugFormat("Adding [{0} {1}] to class [{2}]",
                            person.FirstName, person.LastName, selectedToClass.Name);

                        _presenter.UpdatePersonClassMembership(person, selectedToClass);

                        person.DepartmentId = selectedToDepartment.Id;
                        person.DepartmentName = selectedToDepartment.Name;
                        person.ClassId = selectedToClass.Id;
                        person.ClassName = selectedToClass.Name;                        
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in e.OldItems)
                    {
                        logger.DebugFormat("Removing [{0} {1}] from class [{2}]",
                            person.FirstName, person.LastName, selectedToClass.Name);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }

        /// <summary>
        /// This function will take the passed Exception and display its
        /// details in the ErrorDetailView panel.
        /// </summary>
        /// <param name="ex"></param>
        public void DisplayExceptionDetail(Exception ex)
        {
            // Stop the processing animation
            ShowProcessing(false);

            string errorMessage = ex.Message;
            if (null != ex.InnerException)
            {
                errorMessage = errorMessage + " :: " + ex.InnerException.Message;
            }

            ErrorText.Text = errorMessage;
            ErrorDetailView.Visibility = Visibility.Visible;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void departmentFromComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)departmentFromComboBox.SelectedItem;
            if (null == selectedDepartment) { return; }
            _presenter.GetDataForClassCombo(selectedDepartment.Id, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void departmentToComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)departmentToComboBox.SelectedItem;
            if (null == selectedDepartment) { return; }
            _presenter.GetDataForClassCombo(selectedDepartment.Id, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void classFromComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)departmentFromComboBox.SelectedItem;
            if (null == selectedDepartment) { return; }
            CACCCheckInDb.Class selectedClass = (CACCCheckInDb.Class)classFromComboBox.SelectedItem;
            if (null == selectedClass)
            {
                ClassFromList.ItemsSource = null;
                return;
            }
            _presenter.GetDataForClassList(selectedDepartment.Name, selectedClass.Name, true);

            if (null != _classToComboBoxView && null != _classToComboBoxView.CurrentItem)
            {
                if (((CACCCheckInDb.Class)_classToComboBoxView.CurrentItem).Id == selectedClass.Id)
                {
                    ChangeStateOfMovementButtons(false);
                }
                else
                {
                    ChangeStateOfMovementButtons(true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enable"></param>
        private void ChangeStateOfMovementButtons(bool enable)
        {
            MovementButtonGrid.IsEnabled = enable;
            MoveAllRightButton.IsEnabled = enable;
            MoveSelectedRightButton.IsEnabled = enable;
            MoveSelectedLeftButton.IsEnabled = enable;
            MoveAllLeftButton.IsEnabled = enable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void classToComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)departmentToComboBox.SelectedItem;
            if (null == selectedDepartment) { return; }
            CACCCheckInDb.Class selectedClass = (CACCCheckInDb.Class)classToComboBox.SelectedItem;
            if (null == selectedClass)
            {
                ClassToList.ItemsSource = null;
                return;
            } 
            _presenter.GetDataForClassList(selectedDepartment.Name, selectedClass.Name, false);

            if (null != _classFromComboBoxView && null != _classFromComboBoxView.CurrentItem)
            {
                if (((CACCCheckInDb.Class)_classFromComboBoxView.CurrentItem).Id == selectedClass.Id)
                {
                    ChangeStateOfMovementButtons(false);
                }
                else
                {
                    ChangeStateOfMovementButtons(true);
                }
            }
        }

        /// <summary>
        /// Will take all the current people in the class on left and move them to
        /// the class on the right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveAllRightButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowProcessing(true);

                // Make a copy of the current collection of people in the class on left
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> allPeople =
                     ClassFromListDataContext.ToList();

                // Loop through all the people in class on left and (1) remove them from class
                // on the left and (2) add them to class on right
                foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in allPeople)
                {
                    (ClassFromListDataContext).Remove(person);
                    (ClassToListDataContext).Add(person);
                }

                // Clear class on left to make sure no items left
                ClassFromListDataContext.Clear();

                // Refresh collection views of both left and right classes
                _classFromListView.Refresh();
                _classToListView.Refresh();
            }
            catch (Exception ex)
            {
                DisplayExceptionDetail(ex);
            }
            finally
            {
                ShowProcessing(false);
            }
        }

        /// <summary>
        /// Will take all the selected people in the class on left and move them to
        /// the class on the right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveSelectedRightButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowProcessing(true);

                // Make a copy of the current collection of selected people in the class on left
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> selectedPeople =
                    new List<CACCCheckInDb.PeopleWithDepartmentAndClassView>(
                        ClassFromList.SelectedItems.Cast<CACCCheckInDb.PeopleWithDepartmentAndClassView>());

                // Loop through all the selected people in class on left and (1) remove them from class
                // on the left and (2) add them to class on right
                foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in selectedPeople)
                {
                    (ClassFromListDataContext).Remove(person);
                    (ClassToListDataContext).Add(person);
                }

                // Refresh collection views of both left and right classes
                _classFromListView.Refresh();
                _classToListView.Refresh();
            }
            catch (Exception ex)
            {
                DisplayExceptionDetail(ex);
            }
            finally
            {
                ShowProcessing(false);
            }
        }

        /// <summary>
        /// Will take all the selected people in the class on right and move them to
        /// the class on the left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveSelectedLeftButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowProcessing(true);

                // Make a copy of the current collection of selected people in the class on right
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> selectedPeople =
                    new List<CACCCheckInDb.PeopleWithDepartmentAndClassView>(
                        ClassToList.SelectedItems.Cast<CACCCheckInDb.PeopleWithDepartmentAndClassView>());

                // Loop through all the selected people in class on right and (1) remove them from class
                // on the right and (2) add them to class on left
                foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in selectedPeople)
                {
                    (ClassToListDataContext).Remove(person);
                    (ClassFromListDataContext).Add(person);
                }

                // Refresh collection views of both left and right classes
                _classFromListView.Refresh();
                _classToListView.Refresh();
            }
            catch (Exception ex)
            {
                DisplayExceptionDetail(ex);
            }
            finally
            {
                ShowProcessing(false);
            }
        }

        /// <summary>
        /// Will take all the current people in the class on right and move them to
        /// the class on the left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveAllLeftButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowProcessing(true);

                // Make a copy of the current collection of people in the class on right
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> allPeople =
                     ClassToListDataContext.ToList();

                // Loop through all the people in class on right and (1) remove them from class
                // on the right and (2) add them to class on left
                foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in allPeople)
                {
                    (ClassToListDataContext).Remove(person);
                    (ClassFromListDataContext).Add(person);
                }

                // Clear class on right to make sure no items left
                ClassToListDataContext.Clear();

                // Refresh collection views of both left and right classes
                _classFromListView.Refresh();
                _classToListView.Refresh();
            }
            catch (Exception ex)
            {
                DisplayExceptionDetail(ex);
            }
            finally
            {
                ShowProcessing(false);
            }
        }
    }
}
