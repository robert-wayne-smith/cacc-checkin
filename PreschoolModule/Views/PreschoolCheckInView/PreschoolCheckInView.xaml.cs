using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Infrastructure;
using ServiceAndDataContracts;
using log4net;

namespace CACCCheckIn.Modules.Preschool.Views
{
    /// <summary>
    /// Interaction logic for PreschoolCheckInView.xaml
    /// </summary>
    public partial class PreschoolCheckInView : UserControl, IPreschoolCheckInView
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private PreschoolCheckInPresenter _presenter;
        private IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;
        private ICollectionView _peopleView;
        private ICollectionView _familyView;
        private CACCCheckInDb.PeopleWithDepartmentAndClassViewBindingList _familyViewEditable;
        private int currentSecurityCodeForFamily;
        private const int InvalidSecurityCode = 0;

        public const string ViewName = "PreschoolCheckInView";

        public PreschoolCheckInView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The constructor for the PreschoolCheckInView. Wires up all the necessary
        /// objects and events needed by this View.
        /// </summary>
        /// <param name="presenter"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="configurationService"></param>
        public PreschoolCheckInView(PreschoolCheckInPresenter presenter, IEventAggregator eventAggregator,
            IConfigurationService configurationService) : this()
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
            try
            {
                logger.Debug("UserControl loading.");

                GetDataForView();

                UpdateShellStatusBar("Preschool check-in view loaded.");
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
        /// Retrieves data used in View
        /// </summary>
        private void GetDataForView()
        {
            CloseErrorDetail();

            ShowProcessing(true);

            logger.Debug("Getting list of people.");
            _presenter.GetPeople();
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
                logger.DebugFormat("Retrieving family members for [{0} {1}]", currentPerson.FirstName,
                    currentPerson.LastName);
                _presenter.GetFamilyForPerson(currentPerson);                                               
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
            FamilyList.ItemsSource = null;

            CheckInButton.IsEnabled = false;
        }

        #region IPreschoolCheckInView Members

        /// <summary>
        /// This property allows access to the View Dispatcher object so that Presenter
        /// can update UI from different threads.
        /// </summary>
        public Dispatcher ViewDispatcher
        {
            get { return this.Dispatcher; }
        }

        /// <summary>
        /// This DataContext contains the list of adults in the list for selection
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
        /// This DataContext contains the family members for current selected person
        /// </summary>
        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> FamilyDataContext
        {
            get
            {
                return (List<CACCCheckInDb.PeopleWithDepartmentAndClassView>)_familyView.SourceCollection;
            }
            set
            {
                _familyView = CollectionViewSource.GetDefaultView(value);
                // Assign the view of family members to the FamilyList
                FamilyList.ItemsSource = _familyView;
                _familyView.SortDescriptions.Add(new SortDescription("LastName",
                    ListSortDirection.Ascending));
                _familyView.SortDescriptions.Add(new SortDescription("FirstName",
                    ListSortDirection.Ascending));
                
                // We will set up an editable view for the family members so that we can
                // allow editing of the members. 
                _familyViewEditable = new CACCCheckInDb.PeopleWithDepartmentAndClassViewBindingList(value);
                _familyViewEditable.AllowEdit = true;
                _familyViewEditable.AllowNew = false;
                _familyViewEditable.AllowRemove = false;
                _familyViewEditable.RaiseListChangedEvents = true;
                _familyViewEditable.ListChanged += new ListChangedEventHandler(_familyViewEditable_ListChanged);
                
                // We need to look and see how many family members are already checked in
                // We will start out assuming none are checked in
                int familyMembersNotCheckedIn = FamilyList.Items.Count;
                CheckInButton.IsEnabled = false;

                currentSecurityCodeForFamily = InvalidSecurityCode;

                // Retrieve the currently selected person 
                CACCCheckInDb.PeopleWithDepartmentAndClassView currentPerson = 
                    (CACCCheckInDb.PeopleWithDepartmentAndClassView)_peopleView.CurrentItem;
                
                // Get the attendance records for family of current person
                List<CACCCheckInDb.AttendanceWithDetail> attendance =
                    _presenter.GetAttendanceRecordsForFamilyToday(currentPerson);
                                
                // Loop through all the attendance records
                attendance.ForEach(delegate(CACCCheckInDb.AttendanceWithDetail record)
                {
                    // Get the security code in current record
                    currentSecurityCodeForFamily = record.SecurityCode;

                    // Find the person in the list of family members
                    CACCCheckInDb.PeopleWithDepartmentAndClassView person =
                        FamilyDataContext.FirstOrDefault(p => p.PersonId.Equals(record.PersonId));

                    // If found, the person is already checked in. We will set the security
                    // code assigned to the person based on the attendance records and
                    // decrease the count of family members not already checked in
                    if (person != null)
                    {
                        person.SecurityCode = record.SecurityCode.ToString();
                        familyMembersNotCheckedIn--;
                    }
                });

                // If there are still family members left to check in
                // we will enable the CheckInButton
                CheckInButton.IsEnabled = (familyMembersNotCheckedIn > 0);

                // Make the currently selected person be also selected in
                // family member list
                _familyView.MoveCurrentTo(currentPerson);
            }
        }

        /// <summary>
        /// Whenever a person's info changes in the family list we will save the 
        /// changes here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _familyViewEditable_ListChanged(object sender, ListChangedEventArgs e)
        {
            // Only handle the ItemChanged type of change
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                    logger.Debug("ItemChanged fired. Updating person if they need persisting.");

                    CACCCheckInDb.PeopleWithDepartmentAndClassView updatedPerson =
                        _familyViewEditable.ElementAtOrDefault(e.NewIndex);

                    if (updatedPerson.NeedsPersist)
                    { 
                        _presenter.UpdatePerson((CACCCheckInDb.Person)updatedPerson);
                    }
                    updatedPerson.NeedsPersist = false;
                    break;
            }            
        }

        /// <summary>
        /// After a person has been checked in, we will find that person in the current
        /// family list and then set the security code to one assigned during check in.
        /// </summary>
        /// <param name="person"></param>
        public void CheckInCompleted(CACCCheckInDb.PeopleWithDepartmentAndClassView personCheckedIn)
        {
            // Look for the person that was checked in in the family list.
            CACCCheckInDb.PeopleWithDepartmentAndClassView person =
                        FamilyDataContext.FirstOrDefault(p => p.PersonId.Equals(personCheckedIn.PersonId));
            
            // If person is found, update the security code of person in family list
            // and save the security code to possibly use for other family members
            if (person != null)
            {
                person.SecurityCode = personCheckedIn.SecurityCode;
                currentSecurityCodeForFamily = Int32.Parse(personCheckedIn.SecurityCode);
            }
            
            // As each person is checked in, we check to see if anyone in family list
            // is left to be checked in. If not, we can disable the check-in button
            int familyMembersNotCheckedIn = FamilyDataContext.Count(p =>
                String.IsNullOrEmpty(p.SecurityCode));
            CheckInButton.IsEnabled = (familyMembersNotCheckedIn > 0);
        }

        /// <summary>
        /// To be called when check-in process is totally complete
        /// </summary>
        public void CheckInCompleted()
        {
            // Stop the processing animation
            ShowProcessing(false);
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

        #endregion

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
        /// Handles Click event of CheckIn button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckInButton_Click(object sender, RoutedEventArgs e)
        {
            if (0 == FamilyList.SelectedItems.Count) return;

            ShowProcessing(true);

            IEnumerable<CACCCheckInDb.PeopleWithDepartmentAndClassView> peopleToCheckIn = 
                FamilyList.SelectedItems.Cast<CACCCheckInDb.PeopleWithDepartmentAndClassView>();

            _presenter.CheckInPeople(peopleToCheckIn.ToList(),
                currentSecurityCodeForFamily);
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
        /// Whenever the SpecialConditions TextBox changes, we will update
        /// the Style so that it stands out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpecialConditions_TextChanged(object sender, TextChangedEventArgs e)
        {
            Style hasSpecialConditions = this.FindResource("HasSpecialConditions") as Style;
            SpecialConditions.Style = (SpecialConditions.Text.Length == 0) ? null : hasSpecialConditions;
        }

        /// <summary>
        /// Whenever family members are loading into list, we will update some
        /// Styles if conditions are met
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FamilyList_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            if (e.Row.Item is CACCCheckInDb.PeopleWithDepartmentAndClassView)
            {
                CACCCheckInDb.PeopleWithDepartmentAndClassView person =
                    (CACCCheckInDb.PeopleWithDepartmentAndClassView)e.Row.Item;

                if (null == person) return;

                if (String.IsNullOrEmpty(person.SpecialConditions))
                {
                    e.DetailsElement.Visibility = Visibility.Collapsed;
                    e.DetailsElement.Style = null;
                }
                else
                {
                    Style hasSpecialConditions = this.FindResource("HasSpecialConditions") as Style;
                    e.DetailsElement.Style = hasSpecialConditions;
                }
            }
        }
    }
}
