using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Events;
using Infrastructure;
using ServiceAndDataContracts;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System.Windows.Threading;

namespace CACCCheckIn.Modules.Admin.Views
{
    /// <summary>
    /// Interaction logic for ReportsView.xaml
    /// </summary>
    public partial class ReportsView : UserControl, IReportsView
    {
        private readonly IUnityContainer _container;
        private IEventAggregator _eventAggregator;
        private IRegionManager _regionManager;

        public const string ViewName = "ReportsView";

        public ReportsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The constructor for the ReportsView. Wires up all the necessary
        /// objects and events needed by this View.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="regionManager"></param>
        /// <param name="eventAggregator"></param>
        public ReportsView(IUnityContainer container,
            IRegionManager regionManager, IEventAggregator eventAggregator) : this()
        {
            _container = container;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// Performs actions when this UserControl loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateShellStatusBar("Reports view loaded.");

            reportTypesComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public IRegionManager ReportInputRegionManager
        {
            get { return _regionManager; }
            set { _regionManager = value; }
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
        
        #region IReportsView Members

        /// <summary>
        /// This property allows access to the View Dispatcher object so that Presenter
        /// can update UI from different threads.
        /// </summary>
        public Dispatcher ViewDispatcher
        {
            get { return this.Dispatcher; }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void reportTypesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbi = (ComboBoxItem)e.AddedItems[0];
            ReportTypes reportType = (ReportTypes)Enum.Parse(typeof(ReportTypes), cbi.Tag.ToString());

            switch (reportType)
            { 
                case ReportTypes.AttendanceRecordForPerson:
                    ActivateAView<AttendanceRecordForPersonReportView>("AttendanceRecordForPersonReportView");    
                    break;
                case ReportTypes.ClassAttendanceCountDuringDateRange:
                    ActivateAView<ClassAttendanceCountDuringDateRangeReportView>("ClassAttendanceCountDuringDateRangeReportView");                    
                    break;
                case ReportTypes.ClassAttendanceDuringDateRange:
                    ActivateAView<ClassAttendanceDuringDateRangeReportView>("ClassAttendanceDuringDateRangeReportView");
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        private void ActivateAView<T>(string viewName)
        {
            T view = (T)ReportInputRegionManager.Regions[Infrastructure.RegionNames.ReportInputRegion].GetView(viewName);

            if (null == view)
            {
                view = _container.Resolve<T>();
                ReportInputRegionManager.Regions[Infrastructure.RegionNames.ReportInputRegion].Add(view, viewName);
            }

            ReportInputRegionManager.Regions[Infrastructure.RegionNames.ReportInputRegion].Activate(view);
        }
    }
}
