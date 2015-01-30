using System;
using System.Reflection;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using CACCCheckIn.Modules.MOPS.Views;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.MOPS
{
    public class MOPSModule : IModule
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;

        public MOPSModule(IUnityContainer container,
            IRegionManager regionManager, IEventAggregator eventAggregator,
            IConfigurationService configurationService)
        {
            logger.Debug("Creating the MOPSModule.");

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
            logger.Debug("Initializing the MOPSModule.");

            RegisterViewsAndServices();
            SubscribeToEvents();

            if (_configurationService.GetTargetDepartment().Equals(Departments.MOPS))
            {
                logger.DebugFormat("Target Department is {0}. Loading CheckInView.",
                    Departments.MOPS);
                OnLoadMOPSCheckInView(new EventArgs());
            }
        }

        /// <summary>
        /// Register the Views used in this Module
        /// </summary>
        private void RegisterViewsAndServices()
        {
            logger.Debug("Registering Views used in the MOPSModule.");

            _container.RegisterType<IMOPSCheckInView, MOPSCheckInView>();
        }

        /// <summary>
        /// Subscribes to important events for this Module
        /// </summary>
        private void SubscribeToEvents()
        {
            logger.Debug("Subscribing to events.");

            LoadMOPSCheckInViewEvent loadMOPSCheckInViewEvent =
                _eventAggregator.GetEvent<LoadMOPSCheckInViewEvent>();
            loadMOPSCheckInViewEvent.Subscribe(OnLoadMOPSCheckInView,
                ThreadOption.PublisherThread, true);
        }

        /// <summary>
        /// Loads the MOPSCheckInView into CheckInRegion
        /// </summary>
        /// <param name="args"></param>
        public void OnLoadMOPSCheckInView(EventArgs args)
        {
            logger.DebugFormat("Activating [{0}].", MOPSCheckInView.ViewName);

            // First, we see if the MOPSCheckInView has already been added to the Region.
            MOPSCheckInView view = (MOPSCheckInView)_regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].
                GetView(MOPSCheckInView.ViewName);

            if (null == view)
            {
                logger.DebugFormat("View [{0}] does not exist yet. Creating it.", MOPSCheckInView.ViewName);

                // If the MOPSCheckInView has not been added, we resolve the View and
                // then add it to the CheckInRegion
                logger.Debug("Resolving the MOPSCheckInView and adding to CheckInRegion.");
                view = _container.Resolve<MOPSCheckInView>();
                _regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].Add(view, MOPSCheckInView.ViewName);
            }

            // After View has been retrieved or created, we will activate it.
            _regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].Activate(view);
        }
    }
}
