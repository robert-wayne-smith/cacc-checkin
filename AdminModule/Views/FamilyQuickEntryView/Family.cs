using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CACCCheckIn.Modules.Admin.Views
{
    public class Family
    {
        private List<CACCCheckInDb.PeopleWithDepartmentAndClassView> _familyMembers;

        public Family()
        {
            _familyMembers = new List<CACCCheckInDb.PeopleWithDepartmentAndClassView>();
        }

        public Guid Id { get; set; }

        public string Name { get; set; }
        
        public List<CACCCheckInDb.PeopleWithDepartmentAndClassView> Members
        { 
            get
            {
                return _familyMembers;
            }
        }

        public string MemberFirstNameList
        {
            get
            {
                List<string> firstNames = new List<string>();
                foreach (CACCCheckInDb.PeopleWithDepartmentAndClassView member in _familyMembers)
                {
                    firstNames.Add(member.FirstName);
                }
                
                return String.Join("; ", firstNames);
            }
        }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            Family f = (Family)obj;
            return (Id == f.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{0} ({1})", Name, MemberFirstNameList);
        }
    }
}
