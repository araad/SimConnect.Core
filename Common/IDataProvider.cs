
namespace SIM.Connect.Common
{
    public interface IDataProvider
    {
        void Simconnect_ReceiveSimObject(string objectName, object simObject);

        void Simconnect_ReceiveSimEvent(uint eventID, object simObject);
    }
}
