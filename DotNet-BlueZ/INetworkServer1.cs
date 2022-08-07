using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.NetworkServer1")]
public interface INetworkServer1 : IDBusObject
{
    Task RegisterAsync(string Uuid, string Bridge);
    Task UnregisterAsync(string Uuid);
}