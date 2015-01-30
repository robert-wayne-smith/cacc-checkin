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
    /// Interaction logic for AttendanceRecordForPersonReportView.xaml
    /// </summary>
    public partial class AttendanceRecordForPersonReportView : UserControl, IReportPresenterView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ReportsPresenter _presenter;
        private IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;
        private ICollectionView _departmentComboBoxView;
        private ICollectionView _peopleComboBoxView;

        public AttendanceRecordForPersonReportView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The constructor for the AttendanceRecordForPersonReportView.
        /// Wires up all the necessary objects and events needed by this View.
        /// </summary>
        /// <param name="presenter"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="configurationService"></param>
        public AttendanceRecordForPersonReportView(ReportsPresenter presenter,
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
            AttendanceDate.SelectedDate = DateTime.Today;
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
        /// Handles when user changes department selection. Will retreive correct people.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DepartmentsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CACCCheckInDb.Department selectedDepartment = (CACCCheckInDb.Department)DepartmentsComboBox.SelectedItem;
            if (null == selectedDepartment) { return; }
            _presenter.GetDepartmentPeople(selectedDepartment.Name);
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

            CACCCheckInDb.PeopleWithDepartmentAndClassView selectedPerson =
                (CACCCheckInDb.PeopleWithDepartmentAndClassView)PeopleComboBox.SelectedItem;

            _presenter.GetPersonAttendanceOnDate(selectedPerson.PersonId,
                AttendanceDate.SelectedDate);
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
        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> DepartmentPeopleDataContext
        {
            get
            {
                return (List<CACCCheckInDb.PeopleWithDepartmentAndClassView>)_peopleComboBoxView.SourceCollection;
            }
            set
            {
                _peopleComboBoxView = CollectionViewSource.GetDefaultView(value);
                _peopleComboBoxView.SortDescriptions.Add(new SortDescription("LastName",
                    ListSortDirection.Ascending));
                _peopleComboBoxView.SortDescriptions.Add(new SortDescription("FirstName",
                    ListSortDirection.Ascending));

                _peopleComboBoxView.MoveCurrentToFirst();
                PeopleComboBox.ItemsSource = _peopleComboBoxView;
                PeopleComboBox.SelectedIndex = _peopleComboBoxView.CurrentPosition;

                getReportButton.IsEnabled = true;
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
            const int TableColumnCount = 3;

            FlowDocument flowDoc = new FlowDocument();
            flowDoc.Background = this.FindResource("ReportBackground") as Brush;

            Table table = new Table();

            table.Columns.Add(new TableColumn() { Width = GridLength.Auto });
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());

            // Create and add an empty TableRowGroup to hold the table's Rows.
            table.RowGroups.Add(new TableRowGroup());

            // To be used to alias the current working row for easy reference.
            TableRow currentRow;

            string reportTitle = "Class Attendance";
            
            if (0 != records.Count)
            {
                string person = String.Format("{0} {1}", records[0].FirstName,
                    records[0].LastName);

                if (AttendanceDate.SelectedDate.HasValue)
                {
                    reportTitle = String.Format("Attendance For {0} on {1}",
                        person, AttendanceDate.SelectedDate.Value.ToShortDateString());
                }
                else
                {
                    reportTitle = String.Format("Attendance For {0}", person);
                }
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
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Class"))));
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Security Code"))));
                currentRow.Cells[0].Style = this.FindResource("ReportTableHeader") as Style;
                currentRow.Cells[1].Style = this.FindResource("ReportTableHeader") as Style;
                currentRow.Cells[2].Style = this.FindResource("ReportTableHeader") as Style;
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

                foreach (IGrouping<string, CACCCheckInDb.AttendanceWithDetail> classGroup in classGroups)
                {
                    tableRowGroup = new TableRowGroup();

                    // Create and add an empty TableRowGroup to hold the table's Rows.
                    table.RowGroups.Add(tableRowGroup);

                    // Now enumerate through this grouping of 
                    // CACCCheckInDb.AttendanceWithDetail records.
                    foreach (CACCCheckInDb.AttendanceWithDetail record in classGroup.OrderByDescending(r => r.Date))
                    {
                        currentRow = new TableRow();
                        currentRow.Cells.Add(new TableCell(new Paragraph(new Run(record.Date.ToShortDateString()))));
                        currentRow.Cells.Add(new TableCell(new Paragraph(new Run(record.ClassName))));
                        currentRow.Cells.Add(new TableCell(new Paragraph(new Run(record.SecurityCode.ToString()))));

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
