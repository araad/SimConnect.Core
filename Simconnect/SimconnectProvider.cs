using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using Microsoft.FlightSimulator.SimConnect;
using SIM.Connect.Common;
using System.Collections;

namespace SIM.Connect.Simconnect
{
    public sealed class SimconnectProvider : INotifyPropertyChanged
    {
        #region Events
        private event PropertyValueChangedEventHandler mSimPropertyChanged;
        public event PropertyValueChangedEventHandler SimPropertyChanged
        {
            add
            {
                lock (sEventLocker)
                {
                    mSimPropertyChanged += value;
                    mSubscribersCount++;
                    OnPropertyChanged("Subscribers");
                }
            }
            remove
            {
                lock (sEventLocker)
                {
                    mSimPropertyChanged -= value;
                    mSubscribersCount--;
                    OnPropertyChanged("Subscribers");
                }
            }
        }
        private void OnSimPropertyChanged(string propertyName, object newValue)
        {
            if (mSimPropertyChanged != null)
            {
                mSimPropertyChanged(this, new PropertyValueChangedEventArgs(propertyName, newValue));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                if (UiHandle.InvokeRequired)
                {
                    UiHandle.Invoke(new Action(delegate()
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                        }), null);
                }
                else
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        public event EventHandler Open;
        private void OnOpen()
        {
            if (Open != null)
            {
                Open(this, EventArgs.Empty);
            }
        }

        public event EventHandler Close;
        private void OnClose()
        {
            if (Close != null)
            {
                Close(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when a client joins Simconnect
        /// </summary>
        public event EventHandler Join;
        private void OnJoin()
        {
            if (Join != null)
                Join(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when a client leaves Simconnect
        /// </summary>
        public event EventHandler Leave;
        private void OnLeave()
        {
            if (Leave != null)
                Leave(this, EventArgs.Empty);
        }
        #endregion

        #region Constants
        // User-defined win32 event
        const int WM_USER_SIMCONNECT = 0x0402;

        const string FSX_PROCESS_NAME = "fsx";
        #endregion

        #region Private Members
        private static volatile SimconnectProvider sInstance;
        private static object sLocker = new object();
        private static object sEventLocker = new object();
        private static object sSimPropertyLocker = new object();

        // SimConnect object
        SimConnect simconnect = null;

        string mAppName;
        IntPtr mObjectHandle;

        ConenctionState mSimconnectState;
        private string mSimName;
        private bool mIsStartAvailable;
        private bool mIsStopAvailable;

        System.Timers.Timer mTimer;

        private Dictionary<string, IDataProvider> mSimObjectSubscribers;
        private Dictionary<uint, IDataProvider> mSimEventSubscribers;
        private List<ISimProperty> mSimPropertySubscribers;

        int mSubscribersCount;
        #endregion

        #region Public Members
        /// <summary>
        /// Gets the SimconnectProvider instance
        /// </summary>
        static public SimconnectProvider Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (sLocker)
                    {
                        if (sInstance == null)
                        {
                            sInstance = new SimconnectProvider();
                        }
                    }
                }

                return sInstance;
            }
        }
        /// <summary>
        /// Gets the state of the Simconnect client
        /// </summary>
        public ConenctionState SimconnectState
        {
            get { return this.mSimconnectState; }
        }

        /// <summary>
        /// Gets the value indicating if the join operation is available
        /// </summary>
        public bool IsStartAvailable
        {
            get
            {
                return this.mIsStartAvailable;
            }
        }

        public const string IsStartAvailableKey = "IsStartAvailable";

        /// <summary>
        /// Gets the value indicating if the leave operation is available
        /// </summary>
        public bool IsStopAvailable
        {
            get
            {
                return this.mIsStopAvailable;
            }
        }

        public const string IsStopAvailableKey = "IsStopAvailable";

        /// <summary>
        /// Gets the name of the simulation
        /// </summary>
        public string SimName
        {
            get
            {
                return this.mSimName;
            }
        }

        public const string SimNameKey = "SimName";

        /// <summary>
        /// Gets the value of Simconnect client's win32 message
        /// </summary>
        public int WM_SIMCONNECT
        {
            get
            {
                return WM_USER_SIMCONNECT;
            }
        }

        public int Subscribers
        {
            get { return this.mSubscribersCount; }
        }

        public ISynchronizeInvoke UiHandle { get; set; }
        #endregion

        #region Constructor
        private SimconnectProvider()
        {
            SimLogger.Log(LogMode.Info, "SimconnectProvider", "constructor");

            this.simconnect = null;
            this.mSimconnectState = ConenctionState.DISCONNECTED;
            this.mAppName = string.Empty;
            this.mObjectHandle = new IntPtr();
            this.mIsStartAvailable = false;
            this.mIsStopAvailable = false;
            this.mSimName = string.Empty;

            this.mSimObjectSubscribers = new Dictionary<string, IDataProvider>();
            this.mSimEventSubscribers = new Dictionary<uint, IDataProvider>();
            this.mSimPropertySubscribers = new List<ISimProperty>();

            this.mTimer = new System.Timers.Timer(250);
            this.mTimer.Elapsed += new ElapsedEventHandler(mTimer_Elapsed);
            this.mTimer.Enabled = true;
        }
        #endregion

        #region Event Handlers
        void simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            SimLogger.Log(LogMode.Info, "SimconnectProvider", "Connected to FSX");
            this.mSimconnectState = ConenctionState.CONNECTED;
            this.mSimName = data.szApplicationName;

            this.OnOpen();
        }

        // The case where FXS is shut down
        void simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            SimLogger.Log(LogMode.Info, "SimconnectProvider", "FSX has exited");
            this.closeConnection();
        }

        void simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SimLogger.Log(LogMode.Error, "SimconnectProvider", "Exception received", data.dwException.ToString());
        }

        void simconnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            string objectID = data.dwData[0].GetType().Name;
            IDataProvider cbProvider = this.mSimObjectSubscribers[objectID];
            cbProvider.Simconnect_ReceiveSimObject(objectID, data.dwData[0]);
        }

        void simconnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT recEvent)
        {
            this.mSimEventSubscribers[recEvent.uGroupID].Simconnect_ReceiveSimEvent(recEvent.uEventID, recEvent.dwData);
        }

        void mTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.isStartAvailable_Update();
            this.isStopAvailable_Update();
            this.simProperty_Update();
        }
        #endregion

        #region Private Methods
        void attachSimconnectEventHandlers()
        {
            try
            {
                // listen to connect and quit msgs
                simconnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(simconnect_OnRecvOpen);
                simconnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(simconnect_OnRecvQuit);

                // listen to exceptions
                simconnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(simconnect_OnRecvException);

                simconnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(simconnect_OnRecvSimobjectDataBytype);

                simconnect.OnRecvEvent += new SimConnect.RecvEventEventHandler(simconnect_OnRecvEvent);

                SimLogger.Log(LogMode.Info, "SimconnectProvider", "All Simconnect event handlers are attached");
            }
            catch (COMException ex)
            {
                SimLogger.Log(LogMode.Error, "SimconnectProvider", "Error while attaching event handlers", ex.Message, ex.StackTrace);
            }
        }

        void detachSimconnectEventHandlers()
        {
            try
            {
                // listen to connect and quit msgs
                simconnect.OnRecvOpen -= new SimConnect.RecvOpenEventHandler(simconnect_OnRecvOpen);
                simconnect.OnRecvQuit -= new SimConnect.RecvQuitEventHandler(simconnect_OnRecvQuit);

                // listen to exceptions
                simconnect.OnRecvException -= new SimConnect.RecvExceptionEventHandler(simconnect_OnRecvException);

                SimLogger.Log(LogMode.Info, "SimconnectProvider", "All Simconnect event handlers are detached");
            }
            catch (COMException ex)
            {
                SimLogger.Log(LogMode.Error, "SimconnectProvider", "Error while detaching event handlers", ex.Message, ex.StackTrace);
            }
        }

        void openConnection()
        {
            if (simconnect == null)
            {
                try
                {
                    this.simconnect = new SimConnect(mAppName, mObjectHandle, WM_USER_SIMCONNECT, null, 0);
                    SimLogger.Log(LogMode.Info, "SimconnectProvider", "Connection with the Simconnect client has been established");
                    this.attachSimconnectEventHandlers();
                }
                catch (Exception ex)
                {
                    SimLogger.Log(LogMode.Error, "SimconnectProvider", "Unable to connect to FSX ", ex.Message, ex.StackTrace);
                }
            }
            else
            {
                SimLogger.Log(LogMode.Error, "SimconnectProvider", "Simulation exercise not available - try again in a few moments");
                closeConnection();
            }
        }

        void closeConnection()
        {
            if (simconnect != null)
            {
                // Dispose serves the same purpose as SimConnect_Close()
                this.detachSimconnectEventHandlers();
                simconnect.Dispose();
                simconnect = null;
                this.mSimconnectState = ConenctionState.DISCONNECTED;
                this.mSimObjectSubscribers.Clear();
                this.mSimEventSubscribers.Clear();
                this.mSimPropertySubscribers.Clear();
                SimLogger.Log(LogMode.Info, "SimconnectProvider", "Connection with the Simconnect client has been closed");

                this.OnClose();

                GC.Collect();
            }
        }

        void isStartAvailable_Update()
        {
            bool wIsStartAvailable = false;

            Process[] wProcess = Process.GetProcessesByName(FSX_PROCESS_NAME);
            if (wProcess.Length != 0)
            {
                if (this.mSimconnectState == ConenctionState.DISCONNECTED)
                {
                    wIsStartAvailable = true;
                }
            }

            if (wIsStartAvailable != mIsStartAvailable)
            {
                mIsStartAvailable = wIsStartAvailable;
                SimLogger.Log(LogMode.Debug, "SimconnectProvider", string.Format("IsStartAvailable is set to {0}", mIsStartAvailable));
                OnPropertyChanged(IsStartAvailableKey);
            }
        }

        void isStopAvailable_Update()
        {
            bool wIsStopAvailable = false;

            if (this.mSimconnectState == ConenctionState.CONNECTED)
            {
                wIsStopAvailable = true;
            }

            if (wIsStopAvailable != mIsStopAvailable)
            {
                mIsStopAvailable = wIsStopAvailable;
                SimLogger.Log(LogMode.Debug, "SimconnectProvider", string.Format("IsStopAvailable is set to {0}", mIsStopAvailable));
                OnPropertyChanged(IsStopAvailableKey);
            }
        }

        void simProperty_Update()
        {
            lock (sSimPropertyLocker)
            {
                foreach (ISimProperty simProp in this.mSimPropertySubscribers)
                {
                    RequestDataOnSimObjectType(simProp.Provider, simProp.RequestID, simProp.SimObjectID);
                }
            }
        }

        void subscribe_SimEvent(IDataProvider cbProvider, Enum groupID, Enum eventID, string eventName)
        {
            if (!this.mSimEventSubscribers.Keys.Contains((uint)(SimNotificationGroup)groupID))
            {
                this.mSimEventSubscribers.Add((uint)(SimNotificationGroup)groupID, cbProvider);
            }
            this.simconnect.AddClientEventToNotificationGroup(groupID, eventID, false);
        }

        void unsubscribe_Group(Enum groupID)
        {
            
        }
        #endregion

        #region Internal Methods
        internal void Register_SimObject<DataStruct, PropertyType>(SimProperty<PropertyType> iSimProperty)
        {
            // register data structure definition for this SimObject
            this.simconnect.AddToDataDefinition(iSimProperty.SimObjectID, iSimProperty.SimObjectName, iSimProperty.Units,
                (SIMCONNECT_DATATYPE)iSimProperty.SimObjectType, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            this.simconnect.RegisterDataDefineStruct<DataStruct>(iSimProperty.SimObjectID);

            if(!string.IsNullOrEmpty(iSimProperty.SimEventName))
            {
                this.simconnect.MapClientEventToSimEvent(iSimProperty.SimEventID, iSimProperty.SimEventName);
            }
        }

        internal void Subscribe_SimObject<PropertyType>(Enum iGroupID, SimProperty<PropertyType> iSimProperty)
        {
            // Request the current value for this SimObject (first-pass)
            RequestDataOnSimObjectType(iSimProperty.Provider, iSimProperty.RequestID, iSimProperty.SimObjectID);

            // Subscribe to SimEvents
            if (!string.IsNullOrEmpty(iSimProperty.SimEventName))
            {
                // case where we subscribe using the SimEventname
                subscribe_SimEvent(iSimProperty.Provider, iGroupID, iSimProperty.SimEventID, iSimProperty.SimEventName);
            }
            else
            {
                // case where we add SimObject to SimconnectProvider's update scheduler
                this.mSimPropertySubscribers.Add(iSimProperty);
            }

        }

        internal void Unsubscribe_SimObject<PropertyType>(SimProperty<PropertyType> iSimProperty)
        {
            // Unsubscribe from SimEvents
            if (string.IsNullOrEmpty(iSimProperty.SimEventName))
            {
                // case where we remove SimObject from SimconenctProvider's update scheduler
                this.mSimPropertySubscribers.Remove(iSimProperty);
            }
        }

        internal void Unsubscribe_GroupNotification(Enum iGroupID)
        {
            if (this.mSimEventSubscribers.Keys.Contains((uint)(SimNotificationGroup)iGroupID))
            {
                this.mSimEventSubscribers.Remove((uint)(SimNotificationGroup)iGroupID);
                this.simconnect.ClearNotificationGroup(iGroupID);
            }
        }

        internal void RequestDataOnSimObjectType(IDataProvider cbProvider, Enum iRequestID, Enum iDefineID)
        {
            if (!this.mSimObjectSubscribers.Keys.Contains(iDefineID.ToString()))
            {
                this.mSimObjectSubscribers.Add(iDefineID.ToString(), cbProvider);
            }
            this.simconnect.RequestDataOnSimObjectType(iRequestID, iDefineID, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }

        internal void SetDataOnSimObject(IDataProvider cbProvider, Enum iRequestID, Enum iDefineID, object iSimObject)
        {
            this.simconnect.SetDataOnSimObject(iDefineID, (uint)SIMCONNECT_SIMOBJECT_TYPE.USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, iSimObject);
            RequestDataOnSimObjectType(cbProvider, iRequestID, iDefineID);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the Simconnect provider
        /// </summary>
        /// <param name="iAppName">The name of the requesting application</param>
        /// <param name="iObjectHandle">The object handle of the requesting application</param>
        public void InitializeSimconnect(string iAppName, IntPtr iObjectHandle)
        {
            SimLogger.Log(LogMode.Info, "SimconnectProvider", "Initializing");

            this.mAppName = iAppName;
            this.mObjectHandle = iObjectHandle;

            this.openConnection();
        }

        public void ResetSimconnect()
        {
            SimLogger.Log(LogMode.Info, "SimconnectProvider", "Resetting");

            this.closeConnection();

            this.mAppName = string.Empty;
            this.mObjectHandle = IntPtr.Zero;
        }

        /// <summary>
        /// Request a join to the FSX simulation
        /// </summary>
        public void JoinSimulation()
        {
            SimLogger.Log(LogMode.Debug, "SimconnectProvider", "Joining the simulation");
            this.OnJoin();
        }

        // The case where IOS user leaves simulation
        /// <summary>
        /// Reuest a leave from the FSX simulation
        /// </summary>
        public void LeaveSimulation()
        {
            SimLogger.Log(LogMode.Debug, "SimconnectProvider", "Leaving the simulation");
            this.OnLeave();
        }

        /// <summary>
        /// Notify Simconnect of a waiting win32 message
        /// </summary>
        public void ReceiveMessage()
        {
            if (simconnect != null)
            {
                simconnect.ReceiveMessage();
            }
        }
        #endregion
    }
}
