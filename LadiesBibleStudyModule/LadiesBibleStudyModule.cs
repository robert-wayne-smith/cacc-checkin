using System;
using System.Reflection;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using CACCCheckIn.Modules.LadiesBibleStudy.Views;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.LadiesBibleStudy
{
    public class LadiesBibleStudyModule : IModule
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;

        public LadiesBibleStudyModule(IUnityContainer container,
            IRegionManager regionManager, IEventAggregator eventAggregator,
            IConfigurationService configurationService)
        {
            logger.Debug("Creating the LadiesBibleStudyModule.");

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
            logger.Debug("Initializing the LadiesBibleStudyModule.");

            RegisterViewsAndServices();
            SubscribeToEvents();

            if (_configurationService.GetTargetDepartment().Equals(Departments.LadiesBibleStudy))
            {
                logger.DebugFormat("Target Department is {0}. Loading CheckInView.",
                    Departments.LadiesBibleStudy);
                OnLoadLadiesBibleStudyCheckInView(new EventArgs());
            }
        }

        /// <summary>
        /// Register the Views used in this Module
        /// </summary>
        private void RegisterViewsAndServices()
        {
            logger.Debug("Registering Views used in the LadiesBibleStudyModule.");

            _container.RegisterType<ILadiesBibleStudyCheckInView, LadiesBibleStudyCheckInView>();
        }

        /// <summary>
        /// Subscribes to important events for this Module
        /// </summary>
        private void SubscribeToEvents()
        {
            logger.Debug("Subscribing to events.");

            LoadLadiesBibleStudyCheckInViewEvent loadLadiesBibleStudyCheckInViewEvent =
                _eventAggregator.GetEvent<LoadLadiesBibleStudyCheckInViewEvent>();
            loadLadiesBibleStudyCheckInViewEvent.Subscribe(OnLoadLadiesBibleStudyCheckInView,
                ThreadOption.PublisherThread, true);
        }

        /// <summary>
        /// Loads the LadiesBibleStudyCheckInView into CheckInRegion
        /// </summary>
        /// <param name="args"></param>
        public void OnLoadLadiesBibleStudyCheckInView(EventArgs args)
        {
            logger.DebugFormat("Activating [{0}].", LadiesBibleStudyCheckInView.ViewName);

            // First, we see if the LadiesBibleStudyCheckInView has already been added to the Region.
            LadiesBibleStudyCheckInView view = (LadiesBibleStudyCheckInView)_regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].
                GetView(LadiesBibleStudyCheckInView.ViewName);

            if (null == view)
            {
                logger.DebugFormat("View [{0}] does not exist yet. Creating it.", LadiesBibleStudyCheckInView.ViewName);

                // If the LadiesBibleStudyCheckInView has not been added, we resolve the View and
                // then add it to the CheckInRegion
                logger.Debug("Resolving the LadiesBibleStudyCheckInView and adding to CheckInRegion.");
                view = _container.Resolve<LadiesBibleStudyCheckInView>();
                _regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].Add(view, LadiesBibleStudyCheckInView.ViewName);
            }

            // After View has been retrieved or created, we will activate it.
            _regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].Activate(view);
        }
    }
}
