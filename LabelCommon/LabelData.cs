using System;
using System.Collections.Generic;

namespace CACCCheckIn.Printing.NameBadgePrinter.Common
{
    public class Person
    {
        private string name; 
        private string department;        
        private string specialConditions;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Class
        {
            get { return department; }
            set { department = value; }
        }

        public string SpecialConditions
        {
            get { return specialConditions; }
            set { specialConditions = value; }
        }
    }

    public class LabelData
    {
        private string securityCode;
        private DateTime? date;        
        private string location;
        private List<Person> persons = new List<Person>();

        public string Location
        {
            get { return location; }
            set { location = value; }	
        }

        public DateTime? Date
        {
            get { return date; }
            set { date = value; }
        }

        public string  SecurityCode
        {
            get { return securityCode; }
            set { securityCode = value; }
        }

        public List<Person> Persons
        {
            get { return persons; }
        }
    }
}
