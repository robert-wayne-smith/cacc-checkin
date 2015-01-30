using System;
using System.Reflection;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using CACCCheckIn.Modules.Preschool.Views;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Preschool
{
    public class PreschoolModule : IModule
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;

        public PreschoolModule(IUnityContainer container,
            IRegionManager regionManager, IEventAggregator eventAggregator,
            IConfigurationService configurationService)
        {
            logger.Debug("Creating the PreschoolModule.");

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
            logger.Debug("Initializing the PreschoolModule.");

            RegisterViewsAndServices();
            SubscribeToEvents();

            if (_configurationService.GetTargetDepartment().Equals(Departments.Preschool))
            {
                logger.DebugFormat("Target Department is {0}. Loading CheckInView.",
                    Departments.Preschool);
                OnLoadPreschoolCheckInView(new EventArgs());
            }
        }

        /// <summary>
        /// Register the Views used in this Module
        /// </summary>
        private void RegisterViewsAndServices()
        {
            logger.Debug("Registering Views used in the PreschoolModule.");

            _container.RegisterType<IPreschoolCheckInView, PreschoolCheckInView>();
        }

        /// <summary>
        /// Subscribes to important events for this Module
        /// </summary>
        private void SubscribeToEvents()
        {
            logger.Debug("Subscribing to events.");

            LoadPreschoolCheckInViewEvent loadPreschoolCheckInViewEvent =
                _eventAggregator.GetEvent<LoadPreschoolCheckInViewEvent>();
            loadPreschoolCheckInViewEvent.Subscribe(OnLoadPreschoolCheckInView,
                ThreadOption.PublisherThread, true);
        }

        /// <summary>
        /// Loads the PreschoolCheckInView into CheckInRegion
        /// </summary>
        /// <param name="args"></param>
        public void OnLoadPreschoolCheckInView(EventArgs args)
        {
            logger.DebugFormat("Activating [{0}].", PreschoolCheckInView.ViewName);

            // First, we see if the PreschoolCheckInView has already been added to the Region.
            PreschoolCheckInView view = (PreschoolCheckInView)_regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].
                GetView(PreschoolCheckInView.ViewName);

            if (null == view)
            {
                logger.DebugFormat("View [{0}] does not exist yet. Creating it.", PreschoolCheckInView.ViewName);

                // If the PreschoolCheckInView has not been added, we resolve the View and
                // then add it to the CheckInRegion
                logger.Debug("Resolving the PreschoolCheckInView and adding to CheckInRegion.");
                view = _container.Resolve<PreschoolCheckInView>();
                _regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].Add(view, PreschoolCheckInView.ViewName);
            }

            // After View has been retrieved or created, we will activate it.
            _regionManager.Regions[Infrastructure.RegionNames.CheckInRegion].Activate(view);
        }
    }
}
