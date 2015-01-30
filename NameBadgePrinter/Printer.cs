using System;
using System.Diagnostics;
using System.Reflection;
using CACCCheckIn.Printing.NameBadgePrinter.Common;
using log4net;

namespace CACCCheckIn.Printing.NameBadgePrinter
{
    public class Printer
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private LabelPrinter _printer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="printer"></param>
        public Printer(LabelPrinter printer)
        {
            _printer = printer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labelData"></param>
        public void PrintLabels(LabelData labelData)
        {
            try
            {
                logger.DebugFormat("Checking PrinterType to see what kind of printer and label: [{0}]",
                    _printer.Type);

                INameBadgeLabel label = null;
                // Based on configured Printer Type, get instance of correct NameBadgeLabel class
                switch (_printer.Type) 
                {
                    case LabelPrinterTypes.Dymo:
                        logger.Debug("Creating instance of DymoLabels.NameBadgeLabel.");
                        label = new DymoLabels.NameBadgeLabel(_printer.Name, _printer.TemplateFile);
                        break;
                    case LabelPrinterTypes.Seiko:
                        logger.Debug("Creating instance of SeikoSLPLabels.NameBadgeLabel.");
                        label = new SeikoSLPLabels.NameBadgeLabel(_printer.Name);
                        break;
                    default:
                        throw new ApplicationException("No valid PrinterTypes specified in configuration file. Valid values are 'Dymo' and 'Seiko'.");
                }

                logger.Debug("Calling PrintLabels with label data.");
                // Once we have the correct NameBadgeLabel class, call the PrintLabels method 
                // on the interface passing the label data
                label.PrintLabels(labelData);
            }
            catch (Exception ex)
            {
                logger.Error("Exception printing label:", ex);
                //EventLog.WriteEntry("NameBadgePrinter", ex.ToString(), EventLogEntryType.Error);
                throw new ApplicationException("No name badge could be printed at this time.");
            }
        }
    }
}
