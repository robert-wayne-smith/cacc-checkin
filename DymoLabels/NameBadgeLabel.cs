using System;
using System.Reflection;
using System.Runtime.InteropServices;
using CACCCheckIn.Printing.NameBadgePrinter.Common;
using Dymo;
using log4net;
using System.IO;

namespace CACCCheckIn.Printing.NameBadgePrinter.DymoLabels
{
    /// <summary>
    /// This is NameBadgeLabel class for the Dymo Label Printer.
    /// Will take the LabelData information and expect to print to a Dymo printer
    /// </summary>
    public class NameBadgeLabel : INameBadgeLabel
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _printerName;
        private string _labelTemplateFile;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="printerName"></param>
        /// <param name="labelTemplateFile"></param>
        public NameBadgeLabel(string printerName, string labelTemplateFile)
        {
            logger.DebugFormat("Creating instance of NameBadgeLabel for printer [{0}] with template [{1}]",
                printerName, labelTemplateFile);
            _printerName = printerName;
            _labelTemplateFile = labelTemplateFile;
        }

        #region INameBadgeLabel Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labelData"></param>
        void INameBadgeLabel.PrintLabels(LabelData labelData)
        {
            IDymoAddIn4 dymoAddIn = null;
		    IDymoLabels dymoLabels = null;
            
            try
            {
                // create DYMO COM objects
                dymoAddIn = new DymoAddInClass();
                dymoLabels = new DymoLabelsClass();

                logger.DebugFormat("Calling SelectPrinter for printer [{0}]", _printerName);
                // Set the printer to whatever is configured for printer. Should
                // be a Dymo printer
                if (!dymoAddIn.SelectPrinter(_printerName))
                {
                    string errorMessage = String.Format("Could not open Dymo printer [{0}].",
                        _printerName);
                    throw new ApplicationException(errorMessage);
                }

                //string prtNames = dymoAddIn.GetDymoPrinters();
                //string prtName = dymoAddIn.GetCurrentPrinterName();

                // Template file should be located in same directory as application
                _labelTemplateFile = Path.Combine(Environment.CurrentDirectory,
                    _labelTemplateFile);

                logger.DebugFormat("Opening label template file: [{0}]", _labelTemplateFile);
                // Open the template file for whatever label we are going to print
                // This should be a name badge type label.
                if (!dymoAddIn.Open(_labelTemplateFile))
                {
                    string errorMessage = String.Format("Could not open label template file [{0}].",
                        _labelTemplateFile);
                    throw new ApplicationException(errorMessage);
                }

                //string ObjNames = dymoLabels.GetObjectNames(true);

                logger.Debug("Calling StartPrintJob.");
                dymoAddIn.StartPrintJob();

                logger.Debug("Looping through each person to print label.");
                
                // Loop through each Person in LabelData information that is 
                // contained in current label. Print label for each Person
                foreach (Person person in labelData.Persons)
                {
                    logger.Debug("Setting LOCATION field on label.");
                    dymoLabels.SetField("LOCATION", labelData.Location);

                    logger.Debug("Setting NAME field on label.");
                    dymoLabels.SetField("NAME", person.Name);

                    // After this, print the SecurityCode along with the Class
                    // If SecurityCode is not there, we will just print Class 
                    if (String.IsNullOrEmpty(labelData.SecurityCode))
                    {
                        logger.Debug("Setting SECURITYCODEANDCLASS field on label. Class only.");
                        dymoLabels.SetField("SECURITYCODEANDCLASS", String.Format("{0}", person.Class));
                    }
                    else
                    {
                        logger.Debug("Setting SECURITYCODEANDCLASS field on label.");
                        dymoLabels.SetField("SECURITYCODEANDCLASS", String.Format("{0} -- {1}",
                            labelData.SecurityCode, person.Class));
                    }

                    // Print Date next if specified
                    if (labelData.Date.HasValue)
                    {
                        logger.Debug("Setting DATE field on label.");
                        dymoLabels.SetField("DATE", labelData.Date.Value.ToString("ddd MM/dd/yy"));
                    }
                    else
                    {
                        dymoLabels.SetField("DATE", String.Empty);
                    }

                    logger.Debug("Setting SPECIALCONDITIONS field on label.");
                    if (String.IsNullOrEmpty(person.SpecialConditions))
                    { dymoLabels.SetField("SPECIALCONDITIONS", String.Empty); }
                    else
                    { dymoLabels.SetField("SPECIALCONDITIONS", person.SpecialConditions); }

                    logger.Debug("Calling Print to print label.");
                    // print 1 label, no dialog
                    dymoAddIn.Print(1, false);
                }

                logger.Debug("Calling EndPrintJob.");
                // ATTENTION: Every StartPrintJob() must have a matching EndPrintJob()
                dymoAddIn.EndPrintJob();
            }
            finally
            {
                logger.Debug("Releasing COM objects used.");
                // clean up DYMO COM objects
                if (dymoAddIn != null)
                {
                    Marshal.ReleaseComObject(dymoAddIn);
                    dymoAddIn = null;
                }

                if (dymoLabels != null)
                {
                    Marshal.ReleaseComObject(dymoLabels);
                    dymoLabels = null;
                }
            }
        }

        #endregion
    }
}
