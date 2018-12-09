using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SIM.Connect.Common;
using SIM.Connect.Simconnect;

namespace SIM.Connect.Aircraft
{
	public class AircraftProvider : DataProvider
	{
		#region Singleton Attributes
		private static volatile AircraftProvider sInstance;
		private static object sLocker = new object();

		static public AircraftProvider Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (sLocker)
                    {
                        if (sInstance == null)
                        {
                            sInstance = new AircraftProvider();
                        }
                    }
                }

                return sInstance;
            }
        }
		#endregion

		#region Constructor
		private AircraftProvider()
		{
			SimLogger.Log(LogMode.Info, "AircraftProvider", "constructor");

			this.mSimProperties = new List<ISimProperty>();
            this.resetProperties();
            this.subscribe_SimconnectProvider();
		}
		#endregion

		#region AircraftTitle SimProperty
		private SimProperty<string> mAircraftTitle;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct AircraftTitle
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string Value;
		}

		public const string AircraftTitleKey = "AircraftTitle";

		[Description("Aircraft's title")]
		public string AircraftTitleProp
		{
			get { return this.mAircraftTitle.Value; }
		}
		#endregion

		#region AircraftTotalWeight SimProperty
		private SimProperty<double> mAircraftTotalWeight;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct AircraftTotalWeight
		{
			public double Value;
		}

		public const string AircraftTotalWeightKey = "AircraftTotalWeight";

		[Description("Aircraft's total weight")]
		public double AircraftTotalWeightProp
		{
			get { return this.mAircraftTotalWeight.Value; }
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
						case AircraftTitleKey:
							var wAircraftTitle = simProp as SimProperty<string>;
							wAircraftTitle.Value = ((AircraftTitle)simObject).Value;
							break;
						case AircraftTotalWeightKey:
							var wAircraftTotalWeight = simProp as SimProperty<double>;
							wAircraftTotalWeight.Value = Math.Round(((AircraftTotalWeight)simObject).Value, 1);
							break;
						default:
							SimLogger.Log(LogMode.Warn, "AircraftProvider", "Receiving SimObject that is not registered");
							break;
					}
				}
			}
		}

		public override void Simconnect_ReceiveSimEvent(uint eventID, object simObject)
		{
			switch ((SimEventID)eventID)
			{
				default:
					SimLogger.Log(LogMode.Warn, "AircraftProvider", "Receiving SimEvent that is not registered");
					break;
			}
		}

		protected override void registerProperties()
		{
			try
			{
				SimconnectProvider.Instance.Register_SimObject<AircraftTitle, string>(this.mAircraftTitle);
				SimconnectProvider.Instance.Register_SimObject<AircraftTotalWeight, double>(this.mAircraftTotalWeight);

				SimLogger.Log(LogMode.Info, "AircraftProvider", "All properties have been registered");
			}
			catch (Exception ex)
			{
				SimLogger.Log(LogMode.Error, "AircraftProvider", "Error while registering properties", ex.Message, ex.StackTrace);
			}
		}

		protected override void startSubscriptions()
        {
            try
            {
                SimconnectProvider.Instance.Subscribe_SimObject<string>(SimNotificationGroup.Aircraft, this.mAircraftTitle);
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.Aircraft, this.mAircraftTotalWeight);

                SimLogger.Log(LogMode.Info, "AircraftProvider", "All subscriptions have been started");
            }
            catch (Exception ex)
            {
                SimLogger.Log(LogMode.Error, "AircraftProvider", "Error while starting subscriptions", ex.Message, ex.StackTrace);
            }
        }

		protected override void resetProperties()
        {
            this.mSimProperties.Clear();

            this.mAircraftTitle = new SimProperty<string>
                (this, SimDataRequest.Aircraft, SimObjectID.AircraftTitle, string.Empty, null, "TITLE", SimObjectDataType.String);
            this.mAircraftTitle.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mAircraftTitle);

            this.mAircraftTotalWeight = new SimProperty<double>
                (this, SimDataRequest.Aircraft, SimObjectID.AircraftTotalWeight, 0.0, "pounds", "TOTAL WEIGHT", SimObjectDataType.Double);
            this.mAircraftTotalWeight.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mAircraftTotalWeight);

            SimLogger.Log(LogMode.Info, "AircraftProvider", "All Sim properties have been reset");
        }

		protected override void stopSubscriptions()
        {
            try
            {
                // Unsubscribe all simobjects
                SimconnectProvider.Instance.Unsubscribe_SimObject<string>(this.mAircraftTitle);
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mAircraftTotalWeight);

                // Unsubscribe group
                SimconnectProvider.Instance.Unsubscribe_GroupNotification(SimNotificationGroup.Aircraft);

                SimLogger.Log(LogMode.Info, "AircraftProvider", "All subscriptions have been stopped");
            }
            catch (Exception ex)
            {
                SimLogger.Log(LogMode.Error, "AircraftProvider", "Error while stopping subscriptions", ex.Message, ex.StackTrace);
            }
        }
		#endregion
	}
}
