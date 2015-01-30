using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Infrastructure;
using log4net;

namespace CACCCheckIn.Modules.Children.Views
{
    /// <summary>
    /// Interaction logic for ChildrenCheckInView.xaml
    /// </summary>
    public partial class ChildrenCheckInView : UserControl, IChildrenCheckInView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ChildrenCheckInPresenter _presenter;
        private IEventAggregator _eventAggregator;
        private ICollectionView _peopleView;
        private ICollectionView _classComboBoxView;
        private CACCCheckInDb.Class _exclusiveCheckInClass;

        public const string ViewName = "ChildrenCheckInView";
                
        public ChildrenCheckInView()
        {
            InitializeComponent();
            // Since we make the UserControl Focusable, 
            // turn off the visual style so that the "ants"
            // border is not shown
            this.FocusVisualStyle = null;
        }

        /// <summary>
        /// The constructor for the ChildrenCheckInView. Wires up all the necessary
        /// objects and events needed by this View.
        /// </summary>
        /// <param name="presenter"></param>
        /// <param name="eventAggregator"></param>
        public ChildrenCheckInView(ChildrenCheckInPresenter presenter,
            IEventAggregator eventAggregator) : this()
        {
            _presenter = presenter;
            _presenter.View = this;
            _eventAggregator = eventAggregator;

            logger.Debug("Subscribing to ShellLockedStateChangedEvent.");

            ShellLockedStateChangedEvent shellLockedStateChangedEvent = _eventAggregator.GetEvent<ShellLockedStateChangedEvent>();
            shellLockedStateChangedEvent.Subscribe(OnShellLockStateChanged, ThreadOption.UIThread);

            logger.Debug("Subscribing to ShellRefreshRequestedEvent.");

            ShellRefreshRequestedEvent shellRefreshRequestedEvent = _eventAggregator.GetEvent<ShellRefreshRequestedEvent>();
            shellRefreshRequestedEvent.Subscribe(OnShellRefreshRequested, ThreadOption.UIThread);
        }

        /// <summary>
        /// Performs actions when this UserControl loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChildrenCheckInView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Focus();

                logger.Debug("UserControl loading.");

                GetDataForView();

                UpdateShellStatusBar("Children check-in view loaded.");
            }
            catch (Exception ex)
            {
                DisplayExceptionDetail(ex);
            }
        }

        /// <summary>
        /// Whenever the ShellRefreshRequestedEvent fires, this will handle the event.
        /// </summary>
        /// <param name="args"></param>
        public void OnShellRefreshRequested(EventArgs args)
        {
            try
            {
                logger.Debug("ShellRefreshRequestedEvent was fired. Refreshing lists.");

                GetDataForView();
            }
            catch (Exception ex)
            {
                DisplayExceptionDetail(ex);
            }
        }

        /// <summary>
        /// If the Shell LockState has been changed, we will have a chance
        /// to react to the event here.
        /// </summary>
        /// <param name="args"></param>
        public void OnShellLockStateChanged(LockedStateChangedEventArgs args)
        {
            // For this View, when the Shell is Locked, we want to hide
            // the ExclusiveCheckInPanel and disable use of the ContextMenu
            // which displays the panel. If not Locked, we make sure to 
            // enable the ContextMenu again.
            if (args.State == LockedState.Locked)
            {
                LimitCheckInMenu.IsEnabled = false;
                LimitCheckInMenu.Visibility = Visibility.Collapsed;
                ExclusiveCheckInPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                LimitCheckInMenu.IsEnabled = true;
                LimitCheckInMenu.Visibility = Visibility.Visible;
            }
        }
        
        /// <summary>
        /// Retrieves data used in View
        /// </summary>
        private void GetDataForView()
        {
            CloseErrorDetail();

            ShowProcessing(true);

            logger.Debug("Getting list of children for ListBox.");
            _presenter.GetListOfChildren();

            logger.Debug("Getting list of classes for exclusive check-in ComboBox.");
            _presenter.GetDataForClassCombo();
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

        /// <summary>
        /// Will filter the names which show up in the list for names
        /// </summary>
        /// <param name="filter"></param>
        private void FilterNamesInListBox(string filter)
        {
            if (_peopleView == null) return;

            _peopleView.Filter = p =>
                ((CACCCheckInDb.PeopleWithDepartmentAndClassView)p).LastName.ToLower().
                StartsWith(filter.ToLower());

            _peopleView.MoveCurrentToFirst();
            if (_peopleView.CurrentItem == null) return;
            PeopleListBox.ScrollIntoView(_peopleView.CurrentItem);
        }

        /// <summary>
        /// Whenever one of the buttons is clicked to filter the list of names,
        /// this event handles it and calls function to filter the list
        /// appropriately
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void filterButton_Click(object sender, RoutedEventArgs e)
        {
            Button theButton = ((Button)(e.OriginalSource));
            string filter = theButton.Content.ToString();
            FilterNamesInListBox(filter);
        }

        /// <summary>
        /// Whenever the user selects a new person from the listbox, the DataContext changes 
        /// for the PeopleDetail panel. Change the view based on details for 
        /// current DataContext.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PeopleDetail_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CloseErrorDetail();

            if (e.NewValue == null)
            {
                ClearPeopleDetailPanel();
                return;
            }

            // Retrieves the person newly assigned to the DataContext
            CACCCheckInDb.PeopleWithDepartmentAndClassView currentPerson =
                (CACCCheckInDb.PeopleWithDepartmentAndClassView)e.NewValue;

            if (currentPerson != null)
            {
                CheckInDetailsName.Text = String.Format("{0} {1}",
                    currentPerson.FirstName, currentPerson.LastName);
                // If user has selected an exclusive class for check-in, 
                // we will use that class instead of default class for 
                // the person
                CheckInDetailsClass.Text = (_exclusiveCheckInClass == null) ?
                    currentPerson.ClassName : _exclusiveCheckInClass.Name;

                Guid targetClassId = (_exclusiveCheckInClass == null) ?
                    currentPerson.ClassId : _exclusiveCheckInClass.Id;
                
                // Looking to see if person is already checked in today to class
                if (_presenter.IsChildAlreadyCheckedInToday(currentPerson, targetClassId))
                {
                    CheckInDetailsBegin.Text = String.Empty;
                    CheckInDetailsMiddle.Text = "is already checked into the";
                    if (currentPerson.ClassRole.Equals(ClassRoles.Teacher))
                    {
                        CheckInDetailsPreDate.Text = "class as TEACHER for";
                    }
                    else
                    {
                        CheckInDetailsPreDate.Text = "class for";
                    }
                    CheckInDetailsDate.Text = String.Format("{0}.", DateTime.Today.ToLongDateString());
                    CheckInDetailsEnd.Text = "Duplicate Check-In Not Allowed.";

                    CheckInButton.IsEnabled = false;
                }
                else
                {
                    CheckInDetailsBegin.Text = "To check";
                    CheckInDetailsMiddle.Text = "into the";
                    if (currentPerson.ClassRole.Equals(ClassRoles.Teacher))
                    { 
                        CheckInDetailsPreDate.Text = "class as TEACHER for";
                    }
                    else
                    {
                        CheckInDetailsPreDate.Text = "class for";
                    }                    
                    CheckInDetailsDate.Text = DateTime.Today.ToLongDateString();
                    CheckInDetailsEnd.Text = "click the Check-In button.";

                    CheckInButton.IsEnabled = true;
                }
            }
            else
            {
                ClearPeopleDetailPanel();
            }
        }

        /// <summary>
        /// Clears out the PeopleDetail panel when no person is selected
        /// </summary>
        private void ClearPeopleDetailPanel()
        {
            CheckInDetailsBegin.Text = String.Empty;
            CheckInDetailsName.Text = String.Empty;
            CheckInDetailsMiddle.Text = String.Empty;
            CheckInDetailsClass.Text = String.Empty;
            CheckInDetailsPreDate.Text = String.Empty;
            CheckInDetailsDate.Text = String.Empty;
            CheckInDetailsEnd.Text = String.Empty;

            CheckInButton.IsEnabled = false;
        }

        #region IChildrenCheckInView Members

        /// <summary>
        /// This property allows access to the View Dispatcher object so that Presenter
        /// can update UI from different threads.
        /// </summary>
        public Dispatcher ViewDispatcher
        {
            get { return this.Dispatcher; }
        }

        /// <summary>
        /// This DataContext contains the list of children in the list for selection
        /// </summary>
        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> PeopleDataContext
        {
            get
            {
                return (List<CACCCheckInDb.PeopleWithDepartmentAndClassView>)_peopleView.SourceCollection;
            }
            set
            {
                _peopleView = CollectionViewSource.GetDefaultView(value);
                PeopleListBox.ItemsSource = _peopleView;
                _peopleView.SortDescriptions.Add(new SortDescription("LastName",
                    ListSortDirection.Ascending));
                _peopleView.SortDescriptions.Add(new SortDescription("FirstName",
                    ListSortDirection.Ascending));
                _peopleView.MoveCurrentToFirst();

                if (_peopleView.CurrentItem != null)
                {
                    PeopleListBox.SelectedIndex = _peopleView.CurrentPosition;
                    PeopleListBox.ScrollIntoView(_peopleView.CurrentItem);
                }
                
                ShowProcessing(false);
            }
        }

        /// <summary>
        /// This DataContext contains the list of classes that can be used
        /// for exclusive check-in
        /// </summary>
        public List<CACCCheckInDb.Class> ClassesDataContext
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

                LimitCheckInClassesComboBox.ItemsSource = _classComboBoxView;
                LimitCheckInClassesComboBox.SelectedIndex = _classComboBoxView.CurrentPosition;
            }
        }

        /// <summary>
        /// Once a child has been checked in, this function is called so that
        /// UI can be updated with information for that child.
        /// </summary>
        /// <param name="person"></param>
        public void CheckInCompleted(CACCCheckInDb.PeopleWithDepartmentAndClassView person)
        {
            // Stop the processing animation
            ShowProcessing(false);
            
            // Update the UI
            CheckInDetailsBegin.Text = String.Empty;
            CheckInDetailsName.Text = String.Format("{0} {1}",
                person.FirstName, person.LastName);
            CheckInDetailsMiddle.Text = "was checked into the";
            CheckInDetailsClass.Text = person.ClassName;
            CheckInDetailsPreDate.Text = "class for";
            CheckInDetailsDate.Text = String.Format("{0}.", DateTime.Today.ToLongDateString());
            CheckInDetailsEnd.Text = "Please Take Label From Printer.";

            // Disable check-in button so that check-in can't be performed
            // again for child.
            CheckInButton.IsEnabled = false;
        }

        /// <summary>
        /// This function will take the passed Exception and display its
        /// details in the ErrorDetailView panel.
        /// </summary>
        /// <param name="ex"></param>
        public void DisplayExceptionDetail(Exception ex)
        {
            // Stop the processing animation
            ShowProcessing(false);

            string errorMessage = ex.Message;
            if (null != ex.InnerException)
            {
                errorMessage = errorMessage + " :: " + ex.InnerException.Message;
            }

            ErrorText.Text = errorMessage;
            ErrorDetailView.Visibility = Visibility.Visible;
        }

        #endregion IChildrenCheckInView Members

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
            GetDataForView();
        }

        /// <summary>
        /// Handles when user clicks the Check-In button. Will show the processing control
        /// and start check-in process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckInButton_Click(object sender, RoutedEventArgs e)
        {
            ShowProcessing(true);

            CACCCheckInDb.PeopleWithDepartmentAndClassView currentPerson =
                (CACCCheckInDb.PeopleWithDepartmentAndClassView)
                ((CACCCheckInDb.PeopleWithDepartmentAndClassView)PeopleDetail.DataContext).Clone();

            // If there is currently an exclusive check-in class selected,
            // we will update the person info here so that they get checked 
            // in to that class instead
            if (_exclusiveCheckInClass != null)
            {
                currentPerson.ClassId = _exclusiveCheckInClass.Id;
                currentPerson.ClassName = _exclusiveCheckInClass.Name;
            }
            
            _presenter.CheckInPerson(currentPerson);
        }

        /// <summary>
        /// Turns the processing control on and off. This shows the user
        /// that activity is happening and stops any further activity until
        /// complete
        /// </summary>
        /// <param name="isContentProcessing"></param>
        private void ShowProcessing(bool isContentProcessing)
        {
            processingControl.IsContentProcessing = isContentProcessing;

            if (!isContentProcessing) { UpdateShellStatusBar(String.Empty); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LimitCheckInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CACCCheckInDb.Class selectedClass = (CACCCheckInDb.Class)LimitCheckInClassesComboBox.SelectedItem;
                if (null == selectedClass)
                {
                    _exclusiveCheckInClass = null;
                    return;
                }

                if (selectedClass.Name.Equals(String.Empty) &&
                    selectedClass.Id.Equals(new Guid("00000000-0000-0000-0000-000000000000")))
                {
                    _exclusiveCheckInClass = null;
                    return;
                }

                _exclusiveCheckInClass = selectedClass;
            }
            catch (Exception)
            {
                _exclusiveCheckInClass = null;
            }
            finally
            {
                _peopleView.MoveCurrentToPosition(-1);
                _peopleView.MoveCurrentToFirst();
                PeopleListBox.SelectedIndex = _peopleView.CurrentPosition;
                PeopleListBox.ScrollIntoView(_peopleView.CurrentItem);
                
                ExclusiveCheckInPanel.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// This handles this Click event for 
        /// the MenuItem which is part of ContextMenu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Make the panel for exclusive check in visible
            // so user can set value appropriately
            ExclusiveCheckInPanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the KeyUp event so we can look for CTRL-L
        /// command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ChildrenCheckInView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ExclusiveCheckInPanel.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// To keep Focus on UserControl when user clicks anywhere, we handle
        /// the MouseDown event and set Focus. This helps with handling 
        /// KeyUp events for UserControl.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChildrenCheckInView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }
    }
}
