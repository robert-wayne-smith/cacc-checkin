using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Microsoft.Practices.Prism.Events;
using CACCCheckIn.Printing.NameBadgePrinter;
using Infrastructure;

namespace CACCCheckInClient
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private ICollectionView printerView = null;
        private IEventAggregator _eventAggregator;

        public OptionsWindow(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            _eventAggregator = eventAggregator;
        }

        private LabelPrinterTypes? CurrentPrinterType
        {
            get
            {
                if (null == PrinterTypeComboBox.SelectedItem) return null;

                return (LabelPrinterTypes)Enum.Parse(typeof(LabelPrinterTypes),
                    (string)PrinterTypeComboBox.SelectedItem);
            }
            set
            {
                PrinterTypeComboBox.SelectedItem = value.ToString();                
            }
        }

        private LabelPrinter CurrentPrinter
        {
            get
            {
                return (LabelPrinter)PrinterComboBox.SelectedItem;
            }
        }

        private string LabelTemplateFile
        {
            get
            {
                return PrinterTemplateFileTextBox.Text;
            }
            set
            {
                PrinterTemplateFileTextBox.Text = value;
            }
        }

        private string TargetDepartment
        {
            get
            {
                string department = (null == DepartmentComboBox.SelectedItem) ? String.Empty :
                    (string)DepartmentComboBox.SelectedItem;
                
                return department; 
            }
            set
            {
                DepartmentComboBox.SelectedItem = value;
            }
        }
        
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPrinterType.HasValue)
            {
                Properties.Settings.Default.PrinterType = CurrentPrinterType.Value.ToString();
            }
            if (null != CurrentPrinter)
            {
                Properties.Settings.Default.PrinterName = CurrentPrinter.Name;
                Properties.Settings.Default.LabelTemplateFile = LabelTemplateFile;
            }
            
            Properties.Settings.Default.TargetDepartment = TargetDepartment;
            Properties.Settings.Default.Save();
            
            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Loading up valid values for the printer types
            PrinterTypeComboBox.ItemsSource = Enum.GetNames(typeof(LabelPrinterTypes));

            // Setting the current configured value for printer type
            LabelPrinterTypes currentPrinterType = (LabelPrinterTypes)Enum.Parse(typeof(LabelPrinterTypes),
                Properties.Settings.Default.PrinterType);

            // Loading up valid values for the printers
            printerView = CollectionViewSource.GetDefaultView(ValidLabelPrinters.Printers);
            printerView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            PrinterComboBox.ItemsSource = printerView;
            PrinterComboBox.DisplayMemberPath = "Name";

            if (CheckForValidPrinters(currentPrinterType))
            {
                CurrentPrinterType = currentPrinterType;

                FilterPrintersInComboBox(currentPrinterType);

                printerView.MoveCurrentTo(ValidLabelPrinters.Printers.Find(p =>
                    p.Type.Equals(currentPrinterType) && p.Name.Equals(Properties.Settings.Default.PrinterName)));
            }

            if (currentPrinterType == LabelPrinterTypes.Dymo)
            {
                LabelTemplateFile = Properties.Settings.Default.LabelTemplateFile;
            }

            // Loading up valid values for the DepartmentComboBox
            List<string> departments = new List<string>();
            departments.Add(Infrastructure.Departments.Children);
            departments.Add(Infrastructure.Departments.LadiesBibleStudy);
            departments.Add(Infrastructure.Departments.MOPS);
            departments.Add(Infrastructure.Departments.Preschool);            
            DepartmentComboBox.ItemsSource = departments;

            // Setting the current configured value for TargetDepartment
            TargetDepartment = Properties.Settings.Default.TargetDepartment;
        }

        private bool CheckForValidPrinters(LabelPrinterTypes currentPrinterType)
        {
            if (!AnyPrintersOfCurrentTypeArePresent(currentPrinterType))
            {
                MessageBox.Show(String.Format("There are not any [{0}] printers currently installed on this computer. Please install a [{0}] printer.", currentPrinterType),
                    "No Valid Printer", MessageBoxButton.OK, MessageBoxImage.Error);

                RemoveInvalidPrinterTypeFromSelection(currentPrinterType);

                return false;
            }

            return true;
        }

        private void RemoveInvalidPrinterTypeFromSelection(LabelPrinterTypes invalidPrinterType)
        {
            List<string> currentPrinterTypes = new List<string>(((string[])PrinterTypeComboBox.ItemsSource));
            List<string> newPrinterTypes = currentPrinterTypes.FindAll(p => !p.Equals(invalidPrinterType.ToString()));
            PrinterTypeComboBox.ItemsSource = newPrinterTypes.ToArray();
        }

        private bool AnyPrintersOfCurrentTypeArePresent(LabelPrinterTypes currentPrinterType)
        {
            if (printerView == null) return false;
            return (((List<LabelPrinter>)printerView.SourceCollection).Exists(p => p.Type.Equals(currentPrinterType)));
        }

        private void PrinterTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var currentPrinterType = (LabelPrinterTypes)Enum.Parse(typeof(LabelPrinterTypes),
                    (string)e.AddedItems[0]);

                if (CheckForValidPrinters(currentPrinterType))
                {
                    if (currentPrinterType == LabelPrinterTypes.Dymo)
                    {
                        PrinterTemplateFileTextBox.IsEnabled = true;
                    }
                    else
                    {
                        PrinterTemplateFileTextBox.Clear();
                        PrinterTemplateFileTextBox.IsEnabled = false;
                    }

                    FilterPrintersInComboBox(currentPrinterType);
                }
            }
        }

        private void FilterPrintersInComboBox(LabelPrinterTypes filterType)
        {
            if (printerView == null) return;
            printerView.Filter = p =>
                ((LabelPrinter)p).Type.Equals(filterType);
        }

        private void PrinterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != CurrentPrinter)
            { LabelTemplateFile = CurrentPrinter.TemplateFile; }
        }

        private void DepartmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string newTargetDepartment = e.AddedItems[0].ToString();
            
            if (null == _eventAggregator) { return; }

            _eventAggregator.GetEvent<DepartmentChangedEvent>().Publish(newTargetDepartment);

            if (newTargetDepartment.Equals(Departments.Children))
            {
                _eventAggregator.GetEvent<LoadChildrenCheckInViewEvent>().Publish(new EventArgs());
            }
            else if (newTargetDepartment.Equals(Departments.Preschool))
            {
                _eventAggregator.GetEvent<LoadPreschoolCheckInViewEvent>().Publish(new EventArgs());
            }
            else if (newTargetDepartment.Equals(Departments.MOPS))
            {
                _eventAggregator.GetEvent<LoadMOPSCheckInViewEvent>().Publish(new EventArgs());
            }
            else if (newTargetDepartment.Equals(Departments.LadiesBibleStudy))
            {
                _eventAggregator.GetEvent<LoadLadiesBibleStudyCheckInViewEvent>().Publish(new EventArgs());
            }
        }
    }
}
