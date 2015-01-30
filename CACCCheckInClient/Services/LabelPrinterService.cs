using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ServiceAndDataContracts;
using CACCCheckIn.Printing.NameBadgePrinter;
using CACCCheckIn.Printing.NameBadgePrinter.Common;
using log4net;

namespace CACCCheckInClient.Services
{
    public class LabelPrinterService : ILabelPrinterService
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private LabelPrinter GetLabelPrinter()
        {
            LabelPrinter labelPrinter = new LabelPrinter();
            labelPrinter.Type = (LabelPrinterTypes)Enum.Parse(typeof(LabelPrinterTypes),
                Properties.Settings.Default.PrinterType);
            labelPrinter.Name = Properties.Settings.Default.PrinterName;
            if (labelPrinter.Type == LabelPrinterTypes.Dymo)
            {
                labelPrinter.TemplateFile = Properties.Settings.Default.LabelTemplateFile;
            }

            logger.DebugFormat("Retrieving label printer: Name=[{0}]",
                labelPrinter.Name);
            return labelPrinter;
        }

        #region ILabelPrinterService Members

        /// <summary>
        /// Prints label for one person
        /// </summary>
        /// <param name="date"></param>
        /// <param name="location"></param>
        /// <param name="securityCode"></param>
        /// <param name="person"></param>
        public void PrintLabels(DateTime? date, string location, int securityCode,
            CACCCheckInDb.PeopleWithDepartmentAndClassView person)
        {
            List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people = new List<CACCCheckInDb.PeopleWithDepartmentAndClassView>();
            people.Add(person);

            logger.DebugFormat("Printing label for person: Name=[{0} {1}]",
                person.FirstName, person.LastName);

            PrintLabels(date, location, securityCode, people);             
        }

        /// <summary>
        /// Prints labels for list of people using same date, location
        /// and security code
        /// </summary>
        /// <param name="date"></param>
        /// <param name="location"></param>
        /// <param name="securityCode"></param>
        /// <param name="people"></param>
        public void PrintLabels(DateTime? date, string location, int securityCode,
            List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people)
        {
            LabelData labelData = new LabelData();
            labelData.Date = date;
            labelData.Location = location;
            labelData.SecurityCode = securityCode.Equals(0) ? 
                String.Empty : securityCode.ToString();
            foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView person in people)
            {
                logger.DebugFormat("Printing label for person: Name=[{0} {1}]",
                    person.FirstName, person.LastName);

                labelData.Persons.Add(new CACCCheckIn.Printing.NameBadgePrinter.Common.Person
                {
                    Name = String.Format("{0} {1}", person.FirstName, person.LastName),
                    Class = person.ClassName,
                    SpecialConditions = person.SpecialConditions
                });
            }

            Printer nameBadgePrinter = new Printer(GetLabelPrinter());
            nameBadgePrinter.PrintLabels(labelData);
        }

        #endregion
    }
}
