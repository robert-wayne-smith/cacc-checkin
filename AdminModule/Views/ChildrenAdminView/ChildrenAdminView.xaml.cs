using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Regions;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Admin.Views
{
    /// <summary>
    /// Interaction logic for ChildrenAdminView.xaml
    /// </summary>
    public partial class ChildrenAdminView : UserControl, IChildrenAdminView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUnityContainer _container;
        private IRegionManager _regionManager;
        private readonly IConfigurationService _configurationService;

        public ChildrenAdminView()
        {
            InitializeComponent();
        }

        public ChildrenAdminView(IUnityContainer container, IRegionManager regionManager,
            IConfigurationService configurationService) : this()
        {
            logger.Debug("Creating ChildrenAdminView.");
            _container = container;
            _regionManager = regionManager;
            _configurationService = configurationService;
        }

        public IRegionManager AdminRegionManager        
        {
            get { return _regionManager; }
            set { _regionManager = value; }
        }

        private void AttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateAView<AttendanceView>(AttendanceView.ViewName);
        }

        private void ClassMovementButton_Click(object sender, RoutedEventArgs e)
        {            
            ActivateAView<ClassMovementView>(ClassMovementView.ViewName);
        }

        private void ClassesButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateAView<ClassesView>(ClassesView.ViewName);
        }

        private void DepartmentsButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateAView<DepartmentsView>(DepartmentsView.ViewName);
        }

        private void FamilyQuickEntryButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateAView<FamilyQuickEntryView>(FamilyQuickEntryView.ViewName);
        }
        
        private void PeopleButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateAView<PersonsView>(PersonsView.ViewName);
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            ReportsView view = ActivateAView<ReportsView>(ReportsView.ViewName);
            view.ReportInputRegionManager = AdminRegionManager;
        }

        private void TeachersButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateAView<TeachersView>(TeachersView.ViewName);
        }

        private T ActivateAView<T>(string viewName)
        {
            logger.DebugFormat("Activating [{0}].", viewName);

            // First, we see if the View has already been added to the Region.
            T view = (T)AdminRegionManager.Regions[Infrastructure.RegionNames.AdminDetailRegion].GetView(viewName);

            if (null == view)
            {
                logger.DebugFormat("View [{0}] does not exist yet. Creating it.", viewName);

                // If the View has not been added, we resolve the View and then add it to the Region
                view = _container.Resolve<T>();
                AdminRegionManager.Regions[Infrastructure.RegionNames.AdminDetailRegion].Add(view, viewName);
            }

            // After View has been retrieved or created, we will activate it.
            AdminRegionManager.Regions[Infrastructure.RegionNames.AdminDetailRegion].Activate(view);
            return view;
        }
    }
}