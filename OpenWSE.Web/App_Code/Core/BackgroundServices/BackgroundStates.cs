using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenWSE_Library.Core.BackgroundServices {

    public enum BackgroundStates {
        Stopped,
        Running,
        Sleeping,
        Stopping,
        Error
    };

}
