using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using ServiceAndDataContracts;

namespace CACCCheckIn.Modules.Admin.Views
{
    public interface IReportsView
    {
        Dispatcher ViewDispatcher { get; }
    }
}

