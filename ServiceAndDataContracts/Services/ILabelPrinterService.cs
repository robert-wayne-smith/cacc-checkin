using System;
using System.Collections.Generic;

namespace ServiceAndDataContracts
{
    public interface ILabelPrinterService
    {
        void PrintLabels(DateTime? date, string location, int securityCode,
            CACCCheckInDb.PeopleWithDepartmentAndClassView person);

        void PrintLabels(DateTime? date, string location, int securityCode,
            List<CACCCheckInDb.PeopleWithDepartmentAndClassView> people);
    }
}
