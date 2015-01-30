using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using ServiceAndDataContracts;

namespace CACCCheckIn.Modules.MOPS.Views
{
    public interface IMOPSCheckInView
    {
        Dispatcher ViewDispatcher { get; }
        List<CACCCheckInDb.PeopleWithDepartmentAndClassView> PeopleDataContext { get; set; }
        List<CACCCheckInDb.PeopleWithDepartmentAndClassView> FamilyDataContext { get; set; }
        void CheckInCompleted(CACCCheckInDb.PeopleWithDepartmentAndClassView person);
        void CheckInCompleted();
        void DisplayExceptionDetail(Exception ex);
    }
}
