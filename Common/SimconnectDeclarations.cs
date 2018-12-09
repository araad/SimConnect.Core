using Microsoft.FlightSimulator.SimConnect;

namespace SIM.Connect.Common
{
    public enum SimNotificationGroup
    {
        Aircraft,
        ElectricalSystems,
        Fuel,
        PositionSpeed,
        FlightInstrumentation
    }

    public enum SimDataRequest
    {
        Aircraft,
        ElectricalSystems,
        Fuel,
        PositionSpeed,
        FlightInstrumentation
    }

    public enum SimObjectDataType
    {
        String = SIMCONNECT_DATATYPE.STRING256,
        Bool = SIMCONNECT_DATATYPE.INT32,
        Int = SIMCONNECT_DATATYPE.INT64,
        Double = SIMCONNECT_DATATYPE.FLOAT64
    }

    public enum SimObjectID
    {
        #region Aircraft
        AircraftTitle,
        AircraftTotalWeight,
        #endregion

        #region ElectricalSystems
        ElectricalMasterBattery,
        #endregion

        #region Fuel
        FuelTankCenterLevel,
        FuelTankCenterQuantity,
        FuelTankCenterCapacity,
        FuelTankCenterTwoLevel,
        FuelTankCenterTwoQuantity,
        FuelTankCenterTwoCapacity,
        FuelTotalQuantityWeight,
        FuelTotalQuantity,
        FuelTotalCapacity,
        #endregion

        #region PositionSpeed
        Latitude,
        Longitude,
        MSLAltitude,
        AGLAltitude,
        MagneticHeading,
        TrueHeading,
        #endregion

        #region FlightInstrumentation
        IndicatedAirspeed,
        #endregion

        _Undefined
    }

    public enum SimEventID
    {
        TOGGLE_MASTER_BATTERY
    }
}
