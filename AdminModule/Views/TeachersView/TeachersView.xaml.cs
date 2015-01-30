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
    /// Interaction logic for TeachersView.xaml
    /// </summary>
    public partial class TeachersView : UserControl, ITeachersView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private TeachersPresenter _presenter;
        private IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;
        private ICollectionView _departmentComboBoxView;
        private ICollectionView _classComboBoxView;
        private ICollectionView _adultListView;
        private ICollectionView _teacherListView;

        public const string ViewName = "TeachersView";

        public TeachersView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The constructor for the TeachersView. Wires up all the necessary
        /// objects and events needed by this View.
        /// </summary>
        /// <param name="presenter"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="configurationService"></param>
        public TeachersView(TeachersPresenter presenter, IEventAggregator eventAggregator,
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
            logger.Debug("UserControl loading. Getting departments and adults.");

            UpdateShellStatusBar("Teacher administration view loaded.");

            GetDataForView();
        }

        /// <summary>
        /// Whenever the ShellRefreshRequestedEvent fires, this will handle the event.
        /// </summary>
        /// <param name="args"></param>
        public void OnShellRefreshRequested(EventArgs args)
        {
            logger.Debug("ShellRefreshRequestedEvent was fired.");

            GetDataForView();            
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetDataForView()
        {
            CloseErrorDetail();

            ShowProcessing(true);

            logger.Debug("Getting list of departments and unassigned classes.");
            _presenter.GetDataForDepartmentCombo();
            _presenter.GetDataForAdultList(_configurationService.GetTargetDepartment());
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
        
        #region ITeachersView Members

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
        public List<CACCCheckInDb.Department> DepartmentDataContext
        {
            get
            {
                return (List<CACCCheckInDb.Department>)_departmentComboBoxView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _departmentComboBoxView = CollectionViewSource.GetDefaultView(value);
                _departmentComboBoxView.SortDescriptions.Add(new SortDescription("Name",
                    ListSortDirection.Ascending));

                string targetDeptName = _configurationService.GetTargetDepartment();
                CACCCheckInDb.Department targetDept = ((List<CACCCheckInDb.Department>)
                    _departmentComboBoxView.SourceCollection).Find(d => d.Name.Equals(targetDeptName));

                if (null == targetDept)
                {
                    _departmentComboBoxView.MoveCurrentToFirst();
                }
                else
                {
                    _departmentComboBoxView.MoveCurrentTo(targetDept);
                }

                departmentComboBox.ItemsSource = _departmentComboBoxView;
                departmentComboBox.SelectedIndex = _departmentComboBoxView.CurrentPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CACCCheckInDb.Class> ClassDataContext
        {
            get
            {
                return (List<CACCCheckInDb.Class>)_classComboBoxView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _classComboBoxView = CollectionViewSource.GetDefaultView(value);
                _classComboBoxView.SortDescriptions.Add(new SortDescription("Name",
                    ListSortDirection.Ascending));

                _classComboBoxView.MoveCurrentToFirst();
                classComboBox.ItemsSource = _classComboBoxView;
                classComboBox.SelectedIndex = _classComboBoxView.CurrentPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView> TeachersDataContext
        {
            get
            {
                return (ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView>)_teacherListView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _teacherListView = CollectionViewSource.GetDefaultView(value);
                _teacherListView.SortDescriptions.Add(new SortDescription("LastName",
                    ListSortDirection.Ascending));
                _teacherListView.SortDescriptions.Add(new SortDescription("FirstName",
                    ListSortDirection.Ascending));

                _teacherListView.MoveCurrentToFirst();
                TeacherList.ItemsSource = _teacherListView;
                Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            try
                            {
                                if (_teacherListView.CurrentPosition >= 0)
                                { TeacherList.SelectedIndex = _teacherListView.CurrentPosition; }
                                if (null != _teacherListView.CurrentItem)
                                { TeacherList.ScrollIntoView(_teacherListView.CurrentItem); }
                            }
                            catch (NullReferenceException ex)
                            {
                                logger.Error("ScrollIntoView Exception: ", ex);
                            }                            

                            return null;
                        }), null);

                _teacherListView.CollectionChanged +=
                    new NotifyCollectionChangedEventHandler(_teacherListView_CollectionChanged);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView> AdultsDataContext
        {
            get
            {
                return (ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView>)_adultListView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _adultListView = CollectionViewSource.GetDefaultView(value);
                _adultListView.SortDescriptions.Add(new SortDescription("LastName",
                    ListSortDirection.Ascending));
                _adultListView.SortDescriptions.Add(new SortDescription("FirstName",
                    ListSortDirection.Ascending));

                _adultListView.MoveCurrentToFirst();
                AdultList.ItemsSource = _adultListView;
                Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            try
                            {
                                if (_adultListView.CurrentPosition >= 0)
                                { AdultList.SelectedIndex = _adultListView.CurrentPosition; }
                                if (null != _adultListView.CurrentItem)
                                { AdultList.ScrollIntoView(_adultListView.CurrentItem); }
                            }
                            catch (NullReferenceException ex)
                            {
                                logger.Error("ScrollIntoView Exception: ", ex);
                            }                           

                            return null;
                        }), null);

                ShowProcessing(false);
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

            logger.Error("Exception: ", ex);

            string errorMessage = ex.Message;
            if (null != ex.InnerException)
            {
                errorMessage = errorMessage + " :: " + ex.InnerException.Message;
            }

            ErrorText.Text = errorMessage;
            ErrorDetailView.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Called when label printing operations have completed
        /// </summary>
        public void LabelPrintingCompleted()
        {
            logger.Debug("Teacher label printing has completed.");
            ShowProcessing(false);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _teacherListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)departmentComboBox.SelectedItem;
                CACCCheckInDb.Class selectedClass = (CACCCheckInDb.Class)classComboBox.SelectedItem;

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        // Teachers have been added to the Teacher list for selected class.
                        // We will add the teacher to the class.
                        foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in e.NewItems)
                        {
                            logger.DebugFormat("Adding teacher [{0} {1}] to class [{2}]",
                                person.FirstName, person.LastName, selectedClass.Name);

                            person.ClassId = selectedClass.Id;
                            person.ClassName = selectedClass.Name;
                            person.ClassRole = ClassRoles.Teacher;
                            person.DepartmentId = selectedDepartment.Id;
                            person.DepartmentName = selectedDepartment.Name;
                            
                            _presenter.AddPersonAsTeacherToClass(person, selectedClass);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        // Teachers have been removed from the Teacher list for selected class.
                        // We will delete the teacher from the class.
                        foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in e.OldItems)
                        {
                            logger.DebugFormat("Removing teacher [{0} {1}] from class [{2}]",
                                person.FirstName, person.LastName, selectedClass.Name);

                            _presenter.DeletePersonAsTeacherFromClass(person, selectedClass);
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
                DisplayExceptionDetail(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void departmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)departmentComboBox.SelectedItem;
            if (null == selectedDepartment) { return; }
            _presenter.GetDataForClassCombo(selectedDepartment.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void classComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)departmentComboBox.SelectedItem;
            if (null == selectedDepartment) { return; }
            CACCCheckInDb.Class selectedClass = (CACCCheckInDb.Class)classComboBox.SelectedItem;
            if (null == selectedClass)
            {
                TeacherList.ItemsSource = null;
                return;
            }

            _presenter.GetDataForTeacherList(selectedDepartment.Name, selectedClass.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enable"></param>
        private void ChangeStateOfMovementButtons(bool enable)
        {
            MovementButtonGrid.IsEnabled = enable;
            MoveSelectedRightButton.IsEnabled = enable;
            MoveSelectedLeftButton.IsEnabled = enable;
        }

        /// <summary>
        /// Will take all the selected people in the adult list on left and move them to
        /// the class teachers on the right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveSelectedRightButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowProcessing(true);

                if (null == AdultList.SelectedItems) { return; }

                // Make a copy of the current collection of selected people in the list on left
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> selectedPeople =
                    new List<CACCCheckInDb.PeopleWithDepartmentAndClassView>(
                        AdultList.SelectedItems.Cast<CACCCheckInDb.PeopleWithDepartmentAndClassView>());

                // Loop through all the selected people in list on left and 
                // (1) add them to class teachers on right if not already there
                foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in selectedPeople)
                {
                    if (0 == (TeachersDataContext).Count(p => p.PersonId.Equals(person.PersonId)))
                    { (TeachersDataContext).Add(person); }
                }

                // Refresh collection view of right list
                _teacherListView.Refresh();
            }
            finally
            {
                ShowProcessing(false);
            }

        }

        /// <summary>
        /// Will take all the selected people in the class teachers on right and move them to
        /// the adult list on the left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveSelectedLeftButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowProcessing(true);

                if (null == TeacherList.SelectedItems) { return; }

                // Make a copy of the current collection of selected people in the class teachers on right
                List<CACCCheckInDb.PeopleWithDepartmentAndClassView> selectedPeople =
                    new List<CACCCheckInDb.PeopleWithDepartmentAndClassView>(
                        TeacherList.SelectedItems.Cast<CACCCheckInDb.PeopleWithDepartmentAndClassView>());

                // Loop through all the selected people in list on right and
                // (1) remove them from class teachers on the right
                foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in selectedPeople)
                {
                    (TeachersDataContext).Remove(person);
                }

                // Refresh collection view of right list
                _teacherListView.Refresh();
            }
            finally
            {
                ShowProcessing(false);
            }
        }

        /// <summary>
        /// When user clicks the button to print the teacher labels for a class, 
        /// this will handle the event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintTeacherLabelsForClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (TeachersDataContext.Count == 0) return;

            ShowProcessing(true);

            // Make a copy of the current collection of people in the class teachers on right
            List<CACCCheckInDb.PeopleWithDepartmentAndClassView> allTeachers =
                 TeachersDataContext.ToList();

            _presenter.PrintTeacherLabels(allTeachers);
        }

        /// <summary>
        /// When user clicks the button to print the teacher labels for a department, 
        /// this will handle the event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintTeacherLabelsForDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)departmentComboBox.SelectedItem;
            if (null == selectedDepartment) { return; }

            ShowProcessing(true);

            _presenter.PrintTeacherLabelsForDepartment(selectedDepartment.Name);
        }
    }
}
