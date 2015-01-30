using System;
using System.IO;
using System.Reflection;
using CACCCheckIn.Printing.NameBadgePrinter.Common;
using log4net;

namespace CACCCheckIn.Printing.NameBadgePrinter.SeikoSLPLabels
{
    public class NameBadgeLabel : INameBadgeLabel
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string SmartLabelCrossImage = "Cross only B&W.jpg";
        private string _printerName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="printerName"></param>
        public NameBadgeLabel(string printerName)
        {
            logger.DebugFormat("Creating instance of NameBadgeLabel for printer [{0}]",
                printerName);

            _printerName = printerName;
        }

        #region INameBadgeLabel Members

        void INameBadgeLabel.PrintLabels(LabelData labelData)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                PrintLabels64Bit(labelData);
            }
            else
            {
                PrintLabels32Bit(labelData);
            }
        }

        #endregion

        private void PrintLabels32Bit(LabelData labelData)
        {
            // When this is set to 'true', labels don't actually
            // print. They are just copied to Windows Clipboard. Useful
            // when debugging. Uncomment to use when debugging
            //SLP32.SlpDebugMode(true);

            logger.DebugFormat("Calling SlpOpenPrinter for printer [{0}]", _printerName);

            // Attempt to open the printer that is currently configured.
            // This should be a Seiko printer
            if (SLP32.SlpOpenPrinter(_printerName,
                SLP.Size_NameBadge, SLP.Orient_Landscape) == SLP.FALSE)
            {
                string errorMessage = String.Format("Could not open Seiko printer [{0}]. Error: [{1}]",
                    _printerName, SLP32.GetErrorCodeDescription());
                throw new ApplicationException(errorMessage);
            }

            // if we get here, printer was opened. Place rest in a try/finally block so that
            // we can close printer in finally.
            try
            {
                logger.Debug("Checking PrinterName for possible adjustments.");

                bool adjustForOldPrinter = false;
                // If the Seiko label printer is a 200 or 240 model, we need
                // to adjust the y coordinates of label information so that it
                // prints OK.
                if (_printerName.Contains("200") ||
                    _printerName.Contains("240"))
                {
                    adjustForOldPrinter = true;
                }

                logger.Debug("Calling SlpGetLabelWidth.");
                int labelWidth = SLP32.SlpGetLabelWidth();

                logger.Debug("Calling SlpCreateFont for various fonts.");
                string fontName = "Tahoma";
                int font14ptNormal = SLP32.SlpCreateFont(fontName, 14, SLP.Attr_Normal);
                int font16ptNormal = SLP32.SlpCreateFont(fontName, 16, SLP.Attr_Normal);
                int font16ptBold = SLP32.SlpCreateFont(fontName, 16, SLP.Attr_Bold);
                int font18ptBold = SLP32.SlpCreateFont(fontName, 18, SLP.Attr_Bold);
                int font22ptBold = SLP32.SlpCreateFont(fontName, 22, SLP.Attr_Bold);

                logger.Debug("Looping through each person to print label.");

                string labelImagePath = Path.Combine(Environment.CurrentDirectory,
                    SmartLabelCrossImage);

                // Loop through each Person in LabelData information that is 
                // contained in current label. Print label for each Person
                foreach (Person person in labelData.Persons)
                {
                    logger.Debug("Calling SlpStartLabel.");
                    if (SLP32.SlpStartLabel() != SLP.FALSE)
                    {
                        if (adjustForOldPrinter)
                        {
                            logger.DebugFormat("Drawing label image from: [{0}]", labelImagePath);
                            SLP32.SlpDrawPicture(5, 5, 105, 105, labelImagePath);

                            logger.Debug("Drawing Location on label.");
                            // First, print the Location
                            SLP32.SlpDrawTextXY(105, 5, font14ptNormal, labelData.Location);

                            logger.Debug("Drawing Name on label.");
                            // Next, print the Name. If width of Name is too big, step 
                            // down Font to smaller and print
                            int textWidth = SLP32.SlpGetTextWidth(font22ptBold, person.Name);
                            if (textWidth <= labelWidth)
                            {
                                // If Name will fit within label, print using normal Font for Name
                                SLP32.SlpDrawTextXY(-1, 110, font22ptBold, person.Name);
                            }
                            else
                            {
                                // If Name is too big for normal Name Font, step down to smaller Font and 
                                // print
                                SLP32.SlpDrawTextXY(-1, 110, font18ptBold, person.Name);
                            }

                            // After this, print the SecurityCode along with the Class
                            // If SecurityCode is not there, we will just print Class 
                            if (String.IsNullOrEmpty(labelData.SecurityCode))
                            {
                                logger.Debug("Drawing Class on label.");

                                SLP32.SlpDrawTextXY(-1, 170, font16ptBold,
                                   String.Format("{0}", person.Class));
                            }
                            else
                            {
                                logger.Debug("Drawing SecurityCode and Class on label.");
                                SLP32.SlpDrawTextXY(-1, 170, font16ptBold,
                                    String.Format("{0} -- {1}", labelData.SecurityCode, person.Class));
                            }

                            // Print Date next if specified
                            if (labelData.Date.HasValue)
                            {
                                logger.Debug("Drawing Date on label.");
                                SLP32.SlpDrawTextXY(-1, 230, font14ptNormal,
                                    labelData.Date.Value.ToString("ddd MM/dd/yy"));
                            }

                            // Print SpecialConditions if any were specified
                            if (!String.IsNullOrEmpty(person.SpecialConditions))
                            {
                                logger.Debug("Drawing SpecialConditions on label.");
                                SLP32.SlpDrawTextXY(-1, 290, font14ptNormal, person.SpecialConditions);
                            }
                        }
                        else
                        {
                            logger.DebugFormat("Drawing label image from: [{0}]", labelImagePath);
                            SLP32.SlpDrawPicture(5, 5, 115, 115, labelImagePath);

                            logger.Debug("Drawing Location on label.");
                            // First, print the Location
                            SLP32.SlpDrawTextXY(120, 5, font16ptNormal, labelData.Location);

                            logger.Debug("Drawing Name on label.");
                            // Next, print the Name. If width of Name is too big, step 
                            // down Font to smaller and print
                            int textWidth = SLP32.SlpGetTextWidth(font22ptBold, person.Name);
                            if (textWidth <= labelWidth)
                            {
                                // If Name will fit within label, print using normal Font for Name
                                SLP32.SlpDrawTextXY(-1, 145, font22ptBold, person.Name);
                            }
                            else
                            {
                                // If Name is too big for normal Name Font, step down to smaller Font and 
                                // print
                                SLP32.SlpDrawTextXY(-1, 145, font18ptBold, person.Name);
                            }

                            // After this, print the SecurityCode along with the Class
                            // If SecurityCode is not there, we will just print Class 
                            if (String.IsNullOrEmpty(labelData.SecurityCode))
                            {
                                logger.Debug("Drawing Class on label.");

                                SLP32.SlpDrawTextXY(-1, 235, font16ptBold,
                                   String.Format("{0}", person.Class));
                            }
                            else
                            {
                                logger.Debug("Drawing SecurityCode and Class on label.");
                                SLP32.SlpDrawTextXY(-1, 235, font16ptBold,
                                    String.Format("{0} -- {1}", labelData.SecurityCode, person.Class));
                            }

                            // Print Date next if specified
                            if (labelData.Date.HasValue)
                            {
                                logger.Debug("Drawing Date on label.");
                                SLP32.SlpDrawTextXY(-1, 310, font14ptNormal,
                                    labelData.Date.Value.ToString("ddd MM/dd/yy"));
                            }

                            // Print SpecialConditions if any were specified
                            if (!String.IsNullOrEmpty(person.SpecialConditions))
                            {
                                logger.Debug("Drawing SpecialConditions on label.");
                                SLP32.SlpDrawTextXY(-1, 395, font14ptNormal, person.SpecialConditions);
                            }
                        }

                        logger.Debug("Calling SlpEndLabel.");
                        SLP32.SlpEndLabel();
                    }
                }

                logger.Debug("Calling SlpDeleteFont to delete various fonts.");
                SLP32.SlpDeleteFont(font14ptNormal);
                SLP32.SlpDeleteFont(font16ptNormal);
                SLP32.SlpDeleteFont(font16ptBold);
                SLP32.SlpDeleteFont(font18ptBold);
                SLP32.SlpDeleteFont(font22ptBold);
            }
            finally
            {
                logger.Debug("Calling SlpClosePrinter.");
                // Close the open printer
                SLP32.SlpClosePrinter();
            }
        }

        private void PrintLabels64Bit(LabelData labelData)
        {
            // When this is set to 'true', labels don't actually
            // print. They are just copied to Windows Clipboard. Useful
            // when debugging. Uncomment to use when debugging
            //SLP64.SlpDebugMode(true);

            logger.DebugFormat("Calling SlpOpenPrinter for printer [{0}]", _printerName);

            // Attempt to open the printer that is currently configured.
            // This should be a Seiko printer
            if (SLP64.SlpOpenPrinter(_printerName,
                SLP.Size_NameBadge, SLP.Orient_Landscape) == SLP.FALSE)
            {
                string errorMessage = String.Format("Could not open Seiko printer [{0}]. Error: [{1}]",
                    _printerName, SLP64.GetErrorCodeDescription());
                throw new ApplicationException(errorMessage);
            }

            // if we get here, printer was opened. Place rest in a try/finally block so that
            // we can close printer in finally.
            try
            {
                logger.Debug("Checking PrinterName for possible adjustments.");

                bool adjustForOldPrinter = false;
                // If the Seiko label printer is a 200 or 240 model, we need
                // to adjust the y coordinates of label information so that it
                // prints OK.
                if (_printerName.Contains("200") ||
                    _printerName.Contains("240"))
                {
                    adjustForOldPrinter = true;
                }

                logger.Debug("Calling SlpGetLabelWidth.");
                int labelWidth = SLP64.SlpGetLabelWidth();

                logger.Debug("Calling SlpCreateFont for various fonts.");
                string fontName = "Tahoma";
                int font14ptNormal = SLP64.SlpCreateFont(fontName, 14, SLP.Attr_Normal);
                int font16ptNormal = SLP64.SlpCreateFont(fontName, 16, SLP.Attr_Normal);
                int font16ptBold = SLP64.SlpCreateFont(fontName, 16, SLP.Attr_Bold);
                int font18ptBold = SLP64.SlpCreateFont(fontName, 18, SLP.Attr_Bold);
                int font22ptBold = SLP64.SlpCreateFont(fontName, 22, SLP.Attr_Bold);

                logger.Debug("Looping through each person to print label.");

                string labelImagePath = Path.Combine(Environment.CurrentDirectory,
                    SmartLabelCrossImage);

                // Loop through each Person in LabelData information that is 
                // contained in current label. Print label for each Person
                foreach (Person person in labelData.Persons)
                {
                    logger.Debug("Calling SlpStartLabel.");
                    if (SLP64.SlpStartLabel() != SLP.FALSE)
                    {
                        if (adjustForOldPrinter)
                        {
                            logger.DebugFormat("Drawing label image from: [{0}]", labelImagePath);
                            SLP64.SlpDrawPicture(5, 5, 105, 105, labelImagePath);

                            logger.Debug("Drawing Location on label.");
                            // First, print the Location
                            SLP64.SlpDrawTextXY(105, 5, font14ptNormal, labelData.Location);

                            logger.Debug("Drawing Name on label.");
                            // Next, print the Name. If width of Name is too big, step 
                            // down Font to smaller and print
                            int textWidth = SLP64.SlpGetTextWidth(font22ptBold, person.Name);
                            if (textWidth <= labelWidth)
                            {
                                // If Name will fit within label, print using normal Font for Name
                                SLP64.SlpDrawTextXY(-1, 110, font22ptBold, person.Name);
                            }
                            else
                            {
                                // If Name is too big for normal Name Font, step down to smaller Font and 
                                // print
                                SLP64.SlpDrawTextXY(-1, 110, font18ptBold, person.Name);
                            }

                            // After this, print the SecurityCode along with the Class
                            // If SecurityCode is not there, we will just print Class 
                            if (String.IsNullOrEmpty(labelData.SecurityCode))
                            {
                                logger.Debug("Drawing Class on label.");

                                SLP64.SlpDrawTextXY(-1, 170, font16ptBold,
                                   String.Format("{0}", person.Class));
                            }
                            else
                            {
                                logger.Debug("Drawing SecurityCode and Class on label.");
                                SLP64.SlpDrawTextXY(-1, 170, font16ptBold,
                                    String.Format("{0} -- {1}", labelData.SecurityCode, person.Class));
                            }

                            // Print Date next if specified
                            if (labelData.Date.HasValue)
                            {
                                logger.Debug("Drawing Date on label.");
                                SLP64.SlpDrawTextXY(-1, 230, font14ptNormal,
                                    labelData.Date.Value.ToString("ddd MM/dd/yy"));
                            }

                            // Print SpecialConditions if any were specified
                            if (!String.IsNullOrEmpty(person.SpecialConditions))
                            {
                                logger.Debug("Drawing SpecialConditions on label.");
                                SLP64.SlpDrawTextXY(-1, 290, font14ptNormal, person.SpecialConditions);
                            }
                        }
                        else
                        {
                            logger.DebugFormat("Drawing label image from: [{0}]", labelImagePath);
                            SLP64.SlpDrawPicture(5, 5, 115, 115, labelImagePath);

                            logger.Debug("Drawing Location on label.");
                            // First, print the Location
                            SLP64.SlpDrawTextXY(120, 5, font16ptNormal, labelData.Location);

                            logger.Debug("Drawing Name on label.");
                            // Next, print the Name. If width of Name is too big, step 
                            // down Font to smaller and print
                            int textWidth = SLP64.SlpGetTextWidth(font22ptBold, person.Name);
                            if (textWidth <= labelWidth)
                            {
                                // If Name will fit within label, print using normal Font for Name
                                SLP64.SlpDrawTextXY(-1, 145, font22ptBold, person.Name);
                            }
                            else
                            {
                                // If Name is too big for normal Name Font, step down to smaller Font and 
                                // print
                                SLP64.SlpDrawTextXY(-1, 145, font18ptBold, person.Name);
                            }

                            // After this, print the SecurityCode along with the Class
                            // If SecurityCode is not there, we will just print Class 
                            if (String.IsNullOrEmpty(labelData.SecurityCode))
                            {
                                logger.Debug("Drawing Class on label.");

                                SLP64.SlpDrawTextXY(-1, 235, font16ptBold,
                                   String.Format("{0}", person.Class));
                            }
                            else
                            {
                                logger.Debug("Drawing SecurityCode and Class on label.");
                                SLP64.SlpDrawTextXY(-1, 235, font16ptBold,
                                    String.Format("{0} -- {1}", labelData.SecurityCode, person.Class));
                            }

                            // Print Date next if specified
                            if (labelData.Date.HasValue)
                            {
                                logger.Debug("Drawing Date on label.");
                                SLP64.SlpDrawTextXY(-1, 310, font14ptNormal,
                                    labelData.Date.Value.ToString("ddd MM/dd/yy"));
                            }

                            // Print SpecialConditions if any were specified
                            if (!String.IsNullOrEmpty(person.SpecialConditions))
                            {
                                logger.Debug("Drawing SpecialConditions on label.");
                                SLP64.SlpDrawTextXY(-1, 395, font14ptNormal, person.SpecialConditions);
                            }
                        }

                        logger.Debug("Calling SlpEndLabel.");
                        SLP64.SlpEndLabel();
                    }
                }

                logger.Debug("Calling SlpDeleteFont to delete various fonts.");
                SLP64.SlpDeleteFont(font14ptNormal);
                SLP64.SlpDeleteFont(font16ptNormal);
                SLP64.SlpDeleteFont(font16ptBold);
                SLP64.SlpDeleteFont(font18ptBold);
                SLP64.SlpDeleteFont(font22ptBold);
            }
            finally
            {
                logger.Debug("Calling SlpClosePrinter.");
                // Close the open printer
                SLP64.SlpClosePrinter();
            }
        }
    }
}
