using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace CACCCheckIn.Modules.Admin.Views
{
    public interface ITeachersView
    {
        Dispatcher ViewDispatcher { get; }
        List<CACCCheckInDb.Department> DepartmentDataContext { get; set; }
        List<CACCCheckInDb.Class> ClassDataContext { get; set; }
        ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView> TeachersDataContext { get; set; }
        ObservableCollection<CACCCheckInDb.PeopleWithDepartmentAndClassView> AdultsDataContext { get; set; }
        void DisplayExceptionDetail(Exception ex);
        void LabelPrintingCompleted();
    }
}
