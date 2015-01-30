using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Admin.Views
{
    /// <summary>
    /// Interaction logic for ClassAttendanceCountDuringDateRangeReportView.xaml
    /// </summary>
    public partial class ClassAttendanceCountDuringDateRangeReportView : UserControl, IReportPresenterView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ReportsPresenter _presenter;
        private IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;
        private ICollectionView _departmentComboBoxView;
        private ICollectionView _classComboBoxView;

        public ClassAttendanceCountDuringDateRangeReportView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The constructor for the ClassAttendanceCountDuringDateRangeReportView.
        /// Wires up all the necessary objects and events needed by this View.
        /// </summary>
        /// <param name="presenter"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="configurationService"></param>
        public ClassAttendanceCountDuringDateRangeReportView(ReportsPresenter presenter,
            IEventAggregator eventAggregator, IConfigurationService configurationService) : this()
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
            getReportButton.IsEnabled = false;
            SaveReportButton.IsEnabled = false;
            PrintReportButton.IsEnabled = false;

            _presenter.GetDepartments();
            AttendanceStartDate.SelectedDate = DateTime.Today;
            AttendanceEndDate.SelectedDate = DateTime.Today;
        }
        
        /// <summary>
        /// Whenever the ShellRefreshRequestedEvent fires, this will handle the event.
        /// </summary>
        /// <param name="args"></param>
        public void OnShellRefreshRequested(EventArgs args)
        {
            logger.Debug("ShellRefreshRequestedEvent was fired. Refreshing departments.");

            CloseErrorDetail();

            getReportButton.IsEnabled = false;
            SaveReportButton.IsEnabled = false;
            PrintReportButton.IsEnabled = false;

            _presenter.GetDepartments();
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
            CloseErrorDetail();
        }

        /// <summary>
        /// Handles when user changes department selection. Will retreive correct classes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DepartmentsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)DepartmentsComboBox.SelectedItem;
            if (null == selectedDepartment) { return; }
            _presenter.GetDepartmentClasses(selectedDepartment.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getReportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveReportButton.IsEnabled = false;
            PrintReportButton.IsEnabled = false;

            CACCCheckInDb.Department selectedDepartment =
                (CACCCheckInDb.Department)DepartmentsComboBox.SelectedItem;

            CACCCheckInDb.Class selectedClass =
                (CACCCheckInDb.Class)ClassComboBox.SelectedItem;

            _presenter.GetClassAttendanceInDateRange(selectedDepartment.Id, selectedClass.Id,
                AttendanceStartDate.SelectedDate, AttendanceEndDate.SelectedDate);
        }

        #region IReportPresenterView Members

        /// <summary>
        /// 
        /// </summary>
        public System.Windows.Threading.Dispatcher ViewDispatcher
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

                DepartmentsComboBox.ItemsSource = _departmentComboBoxView;
                DepartmentsComboBox.SelectedIndex = _departmentComboBoxView.CurrentPosition;
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
                _classComboBoxView = CollectionViewSource.GetDefaultView(value);
                _classComboBoxView.SortDescriptions.Add(new SortDescription("Name",
                    ListSortDirection.Ascending));

                _classComboBoxView.MoveCurrentToFirst();
                ClassComboBox.ItemsSource = _classComboBoxView;
                ClassComboBox.SelectedIndex = _classComboBoxView.CurrentPosition;

                getReportButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> DepartmentPeopleDataContext
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<CACCCheckInDb.AttendanceWithDetail> ReportDataContext
        {
            set
            {
                ReportDocument.Document = GetFlowDocument(value);
                SaveReportButton.IsEnabled = true;
                PrintReportButton.IsEnabled = true;
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

        #endregion

        /// <summary>
        /// Creates the report FlowDocument using the attendance records
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        private FlowDocument GetFlowDocument(List<CACCCheckInDb.AttendanceWithDetail> records)
        {
            const int TableColumnCount = 2;

            DateTime startDate = AttendanceStartDate.SelectedDate.Value;
            DateTime endDate = AttendanceEndDate.SelectedDate.Value;

            FlowDocument flowDoc = new FlowDocument();
            flowDoc.Background = this.FindResource("ReportBackground") as Brush;

            Table table = new Table();

            table.Columns.Add(new TableColumn() { Width = GridLength.Auto });
            table.Columns.Add(new TableColumn());

            // Create and add an empty TableRowGroup to hold the table's Rows.
            table.RowGroups.Add(new TableRowGroup());

            // To be used to alias the current working row for easy reference.
            TableRow currentRow;

            string reportTitle = "Class Attendance Counts";
            
            if (startDate.Equals(endDate))
            {
                reportTitle = String.Format("Class Attendance Counts On {0}",
                    startDate.ToShortDateString());
            }
            else
            {
                reportTitle = String.Format("Class Attendance Counts From {0} to {1}",
                    startDate.ToShortDateString(), endDate.ToShortDateString()); 
            }

            // Add the cross image row
            currentRow = new TableRow();
            Image crossImage = new Image();
            crossImage.Stretch = Stretch.Fill;
            crossImage.Source = new BitmapImage(new
                Uri("pack://application:,,,/Infrastructure;component/images/TCACC_Logo_CrossOnly.png"));
            crossImage.Height = 50;
            crossImage.Width = 50;

            currentRow.Cells.Add(new TableCell(new BlockUIContainer(crossImage)));
            currentRow.Cells[0].ColumnSpan = TableColumnCount;
            table.RowGroups[0].Rows.Add(currentRow);

            // Add the title row
            currentRow = new TableRow();
            currentRow.Cells.Add(new TableCell(new Paragraph(new Underline(new Run(reportTitle)))));
            currentRow.Cells[0].ColumnSpan = TableColumnCount;
            currentRow.Cells[0].Style = this.FindResource("ReportTitle") as Style;
            table.RowGroups[0].Rows.Add(currentRow);

            // Add the 'created on' row
            currentRow = new TableRow();
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Created on " + DateTime.Today.ToShortDateString()))));
            currentRow.Cells[0].FontSize = 14;
            currentRow.Cells[0].TextAlignment = TextAlignment.Center;
            currentRow.Cells[0].ColumnSpan = TableColumnCount;
            table.RowGroups[0].Rows.Add(currentRow);

            currentRow = new TableRow();
            currentRow.Cells.Add(new TableCell());
            currentRow.Cells[0].ColumnSpan = TableColumnCount;
            currentRow.Cells[0].Padding = new Thickness(0, 0, 0, 10);
            table.RowGroups[0].Rows.Add(currentRow);

            if (0 == records.Count)
            {
                currentRow = new TableRow();
                currentRow.Cells.Add(new TableCell(new Paragraph(
                    new Run("No records found matching criteria"))));
                currentRow.Cells[0].ColumnSpan = TableColumnCount;
                currentRow.Cells[0].TextAlignment = TextAlignment.Center;
                table.RowGroups[0].Rows.Add(currentRow);
            }
            else
            {
                TableRowGroup tableRowGroup = new TableRowGroup();

                // Create and add an empty TableRowGroup to hold the table's Rows.
                table.RowGroups.Add(tableRowGroup);

                // Add the table header row
                currentRow = new TableRow();
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Date"))));
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Attendees"))));
                currentRow.Cells[0].Style = this.FindResource("ReportTableHeader") as Style;
                currentRow.Cells[1].Style = this.FindResource("ReportTableHeader") as Style;
                tableRowGroup.Rows.Add(currentRow);

                // Add the line under table header row
                currentRow = new TableRow();
                currentRow.Cells.Add(new TableCell());
                currentRow.Cells[0].ColumnSpan = TableColumnCount;
                currentRow.Cells[0].BorderThickness = new Thickness(0, 1, 0, 0);
                currentRow.Cells[0].BorderBrush = Brushes.Maroon;
                tableRowGroup.Rows.Add(currentRow);

                // Group the records by class and order by class name
                IEnumerable<IGrouping<string, CACCCheckInDb.AttendanceWithDetail>> classGroups =
                    records.GroupBy(r => r.ClassName).OrderBy(r => r.Key);

                bool firstGroup = true;

                // Loop through each grouping of class
                foreach (IGrouping<string, CACCCheckInDb.AttendanceWithDetail> classGroup in classGroups)
                {
                    tableRowGroup = new TableRowGroup();

                    // Create and add an empty TableRowGroup to hold the table's Rows.
                    table.RowGroups.Add(tableRowGroup);

                    currentRow = new TableRow();
                    Paragraph p = new Paragraph();
                    p.Inlines.Add(new Run("Class: "));
                    p.Inlines.Add(new Bold(new Run(classGroup.Key)));
                    currentRow.Cells.Add(new TableCell(p));
                    currentRow.Cells[0].ColumnSpan = TableColumnCount;
                    currentRow.Cells[0].Style = this.FindResource("ReportTableHeader") as Style;
                    if (firstGroup)
                    {
                        currentRow.Cells[0].Blocks.FirstBlock.Margin = new Thickness(0, 5, 0, 5);
                        firstGroup = false;
                    }
                    else
                    {
                        currentRow.Cells[0].Blocks.FirstBlock.Margin = new Thickness(0, 20, 0, 5);
                    }
                    tableRowGroup.Rows.Add(currentRow);

                    // Group the class records by date
                    IEnumerable<IGrouping<DateTime, CACCCheckInDb.AttendanceWithDetail>> dateGroups =
                        classGroup.GroupBy(r => r.Date).OrderByDescending(r => r.Key);

                    foreach (IGrouping<DateTime, CACCCheckInDb.AttendanceWithDetail> dateGroup in dateGroups)
                    {
                        currentRow = new TableRow();
                        currentRow.Cells.Add(new TableCell(new Paragraph(new Run(dateGroup.Key.ToShortDateString()))));
                        currentRow.Cells.Add(new TableCell(new Paragraph(new Run(dateGroup.Count().ToString()))));

                        tableRowGroup.Rows.Add(currentRow);
                    }
                }
            }

            flowDoc.Blocks.Add(table);
            return flowDoc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == ReportDocument.Document) return;

            try
            {
                FlowDocumentHelpers.SaveReport(ReportDocument.Document);
            }
            catch (Exception ex)
            {
                DisplayExceptionDetail(ex);
            }
        }
    }
}
