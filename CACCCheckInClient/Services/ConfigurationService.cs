using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceAndDataContracts;

namespace CACCCheckInClient.Services
{
    public class ConfigurationService : IConfigurationService
    {
        #region IConfigurationService Members

        public string GetTargetDepartment()
        {
            return Properties.Settings.Default.TargetDepartment;
        }

        #endregion
    }
}
