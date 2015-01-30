using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;
using System.Threading;
using System.Globalization;

namespace CACCCheckIn.Modules.Admin.Views
{
    /// <summary>
    /// Interaction logic for FamilyQuickEntryView.xaml
    /// </summary>
    public partial class FamilyQuickEntryView : UserControl, IFamilyQuickEntryView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private FamilyQuickEntryPresenter _presenter;
        private IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;
        private ICollectionView _departmentComboBoxView;
        private ICollectionView _classComboBoxView;
        private ICollectionView _familyView;
        private List<CACCCheckInDb.PeopleWithDepartmentAndClassView> _familyMembers =
            new List<CACCCheckInDb.PeopleWithDepartmentAndClassView>();
        private List<Family> _allFamilyMembers = new List<Family>();
        private bool _waitingForAcknowledgement = false;

        public const string ViewName = "FamilyQuickEntryView";

        public FamilyQuickEntryView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The constructor for the FamilyQuickEntryView. Wires up all the necessary
        /// objects and events needed by this View.
        /// </summary>
        /// <param name="presenter"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="configurationService"></param>
        public FamilyQuickEntryView(FamilyQuickEntryPresenter presenter, IEventAggregator eventAggregator,
            IConfigurationService configurationService) : this()
        {
            try
            {
                _presenter = presenter;
                _presenter.View = this;
                _eventAggregator = eventAggregator;
                _configurationService = configurationService;

                logger.Debug("Subscribing to ShellRefreshRequestedEvent.");

                ShellRefreshRequestedEvent shellRefreshRequestedEvent = _eventAggregator.GetEvent<ShellRefreshRequestedEvent>();
                shellRefreshRequestedEvent.Subscribe(OnShellRefreshRequested, ThreadOption.UIThread);
            }
            catch (Exception ex)
            {
                DisplayExceptionDetail(ex);
            }
        }

        /// <summary>
        /// Performs actions when this UserControl loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Debug("UserControl loading.");

                if (!_waitingForAcknowledgement)
                {
                    CompletedPanel.Visibility = Visibility.Collapsed;
                    GetDataForView();
                    InitializeFamilyDataGrid();
                }
                
                UpdateShellStatusBar("Family Quick Entry view loaded.");
            }
            catch (Exception ex)
            {
                DisplayExceptionDetail(ex);
            }
        }

        /// <summary>
        /// Whenever the ShellRefreshRequestedEvent fires, this will handle the event.
        /// </summary>
        /// <param name="args"></param>
        public void OnShellRefreshRequested(EventArgs args)
        {
            try
            {
                logger.Debug("ShellRefreshRequestedEvent was fired.");

                GetDataForView();
            }
            catch (Exception ex)
            {
                DisplayExceptionDetail(ex);
            }
        }

        /// <summary>
        /// Retrieves data used in View
        /// </summary>
        private void GetDataForView()
        {
            CloseErrorDetail();

            ShowProcessing(true);

            FamilyRole.SelectedIndex = 0;

            logger.Debug("Getting list of departments.");
            _presenter.GetDataForDepartmentCombos();

            _presenter.GetAllFamiliesInDepartment(_configurationService.GetTargetDepartment());
        }

        /// <summary>
        /// This will initialize the Family DataGrid for use by the View
        /// </summary>
        private void InitializeFamilyDataGrid()
        {
            _familyView = CollectionViewSource.GetDefaultView(_familyMembers);
            _familyView.SortDescriptions.Add(new SortDescription("LastName",
                ListSortDirection.Ascending));
            _familyView.SortDescriptions.Add(new SortDescription("FirstName",
                ListSortDirection.Ascending));
            _familyView.MoveCurrentToFirst();

            FamilyDataGrid.ItemsSource = _familyView;            
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

        #region IFamilyQuickEntryView Members

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
        public List<CACCCheckInDb.Department> DepartmentsDataContext
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

                DepartmentComboBox.ItemsSource = _departmentComboBoxView;
                DepartmentComboBox.SelectedIndex = _departmentComboBoxView.CurrentPosition;

                ShowProcessing(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CACCCheckInDb.Class> DepartmentClassesDataContext
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
                ClassComboBox.ItemsSource = _classComboBoxView;
                ClassComboBox.SelectedIndex = _classComboBoxView.CurrentPosition;

                ShowProcessing(false);
            }
        }

        public List<Family> FamiliesDataContext
        { 
            get
            {
                return _allFamilyMembers;    
            }
            
            set
            {
                _allFamilyMembers.Clear();
                _allFamilyMembers.AddRange(value);
            }
        }

        /// <summary>
        /// After a person has been checked in, we will find that person in the current
        /// family list and then set the security code to one assigned during check in.
        /// </summary>
        /// <param name="person"></param>
        public void CheckInCompleted(CACCCheckInDb.PeopleWithDepartmentAndClassView personCheckedIn)
        {
            // Look for the person that was checked in in the family list.
            CACCCheckInDb.PeopleWithDepartmentAndClassView person =
                        _familyMembers.Find(p => p.PersonId.Equals(personCheckedIn.PersonId));

            // If person is found, update the security code of person in family list
            if (person != null)
            {
                person.SecurityCode = personCheckedIn.SecurityCode;
            }
        }

        /// <summary>
        /// To be called when check-in process is totally complete
        /// </summary>
        public void ProcessingCompleted()
        {
            // Stop the processing animation
            ShowProcessing(false);

            AddRefreshPanel.Visibility = Visibility.Collapsed;
            CompletedPanel.Visibility = Visibility.Visible;
            _waitingForAcknowledgement = true;

            DoubleAnimation widthAnimation = new DoubleAnimation();
            widthAnimation.AutoReverse = true;
            widthAnimation.From = 50;
            widthAnimation.To = 60;
            widthAnimation.Duration = TimeSpan.FromSeconds(1);
            widthAnimation.RepeatBehavior = RepeatBehavior.Forever;
            CompleteOkButton.BeginAnimation(Button.WidthProperty, widthAnimation);

            SaveButtonsPanel.IsEnabled = false;
        }

        /// <summary>
        /// This function will take the passed Exception and display its
        /// details in the ErrorDetailView panel.
        /// </summary>
        /// <param name="ex"></param>
        public void DisplayExceptionDetail(Exception ex)
        {
            logger.Error("Exception: ", ex);

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
        /// Update the form when valid values are present in the controls.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void FamilySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            Family currentFamily = e.AddedItems[0] as Family;

            _familyMembers.Clear();
            _familyMembers.AddRange(currentFamily.Members);
            _familyView.Refresh();
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
            //processingControl.IsContentProcessing = isContentProcessing;

            if (!isContentProcessing) { UpdateShellStatusBar(String.Empty); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="departmentId"></param>
        private void ChangeDepartmentComboTo(Guid departmentId)
        {
            CACCCheckInDb.Department targetDept = ((List<CACCCheckInDb.Department>)
                _departmentComboBoxView.SourceCollection).Find(d => d.Id.Equals(departmentId));

            if (null == targetDept)
            {
                _departmentComboBoxView.MoveCurrentToFirst();
            }
            else
            {
                _departmentComboBoxView.MoveCurrentTo(targetDept);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classId"></param>
        private void ChangeClassComboTo(Guid classId)
        {
            CACCCheckInDb.Class targetClass = ((List<CACCCheckInDb.Class>)
                _classComboBoxView.SourceCollection).Find(c => c.Id.Equals(classId));

            if (null == targetClass)
            {
                _classComboBoxView.MoveCurrentToFirst();
            }
            else
            {
                _classComboBoxView.MoveCurrentTo(targetClass);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DepartmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)DepartmentComboBox.SelectedItem;
            if (null == selectedDepartment) { return; }
            _presenter.GetDataForClassCombo(selectedDepartment.Id);
        }

        /// <summary>
        /// Will add 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFamilyMembersButton_Click(object sender, RoutedEventArgs e)
        {
            ShowProcessing(true);

            _presenter.SaveAndCheckInPeople(_familyMembers, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAndPrintLabelsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowProcessing(true);

            _presenter.SaveAndCheckInPeople(_familyMembers, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FamilyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                CACCCheckInDb.PeopleWithDepartmentAndClassView person =
                   (CACCCheckInDb.PeopleWithDepartmentAndClassView)e.AddedItems[0];
                PeopleDetail.DataContext = person;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeopleDetail_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            CACCCheckInDb.PeopleWithDepartmentAndClassView person =
                e.NewValue as CACCCheckInDb.PeopleWithDepartmentAndClassView;

            if (person == null)
            { 
                return;
            }

            ChangeDepartmentComboTo(person.DepartmentId);

            FirstName.Text = person.FirstName;
            LastName.Text = person.LastName;
            PhoneNumber.Text = person.PhoneNumber;
            SpecialConditions.Text = person.SpecialConditions;
            FamilyRole.SelectedItem = person.FamilyRole;

            ClassComboBox.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            ChangeClassComboTo(person.ClassId);

                            return null;
                        }), null);
        }

        /// <summary>
        /// Will add the current person to the family members list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddToFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            Guid? currentFamilyId;

            if (null != PeopleDetail.DataContext)
            {
                CACCCheckInDb.PeopleWithDepartmentAndClassView currentPerson =
                    (CACCCheckInDb.PeopleWithDepartmentAndClassView)PeopleDetail.DataContext;
                currentFamilyId = currentPerson.FamilyId;
            }
            else
            {
                currentFamilyId = Guid.NewGuid();
            }

            PhoneNumberConverter cvt = new PhoneNumberConverter();

            CACCCheckInDb.PeopleWithDepartmentAndClassView person =
                new CACCCheckInDb.PeopleWithDepartmentAndClassView();
            person.PersonId = Guid.NewGuid();
            person.FirstName = FirstName.Text;
            person.LastName = LastName.Text;
            person.PhoneNumber = (string)cvt.ConvertBack(PhoneNumber.Text,
                typeof(string), null, null);
            person.SpecialConditions = SpecialConditions.Text;
            person.DepartmentId = ((CACCCheckInDb.Department)DepartmentComboBox.SelectedItem).Id;
            person.DepartmentName = ((CACCCheckInDb.Department)DepartmentComboBox.SelectedItem).Name;
            person.ClassId = ((CACCCheckInDb.Class)ClassComboBox.SelectedItem).Id;
            person.ClassName = ((CACCCheckInDb.Class)ClassComboBox.SelectedItem).Name;
            person.ClassRole = ClassRoles.Member;
            person.FamilyId = currentFamilyId;
            person.FamilyRole = (string)FamilyRole.SelectedItem;
            
            _familyMembers.Add(person);
            PeopleDetail.DataContext = person;
            _familyView.Refresh();
        }

        /// <summary>
        /// Will refresh the data for the currently selected person
        /// in family members list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            CACCCheckInDb.PeopleWithDepartmentAndClassView person = null;

            if (null != PeopleDetail.DataContext)
            {
                CACCCheckInDb.PeopleWithDepartmentAndClassView currentPerson =
                    (CACCCheckInDb.PeopleWithDepartmentAndClassView)PeopleDetail.DataContext;
                
                person = ((List<CACCCheckInDb.PeopleWithDepartmentAndClassView>)
                    _familyView.SourceCollection).Find(p => p.PersonId.Equals(currentPerson.PersonId));
            }

            PhoneNumberConverter cvt = new PhoneNumberConverter();

            if (null != person)
            {
                person.FirstName = FirstName.Text;
                person.LastName = LastName.Text;
                person.PhoneNumber = (string)cvt.ConvertBack(PhoneNumber.Text,
                    typeof(string), null, null);
                person.SpecialConditions = SpecialConditions.Text;
                person.DepartmentId = ((CACCCheckInDb.Department)DepartmentComboBox.SelectedItem).Id;
                person.DepartmentName = ((CACCCheckInDb.Department)DepartmentComboBox.SelectedItem).Name;
                person.ClassId = ((CACCCheckInDb.Class)ClassComboBox.SelectedItem).Id;
                person.ClassName = ((CACCCheckInDb.Class)ClassComboBox.SelectedItem).Name;
                person.FamilyRole = (string)FamilyRole.SelectedItem;

                _familyView.Refresh();
            }            
        }

        /// <summary>
        /// When users acknowledges the processing is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompleteOkButton_Click(object sender, RoutedEventArgs e)
        {
            CompletedPanel.Visibility = Visibility.Collapsed;
            _waitingForAcknowledgement = false;
            AddRefreshPanel.Visibility = Visibility.Visible;
            SaveButtonsPanel.IsEnabled = true;

            ClearViewOfInfo();
        }

        /// <summary>
        /// Clears out the view of info
        /// </summary>
        private void ClearViewOfInfo()
        {
            FirstName.Text = String.Empty;
            LastName.Text = String.Empty;
            PhoneNumber.Text = String.Empty;
            SpecialConditions.Text = String.Empty;

            _departmentComboBoxView.MoveCurrentToFirst();
            _classComboBoxView.MoveCurrentToFirst();
            FamilyRole.SelectedIndex = 0;
            PeopleDetail.DataContext = null;
            _familyMembers.Clear();
            _familyView.Refresh();
        }

        /// <summary>
        /// Whenever the View is unloaded, we want to cancel any edit operation
        /// that is in progress.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            logger.Debug("UserControl unloading.");
        }
    }
}
