using System;
using System.Collections.Generic;
using System.Text;

namespace CACCCheckIn.Printing.NameBadgePrinter.Common
{
    public interface INameBadgeLabel
    {
        void PrintLabels(LabelData labelData);
    }
}
