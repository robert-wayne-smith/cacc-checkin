using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using ServiceAndDataContracts;

namespace CACCCheckIn.Modules.Admin.Views
{
    public interface IClassMovementView
    {
        Dispatcher ViewDispatcher { get; }
        List<CACCCheckInDb.Department> DepartmentFromDataContext { get; set; }
        List<CACCCheckInDb.Class> ClassFromDataContext { get; set; }
        List<CACCCheckInDb.Department> DepartmentToDataContext { get; set; }
        List<CACCCheckInDb.Class> ClassToDataContext { get; set; }
        ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView> ClassFromListDataContext { get; set; }
        ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView> ClassToListDataContext { get; set; }
        void DisplayExceptionDetail(Exception ex);
    }
}
