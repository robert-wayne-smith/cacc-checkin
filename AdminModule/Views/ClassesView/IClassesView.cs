using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace CACCCheckIn.Modules.Admin.Views
{
    public interface IClassesView
    {
        Dispatcher ViewDispatcher { get; }
        List<CACCCheckInDb.Department> DepartmentsDataContext { get; set; }
        CACCCheckInDb.Classes ClassesDataContext { get; set; }
        void DisplayExceptionDetail(Exception ex);
        CACCCheckInDb.Department CurrentDepartment { get; }
    }
}
