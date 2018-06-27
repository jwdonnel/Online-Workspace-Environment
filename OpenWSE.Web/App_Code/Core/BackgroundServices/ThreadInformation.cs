using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenWSE_Library.Core.BackgroundServices {

    public class ThreadInformation {
        public int ManagedId {
            get;
            set;
        }
        public string ThreadName {
            get;
            set;
        }
        public DateTime LastTime {
            get;
            set;
        }
        public TimeSpan LastTotalProcessorTime {
            get;
            set;
        }
        public double CpuUsage {
            get;
            set;
        }
    }

}
