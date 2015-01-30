using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Admin.Views
{
    /// <summary>
    /// Interaction logic for PersonsView.xaml
    /// </summary>
    public partial class PersonsView : UserControl, IPersonsView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private PersonsPresenter _presenter;
        private IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;
        private ICollectionView _departmentsView;
        private ICollectionView _classesView;
        private ICollectionView _peopleView;
        private ICollectionView _familyView;
        private DataGridRow _currentPersonEditingRow;
        private DataGridRow _currentFamilyEditingRow;

        public const string ViewName = "PersonsView";

        public PersonsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The constructor for the ClassesView. Wires up all the necessary
        /// objects and events needed by this View.
        /// </summary>
        /// <param name="presenter"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="configurationService"></param>
        public PersonsView(PersonsPresenter presenter, IEventAggregator eventAggregator,
            IConfigurationService configurationService) : this()
        {
            _presenter = presenter;
            _presenter.View = this;
            _eventAggregator = eventAggregator;
            _configurationService = configurationService;

            logger.Debug("Subscribing to ShellRefreshRequestedEvent.");

            ShellRefreshRequestedEvent shellRefreshRequestedEvent = _eventAggregator.GetEvent<ShellRefreshRequestedEvent>();
            shellRefreshRequestedEvent.Subscribe(OnShellRefreshRequested, ThreadOption.UIThread);

            // Hook into the CanExecuteEvent of the DataGrid so that we can look for 
            // when user tries to Delete
            PeopleDataGrid.AddHandler(CommandManager.CanExecuteEvent,
                new CanExecuteRoutedEventHandler(OnCanExecuteRoutedEventHandler), true);

            // Hook into the CanExecuteEvent of the DataGrid so that we can look for 
            // when user tries to Delete
            FamilyDataGrid.AddHandler(CommandManager.CanExecuteEvent,
                new CanExecuteRoutedEventHandler(OnCanExecuteRoutedEventHandler), true);
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
                
                GetDataForView();

                UpdateShellStatusBar("People administration view loaded");
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
                logger.Debug("ShellRefreshRequestedEvent was fired. Refreshing data.");

                GetDataForView();
            }
            catch (Exception ex)
            {
                DisplayExceptionDetail(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetDataForView()
        {
            // Only want to allow refreshing data for View
            // when not in edit mode. Just exit if editing.
            if (IsInFamilyEditMode || IsInPeopleEditMode)
            {
                return;
            }

            CloseErrorDetail();

            FamilyDetail.DataContext = null;
            FamilyDataGrid.ItemsSource = null;

            ShowProcessing(true);

            logger.Debug("Getting list of departments for ComboBox.");
            _presenter.GetDepartments();
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DepartmentsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != DepartmentsComboBox.SelectedItem)
            {
                ShowProcessing(true);

                CurrentDepartment = (CACCCheckInDb.Department)DepartmentsComboBox.SelectedItem;

                _presenter.GetDataForClassCombo(CurrentDepartment.Id);
            }
        }

        /// <summary>
        /// Retrieves people in selected department and class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClassesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowProcessing(true);

            CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)DepartmentsComboBox.SelectedItem;
            if (null == selectedDepartment) { return; }
            CACCCheckInDb.Class selectedClass = (CACCCheckInDb.Class)ClassesComboBox.SelectedItem;
            if (null == selectedClass)
            {
                PeopleDataGrid.ItemsSource = null;
                return;
            }
            else
            {
                CurrentClass = selectedClass;
            }

            logger.Debug("Getting list of people for department and class.");
            _presenter.GetPeopleByDeptIdAndClassId(selectedDepartment.Id, selectedClass.Id);
        }

        #region IPersonsView Members

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
                return (List<CACCCheckInDb.Department>)_departmentsView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _departmentsView = CollectionViewSource.GetDefaultView(value);                
                _departmentsView.SortDescriptions.Add(new SortDescription("Name",
                    ListSortDirection.Ascending));

                string targetDeptName = _configurationService.GetTargetDepartment();
                CACCCheckInDb.Department targetDept = ((List<CACCCheckInDb.Department>)
                    _departmentsView.SourceCollection).Find(d => d.Name.Equals(targetDeptName));

                if (null == targetDept)
                {
                    _departmentsView.MoveCurrentToFirst();
                }
                else
                {
                    _departmentsView.MoveCurrentTo(targetDept);
                }

                DepartmentsComboBox.ItemsSource = _departmentsView;
                DepartmentsComboBox.SelectedIndex = _departmentsView.CurrentPosition;

                ShowProcessing(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CACCCheckInDb.Class> ClassDataContext
        {
            get
            {
                return (List<CACCCheckInDb.Class>)_classesView.SourceCollection;
            }
            set
            {
                CloseErrorDetail();

                _classesView = CollectionViewSource.GetDefaultView(value);
                ClassesComboBox.ItemsSource = _classesView;
                _classesView.SortDescriptions.Add(new SortDescription("Name",
                    ListSortDirection.Ascending));

                _classesView.MoveCurrentToFirst();
                ClassesComboBox.SelectedIndex = _classesView.CurrentPosition;

                ShowProcessing(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public CACCCheckInDb.People PeopleDataContext
        {
            get
            {
                return (CACCCheckInDb.People)_peopleView.SourceCollection;
            }
            set
            {
                try
                {
                    _peopleView = CollectionViewSource.GetDefaultView(value);
                    ((IEditableCollectionView)_peopleView).NewItemPlaceholderPosition =
                        NewItemPlaceholderPosition.AtBeginning;
                    PeopleDataGrid.ItemsSource = _peopleView;
                    if (null != _peopleView)
                    {
                        _peopleView.SortDescriptions.Add(new SortDescription("LastName",
                            ListSortDirection.Ascending));
                        _peopleView.SortDescriptions.Add(new SortDescription("FirstName",
                            ListSortDirection.Ascending));

                        _peopleView.MoveCurrentToFirst();
                        Dispatcher.BeginInvoke(DispatcherPriority.Background,
                            new DispatcherOperationCallback(
                                delegate(object arg)
                                {
                                    try
                                    {
                                        if (_peopleView.CurrentPosition >= 0)
                                        { PeopleDataGrid.SelectedIndex = _peopleView.CurrentPosition; }
                                        if (null != _peopleView.CurrentItem)
                                        { PeopleDataGrid.ScrollIntoView(_peopleView.CurrentItem); }
                                    }
                                    catch (NullReferenceException ex)
                                    {
                                        logger.Error("ScrollIntoView Exception: ", ex);
                                    }

                                    return null;
                                }), null);
                    }
                }
                finally
                {
                    ShowProcessing(false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public CACCCheckInDb.People FamilyDataContext
        {
            get
            {
                return (CACCCheckInDb.People)_familyView.SourceCollection;
            }
            set
            {
                try
                {
                    _familyView = CollectionViewSource.GetDefaultView(value);
                    FamilyDataGrid.ItemsSource = _familyView;
                    if (null != _familyView)
                    {
                        _familyView.SortDescriptions.Add(new SortDescription("LastName",
                            ListSortDirection.Ascending));
                        _familyView.SortDescriptions.Add(new SortDescription("FirstName",
                            ListSortDirection.Ascending));

                        _familyView.MoveCurrentToFirst();
                        Dispatcher.BeginInvoke(DispatcherPriority.Background,
                            new DispatcherOperationCallback(
                                delegate(object arg)
                                {
                                    try
                                    {
                                        if (_familyView.CurrentPosition >= 0)
                                        { FamilyDataGrid.SelectedIndex = _familyView.CurrentPosition; }
                                        if (null != _familyView.CurrentItem)
                                        { FamilyDataGrid.ScrollIntoView(_familyView.CurrentItem); }
                                    }
                                    catch (NullReferenceException ex)
                                    {
                                        logger.Error("ScrollIntoView Exception: ", ex);
                                    }                                   

                                    return null;
                                }), null);
                    }
                }
                finally
                {
                    ShowProcessing(false);
                }
            }
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

        /// <summary>
        /// This property allows access to the currently selected Department
        /// so that Presenter can use to add new people.
        /// </summary>
        public CACCCheckInDb.Department CurrentDepartment
        {
            get;
            private set;
        }

        /// <summary>
        /// This property allows access to the currently selected Class
        /// so that Presenter can use to add new people.
        /// </summary>
        public CACCCheckInDb.Class CurrentClass
        {
            get;
            private set;
        }

        /// <summary>
        /// Whenever a person is finished being added, we will scroll to that
        /// person in the grid after updating the RowTimestamp
        /// </summary>
        /// <param name="person"></param>
        public void PersonInsertCompleted(CACCCheckInDb.Person person)
        {
            // Look for the person that was just inserted in in the people list.
            CACCCheckInDb.Person existingPerson =
                        PeopleDataContext.FirstOrDefault(p => p.Id.Equals(person.Id));
            
            existingPerson.RowTimestamp = person.RowTimestamp;

            _peopleView.MoveCurrentTo(existingPerson);

            try
            {
                if (null != _peopleView.CurrentItem)
                { PeopleDataGrid.ScrollIntoView(_peopleView.CurrentItem); }
            }
            catch (NullReferenceException ex)
            {
                logger.Error("ScrollIntoView Exception: ", ex);
            }           
        }

        #endregion

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

        /// <summary>
        /// We handle the RoutedCommand CanExecuteEvent which is triggered by the DataGrid
        /// Interested in the Delete command so that we can ask for permission to delete
        /// the Class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
      
            RoutedCommand routedCommand = (e.Command as RoutedCommand);
            if (routedCommand != null)
            {
                if (routedCommand.Name == "Delete")
                {
                    CloseErrorDetail();

                    if (grid.Name.Equals("PeopleDataGrid"))
                    {
                        if (IsInPeopleEditMode) return;

                        CACCCheckInDb.Person person = _peopleView.CurrentItem as CACCCheckInDb.Person;
                        string deleteMessage = String.Format("You have selected the person [{0} {1}] for deletion? Click Yes to delete or No to cancel.",
                            person.FirstName, person.LastName);
                        
                        if (MessageBoxResult.No == MessageBox.Show(deleteMessage, "Delete Person", MessageBoxButton.YesNo,
                            MessageBoxImage.Question, MessageBoxResult.No))
                        {
                            e.CanExecute = false;
                            e.Handled = true;
                        }
                    }
                    else if (grid.Name.Equals("FamilyDataGrid"))
                    {
                        if (IsInFamilyEditMode) return;

                        CACCCheckInDb.Person person = _familyView.CurrentItem as CACCCheckInDb.Person;
                        string deleteMessage = String.Format("You have selected the person [{0} {1}] for removal from family? Click Yes to remove or No to cancel.",
                            person.FirstName, person.LastName);

                        if (MessageBoxResult.No == MessageBox.Show(deleteMessage, "Remove Person From Family", MessageBoxButton.YesNo,
                            MessageBoxImage.Question, MessageBoxResult.No))
                        {
                            e.CanExecute = false;
                            e.Handled = true;
                        }
                        else
                        {
                            // To remove someone from Family, just change 
                            // the FamilyId to something new. This will essentially
                            // disassociate them from the Family.
                            person.FamilyId = Guid.NewGuid();
                            PeopleDataGrid.SelectedItem = person;                            
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeopleDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //if (e.Row.Item == CollectionView.NewItemPlaceholder)
            //{
            //    e.Row.Header = "*";
            //}
            //else
            //{
            //    e.Row.Header = null;
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeopleDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel)
            {
                // cancelling the entire row will reset the state
                IsInPeopleEditMode = false;
            }
            else if (e.EditAction == DataGridEditAction.Commit)
            {
                e.Cancel = IsInPeopleEditMode;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FamilyDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel)
            {
                // cancelling the entire row will reset the state
                IsInFamilyEditMode = false;
            }
            else if (e.EditAction == DataGridEditAction.Commit)
            {
                e.Cancel = IsInFamilyEditMode;
            }
        }

        /// <summary>
        /// Handle the BeginningEdit event so we can possibly prevent editing
        /// or limit to current row being edited
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeopleDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            // The intention here is to prevent edit mode when double-clicking
            // We only want to allow edit mode when F2 is pressed.
            if (e.EditingEventArgs != null)
            {
                // When attempting edit by double-clicking or entering data, we can detect by looking
                // at the RoutedEvent. If MouseLeftButtonDown or TextInput, we will cancel the edit
                if (e.EditingEventArgs.RoutedEvent.Name.Equals("MouseLeftButtonDown") ||
                    e.EditingEventArgs.RoutedEvent.Name.Equals("TextInput"))
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (!IsInPeopleEditMode)
            {
                // set edit mode state
                IsInPeopleEditMode = true;
                _currentPersonEditingRow = e.Row;
            }
            else if (e.Row != _currentPersonEditingRow)
            {
                // cancel all new edits for different rows
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Handle the BeginningEdit event so we can possibly prevent editing
        /// or limit to current row being edited
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FamilyDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            // The intention here is to prevent edit mode when double-clicking
            // We only want to allow edit mode when F2 is pressed.
            if (e.EditingEventArgs != null)
            {
                // When attempting edit by double-clicking or entering data, we can detect by looking
                // at the RoutedEvent. If MouseLeftButtonDown or TextInput, we will cancel the edit
                if (e.EditingEventArgs.RoutedEvent.Name.Equals("MouseLeftButtonDown") ||
                    e.EditingEventArgs.RoutedEvent.Name.Equals("TextInput"))
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (!IsInFamilyEditMode)
            {
                // set edit mode state
                IsInFamilyEditMode = true;
                _currentFamilyEditingRow = e.Row;
            }
            else if (e.Row != _currentFamilyEditingRow)
            {
                // cancel all new edits for different rows
                e.Cancel = true;
            }
        }

        /// <summary>
        /// We will look at keys pressed and take row out of edit mode
        /// only if the user hits Enter key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeopleDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGridRow row = DataGridHelpers.GetRow(PeopleDataGrid,
                PeopleDataGrid.Items.IndexOf(PeopleDataGrid.CurrentItem));

            if (row == null) return;

            if (row.IsEditing && e.Key == Key.Enter)
            {
                // only 'Enter' key on an editing row will allow a commit to occur
                IsInPeopleEditMode = false;
            }
        }

        /// <summary>
        /// We will look at keys pressed and take row out of edit mode
        /// only if the user hits Enter key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FamilyDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGridRow row = DataGridHelpers.GetRow(FamilyDataGrid,
                FamilyDataGrid.Items.IndexOf(FamilyDataGrid.CurrentItem));

            if (row == null) return;

            if (row.IsEditing && e.Key == Key.Enter)
            {
                // only 'Enter' key on an editing row will allow a commit to occur
                IsInFamilyEditMode = false;
            }
        }

        #region EditingRow DependencyProperties

        /// <summary>
        /// Will be true when PeopleDataGrid is in edit mode
        /// </summary>
        public bool IsInPeopleEditMode
        {
            get { return (bool)GetValue(PeopleEditingRowProperty); }
            set { SetValue(PeopleEditingRowProperty, value); }
        }

        /// <summary>
        /// Represents the IsInPeopleEditMode property.
        /// </summary>
        public static readonly DependencyProperty PeopleEditingRowProperty =
            DependencyProperty.Register(
                "IsInPeopleEditMode",
                typeof(bool),
                typeof(PersonsView),
                new FrameworkPropertyMetadata(false, null, null));

        /// <summary>
        /// Will be true when FamilyDataGrid is in edit mode
        /// </summary>
        public bool IsInFamilyEditMode
        {
            get { return (bool)GetValue(FamilyEditingRowProperty); }
            set { SetValue(FamilyEditingRowProperty, value); }
        }

        /// <summary>
        /// Represents the IsInFamilyEditMode property.
        /// </summary>
        public static readonly DependencyProperty FamilyEditingRowProperty =
            DependencyProperty.Register(
                "IsInFamilyEditMode",
                typeof(bool),
                typeof(PersonsView),
                new FrameworkPropertyMetadata(false, null, null));

        #endregion EditingRow DependencyProperties

        /// <summary>
        /// When the DataContext changes for FamilyDetail, we will retrieve the 
        /// Family members for the current Person
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FamilyDetail_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            if (e.NewValue == CollectionView.NewItemPlaceholder)
            {
                FamilyDataGrid.ItemsSource = null;
                return;
            }

            ShowProcessing(true);

            CACCCheckInDb.Person currentPerson = e.NewValue as CACCCheckInDb.Person;
            _presenter.GetFamilyForPerson(currentPerson);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeopleDataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            if (e.Row.Item is CACCCheckInDb.Person)
            {
                CACCCheckInDb.Person person =
                    (CACCCheckInDb.Person)e.Row.Item;

                if (String.IsNullOrEmpty(person.SpecialConditions))
                {
                    e.DetailsElement.Style = null;
                }
                else
                {
                    Style hasSpecialConditions = this.FindResource("HasSpecialConditions") as Style;
                    e.DetailsElement.Style = hasSpecialConditions;
                }
            }
        }

        /// <summary>
        /// Adds the selected Person to the current Family list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddToFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == PeopleDataGrid.SelectedItem) return;

            CACCCheckInDb.Person selectedPerson = PeopleDataGrid.SelectedItem as CACCCheckInDb.Person;

            if (!FamilyDataContext.Contains(selectedPerson))
            {
                Guid currentFamilyId = ((CACCCheckInDb.Person)FamilyDetail.DataContext).FamilyId.Value;
                selectedPerson.FamilyId = currentFamilyId;

                FamilyDataContext.Add(selectedPerson);
            }            
        }

        /// <summary>
        /// Retrieves the Family members for the currently selected Person
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RetrieveFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == PeopleDataGrid.SelectedItem) return;
            FamilyDetail.DataContext = PeopleDataGrid.SelectedItem as CACCCheckInDb.Person;
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

            if (IsInPeopleEditMode)
            {
                logger.Debug("IsInPeopleEditMode is true during unload. Canceling edit.");

                ((IEditableCollectionView)_peopleView).CancelEdit();
                PeopleDataGrid.CancelEdit();
                IsInPeopleEditMode = false;
            } 
            
            if (IsInFamilyEditMode)
            {
                logger.Debug("IsInFamilyEditMode is true during unload. Canceling edit.");
                ((IEditableCollectionView)_familyView).CancelEdit();
                FamilyDataGrid.CancelEdit();
                IsInFamilyEditMode = false;
            }
        }
    }
}
