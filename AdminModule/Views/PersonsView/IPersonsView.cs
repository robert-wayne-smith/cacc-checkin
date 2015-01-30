using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace CACCCheckIn.Modules.Admin.Views
{
    public interface IPersonsView
    {
        Dispatcher ViewDispatcher { get; }
        List<CACCCheckInDb.Department> DepartmentsDataContext { get; set; }
        List<CACCCheckInDb.Class> ClassDataContext { get; set; }
        CACCCheckInDb.People PeopleDataContext { get; set; }
        CACCCheckInDb.People FamilyDataContext { get; set; }
        void DisplayExceptionDetail(Exception ex);
        CACCCheckInDb.Department CurrentDepartment { get; }
        CACCCheckInDb.Class CurrentClass { get; }
        void PersonInsertCompleted(CACCCheckInDb.Person person);
    }
}
