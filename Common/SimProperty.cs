using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SIM.Connect.Common
{
    public class SimProperty<T> : ISimProperty
    {
        private IDataProvider mProvider;
        private Enum mRequestID;
        private Enum mSimObjectID;
        private string mSimObjectName;
        private SimObjectDataType mSimObjectType;
        private Enum mSimEventID;
        private string mSimEventName;
        private T mValue;
        private string mUnits;

        public IDataProvider Provider
        {
            get { return mProvider; }
        }
        public Enum RequestID
        {
            get { return mRequestID; }
        }
        public Enum SimObjectID
        {
            get { return mSimObjectID; }
        }
        public string SimObjectName
        {
            get
            {
                return mSimObjectName;
            }
        }
        public SimObjectDataType SimObjectType
        {
            get { return mSimObjectType; }
        }
        public Enum SimEventID
        {
            get { return mSimEventID; }
        }
        public string SimEventName
        {
            get
            {
                return mSimEventName;
            }
        }
        public T Value
        {
            get { return mValue; }
            set { ChangeProperty("Value", ref mValue, ref value); }
        }
        public string Units
        {
            get { return mUnits; }
        }
        public event PropertyValueChangedEventHandler PropertyChanged;

        public SimProperty(IDataProvider iProvider, Enum iRequestID, Enum iSimObjectID, T iSimObjectValue, string iUnits, string iSimObjectName, SimObjectDataType iSimObjectType)
        {
            mProvider = iProvider;
            mRequestID = iRequestID;
            mSimObjectID = iSimObjectID;
            mValue = iSimObjectValue;
            mUnits = iUnits;
            mSimObjectName = iSimObjectName;
            mSimObjectType = iSimObjectType;
            mSimEventName = string.Empty;
        }

        public SimProperty(IDataProvider iProvider, Enum iRequestID, Enum iSimObjectID, T iSimObjectValue, string iUnits, string iSimObjectName, SimObjectDataType iSimObjectType, Enum iSimEventID, string iSimEventName)
        {
            mProvider = iProvider;
            mRequestID = iRequestID;
            mSimObjectID = iSimObjectID;
            mValue = iSimObjectValue;
            mUnits = iUnits;
            mSimObjectName = iSimObjectName;
            mSimObjectType = iSimObjectType;
            mSimEventID = iSimEventID;
            mSimEventName = iSimEventName;
        }

        private bool ChangeProperty(string propertyName, ref T oldValue, ref T newValue)
        {
            if (oldValue == null && newValue == null)
            {
                return false;
            }
            if ((oldValue == null && newValue != null) || !oldValue.Equals((T)newValue))
            {
                oldValue = newValue;
                OnPropertyChanged(propertyName, newValue);
                return true;
            }
            return false;
        }

        private void OnPropertyChanged(string propertyName, object newValue)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyValueChangedEventArgs(propertyName, newValue));
            }
        }
    }
}
