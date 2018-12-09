using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SIM.Connect.Common;
using SIM.Connect.Simconnect;

namespace SIM.Connect.Aircraft.ElectricalSystems
{
	public class ElectricalSystemsProvider : DataProvider
	{
		#region Singleton Attributes
		private static volatile ElectricalSystemsProvider sInstance;
		private static object sLocker = new object();

		static public ElectricalSystemsProvider Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (sLocker)
                    {
                        if (sInstance == null)
                        {
                            sInstance = new ElectricalSystemsProvider();
                        }
                    }
                }

                return sInstance;
            }
        }
		#endregion

		#region Constructor
		private ElectricalSystemsProvider()
		{
			SimLogger.Log(LogMode.Info, "ElectricalSystemsProvider", "constructor");

			this.mSimProperties = new List<ISimProperty>();
            this.resetProperties();
            this.subscribe_SimconnectProvider();
		}
		#endregion

		#region ElectricalMasterBattery SimProperty
		private SimProperty<bool> mElectricalMasterBattery;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct ElectricalMasterBattery
		{
			public bool Value;
		}

		public const string ElectricalMasterBatteryKey = "ElectricalMasterBattery";

		[Description("State of the electrical master battery switch")]
		public bool ElectricalMasterBatteryProp
		{
			get { return this.mElectricalMasterBattery.Value; }
		}

		[Description("Sets the state of the electrical master battery switch")]
		public void SetElectricalMasterBattery(bool value)
		{
			if (this.mElectricalMasterBattery.Value != value)
			{
				ElectricalMasterBattery obj = new ElectricalMasterBattery();
				obj.Value = value;

				SimconnectProvider.Instance.SetDataOnSimObject(this, SimDataRequest.ElectricalSystems, SimObjectID.ElectricalMasterBattery, obj);
			}
		}
		#endregion

		#region DataProvider Members
		public override void Simconnect_ReceiveSimObject(string simObjectID, object simObject)
		{
			foreach(ISimProperty simProp in this.mSimProperties)
			{
				if (simProp.SimObjectID.ToString() == simObjectID)
				{
					switch(simObjectID)
					{
						case ElectricalMasterBatteryKey:
							var wElectricalMasterBattery = simProp as SimProperty<bool>;
							wElectricalMasterBattery.Value = ((ElectricalMasterBattery)simObject).Value;
							break;
						default:
							SimLogger.Log(LogMode.Warn, "ElectricalSystemsProvider", "Receiving SimObject that is not registered");
							break;
					}
				}
			}
		}

		public override void Simconnect_ReceiveSimEvent(uint eventID, object simObject)
		{
			switch ((SimEventID)eventID)
			{
				case SimEventID.TOGGLE_MASTER_BATTERY:
					SimconnectProvider.Instance.RequestDataOnSimObjectType(this,
						SimDataRequest.ElectricalSystems, SimObjectID.ElectricalMasterBattery);
					break;
				default:
					SimLogger.Log(LogMode.Warn, "ElectricalSystemsProvider", "Receiving SimEvent that is not registered");
					break;
			}
		}

		protected override void registerProperties()
		{
			try
			{
				SimconnectProvider.Instance.Register_SimObject<ElectricalMasterBattery, bool>(this.mElectricalMasterBattery);

				SimLogger.Log(LogMode.Info, "ElectricalSystemsProvider", "All properties have been registered");
			}
			catch (Exception ex)
			{
				SimLogger.Log(LogMode.Error, "ElectricalSystemsProvider", "Error while registering properties", ex.Message, ex.StackTrace);
			}
		}

		protected override void startSubscriptions()
        {
            try
            {
                SimconnectProvider.Instance.Subscribe_SimObject<bool>(SimNotificationGroup.ElectricalSystems, this.mElectricalMasterBattery);

                SimLogger.Log(LogMode.Info, "ElectricalSystemsProvider", "All subscriptions have been started");
            }
            catch (Exception ex)
            {
                SimLogger.Log(LogMode.Error, "ElectricalSystemsProvider", "Error while starting subscriptions", ex.Message, ex.StackTrace);
            }
        }

		protected override void resetProperties()
        {
            this.mSimProperties.Clear();

            this.mElectricalMasterBattery = new SimProperty<bool>
				(this, SimDataRequest.ElectricalSystems, SimObjectID.ElectricalMasterBattery, false, "bool", "ELECTRICAL MASTER BATTERY", SimObjectDataType.Bool, SimEventID.TOGGLE_MASTER_BATTERY, "TOGGLE_MASTER_BATTERY");
            this.mElectricalMasterBattery.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mElectricalMasterBattery);

            SimLogger.Log(LogMode.Info, "ElectricalSystemsProvider", "All Sim properties have been reset");
        }

		protected override void stopSubscriptions()
        {
            try
            {
                // Unsubscribe all simobjects
                SimconnectProvider.Instance.Unsubscribe_SimObject<bool>(this.mElectricalMasterBattery);

                // Unsubscribe group
                SimconnectProvider.Instance.Unsubscribe_GroupNotification(SimNotificationGroup.ElectricalSystems);

                SimLogger.Log(LogMode.Info, "ElectricalSystemsProvider", "All subscriptions have been stopped");
            }
            catch (Exception ex)
            {
                SimLogger.Log(LogMode.Error, "ElectricalSystemsProvider", "Error while stopping subscriptions", ex.Message, ex.StackTrace);
            }
        }
		#endregion
	}
}
