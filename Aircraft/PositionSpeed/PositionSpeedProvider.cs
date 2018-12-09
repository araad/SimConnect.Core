using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SIM.Connect.Common;
using SIM.Connect.Simconnect;

namespace SIM.Connect.Aircraft.PositionSpeed
{
	public class PositionSpeedProvider : DataProvider
	{
		#region Singleton Attributes
		private static volatile PositionSpeedProvider sInstance;
		private static object sLocker = new object();

		static public PositionSpeedProvider Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (sLocker)
                    {
                        if (sInstance == null)
                        {
                            sInstance = new PositionSpeedProvider();
                        }
                    }
                }

                return sInstance;
            }
        }
		#endregion

		#region Constructor
		private PositionSpeedProvider()
		{
			SimLogger.Log(LogMode.Info, "PositionSpeedProvider", "constructor");

			this.mSimProperties = new List<ISimProperty>();
            this.resetProperties();
            this.subscribe_SimconnectProvider();
		}
		#endregion

		#region Latitude SimProperty
		private SimProperty<double> mLatitude;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct Latitude
		{
			public double Value;
		}

		public const string LatitudeKey = "Latitude";

		[Description("Current latitude in radians")]
		public double LatitudeProp
		{
			get { return this.mLatitude.Value; }
		}

		[Description("Sets the current latitude in radians")]
		public void SetLatitude(double value)
		{
			if (this.mLatitude.Value != value)
			{
				Latitude obj = new Latitude();
				obj.Value = value;

				SimconnectProvider.Instance.SetDataOnSimObject(this, SimDataRequest.PositionSpeed, SimObjectID.Latitude, obj);
			}
		}
		#endregion

		#region Longitude SimProperty
		private SimProperty<double> mLongitude;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct Longitude
		{
			public double Value;
		}

		public const string LongitudeKey = "Longitude";

		[Description("Current longitude in radians")]
		public double LongitudeProp
		{
			get { return this.mLongitude.Value; }
		}

		[Description("Sets the current longitude in radians")]
		public void SetLongitude(double value)
		{
			if (this.mLongitude.Value != value)
			{
				Longitude obj = new Longitude();
				obj.Value = value;

				SimconnectProvider.Instance.SetDataOnSimObject(this, SimDataRequest.PositionSpeed, SimObjectID.Longitude, obj);
			}
		}
		#endregion

		#region MSLAltitude SimProperty
		private SimProperty<double> mMSLAltitude;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct MSLAltitude
		{
			public double Value;
		}

		public const string MSLAltitudeKey = "MSLAltitude";

		[Description("Current altitude from mean sea level in feet")]
		public double MSLAltitudeProp
		{
			get { return this.mMSLAltitude.Value; }
		}

		[Description("Sets the current altitude from mean sea level in feet")]
		public void SetMSLAltitude(double value)
		{
			if (this.mMSLAltitude.Value != value)
			{
				MSLAltitude obj = new MSLAltitude();
				obj.Value = value;

				SimconnectProvider.Instance.SetDataOnSimObject(this, SimDataRequest.PositionSpeed, SimObjectID.MSLAltitude, obj);
			}
		}
		#endregion

		#region AGLAltitude SimProperty
		private SimProperty<double> mAGLAltitude;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct AGLAltitude
		{
			public double Value;
		}

		public const string AGLAltitudeKey = "AGLAltitude";

		[Description("Current altitude above ground level in feet")]
		public double AGLAltitudeProp
		{
			get { return this.mAGLAltitude.Value; }
		}

		[Description("Sets the current altitude above ground level in feet")]
		public void SetAGLAltitude(double value)
		{
			if (this.mAGLAltitude.Value != value)
			{
				AGLAltitude obj = new AGLAltitude();
				obj.Value = value;

				SimconnectProvider.Instance.SetDataOnSimObject(this, SimDataRequest.PositionSpeed, SimObjectID.AGLAltitude, obj);
			}
		}
		#endregion

		#region MagneticHeading SimProperty
		private SimProperty<double> mMagneticHeading;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct MagneticHeading
		{
			public double Value;
		}

		public const string MagneticHeadingKey = "MagneticHeading";

		[Description("Current magnetic heading in radians")]
		public double MagneticHeadingProp
		{
			get { return this.mMagneticHeading.Value; }
		}

		[Description("Sets the current magnetic heading in radians")]
		public void SetMagneticHeading(double value)
		{
			if (this.mMagneticHeading.Value != value)
			{
				MagneticHeading obj = new MagneticHeading();
				obj.Value = value;

				SimconnectProvider.Instance.SetDataOnSimObject(this, SimDataRequest.PositionSpeed, SimObjectID.MagneticHeading, obj);
			}
		}
		#endregion

		#region TrueHeading SimProperty
		private SimProperty<double> mTrueHeading;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct TrueHeading
		{
			public double Value;
		}

		public const string TrueHeadingKey = "TrueHeading";

		[Description("Current true heading in radians")]
		public double TrueHeadingProp
		{
			get { return this.mTrueHeading.Value; }
		}

		[Description("Sets the current true heading in radians")]
		public void SetTrueHeading(double value)
		{
			if (this.mTrueHeading.Value != value)
			{
				TrueHeading obj = new TrueHeading();
				obj.Value = value;

				SimconnectProvider.Instance.SetDataOnSimObject(this, SimDataRequest.PositionSpeed, SimObjectID.TrueHeading, obj);
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
						case LatitudeKey:
							var wLatitude = simProp as SimProperty<double>;
							wLatitude.Value = Math.Round(((Latitude)simObject).Value, 10);
							break;
						case LongitudeKey:
							var wLongitude = simProp as SimProperty<double>;
							wLongitude.Value = Math.Round(((Longitude)simObject).Value, 10);
							break;
						case MSLAltitudeKey:
							var wMSLAltitude = simProp as SimProperty<double>;
							wMSLAltitude.Value = Math.Round(((MSLAltitude)simObject).Value, 0);
							break;
						case AGLAltitudeKey:
							var wAGLAltitude = simProp as SimProperty<double>;
							wAGLAltitude.Value = Math.Round(((AGLAltitude)simObject).Value, 0);
							break;
						case MagneticHeadingKey:
							var wMagneticHeading = simProp as SimProperty<double>;
							wMagneticHeading.Value = Math.Round(((MagneticHeading)simObject).Value, 4);
							break;
						case TrueHeadingKey:
							var wTrueHeading = simProp as SimProperty<double>;
							wTrueHeading.Value = Math.Round(((TrueHeading)simObject).Value, 4);
							break;
						default:
							SimLogger.Log(LogMode.Warn, "PositionSpeedProvider", "Receiving SimObject that is not registered");
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
					SimLogger.Log(LogMode.Warn, "PositionSpeedProvider", "Receiving SimEvent that is not registered");
					break;
			}
		}

		protected override void registerProperties()
		{
			try
			{
				SimconnectProvider.Instance.Register_SimObject<Latitude, double>(this.mLatitude);
				SimconnectProvider.Instance.Register_SimObject<Longitude, double>(this.mLongitude);
				SimconnectProvider.Instance.Register_SimObject<MSLAltitude, double>(this.mMSLAltitude);
				SimconnectProvider.Instance.Register_SimObject<AGLAltitude, double>(this.mAGLAltitude);
				SimconnectProvider.Instance.Register_SimObject<MagneticHeading, double>(this.mMagneticHeading);
				SimconnectProvider.Instance.Register_SimObject<TrueHeading, double>(this.mTrueHeading);

				SimLogger.Log(LogMode.Info, "PositionSpeedProvider", "All properties have been registered");
			}
			catch (Exception ex)
			{
				SimLogger.Log(LogMode.Error, "PositionSpeedProvider", "Error while registering properties", ex.Message, ex.StackTrace);
			}
		}

		protected override void startSubscriptions()
        {
            try
            {
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.PositionSpeed, this.mLatitude);
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.PositionSpeed, this.mLongitude);
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.PositionSpeed, this.mMSLAltitude);
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.PositionSpeed, this.mAGLAltitude);
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.PositionSpeed, this.mMagneticHeading);
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.PositionSpeed, this.mTrueHeading);

                SimLogger.Log(LogMode.Info, "PositionSpeedProvider", "All subscriptions have been started");
            }
            catch (Exception ex)
            {
                SimLogger.Log(LogMode.Error, "PositionSpeedProvider", "Error while starting subscriptions", ex.Message, ex.StackTrace);
            }
        }

		protected override void resetProperties()
        {
            this.mSimProperties.Clear();

            this.mLatitude = new SimProperty<double>
                (this, SimDataRequest.PositionSpeed, SimObjectID.Latitude, 0.0, "radians", "PLANE LATITUDE", SimObjectDataType.Double);
            this.mLatitude.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mLatitude);

            this.mLongitude = new SimProperty<double>
                (this, SimDataRequest.PositionSpeed, SimObjectID.Longitude, 0.0, "radians", "PLANE LONGITUDE", SimObjectDataType.Double);
            this.mLongitude.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mLongitude);

            this.mMSLAltitude = new SimProperty<double>
                (this, SimDataRequest.PositionSpeed, SimObjectID.MSLAltitude, 0.0, "feet", "PLANE ALTITUDE", SimObjectDataType.Double);
            this.mMSLAltitude.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mMSLAltitude);

            this.mAGLAltitude = new SimProperty<double>
                (this, SimDataRequest.PositionSpeed, SimObjectID.AGLAltitude, 0.0, "feet", "PLANE ALT ABOVE GROUND", SimObjectDataType.Double);
            this.mAGLAltitude.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mAGLAltitude);

            this.mMagneticHeading = new SimProperty<double>
                (this, SimDataRequest.PositionSpeed, SimObjectID.MagneticHeading, 0.0, "radians", "PLANE HEADING DEGREES MAGNETIC", SimObjectDataType.Double);
            this.mMagneticHeading.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mMagneticHeading);

            this.mTrueHeading = new SimProperty<double>
                (this, SimDataRequest.PositionSpeed, SimObjectID.TrueHeading, 0.0, "radians", "PLANE HEADING DEGREES TRUE", SimObjectDataType.Double);
            this.mTrueHeading.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mTrueHeading);

            SimLogger.Log(LogMode.Info, "PositionSpeedProvider", "All Sim properties have been reset");
        }

		protected override void stopSubscriptions()
        {
            try
            {
                // Unsubscribe all simobjects
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mLatitude);
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mLongitude);
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mMSLAltitude);
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mAGLAltitude);
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mMagneticHeading);
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mTrueHeading);

                // Unsubscribe group
                SimconnectProvider.Instance.Unsubscribe_GroupNotification(SimNotificationGroup.PositionSpeed);

                SimLogger.Log(LogMode.Info, "PositionSpeedProvider", "All subscriptions have been stopped");
            }
            catch (Exception ex)
            {
                SimLogger.Log(LogMode.Error, "PositionSpeedProvider", "Error while stopping subscriptions", ex.Message, ex.StackTrace);
            }
        }
		#endregion
	}
}
