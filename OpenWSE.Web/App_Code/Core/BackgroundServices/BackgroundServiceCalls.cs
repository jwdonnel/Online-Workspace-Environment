using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenWSE_Library.Core.BackgroundServices {

    public class BackgroundServiceCalls {

        #region Private Variables

        private const string BackgroundStates_NAME = "BackgroundStates";
        private const string CurrentState_NAME = "CurrentState";
        private const string BackgroundServiceStateInterface_NAMESPACE = "IBackgroundServiceState";
        private const string BackgroundServiceLog_NAMESPACE = "OpenWSE_Tools.Logging.BackgroundServiceLog";
        private const string BackgroundServices_NAMESPACE = "OpenWSE_Tools.BackgroundServiceDatabaseCalls.BackgroundServices";
        private const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Static;

        #endregion


        #region Public/Protected Methods

        /// <summary> Get the current state of the Service from the Assembly
        /// </summary>
        /// <param name="fullNamespace">Full Namespace</param>
        /// <returns></returns>
        protected BackgroundStates GetCurrentStateOfItem(string fullNamespace) {
            bool errorOccured = false;

            try {
                Type serviceAssemblyType = GetAssemblyType(fullNamespace);
                if (serviceAssemblyType != null && serviceAssemblyType.GetInterface(BackgroundServiceStateInterface_NAMESPACE) != null) {
                    PropertyInfo[] properties = serviceAssemblyType.GetProperties(flags);
                    foreach (PropertyInfo pi in properties) {
                        if (pi.Name == CurrentState_NAME && pi.PropertyType.Name == BackgroundStates_NAME) {
                            object value = pi.GetValue(null);

                            if (value != null) {
                                BackgroundStates result = BackgroundStates.Stopped;
                                Enum.TryParse<BackgroundStates>(value.ToString(), out result);
                                return result;
                            }
                            else {
                                errorOccured = true;
                            }

                            break;
                        }
                    }
                }
                else {
                    errorOccured = true;
                }
            }
            catch (Exception ex) {
                AttemptLogService(true, fullNamespace, ex.Message);
                errorOccured = true;
            }

            if (errorOccured) {
                return BackgroundStates.Error;
            }

            return BackgroundStates.Stopped;
        }

        /// <summary> Set the current state of the Service from the Assembly
        /// </summary>
        /// <param name="fullNamespace">Full Namespace</param>
        /// <param name="state">BackgroundState to set service to</param>
        /// <returns></returns>
        protected bool SetCurrentStateOfItem(string fullNamespace, BackgroundStates state, bool logInformation) {
            bool successfullUpdate = false;

            Type serviceAssemblyType = GetAssemblyType(fullNamespace);
            if (serviceAssemblyType == null || serviceAssemblyType.GetInterface(BackgroundServiceStateInterface_NAMESPACE) == null) {
                // Log the error
                string message = "The background service being called does not inherit the " + BackgroundServiceStateInterface_NAMESPACE + " interface or was simply was not able to get the service assembly. This service will not run until this is fixed.";
                AttemptLogService(logInformation, fullNamespace, message);

                return false;
            }

            #region If Service Assembly is found, attempt to proceed
            try {
                successfullUpdate = SetPropertyState(fullNamespace, state, serviceAssemblyType, logInformation);

                string message = string.Format("<span class='font-bold' style='color: green;'>Successfully</span> updated background state to <span class='state-{0}'>{0}</span>.", state);
                if (!successfullUpdate) {
                    message = string.Format("<span class='font-bold' style='color: red;'>Failed</span> to update the background state to <span class='state-{0}'>{0}</span>.", state);
                }

                // Log the update
                AttemptLogService(logInformation, fullNamespace, message);

                // See if the service has the StartService and StopService Interface methods
                // These methods are not required
                if (successfullUpdate) {
                    ConstructorInfo serviceConstructor = serviceAssemblyType.GetConstructor(Type.EmptyTypes);
                    if (serviceConstructor != null) {
                        object serviceClassObject = serviceConstructor.Invoke(new object[] { });
                        if (serviceClassObject != null) {
                            string methodToCall = string.Empty;

                            switch (state) {
                                case BackgroundStates.Sleeping:
                                case BackgroundStates.Running:
                                    methodToCall = "StartService";
                                    break;

                                case BackgroundStates.Error:
                                case BackgroundStates.Stopped:
                                case BackgroundStates.Stopping:
                                    methodToCall = "StopService";
                                    break;
                            }

                            if (!string.IsNullOrEmpty(methodToCall)) {
                                MethodInfo serviceMethod = serviceAssemblyType.GetMethod(methodToCall);

                                if (serviceMethod != null) {
                                    serviceMethod.Invoke(serviceClassObject, null);

                                    // Log the Successful call
                                    message = string.Format("<span class='font-bold' style='color: green;'>Successfully</span> called the <span class='font-bold'>{0}</span> Interface.", methodToCall);
                                    AttemptLogService(logInformation, fullNamespace, message);
                                }
                                else {
                                    successfullUpdate = false;

                                    // Log the Failed call
                                    message = string.Format("<span class='font-bold' style='color: red;'>Failed</span> to called the <span class='font-bold'>{0}</span> Interface.", methodToCall);
                                    AttemptLogService(logInformation, fullNamespace, message);
                                }
                            }
                        }
                        else {
                            successfullUpdate = false;
                        }
                    }

                    if (!successfullUpdate) {
                        SetPropertyState(fullNamespace, BackgroundStates.Error, serviceAssemblyType, logInformation);
                        successfullUpdate = true;
                    }
                }
            }
            catch (Exception e) {
                if (successfullUpdate) {
                    SetPropertyState(fullNamespace, BackgroundStates.Error, serviceAssemblyType, logInformation);
                }

                // Log the Failed call
                AttemptLogService(logInformation, fullNamespace, "<span class='state-Error pad-right-sml'>{1}:</span>" + e.Message);
            }
            #endregion

            return successfullUpdate;
        }
        private bool SetPropertyState(string fullNamespace, BackgroundStates state, Type serviceAssemblyType, bool logInformation) {
            try {
                PropertyInfo stateProperty = serviceAssemblyType.GetProperty(CurrentState_NAME, flags);
                if (stateProperty != null && stateProperty.PropertyType.Name == BackgroundStates_NAME) {
                    stateProperty.SetValue(null, state);
                    return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary> Get the Service Namespace from the stream of a fileupload
        /// </summary>
        /// <param name="serviceStream">File upload content stream</param>
        /// <returns></returns>
        public static string GetServiceStreamNamespace(Stream serviceStream) {
            string serviceNamespace = string.Empty;

            if (serviceStream != null) {
                try {
                    byte[] data = new byte[serviceStream.Length];
                    serviceStream.Read(data, 0, data.Length);
                    Assembly serviceAssembly = Assembly.Load(data);
                    if (serviceAssembly != null) {
                        IEnumerable<Type> types = serviceAssembly.ExportedTypes;
                        foreach (Type serviceAssemblyType in types) {
                            bool hasStateProperty = false;
                            PropertyInfo[] properties = serviceAssemblyType.GetProperties(flags);
                            foreach (PropertyInfo pi in properties) {
                                if (pi.Name == CurrentState_NAME && pi.PropertyType.Name == BackgroundStates_NAME) {
                                    hasStateProperty = true;
                                    break;
                                }
                            }

                            if (hasStateProperty && serviceAssemblyType.GetInterface(BackgroundServiceStateInterface_NAMESPACE) != null) {
                                serviceNamespace = serviceAssemblyType.FullName;
                            }
                        }
                    }
                }
                catch { }
            }

            return serviceNamespace;
        }

        #endregion


        #region Thread Information

        private static Dictionary<int, ThreadInformation> ThreadInformationList = new Dictionary<int, ThreadInformation>();

        /// <summary> Call this method in the StartService method
        /// </summary>
        /// <param name="threadStart"></param>
        /// <param name="serviceNamespace"></param>
        /// <returns></returns>
        public static Thread CreateServiceThread(ThreadStart threadStart, string serviceNamespace) {
            Thread serviceThread = new Thread(threadStart);
            serviceThread.Name = serviceNamespace;
            serviceThread.IsBackground = true;
            return serviceThread;
        }

        /// <summary> Call this method once you start your thread and are in the ThreadStart method
        /// </summary>
        public static void UpdateThreadInformation() {
            if (Thread.CurrentThread != null) {
                int currThreadId = Thread.CurrentThread.ManagedThreadId;
                if (ThreadInformationList.ContainsKey(currThreadId)) {
                    ThreadInformationList[currThreadId].ManagedId = Thread.CurrentThread.ManagedThreadId;
                    ThreadInformationList[currThreadId].ThreadName = Thread.CurrentThread.Name;
                }
                else {
                    ThreadInformationList.Add(currThreadId, new ThreadInformation() {
                        ManagedId = Thread.CurrentThread.ManagedThreadId,
                        ThreadName = Thread.CurrentThread.Name
                    });
                }
            }
        }

        protected double GetCurrentCpuUsage(string fullNamespace) {
            foreach (KeyValuePair<int, ThreadInformation> info in ThreadInformationList) {
                if (info.Value.ThreadName == fullNamespace) {
                    return info.Value.CpuUsage;
                }
            }

            return 0.0d;
        }

        public static void UpdateCurrentCpuUsage() {
            Process currentProcess = Process.GetCurrentProcess();
            foreach (ProcessThread processThread in currentProcess.Threads) {
                if (ThreadInformationList.ContainsKey(processThread.Id)) {
                    try {
                        DateTime lastTime = ThreadInformationList[processThread.Id].LastTime;
                        TimeSpan lastTotalProcessorTime = ThreadInformationList[processThread.Id].LastTotalProcessorTime;

                        DateTime currTime = DateTime.Now;
                        TimeSpan currTotalProcessorTime = processThread.TotalProcessorTime;

                        double CPUUsage = (currTotalProcessorTime.TotalMilliseconds - lastTotalProcessorTime.TotalMilliseconds) / currTime.Subtract(lastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);
                        double CurrentCpuUsage = CPUUsage * 100;
                        if (CurrentCpuUsage > 100.0d) {
                            CurrentCpuUsage = 100.0d;
                        }
                        else if (CurrentCpuUsage < 0.0d || CurrentCpuUsage.ToString() == "NaN") {
                            CurrentCpuUsage = 0.0d;
                        }

                        ThreadInformationList[processThread.Id].LastTime = currTime;
                        ThreadInformationList[processThread.Id].LastTotalProcessorTime = currTotalProcessorTime;
                        ThreadInformationList[processThread.Id].CpuUsage = CurrentCpuUsage;
                    }
                    catch (Exception ex) {
                        AttemptLogService(true, ThreadInformationList[processThread.Id].ThreadName, ex.Message);
                    }
                }
            }
        }

        #endregion


        #region Log Info Methods

        /// <summary> You can attempt to log a message from the Background Service by calling this method
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void AttemptLogMessage(string message) {
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            if (callingAssembly == null && callingAssembly.GetTypes().Length > 0) {
                return;
            }

            string fullNamespace = string.Empty;

            Type[] types = callingAssembly.GetTypes();
            foreach (Type type in types) {
                if (type.GetInterface(BackgroundServiceStateInterface_NAMESPACE) != null) {
                    fullNamespace = type.FullName;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(fullNamespace)) {
                bool logInfo = CheckIfLogServiceIsOn(fullNamespace);
                if (logInfo) {
                    AttemptLogService(true, fullNamespace, message);
                }
            }
        }

        /// <summary>You can attempt to log a message from the Background Service by calling this method
        /// </summary>
        /// <param name="serviceNamespace">Service's full namespace</param>
        /// <param name="message">Message to log</param>
        public static void AttemptLogMessage(string serviceNamespace, string message) {
            if (!string.IsNullOrEmpty(serviceNamespace)) {
                AttemptLogService(true, serviceNamespace, message);
            }
        }

        /// <summary> You can attempt to update the Last Updated Date for the current background service
        /// </summary>
        public static void AttemptUpdateServiceStateDate() {
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            if (callingAssembly == null && callingAssembly.GetTypes().Length > 0) {
                return;
            }

            string fullNamespace = string.Empty;

            Type[] types = callingAssembly.GetTypes();
            foreach (Type type in types) {
                if (type.GetInterface(BackgroundServiceStateInterface_NAMESPACE) != null) {
                    fullNamespace = type.FullName;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(fullNamespace)) {
                AttemptUpdateServiceDate(fullNamespace);
            }
        }

        private static void AttemptLogService(bool logInformation, string fullNamespace, string message) {
            if (!logInformation) {
                return;
            }

            Type serviceAssemblyType = GetAssemblyType(BackgroundServiceLog_NAMESPACE);
            if (serviceAssemblyType != null) {
                try {
                    ConstructorInfo serviceConstructor = serviceAssemblyType.GetConstructor(Type.EmptyTypes);
                    if (serviceConstructor != null) {
                        object serviceClassObject = serviceConstructor.Invoke(new object[] { });
                        if (serviceClassObject != null) {
                            MethodInfo AddItem = serviceAssemblyType.GetMethod("AddItem");
                            if (AddItem != null) {
                                AddItem.Invoke(serviceClassObject, new object[] { fullNamespace, message });
                            }
                        }
                    }
                }
                catch { }
            }
        }
        private static bool CheckIfLogServiceIsOn(string fullNamespace) {
            Type serviceAssemblyType = GetAssemblyType(BackgroundServiceLog_NAMESPACE);
            if (serviceAssemblyType != null) {
                try {
                    ConstructorInfo serviceConstructor = serviceAssemblyType.GetConstructor(Type.EmptyTypes);
                    if (serviceConstructor != null) {
                        object serviceClassObject = serviceConstructor.Invoke(new object[] { });
                        if (serviceClassObject != null) {
                            MethodInfo AddItem = serviceAssemblyType.GetMethod("DoesLogInformation");
                            if (AddItem != null) {
                                object logInfo = AddItem.Invoke(serviceClassObject, new object[] { fullNamespace });
                                if (logInfo != null) {
                                    return Convert.ToBoolean(logInfo);
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return false;
        }
        private static void AttemptUpdateServiceDate(string fullNamespace) {
            Type serviceAssemblyType = GetAssemblyType(BackgroundServiceLog_NAMESPACE);
            if (serviceAssemblyType != null) {
                try {
                    ConstructorInfo serviceConstructor = serviceAssemblyType.GetConstructor(Type.EmptyTypes);
                    if (serviceConstructor != null) {
                        object serviceClassObject = serviceConstructor.Invoke(new object[] { });
                        if (serviceClassObject != null) {
                            MethodInfo AddItem = serviceAssemblyType.GetMethod("UpdateServiceLastUpdatedDate");
                            if (AddItem != null) {
                                AddItem.Invoke(serviceClassObject, new object[] { fullNamespace });
                            }
                        }
                    }
                }
                catch { }
            }
        }


        #endregion


        #region Get Type Methods

        private static Type GetAssemblyType(string fullNamespace) {
            try {
                Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly asm in asms) {
                    Type tempType = asm.GetType(fullNamespace);
                    if (tempType != null) {
                        return tempType;
                    }
                }
            }
            catch { }

            string binFolder = AttemptGetBinFolderLocation(fullNamespace);
            if (!string.IsNullOrEmpty(binFolder)) {
                string[] fileList = Directory.GetFiles(binFolder);
                foreach (string file in fileList) {
                    if (new FileInfo(file).Extension.ToLower() == ".dll") {
                        Assembly tempAssembly = Assembly.LoadFile(file);
                        if (tempAssembly != null) {
                            IEnumerable<Type> exportedTypes = tempAssembly.ExportedTypes;
                            foreach (Type serviceAssemblyType in exportedTypes) {
                                if (serviceAssemblyType.FullName == fullNamespace) {
                                    return serviceAssemblyType;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
        private static string AttemptGetBinFolderLocation(string fullNamespace) {
            string bingFolderStr = string.Empty;
            try {
                Type serviceAssemblyType = GetAssemblyType(BackgroundServices_NAMESPACE);
                if (serviceAssemblyType != null) {
                    ConstructorInfo serviceConstructor = serviceAssemblyType.GetConstructor(Type.EmptyTypes);
                    if (serviceConstructor != null) {
                        object serviceClassObject = serviceConstructor.Invoke(new object[] { });
                        if (serviceClassObject != null) {
                            MethodInfo AddItem = serviceAssemblyType.GetMethod("GetBinFolder");
                            if (AddItem != null) {
                                object binFolder = AddItem.Invoke(serviceClassObject, new object[] { fullNamespace });
                                if (binFolder != null) {
                                    bingFolderStr = binFolder.ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return bingFolderStr;
        }
        private static string GetTypeName(string fullNamespace) {
            int nsEndIndex = fullNamespace.LastIndexOf('.');
            if (nsEndIndex > 0) {
                try {
                    return fullNamespace.Substring(nsEndIndex + 1);
                }
                catch {
                    // Do Nothing
                }
            }

            return fullNamespace;
        }

        #endregion

    }

}
