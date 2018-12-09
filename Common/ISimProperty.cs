using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIM.Connect.Common
{
    public interface ISimProperty
    {
        IDataProvider Provider { get; }
        Enum RequestID { get; }
        Enum SimObjectID { get; }
    }
}
