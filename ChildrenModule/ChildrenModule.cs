using System;
using System.Reflection;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using CACCCheckIn.Modules.Children.Views;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Children
{
    public class ChildrenModule : IModule
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;

        public ChildrenModule(IUnityContainer container,
            IRegionManager regionManager, IEventAggregator eventAggregator,
            IConfigurationService configurationService)
        {
            logger.Debug("Creating the ChildrenModule.");

            _container = container;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _configurationService = configurationService;
        }

        /// <summary>
        /// Handles initialization for this Module
        /// </summary>
        public void Initialize()
        {
            logger.Debug("Initializing the ChildrenModule.");

            RegisterViewsAndServices();
            SubscribeToEvents();

            if (_configurationService.GetTargetDepartment().Equals(Departments.Children))
            {
                logger.DebugFormat("Target Department is {0}. Loading CheckInView.",
                    Departments.Children);
                OnLoadChildrenCheckInView(new EventArgs());
            }
        }

        /// <summary>
        /// Register the Views used in this Module
        /// </summary>
        private void RegisterViewsAndServices()
        {
            logger.Debug("Registering Views used in the ChildrenModule.");

            _container.RegisterType<IChildrenCheckInView, ChildrenCheckInView>();
        }

        /// <summary>
        /// Subscribes to important events for this Module
        /// </summary>
        private void SubscribeToEvents()
        {
            logger.Debug("Subscribing to events.");

            LoadChildrenCheckInViewEvent loadChildrenCheckInViewEvent =
                _eventAggregator.GetEvent<LoadChildrenCheckInViewEvent>();
            loadChildrenCheckInViewEvent.Subscribe(OnLoadChildrenCheckInView,
                ThreadOption.PublisherThread, true);
        }

        /// <summary>
        /// Loads the ChildrenCheckInView into CheckInRegion
        /// </summary>
        /// <param name="args"></param>
        public void OnLoadChildrenCheckInView(EventArgs args)
        {
            logger.DebugFormat("Activating [{0}].", ChildrenCheckInView.ViewName);

            // First, we see if the ChildrenCheckInView has already been added to the Region.
            ChildrenCheckInView view = (ChildrenCheckInView)_regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].
                GetView(ChildrenCheckInView.ViewName);

            if (null == view)
            {
                logger.DebugFormat("View [{0}] does not exist yet. Creating it.", ChildrenCheckInView.ViewName);

                // If the ChildrenCheckInView has not been added, we resolve the View and
                // then add it to the CheckInRegion
                logger.Debug("Resolving the ChildrenCheckInView and adding to CheckInRegion.");
                view = _container.Resolve<ChildrenCheckInView>();
                _regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].Add(view,
                    ChildrenCheckInView.ViewName);
            }

            // After View has been retrieved or created, we will activate it.
            _regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].Activate(view);
        }
    }
}
