using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Infrastructure;
using log4net;

namespace CACCCheckInClient
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Commands

        public static RoutedUICommand AboutCommand;
        public static RoutedUICommand OptionsCommand;

        #endregion

        private IEventAggregator _eventAggregator;
        private bool LockedDownUI = false;

        static Shell()
        {
            AboutCommand = new RoutedUICommand("_About", "About", typeof(Shell));
            OptionsCommand = new RoutedUICommand("_Options", "Options", typeof(Shell));
        }

        public Shell(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            
            _eventAggregator = eventAggregator;

            logger.Debug("Setting up command bindings for Shell.");

            // Bind ExitCommand
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close,
                CloseExecuted, CloseCanExecute));

            // Bind AboutCommand
            CommandBinding commandBindingAboutCommand = new CommandBinding(AboutCommand);
            commandBindingAboutCommand.Executed += new ExecutedRoutedEventHandler(commandBindingAboutCommand_Executed);
            this.CommandBindings.Add(commandBindingAboutCommand);

            // Bind OptionsCommand
            CommandBinding commandBindingOptionsCommand = new CommandBinding(OptionsCommand);
            commandBindingOptionsCommand.Executed += new ExecutedRoutedEventHandler(commandBindingOptionsCommand_Executed);
            this.CommandBindings.Add(commandBindingOptionsCommand);

            logger.Debug("Subscribing to Shell status bar updates.");
            ShellStatusBarUpdateEvent shellStatusBarUpdateEvent = _eventAggregator.GetEvent<ShellStatusBarUpdateEvent>();
            shellStatusBarUpdateEvent.Subscribe(OnShellStatusBarUpdate, ThreadOption.UIThread);

            DepartmentChangedEvent departmentChangedEvent = _eventAggregator.GetEvent<DepartmentChangedEvent>();
            departmentChangedEvent.Subscribe(OnDepartmentChanged, ThreadOption.UIThread);
        }

        /// <summary>
        /// When publisher sends a department changed event, we receive it and handle it
        /// here
        /// </summary>
        /// <param name="currentDepartment"></param>
        public void OnDepartmentChanged(string currentDepartment)
        {
            CurrentDepartmentLabel.Content = String.Format("[Department: {0}]",
                currentDepartment);
            CurrentDepartmentLabel.UpdateLayout();
        }

        /// <summary>
        /// When publisher sends a status bar update, we receive it and handle it
        /// here
        /// </summary>
        /// <param name="args"></param>
        public void OnShellStatusBarUpdate(StatusBarUpdateEventArgs args)
        {
            Status.Content = args.StatusMessage;
        }

        /// <summary>
        /// Updates the status in StatusBar. Called by the StatusUpdateService.
        /// </summary>
        /// <param name="status">Status text to display</param>
        public void UpdateStatusBar(string status)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new DispatcherOperationCallback(
                    delegate(object arg)
                    {
                        Status.Content = status.Trim();
                        return null;
                    }), null);
        }

        /// <summary>
        /// Handles Shell Initialization event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shell_Initialized(object sender, EventArgs e)
        {
            logger.Debug("Loading settings.");
            Properties.Settings.Default.Reload();

            logger.DebugFormat("Setting Target Department to {0}", Properties.Settings.Default.TargetDepartment);
            OnDepartmentChanged(Properties.Settings.Default.TargetDepartment);

            if (Properties.Settings.Default.WindowState == WindowState.Maximized)
            {
                logger.Debug("Setting WindowState to Maximized.");
                this.WindowState = WindowState.Maximized;
                return;
            }

            Rect zeroRect = new Rect(0, 0, 0, 0);

            if (!Properties.Settings.Default.Location.IsEmpty &&
                !Properties.Settings.Default.Location.Equals(zeroRect))
            {
                logger.Debug("Setting Window Location.");
                this.Left = Properties.Settings.Default.Location.Left;
                this.Top = Properties.Settings.Default.Location.Top;
                this.Width = Properties.Settings.Default.Location.Width;
                this.Height = Properties.Settings.Default.Location.Height;
            }

            if (Properties.Settings.Default.WindowState != WindowState.Maximized)
            {
                logger.Debug("Setting WindowState.");
                this.WindowState = Properties.Settings.Default.WindowState;
            }
        }

        /// <summary>
        /// Handle Shell Closing event so we can save settings or possibly 
        /// prevent shutdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shell_Closing(object sender, CancelEventArgs e)
        {
            // If the Shell is currently LOCKED, we will not allow the application
            // to close
            if (LockedDownUI) { e.Cancel = true; return; }

            logger.Debug("Saving settings.");
            Properties.Settings.Default.WindowState = this.WindowState;
            Properties.Settings.Default.Location = this.RestoreBounds;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Looks for important keystrokes used by this application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shell_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12 && Keyboard.Modifiers == ModifierKeys.Control)
            {
                logger.Debug("Shell LockState Changed. Notifying subscribers.");
                Shell_LockedStateChanged();
            }
            else if (e.Key == Key.F5 && Keyboard.Modifiers == ModifierKeys.None)
            {
                logger.Debug("Shell Refresh Requested. Notifying subscribers.");
                Shell_RefreshRequested();
            }

            e.Handled = false;
        }

        /// <summary>
        /// Whenever the LockState event has been triggered (CTRL-F12),
        /// we handle it here.
        /// </summary>
        private void Shell_LockedStateChanged()
        {
            logger.Debug("Detected Shell LockedState change.");
            LockedStateChangedEventArgs eventArgs = new LockedStateChangedEventArgs();

            LockedDownUI = LockedDownUI ? false : true;

            if (LockedDownUI)
            {
                logger.Debug("Shell is UNLOCKED. Performing lock down actions.");

                // Don't allow resizing of the UI when locked down
                this.ResizeMode = ResizeMode.NoResize;
                // Change the WindowStyle so that control box is not shown
                this.WindowStyle = WindowStyle.None;
                // Maximum the application
                this.WindowState = WindowState.Maximized;
                // Hide the menu
                mainMenu.Visibility = Visibility.Collapsed;
                // Lock the tab on current selection
                if (AdministrationTabItem.IsSelected)
                {
                    CheckInTabItem.Visibility = Visibility.Collapsed;
                }
                else
                {
                    AdministrationTabItem.Visibility = Visibility.Collapsed;
                }
                
                // Display lock status
                LockedStatus.Content = "LOCKED";
                eventArgs.State = LockedState.Locked;
            }
            else
            {
                logger.Debug("Shell is LOCKED. Performing unlock actions.");

                // Allow resizing again
                this.ResizeMode = ResizeMode.CanResizeWithGrip;
                // Place border back for application
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                // Change window back to Normal state
                this.WindowState = WindowState.Normal;
                // Show the menu again
                mainMenu.Visibility = Visibility.Visible;
                // Make sure both tabs are visible
                AdministrationTabItem.Visibility = Visibility.Visible;
                CheckInTabItem.Visibility = Visibility.Visible;

                // Display lock status
                LockedStatus.Content = String.Empty;
                eventArgs.State = LockedState.Unlocked;
            }

            logger.Debug("Publishing ShellLockedStateChangedEvent to subscribers.");
            _eventAggregator.GetEvent<ShellLockedStateChangedEvent>().Publish(eventArgs);
        }
        
        /// <summary>
        /// Whenever the Shell Refresh event has been triggered (F5),
        /// we handle it here.
        /// </summary>
        private void Shell_RefreshRequested()        
        {
            logger.Debug("Publishing ShellRefreshRequestedEvent to subscribers.");
            // Let the subscribers now so they can do whatever they want for the event.
            _eventAggregator.GetEvent<ShellRefreshRequestedEvent>().Publish(new EventArgs());
        }
        
        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Whenever the user chooses the About menu item, this handles that event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void commandBindingAboutCommand_Executed(object sender, RoutedEventArgs e)
        {
            logger.Debug("Opening the AboutBox.");
            AboutBox about = new AboutBox();
            about.Owner = this;
            about.ShowDialog();
        }

        /// <summary>
        /// Whenever the user chooses the Options menu item, this handles that event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void commandBindingOptionsCommand_Executed(object sender, RoutedEventArgs e)
        {
            logger.Debug("Opening the OptionsWindow.");
            OptionsWindow ow = new OptionsWindow(_eventAggregator);
            ow.Owner = this;
            ow.ShowDialog();
        }
    }
}
