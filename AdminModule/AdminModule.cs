using System.Reflection;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using CACCCheckIn.Modules.Admin.Views;
using log4net;

namespace CACCCheckIn.Modules.Admin
{
    public class AdminModule : IModule
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;

        public AdminModule(IUnityContainer container,
            IRegionManager regionManager)
        {
            logger.Debug("Creating the AdminModule.");

            _container = container;
            _regionManager = regionManager;
        }

        /// <summary>
        /// Handles initialization for this Module
        /// </summary>
        public void Initialize()
        {
            logger.Debug("Initializing the AdminModule.");

            RegisterViewsAndServices();

            LoadAdministrationView();
        }

        /// <summary>
        /// Register the Views used in this Module
        /// </summary>
        private void RegisterViewsAndServices()
        {
            logger.Debug("Registering Views used in the AdminModule.");

            _container.RegisterType<IChildrenAdminView, ChildrenAdminView>();
            _container.RegisterType<IAttendanceView, AttendanceView>();
            _container.RegisterType<IClassMovementView, ClassMovementView>();
            _container.RegisterType<IClassesView, ClassesView>();
            _container.RegisterType<IDepartmentsView, DepartmentsView>();
            _container.RegisterType<IFamilyQuickEntryView, FamilyQuickEntryView>();
            _container.RegisterType<IPersonsView, PersonsView>();
            _container.RegisterType<IReportsView, ReportsView>();
            _container.RegisterType<ITeachersView, TeachersView>();
            _container.RegisterType<IReportPresenterView, ClassAttendanceDuringDateRangeReportView>();
            _container.RegisterType<IReportPresenterView, ClassAttendanceCountDuringDateRangeReportView>();
            _container.RegisterType<IReportPresenterView, AttendanceRecordForPersonReportView>();
        }

        /// <summary>
        /// Loads the ChildrenAdminView into AdminRegion
        /// </summary>
        /// <param name="args"></param>
        public void LoadAdministrationView()
        {
            logger.Debug("Resolving the ChildrenAdminView.");
            ChildrenAdminView view = _container.Resolve<ChildrenAdminView>();

            logger.Debug("Adding the ChildrenAdminView to AdminRegion and activating.");
            IRegionManager adminRegionManager = _regionManager.Regions[Infrastructure.RegionNames.AdminRegion].Add(
                view, null, true);
            view.AdminRegionManager = adminRegionManager;
            _regionManager.Regions[Infrastructure.RegionNames.AdminRegion].Activate(view);
        }
    }
}
