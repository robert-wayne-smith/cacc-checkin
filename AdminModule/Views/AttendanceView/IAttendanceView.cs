using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace CACCCheckIn.Modules.Admin.Views
{
    public interface IAttendanceView
    {
        Dispatcher ViewDispatcher { get; }
        List<CACCCheckInDb.Department> DepartmentsDataContext { get; set; }
        CACCCheckInDb.AttendanceRecords AttendanceDataContext { get; set; }
        void ProcessingCompleted();
        void DisplayExceptionDetail(Exception ex);
    }
}
