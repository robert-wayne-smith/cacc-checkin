using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using ServiceAndDataContracts;

namespace CACCCheckIn.Modules.Admin.Views
{
    public interface IReportPresenterView
    {
        Dispatcher ViewDispatcher { get; }
        List<CACCCheckInDb.Department> DepartmentsDataContext { get; set; }
        List<CACCCheckInDb.Class> DepartmentClassesDataContext { get; set; }
        List<CACCCheckInDb.PeopleWithDepartmentAndClassView> DepartmentPeopleDataContext { get; set; }
        List<CACCCheckInDb.AttendanceWithDetail> ReportDataContext { set; }
        void DisplayExceptionDetail(Exception ex);
    }
}

