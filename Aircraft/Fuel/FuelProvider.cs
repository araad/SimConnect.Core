using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SIM.Connect.Common;
using SIM.Connect.Simconnect;

namespace SIM.Connect.Aircraft.Fuel
{
	public class FuelProvider : DataProvider
	{
		#region Singleton Attributes
		private static volatile FuelProvider sInstance;
		private static object sLocker = new object();

		static public FuelProvider Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (sLocker)
                    {
                        if (sInstance == null)
                        {
                            sInstance = new FuelProvider();
                        }
                    }
                }

                return sInstance;
            }
        }
		#endregion

		#region Constructor
		private FuelProvider()
		{
			SimLogger.Log(LogMode.Info, "FuelProvider", "constructor");

			this.mSimProperties = new List<ISimProperty>();
            this.resetProperties();
            this.subscribe_SimconnectProvider();
		}
		#endregion

		#region FuelTankCenterLevel SimProperty
		private SimProperty<double> mFuelTankCenterLevel;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct FuelTankCenterLevel
		{
			public double Value;
		}

		public const string FuelTankCenterLevelKey = "FuelTankCenterLevel";

		[Description("Center fuel tank level as a percentage")]
		public double FuelTankCenterLevelProp
		{
			get { return this.mFuelTankCenterLevel.Value; }
		}

		[Description("Sets the center fuel tank level as a percentage")]
		public void SetFuelTankCenterLevel(double value)
		{
			if (this.mFuelTankCenterLevel.Value != value)
			{
				FuelTankCenterLevel obj = new FuelTankCenterLevel();
				obj.Value = value;

				SimconnectProvider.Instance.SetDataOnSimObject(this, SimDataRequest.Fuel, SimObjectID.FuelTankCenterLevel, obj);
			}
		}
		#endregion

		#region FuelTankCenterQuantity SimProperty
		private SimProperty<double> mFuelTankCenterQuantity;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct FuelTankCenterQuantity
		{
			public double Value;
		}

		public const string FuelTankCenterQuantityKey = "FuelTankCenterQuantity";

		[Description("Center fuel tank level in gallons")]
		public double FuelTankCenterQuantityProp
		{
			get { return this.mFuelTankCenterQuantity.Value; }
		}

		[Description("Sets the center fuel tank level in gallons")]
		public void SetFuelTankCenterQuantity(double value)
		{
			if (this.mFuelTankCenterQuantity.Value != value)
			{
				FuelTankCenterQuantity obj = new FuelTankCenterQuantity();
				obj.Value = value;

				SimconnectProvider.Instance.SetDataOnSimObject(this, SimDataRequest.Fuel, SimObjectID.FuelTankCenterQuantity, obj);
			}
		}
		#endregion

		#region FuelTankCenterCapacity SimProperty
		private SimProperty<double> mFuelTankCenterCapacity;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct FuelTankCenterCapacity
		{
			public double Value;
		}

		public const string FuelTankCenterCapacityKey = "FuelTankCenterCapacity";

		[Description("Center fuel tank capacity in gallons")]
		public double FuelTankCenterCapacityProp
		{
			get { return this.mFuelTankCenterCapacity.Value; }
		}
		#endregion

		#region FuelTotalQuantityWeight SimProperty
		private SimProperty<double> mFuelTotalQuantityWeight;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct FuelTotalQuantityWeight
		{
			public double Value;
		}

		public const string FuelTotalQuantityWeightKey = "FuelTotalQuantityWeight";

		[Description("Current total fuel weight of the aircraft")]
		public double FuelTotalQuantityWeightProp
		{
			get { return this.mFuelTotalQuantityWeight.Value; }
		}
		#endregion

		#region FuelTotalQuantity SimProperty
		private SimProperty<double> mFuelTotalQuantity;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct FuelTotalQuantity
		{
			public double Value;
		}

		public const string FuelTotalQuantityKey = "FuelTotalQuantity";

		[Description("Current total fuel quantity in volume")]
		public double FuelTotalQuantityProp
		{
			get { return this.mFuelTotalQuantity.Value; }
		}
		#endregion

		#region FuelTotalCapacity SimProperty
		private SimProperty<double> mFuelTotalCapacity;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		struct FuelTotalCapacity
		{
			public double Value;
		}

		public const string FuelTotalCapacityKey = "FuelTotalCapacity";

		[Description("Total fuel capacity of the aircraft")]
		public double FuelTotalCapacityProp
		{
			get { return this.mFuelTotalCapacity.Value; }
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
						case FuelTankCenterLevelKey:
							var wFuelTankCenterLevel = simProp as SimProperty<double>;
							wFuelTankCenterLevel.Value = Math.Round(((FuelTankCenterLevel)simObject).Value, 3);
							break;
						case FuelTankCenterQuantityKey:
							var wFuelTankCenterQuantity = simProp as SimProperty<double>;
							wFuelTankCenterQuantity.Value = Math.Round(((FuelTankCenterQuantity)simObject).Value, 1);
							break;
						case FuelTankCenterCapacityKey:
							var wFuelTankCenterCapacity = simProp as SimProperty<double>;
							wFuelTankCenterCapacity.Value = Math.Round(((FuelTankCenterCapacity)simObject).Value, 1);
							break;
						case FuelTotalQuantityWeightKey:
							var wFuelTotalQuantityWeight = simProp as SimProperty<double>;
							wFuelTotalQuantityWeight.Value = Math.Round(((FuelTotalQuantityWeight)simObject).Value, 1);
							break;
						case FuelTotalQuantityKey:
							var wFuelTotalQuantity = simProp as SimProperty<double>;
							wFuelTotalQuantity.Value = Math.Round(((FuelTotalQuantity)simObject).Value, 1);
							break;
						case FuelTotalCapacityKey:
							var wFuelTotalCapacity = simProp as SimProperty<double>;
							wFuelTotalCapacity.Value = Math.Round(((FuelTotalCapacity)simObject).Value, 1);
							break;
						default:
							SimLogger.Log(LogMode.Warn, "FuelProvider", "Receiving SimObject that is not registered");
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
					SimLogger.Log(LogMode.Warn, "FuelProvider", "Receiving SimEvent that is not registered");
					break;
			}
		}

		protected override void registerProperties()
		{
			try
			{
				SimconnectProvider.Instance.Register_SimObject<FuelTankCenterLevel, double>(this.mFuelTankCenterLevel);
				SimconnectProvider.Instance.Register_SimObject<FuelTankCenterQuantity, double>(this.mFuelTankCenterQuantity);
				SimconnectProvider.Instance.Register_SimObject<FuelTankCenterCapacity, double>(this.mFuelTankCenterCapacity);
				SimconnectProvider.Instance.Register_SimObject<FuelTotalQuantityWeight, double>(this.mFuelTotalQuantityWeight);
				SimconnectProvider.Instance.Register_SimObject<FuelTotalQuantity, double>(this.mFuelTotalQuantity);
				SimconnectProvider.Instance.Register_SimObject<FuelTotalCapacity, double>(this.mFuelTotalCapacity);

				SimLogger.Log(LogMode.Info, "FuelProvider", "All properties have been registered");
			}
			catch (Exception ex)
			{
				SimLogger.Log(LogMode.Error, "FuelProvider", "Error while registering properties", ex.Message, ex.StackTrace);
			}
		}

		protected override void startSubscriptions()
        {
            try
            {
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.Fuel, this.mFuelTankCenterLevel);
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.Fuel, this.mFuelTankCenterQuantity);
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.Fuel, this.mFuelTankCenterCapacity);
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.Fuel, this.mFuelTotalQuantityWeight);
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.Fuel, this.mFuelTotalQuantity);
                SimconnectProvider.Instance.Subscribe_SimObject<double>(SimNotificationGroup.Fuel, this.mFuelTotalCapacity);

                SimLogger.Log(LogMode.Info, "FuelProvider", "All subscriptions have been started");
            }
            catch (Exception ex)
            {
                SimLogger.Log(LogMode.Error, "FuelProvider", "Error while starting subscriptions", ex.Message, ex.StackTrace);
            }
        }

		protected override void resetProperties()
        {
            this.mSimProperties.Clear();

            this.mFuelTankCenterLevel = new SimProperty<double>
                (this, SimDataRequest.Fuel, SimObjectID.FuelTankCenterLevel, 0.0, null, "FUEL TANK CENTER LEVEL", SimObjectDataType.Double);
            this.mFuelTankCenterLevel.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mFuelTankCenterLevel);

            this.mFuelTankCenterQuantity = new SimProperty<double>
                (this, SimDataRequest.Fuel, SimObjectID.FuelTankCenterQuantity, 0.0, "gallons", "FUEL TANK CENTER QUANTITY", SimObjectDataType.Double);
            this.mFuelTankCenterQuantity.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mFuelTankCenterQuantity);

            this.mFuelTankCenterCapacity = new SimProperty<double>
                (this, SimDataRequest.Fuel, SimObjectID.FuelTankCenterCapacity, 0.0, "gallons", "FUEL TANK CENTER CAPACITY", SimObjectDataType.Double);
            this.mFuelTankCenterCapacity.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mFuelTankCenterCapacity);

            this.mFuelTotalQuantityWeight = new SimProperty<double>
                (this, SimDataRequest.Fuel, SimObjectID.FuelTotalQuantityWeight, 0.0, "pounds", "FUEL TOTAL QUANTITY WEIGHT", SimObjectDataType.Double);
            this.mFuelTotalQuantityWeight.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mFuelTotalQuantityWeight);

            this.mFuelTotalQuantity = new SimProperty<double>
                (this, SimDataRequest.Fuel, SimObjectID.FuelTotalQuantity, 0.0, "gallons", "FUEL TOTAL QUANTITY", SimObjectDataType.Double);
            this.mFuelTotalQuantity.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mFuelTotalQuantity);

            this.mFuelTotalCapacity = new SimProperty<double>
                (this, SimDataRequest.Fuel, SimObjectID.FuelTotalCapacity, 0.0, "gallons", "FUEL TOTAL CAPACITY", SimObjectDataType.Double);
            this.mFuelTotalCapacity.PropertyChanged += new PropertyValueChangedEventHandler(SimProperty_PropertyChanged);
            this.mSimProperties.Add(this.mFuelTotalCapacity);

            SimLogger.Log(LogMode.Info, "FuelProvider", "All Sim properties have been reset");
        }

		protected override void stopSubscriptions()
        {
            try
            {
                // Unsubscribe all simobjects
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mFuelTankCenterLevel);
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mFuelTankCenterQuantity);
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mFuelTankCenterCapacity);
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mFuelTotalQuantityWeight);
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mFuelTotalQuantity);
                SimconnectProvider.Instance.Unsubscribe_SimObject<double>(this.mFuelTotalCapacity);

                // Unsubscribe group
                SimconnectProvider.Instance.Unsubscribe_GroupNotification(SimNotificationGroup.Fuel);

                SimLogger.Log(LogMode.Info, "FuelProvider", "All subscriptions have been stopped");
            }
            catch (Exception ex)
            {
                SimLogger.Log(LogMode.Error, "FuelProvider", "Error while stopping subscriptions", ex.Message, ex.StackTrace);
            }
        }
		#endregion
	}
}
