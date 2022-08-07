using Tmds.DBus;

namespace HashtagChris.DotNetBlueZ;

[DBusInterface("org.bluez.Media1")]
public interface IMedia1 : IDBusObject
{
    Task RegisterEndpointAsync(ObjectPath Endpoint, IDictionary<string, object> Properties);
    Task UnregisterEndpointAsync(ObjectPath Endpoint);
    Task RegisterPlayerAsync(ObjectPath Player, IDictionary<string, object> Properties);
    Task UnregisterPlayerAsync(ObjectPath Player);
}