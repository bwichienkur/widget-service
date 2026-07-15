using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core
{
    public interface IFileSerializeService
    {
        string GetFileContents(string directory, string fileName);
    }
}
