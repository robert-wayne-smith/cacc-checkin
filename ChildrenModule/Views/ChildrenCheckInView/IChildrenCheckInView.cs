using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using ServiceAndDataContracts;

namespace CACCCheckIn.Modules.Children.Views
{
    public interface IChildrenCheckInView
    {
        Dispatcher ViewDispatcher { get; }
        List<CACCCheckInDb.PeopleWithDepartmentAndClassView> PeopleDataContext { get; set; }
        List<CACCCheckInDb.Class> ClassesDataContext { get; set; }
        void CheckInCompleted(CACCCheckInDb.PeopleWithDepartmentAndClassView person);
        void DisplayExceptionDetail(Exception ex);
    }
}
