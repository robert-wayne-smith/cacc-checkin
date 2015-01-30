using System;
using System.Collections.Generic;
using System.Printing;
using System.Text;

namespace CACCCheckIn.Printing.NameBadgePrinter
{
    public static class DymoPrinters
    {
        public const string Dymo = "DYMO";
        public const string DymoLabelWriter400Turbo = "DYMO LabelWriter 400 Turbo";
        public const string DymoLabelTemplateFile = "CACC-Name Tag.LWL";
    }

    public static class SeikoPrinters
    {
        public const string SLP = "SLP";
        public const string SmartLabelPrinter = "Smart Label Printer";
        public const string SmartLabelPrinter200 = "Smart Label Printer 200";
        public const string SmartLabelPrinter240 = "Smart Label Printer 240";
        public const string SmartLabelPrinter450 = "Smart Label Printer 450";
    }
    
    public enum LabelPrinterTypes
    {
        Dymo,
        Seiko
    }

    public class ValidLabelPrinters
    {
        public static List<LabelPrinter> Printers
        {
            get
            {
                return GetAllInstalledPrinters(null);
            }
        }

        /// <summary>
        /// Gets a list of all printers of specified type
        /// </summary>
        /// <param name="printerType"></param>
        /// <returns></returns>
        public static List<LabelPrinter> GetAllInstalledPrinters(LabelPrinterTypes? printerType)
        {
            List<LabelPrinter> printers = new List<LabelPrinter>();

            // Create a local print server
            LocalPrintServer ps = new LocalPrintServer();
            ps.Refresh();

            PrintQueueCollection printQueues = ps.GetPrintQueues(new EnumeratedPrintQueueTypes[] {
                EnumeratedPrintQueueTypes.Local });

            foreach (PrintQueue printer in printQueues)
            {
                if (printer.Name.Contains(SeikoPrinters.SmartLabelPrinter) || printer.Name.Contains(SeikoPrinters.SLP))
                {
                    printers.Add(new LabelPrinter()
                    {
                        Type = LabelPrinterTypes.Seiko,
                        Name = printer.Name,
                        TemplateFile = String.Empty
                    });
                }

                if (printer.Name.Contains(DymoPrinters.Dymo))
                {
                    printers.Add(new LabelPrinter()
                    {
                        Type = LabelPrinterTypes.Dymo,
                        Name = printer.Name,
                        TemplateFile = DymoPrinters.DymoLabelTemplateFile
                    });
                }
            }

            if (printerType.HasValue)
            {
                return printers.FindAll(p => p.Type.Equals(printerType.Value));
            }

            return printers;
        }

        //private void PurgePrintQueueButton_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        ShowProcessing(true);

        //        if (null == PrinterList.SelectedItems) { return; }

        //        List<PrintQueue> selectedPrinter =
        //            new List<PrintQueue>(PrinterList.SelectedItems.Cast<PrintQueue>());
        //        selectedPrinter[0].Refresh();
        //        PrintJobInfoCollection jobs = selectedPrinter[0].GetPrintJobInfoCollection();
        //        foreach (PrintSystemJobInfo job in jobs)
        //        {
        //            job.Cancel();
        //        }
        //    }
        //    finally
        //    {
        //        ShowProcessing(false);
        //    }
        //} 
    }

    public class LabelPrinter
    {
        public LabelPrinterTypes Type
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string TemplateFile
        {
            get;
            set;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() | Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            LabelPrinter p = (LabelPrinter)obj;
            return (Type == p.Type && Name == p.Name);
        }
    }    
}
