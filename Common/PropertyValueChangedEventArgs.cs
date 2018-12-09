using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIM.Connect.Common
{
    public class PropertyValueChangedEventArgs : EventArgs
    {
        string mName;
        object mValue;

        public PropertyValueChangedEventArgs(string propertyName, bool value)
        {
            initValue(propertyName, value);
        }

        public PropertyValueChangedEventArgs(string propertyName, string value)
        {
            initValue(propertyName, value);
        }

        public PropertyValueChangedEventArgs(string propertyName, int value)
        {
            initValue(propertyName, value);
        }

        public PropertyValueChangedEventArgs(string propertyName, double value)
        {
            initValue(propertyName, value);
        }

        public PropertyValueChangedEventArgs(string propertyName, object value)
        {
            initValue(propertyName, value);
        }

        void initValue(string propertyName, object value)
        {
            this.mName = propertyName;
            this.mValue = value;
        }

        public string Name
        {
            get
            {
                return this.mName;
            }
        }

        public object Value
        {
            get
            {
                return this.mValue;
            }
        }
    }

    public delegate void PropertyValueChangedEventHandler(object sender, PropertyValueChangedEventArgs e);
}
