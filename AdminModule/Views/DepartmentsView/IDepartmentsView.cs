using System;
using System.Windows.Threading;

namespace CACCCheckIn.Modules.Admin.Views
{
    public interface IDepartmentsView
    {
        Dispatcher ViewDispatcher { get; }
        CACCCheckInDb.Departments DepartmentsDataContext { get; set; }
        CACCCheckInDb.ClassBindingList DepartmentClassesDataContext { get; set; }
        CACCCheckInDb.ClassBindingList UnassignedClassesDataContext { get; set; }
        void DisplayExceptionDetail(Exception ex);
    }
}
