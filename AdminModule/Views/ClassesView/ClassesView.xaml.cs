using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ClassesView.xaml
    /// </summary>
    public partial class ClassesView : UserControl, IClassesView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ClassesPresenter _presenter;
        private IEventAggregator _eventAggregator;        
        private readonly IConfigurationService _configurationService;
        private ICollectionView _departmentsView;
        private ICollectionView _departmentClassesView;
        private DataGridRow _currentEditingRow;

        public const string ViewName = "ClassesView";

        public ClassesView()
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
        public ClassesView(ClassesPresenter presenter, IEventAggregator eventAggregator,
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
            ClassesDataGrid.AddHandler(CommandManager.CanExecuteEvent,
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

                UpdateShellStatusBar("Classes administration view loaded");
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
            if (IsInEditMode)
            {
                return;
            }

            CloseErrorDetail();

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
                CurrentDepartment = (CACCCheckInDb.Department)DepartmentsComboBox.SelectedItem;

                Guid departmentId = CurrentDepartment.Id;
                _presenter.GetDepartmentClasses(departmentId);
            }
        }

        #region IClassesView Members

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
        public CACCCheckInDb.Classes ClassesDataContext
        {
            get
            {
                return (CACCCheckInDb.Classes)_departmentClassesView.SourceCollection;
            }
            set
            {
                _departmentClassesView = CollectionViewSource.GetDefaultView(value);
                ClassesDataGrid.ItemsSource = _departmentClassesView;
                _departmentClassesView.SortDescriptions.Add(new SortDescription("Name",
                    ListSortDirection.Ascending));

                _departmentClassesView.MoveCurrentToFirst();
                Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            try
                            {
                                if (_departmentClassesView.CurrentPosition >= 0)
                                { ClassesDataGrid.SelectedIndex = _departmentClassesView.CurrentPosition; }
                                if (null != _departmentClassesView.CurrentItem)
                                { ClassesDataGrid.ScrollIntoView(_departmentClassesView.CurrentItem); }
                            }
                            catch (NullReferenceException ex)
                            {
                                logger.Error("ScrollIntoView Exception: ", ex);
                            }

                            return null;
                        }), null);
            }
        }

        /// <summary>
        /// This function will take the passed Exception and display its
        /// details in the ErrorDetailView panel.
        /// </summary>
        /// <param name="ex"></param>
        public void DisplayExceptionDetail(Exception ex)
        {
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
        /// This property allows access to the currently selected Department
        /// so that Presenter can use to add new classes.
        /// </summary>
        public CACCCheckInDb.Department CurrentDepartment
        {
            get;
            private set;
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
            RoutedCommand routedCommand = (e.Command as RoutedCommand);
            if (routedCommand != null)
            {
                if (routedCommand.Name == "Delete")
                {
                    if (IsInEditMode) return;

                    CloseErrorDetail();

                    CACCCheckInDb.Class aClass = _departmentClassesView.CurrentItem as CACCCheckInDb.Class;
                    string deleteMessage = String.Format("You have selected the [{0}] class for deletion? Click Yes to delete or No to cancel.",
                        aClass.Name);
                    if (MessageBoxResult.No == MessageBox.Show(deleteMessage, "Delete Class", MessageBoxButton.YesNo,
                        MessageBoxImage.Question, MessageBoxResult.No))
                    {
                        e.CanExecute = false;
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClassesDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item == CollectionView.NewItemPlaceholder)
            {
                e.Row.Header = "*";
            }
            else
            {
                e.Row.Header = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClassesDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel)
            {
                // cancelling the entire row will reset the state
                IsInEditMode = false;
            }
            else if (e.EditAction == DataGridEditAction.Commit)
            {
                e.Cancel = IsInEditMode;
            }
        }

        /// <summary>
        /// Handle the BeginningEdit event so we can possibly prevent editing
        /// or limit to current row being edited
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClassesDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
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

            if (!IsInEditMode)
            {
                // set edit mode state
                IsInEditMode = true;
                _currentEditingRow = e.Row;
            }
            else if (e.Row != _currentEditingRow)
            {
                // cancel all new edits for different rows
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClassesDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGridRow row = DataGridHelpers.GetRow(ClassesDataGrid,
                ClassesDataGrid.Items.IndexOf(ClassesDataGrid.CurrentItem));

            if (row == null) return;

            if (row.IsEditing && e.Key == Key.Enter)
            {
                // only 'Enter' key on an editing row will allow a commit to occur
                IsInEditMode = false;
            }
        }

        #region EditingRow DependencyProperty

        /// <summary>
        /// 
        /// </summary>
        public bool IsInEditMode
        {
            get { return (bool)GetValue(EditingRowProperty); }
            set { SetValue(EditingRowProperty, value); }
        }

        /// <summary>
        /// Represents the IsInEditMode property.
        /// </summary>
        public static readonly DependencyProperty EditingRowProperty =
            DependencyProperty.Register(
                "IsInEditMode",
                typeof(bool),
                typeof(ClassesView),
                new FrameworkPropertyMetadata(false, null, null));

        #endregion EditingRow DependencyProperty

        /// <summary>
        /// Whenever the View is unloaded, we want to cancel any edit operation
        /// that is in progress.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            logger.Debug("UserControl unloading.");

            if (IsInEditMode)
            {
                logger.Debug("IsInEditMode is true during unload. Canceling edit.");

                ((IEditableCollectionView)_departmentClassesView).CancelEdit();
                ClassesDataGrid.CancelEdit();
                IsInEditMode = false;
            }
        }
    }
}
