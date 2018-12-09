using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIM.Connect.Simconnect;
using System.Collections;
using System.ComponentModel;

namespace SIM.Connect.Common
{
    public abstract class DataProvider : IDataProvider, INotifyPropertyChanged
    {
        private static object sLocker = new object();

        protected int mSubscribersCount;

        protected List<ISimProperty> mSimProperties;

        public int Subscribers
        {
            get { return this.mSubscribersCount; }
        }

        private event PropertyValueChangedEventHandler mPropertyChanged;
        public event PropertyValueChangedEventHandler PropertyChanged
        {
            add
            {
                lock (sLocker)
                {
                    mPropertyChanged += value;
                    if (mSubscribersCount == 0 && SimconnectProvider.Instance.SimconnectState == ConenctionState.CONNECTED)
                    {
                        startSubscriptions();
                    }
                    mSubscribersCount++;
                    OnSubscribersChanged();
                }
            }
            remove
            {
                lock (sLocker)
                {
                    mPropertyChanged -= value;
                    mSubscribersCount--;
                    if (mSubscribersCount == 0)
                    {
                        stopSubscriptions();
                    }
                    OnSubscribersChanged();
                }
            }
        }
        protected void OnPropertyChanged(string propertyName, object newValue)
        {
            if (mPropertyChanged != null)
            {
                mPropertyChanged(this, new PropertyValueChangedEventArgs(propertyName, newValue));
            }
        }

        protected void SimProperty_PropertyChanged(object sender, PropertyValueChangedEventArgs e)
        {
            ISimProperty simProp = sender as ISimProperty;
            this.OnPropertyChanged(simProp.SimObjectID.ToString(), e.Value);
        }

        public abstract void Simconnect_ReceiveSimObject(string simObjectID, object simObject);

        public abstract void Simconnect_ReceiveSimEvent(uint simEventID, object simObject);

        protected abstract void startSubscriptions();

        protected abstract void stopSubscriptions();

        protected abstract void resetProperties();

        protected abstract void registerProperties();

        protected void subscribe_SimconnectProvider()
        {
            SimconnectProvider.Instance.Open += new EventHandler(Simconnect_Open);
            SimconnectProvider.Instance.Leave += new EventHandler(Simconnect_Close);
        }

        private void unsubscribe_SimconnectProvider()
        {
            SimconnectProvider.Instance.Join -= new EventHandler(Simconnect_Open);
            SimconnectProvider.Instance.Leave -= new EventHandler(Simconnect_Close);
        }

        private void Simconnect_Open(object sender, EventArgs e)
        {
            this.registerProperties();
        }

        private void Simconnect_Close(object sender, EventArgs e)
        {
            this.resetProperties();
        }

        private event PropertyChangedEventHandler mSubscribersChanged;

        private void OnSubscribersChanged()
        {
            if (mSubscribersChanged != null)
            {
                mSubscribersChanged(this, new PropertyChangedEventArgs("Subscribers"));
            }
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { lock (sLocker) { mSubscribersChanged += value; } }
            remove { lock (sLocker) { mSubscribersChanged -= value; } }
        }
    }
}
