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
    /// Interaction logic for AttendanceView.xaml
    /// </summary>
    public partial class AttendanceView : UserControl, IAttendanceView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private AttendancePresenter _presenter;
        private IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;
        private ICollectionView _departmentsView;
        private ICollectionView _attendanceView;

        public const string ViewName = "AttendanceView";

        public AttendanceView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The constructor for the AttendanceView. Wires up all the necessary
        /// objects and events needed by this View.
        /// </summary>
        /// <param name="presenter"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="configurationService"></param>
        public AttendanceView(AttendancePresenter presenter, IEventAggregator eventAggregator,
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
            AttendanceDataGrid.AddHandler(CommandManager.CanExecuteEvent, 
                new CanExecuteRoutedEventHandler(OnCanExecuteRoutedEventHandler), true);
        }

        /// <summary>
        /// Performs actions when this UserControl loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Debug("UserControl loading.");

            GetDataForView();

            UpdateShellStatusBar("Attendance administration view loaded.");
        }

        /// <summary>
        /// Whenever the ShellRefreshRequestedEvent fires, this will handle the event.
        /// </summary>
        /// <param name="args"></param>
        public void OnShellRefreshRequested(EventArgs args)
        {
            logger.Debug("ShellRefreshRequestedEvent was fired. Refreshing records.");

            GetDataForView();
        }

        /// <summary>
        /// Retrieves data used in View
        /// </summary>
        private void GetDataForView()
        {
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
                ShowProcessing(true);

                Guid departmentId = ((CACCCheckInDb.Department)DepartmentsComboBox.SelectedItem).Id;
                _presenter.GetAttendanceByDeptId(departmentId);
            }

            UpdateShellStatusBar(String.Empty);
        }

        #region IAttendanceView Members

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
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public CACCCheckInDb.AttendanceRecords AttendanceDataContext
        {
            get
            {
                return (CACCCheckInDb.AttendanceRecords)_attendanceView.SourceCollection;
            }
            set
            { 
                _attendanceView = CollectionViewSource.GetDefaultView(value);
                AttendanceDataGrid.ItemsSource = _attendanceView;
                _attendanceView.SortDescriptions.Add(new SortDescription("Date",
                    ListSortDirection.Descending));
                _attendanceView.SortDescriptions.Add(new SortDescription("LastName",
                    ListSortDirection.Ascending));
                _attendanceView.SortDescriptions.Add(new SortDescription("FirstName",
                    ListSortDirection.Ascending));

                _attendanceView.MoveCurrentToFirst();

                Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new DispatcherOperationCallback(
                        delegate(object arg)
                        {
                            try
                            {
                                if (_attendanceView.CurrentPosition >= 0)
                                { AttendanceDataGrid.SelectedIndex = _attendanceView.CurrentPosition; }
                                if (null != _attendanceView.CurrentItem)
                                { AttendanceDataGrid.ScrollIntoView(_attendanceView.CurrentItem); }
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
        /// To be called when background processing is complete
        /// </summary>
        public void ProcessingCompleted()
        {
            // Stop the processing animation
            ShowProcessing(false);
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
                    CloseErrorDetail();
                    
                    if (null == _attendanceView.CurrentItem) return;

                    CACCCheckInDb.AttendanceWithDetail record = _attendanceView.CurrentItem as CACCCheckInDb.AttendanceWithDetail;
                    string deleteMessage = String.Format("You have selected an attendance record of [{0} {1}] for deletion? Click Yes to delete or No to cancel.",
                        record.FirstName, record.LastName);
                    if (MessageBoxResult.No == MessageBox.Show(deleteMessage, "Delete Attendance Record", MessageBoxButton.YesNo,
                        MessageBoxImage.Question, MessageBoxResult.No))
                    {
                        e.CanExecute = false;
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handle the BeginningEdit event so we can prevent editing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttendanceDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            // No editing allowed
            e.Cancel = true;
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
        /// Reprints label for selected attendance record
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReprintNameTagLabelButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == _attendanceView.CurrentItem) return;

            ShowProcessing(true);

            logger.Debug("Reprinting label for selected attendance record.");
            CACCCheckInDb.AttendanceWithDetail record = _attendanceView.CurrentItem as
                CACCCheckInDb.AttendanceWithDetail;
            _presenter.ReprintLabel(record);
        }
    }
}
