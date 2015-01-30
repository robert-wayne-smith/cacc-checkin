using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace CACCCheckIn.Modules.Admin.Views
{
    public interface IFamilyQuickEntryView
    {
        Dispatcher ViewDispatcher { get; }
        List<CACCCheckInDb.Department> DepartmentsDataContext { get; set; }
        List<CACCCheckInDb.Class> DepartmentClassesDataContext { get; set; }
        List<Family> FamiliesDataContext { get; set; }
        void CheckInCompleted(CACCCheckInDb.PeopleWithDepartmentAndClassView person);
        void ProcessingCompleted();
        void DisplayExceptionDetail(Exception ex);
    }
}
