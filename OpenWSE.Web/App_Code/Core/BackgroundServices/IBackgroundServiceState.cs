using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenWSE_Library.Core.BackgroundServices {
    public interface IBackgroundServiceState {

        /// <summary> Start the Background Service
        /// </summary>
        void StartService();

        /// <summary> Stop the Background Service
        /// </summary>
        void StopService();

    }

}
