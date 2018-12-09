using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SIM.Connect.Common;
using SIM.Connect.Simconnect;

namespace SIM.Connect.Aircraft.FlightInstrumentation
{
	public class FlightInstrumentationProvider : DataProvider
	{
		#region Singleton Attributes
		private static volatile FlightInstrumentationProvider sInstance;
		private static object sLocker = new object();

		static public FlightInstrumentationProvider Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (sLocker)
                    {
                        if (sInstance == null)
                        {
                            sInstance = new FlightInstrumentationProvider();
                        }
                    }
                }

                return sInstance;
            }
        }
		#endregion

		#region Constructor
		private FlightInstrumentationProvider()
		{
			SimLogger.Log(LogMode.Info, "FlightInstrumentationProvider", "constructor");

			this.mSimProperties = new List<ISimProperty>();
            this.resetProperties();
            this.subscribe_SimconnectProvider();
		}
		#endregion

		#region IndicatedAirspeed SimProperty
		private SimProperty<double> mIndicatedAirspeed;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct IndicatedAirspeed
		{
			public double Value;
		}

		public const string IndicatedAirspeedKey = "IndicatedAirspeed";

		[Description("Current indicated airspeed of the aircraft")]
		public double IndicatedAirspeedProp
		{
			get { return this.mIndicatedAirspeed.Value; }
		}

		[Description("Sets the current indicated airspeed of the aircraft")]
		public void SetIndicatedAirspeed(double value)
		{
			if (this.mIndicatedAirspeed.Value != value)
			{
				IndicatedAirspeed obj = new IndicatedAirspeed();
				obj.Value = value;

				SimconnectProvider.Instance.SetDataOnSimObject(this, SimDataRequest.FlightInstrumentation, SimObjectID.IndicatedAirspeed, obj);
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
						case IndicatedAirspeedKey:
							var wIndicatedAirspeed = simProp as SimProperty<double>;
							wIndicatedAirspeed.Value = Math.Round(((IndicatedAirspeed)simObject).Value, 1);
							break;
						default:
							SimLogger.Log(LogMode.Warn, "FlightInstrumentationProvider", "Receiving SimObject that is not registered");
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
					SimLogger.Log(LogMode.Warn, "FlightInstrumentationProvider", "Receiving SimEvent that is not registered");
					break;
			}
		}

		protected override void registerProperties()
		{
			try
			{
				SimconnectProvider.Instance.Register_SimObject<IndicatedAirspeed, double>(this.mIndicatedAirspeed);

				SimLogger.Log(LogMode.Info, "FlightInstrumentationProvider", "All properties have been registered");
			}
			catch (Exception ex)
			{
				SimLogger.Log(LogMode.Error, "FlightInstrumentationProvider", "Error while registering properties", ex.Message, ex.StackTrace);
			}
		}

		protected override void startSubscriptions()
        {
            try
            {
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.FlightInstrumentation, this.mIndicatedAirspeed);

                SimLogger.Log(LogMode.Info, "FlightInstrumentationProvider", "All subscriptions have been started");
            }
            catch (Exception ex)
            {
                SimLogger.Log(LogMode.Error, "FlightInstrumentationProvider", "Error while starting subscriptions", ex.Message, ex.StackTrace);
            }
        }

		protected override void resetProperties()
        {
            this.mSimProperties.Clear();

            this.mIndicatedAirspeed = new SimProperty<double>
                (this, SimDataRequest.FlightInstrumentation, SimObjectID.IndicatedAirspeed, 0.0, "knots", "AIRSPEED INDICATED", SimObjectDataType.Double);
            this.mIndicatedAirspeed.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mIndicatedAirspeed);

            SimLogger.Log(LogMode.Info, "FlightInstrumentationProvider", "All Sim properties have been reset");
        }

		protected override void stopSubscriptions()
        {
            try
            {
                // Unsubscribe all simobjects
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mIndicatedAirspeed);

                // Unsubscribe group
                SimconnectProvider.Instance.Unsubscribe_GroupNotification(SimNotificationGroup.FlightInstrumentation);

                SimLogger.Log(LogMode.Info, "FlightInstrumentationProvider", "All subscriptions have been stopped");
            }
            catch (Exception ex)
            {
                SimLogger.Log(LogMode.Error, "FlightInstrumentationProvider", "Error while stopping subscriptions", ex.Message, ex.StackTrace);
            }
        }
		#endregion
	}
}
